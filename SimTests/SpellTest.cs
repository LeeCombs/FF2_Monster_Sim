using System;
using System.Diagnostics;
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
            Assert.AreEqual(Element.Fire, spell.Element);
            Assert.AreEqual(400, spell.Price);
            Assert.AreEqual(100, spell.Value);
            Assert.AreEqual("", spell.SuccessMessage);
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

        ///////////////////
        // Status Spells //
        ///////////////////

        // Temp: SLEP, STON, STOP, CHRM, MUTE, MINI, Wink, Blast_2
        // Perm: BLND, CURS, FOG, TOAD, BRAK, WARP, EXIT, Breath, Glare, DETH

        [TestMethod]
        public void TempStatusSpellsTest()
        {
            string[] tempStatusSpells = new String[] { "SLEP", "STON", "STOP", "CHRM", "MUTE", "MINI", "Wink" };

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
            string[] permStatusSpells = { "DETH", "BLND", "CURS", "FOG", "TOAD", "BRAK", "WARP", "EXIT", "Breath", "Glare" };

            // Make sure every spell can effect it's status on a target and followed elemental rules
            foreach (string spellName in permStatusSpells)
            {
                Spell spell = SpellManager.GetSpellByName(spellName);
                Assert.IsTrue(IsAbsorbed(spell));
                Assert.IsTrue(StatusIsResisted(spell));
                Assert.IsTrue(StatusAutoHits(spell));
            }
        }

        [TestMethod]
        public void KOStatusSpellsTest()
        {
            // Setup
            string[] KOStatusSpells = { "DETH", "Glare", "MINI", "TOAD", "BRAK", "WARP", "EXIT", "Breath" };

            // Cast each spell and ensure the target monster is killed
            foreach (string spellName in KOStatusSpells)
            {
                // Setup
                Monster monster = new Monster();
                Spell spell = SpellManager.GetSpellByName(spellName);
                spell.Accuracy = 255;
                // TODO: Cast spell, check for KO/Death/Whatever
            }
        }

        ///////////////////
        // Damage Spells //
        ///////////////////

        // 1 - FIRE, BOLT, ICE, AERO, FLAR, HOLY, FIRE_S, ICE_S, BOLT_S, Mist
        // 2 - Quake, Tnad, Wave
        // 3 - Bow, Rock, Meteo
        // ULTM

        [TestMethod]
        public void DamageSpellTest()
        {
            // Setup
            Monster monster = new Monster();
            monster.HP = monster.HPMax = 50000;

            // TODO: What do I want to test? All damage spells achieve damage?
        }

        /////////////////
        // Buff Spells //
        /////////////////

        // BSRK, HAST, AURA, BARR, BLNK, SAFE, SHEL, WALL, Drink, Spirit, Intelligence

        [TestMethod]
        public void BuffSpellTest()
        {
            // Set up
            Monster monster = new Monster();
            string[] buffSpells = { "BSRK", "HAST", "AURA", "BARR", "BLNK", "SAFE", "SHEL", "WALL", "Drink" };
            // Ignore Spirit and Intelligence for now as they may be irrelevant
            
            // Cast each spell and ensure they add the right buff and return the right result message
            foreach (string name in buffSpells)
            {
                Spell spell = SpellManager.GetSpellByName(name);
                Buff buff = (Buff)Enum.Parse(typeof(Buff), spell.Status);
                spell.Accuracy = 255;

                SpellResult res = SpellManager.CastSpell(monster, monster, spell, 1);
                Assert.IsTrue(monster.GetBuffStacks(buff) > 0);
                monster.RemoveBuff(buff);
                Assert.AreEqual(0, monster.GetBuffStacks(buff));

                // AURA and BARR have unique messages and are covered in other tests
                if (spell.Name == "AURA" || spell.Name == "BARR") continue;
                Assert.AreEqual(spell.SuccessMessage, res.Results[0]);
            }
        }

        ///////////////////
        // Debuff Spells //
        ///////////////////

        // DSPL, SLOW, FEAR

        [TestMethod]
        public void DebuffSpellTest()
        {
            // Setup
            Monster monster = new Monster();
            string[] debuffSpells = { "SLOW", "FEAR" };
            // DSPL is busted and just doesn't work

            // Cast each spell and ensure they add the proper buff to the monster
            foreach (string name in debuffSpells)
            {
                Spell spell = SpellManager.GetSpellByName(name);
                spell.Accuracy = 255;
                Debuff debuff = (Debuff)Enum.Parse(typeof(Debuff), spell.Status);
                Assert.AreEqual(0, monster.GetDebuffStacks(debuff));
                SpellManager.CastSpell(monster, monster, spell, 16);
                Assert.AreNotEqual(0, monster.GetDebuffStacks(debuff));
                monster.RemoveDebuff(debuff);
                Assert.AreEqual(0, monster.GetDebuffStacks(debuff));
            }

            // TODO: Ensure proper results messages
        }

        //////////////////////////
        // Specific Spell Tests //
        //////////////////////////

        [TestMethod]
        public void HEALTest()
        {
            // Setup
            Spell spell = SpellManager.GetSpellByName("HEAL");
            Monster monster = new Monster();
            PermStatus[] permStatOrder = { PermStatus.Darkness, PermStatus.Poison, PermStatus.Curse, PermStatus.Amnesia, PermStatus.Toad, PermStatus.Stone, PermStatus.KO };
            Dictionary<PermStatus, string> expectedMessages = new Dictionary<PermStatus, string>();
            expectedMessages[PermStatus.Darkness] = "Can see";
            expectedMessages[PermStatus.Poison] = "Poison left";
            expectedMessages[PermStatus.Curse] = "Uncursed";
            expectedMessages[PermStatus.Amnesia] = "Remembers";
            expectedMessages[PermStatus.Toad] = "Regained form";
            expectedMessages[PermStatus.Stone] = "Normal body";
            expectedMessages[PermStatus.KO] = ""; // TODO: Find this message

            // Cast spells up to max spell level, and check that only the proper statuses are removed
            for (int i = 0; i < 16; i++)
            {
                foreach (PermStatus stat in permStatOrder) monster.AddPermStatus(stat);
                SpellManager.CastSpell(monster, monster, spell, i + 1);
                for (int j = 0; j < (i < 7 ? i : 7); j++) Assert.IsFalse(monster.HasPermStatus(permStatOrder[j]));
                for (int k = i + 1; k < permStatOrder.Length; k++) Assert.IsTrue(monster.HasPermStatus(permStatOrder[k]));
            }

            // Check that results messages return expected values
            foreach (PermStatus stat in permStatOrder)
            {
                monster.AddPermStatus(stat);
                SpellResult result = SpellManager.CastSpell(monster, monster, spell, 16);
                Assert.AreEqual(expectedMessages[stat], result.Results[0]);
            }

            // Ensure multiple results messages are returned if multiple statuses are cured
            PermStatus[] multStat = { PermStatus.Amnesia, PermStatus.Poison, PermStatus.Curse };
            foreach (PermStatus status in multStat) monster.AddPermStatus(status);
            SpellResult res = SpellManager.CastSpell(monster, monster, spell, 16);
            Assert.AreEqual(3, res.Results.Count);
            foreach (PermStatus status in multStat) Assert.IsTrue(res.Results.Contains(expectedMessages[status]));
        }

        [TestMethod]
        public void PEEPTest()
        {
            // Setup
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("PEEP");
            spell.Accuracy = 255;
            TempStatus[] tempStatOrder = { TempStatus.Venom, TempStatus.Sleep, TempStatus.Mini, TempStatus.Mute, TempStatus.Paralysis, TempStatus.Confuse };
            Dictionary<TempStatus, string> expectedMessages = new Dictionary<TempStatus, string>();
            expectedMessages[TempStatus.Venom] = "Devenomed";
            expectedMessages[TempStatus.Sleep] = "Scared"; // TODO: Verify this message
            expectedMessages[TempStatus.Paralysis] = "Can move";
            expectedMessages[TempStatus.Mute] = "Can speak";
            expectedMessages[TempStatus.Confuse] = "Normal";
            expectedMessages[TempStatus.Mini] = "Grew";

            // Cast spells up to max spell level, and check that only the proper statuses are removed
            for (int i = 0; i < 16; i++)
            {
                foreach (TempStatus stat in tempStatOrder) monster.AddTempStatus(stat);
                SpellManager.CastSpell(monster, monster, spell, i + 1);
                // Level 1 removes both Venom and Sleep
                Assert.IsFalse(monster.HasTempStatus(tempStatOrder[0]));
                for (int j = 1; j < (i < 5 ? i : 5); j++) Assert.IsFalse(monster.HasTempStatus(tempStatOrder[j]));
                for (int k = i + 1; k < tempStatOrder.Length; k++) Assert.IsTrue(monster.HasTempStatus(tempStatOrder[k]));
            }

            // Check that results messages return expected values
            foreach (TempStatus status in tempStatOrder)
            {
                monster.AddTempStatus(status);
                SpellResult result = SpellManager.CastSpell(monster, monster, spell, 16);
                Assert.AreEqual(expectedMessages[status], result.Results[0]);
            }

            // Ensure multiple results messages are returned if multiple statuses are cured
            TempStatus[] multStat = { TempStatus.Confuse, TempStatus.Sleep, TempStatus.Venom };
            foreach (TempStatus status in multStat) monster.AddTempStatus(status);
            SpellResult res = SpellManager.CastSpell(monster, monster, spell, 16);
            Assert.AreEqual(3, res.Results.Count);
            foreach (TempStatus status in multStat) Assert.IsTrue(res.Results.Contains(expectedMessages[status]));
        }

        [TestMethod]
        public void CURETest()
        {
            // Set up the monster to be healed
            Monster monster = new Monster();
            monster.HPMax = 1000;
            monster.HP = 1;
            Assert.AreEqual(1000, monster.HPMax);
            Assert.AreEqual(1, monster.HP);

            // HealHP the monster, ensure it's effective and stays within bounds
            Spell spell = SpellManager.GetSpellByName("CURE");
            SpellManager.CastSpell(monster, monster, spell, 1);
            Assert.IsTrue(monster.HP >= 1);
            Assert.IsTrue(monster.HP <= monster.HPMax);

            // Try to over-heal the target, ensure HP doesn't exceed HPMax
            for (int i = 0; i < 50; i++) SpellManager.CastSpell(monster, monster, spell, 16);
            Assert.IsTrue(monster.HP > 1);
            Assert.IsTrue(monster.HP <= monster.HPMax);

            // Damage vs Undead
            monster.Families.Add(MonsterFamily.Undead);
            monster.HP = monster.HPMax;
            Assert.AreEqual(1000, monster.HP);
            SpellManager.CastSpell(monster, monster, spell, 16);
            Assert.IsTrue(monster.HP < 1000);

            // TODO: Ensure proper messages are returned
        }

        [TestMethod]
        public void LIFETest()
        {
            // Setup
            Monster monster = new Monster();
            monster.HP = monster.HPMax = 100;
            Spell spell = SpellManager.GetSpellByName("LIFE");
            spell.Accuracy = 255;

            // Ensure LIFE has no effect normally
            SpellResult resFail = SpellManager.CastSpell(monster, monster, spell, 16);
            Assert.AreEqual("Ineffective", resFail.Results[0]);

            // Ensure LIFE fails if multi-casted, even against undead
            monster.Families.Add(MonsterFamily.Undead);
            Assert.AreEqual("Ineffective", SpellManager.CastSpell(monster, monster, spell, 16, true).Results[0]);
            Assert.AreEqual("Ineffective", SpellManager.CastSpell(monster, monster, spell, 16, true).Results[0]);
            Assert.AreEqual("Ineffective", SpellManager.CastSpell(monster, monster, spell, 16, true).Results[0]);
            Assert.AreEqual("Ineffective", SpellManager.CastSpell(monster, monster, spell, 16, true).Results[0]);
            Assert.AreEqual("Ineffective", SpellManager.CastSpell(monster, monster, spell, 16, true).Results[0]);
            Assert.AreEqual("Ineffective", SpellManager.CastSpell(monster, monster, spell, 16, true).Results[0]);
            Assert.IsFalse(monster.IsDead());

            // Ensure LIFE kills undead creatures
            monster.Name = "Spooky Ghost";
            SpellResult resUndead = SpellManager.CastSpell(monster, monster, spell, 16);
            Assert.AreEqual(monster.Name + " fell", resUndead.Results[0]);
            Assert.AreEqual("Collapsed", resUndead.Results[1]);
            Assert.IsTrue(monster.IsDead());

            // TODO: Ensure proper messages are returned
        }

        [TestMethod]
        public void BARRTest()
        {
            // Setup
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("BARR");
            spell.Accuracy = 255;
            string[] expectedMessages = { "Fire", "Soul", "Bolt", "Death", "Poison", "Critical Hit!", "Ice" };
            for (int i = 0; i < expectedMessages.Length; i++) expectedMessages[i] += " Df";

            // Test SpellResult messages and ensure they're there and in order
            // Iterate starting at index n and move towards the end, testing in that order
            for (int i = 0; i < (expectedMessages.Length - 1); i++)
            {
                SpellResult res = SpellManager.CastSpell(monster, monster, spell, i + 1);
                int startIndex = (expectedMessages.Length - 1) - i;
                int endIndex = (expectedMessages.Length - 1);
                for (int j = startIndex, ii = 0; j <= endIndex; j++, ii++)
                {
                    Assert.AreEqual(expectedMessages[j], res.Results[ii]);
                }
            }
        }

        [TestMethod]
        public void AURATest()
        {
            // Setup
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("AURA");
            spell.Accuracy = 255;
            string[] expectedMessages = { "Red", "Orange", "Blue", "Black", "Green", "Yellow", "White" };
            for (int i = 0; i < expectedMessages.Length; i++) expectedMessages[i] += " Aura";

            // Test SpellResult messages and ensure they're there and in order
            // Iterate starting at index n and move towards the end, testing in that order
            for (int i = 0; i < (expectedMessages.Length - 1); i++)
            {
                SpellResult res = SpellManager.CastSpell(monster, monster, spell, i + 1);
                int startIndex = (expectedMessages.Length - 1) - i;
                int endIndex = (expectedMessages.Length - 1);
                for (int j = startIndex, ii = 0; j <= endIndex; j++, ii++)
                {
                    Assert.AreEqual(expectedMessages[j], res.Results[ii]);
                }
            }
        }

        [TestMethod]
        public void DSPLTest()
        {
            // Setup
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("DSPL");
            string[] expectedMessages = { "Chng", "Fire", "Soul", "Bolt", "Prot", "Poison", "Body", "Ice" };
            for (int i = 0; i < expectedMessages.Length; i++) expectedMessages[i] += " def. dn";

            // Even though DSPL in non-functional in the NES version, ensure messages are returned properly
            // TODO: That ^
        }

        [TestMethod]
        public void DRANTest()
        {
            // Set up
            Monster caster = new Monster();
            caster.HPMax = 160;
            caster.HP = 1;
            Monster target = new Monster();
            target.HP = target.HPMax = 160;
            Spell spell = SpellManager.GetSpellByName("DRAN");
            spell.Accuracy = 50; // Adjust accuracy so it actually hits

            // Cast the spell and ensure HP is drained
            Assert.IsTrue(caster.HP == 1);
            Assert.IsTrue(target.HP == target.HPMax);
            SpellManager.CastSpell(caster, target, spell, 16);
            Assert.IsTrue(caster.HP > 1);
            Assert.IsTrue(target.HP < target.HPMax);

            // Ensure Undead creatures reverse the effect
            caster.HP = caster.HPMax;
            target.HP = 1;
            target.Families.Add(MonsterFamily.Undead);
            Assert.AreEqual(caster.HPMax, caster.HP);
            Assert.AreEqual(1, target.HP);
            SpellManager.CastSpell(caster, target, spell, 16);
            Assert.IsTrue(caster.HP < caster.HPMax);
            Assert.IsTrue(target.HP > 1);

            // No resistance check due to no element
        }

        [TestMethod]
        public void ASPLTest()
        {
            // Set up
            Monster caster = new Monster();
            caster.MPMax = 160;
            caster.MP = 0;
            Monster target = new Monster();
            target.MP = target.MPMax = 160;
            Spell spell = SpellManager.GetSpellByName("ASPL");
            spell.Accuracy = 50; // Adjust accuracy so it actually hits

            // Cast the spell and ensure MP is drained
            Assert.IsTrue(caster.MP == 0);
            Assert.IsTrue(target.MP == target.MPMax);
            SpellManager.CastSpell(caster, target, spell, 16);
            Assert.IsTrue(caster.MP > 0);
            Assert.IsTrue(target.MP < target.MPMax);

            // Ensure Undead creatures reverse the effect
            caster.MP = caster.MPMax;
            target.MP = 0;
            target.Families.Add(MonsterFamily.Undead);
            Assert.AreEqual(caster.MPMax, caster.MP);
            Assert.AreEqual(0, target.MP);
            SpellManager.CastSpell(caster, target, spell, 16);
            Assert.IsTrue(caster.MP < caster.MPMax);
            Assert.IsTrue(target.MP > 0);

            // No resistance check due to no element
        }

        [TestMethod]
        public void ANTITest()
        {
            // Set up
            Monster monster = new Monster();
            monster.MPMax = 1000;
            monster.MP = 100;
            Assert.AreEqual(100, monster.MP);
            Spell spell = SpellManager.GetSpellByName("ANTI");

            // Cast the spell and ensure that MP was halved in it's bugged fashion
            // TODO: That ^

            // TODO: Ensure the spell fails if the target is resistant to ANTI's element
            monster.Resistances.Add(Element.Mind);
        }

        [TestMethod]
        public void CHNGTest()
        {
            // Set up some monsters
            Monster caster = new Monster();
            caster.HPMax = 500;
            caster.MPMax = 500;
            caster.HP = 100;
            caster.MP = 100;
            Assert.AreEqual(100, caster.HP);
            Assert.AreEqual(100, caster.MP);

            Monster target = new Monster();
            target.HPMax = 500;
            target.MPMax = 500;
            target.HP = 200;
            target.MP = 200;
            Assert.AreEqual(200, target.HP);
            Assert.AreEqual(200, target.MP);

            // Cast the spell and ensure the hp/mp values were swapped
            Spell spell = SpellManager.GetSpellByName("CHNG");
            SpellManager.CastSpell(caster, target, spell, 16, false);
            Assert.AreEqual(200, caster.HP);
            Assert.AreEqual(200, caster.MP);
            Assert.AreEqual(100, target.HP);
            Assert.AreEqual(100, target.MP);

            // Ensure the spell fails if the target is resistant to CHNG's element
            target.Resistances.Add(Element.Dimension);
            SpellManager.CastSpell(caster, target, spell, 16, false);
            Assert.AreEqual(200, caster.HP);
            Assert.AreEqual(200, caster.MP);
            Assert.AreEqual(100, target.HP);
            Assert.AreEqual(100, target.MP);
        }

        [TestMethod]
        public void BlastTest()
        {
            // Set up
            Monster monster = new Monster();
            Spell spell = SpellManager.GetSpellByName("Blast");

            // Unless BUG_FIX, this does not work when caster is at full HP
            // After being cast, the caster should be removed from battle
            // This spell acts like physical damage instead of spell damage
            // If the target has 40+ defense, there should be no damage achieved
        }

    }
}
