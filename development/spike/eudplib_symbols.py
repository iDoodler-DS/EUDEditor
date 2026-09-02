"""Asks the installed euddraft what eudplib offers, and what it can be drawn as.

The editor wants to draw a line of epScript as a block with a drop-down for
each value. To do that it has to know what the values of a call are and what
each one is: a unit, a player, a location, a comparison. eudplib says so in its
own type notes, and euddraft carries the eudplib the user builds with, so the
answer comes from asking the euddraft that is installed.

    python development/spike/eudplib_symbols.py <euddraft.exe> <a map.scx>
    python development/spike/eudplib_symbols.py --read <eudplib_signatures.json>

The first form runs euddraft on a plugin that writes the signatures down. The
second reads a file that was written earlier, so the report can be looked at
again without another run.
"""

import collections
import io
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
PLUGIN = os.path.join(HERE, 'euddraft_dump_plugin.py')

# Names that say how a value is held, not what it means. A drop-down cannot be
# built from these; they fall back to a box the user types in.
PLUMBING = {'ExprProxy', 'EUDVariable', 'ConstExpr', 'ConstType', 'Byte', 'Word',
            'Dword', 'int', 'str', 'bytes', 'bool', 'Iterable', 'None', 'Optional',
            'Union', 'list', 'tuple', 'Literal', 'Sequence', 'Any', 'class', 'float',
            'T_co', 'Self', 'object'}

# What eudplib calls a thing, against the list the editor already fills for it.
# The editor's own name is the one in Data/TriggerEditor and ValueDef.txt.
DRAWN_AS = {
    'TrgUnit': 'Unit', 'DefaultUnit': 'Unit', 'Unit': 'Unit',
    'TrgPlayer': 'Player', '_Player': 'Player', 'Player': 'Player',
    'Location': 'Location',
    'TrgComparison': 'Comparison', 'Comparison': 'Comparison',
    'TrgModifier': 'Modifier', 'Modifier': 'Modifier',
    'TrgResource': 'ResourceType', 'Resource': 'ResourceType',
    'TrgScore': 'ScoreType', '_Score': 'ScoreType',
    'TrgSwitchAction': 'SwitchAction', 'SwitchAction': 'SwitchAction',
    'TrgSwitchState': 'SwitchState', 'SwitchState': 'SwitchState',
    '_Switch': 'Switch', 'Switch': 'Switch',
    'TrgAllyStatus': 'AllyStatus', 'AllyStatus': 'AllyStatus',
    'TrgOrder': 'Order', '_Order': 'Order',
    'UnitOrder': 'UnitOrder', 'DefaultUnitOrder': 'UnitOrder', '_UnitOrder': 'UnitOrder',
    'UnitProperty': 'Property',
    'String': 'Text', 'StringIdMap': 'Text',
    'AIScriptWithoutLocation': 'AIScript',
    'DefaultAIScriptWithoutLocation': 'AIScript',
    'TrgCount': 'Count',
}


def types_of(annotation):
    """The type names in a note, with the values written into it left out."""
    if not annotation:
        return []
    clean = re.sub(r"'[^']*'", ' ', re.sub(r'"[^"]*"', ' ', annotation))
    out = []
    for piece in re.findall(r'[A-Za-z_][\w.]*', clean):
        leaf = piece.split('.')[-1]
        if leaf and not leaf.islower() and leaf not in PLUMBING:
            out.append(leaf)
    return out


def drawn_as(annotation):
    """The drop-down this value would be drawn with, or None."""
    for name in types_of(annotation):
        if name in DRAWN_AS:
            return DRAWN_AS[name]
    return None


def collect(euddraft, a_map, keep=None):
    """Runs euddraft on the plugin and gives back what it wrote."""
    work = tempfile.mkdtemp(prefix='eudplib_symbols_')
    try:
        shutil.copy(PLUGIN, os.path.join(work, 'dump.py'))
        shutil.copy(a_map, os.path.join(work, 'in.scx'))
        io.open(os.path.join(work, 'dump.eds'), 'w', encoding='utf-8').write(
            '[main]\n\ninput: in.scx\noutput: out.scx\n\n[dump.py]\n')

        # euddraft waits for a key when it is done, so it is given one.
        subprocess.run([euddraft, 'dump.eds'], cwd=work, input=b'\n',
                       stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=600)

        written = os.path.join(work, 'eudplib_signatures.json')
        if not os.path.exists(written):
            print('euddraft wrote no signatures. Its output was:')
            log = os.path.join(work, 'dump.eds.log')
            print(io.open(log, encoding='utf-8', errors='replace').read()[-800:]
                  if os.path.exists(log) else '  (no log)')
            return None
        found = json.load(io.open(written, encoding='utf-8'))
        if keep:
            shutil.copy(written, keep)
            print('kept the signatures at %s' % keep)
        return found
    finally:
        shutil.rmtree(work, ignore_errors=True)


def report(found):
    with_signature = {k: v for k, v in found.items() if v.get('params') is not None}
    print('names eudplib offers: %d, with a signature: %d'
          % (len(found), len(with_signature)))

    drawable, partly, plain = [], [], []
    seen = collections.Counter()
    for name, entry in sorted(with_signature.items()):
        values = entry['params']
        if not values:
            plain.append(name)
            continue
        marks = [drawn_as(v.get('annotation')) for v in values]
        for mark in marks:
            if mark:
                seen[mark] += 1
        if all(marks):
            drawable.append(name)
        elif any(marks):
            partly.append(name)
        else:
            plain.append(name)

    print()
    print('every value can be a drop-down:      %4d calls' % len(drawable))
    print('some values can be a drop-down:      %4d calls' % len(partly))
    print('no value has a kind of its own:      %4d calls' % len(plain))
    print()
    print('the drop-downs this asks for, and how many values want each:')
    for name, count in seen.most_common():
        print('   %-16s %4d' % (name, count))

    print()
    print('a few calls as they would be drawn:')
    for name in (drawable + partly)[:8]:
        values = with_signature[name]['params']
        parts = []
        for v in values:
            mark = drawn_as(v.get('annotation'))
            parts.append('%s=[%s]' % (v['name'], mark) if mark else '%s=( )' % v['name'])
        print('   %-18s %s' % (name, ' '.join(parts)[:88]))


def main(argv):
    if not argv:
        print(__doc__)
        return 1

    if argv[0] == '--read':
        found = json.load(io.open(argv[1], encoding='utf-8'))
    else:
        if len(argv) < 2:
            print(__doc__)
            return 1
        found = collect(argv[0], argv[1], argv[2] if len(argv) > 2 else None)
        if found is None:
            return 1

    report(found)
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
