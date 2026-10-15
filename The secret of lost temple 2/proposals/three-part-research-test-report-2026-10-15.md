# Three-part research test report
Date: 15 October 2026 (Asia/Colombo)
Scope: current lever implementation. Test and report only; no production behavior changed.

**Later physical-device follow-up:** See [phone-verification-2026-10-15.md](phone-verification-2026-10-15.md). Redmi Note 9 Pro testing produced 8 passed, 1 failed and 2 blocked device cases. The installed build shows a white camera background and differs from the handover APK. The earlier no-phone status below describes the initial test run only.

## Result
16 existing Unity Edit Mode tests passed, 0 failed/skipped (2.293 seconds).
12 additional direct component assertions: 11 passed, 1 failed.
These are software/component results, not a completed phone demonstration or evidence of educational effectiveness.
Unity compilation failure flag: false. Editor: Unity 6000.0.60f1, SplashScreen, outside Play Mode.
ADB device listing succeeded after a sandbox-limited initial attempt; no phone connected.
No new APK build or physical-device test was performed.

## Setup and repeatable demo preparation
Use a separate trial installation for manual tests. Launch Research (offline), then Research setup / tests.
Participant codes must be P followed by 3–8 digits: use P99001, P99002, P99003, P99004.
Earlier chat advice to use DEMO01 was incorrect: that code is rejected.
Choose New study with a clearly identified pilot protocol. Complete baseline before Start / resume learning.
Use the registered lever marker printed at 20 cm square. Do not mix trial data with real participants.

Automated component probes used fresh synthetic stores under this report's evidence directory.
They temporarily replaced the in-memory store, created inactive temporary components, directly invoked methods, and restored the original store/tracking flag in finally.
They did not click rendered UI, activate a camera, simulate actual touch, or test scene lifecycle.
The adaptive engine thresholds were exercised directly; quiz-to-engine wiring was not established by those threshold tests.

## Kalish — passive versus interactive AR

| ID | Setup and steps | Expected | Actual / evidence |
|---|---|---|---|
| K01 | Passive session, completed baseline, learning started, simulated tracked state. Call AdjustForce(+10), MoveFulcrumStep(+0.1). | Neither value changes. | PASS, direct component probe. |
| K02 | In passive condition request Interactive via StudyPolicy.Resolve. | Returns ViewOnly. | PASS, policy probe; full rendered mode toggle not exercised. |
| K03 | Interactive session, baseline complete, learning started, simulated tracking. Apply same changes. | Force and fulcrum increase. | PASS, direct component probe. |
| K04 | Interactive session; set tracking false; attempt both controls. | Both values stay unchanged. | PASS, simulated tracking flag; camera loss itself untested. |
| K05 | On phone scan registered marker; cover and uncover it; show unrelated image. | One lever appears, hides on loss, returns on recovery; unrelated marker does not start lever. | BLOCKED: no connected phone. |
| K06 | On phone compare passive and interactive rotation, pinch, force, fulcrum and exploded controls. | Passive cannot manipulate; interactive gestures work; visible arrows/labels remain aligned. | BLOCKED: no connected phone. |
| K07 | Track for a timed interval, background app, return; export exposure. | Background/untracked time excluded; passive quiet observation counts. | NOT RUN end-to-end; source includes foreground/tracking gates and periodic flush. |

Meaning: force/fulcrum restrictions work at component level. Actual AR acquisition, gestures, rendering and exposure timing still require acceptance on the intended phone.

## Abeywickrama — adaptive questions and story support

| ID | Setup and steps | Expected | Actual / evidence |
|---|---|---|---|
| A01 | Fresh adaptive participant at tier 1; record two independent successes. | Still tier 1. | PASS. |
| A02 | Record third consecutive success. | Tier 2. | PASS. |
| A03 | At tier 2 record one failed/supported task. | Still tier 2. | PASS. |
| A04 | Record second consecutive failed/supported task. | Return to tier 1. | PASS. |
| A05 | Render first guided question through controller method; submit wrong answer, correct retry, duplicate answer. | Exactly two answer records; hint and support-to-rejoin story event. | PASS. Visual highlighting and on-screen layout not verified. |
| A06 | New participant, adaptation OFF; record three successes. | Tier remains 1. | PASS. |
| A07 | Adaptive tier 1, second question: inspect rendered content and corrective target. | Load question targets load, with load-specific support. | FAIL: question is “Which part is the object being lifted?” but targetPart is fulcrum. |
| A08 | In running app complete three independent tasks, then advance; repeat at higher tier. | Next question changes from identification to relationship/calculation; badge agrees. | NOT RUN end-to-end. Source changes question content by saved tier; direct engine thresholds passed. |
| A09 | Compare wrong answers with adaptation ON/OFF, embedded quiz ON. | Hints remain; performance-driven story transitions occur only with adaptation ON. | PARTIAL: ON retry/events passed; OFF branch suppression inspected in source, not asserted in this run. |
| A10 | Change participants and resume the earlier participant. | Each gets their own tier, streak, and story state. | PARTIAL: generic participant isolation passes in existing tests; live engine reload/UI not exercised. |

Meaning: promotion/demotion and retry recording work. The load question's support is misdirected. Current adaptive narrative is brief feedback and saved node changes, not evidence of a fully distinct playable story path.

## Pathirana — assessment, feedback and retention

| ID | Setup and steps | Expected | Actual / evidence |
|---|---|---|---|
| P01 | Embedded quizzes OFF; call OpenARQuiz. | No formative-start event. | PASS, direct component probe. |
| P02 | Try learning before baseline; try posttest before learning completion. | Both rejected. | PASS, StageOrderAndDelayedGateAreEnforced. |
| P03 | Complete learning and posttest; try retention at 6.999 days and then exactly 7 days using injected test clock. | Early attempt rejected; day-seven test accepted. | PASS, same test. Device clock was not changed. |
| P04 | Answer one baseline item; reopen store and resume. | Same assessment ID and one saved answer; cannot switch to posttest. | PASS, FormalTestSurvivesRestartWithoutDuplicateAnswers. |
| P05 | Leave tests missing, then enter a genuine zero score. | Missing stays missing; zero is displayed as measured zero. | PASS, MissingTestsRemainMissingAndZeroIsARealScore. |
| P06 | Enter external score, attempt correction without reason, then correct with reason two days later. | Reason required; original preserved; test date retained; correction date separate. | PASS, both correction tests. |
| P07 | Reopen the same unanswered formal question; answer it. | timingRestarted flag true. | PASS, ReopenedQuestionHasAnExplicitTimingFlag. |
| P08 | Finish learning/posttest, end visit, reopen follow-up at day eight. | Original session/condition preserved; elapsed days eight. | PASS, ConditionIsRestoredAndFollowupKeepsOriginalSession. |
| P09 | Record first/retry attempts and duplicate an attempt ID; start second participant. | Duplicate ignored; second participant has no previous assessments. | PASS, AttemptsAreIdempotentAndParticipantSpecific. |
| P10 | Complete all three tests correctly, delayed at day nine; export. | 100% each, gain zero points, interval nine days, 18 answer rows. | PASS, ExportsReproduceScoresAndLateInterval. Mixed-score gain not separately tested here. |
| P11 | Take formal tests through rendered UI. | No correctness disclosure; usable keyboard/layout; double-tap safe. | NOT RUN UI acceptance today; no-feedback text and submit guard inspected in source. |
| P12 | Open review after posttest; learn again; export. | Review exposure separate and formal scores unchanged. | NOT RUN end-to-end. |
| P13 | Export ZIP on phone, USB-copy, open CSV/JSON and reconcile counts. | Complete readable backup with matching session IDs and scores. | BLOCKED: no phone. Store CSV export is tested separately. |

## Shared storage tests rerun
All seven passed:
- CorruptPrimaryRecoversBackupAndPreservesEvidence
- ExportKeepsIdsMissingScoresAndInvariantDecimals
- InvalidCodesAndAccidentalSessionSwitchAreRejected
- ParticipantsAndAnswersAreIsolated
- PracticeAndEndedSessionsDoNotAcceptEvents
- RestartRequiresResumeAndPreservesSessionId
- UnrecoverableDataBlocksWithoutOverwriting

These plus the nine protocol tests above make the 16-test suite. Matrix rows sometimes group multiple assertions or share a test; do not count matrix rows as independent automated tests.

## Findings and improvements, in priority order

1. **Fix the load-question hint target (confirmed failure).** In LeverARScreenController.DisplayCurrentQuizQuestion, both guided question variants set targetPart to fulcrum. Use the correct target for each concept and add load-specific hint/highlight handling. Verify wrong load answers highlight the load and give a load explanation. This affects both adaptive support and misconception correction.
2. **Complete phone acceptance before participant use.** No phone is connected. The previous build's marker score was 25/100; this is a historical warning, not a fresh scan result. Test acquisition/reacquisition under classroom lighting and replace the marker if unreliable. Current ProjectSettings still has activeInputHandler: 2 (Both); the earlier Android warning remains an acceptance concern.
3. **Make each research demonstration easier to navigate.** Add clearly named supervised demo presets for interaction comparison, adaptive support, and assessment/retention. Show the assigned settings and next required step. Keep synthetic/demo records separate from participant records. This is a proposed usability improvement, not implemented functionality.
4. **Improve question variety and scientific evaluation.** Source shows the same six formal items at all three stages; repeated-item familiarity can influence results. Teacher-review the content and consider approved equivalent forms. Adaptive question generation repeats identification/relationship/calculation templates. Expand the bank while recording exact content/version.
5. **Make adaptive story changes more visible.** Current tested evidence is feedback text and transition events. Add a visible story path or substantive support/rejoin scene progression if the intended contribution requires distinct narrative experiences.
6. **Protect against accidental study interruption.** End visit currently calls End directly. Handover states an unfinished learning visit cannot be reopened as a completed follow-up. Add a clear warning and distinguish Pause/resume from End visit.
7. **Strengthen repeatable integration coverage.** Convert component probes into maintained tests and add scene/UI tests for quiz-to-engine wiring, tier badge, question changes, review isolation, exposure timing and actual export workflow. Include an unequal pre/post score case.
8. **Finish the research protocol.** Approve question versions, assignment rules and adaptation/quiz settings. Keep other settings controlled for each comparison. Recorded taps/exposure are behavioral evidence; observations/instruments are needed for motivation and engagement claims.

## Environment observations
Console included Unity AI generator NoSubscription messages and Android connection/reverse-network errors. They are not failed research assertions; the automated suite still passed. They do mean this report does not claim an entirely clean editor console.
Production scripts/scenes were not edited. Temporary objects were destroyed; original store and tracking flag restored.

## Evidence and reproduction
- editmode-results.json: full Unity result payload, job 2ef28645c65e48008b39815e395a737b.
- component-probe.cs.txt: exact C# snippet for Unity execute_code; requires not being in Play Mode and uses isolated stores.
- component-results.json: final 12 assertions, including the confirmed hint-target failure.
- synthetic-* directories: clearly synthetic saved records from component probes, not participant data.
Rerun the Edit Mode suite with Unity Test Runner. For physical acceptance use lever-device-acceptance.md; mark device cases only after observing them on each target phone.

