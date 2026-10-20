using System;
using System.Text.RegularExpressions;

namespace SecretOfLostTemple.Core
{
    /// <summary>
    /// Utility for preprocessing and shaping Sinhala Unicode text strings in Unity UI.
    /// Standard Unity UI Text (legacy Text) does not execute OpenType GSUB tables for
    /// complex Indic/Sinhala script shaping. SinhalaTextHelper resolves broken or disjoint
    /// conjuncts (e.g. "යන්ත්ර" -> "යන්ත්‍ර") by reconstructing missing or broken Zero-Width Joiner
    /// (ZWJ \u200D) sequences for Rakaranshaya, Yanshaya, Bandi Akuru (touching letters),
    /// and ensures proper Ispilla/Papilla placement for high-legibility rendering in Iskoola Pota / Noto Sans fonts.
    /// </summary>
    public static class SinhalaTextHelper
    {
        private static readonly Regex ZwnjRegex = new Regex("\u200C", RegexOptions.Compiled);
        private static readonly Regex MultiZwjRegex = new Regex("\u200D{2,}", RegexOptions.Compiled);
        private static readonly Regex InvertedZwjViramaRegex = new Regex("\u200D\u0DCA", RegexOptions.Compiled);

        // Kssa: Ka + Virama + Ssa -> Ka + Virama + ZWJ + Ssa
        private static readonly Regex KssaRegex = new Regex(@"\u0D9A\u0DCA(?!\u200D)\u0DC2", RegexOptions.Compiled);

        // Common Bandi Akuru (Touching / Joined consonants):
        // Nda: Na + Virama + Da
        private static readonly Regex NdaRegex = new Regex(@"\u0DB1\u0DCA(?!\u200D)\u0DAF", RegexOptions.Compiled);
        // Ndda: NNa + Virama + DDa
        private static readonly Regex NddaRegex = new Regex(@"\u0DAB\u0DCA(?!\u200D)\u0DA9", RegexOptions.Compiled);
        // Ndha: Na + Virama + Dha
        private static readonly Regex NdhaRegex = new Regex(@"\u0DB1\u0DCA(?!\u200D)\u0DB0", RegexOptions.Compiled);
        // Mba: Ma + Virama + Ba
        private static readonly Regex MbaRegex = new Regex(@"\u0DB8\u0DCA(?!\u200D)\u0DB6", RegexOptions.Compiled);
        // Nnja: NYa + Virama + Ja
        private static readonly Regex NnjaRegex = new Regex(@"\u0DA4\u0DCA(?!\u200D)\u0DA2", RegexOptions.Compiled);
        // Nnca: NYa + Virama + Ca
        private static readonly Regex NncaRegex = new Regex(@"\u0DA4\u0DCA(?!\u200D)\u0DA0", RegexOptions.Compiled);

        // Yanshaya & Repaya (Consonant + Virama + Ya -> C + Virama + ZWJ + Ya):
        // Includes: ර්ය (Repaya), ත්‍ය, ද්‍ය, ක්‍ර, ප්‍ය, භ්‍ය, ම්‍ය, ල්‍ය, ව්‍ය, ශ්‍ය, ෂ්‍ය, ස්‍ය, ඛ්‍ය, ධ්‍ය
        private static readonly Regex YanshayaRegex = new Regex(@"([\u0D9A-\u0DC6])\u0DCA(?!\u200D)\u0DBA", RegexOptions.Compiled);

        // Rakaranshaya (Consonants except Ra + Virama + Ra -> C + Virama + ZWJ + Ra):
        // Handles "යන්ත්ර" -> "යන්ත්‍ර", "ක්ර" -> "ක්‍ර", "ප්ර" -> "ප්‍ර", "බ්ර" -> "බ්‍ර", "ද්ර" -> "ද්‍ර", etc.
        private static readonly Regex RakaranshayaRegex = new Regex(@"([^\u0DBB\u0D80-\u0D99\u0DC7-\u0DFF])\u0DCA(?!\u200D)\u0DBB", RegexOptions.Compiled);

        // Specific sequence for "න්ත්ර" (N + Ta + Ra without ZWJ -> න්ත්‍ර)
        private static readonly Regex NtraRegex = new Regex(@"\u0DB1\u0DCA\u0DAD\u0DCA(?!\u200D)\u0DBB", RegexOptions.Compiled);

        // Diacritic placement corrections (if Ispilla or Papilla is misplaced)
        private static readonly Regex MisplacedIspillaRegex = new Regex(@"([\u0D9A-\u0DC6])([\u0DD2\u0DD3])\u0DCA", RegexOptions.Compiled);
        private static readonly Regex MisplacedPapillaRegex = new Regex(@"([\u0D9A-\u0DC6])([\u0DD4\u0DD6])\u0DCA", RegexOptions.Compiled);

        /// <summary>
        /// Preprocesses any text. If it contains Sinhala characters, shapes conjuncts,
        /// restores missing ZWJ ligatures, and normalizes diacritics. Returns unmodified if non-Sinhala.
        /// </summary>
        public static string Preprocess(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (!ContainsSinhala(text)) return text;

            string result = text;

            // 1. Clean up rogue joiners and inverted sequences
            result = ZwnjRegex.Replace(result, "");
            result = InvertedZwjViramaRegex.Replace(result, "\u0DCA\u200D");
            result = MultiZwjRegex.Replace(result, "\u200D");

            // 2. Fix diacritic ordering if misplaced
            result = MisplacedIspillaRegex.Replace(result, "$1\u0DCA$2");
            result = MisplacedPapillaRegex.Replace(result, "$1\u0DCA$2");

            // 3. Fix specific multi-letter conjuncts first (e.g. න්ත්ර -> න්ත්‍ර)
            result = NtraRegex.Replace(result, "\u0DB1\u0DCA\u0DAD\u0DCA\u200D\u0DBB");

            // 4. Fix Rakaranshaya (C + Virama + Ra -> C + Virama + ZWJ + Ra, e.g. ත්ර -> ත්‍ර)
            result = RakaranshayaRegex.Replace(result, "$1\u0DCA\u200D\u0DBB");

            // 5. Fix Yanshaya & Repaya (C + Virama + Ya -> C + Virama + ZWJ + Ya, e.g. ර්ය -> ර්‍ය, ත්‍ය -> ත්‍ය)
            result = YanshayaRegex.Replace(result, "$1\u0DCA\u200D\u0DBA");

            // 6. Fix Kssa (Ka + Virama + Ssa -> ක් + ZWJ + ෂ)
            result = KssaRegex.Replace(result, "\u0D9A\u0DCA\u200D\u0DC2");

            // 7. Fix Bandi Akuru (Touching consonants: න්ද, ණ්ඩ, න්ධ, ම්බ, ඤ්ජ, ඤ්ච, න්ත)
            result = NdaRegex.Replace(result, "\u0DB1\u0DCA\u200D\u0DAF");
            result = NddaRegex.Replace(result, "\u0DAB\u0DCA\u200D\u0DA9");
            result = NdhaRegex.Replace(result, "\u0DB1\u0DCA\u200D\u0DB0");
            result = MbaRegex.Replace(result, "\u0DB8\u0DCA\u200D\u0DB6");
            result = NnjaRegex.Replace(result, "\u0DA4\u0DCA\u200D\u0DA2");
            result = NncaRegex.Replace(result, "\u0DA4\u0DCA\u200D\u0DA0");

            return result;
        }

        /// <summary>
        /// Alias for Preprocess for semantic convenience.
        /// </summary>
        public static string FixSinhalaText(string text) => Preprocess(text);

        /// <summary>
        /// Alias for Preprocess.
        /// </summary>
        public static string Shape(string text) => Preprocess(text);

        /// <summary>
        /// Extension method for string chaining.
        /// </summary>
        public static string ToSinhalaShaped(this string text) => Preprocess(text);

        /// <summary>
        /// Checks whether the string contains any Sinhala codepoints or joiners.
        /// </summary>
        public static bool ContainsSinhala(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if ((c >= '\u0D80' && c <= '\u0DFF') || c == '\u200D' || c == '\u200C')
                    return true;
            }
            return false;
        }
    }
}
