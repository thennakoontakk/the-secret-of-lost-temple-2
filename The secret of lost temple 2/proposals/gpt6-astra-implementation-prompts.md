# GPT-6 Astra implementation prompts

## Verified setup — 13 October 2026

- Unity MCP can read this project's live Editor state and console.
- Project: `C:/Users/thenn/OneDrive/Desktop/the secret of lost temple/The secret of lost temple 2`.
- Unity: 6000.0.60f1; selected build platform: Android.
- Active scene at verification: SplashScreen; Editor idle and ready for tools.
- Firebase MCP responds and reports an authenticated account.
- Firebase MCP has no active project selected and no local firebase.json. The app's google-services.json names `the-secret-of-lost-temple`; verify access to that existing project before configuring backend work.
- The sampled console errors concern Unity AI generation's `NoSubscription` response. Those generation features are not required for the planned C# and scene work. This check was not a complete compile/build validation.
- ADB is installed; a real test phone still needs to be connected for device verification.

## How to use these prompts

Select GPT-6 Astra in the model selector and use this project chat, or another chat attached to the same project folder. Use High reasoning for the first architecture/integration pass if the selector offers it; this is a suggested setting, not a requirement.

Send the starter prompt and one milestone at a time. Allow implementation and verification to finish before starting the next milestone. Keep Unity open with its MCP session active. Avoid manually changing the same scenes while the agent edits them.

The milestones deliberately build on each other. An agent should read the current code and progress notes instead of assuming earlier work succeeded. Do not start multiple agents modifying the same scenes or central controllers concurrently.

## Inputs to provide

### Already available locally

1. [Implementation report](<C:/Users/thenn/OneDrive/Desktop/the secret of lost temple/The secret of lost temple 2/proposals/app-improvement-report.md>)
2. [Abeywickrama proposal](<C:/Users/thenn/OneDrive/Desktop/the secret of lost temple/The secret of lost temple 2/proposals/abeywickrama-adaptive-storytelling.md>)
3. [Kalish proposal](<C:/Users/thenn/OneDrive/Desktop/the secret of lost temple/The secret of lost temple 2/proposals/kalish-ar-interaction.md>)
4. [Pathirana proposal](<C:/Users/thenn/OneDrive/Desktop/the secret of lost temple/The secret of lost temple 2/proposals/pathirana-assessment-retention.md>)

The agent can read these files directly; repeated PDF uploads are unnecessary for normal implementation. Consult the original PDFs when a proposal ambiguity needs checking.

### Product and research inputs to supply when known

Copy and fill this form. Unknown answers should remain marked unknown; they must not become invented study decisions.

```text
First milestone scope: participant/session data and honest results
First complete learning chapter: lever
Final machine chapters: [lever / pulley / inclined plane / wheel and axle / undecided]
UI and teaching language(s): [answer]
Target Android phone models: [answer]
Printed markers: [local folder or files / existing project markers]
Approved story and quiz content: [local paths / draft only]
Formal tests: [in-app / printed or Google Forms / undecided]
Study condition assignment: [researcher assigned / approved alternative / undecided]
Adaptive behavior during passive vs interactive comparison: [approved rule / undecided]
Review schedule before the one-week test: [approved rule / undecided]
Existing Firebase project: the-secret-of-lost-temple [confirm ownership/intended use]
Backend development target: [emulator / separate development project / undecided]
Visual design: preserve existing temple theme, characters, and usable layouts
Deadline and must-have demonstration: [answer]
```

Local storage and UI work can start before the entire form is complete. Research-condition decisions should remain configurable and documented until the students approve them. A researcher-selected condition is a practical prototype default, not a claimed experimental allocation method.

Never paste passwords, service-account private keys, refresh tokens, or participant personal records into prompts. Use connected account access and synthetic test participants.

## Starter prompt — send once

```text
Work in C:/Users/thenn/OneDrive/Desktop/the secret of lost temple/The secret of lost temple 2.

Read the following files in this project's proposals folder:
- app-improvement-report.md
- abeywickrama-adaptive-storytelling.md
- kalish-ar-interaction.md
- pathirana-assessment-retention.md
- gpt6-astra-implementation-prompts.md

Use the relevant installed Unity development, Unity MCP, C#, debugging, and Firebase skills. First verify the Unity instance/project path, Editor state, available custom tools, and console. Treat the report as a dated static review: confirm findings against the current code and scene wiring before fixing them.

Preserve the existing temple art, characters, useful UI, and working behavior. Inspect version-control state and establish a recoverable checkpoint using the existing repository workflow before substantial changes. Preserve existing user changes. Do not rerun broad scene-generation menus without inspecting what they overwrite. Avoid changing Unity or package versions unless a verified issue requires it.

Implement the milestone I give you, including C# changes, necessary scene/prefab wiring, persistence, and meaningful verification. Continue through routine implementation choices; ask only for missing decisions that materially affect scope, research validity, or external deployment. Keep unapproved study choices configurable and label development defaults.

Use real measured data. Missing results must remain missing. Never invent scores, learning gains, retention curves, significance claims, or successful sync states. Keep synthetic test data separate from study data.

Use Unity MCP for Editor inspection and verification. After script changes, wait for compilation, check the console, run relevant Edit/Play Mode tests, and exercise the changed flow where possible. Explain any device-only checks not performed. Use a test/emulator environment for backend work; production deployment is a separate step.

Maintain proposals/implementation-progress.md with the milestone, changed files, acceptance checks, actual results, unresolved issues, and next step. End each milestone with a concise result and exact manual test steps. Do not mark work complete merely because scripts exist.

Begin milestone 1 below now.
```

## Milestone 1 — dependable participant and session foundation

Send with the starter prompt:

```text
Implement milestone 1 from the report: P0-01, P0-02, and the local-storage portion of P0-04.

Create a shared anonymous participant/session model and durable local record storage. Assign a session ID once and reuse it across scenes, events, and exports. Separate participant progress, difficulty, answers, and review history. Preserve legacy data safely; do not silently merge or delete it.

Add a minimal researcher participant/session setup flow using the existing UI style. Wire existing learning actions into the session lifecycle. Replace fixed dashboard, learning-gain, and retention values with real saved measurements or clear uncollected/pending states. Make export read saved records with explicit missing values. Do not implement cloud deployment or extra machine chapters yet.

Verify that P001 and P002 do not share state, records survive restarting, export retains session IDs, absent tests do not appear as zero scores, and no fixed research-result claims remain in the affected UI. Include serialization/recovery tests and a Unity scene smoke test. Report any remaining unsupported metrics honestly.
```

## Milestone 2 — correct lever AR and interaction conditions

```text
Read the implementation progress file and verify milestone 1 before continuing. Implement report P0-05, P1-05, and the lever portion of P1-07.

Use one authoritative interaction policy. Passive study mode should show a standard demonstration without learner manipulation of the model. Interactive study mode must support the approved rotation, effort/fulcrum controls, motion, and force indicators. Separate practice mode from a measured condition and record the condition on events.

Fix marker routing so only the lever's mapped marker loads the lever chapter. Handle tracking loss and reacquisition without duplicate instances, rewards, or attempts. Keep content manipulation separate from marker anchoring. Preserve the existing artwork and simulation where correct.

Record meaningful completed actions, not every input frame. Test input restrictions, routing, repeated tracking events, scene transitions, and logging. Demonstrate the Editor flow and clearly identify printed-marker/Android checks that still require a device.
```

## Milestone 3 — assessment, corrective feedback, and review

```text
Read current progress and implement report P0-03, P1-08, P1-09, and P1-10 for the lever chapter.

Unify scoring and attempt recording behind the existing quiz UI. Separate formative quizzes from formal pre/post/retention measurements. After a wrong formative answer, provide a conceptual visual hint and allow a retry before revealing a solution. Save every attempt, first-attempt accuracy, per-question response time, and hint exposure.

Use the formal-test method recorded in the project inputs. If still undecided, implement the shared record model and explicit researcher score entry so externally administered tests can be recorded; do not pretend those answers were collected in-app. Preserve assessment source and test version.

Provide chapter/concept review and log review exposure without overwriting formal results. Schedule the one-week test from learning completion, record the actual interval, and calculate summaries only from available measurements. Keep development clock controls out of participant builds.

Test repeated attempts, per-question timing, score denominators, missing tests, participant isolation, review history, seven-day scheduling, and late assessment recording. Use synthetic records for tests and keep them separate from study records.
```

## Milestone 4 — adaptive storytelling and engagement

```text
Read current progress and implement report P1-01 through P1-04 on the working lever flow.

Connect real task outcomes to configurable adaptation rules. Apply difficulty to actual learning tasks and scaffolding. Build authored success, support, retry, and convergence story routes. Contextual hints must reference the learner's current task/error and prior support.

Preserve the distinction between normal story choices and performance-driven adaptation. Log each branch and difficulty change with its reason and rule version. Use the approved study configuration to control adaptation during passive/interactive comparison sessions.

Track task time, logical actions, story navigation, hints, and voluntary review. Separate background time and tracking interruption from learner inactivity. Report observable behavior rather than claiming a validated motivation or cognitive score from an arbitrary formula.

Test success/failure thresholds, supported retry, branch convergence, scene reload, duplicate-event handling, participant isolation, and the configured comparison condition. Show one independent-success path and one supported-recovery path in Unity.
```

## Milestone 5 — Firebase synchronization and research export

```text
Read current progress. Verify the Firebase MCP environment and access to the existing project named in Assets/google-services.json. Do not create a replacement project. Determine the agreed development/emulator target before backend changes.

Implement the cloud portion of report P0-04 using the durable local records as the basis. Add a persistent upload queue, stable record IDs, retry handling, and acknowledgements. Preserve raw events and attempts alongside derived summaries. Show actual pending/succeeded/failed status.

Separate researcher access from anonymous participant records and prepare appropriate Firestore rules. Do not include child names, email addresses, or credentials in study exports. Test rules and retry behavior in the emulator/development target.

Produce researcher-usable participant summaries and event/attempt CSV files with consistent units, timestamps, correct escaping, and documented fields. Test offline completion, restart, reconnect, duplicate retry, account/token failure, and reconciliation between local records, cloud records, and export. Prepare production deployment as a separate reviewable change.
```

## Milestone 6 — remaining approved chapters

```text
Read the approved machine scope and current progress. Implement the next agreed machine chapter using the completed lever architecture. Do one chapter at a time.

Deliver its printed-marker mapping, model, story activity, passive demonstration, interactive controls, force explanation, quiz, corrective hints, adaptive branches, review, and research events. Reuse the shared session and assessment services instead of copying central logic into another large controller.

Identify missing art, models, marker files, or approved educational content before fabricating final material. Development placeholders must be clearly marked. Verify scientific relationships and units and flag content for teacher review.

Run chapter-specific tests and a regression smoke test of the lever chapter. Update the chapter coverage table in implementation-progress.md with implemented, tested, and device-verified status separately.
```

## Milestone 7 — Android pilot readiness

```text
Audit the implemented app against all three proposals and the acceptance checks in the report. Build a development Android APK and, if a device is connected and authorized, install and exercise it using ADB and Unity diagnostics.

Test camera permission denial, correct/incorrect markers, tracking loss/recovery, background/resume, offline use, repeated uploads/exports, switching participants, and the full lever learning/assessment/review flow. Measure performance on the named target devices. Preserve any existing signing configuration; do not publish the app.

Fix reproducible defects within the agreed scope. Deliver the APK location, actual test results, known limitations, a researcher session checklist, and remaining student/supervisor decisions. Do not claim educational effectiveness or completed one-week participant testing from software tests.
```

## Reusable follow-up for an actual failure

```text
Milestone: [number]
Device/Editor: [model and Android version, or Unity version]
Steps: [exact sequence]
Expected: [behavior]
Actual: [behavior]
Evidence: [screenshot/log path]

Reproduce this failure, determine the cause, fix it without regressing completed milestones, and verify the affected flow. Update implementation-progress.md with the result.
```

## Reusable continuation after a break or new chat

```text
Continue this Unity project using proposals/implementation-progress.md and proposals/gpt6-astra-implementation-prompts.md. Verify the live project and current file state. Resume the first incomplete item in milestone [number], preserving completed work and unrelated user changes. Implement and test through that milestone's acceptance criteria. Report blockers and unperformed checks explicitly.
```

## Recommended first action

Select GPT-6 Astra, send the starter prompt plus milestone 1, and let it complete that milestone. Supply the research/product input form as answers become available. Add new visual features only after the shared data and lever flow are dependable.
