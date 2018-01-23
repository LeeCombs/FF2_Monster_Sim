using System;
using System.Linq;
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
            // Test valid inputs
            BattleScene scene = new BattleScene(1, 0, 0);
            scene.PopulateScene("A;Balloon-Balloon-Mine-Mine-Bomb-Bomb-Grenade-Grenade", null);
            scene.PopulateScene("B;Soldier-Soldier-Soldier-Soldier-Soldier-Soldier", null);
            scene.PopulateScene("C;Behemoth", null);

            // TODO: Test invalid inputs
        }

        [TestMethod]
        public void ClearSceneTest()
        {
            // Setup
            BattleScene scene = new BattleScene(1, 0, 0);
            scene.PopulateScene("A;Balloon-Balloon-Mine-Mine-Bomb-Bomb-Grenade-Grenade", null);
            Assert.IsTrue(scene.HasLivingMonsters());
            Assert.AreEqual(8, scene.GetAllLiveMonsters().Length);

            // Ensure monsters are cleared as expected
            scene.ClearScene();
            Assert.IsFalse(scene.HasLivingMonsters());
            Assert.AreEqual(0, scene.GetAllLiveMonsters().Length);
        }

        [TestMethod]
        public void RemoveMonsterTest()
        {
            // Ensure a monster is removed properly from the scene
            /*
             * I dunno if this can be properly tested. The monster itself calls this
             * to remove itself from the scene. It works as it is now, but without the
             * monster reference, or being able to add the specific mosnter to the scene,
             * this test will go unfinished.
             */
        }

        [TestMethod]
        public void GetMonsterActionsTest()
        {
            // Populate a scene with a valid monster list
            // Generate the actions and ensure validity
            // Do this a few times

            // Setup
            BattleScene scene = new BattleScene(1, 0, 0);
            scene.PopulateScene("A;Balloon-Balloon-Mine-Mine-Bomb-Bomb-Grenade-Grenade", null);
        }

        [TestMethod]
        public void GetAllLiveMonstersTest()
        {
            // Setup
            BattleScene scene = new BattleScene(1, 0, 0);
            scene.PopulateScene("A;Balloon-Balloon-Mine-Mine-Bomb-Bomb-Grenade-Grenade", null);

            // Test just the names for now
            string[] names = new string[]{ "Balloon", "Mine", "Bomb", "Grenade" };
            foreach (Monster mon in scene.GetAllLiveMonsters())
                Assert.IsTrue(names.Contains(mon.Name));

            // Kill monsters and ensure they're not returned
            for (int i = 8; i > 0; i--)
            { 
                Assert.AreEqual(i, scene.GetAllLiveMonsters().Length);
                scene.GetAllLiveMonsters()[0].Kill();
            }
            Assert.AreEqual(0, scene.GetAllLiveMonsters().Length);
        }

        [TestMethod]
        public void HasLivingMonstersTest()
        {
            // Setup
            BattleScene scene = new BattleScene(0, 0, 0);
            Assert.IsFalse(scene.HasLivingMonsters());
            scene.PopulateScene("A;Balloon-Balloon-Mine-Mine-Bomb-Bomb-Grenade-Grenade", null);
            Assert.IsTrue(scene.HasLivingMonsters());

            // Remove the monsters and ensure false is returned
            scene.ClearScene();
            Assert.IsFalse(scene.HasLivingMonsters());
            scene.PopulateScene("A;Balloon-Balloon-Mine-Mine-Bomb-Bomb-Grenade-Grenade", null);
            Assert.IsTrue(scene.HasLivingMonsters());
            foreach (Monster mon in scene.GetAllLiveMonsters())
                mon.Kill();
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

            // Ensure the retrieved monster exists within the scene
            Monster[] monsterRef = scene.GetAllLiveMonsters();
            for (int i = 0; i < 1000; i++)
                Assert.IsTrue(monsterRef.Contains(scene.GetAnySingleTarget()));
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

            // Ensure only valid, front row, names are returned
            string[] names = new string[] { "Bomb", "Grenade" };
            for (int i = 0; i < 100; i++)
                Assert.IsTrue(names.Contains(scene.GetFrontRowTarget().Name));

            scene.PopulateScene("A;Balloon-Balloon-Mine-Mine-Bomb-Bomb", null);
            names = new string[] { "Mine", "Bomb" };
            for (int i = 0; i < 1000; i++)
                Assert.IsTrue(names.Contains(scene.GetFrontRowTarget().Name));

            scene.PopulateScene("A;Balloon-Balloon-Mine-Mine", null);
            names = new string[] { "Balloon", "Mine" };
            for (int i = 0; i < 1000; i++)
                Assert.IsTrue(names.Contains(scene.GetFrontRowTarget().Name));

            scene.PopulateScene("A;Balloon-Balloon", null);
            for (int i = 0; i < 1000; i++)
                Assert.IsTrue(String.Equals("Balloon", scene.GetFrontRowTarget().Name));

        }

        [TestMethod]
        public void SceneB_GetFrontRowTargetTest()
        {
            // Setup
            BattleScene scene = new BattleScene(0, 0, 0);
            scene.PopulateScene("B;Molbor-Molbor-Soldier-Soldier-Panther-Panther", null);

            // Ensure only valid, front row, names are returned
            string[] names = new string[] { "Soldier", "Panther" };
            for (int i = 0; i < 1000; i++)
                Assert.IsTrue(names.Contains(scene.GetFrontRowTarget().Name));

            scene.PopulateScene("B;Molbor-Molbor-Soldier-Soldier", null);
            names = new string[] { "Molbor", "Soldier" };
            for (int i = 0; i < 1000; i++)
                Assert.IsTrue(names.Contains(scene.GetFrontRowTarget().Name));

            scene.PopulateScene("B;Molbor-Molbor", null);
            for (int i = 0; i < 1000; i++)
                Assert.IsTrue(String.Equals("Molbor", (scene.GetFrontRowTarget().Name)));


        }

        [TestMethod]
        public void SceneC_GetFrontRowTargetTest()
        {
            // Setup
            BattleScene scene = new BattleScene(0, 0, 0);
            scene.PopulateScene("C;Behemoth", null);

            for (int i = 0; i < 1000; i++)
                Assert.IsTrue(String.Equals("Behemoth", scene.GetFrontRowTarget().Name));
        }

        [TestMethod]
        public void UpdateSceneTextTest()
        {
            // Ensure text is displayed properly based on a valid monster list...?
        }
    }
}
