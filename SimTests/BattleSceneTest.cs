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
        public void ActionTest()
        {
            // Ensure Action struct behaves properly and things remain within bounds
        }
        
        [TestMethod]
        public void PopulateSceneTest()
        {
            // Ensure a valid monster list is populated as normal
            // Ensure a too-long list doesn't overfill the scene
            // Do this for A, B, and C-type scenes
        }

        [TestMethod]
        public void ClearSceneTest()
        {
            // Populate a scene with a valid monster list
            // Ensure monsters are cleared as expected
        }

        [TestMethod]
        public void GetMonsterActionsTest()
        {
            // Populate a scene with a valid monster list
            // Ensure a list of monster actions are properly generated and returned
            // This one sounds like a pain in the ass
        }


        [TestMethod]
        public void GetAllTargetsTest()
        {
            // Populate a scene with a valid monster list
            // Ensure all active/non-null targets are returned appropriately
        }

        [TestMethod]
        public void GetAnySingleTargetTest()
        {
            // Populate a scene with a valid monster list
            // Ensure only a monster from the scene is returned
            // Ensure that a monster is actually returned if there is one
            // Ensure nothing is returned if no monsters exist
        }

        [TestMethod]
        public void SceneA_GetFrontRowTargetTest()
        {
            // Populate a scene with a valid monster list
            // Ensure only front-row monsters are returned
            // Do this for 4, 3, 2, and 1 column-deep scenes
            // Do this for A, B, and C-type scenes
            // Ensure null is returned or an exception is thrown if no monsters exist

            // Setup
            BattleScene scene = new BattleScene(1, 0, 0, "A");
            List<string> monNames;
            List<Monster> monList = new List<Monster>();

            // Add 8 Monsters to the scene
            monNames = new List<string> { "Balloon", "Balloon", "Changer", "Changer", "DeadHead", "DeadHead", "Eagle", "Eagle" };
            foreach (string name in monNames)
                monList.Add(MonsterManager.GetMonsterByName(name));
            scene.PopulateScene(monList);

            Assert.AreEqual(8, scene.GetAllMonsters().Length);
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
            Assert.AreEqual(0, scene.GetAllMonsters().Length);
            Assert.IsNull(scene.GetAnySingleTarget());
            Assert.IsNull(scene.GetFrontRowTarget());

            // Add 6 Monsters to the scene
            monNames = new List<string> { "ElecFish", "ElecFish", "G.Goblin", "G.Goblin", "Grenade", "Grenade" };
            foreach (string name in monNames)
                monList.Add(MonsterManager.GetMonsterByName(name));
            scene.PopulateScene(monList);

            Assert.AreEqual(6, scene.GetAllMonsters().Length);
            for (int i = 0; i < 1000; i++)
            {
                Monster anyMonster = scene.GetAnySingleTarget();
                Assert.IsNotNull(scene.GetAnySingleTarget());
                Assert.IsTrue(monNames.Contains(anyMonster.Name));

                Monster frontMonster = scene.GetFrontRowTarget();
                Assert.IsTrue(frontMonster.Name == "G.Goblin" || frontMonster.Name == "Grenade");
            }
        }

        [TestMethod]
        public void SceneB_GetFrontRowTargetTest()
        {
            // Populate a scene with a valid monster list
            // Ensure only front-row monsters are returned
            // Do this for 4, 3, 2, and 1 column-deep scenes
            // Do this for A, B, and C-type scenes
            // Ensure null is returned or an exception is thrown if no monsters exist
        }

        [TestMethod]
        public void SceneC_GetFrontRowTargetTest()
        {
            // Populate a scene with a valid monster list
            // Ensure only front-row monsters are returned
            // Do this for 4, 3, 2, and 1 column-deep scenes
            // Do this for A, B, and C-type scenes
            // Ensure null is returned or an exception is thrown if no monsters exist
        }

        [TestMethod]
        public void SceneCreationTest()
        {
            // Test Defaults
            BattleScene scene = new BattleScene(1, 0, 0, "A");

            Assert.AreEqual(0, scene.X);
            Assert.AreEqual(0, scene.Y);
            Assert.AreEqual(0, scene.GetAllMonsters().Length);
            Assert.IsNull(scene.GetAnySingleTarget());
            Assert.IsNull(scene.GetFrontRowTarget());
        }
    }
}
