﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FF2_Monster_Sim;

namespace SimTests
{
    [TestClass]
    public class MonsterTest
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            MonsterManager.Initialize();
            MonsterManager.LoadContent();
        }

        [TestMethod]
        public void MonsterCreationTest()
        {
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
            // race
            // resist
            // absorb
            // weak
            // attacklist
            // attackeffect
            // gildrops
            // itemdrops
        }

        [TestMethod]
        public void MonsterStatChangeTest()
        {
            // Test that changes produce expected values, min/max values are enforced, and overflow checks
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // HP
            Assert.AreEqual(6, monster.HP);
            Assert.AreEqual(6, monster.HPMax);
            monster.HP = 0;
            Assert.AreEqual(0, monster.HP);
            monster.HP = 100;
            Assert.AreNotEqual(100, monster.HPMax);
            Assert.AreEqual(monster.HPMax, monster.HP);
            monster.HP = -100;
            Assert.AreNotEqual(-100, monster.HPMax);
            Assert.AreEqual(0, monster.HP);

            // MP
            Assert.AreEqual(0, monster.MP);
            Assert.AreEqual(0, monster.MPMax);
            monster.MP = 100;
            Assert.AreNotEqual(100, monster.MPMax);
            Assert.AreEqual(monster.MPMax, monster.MP);
            monster.MP = -100;
            Assert.AreNotEqual(-100, monster.MPMax);
            Assert.AreEqual(0, monster.MP);

            // Strength
            Assert.AreEqual(4, monster.Strength);
            monster.Strength = 100;
            Assert.AreEqual(100, monster.Strength);
            monster.Strength = -100;
            Assert.AreNotEqual(-100, monster.Strength);
            Assert.AreEqual(0, monster.Strength);
            monster.Strength = 500;
            Assert.AreNotEqual(500, monster.Strength);
            Assert.AreEqual(255, monster.Strength);

            // Defense
            Assert.AreEqual(0, monster.Defense);
            monster.Defense = 100;
            Assert.AreEqual(100, monster.Defense);
            monster.Defense = -100;
            Assert.AreNotEqual(-100, monster.Defense);
            Assert.AreEqual(0, monster.Defense);
            monster.Defense = 500;
            Assert.AreNotEqual(500, monster.Defense);
            Assert.AreEqual(255, monster.Defense);

            // Fear
            Assert.AreEqual(180, monster.Fear);
            monster.Fear = 200;
            Assert.AreEqual(200, monster.Fear);
            monster.Fear = -100;
            Assert.AreNotEqual(-100, monster.Fear);
            Assert.AreEqual(0, monster.Fear);
            monster.Fear = 500;
            Assert.AreNotEqual(500, monster.Fear);
            Assert.AreEqual(255, monster.Fear);

            // Hits
            Assert.AreEqual(1, monster.Hits);
            monster.Hits = 8;
            Assert.AreEqual(8, monster.Hits);
            monster.Hits = -100;
            Assert.AreNotEqual(-100, monster.Hits);
            Assert.AreEqual(1, monster.Hits);
            monster.Hits = 100;
            Assert.AreNotEqual(500, monster.Hits);
            Assert.AreEqual(16, monster.Hits);

            // Blocks
            Assert.AreEqual(0, monster.Blocks);
            monster.Blocks = 100;
            Assert.AreEqual(100, monster.Blocks);
            monster.Blocks = -100;
            Assert.AreNotEqual(-100, monster.Blocks);
            Assert.AreEqual(0, monster.Blocks);
            monster.Blocks = 300;
            Assert.AreNotEqual(300, monster.Blocks);
            Assert.AreEqual(255, monster.Blocks);

            // MagicBlocks
            Assert.AreEqual(1, monster.MagicBlocks);
            monster.MagicBlocks = 100;
            Assert.AreEqual(100, monster.MagicBlocks);
            monster.MagicBlocks = -100;
            Assert.AreNotEqual(-100, monster.MagicBlocks);
            Assert.AreEqual(0, monster.MagicBlocks);
            monster.MagicBlocks = 300;
            Assert.AreNotEqual(300, monster.MagicBlocks);
            Assert.AreEqual(255, monster.MagicBlocks);

            // Accuracy
            Assert.AreEqual(60, monster.Accuracy);
            monster.Accuracy = 50;
            Assert.AreEqual(50, monster.Accuracy);
            monster.Accuracy = -100;
            Assert.AreNotEqual(-100, monster.Accuracy);
            Assert.AreEqual(0, monster.Accuracy);
            monster.Accuracy = 100;
            Assert.AreNotEqual(100, monster.Accuracy);
            Assert.AreEqual(99, monster.Accuracy);

            // Evasion
            Assert.AreEqual(0, monster.Evasion);
            monster.Evasion = 50;
            Assert.AreEqual(50, monster.Evasion);
            monster.Evasion = -100;
            Assert.AreNotEqual(-100, monster.Evasion);
            Assert.AreEqual(0, monster.Evasion);
            monster.Evasion = 100;
            Assert.AreNotEqual(100, monster.Evasion);
            Assert.AreEqual(99, monster.Evasion);

            // MagicEvasion
            Assert.AreEqual(50, monster.MagicEvasion);
            monster.MagicEvasion = 50;
            Assert.AreEqual(50, monster.MagicEvasion);
            monster.MagicEvasion = -100;
            Assert.AreNotEqual(-100, monster.MagicEvasion);
            Assert.AreEqual(0, monster.MagicEvasion);
            monster.MagicEvasion = 100;
            Assert.AreNotEqual(100, monster.MagicEvasion);
            Assert.AreEqual(99, monster.MagicEvasion);

            // race
            // resist
            // absorb
            // weak
            // attacklist
            // attackeffect
            // gildrops
            // itemdrops
        }

        ///////////
        // Buffs //
        ///////////
        
        [TestMethod]
        public void GeneralBuffTest()
        {
            // Ensure invalid inputs don't add the buff (specific buff does not matter)
            Monster monster = new Monster();
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Aura));
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Berserk));
            Assert.IsFalse(monster.AddBuff(Buff.Aura, -1));
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Aura));
            Assert.IsFalse(monster.AddBuff(Buff.Berserk, 17));
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Berserk));
        }

        [TestMethod]
        public void AddRemoveBuffTest()
        {
            Monster monster = new Monster();
            AddRemoveBuff(monster, Buff.Aura);
            AddRemoveBuff(monster, Buff.Barrier);
            AddRemoveBuff(monster, Buff.Berserk);
            AddRemoveBuff(monster, Buff.Blink);
            AddRemoveBuff(monster, Buff.Haste);
            AddRemoveBuff(monster, Buff.Imbibe);
            AddRemoveBuff(monster, Buff.Protect);
            AddRemoveBuff(monster, Buff.Shell);
            AddRemoveBuff(monster, Buff.Wall);

            // Skip below for now as they may be irrelevant
            // AddRemoveBuff(monster, Buff.Spirit);
            // AddRemoveBuff(monster, Buff.Intelligence);
        }
        
        /// <summary>
        /// Helper. Adds and removes one buff stack from the monster.
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="buff"></param>
        private void AddRemoveBuff(Monster monster, Buff buff)
        {
            // Test that buff can be added and removed
            monster.AddBuff(buff, 1);
            Assert.AreEqual(1, monster.GetBuffStacks(buff));
            monster.RemoveBuff(buff);
            Assert.AreEqual(0, monster.GetBuffStacks(buff));
        }
        
        [TestMethod]
        public void AddRemoveStackingBuffTest()
        {
            Monster monster = new Monster();
            AddRemoveStackingBuff(monster, Buff.Berserk);
            AddRemoveStackingBuff(monster, Buff.Blink);
            AddRemoveStackingBuff(monster, Buff.Imbibe);
            AddRemoveStackingBuff(monster, Buff.Protect);
            AddRemoveStackingBuff(monster, Buff.Shell);
        }

        /// <summary>
        /// Helper. Add and removes stacks from cumulative buffs
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="buff"></param>
        private void AddRemoveStackingBuff(Monster monster, Buff buff)
        {
            // Ensure buff stacks are cumulative and remove properly
            Assert.AreEqual(0, monster.GetBuffStacks(buff));
            monster.AddBuff(buff, 1);
            Assert.AreEqual(1, monster.GetBuffStacks(buff));
            monster.AddBuff(buff, 1);
            Assert.AreEqual(2, monster.GetBuffStacks(buff));
            monster.AddBuff(buff, 2);
            Assert.AreEqual(4, monster.GetBuffStacks(buff));
            monster.RemoveBuff(buff);
            Assert.AreEqual(0, monster.GetBuffStacks(buff));

            // Overflow check: Cannot exceed 255
            for (int i = 0; i < 15; i++) monster.AddBuff(buff, 16);
            Assert.AreEqual(240, monster.GetBuffStacks(buff));
            monster.AddBuff(buff, 16);
            Assert.AreEqual(0, monster.GetBuffStacks(buff));
        }

        [TestMethod]
        public void AddRemoveHighestStackBuffTest()
        {
            Monster monster = new Monster();
            AddRemoveHighestStackBuff(monster, Buff.Aura, 8);
            AddRemoveHighestStackBuff(monster, Buff.Barrier, 8);
            AddRemoveHighestStackBuff(monster, Buff.Haste, 16);
            AddRemoveHighestStackBuff(monster, Buff.Wall, 16);
        }

        /// <summary>
        /// Helper. Add and remove highest-stacking buffs and check maxStack is enforced
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="buff"></param>
        /// <param name="maxStack">How high the buff can stack</param>
        private void AddRemoveHighestStackBuff(Monster monster, Buff buff, int maxStack)
        {
            // Ensure only the highest stack value is active
            Assert.AreEqual(0, monster.GetBuffStacks(buff));
            monster.AddBuff(buff, 1);
            Assert.AreEqual(1, monster.GetBuffStacks(buff));
            monster.AddBuff(buff, 1);
            Assert.AreEqual(1, monster.GetBuffStacks(buff));
            monster.AddBuff(buff, 5);
            Assert.AreEqual(5, monster.GetBuffStacks(buff));
            monster.AddBuff(buff, 1);
            Assert.AreEqual(5, monster.GetBuffStacks(buff));
            monster.RemoveBuff(buff);
            Assert.AreEqual(0, monster.GetBuffStacks(buff));

            // Ensure buff cannot exceed max stacks
            monster.AddBuff(buff, 16);
            Assert.AreEqual(maxStack, monster.GetBuffStacks(buff));
        }

        [TestMethod]
        public void AuraTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Aura));

            // TODO: Ensure stacks have intended effect: Grants family-killing properties to main weapon
            // 1 - Magic Beast
            // 2 - Aquatic
            // 3 - Earth
            // 4 - Giants
            // 5 - Spellcaster
            // 6 - Dragons
            // 7 - Were
            // 8 - Undead(Doesn't work)

            // TODO: Ensure 8th stack doesn't work unless fix is selected
        }

        [TestMethod]
        public void BarrierTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Barrier));

            // TODO: Ensure stacks have intended effect: Elemental resistance bsaed on stack level
            // 1 - Matter
            // 2 - Fire
            // 3 - Mind
            // 4 - Lighting
            // 5 - Death
            // 6 - Poison
            // 7 - Body
            // 8 - Ice (Doesn't work)

            // TODO: Ensure 8th stack doesn't work unless fix is selected
        }

        [TestMethod]
        public void BerserkTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Berserk));
            Assert.AreEqual(4, monster.Strength);
            int baseStr = monster.Strength;
            
            // Ensure stacks have intended effect (+5 str per stack)
            monster.AddBuff(Buff.Berserk, 1);
            Assert.AreEqual(baseStr + 5, monster.Strength);
            monster.AddBuff(Buff.Berserk, 2);
            Assert.AreEqual(baseStr + (3 * 5), monster.Strength);
            monster.RemoveBuff(Buff.Berserk);
            Assert.AreEqual(baseStr, monster.Strength);

            // Ensure Str + Buff doesn't exceed 255
            monster.AddBuff(Buff.Berserk, 16);
            monster.AddBuff(Buff.Berserk, 16);
            monster.AddBuff(Buff.Berserk, 16);
            monster.AddBuff(Buff.Berserk, 3); // 51 * 5 = 255 Buff Bonus
            Assert.AreEqual(255, monster.Strength); // min(255, str+(buff%256))

            // Check Buff Overflow
            monster.AddBuff(Buff.Berserk, 1); // 52 * 5 = 260 Buff Bonus
            Assert.AreEqual(8, monster.Strength); // str + (buff % 256)
        }

        [TestMethod]
        public void BlinkTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Blink));
            Assert.AreEqual(0, monster.Blocks);
            int baseBlocks = monster.Blocks;

            // Ensure stacks have intended effect (+1 block per stack)
            monster.AddBuff(Buff.Blink, 1);
            Assert.AreEqual(baseBlocks + 1, monster.Blocks);
            monster.AddBuff(Buff.Blink, 2);
            Assert.AreEqual(baseBlocks + 3, monster.Blocks);
            monster.RemoveBuff(Buff.Blink);
            Assert.AreEqual(baseBlocks, monster.Blocks);

            // Ensure Stat + Buff doesn't exceed 255
            monster.Blocks = 250; // Temp stat adjustment
            Assert.AreEqual(250, monster.Blocks);
            monster.AddBuff(Buff.Blink, 16); // 266 total (250 + 16)
            Assert.AreEqual(255, monster.Blocks);
        }

        [TestMethod]
        public void HasteTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Haste));
            Assert.AreEqual(1, monster.Hits);
            int baseHits = monster.Hits;
            
            // Ensure stacks have intended effect (+1 hit per stack)
            monster.AddBuff(Buff.Haste, 1);
            Assert.AreEqual(baseHits + 1, monster.Hits);
            monster.AddBuff(Buff.Haste, 5);
            Assert.AreEqual(baseHits + 5, monster.Hits);
            monster.AddBuff(Buff.Haste, 1); // Ensure lower-level Haste doesn't change hits
            Assert.AreEqual(baseHits + 5, monster.Hits);
            monster.RemoveBuff(Buff.Haste);
            Assert.AreEqual(1, monster.Hits);
        }

        [TestMethod]
        public void ProtectTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Protect));
            Assert.AreEqual(0, monster.Defense);
            int baseDefense = monster.Defense;

            // Note: This can't be completed until the + defense value is figured out
            // TODO: Ensure stacks have intended effect (+__ defense per stack)
            // TODO: Ensure Stat + Buff doesn't exceed 255
        }

        [TestMethod]
        public void ShellTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Shell));
            Assert.AreEqual(1, monster.MagicBlocks);
            int baseBlocks = monster.MagicBlocks;
            
            // Ensure stacks have intended effect (+1 mBlock per stack)
            monster.AddBuff(Buff.Shell, 1);
            Assert.AreEqual(baseBlocks + 1, monster.MagicBlocks);
            monster.AddBuff(Buff.Shell, 2);
            Assert.AreEqual(baseBlocks + 3, monster.MagicBlocks);
            monster.RemoveBuff(Buff.Shell);
            Assert.AreEqual(baseBlocks, monster.MagicBlocks);

            // Ensure Stat + Buff doesn't exceed 255
            monster.MagicBlocks = 250; // Temp stat adjustment
            Assert.AreEqual(250, monster.MagicBlocks);
            monster.AddBuff(Buff.Shell, 16); // 266 total (250 + 16)
            Assert.AreEqual(255, monster.MagicBlocks);
        }

        [TestMethod]
        public void WallTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Wall));

            // TODO: Ensure stacks have intended effect: Negates ALL spells up to it's level

            // TODO: If wall exists and an instant KO spell is used, it succeeds?
        }

        [TestMethod]
        public void ImbibeTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Imbibe));
            Assert.AreEqual(4, monster.Strength);
            int baseStr = monster.Strength;

            // Ensure stacks have intended effects (+10 str per stack)
            monster.AddBuff(Buff.Imbibe, 1);
            Assert.AreEqual(baseStr + 10, monster.Strength);
            monster.AddBuff(Buff.Imbibe, 2);
            Assert.AreEqual(baseStr + (3 * 10), monster.Strength);
            monster.RemoveBuff(Buff.Imbibe);
            Assert.AreEqual(baseStr, monster.Strength);

            // Ensure Str + Buff doesn't exceed 255
            monster.Strength = 10; // Temp increase LegEater's base str to execute test
            monster.AddBuff(Buff.Imbibe, 16);
            monster.AddBuff(Buff.Imbibe, 9); // 25 * 10 = 250 Buff Bonus
            Assert.AreEqual(255, monster.Strength); // min(255, str+(buff%256))
            monster.Strength = baseStr;

            // Check Buff Overflow
            monster.AddBuff(Buff.Imbibe, 1); // 26 * 10 = 260 Buff Bonus
            Assert.AreEqual(8, monster.Strength); // str + (buff % 256)
        }

        [TestMethod]
        public void IntelligenceTest()
        {
            // Skip for now, might only be relevant for PCs
        }

        [TestMethod]
        public void SpiritTest()
        {
            // Skip for now, might only be relevant for PCs
        }

        /////////////
        // Debuffs //
        /////////////

        [TestMethod]
        public void SlowTest()
        {
            //
        }

        [TestMethod]
        public void FearTest()
        {
            // Test that Debuffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetDebuffStacks(Debuff.Fear));
            Assert.AreEqual(180, monster.Fear);
            int baseFear = monster.Fear;

            // Ensure stacks are cumulative and remove properly
            monster.AddDebuff(Debuff.Fear, 1);
            Assert.AreEqual(1, monster.GetDebuffStacks(Debuff.Fear));
            Assert.AreEqual(baseFear + 20, monster.Fear);

            monster.AddDebuff(Debuff.Fear, 2);
            Assert.AreEqual(3, monster.GetDebuffStacks(Debuff.Fear));
            Assert.AreEqual(baseFear + (3 * 20), monster.Fear);

            monster.RemoveDebuff(Debuff.Fear);
            Assert.AreEqual(0, monster.GetDebuffStacks(Debuff.Fear));
            Assert.AreEqual(baseFear, monster.Fear);

            // Check overflow
            monster.AddDebuff(Debuff.Fear, 13); // 13 * 20 = 260 total
            Assert.AreEqual(184, monster.Fear); // 180 base + (260 % 256)
        }

        ////////////////
        // PermStatus //
        ////////////////

        [TestMethod]
        public void AddRemovePermStatusTest()
        {
            Monster monster = new Monster();
            AddRemovePermStatus(monster, PermStatus.Amnesia);
            AddRemovePermStatus(monster, PermStatus.Curse);
            AddRemovePermStatus(monster, PermStatus.Darkness);
            AddRemovePermStatus(monster, PermStatus.Poison);
            // KO, Stone, Toad all kill on success
        }

        /// <summary>
        /// Helper. Add and remove a PermStatus from a monster.
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="permStatus"></param>
        private void AddRemovePermStatus(Monster monster, PermStatus permStatus)
        {
            // Test that the status can be applied and removed
            monster.AddPermStatus(permStatus);
            Assert.IsTrue(monster.HasPermStatus(permStatus));
            monster.RemovePermStatus(permStatus);
            Assert.IsFalse(monster.HasPermStatus(permStatus));
        }

        [TestMethod]
        public void AmnesiaTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasPermStatus(PermStatus.Amnesia));
            
            // Test intended effects
            // TODO: Get monster who can only cast spells (Wizard) to cast a spell
            monster.AddPermStatus(PermStatus.Amnesia);
            // TODO: Cast a spell again and get Attack instead
            monster.RemovePermStatus(PermStatus.Amnesia);
            // TODO: Cast a spell again and get a spell

            // TODO: Esuna 4, Mallet removes status check
        }

        [TestMethod]
        public void CurseTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasPermStatus(PermStatus.Curse));
            Assert.AreEqual(4, monster.Strength);
            Assert.AreEqual(0, monster.Defense);
            // TODO: Spell power

            // Test intended effects: Halve Offense and defense
            // TODO: Spell power reduction check
            monster.Defense = 5; // Temp stat adjustment
            Assert.AreEqual(5, monster.Defense);

            monster.AddPermStatus(PermStatus.Curse);
            Assert.AreEqual(2, monster.Strength);
            Assert.AreEqual(2, monster.Defense); // Stat reductions round down

            monster.RemovePermStatus(PermStatus.Curse);
            Assert.AreEqual(4, monster.Strength);
            Assert.AreEqual(5, monster.Defense);
            monster.Defense = 0; // Set it back

            // TODO: Esuna 3, Cross removes status check
        }

        [TestMethod]
        public void DarknessTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasPermStatus(PermStatus.Darkness));
            Assert.AreEqual(60, monster.Accuracy);

            // Test intended effects: Halves accuracy
            monster.AddPermStatus(PermStatus.Darkness);
            Assert.AreEqual(30, monster.Accuracy);
            monster.RemovePermStatus(PermStatus.Darkness);
            Assert.AreEqual(60, monster.Accuracy);

            // Ensure values are rounded down
            monster.Accuracy = 61; // Temp stat adjustment
            Assert.AreEqual(61, monster.Accuracy);
            monster.AddPermStatus(PermStatus.Darkness);
            Assert.AreEqual(30, monster.Accuracy);
            monster.RemovePermStatus(PermStatus.Darkness);
            Assert.AreEqual(61, monster.Accuracy);
            monster.Accuracy = 60; // Set it back
            
            // TODO: Esuna 1, Eyedrops removes status check
        }

        [TestMethod]
        public void PoisonTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasPermStatus(PermStatus.Poison));
            Assert.AreEqual(6, monster.HP);

            // TODO: Test intended effects
            // Progress a turn and check HP reduction

            // TODO: Esuna 2, Antidote removes status check
        }

        [TestMethod]
        public void KOTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasPermStatus(PermStatus.KO));

            // Test that the status can be applied and the monster is removed from battle
            monster.AddPermStatus(PermStatus.KO);
            // TODO: Check for death
        }

        [TestMethod]
        public void StoneTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasPermStatus(PermStatus.Stone));

            // Test that the status can be applied and the monster is removed from battle
            monster.AddPermStatus(PermStatus.Stone);
            // TODO: Check for death
        }

        [TestMethod]
        public void ToadTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasPermStatus(PermStatus.Toad));

            // Test that the status can be applied and the monster is removed from battle
            monster.AddPermStatus(PermStatus.Toad);
            // TODO: Check for death
        }

        ////////////////
        // TempStatus //
        ////////////////

        [TestMethod]
        public void AddRemoveTempStatusTest()
        {
            Monster monster = new Monster();
            AddRemoveTempStatus(monster, TempStatus.Confuse);
            AddRemoveTempStatus(monster, TempStatus.Mute);
            AddRemoveTempStatus(monster, TempStatus.Paralysis);
            AddRemoveTempStatus(monster, TempStatus.Sleep);
            AddRemoveTempStatus(monster, TempStatus.Venom);
            // Mini kills on success
        }

        /// <summary>
        /// Helper. Add and remove a TempStatus from a monster.
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="tempStatus"></param>
        private void AddRemoveTempStatus(Monster monster, TempStatus tempStatus)
        {
            // Test that the status can be applied and removed
            monster.AddTempStatus(tempStatus);
            Assert.IsTrue(monster.HasTempStatus(tempStatus));
            monster.RemoveTempStatus(tempStatus);
            Assert.IsFalse(monster.HasTempStatus(tempStatus));
        }

        [TestMethod]
        public void ConfuseTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasTempStatus(TempStatus.Confuse));

            // Test that the status can be applied and removed
            monster.AddTempStatus(TempStatus.Confuse);
            Assert.IsTrue(monster.HasTempStatus(TempStatus.Confuse));
            monster.RemoveTempStatus(TempStatus.Confuse);
            Assert.IsFalse(monster.HasTempStatus(TempStatus.Confuse));

            // Test intended effects
            // TODO: Monster targets enemy as usual
            monster.AddTempStatus(TempStatus.Confuse);
            // TODO: Monster targets ally
            monster.RemoveTempStatus(TempStatus.Confuse);
            // TODO: Monster targets enemy

            // TODO: Basuna 5 removes status check
            // TODO: Chance to cure self after turn check
        }

        [TestMethod]
        public void MuteTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasTempStatus(TempStatus.Mute));

            // Test that the status can be applied and removed
            monster.AddTempStatus(TempStatus.Mute);
            Assert.IsTrue(monster.HasTempStatus(TempStatus.Mute));
            monster.RemoveTempStatus(TempStatus.Mute);
            Assert.IsFalse(monster.HasTempStatus(TempStatus.Mute));

            // Test intended effects
            // TODO: Get monster who can only cast spells (Wizard) to cast a spell
            monster.AddTempStatus(TempStatus.Mute);
            // TODO: Cast a spell again and get Attack instead
            monster.RemoveTempStatus(TempStatus.Mute);
            // TODO: Cast a spell again and get a spell

            // TODO: Basuna 2 removes status check
            // TODO: Chance to cure self after turn check
        }

        [TestMethod]
        public void ParalysisTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasTempStatus(TempStatus.Paralysis));
            Assert.AreEqual(0, monster.Evasion);

            // Test that the status can be applied and removed
            monster.AddTempStatus(TempStatus.Paralysis);
            Assert.IsTrue(monster.HasTempStatus(TempStatus.Paralysis));
            monster.RemoveTempStatus(TempStatus.Paralysis);
            Assert.IsFalse(monster.HasTempStatus(TempStatus.Paralysis));

            // Test intended effects: No actions, no Evasion
            monster.Evasion = 10; // Temp stat change
            Assert.AreEqual(10, monster.Evasion);
            // TODO: Get action from monster
            monster.AddTempStatus(TempStatus.Paralysis);
            Assert.AreEqual(0, monster.Evasion);
            // TODO: Ensure no action from monster
            monster.RemoveTempStatus(TempStatus.Paralysis);
            Assert.AreEqual(10, monster.Evasion);
            // TODO: Get action from monster
            monster.Evasion = 0; // Set stat back

            // TODO: Basuna 4 removes status check
            // TODO: Chance to cure self after turn check
        }

        [TestMethod]
        public void SleepTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasTempStatus(TempStatus.Sleep));
            Assert.AreEqual(0, monster.Evasion);

            // Test that the status can be applied and removed
            monster.AddTempStatus(TempStatus.Sleep);
            Assert.IsTrue(monster.HasTempStatus(TempStatus.Sleep));
            monster.RemoveTempStatus(TempStatus.Sleep);
            Assert.IsFalse(monster.HasTempStatus(TempStatus.Sleep));

            // Test intended effects: No actions, no Evasion
            monster.Evasion = 10; // Temp stat change
            Assert.AreEqual(10, monster.Evasion);
            // TODO: Get action from monster
            monster.AddTempStatus(TempStatus.Sleep);
            Assert.AreEqual(0, monster.Evasion);
            // TODO: Ensure no action from monster
            monster.RemoveTempStatus(TempStatus.Sleep);
            Assert.AreEqual(10, monster.Evasion);
            // TODO: Get action from monster
            monster.Evasion = 0; // Set stat back

            // TODO: Basuna 1 removes status check
            // TODO: Chance to cure self after turn check
        }

        [TestMethod]
        public void VenomTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasTempStatus(TempStatus.Venom));
            Assert.AreEqual(6, monster.HP);

            // Test that the status can be applied and removed
            monster.AddTempStatus(TempStatus.Venom);
            Assert.IsTrue(monster.HasTempStatus(TempStatus.Venom));
            monster.RemoveTempStatus(TempStatus.Venom);
            Assert.IsFalse(monster.HasTempStatus(TempStatus.Venom));

            // Test intended effects: HP reduction
            monster.AddTempStatus(TempStatus.Venom);
            // TODO: Progress a turn and check HP reduction

            // TODO: Basuna 1 removes status check
            // TODO: Chance to cure self after turn check
        }

        [TestMethod]
        public void MiniTest()
        {
            // Ensure normal base stats
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.IsFalse(monster.HasTempStatus(TempStatus.Mini));

            // Test that the status can be applied and the monster is removed from battle
            monster.AddTempStatus(TempStatus.Mini);
            // TODO: Check for death
        }
    }
}
