using System;
using System.Reflection;
using NUnit.Framework;

namespace SecretOfLostTemple.Tests.Editor
{
    [TestFixture]
    public class SinhalaTextHelperTests
    {
        private MethodInfo _preprocessMethod;

        [SetUp]
        public void Setup()
        {
            Type helperType = Type.GetType("SecretOfLostTemple.Core.SinhalaTextHelper, Assembly-CSharp", true);
            _preprocessMethod = helperType.GetMethod("Preprocess", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(_preprocessMethod, "SinhalaTextHelper.Preprocess method not found.");
        }

        private string Preprocess(string text)
        {
            return (string)_preprocessMethod.Invoke(null, new object[] { text });
        }

        [Test]
        public void Preprocess_ReplacesBrokenYanthraWithProperLigature()
        {
            // "යන්ත්ර" (without ZWJ) must become "යන්ත්‍ර" (with ZWJ)
            string broken = "යන්ත්ර";
            string shaped = Preprocess(broken);

            Assert.AreEqual("යන්ත්‍ර", shaped);
            Assert.IsTrue(shaped.Contains("\u0DAD\u0DCA\u200D\u0DBB"));
        }

        [Test]
        public void Preprocess_PreservesExistingProperLigature()
        {
            string proper = "යන්ත්‍ර";
            string shaped = Preprocess(proper);

            Assert.AreEqual("යන්ත්‍ර", shaped);
        }

        [Test]
        public void Preprocess_ShapesAllRequiredConjuncts()
        {
            // 1. "ත්ර" -> "ත්‍ර"
            Assert.AreEqual("ත්‍ර", Preprocess("ත්ර"));

            // 2. "න්ත්ර" -> "න්ත්‍ර"
            Assert.AreEqual("න්ත්‍ර", Preprocess("න්ත්ර"));

            // 3. "ක්ෂ" -> "ක්‍ෂ"
            Assert.IsTrue(Preprocess("ක්ෂ").Contains("\u0D9A\u0DCA\u200D\u0DC2"));

            // 4. "න්ද" -> "න්‍ද"
            Assert.IsTrue(Preprocess("න්ද").Contains("\u0DB1\u0DCA\u200D\u0DAF"));

            // 5. "ර්ය" -> "ර්‍ය"
            Assert.IsTrue(Preprocess("ර්ය").Contains("\u0DBB\u0DCA\u200D\u0DBA"));

            // 6. "යාන්ත්රික" -> "යාන්ත්‍රික"
            Assert.AreEqual("යාන්ත්‍රික", Preprocess("යාන්ත්රික"));
        }

        [Test]
        public void Preprocess_LeavesEnglishAndNullTextUnmodified()
        {
            Assert.IsNull(Preprocess(null));
            Assert.AreEqual("", Preprocess(""));
            Assert.AreEqual("START EXPEDITION", Preprocess("START EXPEDITION"));
            Assert.AreEqual("12345", Preprocess("12345"));
        }
    }
}
