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
            BattleScene scene = new BattleScene(1, 0, 0);

            Assert.AreEqual(0, scene.X);
            Assert.AreEqual(0, scene.Y);
            Assert.AreEqual(0, scene.GetAllLiveMonsters().Length);
            Assert.ThrowsException<Exception>(() => scene.GetAnySingleTarget());
            Assert.ThrowsException<Exception>(() => scene.GetFrontRowTarget());
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
        public void RemoveMonsterTest()
        {
            // Ensure a monster is removed properly from the scene
        }

        [TestMethod]
        public void GetMonsterActionsTest()
        {
            // Populate a scene with a valid monster list
            // Generate the actions and ensure validity
            // Do this a few times
        }

        [TestMethod]
        public void GetAllLiveMonstersTest()
        {
            // Populate a scene with a valid monster list
            // Ensure that list is retrieved
        }

        [TestMethod]
        public void HasLivingMonstersTest()
        {
            // Test that a scene returns false before populating
            // Populate a scene with a valid monster list
            // Ensure that true is returned
            // Clear scene and ensure false is returned

            // Setup
            BattleScene scene = new BattleScene(0, 0, 0);
            Assert.IsFalse(scene.HasLivingMonsters());
            scene.PopulateScene("A;Balloon-Balloon-Mine-Mine-Bomb-Bomb-Grenade-Grenade", null);
            Assert.IsTrue(scene.HasLivingMonsters());
            scene.ClearScene();
            Assert.IsFalse(scene.HasLivingMonsters());
        }

        [TestMethod]
        public void GetAnySingleTargetTest()
        {
            // Populate a scene with a valid monster list
            // Ensure only a monster from the scene is returned
            // Ensure that a monster is actually returned if there is one
            // Ensure nothing is returned if no monsters exist

            // Setup
            BattleScene scene = new BattleScene(0, 0, 0);
            scene.PopulateScene("A;Balloon-Balloon-Mine-Mine-Bomb-Bomb-Grenade-Grenade", null);
        }

        [TestMethod]
        public void SceneA_GetFrontRowTargetTest()
        {
            // Populate a scene with a valid monster list
            // Ensure only front-row monsters are returned
            // Do this for 4, 3, 2, and 1 column-deep scenes
            // Ensure null is returned or an exception is thrown if no monsters exist

            // Setup
            BattleScene scene = new BattleScene(0, 0, 0);
            scene.PopulateScene("A;Balloon-Balloon-Mine-Mine-Bomb-Bomb-Grenade-Grenade", null);
        }

        [TestMethod]
        public void SceneB_GetFrontRowTargetTest()
        {
            // Setup
            BattleScene scene = new BattleScene(0, 0, 0);
            scene.PopulateScene("B;Molbor-Molbor-Soldier-Soldier-Panther-Panther", null);
        }

        [TestMethod]
        public void SceneC_GetFrontRowTargetTest()
        {
            // Setup
            BattleScene scene = new BattleScene(0, 0, 0);
            scene.PopulateScene("C;Behemoth", null);
        }

        [TestMethod]
        public void UpdateSceneTextTest()
        {
            // Ensure text is displayed properly based on a valid monster list...?
        }
    }
}
