# The Script tab: a trigger editor whose source is epScript

A second trigger editor, on a main tab of its own called **Script**. The trigger
editor of today is untouched and still on the **Triggers** tab.

The difference is what is kept. The old editor keeps a tree of nodes and writes
epScript out of it when it builds. This one keeps the epScript, and the tree is
a way of looking at it. Nothing is converted, so nothing is lost: a line the
editor cannot draw is still that line, spelled the way it was written.

## What it does

The commands are the ones the old editor puts on its own tree, on a right click:

    New >    Folder, Comment, Action, Condition,
             If, Else if, Else, While, For
    Fold, Unfold, Fold all, Unfold all
    Edit, Turn off
    Cut, Copy, Copy as text, Paste, Delete
    Move up, Move down

with Enter to edit, Delete to remove, and Ctrl with X, C, V, Up and Down.

- **A tree, like the advanced view.** Folders, functions, `if` and `while`
  blocks nest; each statement is a node.
- **A node reads as words, not as code.** A call the editor has a sentence for
  is shown as that sentence, the same one the edit window shows: *Create 1
  Terran Marine at 'Anywhere' for Player 1.* A condition reads the same way.
  A call the editor has no words for is shown as it is written, so nothing is
  ever hidden.
- **A node is edited in a window of its own**, opened by a double click or by
  Enter, and nothing is edited as text if the editor knows what it is:

  | What it is | What the window shows |
  | --- | --- |
  | A call | Which call, from a list of all 506, and each of its values with the list that fills it |
  | An if, else if or while | One row for each condition: which condition, then its values. Conditions can be added and taken away |
  | A function | Its name, and a row for each argument |
  | A for | The variable, where it starts, what it counts to, and what it does each time |
  | A folder | Its name |
  | Anything else | Itself, in a box, so nothing is ever out of reach |

  The line it will write is shown underneath as it is built. Changing which
  call a row holds draws the rest of the row again, with each value at what it
  starts as.
- **A new value starts at the first of its list**, because that is a value the
  game will take. A value with no list starts at 0.
- **A list shows a word and writes a number.** See below.
- **Turn a node off.** It is written as a comment, so euddraft passes over it
  and a person still reads it. A folder that is off takes everything in it.
- **Edit as text.** The source is shown beside the tree, coloured the way
  TypeScript is — epScript is spelled the same way — with a colour of its own
  for a call the editor knows and for a constant eudplib names, because those
  are what a person looks for when reading a trigger. It can be edited
  directly; pressing the button again reads it back into the tree. The colours
  are chosen against whatever ground the theme gives the box, so both the light
  and the dark theme read.
- **Build map.** Writes the source and a settings file, and runs euddraft on
  them, with the output going to the same log at the foot of the window. It
  writes its own map, `[Script] <the project's output map>`, beside the
  project's — never over it, because this build has only the epScript in it.
- **A sample to look at.** `development/sample.triggers.eps` shows every kind
  of node at once. Copy it beside a project as `<the project name>.triggers.eps`
  and open that project.

### What the new items write

    Folder      //@folder <name> ... //@end
    If          if (Always()) { }
    Else if     else if (Always()) { }
    Else        else { }
    While       while (Always()) { }
    For         for (var i = 0; i < 10; i++) { }

An else or an else if is put after the block it follows, not inside it. The
spellings are the ones euddraft's own sample uses, and all of them were put
through euddraft to check that they compile.

A condition is a test, so it joins the head of the `if`, `else if` or `while`
that is selected, with `&&`. On anything else it starts an `if` of its own.

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
runs euddraft on them, into a map of its own called `[Script] <the project's
output map>`. That build has **only** the epScript in it. It leaves out:

- everything the data editors changed
- button sets and requirements
- the plugins
- the triggers of the old editor

So it is a way of trying the source out, not a replacement for the build on the
toolbar, which is unchanged and does not know about the epScript source at all.
That is why it writes its own map: it built over the project's finished map
once, and a triggers-only build is not what anybody wants in its place.

### The top level is three blocks and nothing else

euddraft calls three functions, and they are the whole of the top level:

    function onPluginStart() { }
    function beforeTriggerExec() { }
    function afterTriggerExec() { }

The editor puts back any that a file does not have, will not let them be
edited, moved, switched off or deleted, and puts everything new inside one of
them: whichever one the selection sits under, or `onPluginStart` when the
selection is outside them all.

Anything else already at the top level of a hand-written file is left where it
is and still shown. Only blank lines between the blocks are dropped, because
they cannot be picked and stand for nothing.

**New ▸ Function is gone.** epScript does not nest a function inside a
function, and nothing may be added outside the three, so a new one has no home.

### A drop-down shows a word and writes the constant

eudplib names a constant for nearly everything these lists hold, and a name
says what it means where a number does not:

| The kind | What the list shows | What is written |
| --- | --- | --- |
| Player, Modifier, Comparison, Score, Resource, Order, Switch | `Player 2`, `Set To`, `At Least` | `Player2`, `SetTo`, `AtLeast` |
| Unit, Location, Text | `Terran Marine`, `Anywhere` | the same in quotes: `"Terran Marine"` |
| Number, Count | the number | the number |

**The number is not the place in the list.** `Set To` is the first entry the
editor offers and eudplib's `SetTo` is 7; `Exactly` is the third and is 10.
Writing the constant is what makes that come out right without the editor
holding a table of numbers of its own.

The names are matched to the lists ahead of time, on the name with the spaces
taken out — "Set To" is `SetTo`, "Ore and Gas" is `OreAndGas` — and kept in
`Data/TriggerEditor/eudplib_constants.json`. Two entries the two sides call by
different words (`Closed` is `Cleared`, `Randomize` is `Random`) are written
down in the spike that builds the table. To build it again:

    python development/spike/eudplib_constants.py <euddraft.exe> <a map.scx>

A list entry eudplib has no name for — the four `Unknown` players and
`NonAlliedVictoryPlayers` — is written as its place in the list, which for a
player is also its number.

A value the editor cannot match to a choice is left exactly as it was written
and the window opens on **Custom**, so an expression such as
`SetDeaths(foo + 1, ...)` survives being opened and saved.

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
- No search. A node moves up and down among its own kin, but not from one
  block into another except by cut and paste.
- Switch and case are not offered, because the old editor writes them as a
  state machine of its own rather than as anything epScript spells.
- The classic triggers of a project are not offered for import.
- A statement is read a line at a time, so a call written across several lines
  is one node holding all of them, and its fields are not drawn.
