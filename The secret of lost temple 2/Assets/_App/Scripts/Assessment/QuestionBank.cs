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
                questionId = "L_Rem_1", questionText = "What are the three main parts of a lever?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "Fulcrum, Effort, Load", "Wheel, Axle, Rope", "Base, Height, Length", "Force, Mass, Acceleration" },
                correctAnswerIndex = 0, explanation = "A lever consists of a rigid beam pivoting on a fulcrum to move a load with effort.",
                templeContext = "The ancient texts speak of a pivoting stone that controls the hidden door.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 10f
            });
            // Remember 2 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "L_Rem_2", questionText = "Which point does a lever pivot around?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "The Load", "The Effort", "The Fulcrum", "The Anchor" },
                correctAnswerIndex = 2, explanation = "The fulcrum is the fixed pivot point of a lever.",
                templeContext = "Notice how the stone beam balances on this central triangular rock.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 10f
            });
            // Understand 1 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "L_Und_1", questionText = "How does moving the fulcrum closer to the load affect the effort required?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "Increases effort", "Decreases effort", "Effort stays the same", "Makes it impossible" },
                correctAnswerIndex = 1, explanation = "A shorter load arm and longer effort arm gives you a mechanical advantage, requiring less effort.",
                templeContext = "By adjusting this fulcrum stone, the heavy gate feels much lighter.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 15f
            });
            // Understand 2 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "L_Und_2", questionText = "Why is a crowbar considered a lever?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "It spins", "It has a wheel", "It pivots on a point to lift something", "It rolls heavy stones" },
                correctAnswerIndex = 2, explanation = "A crowbar uses a pivot point (fulcrum) to lift heavy objects (load).",
                templeContext = "The temple builders used long wooden beams just like crowbars to pry these statues into place.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 15f
            });
            // Apply 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "L_App_1", questionText = "You need to lift a 100kg stone block. Where should you place the fulcrum?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 3, bloomLevel = BloomLevel.Apply,
                options = new List<string> { "Exactly in the middle", "As close to the stone block as possible", "As far from the stone block as possible", "Directly under your hands" },
                correctAnswerIndex = 1, explanation = "Placing the fulcrum close to the load maximizes the effort arm's length.",
                templeContext = "To move this massive obsidian block, position the pivot carefully.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 20f
            });
            // Analyze 1 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "L_Ana_1", questionText = "If a Class 2 lever (load in middle) is used instead of a Class 1 lever (fulcrum in middle), what is the trade-off?", type = QuestionType.MultipleChoice,
                machineType = "Lever", difficultyTier = 3, bloomLevel = BloomLevel.Analyze,
                options = new List<string> { "You move the load a shorter distance", "You must use more effort", "The lever breaks", "It becomes a pulley" },
                correctAnswerIndex = 0, explanation = "Class 2 levers always provide mechanical advantage, but the load moves less distance than the effort.",
                templeContext = "This wheelbarrow-like mechanism transports the sacred relics.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 25f
            });

            // ================= PULLEY (6 questions) =================
            // Remember 1 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_Rem_1", questionText = "What defines a simple pulley?", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "A wheel with a groove for a rope", "A flat sliding board", "A rigid pivoting bar", "Two interlocked gears" },
                correctAnswerIndex = 0, explanation = "A pulley is a wheel that carries a flexible rope, cord, or chain on its rim.",
                templeContext = "Look up! Ropes pass through these grooved wooden wheels attached to the ceiling.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 10f
            });
            // Remember 2 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_Rem_2", questionText = "A fixed pulley changes the ___ of the applied force.", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "Magnitude", "Direction", "Color", "Speed" },
                correctAnswerIndex = 1, explanation = "A fixed pulley doesn't reduce the effort needed, but lets you pull down to lift something up.",
                templeContext = "Pulling down on this vine raises the water bucket from the temple well.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 10f
            });
            // Understand 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_Und_1", questionText = "How does a movable pulley provide a mechanical advantage?", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "By changing direction", "By supporting half the load's weight on a fixed anchor", "By making the rope shorter", "By spinning faster" },
                correctAnswerIndex = 1, explanation = "In a movable pulley, one end of the rope is fixed, taking half the load's weight.",
                templeContext = "This pulley moves with the block, sharing the heavy burden with the temple wall.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 15f
            });
            // Understand 2 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_Und_2", questionText = "If you use a block and tackle with 4 rope sections supporting the load, how much effort is needed to lift 100N?", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "100N", "50N", "25N", "400N" },
                correctAnswerIndex = 2, explanation = "100N divided by 4 supporting ropes equals 25N of effort.",
                templeContext = "The grand chandelier requires complex rope systems to be hoisted by a single monk.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 15f
            });
            // Apply 1 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_App_1", questionText = "To lift a cage of artifacts out of a deep pit with the least force, you should use:", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 3, bloomLevel = BloomLevel.Apply,
                options = new List<string> { "One fixed pulley", "No pulleys", "A system of multiple pulleys", "A lever" },
                correctAnswerIndex = 2, explanation = "Adding multiple pulleys creates a block and tackle, greatly reducing the required force.",
                templeContext = "The artifact pit is deep; combine these ancient wheels to haul the treasure.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 20f
            });
            // Analyze 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "P_Ana_1", questionText = "Why do you have to pull more rope when using a mechanical advantage pulley system?", type = QuestionType.MultipleChoice,
                machineType = "Pulley", difficultyTier = 3, bloomLevel = BloomLevel.Analyze,
                options = new List<string> { "Because work equals force times distance", "Because the rope stretches", "Because friction increases", "Because the load gets heavier" },
                correctAnswerIndex = 0, explanation = "To do the same amount of work with less force, you must apply that force over a greater distance.",
                templeContext = "Notice how you must pull meters of rope just to lift the gate a few centimeters.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 25f
            });

            // ================= INCLINED PLANE (6 questions) =================
            // Remember 1 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_Rem_1", questionText = "What is an inclined plane?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "A flat, sloped surface", "A spinning disk", "A rigid bar", "A grooved wheel" },
                correctAnswerIndex = 0, explanation = "An inclined plane is simply a ramp or sloped surface used to raise or lower objects.",
                templeContext = "The grand staircase was too steep, so they built this smooth stone ramp.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 10f
            });
            // Remember 2 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_Rem_2", questionText = "Which of these is an example of an inclined plane?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "A wheelbarrow", "A ramp", "A seesaw", "A flagpole" },
                correctAnswerIndex = 1, explanation = "A ramp is a classic inclined plane.",
                templeContext = "The pyramid's blocks were dragged up enormous ramps.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 10f
            });
            // Understand 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_Und_1", questionText = "How does a longer, gentler slope affect the effort needed to push a block up?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "Increases effort", "Decreases effort", "No change", "Makes it heavier" },
                correctAnswerIndex = 1, explanation = "A gentler slope spreads the work over a longer distance, requiring less effort.",
                templeContext = "The builders extended this ramp so the massive statues could be pushed easily.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 15f
            });
            // Understand 2 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_Und_2", questionText = "A wedge is a modification of which simple machine?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "Lever", "Pulley", "Wheel and Axle", "Inclined Plane" },
                correctAnswerIndex = 3, explanation = "A wedge is essentially two inclined planes placed back-to-back.",
                templeContext = "This sharp stone axe used to split wood is a double ramp.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 15f
            });
            // Apply 1 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_App_1", questionText = "To raise a sarcophagus 2 meters with minimal force, what kind of ramp should you build?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 3, bloomLevel = BloomLevel.Apply,
                options = new List<string> { "A 2-meter long ramp", "A 10-meter long ramp", "A vertical wall", "A step-like structure" },
                correctAnswerIndex = 1, explanation = "The 10-meter ramp has a gentler slope, reducing the required force.",
                templeContext = "Choose the longest wooden planks to form the gentlest path for the royal tomb.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 20f
            });
            // Analyze 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "IP_Ana_1", questionText = "Why might pushing a block up a ramp be harder than expected in reality?", type = QuestionType.MultipleChoice,
                machineType = "InclinedPlane", difficultyTier = 3, bloomLevel = BloomLevel.Analyze,
                options = new List<string> { "Because of gravity", "Because of friction between the block and ramp", "Because ramps generate heat", "Because of air resistance" },
                correctAnswerIndex = 1, explanation = "Friction opposes motion. While the ramp reduces lifting force, friction adds a new resistance.",
                templeContext = "The stone blocks drag harshly against the sandy limestone ramp.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 25f
            });

            // ================= WHEEL & AXLE (6 questions) =================
            // Remember 1 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_Rem_1", questionText = "What consists of a larger disk connected to a smaller cylinder?", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "A Lever", "An Inclined Plane", "A Wheel and Axle", "A Pulley" },
                correctAnswerIndex = 2, explanation = "A wheel and axle features a large wheel attached to a smaller axle.",
                templeContext = "The giant steering wheel is fixed to a sturdy wooden shaft.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 10f
            });
            // Remember 2 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_Rem_2", questionText = "Which part of the wheel and axle turns a greater distance?", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 1, bloomLevel = BloomLevel.Remember,
                options = new List<string> { "The Axle", "The Wheel", "They turn the same distance", "The Base" },
                correctAnswerIndex = 1, explanation = "The outer rim of the larger wheel covers more distance per rotation than the axle.",
                templeContext = "Trace the outer edge of this sun dial wheel compared to its center peg.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 10f
            });
            // Understand 1 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_Und_1", questionText = "Applying force to the larger wheel to turn the axle creates a mechanical advantage in...", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "Speed", "Force", "Friction", "Distance" },
                correctAnswerIndex = 1, explanation = "Turning the larger wheel multiplies your force on the smaller axle.",
                templeContext = "By gripping the wide outer ring, you can easily turn the heavy central gear.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 15f
            });
            // Understand 2 (Post)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_Und_2", questionText = "Applying force to the axle to turn the wheel creates a mechanical advantage in...", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 2, bloomLevel = BloomLevel.Understand,
                options = new List<string> { "Speed and Distance", "Force", "Weight", "Balance" },
                correctAnswerIndex = 0, explanation = "Turning the axle requires more force, but the outer wheel moves faster and farther (like a car axle).",
                templeContext = "The fast-spinning ancient fan operates by turning the central rod rapidly.",
                isPreTest = false, isPostTest = true, isRetentionTest = false, pointValue = 15f
            });
            // Apply 1 (Retention)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_App_1", questionText = "To make it easier to open a heavy temple door using a doorknob, you should...", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 3, bloomLevel = BloomLevel.Apply,
                options = new List<string> { "Use a smaller doorknob", "Use a larger doorknob", "Remove the doorknob", "Push harder" },
                correctAnswerIndex = 1, explanation = "A larger doorknob (wheel) increases the mechanical advantage over the latch (axle).",
                templeContext = "The giant golden knobs were designed to let even a child open the massive gates.",
                isPreTest = false, isPostTest = false, isRetentionTest = true, pointValue = 20f
            });
            // Analyze 1 (Pre)
            _allQuestions.Add(new QuizQuestion {
                questionId = "WA_Ana_1", questionText = "How does the ratio of the wheel radius to the axle radius affect the machine?", type = QuestionType.MultipleChoice,
                machineType = "WheelAndAxle", difficultyTier = 3, bloomLevel = BloomLevel.Analyze,
                options = new List<string> { "A larger ratio means greater mechanical advantage", "A larger ratio means less advantage", "The ratio has no effect", "It only affects speed, not force" },
                correctAnswerIndex = 0, explanation = "Mechanical advantage is calculated by dividing the radius of the wheel by the radius of the axle.",
                templeContext = "The engineers calculated the exact size of this capstan to ensure the guards could lift the portcullis.",
                isPreTest = true, isPostTest = false, isRetentionTest = false, pointValue = 25f
            });
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
