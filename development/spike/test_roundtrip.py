"""Checks the round trip on trees built here, where the corpus is thin.

    python development/spike/test_roundtrip.py

A folder that is switched off is the case worth pinning down. The editor writes
no code for a node that is off, and none for anything under it, so the text has
to say the same and has to say it in a way that reads back.
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from eps_roundtrip import (ACTION, CONDITION, FOLDER, FOLDER_BODY, RAW, CODE,
                           Node, Reader, Writer, load_signatures, meaningful, shape)

DATA = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)),
                                    '..', '..', 'EUD Editor', 'Data'))

failures = []


def check(title, tree, signatures, expect_lines=None):
    writer, reader = Writer(signatures), Reader(signatures)
    text = '\n'.join(writer.write(tree))
    back = reader.read(text)

    before = [shape(n) for n in meaningful(tree)]
    after = [shape(n) for n in meaningful(back)]

    ok = before == after
    if expect_lines is not None:
        for wanted in expect_lines:
            if not any(wanted in line for line in text.split('\n')):
                ok = False
                print('   wanted a line holding %r' % wanted)

    print('%-46s %s' % (title, 'ok' if ok else 'FAILED'))
    if not ok:
        failures.append(title)
        print('   text:')
        for line in text.split('\n'):
            print('     | ' + line)
        print('   before: %s' % before)
        print('   after:  %s' % after)
    return text


def action(call, *values):
    node = Node(ACTION)
    node.call = call
    node.values = list(values)
    return node


def condition(call, *values):
    node = Node(CONDITION)
    node.call = call
    node.values = list(values)
    return node


def folder(title, children, off=False):
    node = Node(FOLDER, off=off)
    node.values = [title]
    body = Node(FOLDER_BODY)
    body.children = list(children)
    node.children = [body]
    return node


def main():
    signatures = load_signatures(DATA)
    if not signatures:
        print('no signature tables under %s' % DATA)
        return 1
    print('signatures: %d' % len(signatures))
    print()

    root = Node(CODE)
    root.children = [action('Victory')]
    check('one action', root, signatures, ['Victory()'])

    root = Node(CODE)
    off = action('Victory')
    off.off = True
    root.children = [off]
    check('an action that is switched off', root, signatures, ['//@off Victory()'])

    root = Node(CODE)
    root.children = [folder('Setup', [action('Victory'), action('Draw')])]
    check('a folder', root, signatures, ['//@folder Setup', '//@end'])

    # The case this file is here for.
    root = Node(CODE)
    root.children = [folder('Setup', [action('Victory'), action('Draw')], off=True)]
    text = check('a folder that is switched off', root, signatures,
                 ['//@folder-off Setup', '//Victory()', '//Draw()'])
    for line in text.split('\n'):
        body = line.strip()
        if body and not body.startswith('//'):
            print('   a line of a folder that is off is not commented: %r' % line)
            failures.append('a folder that is switched off leaves live code')
            break

    root = Node(CODE)
    root.children = [folder('Outer',
                            [action('Victory'),
                             folder('Inner', [action('Draw')])],
                            off=True)]
    # A folder does not indent what it holds, so the mark of the folder inside
    # lands straight after the mark the folder outside put on: ////@folder.
    check('a folder inside a folder that is off', root, signatures,
          ['//@folder-off Outer', '////@folder Inner'])

    root = Node(CODE)
    inner_off = action('Draw')
    inner_off.off = True
    root.children = [folder('Outer', [action('Victory'), inner_off], off=True)]
    check('a node already off inside a folder that is off', root, signatures)

    root = Node(CODE)
    root.children = [condition('Always'), action('Victory')]
    check('a condition and an action', root, signatures)

    raw = Node(RAW)
    raw.values = ['const x = 1;\nconst y = 2;']
    root = Node(CODE)
    root.children = [raw]
    check('a raw block', root, signatures, ['//@raw', 'const x = 1;'])

    print()
    if failures:
        print('%d failed: %s' % (len(failures), ', '.join(failures)))
        return 1
    print('all of them passed')
    return 0


if __name__ == '__main__':
    sys.exit(main())
