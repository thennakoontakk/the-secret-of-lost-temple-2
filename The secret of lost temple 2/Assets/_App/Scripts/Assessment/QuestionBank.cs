using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SecretOfLostTemple.Assessment
{
    /// <summary>
    /// Represents Bloom's Taxonomy cognitive levels for questions.
    /// </summary>
    public enum BloomLevel
    {
        Remember,
        Understand,
        Apply,
        Analyze
    }

    /// <summary>
    /// Represents the format of the assessment question.
    /// </summary>
    public enum QuestionType
    {
        MultipleChoice,
        DragToMatch,
        SpatialReasoning,
        TrueFalse
    }

    /// <summary>
    /// Data class holding information for a single quiz question within the AR assessment system.
    /// </summary>
    [Serializable]
    public class QuizQuestion
    {
        public string questionId;
        public string questionText;
        public QuestionType type;
        public string machineType; // e.g., "Lever", "Pulley", "InclinedPlane", "WheelAndAxle"
        public int difficultyTier; // 1, 2, 3
        public BloomLevel bloomLevel;
        public List<string> options; // Used for MultipleChoice
        public int correctAnswerIndex;
        public string explanation;
        public string templeContext; // e.g., "This lever mechanism guards the inner sanctum..."
        public bool isPreTest;
        public bool isPostTest;
        public bool isRetentionTest;
        public float pointValue;
    }

    /// <summary>
    /// Central repository holding all quiz questions for the AR assessment system.
    /// </summary>
    public static class QuestionBank
    {
        private static List<QuizQuestion> _allQuestions;

        static QuestionBank()
        {
            InitializeQuestions();
        }

        private static void InitializeQuestions()
        {
            _allQuestions = new List<QuizQuestion>();

            // ================= LEVER (6 questions) =================
            // Remember 1 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "L_Rem_1", questionText = "What are the three main components of a lever?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "Fulcrum, Effort, Load", "Wheel, Axle, Rope", "Base, Height, Length", "Force, Mass, Acceleration" },
                correctAnswerIndex = 0, explanation = "A lever is a rigid bar that pivots on a fixed fulcrum to move a load using an applied effort force.",
                templeContext = "Ancient temple records describe the secret mechanism as balanced on a pivotal stone.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 10f
            });
            // Remember 2 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "L_Rem_2", questionText = "Around which point does a lever rotate or pivot?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "Load", "Effort", "Fulcrum", "Anchor" },
                correctAnswerIndex = 2, explanation = "The fulcrum is the fixed pivot point around which a lever rotates.",
                templeContext = "Observe how the stone beam balances steadily upon the triangular fulcrum.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 10f
            });
            // Understand 1 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "L_Und_1", questionText = "What happens to the required effort force when the fulcrum is moved closer to the load?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "Effort increases", "Effort decreases", "Effort remains unchanged", "Lifting becomes impossible" },
                correctAnswerIndex = 1, explanation = "A shorter load arm and a longer effort arm provide higher mechanical advantage, requiring less effort.",
                templeContext = "Adjusting the fulcrum closer allows the heavy temple gate to be lifted effortlessly.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 15f
            });
            // Understand 2 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "L_Und_2", questionText = "Why is a crowbar or hoe considered a lever?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "Because it rolls", "Because it contains a wheel", "Because it pivots around a point to lift something", "Because it crushes heavy stones" },
                correctAnswerIndex = 2, explanation = "A crowbar uses a pivot point (fulcrum) to lift heavy objects (load) using applied force (effort).",
                templeContext = "Temple builders used long wooden beams as crowbars to position the colossal statues.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 15f
            });
            // Apply 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "L_App_1", questionText = "You need to lift a 100 kg stone block. Where should you place the fulcrum?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 3, bloomLevel = BloomLevel.Apply,
                options = new List<string> { "Exactly in the center", "As close to the stone block as possible", "As far from the stone block as possible", "Directly under your hands" },
                correctAnswerIndex = 1, explanation = "Placing the fulcrum close to the load maximizes the effort arm, providing maximum mechanical advantage.",
                templeContext = "To lift this massive obsidian block, position the fulcrum carefully near the weight.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 20f
            });
            // Analyze 1 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "L_Ana_1", questionText = "What is the difference when using a second-class lever (load in the middle) instead of a first-class lever?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 3, bloomLevel = BloomLevel.Analyze,
                options = new List<string> { "The load moves a shorter distance than the effort", "You have to apply greater effort than the load", "The lever breaks under pressure", "It functions as a pulley" },
                correctAnswerIndex = 0, explanation = "Second-class levers always offer a mechanical advantage (>1), but the load travels a shorter distance than the effort.",
                templeContext = "This wheelbarrow-like carrier transports sacred artifacts across the temple.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 25f
            });

            // ================= PULLEY (6 questions) =================
            // Remember 1 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_Rem_1", questionText = "What is a simple pulley?", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "A grooved wheel that carries a rope or cord", "A flat inclined sliding board", "A rigid pivoting beam", "A pair of interlocking gears" },
                correctAnswerIndex = 0, explanation = "A pulley is a wheel with a grooved rim that guides a flexible rope, belt, or chain.",
                templeContext = "Look up! Ropes pass smoothly through grooved wooden wheels attached to the temple ceiling.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 10f
            });
            // Remember 2 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_Rem_2", questionText = "A fixed pulley changes the ___ of the applied force.", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "Magnitude", "Direction", "Color", "Speed" },
                correctAnswerIndex = 1, explanation = "A fixed pulley does not reduce the required effort, but it conveniently changes the direction of force.",
                templeContext = "Pulling this vine downward raises the water bucket from the temple well.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 10f
            });
            // Understand 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_Und_1", questionText = "How does a movable pulley provide mechanical advantage?", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "By changing force direction", "By sharing half the load weight with a fixed anchor", "By shortening the rope", "By spinning faster" },
                correctAnswerIndex = 1, explanation = "In a movable pulley, one end of the rope is fixed to an anchor, supporting half the load weight.",
                templeContext = "This pulley travels along with the stone block, transferring half its weight onto the temple wall.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 15f
            });
            // Understand 2 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_Und_2", questionText = "With a pulley system having 4 supporting rope strands, how much effort is needed to lift a 100 N load?", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "100 N", "50 N", "25 N", "400 N" },
                correctAnswerIndex = 2, explanation = "Dividing the 100 N load by 4 supporting rope segments yields an effort of 25 N.",
                templeContext = "Lifting the heavy chandelier requires a multi-rope tackle system for a single explorer.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 15f
            });
            // Apply 1 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_App_1", questionText = "To hoist a heavy artifact crate from a deep pit with minimum effort, which system should you use?", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 3, bloomLevel = BloomLevel.Apply,
                options = new List<string> { "A single fixed pulley", "No pulleys", "A multi-pulley system (block and tackle)", "A first-class lever" },
                correctAnswerIndex = 2, explanation = "Combining multiple pulleys into a block-and-tackle system significantly reduces the input effort needed.",
                templeContext = "The treasure pit is deep; combine multiple ancient pulleys to retrieve the relic easily.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 20f
            });
            // Analyze 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_Ana_1", questionText = "When using a high mechanical advantage pulley system, why must you pull more rope?", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 3, bloomLevel = BloomLevel.Analyze,
                options = new List<string> { "Because Work = Force × Distance", "Because the rope stretches", "Because friction increases", "Because the load gets heavier" },
                correctAnswerIndex = 0, explanation = "To do the same work with less force, that force must be applied over a proportionally greater distance.",
                templeContext = "Notice how pulling several meters of rope raises the heavy gate by just a few centimeters.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 25f
            });

            // ================= INCLINED PLANE (6 questions) =================
            // Remember 1 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_Rem_1", questionText = "What is an inclined plane?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "A flat, sloping surface", "A rotating disk", "A rigid pivoting rod", "A grooved wheel" },
                correctAnswerIndex = 0, explanation = "An inclined plane is a flat surface tilted at an angle, used to raise or lower heavy objects.",
                templeContext = "Since the stairway was too steep, builders laid a smooth sloping stone ramp alongside it.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 10f
            });
            // Remember 2 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_Rem_2", questionText = "Which of the following is an example of an inclined plane?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "Wheelbarrow", "Ramp", "Seesaw", "Flagpole" },
                correctAnswerIndex = 1, explanation = "A loading ramp is a classic inclined plane connecting a lower level to an elevated level.",
                templeContext = "Giant pyramid stones were hauled upward along massive earthen ramps.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 10f
            });
            // Understand 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_Und_1", questionText = "How does a longer, gentler ramp affect the effort needed to push a heavy stone upward?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "It increases the effort", "It decreases the effort", "No change", "It makes the stone heavier" },
                correctAnswerIndex = 1, explanation = "A gentler slope spreads the vertical lifting work across a longer travel distance, reducing the required pushing force.",
                templeContext = "Builders lengthened the stone ramp so massive guardian statues could be rolled into place easily.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 15f
            });
            // Understand 2 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_Und_2", questionText = "A wedge is a modified version of which simple machine?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "Lever", "Pulley", "Wheel and Axle", "Inclined Plane" },
                correctAnswerIndex = 3, explanation = "A wedge consists essentially of two inclined planes placed back to back.",
                templeContext = "The sharp stone axe used for cutting timber acts as a double inclined plane.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 15f
            });
            // Apply 1 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_App_1", questionText = "To raise a sarcophagus 2 meters high with minimum pushing effort, which ramp design should you construct?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 3, bloomLevel = BloomLevel.Apply,
                options = new List<string> { "A 2-meter long ramp", "A 10-meter long ramp", "A vertical wall", "A step ladder" },
                correctAnswerIndex = 1, explanation = "The 10-meter ramp has a much gentler incline, minimizing the effort force needed to push the weight upward.",
                templeContext = "Select the longest timber planks to construct a smooth, low-effort path to the royal altar.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 20f
            });
            // Analyze 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_Ana_1", questionText = "Why might pushing a stone block up a ramp be harder in practice than theoretical calculations suggest?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 3, bloomLevel = BloomLevel.Analyze,
                options = new List<string> { "Due to gravity", "Due to friction between the block and the ramp", "Because the ramp generates heat", "Due to air resistance" },
                correctAnswerIndex = 1, explanation = "Friction resists sliding motion. While the ramp reduces lifting effort, friction introduces additional resisting force.",
                templeContext = "The sandstone blocks drag heavily against the rough ramp surface, creating friction resistance.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 25f
            });

            // ================= WHEEL & AXLE (6 questions) =================
            // Remember 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_Rem_1", questionText = "Which simple machine consists of a large disk attached to a smaller central cylinder?", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "Lever", "Inclined Plane", "Wheel and Axle", "Pulley" },
                correctAnswerIndex = 2, explanation = "A wheel and axle consists of a larger wheel secured to a smaller concentric axle shaft.",
                templeContext = "The colossal stone steering wheel is rigidly coupled to a central wooden axle.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 10f
            });
            // Remember 2 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_Rem_2", questionText = "In a wheel and axle system, which part travels a greater distance in one complete rotation?", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "The axle", "The wheel", "Both travel the same distance", "The base" },
                correctAnswerIndex = 1, explanation = "The larger outer circumference of the wheel travels a greater linear distance than the smaller axle in each rotation.",
                templeContext = "Compare the outer rim of the temple sundial wheel against its central mounting shaft.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 10f
            });
            // Understand 1 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_Und_1", questionText = "Applying effort to the larger wheel to rotate the smaller axle provides a mechanical advantage in...", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "Speed", "Force", "Friction", "Distance" },
                correctAnswerIndex = 1, explanation = "Turning the larger wheel multiplies your input force on the smaller axle (e.g., a steering wheel or doorknob).",
                templeContext = "By turning the wide outer ring, you effortlessly turn the heavy central sanctuary gear.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 15f
            });
            // Understand 2 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_Und_2", questionText = "Applying force to the axle to spin the outer wheel provides an advantage in...", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "Speed and distance", "Force", "Weight", "Balance" },
                correctAnswerIndex = 0, explanation = "Turning the axle requires greater effort force, but causes the outer wheel rim to move much faster and farther.",
                templeContext = "The rapidly rotating temple windmill operates by driving its central spindle shaft.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 15f
            });
            // Apply 1 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_App_1", questionText = "To make opening a heavy temple door using a doorknob easier, what design should you choose?", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 3, bloomLevel = BloomLevel.Apply,
                options = new List<string> { "Use a smaller doorknob", "Use a larger diameter doorknob", "Remove the knob", "Push directly on the hinges" },
                correctAnswerIndex = 1, explanation = "A larger diameter knob (wheel) provides greater mechanical advantage over the latch spindle (axle).",
                templeContext = "Giant circular golden knobs enable even an explorer to turn massive temple latches.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 20f
            });
            // Analyze 1 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_Ana_1", questionText = "How does the ratio of wheel radius to axle radius affect the machine mechanical advantage?", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 3, bloomLevel = BloomLevel.Analyze,
                options = new List<string> { "A larger radius ratio produces greater mechanical advantage", "A larger radius ratio reduces mechanical advantage", "The ratio has no effect", "It only affects speed, not force" },
                correctAnswerIndex = 0, explanation = "Mechanical advantage is calculated as Wheel Radius divided by Axle Radius; increasing this ratio multiplies force further.",
                templeContext = "Temple engineers calculated the precise wheel-to-axle diameter ratio so gates could be lifted smoothly.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 25f
            });

            // Ensure all Sinhala question texts, contexts, options, and explanations are preprocessed for ligatures
            foreach (var q in _allQuestions)
            {
                q.questionText = SecretOfLostTemple.Core.SinhalaTextHelper.Preprocess(q.questionText);
                q.templeContext = SecretOfLostTemple.Core.SinhalaTextHelper.Preprocess(q.templeContext);
                q.explanation = SecretOfLostTemple.Core.SinhalaTextHelper.Preprocess(q.explanation);
                if (q.options != null)
                {
                    for (int i = 0; i < q.options.Count; i++)
                    {
                        q.options[i] = SecretOfLostTemple.Core.SinhalaTextHelper.Preprocess(q.options[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the entire bank of questions.
        /// </summary>
        public static List<QuizQuestion> GetAllQuestions()
        {
            return new List<QuizQuestion>(_allQuestions);
        }

        /// <summary>
        /// Retrieves questions pertaining to a specific machine type.
        /// </summary>
        public static List<QuizQuestion> GetQuestionsByMachine(string machineType)
        {
            return _allQuestions.Where(q => q.machineType.Equals(machineType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Retrieves questions matching a specific difficulty tier.
        /// </summary>
        public static List<QuizQuestion> GetQuestionsByDifficulty(int tier)
        {
            return _allQuestions.Where(q => q.difficultyTier == tier).ToList();
        }

        /// <summary>
        /// Retrieves questions matching a specific Bloom's Taxonomy level.
        /// </summary>
        public static List<QuizQuestion> GetQuestionsByBloom(BloomLevel level)
        {
            return _allQuestions.Where(q => q.bloomLevel == level).ToList();
        }

        /// <summary>
        /// Retrieves all questions designated for the Pre-Test.
        /// </summary>
        public static List<QuizQuestion> GetPreTestQuestions()
        {
            return _allQuestions.Where(q => q.isPreTest).ToList();
        }

        /// <summary>
        /// Retrieves all questions designated for the Post-Test.
        /// </summary>
        public static List<QuizQuestion> GetPostTestQuestions()
        {
            return _allQuestions.Where(q => q.isPostTest).ToList();
        }

        /// <summary>
        /// Retrieves all questions designated for Retention Testing.
        /// </summary>
        public static List<QuizQuestion> GetRetentionTestQuestions()
        {
            return _allQuestions.Where(q => q.isRetentionTest).ToList();
        }
    }
}
