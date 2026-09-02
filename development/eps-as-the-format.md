# epScript as the format the editor saves

A note on what it would take, with a spike that shows the shape of the work.
Run it with:

    python development/spike/eps_roundtrip.py --corpus <folder of projects>
    python development/spike/test_roundtrip.py

## Where this comes from

The editor already writes epScript. `TriggerToEPS` walks the trigger tree and
writes `eudplibdata/TriggerEditor.eps`, and euddraft compiles that. One way
works and has worked for years.

The question is the other way. If the same text can be read back into the tree
it came from, epScript can become what the editor saves, and a trigger becomes
something a person can read, diff and review. Today 83% of the Corona TD
project file is one opaque block of trigger tree that no diff can speak about.

## Signatures come from the editor, not from eudplib

`Data/TriggerEditor/action.json` and `condition.json` describe every node the
GUI can draw, and each carries the epScript it stands for:

| Shape | Example | Count |
| --- | --- | --- |
| A plain call | `Bring($Player$, $Comparison$, $Number$, $Unit$, $Location$)` | 119 |
| A piece of epScript | `var $Name$ = $Count$` | 53 |
| A marker with no code | `EUDPart` | 7 |

A template written backwards is a pattern: put a group where each `$Value$`
stands, and the same table that writes a node also reads it. The spike does
this and needs nothing from eudplib itself.

That matters, because eudplib ships as compiled `.pyc` for a Python the machine
may not have (the copy here is built for 3.13; the machine has 3.12), and its
native binding is built for yet another. Reading signatures out of it would mean
matching all of that. The editor's own tables are the set the GUI can draw,
which is the only set worth reading back anyway.

## What belongs to the editor rides in a comment

The tree holds three things epScript has no word for: a node that is switched
off, a folder, and whether a folder is folded. They go in as comments, so
euddraft passes over them and a person still reads them:

    //@folder Setup
        CreateUnit(1, "Terran Marine", "Anywhere", 1);
        //@off DisplayText("debug", 4)
    //@end

A switched-off node is a commented-out node. That is what a reader would expect
it to mean, and it is what the editor means by it.

A folder that is switched off takes everything in it with it. The editor writes
no code for a node that is off and none for anything under it, so the text
comments the whole block, a line at a time:

    //@folder-off Old wave code
    //CreateUnit(1, "Zergling", "Anywhere", 2);
    //DisplayText("wave 3", 4);
    //@end

One mark goes on and one mark comes off, so a folder inside a folder keeps its
own mark and reads back with its own state. Marking each line rather than
wrapping the block in /* */ matters: the blocks people already keep in raw
nodes contain comments of their own, and a wrapper would end at the first one.

## Anything else stays raw

A node with no template is written as it stands and read back as one block. The
tree already has this idea: `RawString`, which holds hand-written epScript. One
of the five projects in the corpus, Pictionary, is written almost entirely that
way, so people already use the tree as a container for text.

## What the spike shows

Across five published projects, 11,287 lines of generated epScript:

| Project | Lines | Hold still | Raw |
| --- | --- | --- | --- |
| Maze D ReforgeD | 2,556 | **99.0%** | 1 |
| Random Tower Survival | 219 | 52.5% | 1 |
| Pictionary | 575 | 32.7% | 1 |
| Ad Infinitum | 1,070 | 11.6% | 1 |
| Corona TD | 6,867 | 5.7% | 1 |

"Hold still" means: write the tree as text, read it back, write it again, and
the two texts agree. Maze D at 99% is the finding that matters. Where a project
uses the kinds the spike models, the text is a faithful place to keep it. The
low scores are not a different problem; they are the same one, in projects that
lean on kinds the spike has not modelled yet.

### What is left to model

The kinds with no template, by the number in `ElementType`:

- `0` main, `25` RawTriggers — containers that only hold other nodes.
- `24` RawTrigger, `26` TriggerCond, `27` TriggerAct — a classic trigger, which
  has a shape of its own that epScript writes as a function.
- `9` while, `10` for, `20` Wait, `28`/`29` switch — control flow the writer
  covers for `if` but not yet for these.

Nesting these behind a marker of their own was tried and read back worse than
leaving them flat, so the spike leaves them flat and counts them as work.

### Templates that cannot hold every value

Ten templates take fewer values than the node keeps, so a value has nowhere to
go in the text. The biggest are:

| Node | Nodes | Template takes |
| --- | --- | --- |
| CreateVariableWithNoini | 151 | 1 |
| PreserveTrigger | 138 | 0 |
| DisplayCText | 125 | 1 |
| ChatAnnouncement | 124 | 1 |
| CreatePlayerVariable | 60 | 1 |

`CreatePlayerVariable` is written `const $Name$ = [0, 0, 0, 0, 0, 0, 0, 0];`,
but the tree keeps a starting value beside the name. Each of these needs either
a fuller template or a `//@` note beside it. The list is the work, and it is
short.

## Keeping euddraft, and the symbols, current

The editor asks the user where euddraft is. That means the editor does not know
which version it is talking to, and the templates above are only true for the
version they were written against.

The plan:

1. The editor fetches euddraft itself, into a folder of its own under the user's
   profile, so it knows the version it has.
2. euddraft can update itself. The editor asks it to, on start, and finds out
   when the version changed.
3. When the version changes, the editor builds its symbols again and says so.

Only step 3 needs thought. The tables in `Data/TriggerEditor` are the editor's
own and do not come from euddraft, so what the build has to check is that they
still agree with the euddraft that is installed. A compile of a small project
that uses every template, compared against a kept result, answers that. Which is
roadmap item 04, the reference build, wearing a second hat.

## The order to do this in

1. **Item 04, the reference build.** Nothing else is safe without it.
2. **Model the kinds that are left**, and fill in the ten templates. The spike
   reports both lists every time it runs, so progress is a number.
3. **Only then** consider making `.eps` the file the editor saves. By that
   point the parser has been proven against every published project we have,
   and the change is a change of file name.
