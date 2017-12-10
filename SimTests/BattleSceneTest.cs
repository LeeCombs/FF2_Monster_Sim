using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FF2_Monster_Sim;

namespace SimTests
{
    [TestClass]
    public class BattleSceneTest
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            MonsterManager.Initialize();
            MonsterManager.LoadContent();
        }

        [TestMethod]
        public void SceneCreationTest()
        {
            // Test Defaults
            BattleScene scene = new BattleScene();

            Assert.AreEqual(0, scene.X);
            Assert.AreEqual(0, scene.Y);
            Assert.AreEqual(0, scene.GetAllTargets().Length);
            Assert.IsNull(scene.GetAnySingleTarget());
            Assert.IsNull(scene.GetFrontRowTarget());
        }

        [TestMethod]
        public void SceneATest()
        {
            // Setup
            BattleScene scene = new BattleScene(type: "A");
            List<string> monNames;
            List<Monster> monList = new List<Monster>();

            // Add 8 Monsters to the scene
            monNames = new List<string>{ "Balloon", "Balloon", "Changer", "Changer", "DeadHead", "DeadHead", "Eagle", "Eagle" };
            foreach (string name in monNames)
                monList.Add(MonsterManager.GetMonsterByName(name));
            scene.PopulateScene(monList);

            Assert.AreEqual(8, scene.GetAllTargets().Length);
            Assert.IsNotNull(scene.GetFrontRowTarget());
            for (int i = 0; i < 1000; i++)
            {
                Monster anyMonster = scene.GetAnySingleTarget();
                Assert.IsNotNull(scene.GetAnySingleTarget());
                Assert.IsTrue(monNames.Contains(anyMonster.Name));

                Monster frontMonster = scene.GetFrontRowTarget();
                Assert.IsTrue(frontMonster.Name == "DeadHead" || frontMonster.Name == "Eagle");
            }

            // Clear the scene
            scene.ClearScene();
            monList.Clear();
            Assert.AreEqual(0, scene.GetAllTargets().Length);
            Assert.IsNull(scene.GetAnySingleTarget());
            Assert.IsNull(scene.GetFrontRowTarget());

            // Add 6 Monsters to the scene
            monNames = new List<string> { "ElecFish", "ElecFish", "G.Goblin", "G.Goblin", "Grenade", "Grenade" };
            foreach (string name in monNames)
                monList.Add(MonsterManager.GetMonsterByName(name));
            scene.PopulateScene(monList);

            Assert.AreEqual(6, scene.GetAllTargets().Length);
            for (int i = 0; i < 1000; i++)
            {
                Monster anyMonster = scene.GetAnySingleTarget();
                Assert.IsNotNull(scene.GetAnySingleTarget());
                Assert.IsTrue(monNames.Contains(anyMonster.Name));

                Monster frontMonster = scene.GetFrontRowTarget();
                Assert.IsTrue(frontMonster.Name == "G.Goblin" || frontMonster.Name == "Grenade");
            }
        }

    }
}
