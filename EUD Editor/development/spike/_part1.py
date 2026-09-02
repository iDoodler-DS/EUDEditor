"""Turns the trigger tree of a project into epScript, and back again.

Why: the editor already writes epScript for euddraft to compile, but only one
way. If the same text can be read back into the tree it came from, then
epScript can become the format the editor saves, and a trigger becomes
something a person can read, diff and review.

This is a spike. It covers the shape of the tree, not every kind of value, and
it reports what it cannot do rather than guessing.

    python development/spike/eps_roundtrip.py <project.e2s> [more.e2s ...]
    python development/spike/eps_roundtrip.py --corpus <folder>

Three things it shows:

  1. Signatures come from the editor's own tables. Data/TriggerEditor/action.json
     and condition.json give a template for each node, such as
     "Bring($Player$, $Comparison$, $Number$, $Unit$, $Location$)". That is the
     set the GUI can draw, so it is exactly the set worth parsing back.

  2. State that belongs to the editor and not to the language rides in comments.
     A node that is switched off is written as a comment, so euddraft skips it
     and the reader still sees it. Folders and folded state work the same way.

  3. Anything else stays raw. A node the tables do not describe is written out
     as it is and read back as one block, which is what the tree already does
     with its RawString kind.
"""

import io
import json
import os
import re
import sys

SEPARATOR = 'ஐ'          # the character the editor puts between values
EDITOR_MARK = '//@'           # what carries editor-only state
QUOTES = '"' + chr(39)


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


ACTION, CONDITION, RAW = 8, 7, 23
IF_THEN, IF_ELSE = 1, 2
IF_CLAUSE, ACTION_CLAUSE, THEN, ELSE = 3, 4, 5, 6
WHILE, FOR, WHILE_COND, WHILE_BODY, FOR_BODY = 9, 10, 12, 13, 14
FUNCTIONS, FUNCTION_DEF, ARGUMENT, CODE, CALL = 15, 16, 17, 18, 19
WAIT, FOLDER, FOLDER_BODY = 20, 21, 22


def read_section(path, name):
    """The text between S_<name> and E_<name> of a project file."""
    raw = io.open(path, 'rb').read().decode('utf-8', 'replace')
    start, end = raw.find('S_' + name), raw.find('E_' + name)
    if start < 0 or end < start:
        return ''
    return raw[start + len('S_' + name):end]


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


def load_signatures(data_dir):
    """
    Every node the GUI can draw, from the tables the editor already ships.
    Gives back name -> (function name, the values it takes, kind).
    """
    found = {}
    for file_name, kind in (('action.json', ACTION), ('condition.json', CONDITION)):
        path = os.path.join(data_dir, 'TriggerEditor', file_name)
        if not os.path.exists(path):
            continue
        for entry in json.load(io.open(path, encoding='utf-8-sig')):
            template = entry.get('CodeText') or ''
            call = re.match(r'\s*([A-Za-z_]\w*)\s*\(', template)
            if not call:
                continue
            found[entry['Name']] = (call.group(1), re.findall(r'\$(\w+)\$', template), kind)
    return found


def by_function(signatures):
    """The same table, to look up what a call in the text means."""
    out = {}
    for name, (function, values, kind) in signatures.items():
        out.setdefault((function, kind), (name, values))
    return out
