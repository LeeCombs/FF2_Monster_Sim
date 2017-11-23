using System;
using System.Collections.Generic;
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
            Assert.AreEqual("Fire", spell.Element);
            Assert.AreEqual(400, spell.Price);
            Assert.AreEqual(100, spell.Value);
        }


        //////////////////
        // Helper Tests //
        //////////////////

        private bool IsAbsorbed(Spell spell)
        {
            Monster monster = new Monster();
            // TODO: This
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
                Assert.AreEqual(false, monster.HasPermStatus(stat));
                monster.AddPermStatus(stat);
                Assert.AreEqual(true, monster.HasPermStatus(stat));
            }

            // Cast spells up to max spell level, and check that only the proper statuses are removed
            for (int i = 0; i < 16; i++)
            {
                SpellManager.CastSpell(monster, monster, spell, i + 1);
                for (int j = 0; j < (i < 7 ? i : 7); j++) Assert.AreEqual(false, monster.HasPermStatus(permStatOrder[j]));
                for (int k = i + 1; k < permStatOrder.Length; k++) Assert.AreEqual(true, monster.HasPermStatus(permStatOrder[k]));
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
                Assert.AreEqual(false, monster.HasTempStatus(stat));
                monster.AddTempStatus(stat);
                Assert.AreEqual(true, monster.HasTempStatus(stat));
            }

            // Cast spells up to max spell level, and check that only the proper statuses are removed
            for (int i = 0; i < 16; i++)
            {
                SpellManager.CastSpell(monster, monster, spell, i + 1);
                // Level 1 removes both Venom and Sleep
                Assert.AreEqual(false, monster.HasTempStatus(tempStatOrder[0]));
                for (int j = 1; j < (i < 5 ? i : 5); j++) Assert.AreEqual(false, monster.HasTempStatus(tempStatOrder[j]));
                for (int k = i + 1; k < tempStatOrder.Length; k++) Assert.AreEqual(true, monster.HasTempStatus(tempStatOrder[k]));
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
            Assert.AreEqual(true, monster.HP >= 1);
            Assert.AreEqual(true, monster.HP <= monster.HPMax);

            // Try to over-heal the target, ensure HP doesn't exceed HPMax
            for (int i = 0; i < 50; i++) SpellManager.CastSpell(monster, monster, spell, 16);
            Assert.AreEqual(true, monster.HP > 1);
            Assert.AreEqual(true, monster.HP <= monster.HPMax);

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

            // Ensure the spell has only intended effect: Sleep
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void STONTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("STON");

            // Ensure the spell has only intended effect: Paralysis
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void STOPTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("STOP");

            // Ensure the spell has only intended effect: Paralysis
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void CHRMTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("CHRM");

            // Ensure the spell has only intended effect: Confusion
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void MINITest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("MINI");

            // Ensure the spell has only intended effect: Mini (KO)
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void MUTETest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("MUTE");

            // Ensure the spell has only intended effect: MUTE
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void WinkTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("Wink");

            // Ensure the spell has only intended effect: Confusion
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void Blast2Test()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("Blast_2");

            // Ensure the spell has only intended effect: Paralysis
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
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

            // Ensure the spell has only intended effect: Darkness
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void CURSTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("CURS");

            // Ensure the spell has only intended effect: Curse
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void TOADTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("TOAD");

            // Ensure the spell has only intended effect: Toad (KO)
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void BRAKTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("BRAK");

            // Ensure the spell has only intended effect: Stone (KO)
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void XZONTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("XZON");

            // Ensure the spell has only intended effect: KO
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void FOGTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("FOG");

            // Ensure the spell has only intended effect: Amnesia
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void EXITTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("EXIT");

            // Ensure the spell has only intended effect: KO
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element

        }

        [TestMethod]
        public void BreathTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("Breath");

            // Ensure the spell has only intended effect: Stone (KO)
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }

        [TestMethod]
        public void GlareTest()
        {
            // Ensure normal base stats
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("Glare");

            // Ensure the spell has only intended effect: Stone (KO)
            // TODO: Ensure status doesn't work against enemis who resistant its element
            // TODO: Ensure spell heals enemies who absorb its element
            // TODO: Ensure status auto-hits enemies who are weak to its element
        }
    }
}
