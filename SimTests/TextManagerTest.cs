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
            //
        }

        [TestMethod]
        public void SetTextTests()
        {
            // Setup
            TextManager textManager = new TextManager();
            textManager.Initialize(0, 0);

            // Test invalid input
            Assert.ThrowsException<ArgumentException>(() => textManager.SetActorText(null));
            Assert.ThrowsException<ArgumentException>(() => textManager.SetActorText(""));
            Assert.ThrowsException<ArgumentException>(() => textManager.SetTargetText(null));
            Assert.ThrowsException<ArgumentException>(() => textManager.SetTargetText(""));
            Assert.ThrowsException<ArgumentException>(() => textManager.SetHitsText(null));
            Assert.ThrowsException<ArgumentException>(() => textManager.SetHitsText(""));
            Assert.ThrowsException<ArgumentException>(() => textManager.SetDamageText(null));
            Assert.ThrowsException<ArgumentException>(() => textManager.SetDamageText(""));
            Assert.ThrowsException<ArgumentException>(() => textManager.SetResultsText(null));
            Assert.ThrowsException<ArgumentException>(() => textManager.SetResultsText(""));

            // Test valid input
            // Just give it a valid string since nothing is returned or checkable...
            textManager.SetActorText("Actor");
            textManager.SetTargetText("Target");
            textManager.SetHitsText("Hits");
            textManager.SetDamageText("Damage");
            textManager.SetResultsText("Results");
        }

        [TestMethod]
        public void TearDownTest()
        {
            // Setup
            TextManager textManager = new TextManager();
            textManager.Initialize(0, 0);

            // Build up a couple of texts and then tear them down
            textManager.SetActorText("Actor");
            textManager.SetTargetText("Target");
            textManager.SetHitsText("Hits");
            textManager.SetDamageText("Damage");
            textManager.SetResultsText("Results");
            
            // Should be true 5 times before there's no text to tear down
            Assert.IsTrue(textManager.TearDownText());
            Assert.IsTrue(textManager.TearDownText());
            Assert.IsTrue(textManager.TearDownText());
            Assert.IsTrue(textManager.TearDownText());
            Assert.IsTrue(textManager.TearDownText());
            Assert.IsFalse(textManager.TearDownText());
        }

        [TestMethod]
        public void TearDownResultTest()
        {
            // Setup
            TextManager textManager = new TextManager();
            textManager.Initialize(0, 0);

            // Build up a couple of texts
            textManager.SetActorText("Actor");
            textManager.SetTargetText("Target");
            textManager.SetHitsText("Hits");
            textManager.SetDamageText("Damage");
            textManager.SetResultsText("Results");

            // Teardown results, then ensure the rest of the teardowns work as expected
            textManager.TearDownResults();
            Assert.IsTrue(textManager.TearDownText());
            Assert.IsTrue(textManager.TearDownText());
            Assert.IsTrue(textManager.TearDownText());
            Assert.IsTrue(textManager.TearDownText());
            Assert.IsFalse(textManager.TearDownText());


        }

    }
}
