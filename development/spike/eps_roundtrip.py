"""Turns the trigger tree of a project into epScript, and back again.

The editor already writes epScript for euddraft to compile, but only one way.
If that text can be read back into the tree it came from, epScript can become
the format the editor saves, and a trigger becomes something a person can
read, diff and review.

    python development/spike/eps_roundtrip.py <project.e2s> [more.e2s ...]
    python development/spike/eps_roundtrip.py --corpus <folder>
    python development/spike/eps_roundtrip.py --show <project.e2s>

This is a spike. It covers the shape of the tree, not every kind of value, and
it reports what it cannot do rather than guessing.

Three things it shows.

1. Signatures come from the tables the editor already ships. Every node the
   GUI can draw is in Data/TriggerEditor/action.json or condition.json, with a
   template such as "Bring($Player$, $Comparison$, $Number$, $Unit$,
   $Location$)". That is the set the GUI can draw, so it is exactly the set
   worth reading back. Nothing has to be dug out of eudplib.

2. What belongs to the editor and not to the language rides in a comment. A
   node that is switched off is written as a comment, so euddraft passes over
   it and a person still reads it. Folders work the same way.

3. Anything else stays raw. A node the tables do not describe is written as it
   is and read back as one block, which is what the tree already does with its
   RawString kind.
"""

import io
import json
import os
import re
import sys

SEPARATOR = 'ஐ'        # what the editor puts between the values of a node
EDITOR_MARK = '//@'         # what carries state that is the editor's own
QUOTES = chr(34) + chr(39)
EMPTY = chr(0) * 0

# The kinds of element, from ElementType in Module/Trigger.vb.
IF_THEN, IF_ELSE = 1, 2
IF_CLAUSE, ACTION_CLAUSE, THEN, ELSE = 3, 4, 5, 6
CONDITION, ACTION = 7, 8
WHILE, FOR, WHILE_COND, WHILE_BODY, FOR_BODY = 9, 10, 12, 13, 14
FUNCTIONS, FUNCTION_DEF, ARGUMENT, CODE, CALL = 15, 16, 17, 18, 19
WAIT, FOLDER, FOLDER_BODY, RAW = 20, 21, 22, 23

PASS_THROUGH = (FOLDER_BODY, ACTION_CLAUSE, THEN, WHILE_BODY, FOR_BODY, CODE, FUNCTIONS)
MEANINGFUL = (ACTION, CONDITION, RAW, FOLDER, FUNCTION_DEF, IF_THEN, IF_ELSE)


class Node:
    """One element of the trigger tree."""

    def __init__(self, kind, off=False, folded=False, notcon=False):
        self.kind = int(kind)
        self.off = off
        self.folded = folded
        self.notcon = notcon
        self.call = None
        self.values = []
        self.children = []

    def __repr__(self):
        return 'Node(%d, %s, %d children)' % (self.kind, self.call, len(self.children))

    def walk(self):
        yield self
        for child in self.children:
            for node in child.walk():
                yield node


# ---------------------------------------------------------------- the file

def read_section(path, name):
    """The text between S_<name> and E_<name> of a project file."""
    raw = io.open(path, 'rb').read().decode('utf-8', 'replace')
    start, end = raw.find('S_' + name), raw.find('E_' + name)
    if start < 0 or end < start:
        return EMPTY
    # The file ends its lines with a return and a feed. Only the feed divides
    # the lines here; a return left behind would travel inside a value.
    return raw[start + len('S_' + name):end].replace('\r\n', '\n')


def parse_tree(lines, at):
    """Reads one node, and gives back the node and where the reader stopped."""
    parts = lines[at].strip()[len('Type:'):].split(',')
    node = Node(parts[0],
                off=len(parts) > 1 and parts[1] == 'True',
                folded=len(parts) > 2 and parts[2] == 'True',
                notcon=len(parts) > 3 and parts[3] == 'True')
    at += 1

    if at < len(lines) and (lines[at].startswith('act:') or lines[at].startswith('con:')):
        node.call = lines[at][4:].strip()
        at += 1

    # Whatever sits before ElementsCount is the values of this node. Raw text
    # runs to many lines, so all of it belongs here.
    value_lines = []
    while at < len(lines) and not lines[at].startswith('ElementsCount:'):
        value_lines.append(lines[at])
        at += 1
    if value_lines:
        node.values = '\n'.join(value_lines).split(SEPARATOR)

    count = 0
    if at < len(lines) and lines[at].startswith('ElementsCount:'):
        count = int(lines[at][len('ElementsCount:'):].strip() or 0)
        at += 1

    for _ in range(count):
        while at < len(lines) and not lines[at].startswith('Type:'):
            at += 1
        if at >= len(lines):
            break
        child, at = parse_tree(lines, at)
        node.children.append(child)

    while at < len(lines) and lines[at].strip() != 'END':
        at += 1
    return node, at + 1


def read_project(path):
    """The named parts of the trigger tree of a project."""
    lines = read_section(path, 'TriggerEditorSET').split('\n')
    parts, current = {}, None
    for i, line in enumerate(lines):
        mark = re.match(r'^&(\w+)&\s*$', line.strip())
        if mark:
            current = mark.group(1)
            continue
        if current and line.startswith('Type:'):
            node, _end = parse_tree(lines, i)
            parts.setdefault(current, node)
            current = None
    return parts


# ------------------------------------------------------------- signatures

class Signature:
    """
    One node the GUI can draw, as the editor's own table describes it.

    A template is either a plain call, "Bring($Player$, $Number$)", or a piece
    of epScript in its own right, "var $Name$ = $Count$". Both are written by
    putting the values in place of the marks, and both are read back by the
    same template turned into a pattern, so one table serves each way.
    """

    def __init__(self, name, template, kind):
        self.name = name
        self.template = template
        self.kind = kind
        self.values = re.findall(r'\$(\w+)\$', template)
        # A plain call is a name, brackets, and the values between them.
        body = template.strip().rstrip(';').strip()
        call = re.match(r'^([A-Za-z_]\w*)\s*\(.*\)$', body, re.S)
        self.function = call.group(1) if call else None
        self.pattern = self.compile(template)

    @staticmethod
    def compile(template):
        """The template as a pattern, with a group where each value goes."""
        out = []
        for piece in re.split(r'(\$\w+\$)', template.strip().rstrip(';').strip()):
            if re.match(r'^\$\w+\$$', piece):
                out.append('(.*?)')
            else:
                # Room to breathe: the editor writes one space, a person may
                # write none or several.
                out.append(re.escape(piece.strip()).replace(r'\ ', r'\s*'))
        return re.compile(r'^\s*' + r'\s*'.join(x for x in out if x) + r'\s*$', re.S)

    def literal_length(self):
        """How much of the template is its own words, and not a value."""
        return len(EMPTY.join(re.split(r'\$\w+\$', self.template)).strip())

    def write(self, values):
        """The template with the values of a node in it."""
        text = self.template
        for i, mark in enumerate(self.values):
            text = text.replace('$' + mark + '$', values[i] if i < len(values) else EMPTY, 1)
        return text.strip()


def load_signatures(data_dir):
    """Every node the GUI can draw, from the tables the editor already ships."""
    found = {}
    for file_name, kind in (('action.json', ACTION), ('condition.json', CONDITION)):
        path = os.path.join(data_dir, 'TriggerEditor', file_name)
        if not os.path.exists(path):
            continue
        for entry in json.load(io.open(path, encoding='utf-8-sig')):
            template = entry.get('CodeText') or EMPTY
            if not template.strip() or '$' not in template and '(' not in template:
                continue          # a marker such as EUDPart, which holds no code
            found[entry['Name']] = Signature(entry['Name'], template, kind)
    return found


def by_function(signatures):
    """The plain calls, to look up what a call in the text means."""
    out = {}
    for signature in signatures.values():
        if signature.function:
            out.setdefault((signature.function, signature.kind), signature)
    return out


# ---------------------------------------------------------- tree to script

class Writer:
    def __init__(self, signatures):
        self.signatures = signatures
        self.unknown = set()

    def write(self, node, depth=0):
        pad = '    ' * depth

        # A folder is the editor's own idea, so it goes in as a comment.
        if node.kind == FOLDER:
            title = (node.values[0] if node.values else EMPTY).replace('\n', ' ')
            out = [pad + EDITOR_MARK + 'folder ' + title]
            for child in node.children:
                out += self.write(child, depth)
            return out + [pad + EDITOR_MARK + 'end']

        if node.kind in PASS_THROUGH:
            out = []
            for child in node.children:
                out += self.write(child, depth)
            return out

        if node.kind == RAW:
            body = SEPARATOR.join(node.values) if node.values else EMPTY
            return ([pad + EDITOR_MARK + 'raw'] +
                    [pad + line for line in body.split('\n')] +
                    [pad + EDITOR_MARK + 'endraw'])

        if node.kind in (ACTION, CONDITION):
            text = self.call_text(node)
            if node.off:
                # Switched off: a comment, so euddraft passes over it and a
                # person still reads it.
                return [pad + EDITOR_MARK + 'off ' + text]
            return [pad + text + (';' if node.kind == ACTION else EMPTY)]

        if node.kind in (IF_THEN, IF_ELSE):
            conditions = []
            for clause in [c for c in node.children if c.kind == IF_CLAUSE]:
                for child in clause.children:
                    conditions += self.write(child, 0)
            joined = ' && '.join(x.strip().rstrip(';') for x in conditions) or 'True'
            out = [pad + 'if (' + joined + ') {']
            for child in node.children:
                if child.kind in (ACTION_CLAUSE, THEN):
                    out += self.write(child, depth + 1)
            if node.kind == IF_ELSE:
                out.append(pad + '} else {')
                for child in node.children:
                    if child.kind == ELSE:
                        out += self.write(child, depth + 1)
            return out + [pad + '}']

        if node.kind == FUNCTION_DEF:
            name = node.values[0] if node.values else 'unnamed'
            args = []
            for child in node.children:
                if child.kind == ARGUMENT:
                    args = [(a.values[0] if a.values else EMPTY) for a in child.children]
            out = [pad + 'function ' + name + '(' + ', '.join(args) + ') {']
            for child in node.children:
                if child.kind != ARGUMENT:
                    out += self.write(child, depth + 1)
            return out + [pad + '}']

        if node.kind == CALL:
            return [pad + (node.values[0] if node.values else EMPTY) + ';']

        # A kind this spike does not model, but which only holds other nodes,
        # steps aside so what is inside it still reaches the text.
        if node.children and not node.values:
            self.unknown.add(node.kind)
            out = []
            for child in node.children:
                out += self.write(child, depth)
            return out

        # Anything else keeps its place and its content. Nesting these behind a
        # marker of their own was tried and read back worse, so a kind with no
        # template is written flat and counted as work still to do.
        self.unknown.add(node.kind)
        out = [pad + EDITOR_MARK + 'node %d %s' % (node.kind, node.call or EMPTY)]
        for child in node.children:
            out += self.write(child, depth)
        return out

    def call_text(self, node):
        found = self.signatures.get(node.call)
        if not found:
            self.unknown.add(node.call)
            return EDITOR_MARK + 'unknown ' + (node.call or '?')
        return found.write(node.values)


# ---------------------------------------------------------- script to tree

class Reader:
    def __init__(self, signatures):
        self.lookup = by_function(signatures)
        # The templates that are not a plain call are tried in turn, the one
        # that says most first. "var $Name$ = $Count$" holds the words var and
        # =, while "$Variable$ $VariableComparison$ $Count$" holds nothing of
        # its own and would match any three words, so it goes last.
        self.templates = sorted(
            [s for s in signatures.values() if not s.function],
            key=lambda s: (-s.literal_length(), -len(s.template)))
        self.raw_blocks = 0

    def read(self, text):
        lines = text.split('\n')
        root = Node(CODE)
        self.fill(root, lines, 0, len(lines))
        return root

    def fill(self, parent, lines, at, stop):
        while at < stop:
            line = lines[at].strip()
            at += 1
            if not line:
                continue

            if line.startswith(EDITOR_MARK + 'folder '):
                folder = Node(FOLDER)
                folder.values = [line[len(EDITOR_MARK + 'folder '):]]
                end = self.matching(lines, at, stop,
                                    EDITOR_MARK + 'folder ', EDITOR_MARK + 'end')
                self.fill(folder, lines, at, end)
                parent.children.append(folder)
                at = end + 1
                continue

            if line == EDITOR_MARK + 'raw':
                end = at
                while end < stop and lines[end].strip() != EDITOR_MARK + 'endraw':
                    end += 1
                node = Node(RAW)
                node.values = ['\n'.join(l.strip() for l in lines[at:end])]
                parent.children.append(node)
                self.raw_blocks += 1
                at = end + 1
                continue

            if line.startswith(EDITOR_MARK + 'node '):
                rest = line[len(EDITOR_MARK + 'node '):].split(None, 1)
                node = Node(int(rest[0]) if rest and rest[0].isdigit() else 0)
                if len(rest) > 1:
                    node.call = rest[1].strip()
                parent.children.append(node)
                continue

            if line.startswith(EDITOR_MARK + 'off '):
                node = self.statement(line[len(EDITOR_MARK + 'off '):])
                if node is not None:
                    node.off = True
                    parent.children.append(node)
                continue

            if line.startswith('if ('):
                node = Node(IF_THEN)
                clause = Node(IF_CLAUSE)
                inside = line[line.index('(') + 1:line.rindex(')')]
                for piece in inside.split('&&'):
                    got = self.statement(piece.strip(), CONDITION)
                    if got is not None:
                        clause.children.append(got)
                node.children.append(clause)
                body = Node(ACTION_CLAUSE)
                end = self.block_end(lines, at, stop)
                self.fill(body, lines, at, end)
                node.children.append(body)
                parent.children.append(node)
                at = end + 1
                continue

            if line.startswith('function '):
                head = re.match(r'function\s+(\w+)\s*\((.*?)\)', line)
                node = Node(FUNCTION_DEF)
                node.values = [head.group(1) if head else 'unnamed']
                args = Node(ARGUMENT)
                written = head.group(2) if head else EMPTY
                for name in [a.strip() for a in written.split(',') if a.strip()]:
                    got = Node(ARGUMENT)
                    got.values = [name]
                    args.children.append(got)
                node.children.append(args)
                body = Node(CODE)
                end = self.block_end(lines, at, stop)
                self.fill(body, lines, at, end)
                node.children.append(body)
                parent.children.append(node)
                at = end + 1
                continue

            if line in ('}', '} else {'):
                continue

            node = self.statement(line)
            if node is not None:
                parent.children.append(node)
        return at

    def statement(self, text, prefer=None):
        """One call, as a node. Anything else becomes a raw block."""
        text = text.strip().rstrip(';').strip()
        if not text:
            return None
        call = re.match(r'^([A-Za-z_]\w*)\s*\((.*)\)$', text, re.S)
        if call:
            order = (prefer, ACTION, CONDITION) if prefer else (ACTION, CONDITION)
            for kind in order:
                found = self.lookup.get((call.group(1), kind))
                if found:
                    node = Node(kind)
                    node.call = found.name
                    node.values = split_arguments(call.group(2))
                    return node

        # Not a plain call, so try the templates that are a piece of epScript
        # in their own right, such as "var $Name$ = $Count$". Two of them can
        # have the same shape, one an action and one a condition, so where the
        # line stands decides which is meant.
        wanted = prefer or ACTION
        for kind in (wanted, CONDITION if wanted == ACTION else ACTION):
            for signature in self.templates:
                if signature.kind != kind:
                    continue
                got = signature.pattern.match(text)
                if got:
                    node = Node(signature.kind)
                    node.call = signature.name
                    node.values = [g.strip() for g in got.groups()]
                    return node

        node = Node(RAW)
        node.values = [text]
        self.raw_blocks += 1
        return node

    @staticmethod
    def block_end(lines, at, stop):
        depth = 1
        while at < stop:
            line = lines[at].strip()
            depth += line.count('{') - line.count('}')
            if depth <= 0:
                return at
            at += 1
        return stop

    @staticmethod
    def matching(lines, at, stop, opener, closer):
        depth = 1
        while at < stop:
            line = lines[at].strip()
            if line.startswith(opener):
                depth += 1
            elif line == closer:
                depth -= 1
                if depth == 0:
                    return at
            at += 1
        return stop


def split_arguments(text):
    """Splits on the commas that are not inside brackets or quotes."""
    out, depth, quote, current = [], 0, None, []
    for ch in text:
        if quote:
            current.append(ch)
            if ch == quote:
                quote = None
            continue
        if ch in QUOTES:
            quote = ch
            current.append(ch)
        elif ch in '([{':
            depth += 1
            current.append(ch)
        elif ch in ')]}':
            depth -= 1
            current.append(ch)
        elif ch == ',' and depth == 0:
            out.append(EMPTY.join(current).strip())
            current = []
        else:
            current.append(ch)
    if current:
        out.append(EMPTY.join(current).strip())
    return out


# -------------------------------------------------------------- comparing

def shape(node):
    """What a node means, without the parts that are only how it is drawn."""
    if node.kind in (ACTION, CONDITION):
        return (node.kind, node.call, tuple(v.strip() for v in node.values), node.off)
    if node.kind == RAW:
        text = node.values[0] if node.values else EMPTY
        return (RAW, EMPTY.join(text.split()))
    if node.kind == FOLDER:
        return (FOLDER, (node.values[0] if node.values else EMPTY).strip())
    if node.kind == FUNCTION_DEF:
        return (FUNCTION_DEF, (node.values[0] if node.values else EMPTY).strip())
    return (node.kind,)


def meaningful(node):
    return [got for got in node.walk() if got.kind in MEANINGFUL]


def compare(before, after):
    a = [shape(n) for n in meaningful(before)]
    b = [shape(n) for n in meaningful(after)]
    same = sum(1 for x, y in zip(a, b) if x == y)
    gap = None
    for i, (x, y) in enumerate(zip(a, b)):
        if x != y:
            gap = (i, x, y)
            break
    if gap is None and len(a) != len(b):
        gap = (min(len(a), len(b)), 'one side ends', 'the other goes on')
    return len(a), len(b), same, gap


# ----------------------------------------------------------------- report

def lossy_values(signatures, tree):
    """
    Nodes whose values the template cannot hold. CreatePlayerVariable, for one,
    is written as "const $Name$ = [0, 0, ...]" but the tree keeps a starting
    value beside the name, so that value has nowhere to go in the text.
    """
    out = {}
    for node in tree.walk():
        if node.kind not in (ACTION, CONDITION) or not node.call:
            continue
        found = signatures.get(node.call)
        if found and len(node.values) > len(found.values):
            out[node.call] = out.get(node.call, 0) + 1
    return out


def run(paths, data_dir, show=False):
    signatures = load_signatures(data_dir)
    plain = sum(1 for s in signatures.values() if s.function)
    print('signatures read from the editor tables: %d (%d plain calls, %d templates)'
          % (len(signatures), plain, len(signatures) - plain))
    print()

    total_nodes = total_steady = total_raw = 0
    all_lossy = {}
    for path in paths:
        parts = read_project(path)
        if not parts:
            print('%-32s no trigger tree' % os.path.basename(path)[:32])
            continue

        writer, reader = Writer(signatures), Reader(signatures)
        nodes = steady = 0
        lossy = {}
        for name, tree in sorted(parts.items()):
            first = '\n'.join(writer.write(tree))
            if show:
                print('----- %s -----' % name)
                print('\n'.join(first.split('\n')[:40]))
                print()
            again = writer.write(reader.read(first))
            second = '\n'.join(again)

            # What matters is that the text holds still: read it and write it
            # again, and nothing moves. Then the text is a safe place to keep
            # the triggers, whatever else the tree carries for the screen.
            a, b = first.split('\n'), second.split('\n')
            nodes += len(a)
            steady += sum(1 for x, y in zip(a, b) if x.strip() == y.strip())
            for call, count in lossy_values(signatures, tree).items():
                lossy[call] = lossy.get(call, 0) + count

        total_nodes += nodes
        total_steady += steady
        total_raw += reader.raw_blocks
        for call, count in lossy.items():
            all_lossy[call] = all_lossy.get(call, 0) + count

        share = (100.0 * steady / nodes) if nodes else 100.0
        print('%-32s %6d lines  %5.1f%% hold still  %3d raw' % (
            os.path.basename(path)[:32], nodes, share, reader.raw_blocks))
        if writer.unknown:
            names = sorted(str(u) for u in writer.unknown)
            print('%-32s   kinds with no template: %s%s' % (
                EMPTY, ', '.join(names[:6]), ' ...' if len(names) > 6 else EMPTY))

    print()
    if total_nodes:
        print('all of them: %d lines, %.1f%% hold still, %d raw blocks' % (
            total_nodes, 100.0 * total_steady / total_nodes, total_raw))
    if all_lossy:
        print()
        print('templates that cannot hold every value of their node:')
        for call, count in sorted(all_lossy.items(), key=lambda kv: -kv[1]):
            found = signatures.get(call)
            print('  %-26s %5d nodes   template takes %d, the tree keeps more'
                  % (call, count, len(found.values) if found else 0))


def main(argv):
    here = os.path.dirname(os.path.abspath(__file__))
    data_dir = os.path.abspath(os.path.join(here, '..', '..', 'EUD Editor', 'Data'))

    if not argv:
        print(__doc__)
        return 1

    show = False
    if argv[0] == '--show':
        show, argv = True, argv[1:]

    if argv and argv[0] == '--corpus':
        paths = []
        for folder, _dirs, files in os.walk(argv[1]):
            paths += [os.path.join(folder, f) for f in files if f.endswith('.e2s')]
        paths.sort()
    else:
        paths = argv

    run(paths, data_dir, show)
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
