using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FF2_Monster_Sim;

namespace SimTests
{
    [TestClass]
    public class TextManagerTest
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            TextManager.Initialize(0, 0);
        }

        [TestMethod]
        public void SetTextTests()
        {
            // Test invalid input
            Assert.ThrowsException<ArgumentException>(() => TextManager.SetActorText(null));
            Assert.ThrowsException<ArgumentException>(() => TextManager.SetActorText(""));
            Assert.ThrowsException<ArgumentException>(() => TextManager.SetTargetText(null));
            Assert.ThrowsException<ArgumentException>(() => TextManager.SetTargetText(""));
            Assert.ThrowsException<ArgumentException>(() => TextManager.SetHitsText(null));
            Assert.ThrowsException<ArgumentException>(() => TextManager.SetHitsText(""));
            Assert.ThrowsException<ArgumentException>(() => TextManager.SetDamageText(null));
            Assert.ThrowsException<ArgumentException>(() => TextManager.SetDamageText(""));
            Assert.ThrowsException<ArgumentException>(() => TextManager.SetResultsText(null));
            Assert.ThrowsException<ArgumentException>(() => TextManager.SetResultsText(""));

            // Test valid input
            // Just give it a valid string since nothing is returned or checkable...
            TextManager.SetActorText("Actor");
            TextManager.SetTargetText("Target");
            TextManager.SetHitsText("Hits");
            TextManager.SetDamageText("Damage");
            TextManager.SetResultsText("Results");

            // Static cleanup
            TextManager.Clear();
        }

        [TestMethod]
        public void TearDownTest()
        {
            // Build up a couple of texts and then tear them down
            TextManager.SetActorText("Actor");
            TextManager.SetTargetText("Target");
            TextManager.SetHitsText("Hits");
            TextManager.SetDamageText("Damage");
            TextManager.SetResultsText("Results");
            
            // Should be true 5 times before there's no text to tear down
            Assert.IsTrue(TextManager.TearDownText());
            Assert.IsTrue(TextManager.TearDownText());
            Assert.IsTrue(TextManager.TearDownText());
            Assert.IsTrue(TextManager.TearDownText());
            Assert.IsTrue(TextManager.TearDownText());
            Assert.IsFalse(TextManager.TearDownText());

            // Static cleanup
            TextManager.Clear();
        }

        [TestMethod]
        public void TearDownResultTest()
        {
            // Build up a couple of texts
            TextManager.SetActorText("Actor");
            TextManager.SetTargetText("Target");
            TextManager.SetHitsText("Hits");
            TextManager.SetDamageText("Damage");
            TextManager.SetResultsText("Results");

            // Teardown results, then ensure the rest of the teardowns work as expected
            TextManager.TearDownResults();
            Assert.IsTrue(TextManager.TearDownText());
            Assert.IsTrue(TextManager.TearDownText());
            Assert.IsTrue(TextManager.TearDownText());
            Assert.IsTrue(TextManager.TearDownText());
            Assert.IsFalse(TextManager.TearDownText());

            // Static cleanup
            TextManager.Clear();
        }
    }
}
