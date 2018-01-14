using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FF2_Monster_Sim;

namespace SimTests
{
    [TestClass]
    public class MonsterManagerTest
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            MonsterManager.Initialize();
            MonsterManager.LoadContent();
            SpellManager.Initialize();
            SpellManager.LoadContent();
        }

        [TestMethod]
        public void GetMonsterByNameTest()
        {
            // Test invalid inputs
            Assert.ThrowsException<ArgumentException>(() => MonsterManager.GetMonsterByName(null));
            Assert.ThrowsException<ArgumentException>(() => MonsterManager.GetMonsterByName(""));
            Assert.ThrowsException<Exception>(() => MonsterManager.GetMonsterByName("INVALIDNAMEDONTNAMEAMONTSERTHIS"));

            // Test that monsters properly load with known stats from data
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            Assert.AreEqual(6, monster.HP);
            Assert.AreEqual(6, monster.HPMax);
            Assert.AreEqual(0, monster.MP);
            Assert.AreEqual(0, monster.MPMax);
            Assert.AreEqual(4, monster.Strength);
            Assert.AreEqual(0, monster.Defense);
            Assert.AreEqual(180, monster.Fear);
            Assert.AreEqual(1, monster.Hits);
            Assert.AreEqual(0, monster.Blocks);
            Assert.AreEqual(1, monster.MagicBlocks);
            Assert.AreEqual(60, monster.Accuracy);
            Assert.AreEqual(0, monster.Evasion);
            Assert.AreEqual(50, monster.MagicEvasion);

            HashSet<MonsterFamily> familySet = new HashSet<MonsterFamily>() { MonsterFamily.Earth };
            Assert.IsTrue(monster.Families.SetEquals(familySet));
            HashSet<Element> resistSet = new HashSet<Element>() { Element.Mind, Element.Body };
            Assert.IsTrue(monster.Resistances.SetEquals(resistSet));
            HashSet<Element> weakSet = new HashSet<Element>() { };
            Assert.IsTrue(monster.Weaknesses.SetEquals(weakSet));
            HashSet<Element> absorbSet = new HashSet<Element>() { };
            Assert.IsTrue(monster.Absorbs.SetEquals(absorbSet));
            HashSet<string> atkEffectList = new HashSet<string>() { };
            Assert.IsTrue(monster.AttackEffects.SetEquals(atkEffectList));

            // Create an all-attack actionList to compare against
            MonsterAction attack = new MonsterAction("Attack", 0, 0, 0, "SingleTarget");
            List<MonsterAction> actionList = new List<MonsterAction>();
            for (int i = 0; i < 8; i++)
                actionList.Add(attack);
            Assert.IsTrue(monster.ActionList.SequenceEqual(actionList));

            // gildrops
            // itemdrops
        }

        [TestMethod]
        public void MonsterDataTest()
        {
            string[] validSizes = new string[] { "SMALL", "MEDIUM", "TALL", "LARGE" };

            // Ensure each monster in the data has the basics set up
            for (int i = 0; i < MonsterManager.MonsterNames.Count; i++)
            {
                string name = MonsterManager.MonsterNames[i];
                Monster monster = MonsterManager.GetMonsterByName(name);
                Assert.IsNotNull(monster.Name);

                // TODO: Ensure graphics exist for each monster

                // Size
                Assert.IsTrue(validSizes.Contains(monster.size.ToUpper()));

                // Stat ranges
                Assert.IsTrue(Utils.NumIsWithinRange(monster.HP, 0, ushort.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.HPMax, 0, ushort.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.MP, 0, ushort.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.MPMax, 0, ushort.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.Strength, 0, byte.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.Defense, 0, byte.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.Blocks, 0, byte.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.Evasion, 0, byte.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.MagicBlocks, 0, byte.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.Accuracy, 0, byte.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.MagicEvasion, 0, byte.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.Fear, 0, byte.MaxValue));
                Assert.IsTrue(Utils.NumIsWithinRange(monster.Hits, 0, 16));

                // Action list exists and is the right length
                Assert.IsNotNull(monster.ActionList);
                Assert.AreEqual(8, monster.ActionList.Count);

                // TODO: Ensure the actions in the ActionList are valid
                // Attack or valid spell name
                // Level within range
                // Power, Accuracy, MpConsumption >= 0
                // Target string is valid

                // Everything else is not null
                Assert.IsNotNull(monster.AttackEffects);
                // TODO: Ensure each attack effect is valid

                Assert.IsNotNull(monster.Families);
                // TODO: Ensure each family is valid

                Assert.IsNotNull(monster.Weaknesses);
                // TODO: Ensure each weakness is valid

                Assert.IsNotNull(monster.Resistances);
                // TODO: Ensure each resistance is valid

                Assert.IsNotNull(monster.Absorbs);
                // TODO: Ensure each absorb is valid
            }
        }

        [TestMethod]
        public void GenerateMonsterListTest()
        {
            // Test invalid sceneTypes
            Assert.ThrowsException<ArgumentException>(() => MonsterManager.GenerateMonsterList(null));
            Assert.ThrowsException<ArgumentException>(() => MonsterManager.GenerateMonsterList(""));
            Assert.ThrowsException<ArgumentException>(() => MonsterManager.GenerateMonsterList("D"));
            Assert.ThrowsException<ArgumentException>(() => MonsterManager.GenerateMonsterList("InvalidInput"));

            // Test valid input
            MonsterManager.GenerateMonsterList("a");
            MonsterManager.GenerateMonsterList("A");
            MonsterManager.GenerateMonsterList("b");
            MonsterManager.GenerateMonsterList("B");
            MonsterManager.GenerateMonsterList("c");
            MonsterManager.GenerateMonsterList("C");

            // Ensure that the monster sizes returned match the scene type
            for (int i = 0; i < 1000; i++)
            {
                foreach (string name in MonsterManager.GenerateMonsterList("A"))
                    Assert.IsTrue(String.Equals(MonsterManager.GetMonsterByName(name).size.ToUpper(), "SMALL"));

                foreach (string name in MonsterManager.GenerateMonsterList("B"))
                    Assert.IsTrue(
                        String.Equals(MonsterManager.GetMonsterByName(name).size.ToUpper(), "MEDIUM") ||
                        String.Equals(MonsterManager.GetMonsterByName(name).size.ToUpper(), "TALL")
                        );

                foreach (string name in MonsterManager.GenerateMonsterList("C"))
                    Assert.IsTrue(String.Equals(MonsterManager.GetMonsterByName(name).size.ToUpper(), "LARGE"));
            }

            // Ensure A always gives 8, B gives 3-6, and C gives 1
            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(8, MonsterManager.GenerateMonsterList("A").Count);
                Assert.IsTrue(Utils.NumIsWithinRange(MonsterManager.GenerateMonsterList("B").Count, 3, 6));
                Assert.AreEqual(1, MonsterManager.GenerateMonsterList("C").Count);
            }
        }
    }
}
