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
        public void AuraTest()
        {
            //
        }

        [TestMethod]
        public void BarrierTest()
        {
            //
        }

        [TestMethod]
        public void BerserkTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Ensure stacks are cumulative and remove properly
            Assert.AreEqual(4, monster.Strength);
            Assert.AreEqual(0, monster.HasBuff(Buff.Berserk));

            monster.AddBuff(Buff.Berserk, 1); // 1 stack total, + 5 buff
            Assert.AreEqual(1, monster.HasBuff(Buff.Berserk));
            Assert.AreEqual(9, monster.Strength);

            monster.AddBuff(Buff.Berserk, 2); // 3 stacks total, + 15 buff
            Assert.AreEqual(3, monster.HasBuff(Buff.Berserk));
            Assert.AreEqual(19, monster.Strength);

            monster.RemoveBuff(Buff.Berserk);
            Assert.AreEqual(4, monster.Strength);
            Assert.AreEqual(0, monster.HasBuff(Buff.Berserk));

            // Ensure Str + Buff doesn't exceed 255
            monster.AddBuff(Buff.Berserk, 51); // 51 * 5 = 255 Buff Bonus
            Assert.AreEqual(255, monster.Strength); // min(255, str+(buff%256))

            // Check Buff Overflow
            monster.AddBuff(Buff.Berserk, 1); // 52 * 5 = 260 Buff Bonus.
            Assert.AreEqual(8, monster.Strength); // str + (buff % 256)
        }

        [TestMethod]
        public void BlinkTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Ensure stacks are cumulative and remove properly
            Assert.AreEqual(0, monster.Blocks);
            Assert.AreEqual(0, monster.HasBuff(Buff.Blink));

            monster.AddBuff(Buff.Blink, 1); // 1 stack total, + 1 buff
            Assert.AreEqual(1, monster.HasBuff(Buff.Blink));
            Assert.AreEqual(1, monster.Blocks);

            monster.AddBuff(Buff.Blink, 2); // 3 stacks total, + 15 buff
            Assert.AreEqual(3, monster.HasBuff(Buff.Blink));
            Assert.AreEqual(3, monster.Blocks);

            monster.RemoveBuff(Buff.Blink);
            Assert.AreEqual(0, monster.Blocks);
            Assert.AreEqual(0, monster.HasBuff(Buff.Blink));
        }

        [TestMethod]
        public void HasteTest()
        {
            //
        }

        [TestMethod]
        public void ProtectTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Ensure stacks are cumulative and remove properly
            Assert.AreEqual(0, monster.Blocks);
            monster.AddBuff(Buff.Blink, 1);


            // Ensure Stat + Buff doesn't exceed 255

            // Check Buff Overflow
        }

        [TestMethod]
        public void ShellTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");

            // Ensure stacks are additive and removes properly

            // Ensure Stat + Buff doesn't exceed 255

            // Check Buff Overflow
        }

        [TestMethod]
        public void WallTest()
        {
            //
        }

        [TestMethod]
        public void EmbibeTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
        }

        [TestMethod]
        public void IntelligenceTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
        }

        [TestMethod]
        public void SpiritTest()
        {
            // Test that Buffs can be added and have intended effects
            Monster monster = MonsterManager.GetMonsterByName("LegEater");
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
            //
        }

        ////////////////
        // PermStatus //
        ////////////////

        [TestMethod]
        public void AmnesiaTest()
        {
            //
        }

        [TestMethod]
        public void CurseTest()
        {
            //
        }

        [TestMethod]
        public void DarknessTest()
        {
            //
        }

        [TestMethod]
        public void PoisonTest()
        {
            //
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
            //
        }

        [TestMethod]
        public void MiniTest()
        {
            // Removes monster from fight
        }

        [TestMethod]
        public void MuteTest()
        {
            //
        }

        [TestMethod]
        public void ParalysisTest()
        {
            //
        }

        [TestMethod]
        public void SleepTest()
        {
            //
        }

        [TestMethod]
        public void VenomTest()
        {
            //
        }
    }
}
