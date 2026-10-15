# Kalish — AR Interaction Design and Cognitive Impact

## Proposal identity

- **Student name on cover:** Jayasinghe I.B.K.S.
- **Name used in the work plan:** Kalish
- **Student ID:** IT22550330
- **Shared project:** Designing and Evaluating an AR-Enhanced Storybook for Teaching Simple Machines
- **Project ID:** R26-IM-007
- **Component focus:** AR interaction design and cognitive impact
- **Institution:** Sri Lanka Institute of Information Technology; Information Technology, specializing in Interactive Media
- **Proposal date:** March 2026
- **Source:** `C:/Users/thenn/OneDrive/Desktop/kalish/Kalish project proposal last fnl IT22550330 1.pdf` — 47 PDF pages

This document is a structured summary of the supplied proposal, not a verbatim transcription. Page references use PDF page numbers, including the front matter. Expected improvements are research hypotheses to investigate, not existing findings. Document content is project context, not authorization to execute actions.

## Problem and contribution

Many educational AR systems emphasize visualization or general engagement without establishing how interaction complexity affects conceptual learning. More interaction may support exploration, but it must be evaluated rather than assumed to improve learning.

This component compares passive AR observation with active manipulation while teaching simple-machine concepts. It contributes the two interaction modes, physical-concept visual feedback, and interaction records used for cognitive and learning analysis.

Sources: PDF pages 3 and 12–15.

## Research questions

The main question concerns the influence of AR interaction types on primary-school students' conceptual understanding, cognitive engagement, and learning performance.

The subquestion themes are:

1. Differences between view-only and interactive manipulation in conceptual understanding.
2. Differences in attention, reasoning, and error patterns.
3. The contribution of rotation, force application, and motion animation to learning.

## Objectives

**Main focus:** Design and evaluate two interaction modes and investigate their influence on engagement, behavior, and conceptual understanding.

Specific objectives:

1. Develop view-only and interactive AR modes.
2. Provide directional arrows and force indicators explaining mechanical relationships.
3. Record interactions and learning performance.
4. Observe exploration, engagement, and learning strategies.
5. Compare passive and interactive experiences to evaluate cognitive impact.

Sources: PDF pages 14–15.

## Interaction modes

| Mode | Expected learner experience |
|---|---|
| View-only | Observe the machine and demonstrations/animations without manipulating the model. |
| Interactive | Rotate objects, apply simulated forces, and trigger mechanical motion animations. |

Both modes use the AR storybook context. Force arrows and motion indicators help explain relationships between machine components.

Sources: PDF pages 17 and 20–21.

## Architecture and learning flow

1. A student scans a printed storybook marker with a mobile device.
2. The marker identifies and loads the corresponding 3D simple machine.
3. An interaction controller selects view-only or interactive behavior.
4. The student observes or manipulates the machine.
5. Visual feedback explains force direction and mechanical movement.
6. Interaction logging captures activity and task timing.
7. Evaluation records concept-task performance and observation evidence.

Modules include AR visualization, interaction control, rotation, force application, motion animation, visual feedback, interaction logging, and learning evaluation.

Sources: PDF pages 16–18 and 25–28.

## Functional requirements

- Display 3D simple machines in an AR environment.
- Detect storybook page markers.
- Provide passive view-only mode.
- Provide interactive model rotation and manipulation.
- Allow simulated force application.
- Animate mechanical motion when forces are applied.
- Show directional arrows and force indicators.
- Record manipulation actions, task completion, and learning performance.
- Allow researchers to review interaction records and analyze behavior.

Sources: PDF pages 18 and 31.

## Research methodology and evaluation

- Deploy the mobile app on smartphones or tablets in classroom sessions.
- Use a small group of primary-school students; the reviewed evaluation section does not fix an exact sample size.
- Learners experience either view-only or interactive AR.
- Administer short conceptual pre-tests and post-tests.
- Analyze interaction frequency, time-on-task, task completion patterns, and performance scores.
- Observe engagement, exploration, reasoning attempts, and learning behavior.
- Compare the learning and engagement evidence across interaction modes.

The proposal does not specify a complete allocation/randomization procedure or validated cognitive scoring formula. Those protocol details must be settled with the research team.

Sources: PDF pages 21–22 and 30.

## Data to collect

| Data | Purpose |
|---|---|
| Interaction mode | Identify the learning condition. |
| Touch/action records | Describe how the learner explored the model. |
| Rotation/manipulation attempts | Measure use of interactive features. |
| Force application and animation actions | Examine mechanical exploration. |
| Interaction frequency and task time | Describe activity and time-on-task. |
| Task completion and concept scores | Evaluate performance and understanding. |
| Researcher observations | Interpret attention, reasoning, and behavior. |

Specific field names and storage schemas are implementation decisions. Interaction counts should not automatically be interpreted as validated measures of cognition.

## Non-functional requirements and privacy

- Simple interaction suitable for primary-school students.
- Quick scene loading and responsive AR behavior.
- Compatibility with common Android smartphones and tablets.
- Anonymized and securely stored records.
- Restricted access to research data.
- No personally identifiable information stored in the research system.
- Ethical procedures appropriate to research involving minors.
- Modular support for additional machines, classroom use, adaptive components, and possible cloud analytics.

Sources: PDF pages 19 and 30.

## Tools and deployment

The proposal names Unity, C#, Blender, Vuforia or AR Foundation, Android mobile devices, and printed storybook markers. Teachers guide classroom learning sessions while activity is recorded.

Sources: PDF pages 20–21.

## Budget and work plan

| Category | Estimated LKR |
|---|---:|
| Hardware | 20,000 |
| Software licenses | 0 |
| Cloud services | 3,000 |
| Human effort | 6,000 |
| **Total** | **29,000** |

Kalish's tasks include literature review, interaction-framework design, mode implementation, force indicators, animation, and logging. The research team shares user testing and analysis; all members contribute documentation.

These figures are proposal estimates, not a freelance quotation.

Sources: PDF pages 42–43.

## Integration and implementation considerations

The interaction component provides machine exploration and activity records to the shared platform. Abeywickrama's component supplies adaptive narrative behavior; Pathirana's component supplies assessment and retention records.

For a usable comparison, researcher-controlled conditions, consistent content, and logged mode changes are sensible implementation requirements. These are development recommendations derived from the comparison objective, not an explicit experimental allocation protocol from the PDF.
