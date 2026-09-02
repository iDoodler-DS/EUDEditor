# EUD Editor 2 SE — Developer Guide

This file provides guidance to Claude Code (claude.ai/code) and human contributors working in this repository.

EUD Editor 2 SE is a fork of armoha/EUDEditor: a VB.NET WinForms front-end for building EUD-powered StarCraft: Brood War maps. The GUI edits triggers, `.dat` tables, FireGraft requirements, GRPs, MPQs, etc., then generates eudplib Python + epScript and shells out to an external `euddraft.exe` to inject the result into the map. SE is *not* backwards compatible with EUD Editor 2 (SE projects don't load in EE2; EE2 projects load in SE).

Where this fork is going: [`roadmap.html`](roadmap.html) (open it in a browser). Four phases in dependency order — stability, folding the overlapping windows into one surface per job, replacing the trigger system's format/model/generator, then the features that unlocks. It is the plan of record; keep it updated as items land.

## Build

Single project, .NET Framework 4.8, **x86 only**. There are no tests and no CI.

```powershell
$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

# One-time (packages/ is git-ignored; packages.config, not PackageReference)
& $msbuild "EUD Editor.sln" -t:restore -p:RestorePackagesConfig=true

# Build -> "EUD Editor\bin\x86\Release\EUD Editor SE.exe"
& $msbuild "EUD Editor.sln" -p:Configuration=Release -p:Platform=x86 -m -v:minimal
```

- Use `-p:Platform=x86`. The `.vbproj` also carries AnyCPU configs with `Prefer32Bit=false`; a 64-bit process crashes with `BadImageFormatException` on the native DLLs.
- The pre-build event `taskkill`s a running `EUD Editor SE.exe`; expect that line in the log.
- `dll\**`, `Data\**`, and `TEFunction\**` are `Content` items copied flat into the output folder. The app reads *and writes* `Data\` and `TEFunction\` under its own directory at runtime, so running from `bin\` mutates the copied tree, not the source tree.
- `CascLib.dll` is P/Invoked (`Module\CascLib.vb`) but is not shipped in `dll\`; Remastered CASC paths fail unless the user supplies it. `ffmpeg.exe` is likewise expected next to the exe for BGM transcoding.
- Running the app needs paths to `StarCraft.exe`, `euddraft.exe`, and the four game MPQs (`Patch_rt`, `BrooDat`, `BroodWar`, `StarDat`); the settings dialogs are forced open until they exist.

## Versioning and releases

- The authoritative version string is `ProgramSet.Version` in `EUD Editor\Module\SettingModule.vb`. `AssemblyInfo.vb` (`0.0.0.0`) and the `.vbproj` `ApplicationVersion` are unused.
- `version\version` is a two-line manifest (`<version>` then the release zip URL) fetched from `master` by the in-app update check; `version\PatchNote` is the changelog, newest block first. Bump all three together for a release.
- The update check is plain string inequality, so a downgrade also reports "update available". `Data\EUDEditorUpdate.exe` is a cx_Freeze build of `Updater\EUDEditorUpdate.py` (no signature verification by design).

## Architecture

### Global state

Three module-level "singletons" hold nearly all state:

- **`ProgramSet`** (`Module\SettingModule.vb`): app-wide settings copied from `My.Settings` at startup: StarCraft/euddraft paths, `StarVersion` (`"1.16.1"` or `"Remastered"`), MPQ paths, theme colors.
- **`ProjectSet`** (same file): the open project. Filename, input/output map, `UsedSetting()` feature flags (indexed by `Enum Settingtype`), parsed CHK data, and `Load`/`Save`.
- **`ProgramData.vb`**: static game data (`stat_txt`, `CODE()` name lists, `DatEditDATA As List(Of CDatEdit)`, wireframes) and plugin toggles.

Startup is `Module\initModule.vb init()`: language fallback (unknown language falls back to **Korean**, not English), single-instance check, settings copy, forced setup dialogs, `DataLoad()`, HKCU file-association writes for `.e2s/.e2p/.ees/.mem` on every launch, then the update check.

### Project files

- **`.e2s`**: single-file project in a custom line-oriented text format, `S_<Section>` ... `E_<Section>` blocks of `Key : Value`, parsed with `FindSection`/`FindSetting` in `parsingModule.vb`. Sections map 1:1 to editor subsystems; `TriggerEditorSET` embeds the serialized trigger tree.
- **`.e2p`**: "packed" project. A folder named after the file holding the `.e2p` plus `Resource\`, `Map\`, `Grp\`, `Sound\`, `eudplibdata\`, `temp\`. Managed by `Module\EUDProjectManagerModule.vb`. Saving calls `DeleteDumpFileAll()`, which removes unreferenced files from those folders.
- `.ees` (EUD Editor 1) and `.mem` (memory dump) are import-only.
- `AESModule`, `CRC32`, and `CheckSumMoudle` do **not** protect project files. They serve SCDB only (login obfuscation and a post-build CRC patch written into the output map's CHK).

### Trigger editor

Three layers, all under `Module\Trigger.vb`, `Module\TriggerEditorDataMoudle.vb`, `Form\TriggerEditor\`:

1. **Action/condition catalogue** is data-driven from `Data\TriggerEditor\action.json` and `condition.json`, deserialized into `Class Action` / `Class Condiction` (sic). Each entry has `Texts` (alternating language name / label pairs), `CodeText` (epScript with `$Placeholder$` tokens), and `ValuesDef` names that resolve to widget types via `Module\ValueDefsModule.vb`.
   **Tab grouping is positional.** The flat list is split at sentinel entries named `EUDPart`, `STRUCTPart`, `CUSTOMPart` into the Classic / EUD / Structure / Custom tabs. New SE actions go at the **end of `action.json`, after `CUSTOMPart`**. Inserting elsewhere silently moves an action to another tab.
2. **Trigger tree**: `Class Element` is a generic n-ary node typed by `Enum ElementType` (members are Korean identifiers). `ToSaveFile()`/`LoadFile()` is the on-disk format (`Type:n,disabled,folded,not` / `act:Name` / values joined by the separator constant `ஐ` / `ElementsCount:n` / children / `END`). `ToCode()` emits **epScript**, not Python. `TriggerToEPS()` assembles the whole `TriggerEditor.eps` and tracks line numbers so euddraft `[Error] ... Line N` output maps back to nodes.
3. **`.tfn` files** in `EUD Editor\TEFunction\` are exactly one serialized function-definition subtree (same grammar, same `ஐ` separator). They are loaded by name at runtime, not compiled in. The `Korean`/`English` tooltip text lives inside the file as `RawCode` fragments. **EPD variants are duplicated files** (`Foo.tfn` / `FooEPD.tfn`) with no shared code; fix both.

### Build pipeline (map compilation)

Entry point is `eudplib.Toflie()` in `Module\eudplibModule.vb`. It writes into `<appdir>\Data\eudplibdata\` (or the `.e2p` folder):

| File | What it is |
|---|---|
| `EUDEditor.eds` | euddraft plugin manifest (INI-like): `[main]` input/output, then one section per enabled plugin (`[EUDEditor.py]`, `[TriggerEditor.eps]`, `[MSQC]`, `[dataDumper]`, `[grpinjector]`, `[iscriptPatcher]`, `[unlimiter]`, `[eudTurbo]`, ...). User "extra eds" text is merged section-wise. |
| `EUDEditor.edd` | Same content, different extension: euddraft runs/debugs the map instead of injecting and exiting. |
| `EUDEditor.py` | eudplib Python: GRP declarations plus one big `DoActions([...])` in `onPluginStart()`. DatEdit and FireGraft edits become `SetMemory(0x<addr>, Add/Subtract, delta)` using `Data\Offset*.txt`. |
| `TriggerEditor.eps` | epScript from the trigger tree. |
| `EUDEditorDebug.py` | From `Resources\EUDEditorDEBUG.txt`, created/deleted by `CreateDebugpyModule.vb`. |

`Form\BulidForm.vb` (sic) launches `ProgramSet.euddraftDirec` with the `.eds` path as its only argument, polls stdout/stderr in a `BackgroundWorker` busy-loop, and treats non-empty stderr or `[Error]` in stdout as failure. It mutates WinForms controls from the worker thread; that is a known latent bug, not a pattern to copy. On success with SCDB enabled it runs the CHK checksum patch.

### .dat editor and data files

- Each `Data\*.dat` has a sibling `*.def` schema (`[HEADER]` counts, `[FORMAT]` `<i>Name=`/`<i>Size=`/`VarStart`/`VarEnd`). `ProgramData.vb ReadDEF`/`ReadDATAFile` build `Class CDatEdit`, which keeps `data` (stock), `mapdata` (from CHK) and `projectdata` (**deltas only**; `0` means unchanged, so `Save` writes only non-zero entries).
- Field to memory address is `Data\Offset1.16.1.txt` / `OffsetRemastered.txt`, read by `parsingModule.vb ReadOffset`, which takes a fixed-width substring: **offsets must be exactly 6 hex digits**. Field to widget is `Data\ValueDef.txt`.
- Display name lists (`Data\Language\<Lang>\Units.txt`, `Orders.txt`, ...) are loaded positionally into `CODE()`: **line index == in-game ID**. Never reorder or insert lines.

### Other subsystems

- **FireGraft** (`ReqModule.vb`, `Data\reqopcode.txt`, `require.dat`, `statusInfor.dat`): unit requirement opcodes and status/display function pointers, injected via euddraft `[dataDumper]`.
- **iscript** (`IscriptModule.vb`, `Data\AnimOpcodes.txt`): animation script editing for `[iscriptPatcher]`.
- **GRP/images** (`GRPModule.vb`, `ImageToGRPModule.vb`, `Data\Palletes\`): GRP decode/encode. DevIL and XNA are used only for Remastered tileset rendering.
- **MPQ**: `StormLib.vb` for the map (`staredit\scenario.chk`), `SFmpq.vb` for the retail game archives, `CascLib.vb` for Remastered CASC.
- **Debugger** (`RWMem.vb`, `DebugClass.vb`, `Form\Debug*`): live `ReadProcessMemory` inspector of a running StarCraft. Ships in Release; there is no `#If DEBUG` anywhere.
- **SCDB**: an online save/load service for maps, authenticated against a hard-coded Naver blog post. Effectively dead infrastructure; `SCDB:`-prefixed actions are no-ops when `ProjectSet.SCDBUse` is off.

### Localization

`Module\LangageModule.vb` (sic) contains `Namespace Lan`. `Data\Language\English\` and `Data\Language\Korean\` must contain the **same file set** (122 each today).

- `<FormName>.json` is a flat `{ "ControlName": "Text" }` map applied by `Lan.SetLanguage(Me)`, which walks `form.Controls` and matches on `Name`. Filenames must equal `Form.Name` exactly, typos included (`CondictionForm.json`, `FoudlerNamedialog.json`).
- Menus/context menus get separate `<FormName><MenuName>[Suffix].json` files via `Lan.SetMenu` / `Lan.SetTooltip`.
- Free-form strings: `Lan.GetText(file, key)`, `Lan.GetMsgText(key)` reads `Msgbox.json`, `Lan.GetArray` splits on `\`.
- Trigger action/condition labels are **not** here; they are inline in `action.json`/`condition.json` `Texts`. `.tfn` tooltips are inline in the `.tfn`.

Adding a form: create the JSON in both language folders with identical keys, add menu JSONs for every strip, call `Lan.SetLanguage(Me)` (and `SetMenu`/`SetTooltip`) in `Load`, and put new messages in both `Msgbox.json`s. The `Lan.GetLanguage/GetMenu/GetTooltip` generators exist but every call site is commented out.

## Conventions and gotchas

- **`Option Strict Off`** project-wide with a long `NoWarn` list. Late binding and implicit string-to-Boolean/UInteger conversions are pervasive; don't "fix" them wholesale, it changes behaviour.
- **Misspelled names are load-bearing.** Do not rename: `BulidForm`, `CheckSumMoudle`, `TriggerEditorDataMoudle`, `tempmoudle`, `LangageModule.vb`, `MapModule..vb` (double dot), `Condiction`, `FuctionNameForm`, `AddTectDialog`, `isdisalbe`, `Separater`. Language JSON filenames and `.e2s` section names depend on them.
- **Korean identifiers and comments** are normal here (`ElementType` members, `Main.vb` handlers like `맵에삽입_Click`, subs `저장()`/`열기()`). Keep new code in English but don't translate existing identifiers.
- **Encodings are mixed.** Language JSON is written UTF-8 but read with `Encoding.Default` (ANSI codepage). `Offset*.txt`, `ValueDef.txt` and `.tbl` files are read as CP949 (`ks_c_5601-1987`). Map CHK strings use a CP949/UTF-8 autodetect heuristic in `SettingModule.vb`. Generated `.eds/.edd/.py/.eps` are deliberately UTF-8 without BOM (the 0.18.1.7 fix); keep the commented-out CP949 encodings commented out.
- Source files are CRLF with UTF-8 BOM; `core.autocrlf=true` normalizes them in git, so a "LF will be replaced by CRLF" warning is expected.
- `Recent.txt` (the Open Recent list) is written to `Environment.CurrentDirectory`, not the app directory.
- `Module\GetMRUModule.vb` and `Module\RegModule.vb` are fully commented out; `UnitModule.vb` and `NPAModule.vb` are empty stubs. Dev-machine paths (`C:\Users\skslj\...`) survive behind disabled flags in `initModule.vb` and `tempmoudle.vb`.
- Remastered projects: `RemasterModule.vb CheckCompatiblity()` silently disables BinEditor, TileSet, GRP and the debugger; `Main.vb` also hard-codes different window sizes per `StarVersion`.
