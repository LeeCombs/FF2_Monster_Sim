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

            //// Test valid input
            Assert.IsNotNull(AttackManager.AttackMonster(actor, target));

            //// Test invalid inputs
            Assert.ThrowsException<ArgumentNullException>(() => AttackManager.AttackMonster(null, target));
            Assert.ThrowsException<ArgumentNullException>(() => AttackManager.AttackMonster(actor, null));
            Assert.ThrowsException<ArgumentNullException>(() => AttackManager.AttackMonster(null, null));

            //// Test expected hit messages and miss threshold
            int missThreshold = 20; // 2% (1% expected)
            for (int i = 0; i < 1000; i++)
            {
                AttackResult res = AttackManager.AttackMonster(actor, target);
                if (String.Equals(res.HitsMessage, "Miss"))
                    missThreshold--;
                else
                    Assert.IsTrue(String.Equals(res.HitsMessage, "0xHit") || String.Equals(res.HitsMessage, "1xHit"));
            }
            // Ensure some misses happened, but not too many
            Assert.AreNotEqual(20, missThreshold);
            Assert.IsTrue(missThreshold > 0);

            //// Test expected damage, crit expectancy
            int critThreshold = 75; // 7.5% (5% expected)
            for (int i = 0; i < 1000; i++)
            {
                AttackResult atkRes = AttackManager.AttackMonster(actor, target);
                // Ignore misses
                if (!String.Equals(atkRes.HitsMessage, "Miss"))
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

            //// TODO: Test aura buff effects
            // Iterate through MonsterFamily enum
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
                if (!String.Equals(atkRes.HitsMessage, "Miss"))
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
