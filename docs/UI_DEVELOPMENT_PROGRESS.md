# UI development progress

## Current release - 0.2.1 (2026-11-06)

The original shell has been extended into a working introductory learning app.
The Stage 1 notes below are historical, not the current implementation status.

| Screen | Implemented and exercised | Actual screenshot |
|---|---|---|
| Home | Live next-lesson recommendation, saved practice summary, honest diagnostic status | [Home](development-previews/02-home-learning.png) |
| Courses | 35 short lessons / 105 questions, level filter and search | [Course library](development-previews/03-courses.png) |
| Lesson | Original explanation, example, common mistake, practice launch and speech control | [Grammar lesson](development-previews/04-grammar-lesson.png) |
| Exam centre | Source-dated official structure facts and independently authored short practice options | [Exam centre](development-previews/05-examination-centre.png) |
| Timed mock | Countdown, locally saved session/deadline, objective choices, no pre-submission answers | [Running short mock](development-previews/06-timed-mock.png) |
| Writing | Original prompts, editable auto-saving draft, word/paragraph check | [Writing editor](development-previews/07-writing-editor.png) |
| Speaking | Original prompts, permission confirmation, local WAV controls | Inspected through Windows accessibility; no screenshot claimed |
| Progress | Actual persisted attempt history, weekly activity and raw skill charts | Inspected through Windows accessibility; no screenshot claimed |

Screenshots are rendered client areas of the actual installed WPF application.
The sample progress visible on Home comes from actual local interactions, not
seeded demonstration results. No personal identity or profile is included.

Other implemented workspaces: reading, synthesised listening, vocabulary with
30 spaced-review flashcards, mistake revision, diagnostic, learner profile,
readiness evidence, backup/restore, JSON export and confirmed record deletion.
Historical resources explicitly reports that no imported papers are bundled.

### Packaging / verification

- Full solution and Release publish build successfully.
- 30 focused automated checks pass (see [test report](TEST_REPORT.md)).
- MSI 0.2.0 installed successfully; executable and both Start menu shortcuts plus
  optional desktop shortcut verified.
- All 419 installed payload files matched their published SHA-256 hashes.
- Actual 0.2.0 -> 0.2.1 major upgrade succeeded. The learning database hash was
  identical before and after the installer ran.
- Installed 0.2.1 launches with existing learning history intact.
- Daily startup SQLite backup was created and the Save view PNG action was tested.
- Dependency advisory check found no known vulnerable packages from NuGet.
- Microphone capture/device playback, English voice output, full offline network
  isolation, uninstall and screen-reader certification are not device-verified.

### Current boundaries

This is not a complete CEFR curriculum or full official-format exam simulator.
The combined original mock is 30 questions / 45 minutes. Writing has mechanical
feedback only; speaking is self-review. Neither receives an automated quality mark.
No calibrated scale conversion, readiness certification, other-board pathway,
content import, signed installer or automatic updater is provided.

Visual limits: smaller windows need scrolling. Standard Windows input controls
retain their native chrome, including a light editor surface in dark appearance.
No text-overlap problem was observed in the inspected lesson/mock views.

Use **Save view** in the header or F12 to capture the current viewport locally.
Settings also supports exporting the Home dashboard through a file picker.
The installed app remains available for inspection.

## Stage 1 - visible Windows foundation

Native C# / .NET 10 LTS / WPF. The owner selected WPF rather than WinUI after
reviewing the difference between open-source code licences and Microsoft binary
SDK terms. This stage intentionally stops short of the entire learning system.

### Home dashboard

Screenshot: [actual running dashboard](development-previews/01-home-dashboard.png).
Captured 2026-11-06 from the running WPF client area at 1732 x 933 pixels,
using the saved dark appearance. The home dashboard was left running.

Implemented: navigation, welcome panel, daily goal, honest unassessed proficiency,
default target pathway, practice-plan overview and skill destination buttons.
Teal/ivory palette, original vector motif, system typography and no external art.
The screenshot is an actual rendered client area, without Windows title-bar chrome.

Remaining: real lesson recommendations, learning statistics, streaks and activity
charts, all dependent on genuine recorded activity.

### Learning plan / learning workspace

Implemented: working navigation, a clearly labelled plan layout and original
present-perfect example to inspect reading typography. Back navigation works.

Remaining: placement, personalised plan, full lessons, exercises and persistence.
No completion or score is awarded. No screenshot is claimed for these screens.

### Settings and help

Implemented: locally saved daily goal, light/dark palette and larger reading size;
privacy information, about dialogue, actual dashboard PNG export.

Remaining: future audio, exam profile, backup and learning-data controls.
No screenshot is claimed for this screen.

### Other destinations

Courses, grammar, vocabulary, reading, writing, listening, speaking, examination
practice, mock examinations, historical resources, mistakes, progress and
readiness all navigate to explicit foundation-status pages. Their learning
functions have not been implemented. Readiness is not assessed.

### Verification

- Native Windows build succeeds; no application NuGet dependencies.
- Eight isolated preference checks pass: first-launch defaults, round-trip,
  replacement, invalid JSON, invalid theme, invalid goal, invalid reading size,
  and null-file handling. Invalid files remain unchanged.
- Actual native window inspected through Windows accessibility and image tools.
- Plan and lesson preview navigation exercised in the running WPF app.
- Saved dark appearance survived reopening; the actual dashboard PNG was exported,
  inspected and saved in the project's development-previews folder.
- Window dimensions constrained to the Windows working area for high-DPI displays.
- Compact layout stacks the lower cards; long content and navigation scroll.

Visual issues/limits: smaller windows need vertical scrolling by design.
The native title bar and standard picker/combobox chrome follow Windows defaults;
they are not custom themed. Full accessibility, multiple-monitor/DPI, offline
network-isolation, installer and full-product testing remain outstanding.

See [run instructions](../README.md) and [privacy notice](../PRIVACY_POLICY.md).
