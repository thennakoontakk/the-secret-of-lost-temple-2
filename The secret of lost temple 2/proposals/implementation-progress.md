# Implementation progress

## Lever research handover — 14 October 2026

Scope updated by the user: implement the research parts for **lever only**; other machines are excluded. User explicitly selected **in-app tests plus researcher score entry**. The earlier milestone-1 section below remains historical.

### Added since milestone 1

- Configured study sessions: protocol, locked passive/interactive condition, optional adaptation and embedded quizzes, content/app versions, explicit learning start/completion and follow-up visits.
- In-app baseline/post/one-week assessments with per-answer persistence and no answer disclosure during measurement. External score entry with source/version/denominator, confirmation and append-only corrections preserving original test dates.
- Actual score summaries, percentage-point change, seven-day due dates and recorded delayed-test intervals. Interrupted question timing is flagged.
- Corrective formative hints and one retry, exact question/answer context, first/retry attempt IDs, adaptive question difficulty and independent/support/rejoin narrative responses.
- Explicit lever marker routing; strict Tracking state; a content transform separate from the marker anchor. Passive input restrictions and measured condition enforcement across the existing mode controllers.
- Foreground tracked exposure and review logging, observations, expanded CSV tables and ZIP export.
- Lever-only navigation; offline researcher entry from authentication screen; existing art and scenes retained.

### Verification so far

- **16 Edit Mode tests passed**, 0 failures/skips. Includes original seven storage tests and study sequence, follow-up, actual scores, correction dates and interrupted timing tests.
- Unity compilation passed after integration.
- Isolated Play Mode synthetic stores: new-study UI, all six baseline responses, stable scene-to-scene session ID, passive mode lock/force rejection, interactive force acceptance/logging and formative retry/hint/story events passed.
- The passive regression initially exposed an older visibility function overwriting the mode. That override was removed, and the same regression then passed.
- Research setup and formal question screens were visually inspected; the session summary was compacted to prevent clipping.
- Desktop AR scene reports missing XR providers; no claim is made that Editor smoke tests validate real image tracking.
- ADB successfully listed devices outside the sandbox: **none connected**.
- Final Android ARM64/IL2CPP APK build **succeeded**, job `build-4b689e21f3`, 0 errors / 7 warnings, 78.3 seconds. APK: `Builds/LeverResearch.apk`, 136,523,917 bytes (~130.2 MiB), minimum API 24 / target API 36. APK metadata verified with aapt.
- APK SHA-256: `B0DDD5B82CD081DC0F65281BE943CE163D6C08612068F5C223B99C5C7B74C4FD`.
- Final marker-model binding check passed: three force vectors attached to the spawned prefab; load 100 N and support equal to effort + 100 N. Non-moving taps no longer mark a completed model gesture.
- Backup PowerShell script syntax parsed successfully. Actual phone extraction remains untested.
- Outstanding device gates: existing lever marker ARCore score **25/100**; existing **Both** input-backend Android warning. These require actual-phone validation and remediation if unreliable before participant collection. See `lever-device-acceptance.md`.

### Handover documents

Use [lever-research-handover.md](lever-research-handover.md) for the complete operator flow, dataset interpretation and pre-collection device checks. [pull-research-data.ps1](pull-research-data.ps1) copies phone records without deleting them.

The supplied question bank and rules are pilot defaults requiring the students' approval. A final protocol/assignment rule was requested but not supplied; the software records researcher-selected settings rather than inventing an approved study design. Cloud research sync is intentionally excluded from this offline-first handover.

Source checkpoint before this scope: `C:/Users/thenn/.codex/visualizations/2026/10/13/01a13451-d3f3-7990-ba4c-9e147852e870/lever-research-before.zip`.

---

## Milestone 1 — local participant/session foundation

Implemented 13 October 2026. Unity compilation and local storage tests passed; Editor scene smoke tests passed with the Android AR limitations below. Other milestones remain pending.

### Implemented

- Research setup on the home map: anonymous codes such as P001, new/resume/end session, practice mode, and local dataset export. Switching requires ending the active session. An unfinished session must be resumed explicitly after restarting.
- Stable session IDs across scenes, saved events, and exports. Research records contain anonymous codes rather than Firebase account identifiers. Study condition stays `unassigned` until the study protocol is implemented.
- Participant-specific quest unlocks, difficulty tier and promotion/demotion streaks, avatar preferences, story node, and retention/review records. Practice uses separate preference keys. Original unscoped PlayerPrefs are preserved and are not silently attributed to a participant.
- Durable JSON snapshots under `Application.persistentDataPath/research_v1/records.json`. Each accepted mutation writes a temporary file, flushes, and replaces the primary file while retaining a backup. Recovery preserves a copy of a corrupt primary. Unrecoverable storage blocks recording instead of silently resetting it.
- Existing AR interactions, story choices, hint events, review openings, quiz answers and scene entries feed the local session record. Lever response timing starts for each question; duplicate answer submissions are ignored.
- Dashboard, lever quiz summary, retention panel and live telemetry no longer display fabricated research results. Unsupported attention, motivation, learning gain and formal retention measures remain explicitly unavailable.
- Export creates a unique directory containing `sessions.csv`, `events.csv`, and `records.json` from saved records. Formal score fields are blank with `not_collected` status, numbers use invariant formatting, and session IDs are retained. Export includes all saved local participants; the directory path is shown after success.
- No research cloud deployment. Existing research export entry points now use local saved records and do not claim a successful cloud sync.

### Main changed files

- `Assets/_App/Scripts/Research/Core/ResearchStore.cs` and assembly definition: models, durable store, local export and scoped progress access.
- `Assets/_App/Scripts/Research/ResearchSetupUI.cs`: runtime-wired home-map setup panel; no broad scene-generation tool was run.
- `Home/HomeMapController.cs`, `AR/LeverARScreenController.cs`, `AR/ARSceneUIController.cs`: honest results, export feedback, answer logging.
- `Assessment/ResearchDataExporter.cs`, `Assessment/ARQuizManager.cs`, `Assessment/LearningRetentionTracker.cs`: saved-record exports and participant separation.
- `Adaptive/AdaptiveDifficultyEngine.cs`, `Adaptive/StoryBranchManager.cs`, `Adaptive/HintSystem.cs`, `Adaptive/EngagementTracker.cs`, `AR/CognitiveTelemetryLogger.cs`: scoped state, stable IDs and event integration.
- `Auth/AuthController.cs` and `AR/Physics3DLabController.cs`: route avatar preferences through participant/practice scope; authentication credentials were not modified.
- `Editor/ProjectAutoSetup.cs` and `Assets/_App/Scenes/MainARScene.unity`: remove the fabricated initial telemetry text.
- `Assets/_App/Tests/Editor/ResearchStoreTests.cs` and test assembly definition.

Script paths above are relative to `Assets/_App/Scripts/` unless otherwise stated. Unity generated the corresponding new asset metadata.

### Actual verification

Unity 6000.0.60f1 reported `EditorUtility.scriptCompilationFailed == false` after integration and UI changes.

Seven Edit Mode tests passed (0 failed, 0 skipped):

1. Participant progress and answer isolation.
2. Restart/load/resume preserves session identity and saved answers.
3. Corrupt primary recovers the backup and preserves corrupt evidence.
4. Unrecoverable data blocks without overwriting the original.
5. Export preserves IDs, missing scores, CSV quoting, decimal formatting under French culture, and unique output paths.
6. Practice and ended sessions reject research-event recording.
7. Invalid codes and accidental active-session switching are rejected.

Editor Play Mode smoke verification:

- Home-map setup panel and all seven controls existed; the panel was opened and visually inspected.
- New-session button started synthetic P001. Synthetic storage was redirected to a uniquely named temporary test directory, separate from study records.
- HomeMapScene → MainARScene kept session ID `49d30e36ef1d44388392788862a2922c`.
- Opening the lever quiz and submitting an answer produced one saved answer under that ID.
- P002 had neither P001's quest unlock nor its answer; saved data exported successfully.
- Fixed launcher positioning and verified it remains inside the wide Game View viewport.
- Home-map smoke check had no console errors. MainARScene reported missing desktop XR subsystems (camera/session/plane/raycast/input); device AR operation was not verified.
- Exited Play Mode and restored SplashScreen without saving temporary scene objects.

### Manual acceptance steps

1. Play from SplashScreen, sign in normally and reach the home map.
2. Click **Research setup** at the bottom. Enter **P001** and choose **New session**. Use synthetic codes during testing.
3. Enter the lever chapter, answer a question, return to the map and open the research dashboard. Confirm the saved session and answer count; formal scores must still say not collected.
4. Exit and restart Play Mode. Open Research setup, enter P001 and choose **Resume session**. Confirm the same session ID and saved answer.
5. End P001's session. Start P002 and confirm its quest progression and answer history are separate. Returning to P001's next session should retain P001's participant progress.
6. Choose **Export saved data**. Inspect sessions.csv and events.csv in the reported directory. Missing formal tests must be blank, not zero; events must share their session's ID.
7. Connect an Android test phone for keyboard, touch layout, persistence across a real app restart, marker tracking and filesystem/export access checks.

### Limits and next step

- Formal pre/post/one-week assessment scoring is not implemented in this milestone. The older ARQuizManager results are preserved as **draft quiz events**, not validated formal research scores. The current formative summary pools saved answer events; it is not a learning-gain measure.
- Retention scheduling and calculated curves remain milestone 3 work. Existing review history is isolated and saved, but its old simplified due-question logic is not a validated study schedule.
- Engagement and motivation algorithms remain unvalidated and are not presented as research measurements. Active study recording is opt-in through the map setup.
- Avatar selection still uses the existing app flow; new participants default to Maya until they select a character through that flow.
- No Android APK build, physical-phone test, backend rule test or Firebase research sync was performed. Local storage is not encrypted and exports are app-local files; device sharing and study data-handling requirements remain to be specified.
- Full-file snapshot writes prioritize durability for this prototype. Profile event volume and storage growth before a larger pilot.
- Next: milestone 2, authoritative passive/interactive lever controls and marker routing. Keep research-condition rules configurable until approved by the students.

### Recovery checkpoint

Before edits, the existing scripts and scenes were archived at:
`C:/Users/thenn/.codex/visualizations/2026/10/13/01a13451-d3f3-7990-ba4c-9e147852e870/milestone1-before.zip`.

No Git repository or usable Plastic command was found, so this is a source checkpoint, not a version-control commit. Restore only the intended files after comparing subsequent user changes; do not blindly overwrite the project.
