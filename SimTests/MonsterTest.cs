using System;
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
        public void CreationTest()
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
        public void StatChangeTest()
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
            // Ensure invalid inputs don't add the buff
            Monster monster = new Monster();
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Aura));
            Assert.AreEqual(false, monster.AddBuff(Buff.Aura, -1));
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Aura));
            Assert.AreEqual(false, monster.AddBuff(Buff.Aura, 17));
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Aura));
        }

        [TestMethod]
        public void AuraTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Ensure stacks are highest-value and remove properly

            // Ensure buff doesn't exceed 7 (8 with fix)
        }

        [TestMethod]
        public void BarrierTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Ensure stacks are highest-value and remove properly

            // Ensure buff doesn't exceed 7 (8 with fix)
        }

        [TestMethod]
        public void BerserkTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Berserk));
            Assert.AreEqual(4, monster.Strength);
            int baseStr = monster.Strength;

            // Ensure stacks are cumulative and remove properly
            monster.AddBuff(Buff.Berserk, 1); // 1 stack total, + 5 buff
            Assert.AreEqual(1, monster.GetBuffStacks(Buff.Berserk));
            Assert.AreEqual(baseStr + 5, monster.Strength);

            monster.AddBuff(Buff.Berserk, 2); // 3 stacks total, + 15 buff
            Assert.AreEqual(3, monster.GetBuffStacks(Buff.Berserk));
            Assert.AreEqual(baseStr + (3 * 5), monster.Strength);

            monster.RemoveBuff(Buff.Berserk);
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Berserk));
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
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Blink));
            Assert.AreEqual(0, monster.Blocks);
            int baseBlocks = monster.Blocks;

            // Ensure stacks are cumulative and remove properly
            monster.AddBuff(Buff.Blink, 1);
            Assert.AreEqual(1, monster.GetBuffStacks(Buff.Blink));
            Assert.AreEqual(baseBlocks + 1, monster.Blocks);

            monster.AddBuff(Buff.Blink, 2);
            Assert.AreEqual(3, monster.GetBuffStacks(Buff.Blink));
            Assert.AreEqual(baseBlocks + 3, monster.Blocks);

            monster.RemoveBuff(Buff.Blink);
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Blink));
            Assert.AreEqual(baseBlocks, monster.Blocks);

            // TODO: Overflow check?
        }

        [TestMethod]
        public void HasteTest()
        {
            // Haste cannot exceed 16 since it is not cumulative, and can't be set above 16

            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Haste));
            Assert.AreEqual(1, monster.Hits);
            int baseHits = monster.Hits;

            // Ensure stacks are highest-value and remove properly
            monster.AddBuff(Buff.Haste, 1);
            Assert.AreEqual(1, monster.GetBuffStacks(Buff.Haste));
            Assert.AreEqual(baseHits + 1, monster.Hits);

            monster.AddBuff(Buff.Haste, 1);
            Assert.AreEqual(1, monster.GetBuffStacks(Buff.Haste));
            Assert.AreEqual(baseHits + 1, monster.Hits);

            monster.AddBuff(Buff.Haste, 5);
            Assert.AreEqual(5, monster.GetBuffStacks(Buff.Haste));
            Assert.AreEqual(baseHits + 5, monster.Hits);

            monster.AddBuff(Buff.Haste, 1);
            Assert.AreEqual(5, monster.GetBuffStacks(Buff.Haste));
            Assert.AreEqual(baseHits + 5, monster.Hits);

            monster.RemoveBuff(Buff.Haste);
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Haste));
            Assert.AreEqual(1, monster.Hits);
        }

        [TestMethod]
        public void ProtectTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Ensure stacks are cumulative and remove properly

            // Ensure Stat + Buff doesn't exceed 255

            // Check Buff Overflow
        }

        [TestMethod]
        public void ShellTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Shell));
            Assert.AreEqual(1, monster.MagicBlocks);
            int baseBlocks = monster.MagicBlocks;

            // Ensure stacks are cumulative and remove properly
            monster.AddBuff(Buff.Shell, 1);
            Assert.AreEqual(1, monster.GetBuffStacks(Buff.Shell));
            Assert.AreEqual(baseBlocks + 1, monster.MagicBlocks);

            monster.AddBuff(Buff.Shell, 2);
            Assert.AreEqual(3, monster.GetBuffStacks(Buff.Shell));
            Assert.AreEqual(baseBlocks + 3, monster.MagicBlocks);

            monster.RemoveBuff(Buff.Shell);
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Shell));
            Assert.AreEqual(baseBlocks, monster.MagicBlocks);

            // TODO: Overflow check?
        }

        [TestMethod]
        public void WallTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Ensure stacks are highest-value and remove properly

            // Ensure buff doesn't exceed 16 (max spell level)
        }

        [TestMethod]
        public void ImbibeTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Imbibe));
            Assert.AreEqual(4, monster.Strength);
            int baseStr = monster.Strength;

            // Ensure stacks are cumulative and remove properly
            monster.AddBuff(Buff.Imbibe, 1); // 1 stack total, + 5 buff
            Assert.AreEqual(1, monster.GetBuffStacks(Buff.Imbibe));
            Assert.AreEqual(baseStr + 10, monster.Strength);

            monster.AddBuff(Buff.Imbibe, 2); // 3 stacks total, + 15 buff
            Assert.AreEqual(3, monster.GetBuffStacks(Buff.Imbibe));
            Assert.AreEqual(baseStr + (3 * 10), monster.Strength);

            monster.RemoveBuff(Buff.Imbibe);
            Assert.AreEqual(0, monster.GetBuffStacks(Buff.Imbibe));
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
        public void AmnesiaTest()
        {
            // Test that the status can be applied and removed
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(false, monster.HasPermStatus(PermStatus.Amnesia));

            monster.AddPermStatus(PermStatus.Amnesia);
            Assert.AreEqual(true, monster.HasPermStatus(PermStatus.Amnesia));

            monster.AddPermStatus(PermStatus.Amnesia);
            Assert.AreEqual(true, monster.HasPermStatus(PermStatus.Amnesia));

            monster.RemovePermStatus(PermStatus.Amnesia);
            Assert.AreEqual(false, monster.HasPermStatus(PermStatus.Amnesia));

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
            // Test that the status can be applied and removed
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
            Assert.AreEqual(false, monster.HasPermStatus(PermStatus.Curse));

            monster.AddPermStatus(PermStatus.Curse);
            Assert.AreEqual(true, monster.HasPermStatus(PermStatus.Curse));

            monster.AddPermStatus(PermStatus.Curse);
            Assert.AreEqual(true, monster.HasPermStatus(PermStatus.Curse));

            monster.RemovePermStatus(PermStatus.Curse);
            Assert.AreEqual(false, monster.HasPermStatus(PermStatus.Curse));

            // Test intended effects: Halve Offense and defense
            // TODO: Spell power reduction check
            // TODO: Stat reductions take the floor of divison (5/2 = 2). Check this?

            monster.Defense = 4; // Temp stat adjustment
            Assert.AreEqual(4, monster.Strength);
            Assert.AreEqual(4, monster.Defense);

            monster.AddPermStatus(PermStatus.Curse);
            Assert.AreEqual(2, monster.Strength);
            Assert.AreEqual(2, monster.Defense);

            monster.RemovePermStatus(PermStatus.Curse);
            Assert.AreEqual(4, monster.Strength);
            Assert.AreEqual(4, monster.Defense);
            monster.Defense = 0; // Set it back

            // TODO: Esuna 3, Cross removes status check
        }

        [TestMethod]
        public void DarknessTest()
        {
            // Test that the status can be applied and removed
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Test intended effects
            
            // TODO: Esuna 1, Eyedrops removes status check
        }

        [TestMethod]
        public void PoisonTest()
        {
            // Test that the status can be applied and removed
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Test intended effects
            
            // TODO: Esuna 2, Antidote removes status check
        }

        [TestMethod]
        public void KOTest()
        {
            // Removes monster from fight
        }

        [TestMethod]
        public void StoneTest()
        {
            // Removes monster from fight
        }

        [TestMethod]
        public void ToadTest()
        {
            // Removes monster from fight
        }

        ////////////////
        // TempStatus //
        ////////////////

        [TestMethod]
        public void ConfuseTest()
        {
            // Test that the status can be applied and removed
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Test intended effects

            // TODO: Basuna 5 removes status check
            // TODO: Chance to cure self after turn check
        }

        [TestMethod]
        public void MuteTest()
        {
            // Test that the status can be applied and removed
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Test intended effects
            // TODO: Get monster who can only cast spells (Wizard) to cast a spell
            monster.AddPermStatus(PermStatus.Amnesia);
            // TODO: Cast a spell again and get Attack instead
            monster.RemovePermStatus(PermStatus.Amnesia);
            // TODO: Cast a spell again and get a spell

            // TODO: Basuna 2 removes status check
            // TODO: Chance to cure self after turn check
        }

        [TestMethod]
        public void ParalysisTest()
        {
            // Test that the status can be applied and removed
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Test intended effects
            // No actions
            // No evasion

            // TODO: Basuna 4 removes status check
            // TODO: Chance to cure self after turn check
        }

        [TestMethod]
        public void SleepTest()
        {
            // Test that the status can be applied and removed
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Test intended effects
            // No actions
            // No evasion

            // TODO: Basuna 1 removes status check
            // TODO: Chance to cure self after turn check
        }

        [TestMethod]
        public void VenomTest()
        {
            // Test that the status can be applied and removed
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Test intended effects

            // TODO: Basuna 1 removes status check
            // TODO: Chance to cure self after turn check
        }

        [TestMethod]
        public void MiniTest()
        {
            // Removes monster from fight
        }
    }
}
