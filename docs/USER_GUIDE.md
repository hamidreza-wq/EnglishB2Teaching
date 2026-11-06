# English B2 Academy 0.2 - user guide

## Installation

Open `EnglishB2Academy-0.2.1-x64.msi` on Windows 10 1809+ or Windows 11 x64.
The wizard installs for the current Windows account in
`%LOCALAPPDATA%\Programs\EnglishB2Academy`, creates Start menu shortcuts and
optionally a desktop shortcut. The .NET runtime is included; no SDK is required
on the learner's computer. The package is unsigned. Do not bypass an operating
system security warning; obtain a trusted/signed release if your policy requires it.

Open the app from Start. No account, payment or cloud service is required.

## Begin learning

1. Use **My learning plan > Edit learner profile** to set preferences and target date.
2. The optional 15-question diagnostic suggests a grammar/vocabulary starting point.
   It does not assess all skills or certify a CEFR level.
3. Browse **English courses**. Filter by level or search. Each original short lesson
   includes an explanation, example, common mistake and three marked questions.
4. Submit a practice check to see your score, accepted answers and explanations.
5. Revisit **My mistakes** to create revision tests from your latest incorrect answers.
6. **Vocabulary** includes 30 flashcards with local spaced-review schedules.

There are 35 short lessons / 105 lesson questions across A1, A2, B1, B1+ and B2.
This is an introductory collection, not a complete curriculum.

## Four skills

- **Reading:** original passages and comprehension questions.
- **Listening:** installed English Windows voices speak original scripts locally.
  Untimed lessons have transcripts. If no English voice is available, the app
  reports the problem; it does not download one.
- **Writing:** four original prompts, auto-saved drafts and a mechanical
  word/paragraph check. No language-quality mark is awarded.
  Timed writing provides two original tasks with an 80-minute persistent deadline.
- **Speaking:** original prompts, local recording after explicit microphone
  confirmation, playback and individual recording deletion. Recordings stop after
  two minutes or when leaving the workspace. Windows microphone privacy settings
  and a working recording device are required. No pronunciation score or
  transcription is provided.

## Examinations

The official Cambridge B2 First structure is displayed with its verification
source/date. The supplied activities are **short original samples**, not complete
official-format mocks:

- Reading and language: 22 questions / 30 minutes.
- Listening: 8 questions / 15 minutes.
- Combined short mock: 30 questions / 45 minutes.

Timers use absolute deadlines and do not pause when the app is closed.
Answers save locally and can be resumed from the plan or exam centre. Answer
keys appear only after submission. Unanswered questions count as incorrect.
Shared audio clips can be started twice in timed mode. Stopping early consumes
that play. Every objective item is one raw point, without partial credit.
These scores are not Cambridge English Scale scores.

Practice results, repeated questions and self-review cannot establish official
exam readiness. No pass guarantee or examination-board affiliation is claimed.
Other examination-board pathways are not yet implemented.

## Saved data and recovery

Learning data: `%LOCALAPPDATA%\EnglishB2Academy\learning.db`.
Appearance: `preferences.json` in the same folder. WAV files: `recordings`.
These files are local and unencrypted; protect your Windows account.

In **Settings & help**:
- Back up the live database to a **new** `.db` filename. WAV recordings and
  appearance preferences are not included.
- Restore a validated database backup. This replaces current learning records,
  after confirmation, and creates a safety backup before replacement.
- Export a readable JSON copy of learning records and drafts.
- Delete learning records after confirmation. Separate backups, exports and
  voice recordings are not deleted by this action.
- Delete individual WAV recordings in **Speaking**.
- **Save view** in the header (or F12) saves any current screen as a PNG in the
  local `previews` folder. Hover over the saved-status label for its path.

A daily startup database backup is saved in `backups`. These files are retained
until you explicitly remove them; no automatic deletion is performed.

Backups and exports can contain your written answers. Do not share them casually.
Invalid data is reported rather than silently reset. Keep a backup before upgrading.

## Uninstall / upgrade

Use Windows **Installed apps** or the Start menu uninstall shortcut.
Normal upgrade and uninstall preserve learning data. Use the in-app deletion
controls before uninstall if desired; remove separately created backups and
exports yourself. No automatic update service is installed.

## Limitations

This is an early usable learning release. Full CEFR coverage, complete unseen
exam papers, reviewed writing/speaking assessment, other boards, first-language
teaching assistance, content import, signed updates and ARM64 packaging remain
future work. Keep independent teacher/exam-board guidance for real examination
decisions. Educational and legal distribution review remains necessary.
