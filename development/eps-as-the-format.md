# epScript as the format, and the GUI as a view of it

The aim is not to turn epScript back into the tree of nodes the editor keeps
today. It is the other way round: **epScript is the thing, and the editor draws
it as blocks with a drop-down for each value.** A file of epScript stays a file
of epScript, and the GUI is one way of looking at it.

Two spikes stand behind this note:

    python development/spike/eps_roundtrip.py --corpus <folder of projects>
    python development/spike/test_roundtrip.py
    python development/spike/eudplib_symbols.py <euddraft.exe> <a map.scx>

## Where this comes from

The editor already writes epScript. `TriggerToEPS` walks the tree of nodes and
writes `eudplibdata/TriggerEditor.eps`, and euddraft compiles it. One way works
and has worked for years.

What the tree costs is everything else. Today 83% of the Corona TD project file
is one block of nodes that no diff can say anything about, no one can review,
and nothing but this editor can read.

## To draw a line of epScript, the editor has to know what its values are

`CreateUnit(1, "Terran Marine", "Anywhere", 1)` is drawn as four fields: a
number, a unit from a list, a location from a list, a player from a list. That
needs a name and a kind for each value of each call.

There are two places to get that, and both are needed.

### The editor's own tables, for the classic trigger set

`Data/TriggerEditor/action.json` and `condition.json` hold 172 nodes, each with
the epScript it stands for:

| Shape | Example | Count |
| --- | --- | --- |
| A plain call | `Bring($Player$, $Comparison$, $Number$, $Unit$, $Location$)` | 119 |
| A piece of epScript | `var $Name$ = $Count$` | 53 |
| A marker with no code | `EUDPart` | 7 |

A template read backwards is a pattern: a group where each `$Value$` stands.
The same table writes a node and reads it, and it carries the value kinds the
GUI already fills lists for. This is the set the editor draws today.

### eudplib itself, for everything else

Real epScript calls things no editor table describes. eudplib says what those
take, in its own type notes, and `eudplib_symbols.py` asks the installed
euddraft for them.

It cannot be read from the outside. eudplib ships compiled for a Python the
machine may not have (the copy here is built for 3.13; this machine has 3.12)
and its native part is built for another one again. So the question goes to
euddraft, which carries the eudplib the user builds with: euddraft runs a
plugin, the plugin reads its own eudplib, and writes down what it finds.

From euddraft 0.10.2.5:

| | |
| --- | --- |
| Names eudplib offers | 503 |
| With a signature | 501 |
| Every value can be a drop-down | 41 calls |
| Some values can be a drop-down | 44 calls |
| No value has a kind of its own | 416 calls |

The kinds it asks for are the lists the editor already fills:

    Unit 38   Player 37   Location 27   Text 18   Comparison 13
    Modifier 11   Count 9   ResourceType 6   ScoreType 6   Property 3
    Switch 2   Order 1   AIScript 1   AllyStatus 1   SwitchAction 1
    SwitchState 1   UnitOrder 1

So `CommandLeastAt(unit, location)` draws itself: a unit list and a location
list, from eudplib's own words, with nothing written by hand. The 416 with no
typed values are mostly the low-level helpers; they still draw as a block, with
a plain field for each value instead of a list.

## What belongs to the editor rides in a comment

Three things the editor knows and epScript has no word for: a node that is
switched off, a folder, and whether a folder is folded. They go in as comments,
so euddraft passes over them and a person still reads them:

    //@folder Setup
        CreateUnit(1, "Terran Marine", "Anywhere", 1);
        //@off DisplayText("debug", 4)
    //@end

A switched-off node is a commented-out node, which is what a reader would take
it to mean anyway.

A folder that is switched off takes everything in it with it, because that is
what the editor does: `ToCode` writes nothing for a node that is off and
nothing for anything under it. So the whole block is commented, a line at a
time:

    //@folder-off Old wave code
    //CreateUnit(1, "Zergling", "Anywhere", 2);
    //DisplayText("wave 3", 4);
    //@end

One mark goes on and one comes off, so a folder inside a folder keeps its own
state. Marking each line rather than wrapping the block in `/* */` matters: the
blocks people already keep by hand hold comments of their own, and a wrapper
would end at the first one.

## Anything else stays raw

A line the editor cannot draw is shown as itself, in a block the user can type
in. The tree already has this idea in `RawString`, and one of the five projects
in the corpus, Pictionary, is written almost entirely that way. People already
use the editor as a place to keep epScript.

This is what makes the whole thing safe: nothing has to be understood before it
can be opened.

## What the round-trip spike shows

`eps_roundtrip.py` writes the tree of a project as epScript and reads it back,
over five published projects, 11,287 lines:

| Project | Lines | Hold still |
| --- | --- | --- |
| Maze D ReforgeD | 2,556 | **99.0%** |
| Random Tower Survival | 219 | 52.5% |
| Pictionary | 575 | 32.7% |
| Ad Infinitum | 1,070 | 11.6% |
| Corona TD | 6,867 | 5.7% |

"Hold still" means: write it, read it, write it again, and the two agree. Maze D
at 99% is the finding. Where a project uses the kinds the spike models, the text
holds. The low ones lean on kinds it has not modelled: classic triggers
(`24`, `26`, `27`), `while`, `for`, `Wait`, `switch`. The spike names them on
every run, with ten templates that take fewer values than the node keeps, such
as `CreatePlayerVariable`, written `const $Name$ = [0, 0, ...]` while the tree
keeps a starting value too.

## Keeping euddraft, and the symbols, current

The editor asks the user where euddraft is, so it does not know which version it
is talking to. The symbols above are only true for the version they came from.

1. The editor fetches euddraft itself, into a folder of its own under the user's
   profile, so it knows the version it has.
2. euddraft can update itself. The editor asks it to on start, and so learns
   when the version changed.
3. When it changes, the editor reads the symbols again, the same way
   `eudplib_symbols.py` does, and says what appeared or went away.

Step 3 is cheap because it is one euddraft run against a small map, and it is
worth doing on a version change alone.

## The order to do this in

1. **Item 04, the reference build.** Nothing else is safe without it.
2. **Read the symbols on start**, from the euddraft that is installed. That is
   the whole of the drop-down question answered, for every call, without a
   table written by hand.
3. **Model the kinds that are left**, and fill in the ten templates. Both lists
   print on every run, so what is left is a number.
4. **Then** the file the editor saves can be `.eps`, and the change is a change
   of file name.
