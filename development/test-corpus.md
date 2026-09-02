# Test corpus

Real projects, used to check that a change does not break an editor people use.
The maps belong to their authors and are **not** in this repository. They live
outside it:

    C:\Users\kalle\devel\EUDEditor-testdata

with the two archives they came from beside them.

| Project | Saved by | Triggers walked | Notes |
| --- | --- | --- | --- |
| Ad Infinitum ReforgeD | 0.17.8.2 | 84, 8 players | |
| Corona TD ReforgeD | 0.17.9.7 | 347, 9 players | The largest: 819 KB, 8,468 elements |
| Maze D ReforgeD v0.2.0 | 0.17.8.2 | 47, 5 players | |
| Pictionary ReforgeD | 0.18.x | trigger list empty | |
| Random Tower Survival ReforgeD | 0.18.x | trigger list empty | |

## Preparing a project

A project file holds the full path of its map, from the machine that saved it.
Point it at the copy beside it, in bytes, because the file holds text in more
than one encoding and a decode and re-encode loses it:

```python
pat = re.compile(b'(InputMap : )([^\r\n]*)')      # and OutputMap
```

## What the corpus is for

- **Loading.** Open each one. The error log must stay empty.
- **Walking.** Open every main tab, every kind of data, every sub tab, and every
  trigger of every player. The error log must stay empty.
- **Saving.** A save with no edit must give back the same bytes.

The walk found three faults that the test project could not: two reads outside
the range of a list in the trigger value parser, and one in the checkbox lists.
Two of the projects are older than the current build, which also tests reading
a file an older version wrote.
