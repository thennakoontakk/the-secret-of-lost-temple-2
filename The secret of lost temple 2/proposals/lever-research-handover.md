# Lever research handover

Updated 14 October 2026. Scope: **lever only**. This supersedes the milestone-1 limitations where functionality has since been implemented.

## What the app collects

- Anonymous participant code, stable session ID, protocol/version, assigned passive/interactive condition, adaptation setting and embedded-quiz setting.
- Separate in-app baseline, immediate post-test and one-week retention records, including each answer, correctness key, time and content version. Correct answers are not revealed during formal tests.
- Alternatively, researcher-entered raw scores with denominator, test version, source and explicit administrator confirmation. Corrections append a record linked to the original; original test dates remain unchanged and correction time is separate.
- Formative first attempts and retries, chosen answers, exact question/options, support hints, difficulty tier and performance-driven story transitions.
- Force changes, committed fulcrum changes, completed model gestures, navigation, tracking loss/recovery, review exposure, and foreground tracked learning time.
- Anonymous researcher observations and instrument responses. These support the students' observation/feedback instruments; the app does not claim to measure psychological motivation from taps.

Research data is local and does not require Firebase login or internet. Authentication remains available for the existing app experience. **Research (offline)** on the authentication screen opens the map without requiring a child account.

## Before using real participants

1. The students/teacher must approve the included six-question test, formative content, study assignment and adaptation rules. The supplied versions are clearly named **lever-pilot-v1** and **lever-pilot-1**. They are working software defaults, not a validated instrument or an approved experimental design.
2. Select the correct protocol/version, condition and optional adaptation/embedded-quiz settings for each participant according to that approved protocol. The app does not randomize assignments. Keep the condition and teaching settings consistent with the comparison the students plan.
3. Test the APK on the actual ARCore-capable Android phones. Confirm camera permission, printed lever marker acquisition, passive restrictions, interactive controls, pause/resume and data extraction. Editor tests cannot establish these.
4. Set the device date/time correctly. Records use device UTC. There is no developer clock control in the participant interface.
5. Use a separate test installation/device for the dry run. Export and archive trial data separately. Do not reuse a real participant code for synthetic testing.

## Marker and known build warnings

The supplied `lever_target_scaled.jpg` is the exact registered marker. Print the image at **20 cm wide and 20 cm high**, without cropping or stretching, on a flat matte surface. The reference library uses a 0.20 m physical size.

The current marker scored **25/100** in the build's ARCore image evaluation; the report recommends at least 75. Treat reliable acquisition and reacquisition as a release gate on the target phones. If the marker is unreliable, replace the registered texture and printed copy together with a more distinctive image, then repeat device tests before data collection.

The project also retains its existing **Both** input-backend setting (legacy touch/UI and Input System XR pose driver). Unity warns that this setting is unsupported on Android. Verify camera pose, all touch controls and text entry on the actual phones; a migration to a single input backend is still outstanding. Existing package/shader warnings are listed by Unity. A successful APK build does not resolve these runtime checks.

## First visit

1. Launch the app. At authentication choose **Research (offline)**, or use the existing researcher account.
2. On the home map open **Research setup / tests**.
3. Enter an anonymous code such as **P001** and the protocol/version. Choose **passive** or **interactive**, adaptation on/off and embedded quizzes on/off. Choose **New study**.
4. Open **Assessments** and choose **In-app pretest**. Each response is saved immediately. For external administration, choose **Enter pretest score**, provide the source/version/raw/maximum scores and confirm them.
5. Return to **Session** and choose **Start / resume learning**. Scan the printed lever marker. Learners follow the existing lever content and embedded tasks where enabled.
6. After the agreed learning activity, choose **Return to research**, open the session panel and choose **Finish learning**. This records the completion timestamp that anchors the seven-day follow-up. At least some tracked learning exposure must have been saved.
7. Take/enter the **posttest** immediately.
8. Export the data and then **End visit**.

Use **Resume unfinished** after an interruption. It preserves the existing assignment and session ID. Completed formal answers are not repeated. An unanswered question restarts its timer when reopened; its eventual answer is explicitly marked with `timing_restarted=true`.

## Review and delayed visit

1. Enter the same participant code and choose **Open follow-up visit**. This opens the latest completed learning episode for that code with its original session ID.
2. **Review lever** becomes available after the post-test. It opens the lever with the original condition and records review exposure separately.
3. The **retention** test becomes available at least seven days after learning completion and after the post-test. A late test is accepted and its actual elapsed interval is exported. Do not move the phone clock forward to unlock it.
4. Export again and **End visit**. Export directories are unique; previous exports are not overwritten.

If a visit was ended before learning completed, a new study is needed; do not end a baseline/learning visit merely to pause it. Use app close/resume instead. For a new, separate learning episode, use New study after ending the prior visit. The students should normally have one planned learning episode per participant.

## External score entry and corrections

External tests must be entered immediately after administration; the current device time is the collection time. The UI deliberately does not support backdating. Use the source field for an anonymous paper/form reference, not a learner name.

To correct a saved score, select the same stage, enter the corrected values and provide a correction reason. The original remains in the data. `replaces` links the new row to its predecessor, `completed_utc` remains the original test time, and `recorded_at_utc` indicates when the correction was entered. Use the latest completed row per session/stage for analysis and keep the audit trail.

## Export and backup

In **Export / notes**, choose **Export all saved data (ZIP + CSV + JSON)**. The app displays the complete path. The export includes every saved local participant, not just the selected one.

Data lives under:

```
Application.persistentDataPath/research_v1/
  records.json
  records.json.bak
  exports/<timestamp>-<unique-id>/
  exports/<timestamp>-<unique-id>.zip
```

The existing Android package ID is retained to avoid silently creating a separate data location:
`com.UnityTechnologies.com.unity.template.urpblank`.
On Android, the corresponding external app-files directory is normally:
`/sdcard/Android/data/com.UnityTechnologies.com.unity.template.urpblank/files/research_v1`.

For reliable handover backup, connect the phone by USB, enable USB debugging and authorize the computer. Run the accompanying **pull-research-data.ps1** on a Windows computer with Android platform-tools installed. It copies the entire research directory into a new dated folder and never deletes phone data. Confirm the copied CSV/JSON opens and keep a second backup.

Do not uninstall the app, clear its storage, or change its package ID until all records have been copied and verified. The data and exports are not encrypted by this app. The research team controls storage access and participant-code linkage separately.

## Dataset guide

| File | Meaning |
|---|---|
| sessions.csv | Session IDs, event/attempt totals, available pre/post/retention percentages, post-minus-pre percentage-point change, collection status. |
| protocols.csv | Protocol, app/content versions, condition, adaptation/embedded settings and learning timestamps. |
| formal_tests.csv | Every formal assessment and correction, denominator, source, timestamps and actual days since learning. |
| formal_answers.csv | In-app item responses and answer keys; external tests have no fabricated answer rows. Includes timing-restart flag. |
| attempts.csv | Formative attempts and retry number; exact rendered content is in context. Review attempts carry a review context flag. |
| events.csv | Raw behavior and exposure events, UTC, condition, stage and seconds where applicable. |
| records.json | Complete authoritative snapshot, including participant progress and all records. |
| README.txt | Export interpretation notes. |

Blank formal scores mean **missing**, whereas 0 is a measured zero score. Learning gain is **post-test percentage minus baseline percentage**, not normalized gain. No significance or superiority claims are calculated.

Formative summary accuracy pools attempts, including retries. For independent accuracy, filter `attempt_number=1`. For correction analysis, group by session and the quiz-run portion of `attempt_id`, task and attempt number. Do not treat taps/rotations as conceptual reasoning scores.

`learning_exposure` and `review_exposure` count foreground time with active marker tracking, including quiet observation in passive mode. They are not attention scores. These durations checkpoint approximately every ten seconds and on pause/exit; abrupt power loss can lose the final unflushed interval. Task/answer records are saved immediately.

## Implemented study behavior

- Only `lever_target` routes to the quest lever. Limited/lost tracking hides content and pauses tracked exposure. Reacquisition reuses the model while available; its content transform is separate from the marker anchor.
- Assigned conditions are locked. Passive shows a standard lever demonstration; direct rotation, force, fulcrum and exploded-view manipulation are blocked. Interactive supports rotation, scaling, force and fulcrum changes.
- Incorrect formative answers receive a conceptual hint and highlighted part, then one retry. The explanation follows a correct retry or a second wrong answer. All attempts remain saved.
- With adaptation enabled, independent successes and supported/failed tasks drive the existing three-success/two-failure tier rule. Subsequent questions change from guided identification to lever relationships and torque calculations. Feedback narrative branches to independent progress or support/rejoin.
- With adaptation disabled, performance-driven narrative branches and tier changes are disabled. Corrective formative feedback still follows the embedded-quiz setting.
- Review does not overwrite formal scores. Retention summaries use only actual post/delayed measurements, without fabricated day-one/day-three points.

## Verification status

- 16 Edit Mode tests passed: storage/recovery, participant isolation, stable IDs, missing vs zero scores, stage ordering, delayed-test gating, correction audit trail/dates, interrupted timing flags, idempotent attempts, invariant CSV and score reproduction.
- Unity compilation passed.
- Play Mode: new-study setup, in-app six-answer baseline, retained session identity across scenes, passive mode/force guard, interactive force recording, incorrect-answer retry and hint/story events passed using isolated synthetic stores.
- Research setup and formal-test UI were visually inspected.
- Android APK build status and final device limitations are recorded in implementation-progress.md.
- No physical Android phone was connected. Printed-marker tracking, real camera behavior, touch/keyboard layout and USB data extraction remain mandatory device checks before real collection.

This is a supervised research workflow. Researcher controls are not protected by an administrator PIN; supervise access while learners use the app.
