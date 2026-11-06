# Version 0.2.1 validation report

Date: 2026-11-06. Environment: this Windows x64 development machine.

## Automated

Command:

```powershell
& "$env:LOCALAPPDATA\EnglishB2Academy\dotnet\dotnet.exe" run `
  --project .\tests\EnglishB2Academy.Checks\EnglishB2Academy.Checks.csproj
```

**30 checks passed.**

Coverage:
- Preferences defaults, persistence/replacement, invalid JSON/theme/goal/size/null
  handling and preservation of invalid source files.
- Exact library size: 35 lessons / 105 questions / seven lessons per level.
  Unique content identifiers, valid options, single consistent answer keys.
- Independent exam bank: exactly 30 items, 22 language/reading + eight listening.
  Transformation word limits and retained key words.
- Case/whitespace normalisation without forgiving incorrect answers.
  Unanswered items count as wrong and preserve the score denominator.
- Absolute timer boundary immediately before, at and after expiry.
- Diagnostic sampling and its deliberately limited starting-point output.
- Word count, apostrophes/hyphens and exact 140/190/191-word boundaries.
- SQLite migration/new database, persisted response/position/audio count/deadline,
  completed-attempt updates, quoted draft input and learner profile.
- Spaced review 1/2/4-day intervals and ten-minute reset.
- Live backup, typed validation, restoration of all record types.
- Rejection of existing-file overwrite, same-database backup, non-SQLite backup
  and valid SQLite containing an invalid JSON session.
- Invalid-position save rejection, structured JSON export, reopen persistence,
  next-lesson recommendation and distinct-local-day streaks.

Tests use isolated temporary files. They do not overwrite the learner's database.
Structural answer-key checks are not independent educational review.

## Build and package

- `dotnet build EnglishB2Academy.sln`: success, zero warnings/errors.
- Self-contained Release publish for `win-x64`: success.
- WiX 3.14.1 compiler and Windows Installer validation: MSI created.
- ICE91 is intentionally excluded for this explicitly per-user product.
- One ICE60 warning concerns the language-neutral native SQLite library.
  No invented file language is assigned. Other MSI validation remains enabled.
- Package is unsigned; no code-signing claim is made.

## Actual installation and upgrade

- Windows Installer installed 0.2.0 with exit code **0**.
- Verified installed executable, Start menu app/uninstall shortcuts and desktop shortcut.
- SHA-256 comparison of all **419** installed payload files: **zero mismatches**.
  Repeated after the 0.2.1 upgrade, with the same result. Development `.pdb` files
  are intentionally excluded by the installer and the payload comparison.
- Windows Installer upgraded 0.2.0 to 0.2.1 with exit code **0**.
- SHA-256 of `learning.db` immediately before/after upgrade: **unchanged**.
- Installed executable reports product version **0.2.1**, file version **0.2.1.0**.
- Installed app launches and displays saved attempt history.
- Daily startup safety backup exists.

Final MSI SHA-256:
`4A1674F1CDC3AA41F3E6729F161567CC42239DAF1352A39AACD7FA2B1304908C`

The installation directory is separate from the learning-data directory.
Installer tables do not include the learner's database or recordings.
Actual uninstall has not been exercised because the user is using the installed
application; do not describe uninstall as independently tested.

## Actual UI

Observed the installed app's Home, course library, original lesson, marked result,
progress history, exam centre, running timed mock, writing editor and speaking
controls. Saved and visually inspected real PNG captures of core workspaces.
The timed mock displayed 30 items and a decreasing 45-minute countdown.
No answer key appeared on the active question page.
The preview opened an unanswered timed mock and an empty writing draft in the
running application. They were not submitted as learner achievements. The mock's
absolute deadline continues while other screens are open.

## Dependency check

`dotnet list package --vulnerable --include-transitive` reported no known
vulnerable packages from the configured NuGet source on the verification date.
This is an advisory result, not a penetration test or security certification.

## Not verified / not implemented

- Microphone recording, device playback and installed English voice audio output
  have not been tested end-to-end on hardware.
- Network-disconnected operation has not been certified, although application
  learning/storage/audio code does not invoke a cloud service or network API.
- Full accessibility, other monitors/DPI settings, Windows ARM64 and clean-VM
  installation remain unverified.
- Complete CEFR curriculum, full-length unseen examination papers, calibrated
  scale scoring and independent writing/speaking assessment are not implemented.
- No production signing, automatic update service or legal/educational
  certification is claimed.
