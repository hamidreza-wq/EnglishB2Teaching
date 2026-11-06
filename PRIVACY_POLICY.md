# Privacy notice - version 0.2

The application has no account, analytics, application network requests, paid API,
cloud transcription or automatic update service.

## Data stored for the current Windows user

In `%LOCALAPPDATA%\EnglishB2Academy`:

- `preferences.json`: appearance, reading size and daily study goal.
- `learning.db` and SQLite journal files: learner profile, optional first language
  and examination date, questions/answers, raw scores, timestamps, active objective
  practice time, persistent timed-session deadlines, writing drafts and flashcard
  review schedules.
- `recordings`: WAV files, created only after the learner confirms microphone use.
- Safety backups created during restoration.
- Daily startup database backups in `backups`, retained until manually removed.
- Current-screen PNGs in `previews` when Save view / F12 is used.

The files are **not encrypted**. Windows account/file permissions provide normal
host access protection. No stronger protection is claimed.
The development `--data-directory` option relocates learning data, not preferences.

## Speech

Original scripts and drafts are passed to installed Windows speech voices for
local playback. No models are downloaded or speech-recognition service invoked.
Microphone recording is optional, visibly indicated and limited to two minutes.
Recording stops and saves when leaving the workspace or closing normally.
Windows microphone settings and device availability can prevent recording.
No pronunciation, transcription or speaking-quality mark is generated.

## Export, backup and deletion

SQLite backups and JSON exports are written only to a chosen local destination.
They can contain profile information and written answers. Do not share them
without reviewing their contents.
Database backups exclude appearance preferences and WAV recordings.
Dashboard PNG export renders the actual visible client area; no image is uploaded.

Database restore requires confirmation, validates the selected backup and keeps
a safety backup before replacement. Invalid data is reported, not silently reset.

**Delete learning records** removes attempts, drafts, profile and vocabulary review
schedules after confirmation. It does not delete WAV files, appearance preferences,
backups or exports. Delete individual recordings in Speaking.
Normal upgrade and uninstall preserve learning data. Separately exported files and
backups must be managed separately by the learner.

## Scope

Windows and development tools have their own diagnostic/network behaviour.
The app does not control operating-system diagnostics or other installed software.
Full network-isolation testing and an independent privacy/security audit have not
been completed. Revise this notice before introducing online functionality.
