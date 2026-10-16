# Research flow and repair verification — 2026-10-15

## What failed
The saved MainARScene contained the legacy Temple Gates UI and lacked LeverAR_UIRoot, ScanningMarkerOverlay and QuestARMarkerSpawner. The legacy panel text could appear even with no active research recording. This was not a valid Kalish demonstration.

## Applied repair
Restored the custom Lever interface and scanner; disabled legacy ARSceneUIController and its panels; wired the printed-marker library and Lever prefab to QuestARMarkerSpawner; removed competing legacy anchor/detector components. Added Android Back handling and return-to-panel navigation for measured Lever sessions. The Android build menu now repairs this scene before building. Fixed Unity null handling for three force-line components in the UI generator. A scene backup is retained in proposals/MainARScene-before-flow-repair.unity.txt.

## Tests
Unity EditMode: 22/22 passed, no skipped tests. Includes data integrity, study stage gates, presentation presets and printed target reference. Job f1d841695bf64f7095730e53f92b52ee. Physical recognition and Android navigation require separate device evidence.

## Presentation walkthrough
Enter Researcher Panel > Quick Demo Presets. These are synthetic presentation participants, separate from real study records.

1. Kalish: select Kalish_Passive, scan the complete printed illustration, observe the lever and try manipulation (must be blocked). Return to research, select Kalish_Interactive, scan the same page and rotate/apply force. Both presets have adaptation and quizzes off. Explain the controlled comparison; do not claim a learning benefit from a demo alone.
2. Abeywickrama: select Adaptive + Quizzes. Scan the page; answer incorrectly to demonstrate feedback and retry; answer consecutive tasks correctly to demonstrate progression. Show the recorded attempts. The current implementation begins at tier 1; verify its task threshold during the live demo.
3. Pathirana: select Assessment flow. Complete pre-test, start Lever learning, keep the marker tracked, return and finish learning, complete post-test, then review. Real retention remains gated by seven days. Load the separately labeled synthetic retention example only to demonstrate the later record/export.

## Book and home routes
The printed book provides the narrative and physical image target. It is shared by the research conditions. Read Story & Scan is the ordinary story route; Free Practice and Physics Lab are exploration routes, not substitutes for an assigned recorded participant session. The Researcher Panel selects and records the study conditions/stages.

## Follow-up navigation diagnosis — 2026-10-16
The first installed repaired scene still returned immediately to AR. Editor playback reproduced the cause: HomeMapController.Start automatically called OpenQuestModal when the saved mode was Quest. On the regenerated map the modal was absent, so its fallback entered AR again. Removed the automatic launch; explicit user buttons still launch Quest.

Verified in editor playback after correction:
- Scanner Return button invokes navigation to HomeMapScene, with StudyOverlay active; scene stays on the map.
- Quick Demo Presets -> Kalish_Passive opens MainARScene, condition passive, CanManipulate=false, adaptation=false, quizzes=false.
- Return -> Quick Demo Presets -> Kalish_Interactive opens MainARScene, condition interactive, CanManipulate=true, adaptation=false, quizzes=false. InputAllowed=false without marker tracking, as intended.
- uGUI now uses InputSystemUIInputModule consistently for the GameActivity build. Android Back checks both supported input paths.

Device build/install and physical tracking results must be recorded separately below.

Latest status: the navigation fix passed editor playback, but build-7b2298ad25 lost its MCP connection and Builds/LeverNavigationFixed.apk has not appeared. The phone currently has the earlier LeverPrintedMarker.apk, which still reproduces the return loop. Do not treat device navigation or physical tracking as passed. Awaiting confirmation of any visible Unity build dialog.

## Restored expedition story flow — 2026-10-16
User clarified that Start Expedition and Review Machines must remain separate. Restored those labels and rebuilt the missing HomeMap chapter/book UI. Lever expedition now displays all 18 story pages, with marker activity at page 9. Story activity saves its page context. Continue Story returns to page 10; Back returns to the marker page. Research routes remain separate and assigned conditions remain locked. Review fallback explicitly sets Sandbox mode instead of reusing a stale Quest mode.

Editor playback evidence:
- Open Lever book, advance eight pages, start AR: MainARScene has Continue Story button.
- Click Continue Story: HomeMapScene, currentPageIndex=9, visible text PAGE 10 / 18.
- Review Lever: MainARScene, Sandbox mode, SimpleMachine_Lever and LeverAR_UIRoot active.
Phone build job: build-103ccb1909, Builds/LeverStoryFlow.apk; completion/install still pending at time of this entry.
