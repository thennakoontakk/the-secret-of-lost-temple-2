# Printed marker fix — 2026-10-15

Changes:
- Imported Assets/Art/Targets/lever_printed_target.png, byte-identical to the supplied Downloads/Simple Machine AR.png (SHA256 7E19B61A85F145C7719F6554BB263D9DA6231B8250D1E99BEE863A364ED066B1).
- Updated SimpleMachinesReferenceImageLibrary lever_target texture and serialized texture GUID. Disabled specified physical size because the actual print size is unknown.
- Enabled ARTrackedImageManager in MainARScene; QuestARMarkerSpawner also enables it and assigns its library if missing.
- Updated scanning instructions to show the full printed illustration and move slowly for size estimation.
- Added PrintedMarkerBuild editor build command and PrintedMarkerTests reference regression test.

Validation:
- PASS: exact source/copy hashes match.
- PASS: ARCore arcoreimg generated a database using this image without a fixed physical width.
- Image quality score: 35/100. This is not proof of successful live recognition.
- Source inspection: Kalish presets differ in condition; both disable adaptation and embedded quizzes. StudyPolicy.Resolve and InputAllowed gate passive versus interactive manipulation.
- NOT RUN: new Unity test and full Android build. Batch Unity refused the already-open project. Existing editor MCP endpoint was unavailable. Existing editor was left open.
- NOT VERIFIED: physical recognition, tracking loss/recovery, passive controls, interactive rotation/force on the updated phone build. No updated APK installed.

Next build: Unity menu Lost Temple > Build Android with Printed Lever Marker produces Builds/LeverPrintedMarker.apk.
Demo acceptance: Researcher Panel > Quick Demo Presets > Kalish_Passive, scan complete printed page, observe model and verify gestures/force cannot manipulate. Return to research, choose Kalish_Interactive, scan same page and verify rotation/force. Cover page: model and manipulation should stop; reveal and reacquire. Use the same print and lighting for both conditions.
