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

        /////////////////////////
        // AttackMonster Tests //
        /////////////////////////

        [TestMethod]
        public void AttackMonsterInputsTest()
        {
            // Setup
            Monster actor = new Monster();
            Monster target = new Monster();

            // Test valid input
            Assert.IsNotNull(AttackManager.AttackMonster(actor, target));

            // Test invalid inputs
            Assert.ThrowsException<ArgumentNullException>(() => AttackManager.AttackMonster(null, target));
            Assert.ThrowsException<ArgumentNullException>(() => AttackManager.AttackMonster(actor, null));
            Assert.ThrowsException<ArgumentNullException>(() => AttackManager.AttackMonster(null, null));
        }

        [TestMethod]
        public void DrainHPTouchTest()
        {
            // Setup
            Monster actor = new Monster
            {
                HPMax = 1000,
                HP = 1,
                Hits = 10,
                Accuracy = 99,
                Strength = 10
            };
            actor.AttackEffects.Add("Drain HP");

            Monster target = new Monster
            {
                HPMax = 1600,
                HP = 1600
            };

            // Test normal logic
            AttackManager.AttackMonster(actor, target);
            Assert.IsTrue(actor.HP > 1);
            Assert.IsTrue(target.HP < target.HPMax);

            // Ensure an undead target reverses the drain
            actor.HP = actor.HPMax;
            target.HP = 1;
            target.Families.Add(MonsterFamily.Undead);

            AttackManager.AttackMonster(actor, target);
            Assert.IsTrue(actor.HP < actor.HPMax);
            Assert.IsTrue(target.HP > 1);
            target.Families.Clear();

            // Ensure that the drain effect alone deals 1/16th max HP damage per hit
            target.HP = target.HPMax;
            AttackResult atkRes = AttackManager.AttackMonster(actor, target);
            int hits = int.Parse(atkRes.HitsMessage.Split('x')[0]);
            Assert.AreNotEqual(1600 - (hits * 100), target.HP);

            actor.Strength = 0;
            target.HP = target.HPMax;
            atkRes = AttackManager.AttackMonster(actor, target);
            hits = int.Parse(atkRes.HitsMessage.Split('x')[0]);
            Assert.AreEqual(1600 - (hits * 100), target.HP);
        }


        [TestMethod]
        public void DrainMPTouchTest()
        {
            // Setup
            Monster actor = new Monster
            {
                MPMax = 1000,
                MP = 1,
                Hits = 10,
                Accuracy = 99,
                Strength = 0
            };
            actor.AttackEffects.Add("Drain MP");

            Monster target = new Monster
            {
                MPMax = 1600,
                MP = 1600
            };

            // Test normal logic
            AttackManager.AttackMonster(actor, target);
            Assert.IsTrue(actor.MP > 1);
            Assert.IsTrue(target.MP < target.MPMax);

            // Ensure an undead target reverses the drain
            actor.MP = actor.MPMax;
            target.MP = 1;
            target.Families.Add(MonsterFamily.Undead);

            AttackManager.AttackMonster(actor, target);
            Assert.IsTrue(actor.MP < actor.MPMax);
            Assert.IsTrue(target.MP > 1);
            target.Families.Clear();

            // Ensure that the drain effect alone deals 1/16th max MP damage per hit
            target.MP = target.MPMax;
            AttackResult atkRes = AttackManager.AttackMonster(actor, target);
            int hits = int.Parse(atkRes.HitsMessage.Split('x')[0]);
            Assert.AreEqual(1600 - (hits * 100), target.MP);
        }

        [TestMethod]
        public void PermStatusTouchTest()
        {
            // Setup
            Monster actor = new Monster();
            Monster target = new Monster();
        }

        [TestMethod]
        public void TempStatusTouchTest()
        {
            // Setup
            Monster actor = new Monster();
            Monster target = new Monster();
        }

        [TestMethod]
        public void ExpectedDamageAndCritsTest()
        {
            // Setup
            Monster actor = new Monster
            {
                Hits = 1,
                Accuracy = 99,
                Strength = 10
            };

            Monster target = new Monster();

            // Test expected damage, crit expectancy
            int critThreshold = 75; // 7.5% (5% expected)
            for (int i = 0; i < 1000; i++)
            {
                AttackResult atkRes = AttackManager.AttackMonster(actor, target);
                // Ignore misses
                if (!String.Equals(atkRes.DamageMessage, "Miss"))
                {
                    // Damage range is (str...str*2+str);
                    // Extract the damage value from the damage message ("xxx DMG");
                    int dmg = int.Parse(atkRes.DamageMessage.Split()[0]);
                    Assert.IsTrue(dmg >= actor.Strength);
                    Assert.IsTrue(dmg <= actor.Strength * 3);
                    if (atkRes.Results.Contains("Critical Hit!"))
                        critThreshold--;
                }
            }
            // Ensure some crits happened, but not too many
            Assert.AreNotEqual(75, critThreshold);
            Assert.IsTrue(critThreshold > 0);
        }

        [TestMethod]
        public void ExpectedHitsTest()
        {
            // Setup
            Monster actor = new Monster
            {
                Hits = 1,
                Accuracy = 99
            };

            Monster target = new Monster();

            // Test expected hit messages and miss threshold
            int missThreshold = 20; // 2% (1% expected)
            for (int i = 0; i < 1000; i++)
            {
                AttackResult res = AttackManager.AttackMonster(actor, target);
                if (String.Equals(res.DamageMessage, "Miss"))
                    missThreshold--;
                else
                    Assert.IsTrue(String.Equals(res.HitsMessage, "1xHit"));
            }

            // Ensure some misses happened, but not too many
            Assert.AreNotEqual(20, missThreshold);
            Assert.IsTrue(missThreshold > 0);
        }

        [TestMethod]
        public void AuraStacksTest()
        {
            // Setup
            Monster actor = new Monster();
            Monster target = new Monster();

            // Test Aura Buff damage bonus
            foreach (int i in Enum.GetValues(typeof(MonsterFamily)))
            {
                MonsterFamily fam = (MonsterFamily)i;

                int minDmg = actor.Strength;
                int maxDmg = actor.Strength * 3;

                target.Families.Add(fam);
                testDamageRange(actor, target, minDmg, maxDmg);
                actor.AddBuff(Buff.Aura, i + 1);

                // NES Bug. Aura buff ignores undead, and therefore applies no bonus damage. 
                // Check this, flip the flag, then ensure it works properly if fixed
                Globals.BUG_FIXES = false;
                if (!Globals.BUG_FIXES && fam == MonsterFamily.Undead)
                {
                    testDamageRange(actor, target, minDmg, maxDmg);
                    Globals.BUG_FIXES = true;
                }

                testDamageRange(actor, target, minDmg + 20, maxDmg + 60);
                target.Families.Remove(fam);
                testDamageRange(actor, target, minDmg, maxDmg);
                actor.RemoveBuff(Buff.Aura);
            }

            // Ensure Aura bonus damage doesn't stack
            target.Families.Add(MonsterFamily.Air);
            target.Families.Add(MonsterFamily.Dragon);
            target.Families.Add(MonsterFamily.Earth);
            testDamageRange(actor, target, actor.Strength, actor.Strength * 3);
            actor.AddBuff(Buff.Aura, 1);
            testDamageRange(actor, target, actor.Strength + 20, (actor.Strength * 3) + 60);
            actor.AddBuff(Buff.Aura, 7);
            testDamageRange(actor, target, actor.Strength + 20, (actor.Strength * 3) + 60);
            target.Families.Clear();
            actor.RemoveBuff(Buff.Aura);
        }

        /////////////
        // Helpers //
        /////////////

        [TestMethod]
        private void testDamageRange(Monster actor, Monster target, int min, int max)
        {
            for (int i = 0; i < 100; i++)
            {
                AttackResult atkRes = AttackManager.AttackMonster(actor, target);
                // Ignore misses
                if (!String.Equals(atkRes.DamageMessage, "Miss"))
                {
                    // Extract the damage value from the damage message ("xxx DMG");
                    int dmg = int.Parse(atkRes.DamageMessage.Split()[0]);
                    Assert.IsTrue(dmg >= min);
                    Assert.IsTrue(dmg <= max);
                }
            }
        }
    }
}
