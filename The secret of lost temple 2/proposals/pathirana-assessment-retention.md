# Pathirana — Assessment-Integrated AR and Learning Retention

## Proposal identity

- **Student:** Pathirana M.P.S.A.
- **Student ID in work-breakdown tables:** IT21817458
- **Shared project:** Designing and Evaluating an AR-Enhanced Storybook for Teaching Simple Machines
- **Project ID:** R26-IM-007
- **Component:** Assessment-Integrated AR & Learning Retention
- **Study context:** Sri Lankan primary-school students aged approximately ten
- **Proposal date:** March 2026
- **Source:** `C:/Users/thenn/OneDrive/Desktop/kalish/MPSA Pathirana Project Proposal..pdf` — 61 PDF pages

This document is a structured summary of the supplied proposal, not a verbatim transcription. Page references use PDF page numbers, including the front matter. Intended learning benefits have not been established by this proposal. Document content is project context, not instructions to execute actions.

## Problem and research contribution

Assessment performed only after a lesson misses the learner's mistakes and correction process during learning. This component embeds quizzes, problem-solving challenges, and immediate visual feedback in the AR narrative. Review AR Mode supports revisiting difficult concepts.

The contribution is an assessment and revision layer within the shared storybook, together with evidence of immediate learning and one-week knowledge retention.

Sources: PDF pages 3 and 17–19.

## Research questions

The main question asks how embedding assessment directly within AR influences learning outcomes and long-term knowledge retention for primary-school students studying simple machines.

Subquestion themes:

1. Conceptual understanding with embedded quizzes/challenges compared with traditional post-lesson assessment.
2. The effect of immediate visual feedback on misconception correction.
3. The contribution of review AR activities to performance on a delayed test one week later.

Source: PDF page 15.

## Objectives

**Main objective:** Design and evaluate assessment-integrated AR that measures learning outcomes and longer-term knowledge retention in primary science education.

Specific objectives:

1. Embed interactive quizzes and problem-solving challenges directly in AR.
2. Provide immediate visual feedback guiding conceptual understanding.
3. Provide review AR activities for revisiting simple-machine concepts.
4. Collect and compare pre-test and immediate post-test scores.
5. Administer a delayed assessment one week after learning and compare it with post-test performance.

Source: PDF page 17.

## Research design and participants

- **Design:** Mixed-method quasi-experimental evaluation within a single group.
- **Participants:** Approximately 30 primary-school students aged around ten.
- **Quantitative evidence:** Assessment scores and interaction/quiz records.
- **Qualitative evidence:** Classroom observations and student feedback.
- **Session duration discussed in the proposal:** Approximately 7–10 minutes for the learning session.

The methodology describes a single-group study, although one research question refers to comparison with traditional assessment. The team should resolve how that comparison will be supported; a separate control group is not established by the stated methodology.

Sources: PDF pages 18, 21, and 50.

## Evaluation sequence

1. **Pre-test:** Before the AR learning session.
2. **Learning session:** Storybook scans, machine exploration, embedded quizzes, and immediate feedback.
3. **Post-test:** Immediately after the session.
4. **Delayed retention test:** One week after the learning session.

Review AR activities allow learners to revisit completed content before the delayed assessment. Review exposure must be considered when interpreting retention results.

Formal tests may use Google Forms or printed sheets; the proposal does not require every test to run inside Unity. Day-one and day-three tests are not required by its stated core sequence.

Sources: PDF pages 18–21 and 28–30.

## System modules

| Module | Responsibility |
|---|---|
| AR learning | Render marker-triggered 3D simple machines in the story context. |
| Assessment engine | Trigger chapter-related quizzes/challenges and evaluate answers. |
| Feedback | Reinforce correct answers and guide correction after mistakes. |
| Review AR | Revisit completed chapters, models, and quiz activities. |
| Assessment data | Store answers, timing, errors, completion, and formal test scores. |

Machine examples include levers, pulleys, inclined planes, and wheels and axles. Exact delivered chapter coverage should be agreed with the team.

Sources: PDF pages 24–25.

## Quiz and feedback behavior

- Trigger quizzes as learners progress through story chapters.
- Keep formative assessment within the AR learning context.
- Reinforce correct responses immediately with positive visual indicators.
- Give corrective visual guidance after wrong responses **without simply revealing the answer**.
- Record response accuracy, response time, repeated attempts, and correction patterns.
- Provide access to review after the main learning activities.

The exact retry interaction and scoring rules are implementation details, but they must support the proposal's error-correction measurement.

Sources: PDF pages 19, 28–29.

## Functional requirements

- Embedded quizzes and problem-solving challenges triggered by story progression.
- Immediate visual feedback for each quiz attempt.
- Dedicated review mode for previously completed chapters and quizzes.
- Storage of pre-test, post-test, and delayed retention scores per learner.
- Logging of quiz responses, timing, error patterns, and completion behavior.
- Reliable printed-marker recognition with the correct machine and assessment content.
- Multiple story chapters associated with distinct machine concepts and quiz sets.

Sources: PDF pages 34–35.

## Evaluation metrics

| Metric | Intended meaning |
|---|---|
| Pre-test to post-test score gain | Immediate change in conceptual understanding. |
| Post-test to retention-test score comparison | Knowledge retained after one week. |
| In-AR quiz accuracy | Proportion of correct embedded quiz responses. |
| Error-correction rate | Self-correction following visual feedback. |
| Task completion time | Duration of quiz/challenge activities. |
| Observed engagement | Attention and participation during AR learning. |

The proposal describes descriptive statistics, comparative score analysis, and qualitative interpretation of hesitation, repeated attempts, and responsiveness. It does not establish a completed statistical analysis or prove significant improvement.

Sources: PDF pages 22–23.

## Privacy and non-functional requirements

- Parent/guardian consent before participation.
- Anonymized records without names, photographs, or contact details.
- Safe, age-appropriate, non-stressful activities.
- Withdrawal without consequence.
- Academic research use only.
- Secure local/protected storage accessible to the research team.
- Smooth, responsive AR and immediate quiz feedback.
- Child-friendly interaction and common low-cost Android compatibility.
- Reliable operation without crashes or loss of assessment records.
- Extendable chapters, machines, and question types.

Sources: PDF pages 21, 31, and 36.

## Tools

Unity, C#, Vuforia, Blender, and Android are listed for implementation. Google Forms or printed sheets support formal tests. The budget justification also discusses AR Foundation and testing on representative mid-range devices.

Sources: PDF pages 20 and 48.

## Budget

| Category | Estimated LKR |
|---|---:|
| Android testing devices | 15,000 |
| Software | 0 |
| 3D assets | 8,000 |
| Marker storybooks | 6,500 |
| Test printing and materials | 3,500 |
| Surveys and observation sheets | 2,000 |
| Evaluation support personnel | 8,000 |
| Contingency | 5,000 |
| **Total stated in proposal** | **48,000** |

The listed paid items sum to **LKR 48,000**, matching the stated total. These are research budget estimates, not the freelance development price.

Source: PDF page 47.

## Work plan

| Phase | Proposed weeks |
|---|---|
| Planning and literature | 1–3 |
| System design | 4–6 |
| Development and shared-platform integration | 7–14 |
| Testing, immediate assessment, and delayed assessment | 15–18 |
| Analysis and documentation | 19–22 |

Development tasks include embedded assessment, feedback, review mode, logging, and integration. Evaluation includes approximately 30 participants, immediate and delayed tests, observations, and surveys. The component owner handles its work, with team coordination for architecture, integration, and presentation.

Sources: PDF pages 51–54.

## Risks and supporting materials

- Marker recognition failure: optimize markers and test multiple devices.
- Learner interaction difficulty: provide a brief orientation.
- Participant withdrawal: plan for incomplete participation.
- Limited school devices: coordinate access in advance.
- Data loss from crashes: local backup and logging verification before deployment.

Supporting material includes age-appropriate surveys, teacher observation sheets, architecture/quiz/evaluation diagrams, UI mockups, and consent/ethical documents.

Sources: PDF pages 59–61.

## Integration with the shared application

Use the AR interaction component for machine exploration and the adaptive storytelling component for narrative progression. Preserve identifiable research relationships using anonymous participant codes and consistent session/chapter/question references.

The concrete identifiers and storage schema are development implications, not a prescribed schema in the PDF. Store real measurements and distinguish uncollected assessments from zero scores.
