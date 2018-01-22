using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FF2_Monster_Sim;

namespace SimTests
{
    [TestClass]
    public class SpellManagerTest
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
        public void SpellResultTest()
        {
            // Ensure spellResults act as expected and things stay within bounds
            SpellResult spellRes = new SpellResult();
        }

        [TestMethod]
        public void GetSpellByNameTest()
        {
            // Test a known valid input and that it ignores case
            Assert.IsNotNull(SpellManager.GetSpellByName("FIRE"));
            Assert.IsNotNull(SpellManager.GetSpellByName("fire"));
            Assert.IsNotNull(SpellManager.GetSpellByName("Fire"));
            Assert.IsNotNull(SpellManager.GetSpellByName("FiRe"));
            Assert.IsNotNull(SpellManager.GetSpellByName("firE"));

            // Test a known invalid input
            Assert.ThrowsException<Exception>(() => SpellManager.GetSpellByName("WetNoodlez"));

            // Test that exceptions are thrown for invalid input
            Assert.ThrowsException<ArgumentException>(() => SpellManager.GetSpellByName(null));
            Assert.ThrowsException<ArgumentException>(() => SpellManager.GetSpellByName(""));
        }

        [TestMethod]
        public void CastSpellTest()
        {
            // Setup
            Monster actor = new Monster();
            Monster target = new Monster();
            Spell spell = SpellManager.GetSpellByName("FIRE");

            // Test valid input
            Assert.IsNotNull(SpellManager.CastSpell(actor, target, spell, 1));

            // Test invalid inputs
            Assert.ThrowsException<ArgumentNullException>(() => SpellManager.CastSpell(null, target, spell, 1));
            Assert.ThrowsException<ArgumentNullException>(() => SpellManager.CastSpell(null, null, spell, 1));
            Assert.ThrowsException<ArgumentNullException>(() => SpellManager.CastSpell(actor, null, spell, 1));
            Assert.ThrowsException<ArgumentNullException>(() => SpellManager.CastSpell(actor, null, null, 1));
            Assert.ThrowsException<ArgumentNullException>(() => SpellManager.CastSpell(actor, target, null, 1));
            Assert.ThrowsException<ArgumentNullException>(() => SpellManager.CastSpell(null, target, null, 1));
            Assert.ThrowsException<ArgumentNullException>(() => SpellManager.CastSpell(null, null, null, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => SpellManager.CastSpell(actor, target, spell, -1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => SpellManager.CastSpell(actor, target, spell, 17));

            spell.Effect = "Banana";
            Assert.ThrowsException<Exception>(() => SpellManager.CastSpell(actor, target, spell, 1));

            // TODO: Determine which cases should be tested here that are not already tested
            // through the individual spell tests
        }

        [TestMethod]
        public void GetSpellNamesTest()
        {
            // Test that this returns a HashSet of spell names
            Assert.IsNotNull(SpellManager.GetSpellNames());
            Assert.AreNotEqual(0, SpellManager.GetSpellNames().Count);
        }
        
        [TestMethod]
        public void SpellDataTest()
        {
            foreach (String name in SpellManager.GetSpellNames())
            {
                Spell spell = SpellManager.GetSpellByName(name);
                Assert.IsNotNull(spell);
                Assert.IsFalse(String.IsNullOrEmpty(spell.Name));
                Assert.IsNotNull(spell.SpellType);
                Assert.IsFalse(String.IsNullOrEmpty(spell.Effect));
                
                Assert.IsTrue(Utils.NumIsWithinRange(spell.Power, 0, 255));
                Assert.IsTrue(spell.Accuracy >= 0);
                Assert.IsNotNull(spell.Status);
                Assert.IsNotNull(spell.Element);
                Assert.IsNotNull(spell.SuccessMessage);

                // Necessary?
                Assert.IsTrue(spell.Price >= 0);
                Assert.IsTrue(spell.Value >= 0);

                // Cast the spell and ensure nothing pops up
                Monster mon = new Monster();
                mon.HPMax = mon.HP = mon.MPMax = mon.MP = 1;
                SpellManager.CastSpell(mon, mon, spell, 1);
            }
        }
    }
}
