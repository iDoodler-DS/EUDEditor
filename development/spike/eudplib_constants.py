"""Lines the editor's drop-downs up against the constants eudplib names.

A drop-down holds a place in a list; epScript reads better written the way a
person writes it, with `Player1` and `AtLeast`. eudplib names those constants
and says what each stands for, so the two only have to be matched up.

They match on the name with the spaces taken out: "Set To" is `SetTo`, "Ore and
Gas" is `OreAndGas`, "Player 1" is `Player1`. Nothing is guessed: a list entry
that matches no constant is left out, and the editor writes its place in the
list for that one, as it did before.

    python development/spike/eudplib_constants.py <euddraft.exe> <a map.scx>
    python development/spike/eudplib_constants.py --read <eudplib_constants.json>

The second form works from a dump kept earlier. Either way the table lands in
Data/TriggerEditor/eudplib_constants.json, which the editor reads.
"""

import io
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(os.path.dirname(HERE))
PLUGIN = os.path.join(HERE, 'euddraft_dump_constants.py')
DEFS = os.path.join(ROOT, 'EUD Editor', 'Module', 'ValueDefsModule.vb')
LANG = os.path.join(ROOT, 'EUD Editor', 'Data', 'Language', 'English')
OUT = os.path.join(ROOT, 'EUD Editor', 'Data', 'TriggerEditor', 'eudplib_constants.json')

# What eudplib calls a family of constants, against the list the editor fills.
# The same names eudplib_symbols.py uses, read the other way round.
FAMILY_OF = {
    'Player': 'TrgPlayer', 'PlayerX': 'TrgPlayer', 'Owner': 'TrgPlayer',
    'NewOwner': 'TrgPlayer', 'ForPlayer': 'TrgPlayer',
    'Comparison': 'TrgComparison',
    'Modifier': 'TrgModifier', 'TimeModifier': 'TrgModifier',
    'ResourceType': 'TrgResource',
    'ScoreType': 'TrgScore',
    'OrderType': 'TrgOrder',
    'State': 'TrgSwitchAction',
    'SState': 'TrgSwitchAction',
    'CState': 'TrgSwitchState',
}


# Two entries the editor and eudplib call by different words. Everything else
# matches on its own name; these are written down because a person had to read
# both lists to see they are the same thing.
ALIAS = {
    'closed': 'Cleared',      # a switch that is not set
    'randomize': 'Random',    # set a switch either way
}


def plainly(text):
    """A name with nothing in it but its letters and digits, in lower case."""
    return re.sub(r'[^a-z0-9]', '', (text or '').lower())


def lists_of_the_editor():
    """Each kind the editor draws as a numbered list, and what it shows."""
    source = io.open(DEFS, encoding='utf-8-sig').read()
    source = re.sub(r'\n\s*&\s*\n', ' ', source).replace('\n', ' ')

    out = {}
    for found in re.finditer(
            r'ValueDefiniction\.Add\(New ValueDefs\((.*?),\s*ValueDefs\.OutPutType\.(\w+)\s*'
            r'(?:,\s*\{(.*?)\})?\s*\)\)', source):
        names, kind, written = found.group(1), found.group(2), found.group(3)
        if kind not in ('ListNum', 'ComboboxNum'):
            continue
        for name in re.findall(r'"([^"]+)"', names):
            # A list is kept in a file of its own where there is one, and
            # written into the definition where there is not.
            beside = os.path.join(LANG, name + '.txt')
            if os.path.exists(beside):
                shown = [line.rstrip('\r\n') for line
                         in io.open(beside, encoding='latin-1').read().split('\n')]
                out[name] = [one for one in shown if one != '']
            elif written:
                out[name] = re.findall(r'"([^"]*)"', written)
    return out


def collect(euddraft, a_map):
    """Runs euddraft on the plugin and gives back the constants it wrote."""
    work = tempfile.mkdtemp(prefix='eudplib_constants_')
    try:
        shutil.copy(PLUGIN, os.path.join(work, 'dump.py'))
        shutil.copy(a_map, os.path.join(work, 'in.scx'))
        io.open(os.path.join(work, 'dump.eds'), 'w', encoding='utf-8').write(
            '[main]\n\ninput: in.scx\noutput: out.scx\n\n[dump.py]\n')
        subprocess.run([euddraft, 'dump.eds'], cwd=work, input=b'\n',
                       stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=600)
        written = os.path.join(work, 'eudplib_constants.json')
        if not os.path.exists(written):
            print('euddraft wrote no constants.')
            return None
        shutil.copy(written, os.path.join(HERE, 'eudplib_constants.json'))
        return json.load(io.open(written, encoding='utf-8'))
    finally:
        shutil.rmtree(work, ignore_errors=True)


def match(constants, lists):
    """The place in each list against the constant that stands for it."""
    table, report = {}, []
    for kind, family in sorted(FAMILY_OF.items()):
        shown = lists.get(kind)
        if not shown:
            report.append((kind, 0, 0, 'the editor has no list for it'))
            continue

        # Every constant of this family, by its plain name. eudplib gives some
        # of them more than one name; what it prints as is the one to write.
        named = {}
        for entry in constants.values():
            if family not in entry['family']:
                continue
            named.setdefault(plainly(entry['prints']), entry['prints'])

        lined = {}
        for at, one in enumerate(shown):
            standing = named.get(plainly(one)) or named.get(
                plainly(ALIAS.get(plainly(one), '')))
            if standing:
                lined[str(at)] = standing
        if lined:
            table[kind] = lined
        report.append((kind, len(lined), len(shown), ''))
    return table, report


def main(argv):
    if not argv:
        print(__doc__)
        return 1

    if argv[0] == '--read':
        constants = json.load(io.open(argv[1], encoding='utf-8'))
    else:
        if len(argv) < 2:
            print(__doc__)
            return 1
        constants = collect(argv[0], argv[1])
        if constants is None:
            return 1

    lists = lists_of_the_editor()
    table, report = match(constants, lists)

    print('constants eudplib names: %d' % len(constants))
    print()
    print('%-14s %8s  %s' % ('the list', 'matched', 'of the entries it shows'))
    for kind, hit, total, why in report:
        print('%-14s %8s  %s' % (kind, '%d' % hit if total else '-',
                                 why or '%d' % total))

    io.open(OUT, 'w', encoding='utf-8').write(json.dumps(table, indent=1, sort_keys=True))
    print()
    print('wrote %s' % OUT)
    for kind in sorted(table):
        shown = lists[kind]
        pairs = ['%s=%s' % (shown[int(at)], name)
                 for at, name in sorted(table[kind].items(), key=lambda p: int(p[0]))]
        print('   %-14s %s' % (kind, ', '.join(pairs)[:96]))
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
