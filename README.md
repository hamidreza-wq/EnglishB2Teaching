# English B2 Academy

Native Windows English practice application: **C# / .NET 10 LTS / WPF / SQLite**.
Version 0.2 is an early usable learning release, not the complete curriculum
described in the long-term product brief.

## Install

Build output: `artifacts\installer\EnglishB2Academy-0.2.1-x64.msi`.
The per-user Windows Installer wizard includes the .NET runtime, Start menu
shortcuts, an optional desktop shortcut, major-upgrade support and uninstall.
Windows 10 1809+ or Windows 11 x64 is required. The MSI is **unsigned**.
No SDK, account, subscription or cloud API is needed by learners.

See the [user and installation guide](docs/USER_GUIDE.md).

## Implemented

- 35 original short lessons / 105 questions across A1, A2, B1, B1+ and B2.
- Grammar, vocabulary, original reading passages and synthesised listening.
- 15-question grammar/vocabulary starting-point diagnostic, not CEFR certification.
- Marked objective practice, answer explanations, saved sessions and mistake revision.
- 30 vocabulary flashcards with local spaced-review schedules.
- Four writing prompts, auto-saved drafts, mechanical word/paragraph feedback and
  an 80-minute two-task writing workspace. No automatic writing-quality score.
- Speaking prompts, permission-gated local WAV recording, playback and deletion.
- A separate original 30-question timed bank: 22 reading/language and 8 listening.
  Short mocks have persistent deadlines, hidden answer keys and two-play audio limits.
- SQLite migration/version checks, local profiles, history, progress charts,
  daily startup backups/validated restore, JSON export and confirmed record deletion.
- Honest readiness evidence: no invented scale scores or guaranteed exam passes.
- Light/dark themes, larger reading size, keyboard controls and adaptive layout.

## Not yet complete

The introductory library is not a comprehensive CEFR course. There are no full
52-question reading / 30-question listening official-format mocks, calibrated
Cambridge English Scale scoring, independently reviewed writing/speaking marks,
other exam boards, imported historical papers, first-language teaching assistance,
content editor, ARM64 installer, signed release or automatic update service.
Questions and answer keys have structural tests, but need independent educational
review before a production curriculum release.

Official structure facts were checked against
https://www.cambridgeenglish.org/exams-and-tests/qualifications/first/format/
on 2026-11-06. No official questions or papers were copied.

## Build and checks

Install the SDK from `global.json`. On this development machine:

```powershell
$dotnet = "$env:LOCALAPPDATA\EnglishB2Academy\dotnet\dotnet.exe"
& $dotnet build .\src\EnglishB2Academy\EnglishB2Academy.csproj -p:Platform=x64
& $dotnet run --project .\tests\EnglishB2Academy.Checks\EnglishB2Academy.Checks.csproj
```

Or open `EnglishB2Academy.sln` in a compatible Visual Studio. The VS Code
**Build Windows dashboard** task also builds the application.
The checks cover content shape, answer keys, exact grading, timers, diagnostics,
word-count boundaries, SQLite persistence, corrupt backups, review schedules and
recommendations using isolated temporary files.

To build the MSI, extract official **WiX Toolset 3.14.1 binaries** locally, then:

```powershell
.\installer\Build-Installer.ps1 -Dotnet $dotnet `
  -WixDirectory "$env:LOCALAPPDATA\EnglishB2Academy\tools\wix314"
```

Compiler source/licence: https://github.com/wixtoolset/wix3/tree/wix3141rtm
WiX 3 is MS-RL-licensed build tooling. The installer uses native Windows Installer
tables without custom executable actions. ICE91 is intentionally excluded because
the product is explicitly per-user and is never intended for per-machine install.
Other MSI validation is enabled. Native SQLite may produce a language-neutral
binary-version warning; it is not assigned a fabricated language.

The source directory and every checked-in notice/guide are needed for packaging.
Do not distribute obsolete WinUI build outputs.

## Development preview

Run the published executable from `artifacts\app` or the installed Start shortcut.
**Settings & help > Export dashboard screenshot** saves the actual client area.
**Save view** in the header (or F12) captures any current screen to the local
learning-data `previews` folder; the saved-path tooltip is shown in the header.

Optional development launch switches:

```powershell
& ".\artifacts\app\EnglishB2Academy.exe" `
  --data-directory "$env:TEMP\EnglishB2Academy-ui-check" `
  --preview-path "$PWD\docs\development-previews\01-home-dashboard.png"
```

The isolated data switch is for testing; appearance preferences still use the
normal per-user preference file. Nothing is uploaded.

See [development progress](docs/UI_DEVELOPMENT_PROGRESS.md),
[privacy](PRIVACY_POLICY.md), [licences](OPEN_SOURCE_LICENCES.md) and
[content provenance](CONTENT_PROVENANCE_REGISTER.md).
