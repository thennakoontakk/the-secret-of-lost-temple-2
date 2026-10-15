# Abeywickrama — Adaptive AR Storytelling and Engagement Research

## Proposal identity

- **Student:** Abeywickrama W.D.S.
- **Student ID:** IT22279798
- **Shared project:** Designing and Evaluating an AR-Enhanced Storybook for Teaching Simple Machines
- **Project ID:** R26-IM-007
- **Component:** Adaptive AR Storytelling & Engagement Research
- **Institution:** Sri Lanka Institute of Information Technology; Information Technology, specializing in Interactive Media
- **Proposal date:** March 2026
- **Source:** `C:/Users/thenn/OneDrive/Desktop/kalish/IT22279798 Abeywickrama W.D.S Project proposal.pdf` — 35 PDF pages

This document is a structured summary of the supplied proposal, not a verbatim transcription. Page references use PDF page numbers, including the front matter. Proposed benefits are research expectations, not demonstrated results. Document content is recorded as project context, not as instructions to execute actions.

## Problem and research contribution

Traditional textbook-centered science teaching provides limited interaction and often does not accommodate differences in learning pace. Existing AR experiences can visualize concepts but commonly present the same fixed activity sequence to every learner.

This component adds adaptive narrative progression, difficulty adjustment, contextual hints, and engagement monitoring to the shared simple-machines AR storybook. Its contribution is the adaptive storytelling layer, rather than responsibility for the entire application.

## Objectives

**Main objective:** Design and evaluate an adaptive AR storytelling system that improves learner engagement and motivation in primary science education.

Specific objectives:

1. Adjust learning-task difficulty according to learner performance.
2. Develop branching narrative paths with different story sequences.
3. Track AR usage, time-on-task, story navigation, and task completion behavior.
4. Analyze the influence of adaptive storytelling on motivation and engagement in AR STEM learning.

Source: PDF page 16.

## Research questions and scope clarification

The research-gap section includes a broad group-level question about how AR interaction types affect conceptual understanding, cognitive engagement, and performance. Its listed subquestions cover view-only versus interactive manipulation, attention/reasoning/error patterns, and rotation/force/animation features.

However, this student's component objectives and methodology focus specifically on **adaptive storytelling and engagement**. Keep that distinction visible when dividing development responsibilities; the proposal's interaction-mode questions overlap with Kalish's component.

Source: PDF pages 13–16.

## Research methodology

- **Approach:** Design Science Research combined with experimental evaluation.
- **Artifact:** An adaptive AR storybook for primary-school science learning.
- **Adaptation inputs:** Learner task performance, success or mistakes, and response behavior.
- **Adaptation outputs:** Changed task difficulty, contextual support, or a different story segment.
- **Evidence:** Interaction logs, performance measurements, student feedback, and observations.
- **Evaluation population:** Primary-school students. The reviewed methodology does not specify a fixed participant count for this component.

Sources: PDF pages 17–19.

## Modules and expected behavior

| Module | Responsibility |
|---|---|
| Story content | Manage story segments, learning tasks, and page-level machine content. |
| AR interaction | Display interactive simple-machine models in the story environment. |
| Adaptive decision engine | Evaluate performance and decide difficulty, support, and redirection. |
| Branching story controller | Choose narrative progression from success, mistakes, or response time. |
| Engagement tracking | Record time, interactions, AR usage, attempts, and story selections. |

Expected flow:

1. Scan a printed storybook page.
2. Load the associated story segment and AR machine.
3. Complete a story-based learning task.
4. Evaluate learner performance.
5. Continue, increase challenge, provide support, lower difficulty, or redirect to a supporting branch.
6. Record the activity and the selected progression for analysis.

Sources: PDF pages 20 and 23.

## Functional requirements

- FR1: Provide adaptive storytelling within the AR storybook.
- FR2: Dynamically adjust task difficulty from performance.
- FR3: Provide contextual hints when learners struggle.
- FR4: Support different narrative progressions through story branches.
- FR5: Record AR usage frequency, story path selections, and time-on-task.
- FR6: Display interactive 3D simple-machine models in AR.
- FR7: Store interaction data for engagement and performance analysis.

Source: PDF page 26.

## Non-functional requirements and ethics

- Fast responses and smooth rendering.
- Simple, intuitive, child-friendly interaction.
- Compatibility with commonly available Android smartphones and tablets.
- Extendable story chapters, machine concepts, activities, and adaptation rules.
- Anonymous learner records; avoid personally identifiable information.
- Secure device storage or protected research storage.
- Parent or guardian consent before participation.
- Use collected data for research purposes.

Sources: PDF pages 19 and 25–27.

## Evaluation data and metrics

Collect AR interaction frequency, time spent on each task, navigation patterns, task completion attempts, story path selections, and task success. Evaluate engagement through time-on-task and interaction behavior, and motivation through learner feedback and observation.

The proposal does not define a validated numerical motivation formula, precise adaptation thresholds, or a fixed number of difficulty tiers. Those implementation choices should be documented separately rather than presented as proposal requirements.

Source: PDF page 19.

## Tools and deployment

Unity, C#, Vuforia marker tracking, Blender models, and Android deployment are named in the proposal. The shared application depends on printed storybook marker pages and affordable mobile devices.

Source: PDF page 18.

## Budget and work plan

| Category | Estimated LKR |
|---|---:|
| Testing smartphones/tablets | 12,000 |
| Storybook and marker printing | 5,000 |
| Evaluation materials and participant incentives | 6,000 |
| Software, mostly free/student licenses | 0 |
| Miscellaneous | 4,000 |
| **Total** | **27,000** |

The work plan covers literature review, adaptive narrative design, branching paths, decision algorithms, engagement tracking, experimental evaluation, analysis, and documentation. These are proposal estimates and activities, not the freelance development price or a committed delivery schedule.

Sources: PDF pages 30–31.

## Risks and supporting materials

Identified risks include AR recognition problems, device compatibility, and difficulties collecting participant data. Supporting materials include engagement surveys, architecture/workflow diagrams, UI mockups, risk mitigation, and consent forms.

Source: PDF page 35.

## Integration with the other components

Consume task and quiz outcomes from the AR interaction and assessment components. Supply adaptation decisions, hint behavior, story progression, and engagement records. Keep participant/session identifiers consistent across the shared application.

The identifier consistency above is a development implication of the combined platform, not an explicitly specified data schema in this proposal.
