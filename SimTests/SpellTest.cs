using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FF2_Monster_Sim;

namespace SimTests
{
    [TestClass]
    public class SpellTest
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            MonsterManager.Initialize();
            MonsterManager.LoadContent();
            SpellManager.Initialize();
            SpellManager.LoadContent();
        }

        ///////////////////
        // General Tests //
        ///////////////////

        [TestMethod]
        public void SpellCreationTest()
        {
            // Ensure that a known spell can be created and returned
            Spell spell = SpellManager.GetSpellByName("FIRE");
            Assert.AreEqual("FIRE", spell.Name);
            Assert.AreEqual(SpellType.Black, spell.SpellType);
            Assert.AreEqual("Damage", spell.Effect);
            Assert.AreEqual(10, spell.Power);
            Assert.AreEqual(0, spell.Accuracy);
            Assert.AreEqual("", spell.Status);
            Assert.AreEqual(Element.Fire, spell.Element);
            Assert.AreEqual(400, spell.Price);
            Assert.AreEqual(100, spell.Value);
        }


        //////////////////
        // Helper Tests //
        //////////////////
        
        /// <summary>
        /// Returns whether a spell is able to be absorbed by a monster or not
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        private bool IsAbsorbed(Spell spell)
        {
            // Create a monster that absorbs all elements
            Monster monster = new Monster();
            foreach (Element e in Enum.GetValues(typeof(Element))) monster.Absorbs.Add(e);
            monster.Absorbs.Remove(Element.None);  // Monsters cannot absorb the fabled None element

            // For 0 accuracy spells, just being absorbent is good enough
            if (spell.Accuracy == 0 && monster.IsAbsorbantTo(spell.Element)) return true;

            // If it can hit the monster, ensure sure it heals them
            monster.HPMax = 1000;
            monster.HP = 1;
            SpellManager.CastSpell(monster, monster, spell, 16);
            SpellManager.CastSpell(monster, monster, spell, 16);
            SpellManager.CastSpell(monster, monster, spell, 16);
            SpellManager.CastSpell(monster, monster, spell, 16);
            SpellManager.CastSpell(monster, monster, spell, 16);
            return monster.HP > 1;
        }

        private bool ResistsStatus(Spell spell)
        {
            // If a monster resists a status spell's element, it cannot be inflicted by the status

            // Create a monster that resists all elements
            Monster monster = new Monster();
            foreach (Element e in Enum.GetValues(typeof(Element))) monster.Resistances.Add(e);
            monster.Resistances.Remove(Element.None);  // Monsters cannot resist the fabled None element

            if (String.Equals(spell.Effect.ToUpper(), "TEMPSTATUS"))
            {
                // Cast the spell a bunch and make sure the monster isn't inflicted with the status
                TempStatus tempStatus = (TempStatus)Enum.Parse(typeof(TempStatus), (string)spell.Status);
                Assert.IsFalse(monster.HasTempStatus(tempStatus));
                for (int i = 0; i < 10; i++) SpellManager.CastSpell(monster, monster, spell, 16);
                return !monster.HasTempStatus(tempStatus); // Opposite. If inflicted, it didn't resist
            }
            if (String.Equals(spell.Effect.ToUpper(), "PERMSTATUS"))
            {
                // Cast the spell a bunch and make sure the monster isn't inflicted with the status
                PermStatus permStatus = (PermStatus)Enum.Parse(typeof(PermStatus), (string)spell.Status);
                Assert.IsFalse(monster.HasPermStatus(permStatus));
                for (int i = 0; i < 10; i++) SpellManager.CastSpell(monster, monster, spell, 16);
                return !monster.HasPermStatus(permStatus); // Opposite. If inflicted, it didn't resist
            }

            Debug.WriteLine("ResistsStatus spell fell through: " + spell.Name);
            return false;
        }


        //////////////////////////
        // Specific Spell Tests //
        //////////////////////////

        [TestMethod]
        public void EsunaHEALTest()
        {
            // This is HEAL/Esuna
            Spell spell = SpellManager.GetSpellByName("HEAL");
            Monster monster = new Monster();
            PermStatus[] permStatOrder = { PermStatus.Darkness, PermStatus.Poison, PermStatus.Curse, PermStatus.Amnesia, PermStatus.Toad, PermStatus.Stone, PermStatus.KO };

            // Ensure each status doesn't exist, add it, and test for it
            foreach (PermStatus stat in permStatOrder)
            {
                Assert.IsFalse(monster.HasPermStatus(stat));
                monster.AddPermStatus(stat);
                Assert.IsTrue(monster.HasPermStatus(stat));
            }

            // Cast spells up to max spell level, and check that only the proper statuses are removed
            for (int i = 0; i < 16; i++)
            {
                SpellManager.CastSpell(monster, monster, spell, i + 1);
                for (int j = 0; j < (i < 7 ? i : 7); j++) Assert.IsFalse(monster.HasPermStatus(permStatOrder[j]));
                for (int k = i + 1; k < permStatOrder.Length; k++) Assert.IsTrue(monster.HasPermStatus(permStatOrder[k]));
            }
        }

        [TestMethod]
        public void BasunaPEEPTest()
        {
            // This is PEEP/Basuna
            Spell spell = SpellManager.GetSpellByName("PEEP");
            Monster monster = new Monster();
            TempStatus[] tempStatOrder = { TempStatus.Venom, TempStatus.Sleep, TempStatus.Mini, TempStatus.Mute, TempStatus.Paralysis, TempStatus.Confuse };

            // Ensure each status doesn't exist, add it, and test for it
            foreach (TempStatus stat in tempStatOrder)
            {
                Assert.IsFalse(monster.HasTempStatus(stat));
                monster.AddTempStatus(stat);
                Assert.IsTrue(monster.HasTempStatus(stat));
            }

            // Cast spells up to max spell level, and check that only the proper statuses are removed
            for (int i = 0; i < 16; i++)
            {
                SpellManager.CastSpell(monster, monster, spell, i + 1);
                // Level 1 removes both Venom and Sleep
                Assert.IsFalse(monster.HasTempStatus(tempStatOrder[0]));
                for (int j = 1; j < (i < 5 ? i : 5); j++) Assert.IsFalse(monster.HasTempStatus(tempStatOrder[j]));
                for (int k = i + 1; k < tempStatOrder.Length; k++) Assert.IsTrue(monster.HasTempStatus(tempStatOrder[k]));
            }
        }

        [TestMethod]
        public void CureTest()
        {
            // Set up the monster to be healed
            Monster monster = new Monster();
            monster.HPMax = 1000;
            monster.HP = 1;
            Assert.AreEqual(1000, monster.HPMax);
            Assert.AreEqual(1, monster.HP);

            // Heal the monster, ensure it's effective and stays within bounds
            Spell spell = SpellManager.GetSpellByName("CURE");
            SpellManager.CastSpell(monster, monster, spell, 1);
            Assert.IsTrue(monster.HP >= 1);
            Assert.IsTrue(monster.HP <= monster.HPMax);

            // Try to over-heal the target, ensure HP doesn't exceed HPMax
            for (int i = 0; i < 50; i++) SpellManager.CastSpell(monster, monster, spell, 16);
            Assert.IsTrue(monster.HP > 1);
            Assert.IsTrue(monster.HP <= monster.HPMax);

            // TODO: Damage vs Undead
        }

        ////////////////////////
        // Temp Status Spells //
        ////////////////////////

        [TestMethod]
        public void SLEPTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("SLEP");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Sleep
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void STONTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("STON");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Paralysis
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void STOPTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("STOP");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Paralysis
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void CHRMTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("CHRM");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Confusion
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void MINITest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("MINI");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Mini (KO)
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void MUTETest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("MUTE");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: MUTE
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void WinkTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("Wink");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Confusion
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void Blast2Test()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("Blast_2");
            // No element, cannot be resisted
            Assert.IsFalse(IsAbsorbed(spell));
            Assert.IsFalse(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Paralysis
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        ////////////////////////
        // Perm Status Spells //
        ////////////////////////

        [TestMethod]
        public void BLNDTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("BLND");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Darkness
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void CURSTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("CURS");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Curse
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void TOADTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("TOAD");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Toad (KO)
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void BRAKTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("BRAK");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Stone (KO)
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void XZONTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("XZON");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: KO
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void FOGTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("FOG");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Amnesia
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void EXITTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("EXIT");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: KO
            // TODO: Ensure status auto-hits enemies who are weak to its element

        }

        [TestMethod]
        public void BreathTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("Breath");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Stone (KO)
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void GlareTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("Glare");
            Assert.IsTrue(IsAbsorbed(spell));
            Assert.IsTrue(ResistsStatus(spell));

            // Ensure the spell has only intended effect: Stone (KO)
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }
    }
}
