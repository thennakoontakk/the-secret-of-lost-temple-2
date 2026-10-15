# Physical phone verification — 15 October 2026

## Decision
The installed app passes the tested baseline, persistence and export workflow, but **does not pass AR demonstration readiness**: the camera area stays solid white. The user confirmed it remained white when asked to point the rear camera at a textured object. No marker was available, so marker-dependent tests remain blocked separately.

## Device and build
- Redmi Note 9 Pro (joyeuse), Android 12; physical display 1080×2400, app tested in landscape.
- Package com.UnityTechnologies.com.unity.template.urpblank, version 0.1.0, versionCode 1, ARM64, camera permission granted.
- ARCore 1.56.262080393.
- Installed APK SHA-256: 816347B7457E98E1EFF6DDC26B4192DE73796E924511D23BF537D2F42E665791.
- Workspace Builds/LeverResearch.apk SHA-256: B0DDD5B82CD081DC0F65281BE943CE163D6C08612068F5C223B99C5C7B74C4FD.
- These hashes differ. Results below apply to the installed APK, not a verified identical copy of the handover APK. No installation, uninstall, data clearing or production code edits were performed.

## Test execution
Synthetic participant P99101; passive, adaptation ON, embedded quizzes ON; protocol lever-pilot-1. These settings test the workflow, not a controlled research comparison. Initial backup attempt found no research files. All resulting study records are synthetic and must be excluded from research analysis.

| ID | Steps and expected result | Observed result |
|---|---|---|
| D01 | Open Research setup, enter P99101 with phone keyboard, create study. Input should save and controls remain usable. | PASS. Code accepted, study created, layout readable. Keyboard Enter commits input. |
| D02 | Start in-app pretest, answer first item. Answer should persist immediately without displaying correctness feedback. | PASS. Advanced to question 2; copied JSON contains L1 selected=1, correctIndex=1. No per-item correctness screen observed. |
| D03 | Force-stop app, relaunch, explicitly resume P99101 and reopen pretest. | PASS. Returned to question 2, preserving session and assessment identity. |
| D04 | Answer remaining five baseline items with known correct choices. | PASS. Screen displays 6/6, 100%; JSON contains six answers. Question 2 has timingRestarted=true after restart. |
| D05 | Start learning after baseline. Original research session should survive scene transition. | PASS. Entered marker-scanning scene and returned with the same session, passive condition and learning stage. |
| D06 | AR scene should display a live rear-camera background. | FAIL. Solid white background in ar-start.png and ar-camera.png, confirmed by user. Camera permission granted and camera privacy indicator active. |
| D07 | Return to research; press Finish learning without any tracked marker exposure. | PASS. Shows “No tracked learning exposure recorded yet.” Learning completion remains blank. |
| D08 | Attempt posttest before completed learning. | PASS. Shows “Complete learning first.” No posttest score created. |
| D09 | Export all saved data, USB-copy research directory, open ZIP and CSV/JSON. | PASS. ZIP readable with eight expected files; six formal answer rows share original session; baseline 100%, post/retention/gain blank. |
| D10 | Scan marker, lose/reacquire tracking, compare passive and interactive gestures, run embedded adaptive tasks. | BLOCKED. User has no marker; white background also needs resolution. |
| D11 | Complete real learning then posttest, review and seven-day follow-up. | BLOCKED in this device run. No tracked learning completed; no clock manipulation or fabricated exposure used. |

Result: **8 passed, 1 failed, 2 blocked device test cases**. These are separate from the earlier 16 automated tests and 12 component assertions. Do not count the blocked cases as passes.

## Verified data
- Session: ae776111862141508287c9301228ea69.
- Assessment: bb81a2cc00c2431388215c610b9f9a33.
- Baseline: 6/6; six persisted formal answers; resumed question timing flagged.
- No learning_exposure events; learningCompleted empty; posttest and retention missing.
- Export: 20261015-080822-41d34a73912b40bab920c68039a30867.zip, 5,302 bytes.
- ZIP contains sessions.csv, protocols.csv, formal_tests.csv, formal_answers.csv, attempts.csv, events.csv, records.json and README.txt.
- sessions.csv answer_count=0 refers to formative attempts; formal_answers.csv has the six baseline answers. Rename this column to formative_answer_count to avoid misinterpretation.

## Findings and improvements
1. **Highest priority: resolve the white camera background on this phone.** Logs show ARCore camera activity and kNotTracking/depth-measurement errors, but these do not prove the rendering root cause. Inspect AR camera background setup, renderer features/materials and any opaque UI/background objects; reproduce with an identified build. Do not assume the missing marker explains an absent camera feed.
2. **Identify builds clearly.** Installed and handover APKs have different hashes but the same simple version label. Add a build identifier to researcher setup and exports, and verify the intended APK before acceptance testing.
3. **Complete the blocked marker tests.** Use the registered print at the documented size after camera rendering works. Passive/interactivity and adaptive story behavior were not newly validated on-device today.
4. **Retain the previous hint-target defect.** The earlier component audit found load-question support targets the fulcrum. Today's marker-less run neither fixes nor clears that finding.
5. **Improve return/resume guidance.** Return to research lands on the home map/chapter UI rather than opening the research panel directly. The participant input becomes blank after the scene change even while the status correctly shows P99101. Prefill the active participant and open the session panel directly.
6. **Improve export usability.** USB extraction works. A native Android share/save action would avoid requiring researchers to copy long app-storage paths or use ADB. Clarify formative versus formal answer counts.
7. A development-console “message header is corrupted” connection warning briefly appeared after relaunch. It did not prevent baseline completion; remove development-only console overlays from participant builds and investigate connection diagnostics separately.

## Evidence and final state
Evidence folder: artifacts/phone-2026-10-15 relative to this report. Key screenshots: entered.png, resumed-q2.png, pretest-complete.png, ar-camera.png, finish-blocked.png, export-success.png, posttest-blocked.png. app-logcat.txt contains app-process diagnostics. final-backup contains the copied research directory and export.

P99101 remains an unfinished synthetic learning visit, allowing later resume when the camera/marker is available. The phone was left on the assessments screen. Existing account login was retained. Do not include this participant in study analysis.
