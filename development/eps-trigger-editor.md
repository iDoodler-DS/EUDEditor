# The Script tab: a trigger editor whose source is epScript

A second trigger editor, on a main tab of its own called **Script**. The trigger
editor of today is untouched and still on the **Triggers** tab.

The difference is what is kept. The old editor keeps a tree of nodes and writes
epScript out of it when it builds. This one keeps the epScript, and the tree is
a way of looking at it. Nothing is converted, so nothing is lost: a line the
editor cannot draw is still that line, spelled the way it was written.

## What it does

- **A tree, like the advanced view.** Folders, functions, `if` and `while`
  blocks nest; each statement is a node.
- **Fields for a line it knows.** Pick `CreateUnit(...)` and its four values are
  named and typed: Unit, Where and ForPlayer are drop-downs filled from this
  project, Number is a box. 506 names are known.
- **Turn a node off.** It is written as a comment, so euddraft passes over it
  and a person still reads it. A folder that is off takes everything in it.
- **Edit as text.** The source is shown beside the tree and can be edited
  directly; pressing the button again reads it back into the tree.
- **Build map.** Writes the source and a settings file, and runs euddraft on
  them, with the output going to the same log at the foot of the window.

## Where the source is kept

Beside the project file, with the same name:

    survival.e2s
    survival.triggers.eps        <- here

It is written whenever the project is saved, and read whenever a project opens.

## What is not backward compatible

This is the part to account for.

### The source file travels separately

The `.eps` is a file of its own. An editor that does not know about it will not
carry it: **Save As in an older build writes the `.e2s` and leaves the triggers
behind.** Copying a project by hand means copying both files.

The same goes for a `.e2p`, where the project is a folder. The source lands in
that folder, but nothing else in the packing and unpacking knows about it yet.

### The two editors do not share

A trigger made on the Script tab does not appear on the Triggers tab, and the
other way round. There is no conversion in either direction. A project can hold
both, and both will be kept, but they are two sets of triggers.

The old editor's triggers still live in the `S_TriggerEditorSET` section of the
project file, exactly as before.

### Build map on the Script tab builds only that source

It writes `eudplibdata/EpsTriggers.eps` and `eudplibdata/EpsTriggers.eds`, and
runs euddraft on them. That build has **only** the epScript in it. It leaves out:

- everything the data editors changed
- button sets and requirements
- the plugins
- the triggers of the old editor

So it is a way of trying the source out, not a replacement for the build on the
toolbar, which is unchanged and does not know about the epScript source at all.

### The comment marks are part of the format

Four marks carry what the editor knows and epScript has no word for:

    //@folder <name>        a folder
    //@folder-off <name>    a folder that is switched off
    //@end                  the end of a folder
    //@off <line>           a line that is switched off

A person who writes one of those by hand will find the editor takes it at its
word. They are ordinary comments to euddraft either way.

### Opening and saving reformats

The reader keeps blank lines and comments, but it lays the text out again on
save: four spaces to a level, and the line endings of this platform. So the
first save after opening a hand-written file will show as a change, even when
nothing was edited. After that it holds still, which the spike measures.

### The list of names is a snapshot

`Data/TriggerEditor/eudplib_signatures.json` was read from euddraft 0.10.2.5. A
user on another version may have names this file does not, or the other way
round. Nothing breaks: an unknown call is drawn as a plain line and built
unchanged. To read them again:

    python development/spike/eudplib_symbols.py <euddraft.exe> <a map.scx> out.json

## What it is not yet

- No undo of its own. The Edit menu of this tab does nothing yet.
- No search, and no move of a node from one place to another except by editing
  the text.
- The classic triggers of a project are not offered for import.
- A statement is read a line at a time, so a call written across several lines
  is one node holding all of them, and its fields are not drawn.
