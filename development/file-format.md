# Project file changes

The project file (`.e2s`, and the same text inside `.e2p`) is read section by
section. A reader asks for one section by name, from `S_<name>` to `E_<name>`, and
never reads the file line by line. A section a reader does not know is therefore
passed over.

This file records each change to the format, so an older build can be tested
against a newer file.

## How to test an old build

1. Take a build from before the change.
2. Open a project file written by the new build.
3. Check the values the old build knows are all there.
4. Save with the old build, and open that file with the new build.

Step 4 is the one that loses data: an old build writes only the sections it knows,
so anything newer is dropped on that save.

## Changes

### 2026-09-02 — `S_BookmarkSET`

Bookmarks of the project: one line for each, `Bookmark : <kind of data>,<entry>`.

The section is written only when the project holds a bookmark, so a project with
none writes a file byte for byte the same as before.

| Direction | Result |
| --- | --- |
| New build reads an old file | No section. No bookmarks. Nothing else changes. |
| Old build reads a new file | Expected to pass over the section. **Not yet tested against an old binary.** |
| Old build saves a new file | Bookmarks are lost. Everything else is kept. |

What was tested: a project file holding a section no build knows
(`S_FutureThingSET`) opens in the current build with every setting intact and
nothing in the error log. That tests the mechanism an old build uses, on the
current build. It is not the same as running an old binary, which is still to do.
