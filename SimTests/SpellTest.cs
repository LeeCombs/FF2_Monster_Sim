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

        /// <summary>
        /// Returns whether a status spell is resisted (ineffective) against a monster who resists its element
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        private bool StatusIsResisted(Spell spell)
        {
            // Create a monster that resists all elements
            Monster monster = new Monster();
            foreach (Element e in Enum.GetValues(typeof(Element))) monster.Resistances.Add(e);
            monster.Resistances.Remove(Element.None);  // Monsters cannot resist the fabled None element

            // Cast the spell a ton of times and make sure the monster isn't inflicted with the status
            if (String.Equals(spell.Effect.ToUpper(), "TEMPSTATUS"))
            {
                TempStatus tempStatus = (TempStatus)Enum.Parse(typeof(TempStatus), (string)spell.Status);
                Assert.IsFalse(monster.HasTempStatus(tempStatus));
                for (int i = 0; i < 10; i++) SpellManager.CastSpell(monster, monster, spell, 16);
                return !monster.HasTempStatus(tempStatus); // Opposite. If inflicted, it didn't resist
            }
            if (String.Equals(spell.Effect.ToUpper(), "PERMSTATUS"))
            {
                PermStatus permStatus = (PermStatus)Enum.Parse(typeof(PermStatus), (string)spell.Status);
                Assert.IsFalse(monster.HasPermStatus(permStatus));
                for (int i = 0; i < 10; i++) SpellManager.CastSpell(monster, monster, spell, 16);
                return !monster.HasPermStatus(permStatus); // Opposite. If inflicted, it didn't resist
            }

            Debug.WriteLine("StatusIsResisted spell fell through: " + spell.Name);
            return false;
        }

        /// <summary>
        /// Returns whether a status spell automatically hits a monster based purely on elemental weakness.
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        private bool StatusAutoHits(Spell spell)
        {
            // Create a monster that is weak to all elements
            Monster monster = new Monster();
            foreach (Element e in Enum.GetValues(typeof(Element))) monster.Weaknesses.Add(e);
            monster.Weaknesses.Remove(Element.None);  // Monsters cannot be weak to the fabled None element

            // Set spell's accuracy to 0, which normally would not allow it to hit, and make sure it hits on a single cast
            spell.Accuracy = 0;
            Assert.AreEqual(0, spell.Accuracy);

            if (String.Equals(spell.Effect.ToUpper(), "TEMPSTATUS"))
            {
                TempStatus tempStatus = (TempStatus)Enum.Parse(typeof(TempStatus), (string)spell.Status);
                Assert.IsFalse(monster.HasTempStatus(tempStatus));
                SpellManager.CastSpell(monster, monster, spell, 1);
                return monster.HasTempStatus(tempStatus);
            }
            if (String.Equals(spell.Effect.ToUpper(), "PERMSTATUS"))
            {
                PermStatus permStatus = (PermStatus)Enum.Parse(typeof(PermStatus), (string)spell.Status);
                Assert.IsFalse(monster.HasPermStatus(permStatus));
                SpellManager.CastSpell(monster, monster, spell, 1);
                return monster.HasPermStatus(permStatus);
            }

            Debug.WriteLine("StatusAutoHits spell fell through: " + spell.Name);
            return false;
        }

        //////////////////////////
        // Specific Spell Tests //
        //////////////////////////

        [TestMethod]
        public void EsunaHEALSpellTest()
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
        public void BasunaPEEPSpellTest()
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
        public void CureSpellTest()
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

        ///////////////////
        // Status Spells //
        ///////////////////

        [TestMethod]
        public void TempStatusSpellsTest()
        {
            string[] tempStatusSpells = new String[] { "SLEP", "STON", "STOP", "CHRM", "MINI", "MUTE", "Wink" };

            foreach (string spellName in tempStatusSpells)
            {
                // Every spell has an element and get be absorbed/resisted/weaked
                Spell spell = SpellManager.GetSpellByName(spellName);
                Assert.IsTrue(IsAbsorbed(spell));
                Assert.IsTrue(StatusIsResisted(spell));
                Assert.IsTrue(StatusAutoHits(spell));
            }

            // Special case. Blast_2 is the only non-elemental status spell.
            Spell blastSpell = SpellManager.GetSpellByName("Blast_2");
            Assert.IsFalse(IsAbsorbed(blastSpell));
            Assert.IsFalse(StatusIsResisted(blastSpell));
            Assert.IsFalse(StatusAutoHits(blastSpell));
        }

        [TestMethod]
        public void PermStatusSpellsTest()
        {
            string[] permStatusSpells = new String[] { "BLND", "CURS", "TOAD", "BRAK", "XZON", "FOG", "EXIT", "Breath", "Glare" };

            // Make sure every spell can effect it's status on a target and followed elemental rules
            foreach (string spellName in permStatusSpells)
            {
                Spell spell = SpellManager.GetSpellByName(spellName);
                Assert.IsTrue(IsAbsorbed(spell));
                Assert.IsTrue(StatusIsResisted(spell));
                Assert.IsTrue(StatusAutoHits(spell));
            }
        }

        ///////////////////
        // Damage Spells //
        ///////////////////

        // 1 - FIRE, BOLT, ICE, AERO, FLAR, HOLY, FIRE_S, ICE_S, BOLT_S, Mist
        // 2 - Quake, Tnad, Wave
        // 3 - Bow, Rock, Meteo
        // ULTM

        /////////////////
        // Buff Spells //
        /////////////////

        // BSRK, HAST, AURA, BARR, BLNK, SAFE, SHEL, WALL, Drink, Spirit, Intelligence

        ///////////////////
        // Debuff Spells //
        ///////////////////

        // DSPL, SLOW, FEAR

        ////////////////////
        // Special Spells //
        ////////////////////

        // DRAN, ASPL, ANTI, CHNG, Blast

    }
}
