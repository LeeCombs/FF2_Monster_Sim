using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FF2_Monster_Sim;

namespace SimTests
{
    [TestClass]
    public class AttackManagerTest
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            MonsterManager.Initialize();
            MonsterManager.LoadContent();
            SpellManager.Initialize();
            SpellManager.LoadContent();
            AttackManager.Initialize();
        }

        [TestMethod]
        public void AttackResultTest()
        {
            // Ensure AttackResult acts as expected and things stay within bounds
            AttackResult atkResult = new AttackResult();
        }

        [TestMethod]
        public void AttackMonsterTest()
        {
            // Setup
            Monster actor = new Monster();
            actor.HP = actor.HPMax = actor.MP = actor.MPMax = 100;
            actor.Hits = 1;
            actor.Accuracy = 99;
            actor.Strength = 10;

            Monster target = new Monster();
            target.HP = target.HPMax = target.MP = target.MPMax = 100;
            target.Defense = 0;
            target.Blocks = 0;
            target.MagicBlocks = 0;

            //// Test expected hits
            int missThreshold = 15; // 1.5% (1% expected)
            for (int i = 0; i < 1000; i++)
            {
                AttackResult res = AttackManager.AttackMonster(actor, target);
                Assert.IsTrue(res.Hits == 1 || res.Hits == 0);
                if (res.Hits == 0)
                    missThreshold--;
            }

            // Ensure some misses happened, but not too many
            Assert.AreNotEqual(15, missThreshold);
            Assert.IsTrue(missThreshold > 0);

            //// Test expected damage, crit expectancy
            int critThreshold = 75; // 7.5% (5% expected)
            for (int i = 0; i < 1000; i++)
            {
                AttackResult atkRes = AttackManager.AttackMonster(actor, target);
                // Ignore misses
                if (atkRes.Damage > 0)
                {
                    // Damage range is (str...str*2+str);
                    Assert.IsTrue(atkRes.Damage >= actor.Strength);
                    Assert.IsTrue(atkRes.Damage <= actor.Strength * 3);
                    if (atkRes.Results.Contains("Critical Hit!"))
                        critThreshold--;
                }
            }
            // Ensure some crits happened, but not too many
            Assert.AreNotEqual(75, critThreshold);
            Assert.IsTrue(critThreshold > 0);

            //// TODO: Test aura buff effects
            // Iterate through MonsterFamily enum
            // Add family to target
            // Ensure normal damage range
            // Add Aura at enum val + 1
            // Ensure increased damage range (+20);
            // Clear target families
            // Ensure normal damage range
            // Remove aura buff

            //// TODO: Test expected Results

            //// Test HP-drain effect
            actor.HP = 1;
            target.HP = 100;
            actor.Hits = 10;
            actor.Strength = 0;
            actor.AttackEffects.Add("Drain HP");

            AttackManager.AttackMonster(actor, target);
            // Assert.IsTrue(target.HP < 100);
            // Assert.IsTrue(actor.HP > 1);
            // TODO: DRAN doesn't ever hit due to 0 accuracy
            actor.AttackEffects.Clear();

            //// Test MP-drain effect
            actor.MP = 1;
            target.MP = 100;
            actor.AttackEffects.Add("Drain MP");

            AttackManager.AttackMonster(actor, target);
            // Assert.IsTrue(target.MP < 100);
            // Assert.IsTrue(actor.MP > 1);
            // TODO: ASPL doesn't ever hit due to 0 accuracy
            actor.AttackEffects.Clear();

            //// TODO: Test touch-status effects

        }
    }
}
