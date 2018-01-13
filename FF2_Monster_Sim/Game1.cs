using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FF2_Monster_Sim
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        // Monster stuff
        BattleScene sceneOne, sceneTwo;

        // Turn Logic
        private int turn = 0, round = 0;
        private Thread combatThread;
        // private int gameTick = 150, teardownTick = 100;
        private int gameTick = 150, teardownTick = 100;

        // Graphics
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D gameBackground;
        private SpriteFont font;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1008;
            graphics.PreferredBackBufferHeight = 604;
            graphics.ApplyChanges();
        }

        // Scene Layout


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            MonsterManager.Initialize();
            SpellManager.Initialize();
            AttackManager.Initialize();
            SoundManager.Initialize();
            TextManager.Initialize(360, 413);

            sceneOne = new BattleScene(50, 139);
            sceneTwo = new BattleScene(665, 139, "B", true);

            combatThread = new Thread(CombatLoop);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // TODO: use this.Content to load your game content here
            MonsterManager.LoadContent();
            SpellManager.LoadContent();
            SoundManager.LoadContent(Content);
            
            // TODO: Loading monsters should be handled by the scene itself?
            // Or, should monsters be generated and supplied to the scene as it is currently?

            List<Monster> sceneOneMonsters = new List<Monster>();
            foreach (string name in MonsterManager.GenerateMonsterList("A"))
            {
                Monster monster = MonsterManager.GetMonsterByName(name);
                if (monster == null)
                    continue;

                monster.Initialize(Content.Load<Texture2D>("Graphics\\Monsters\\" + monster.Name));
                monster.scene = sceneOne;
                sceneOneMonsters.Add(monster);
            }
            sceneOne.PopulateScene(sceneOneMonsters);

            List<Monster> sceneTwoMonsters = new List<Monster>();
            List<string> monNames = new List<string> { "Molbor", "Molbor", "Worm", "Worm", "DthRider", "DthRider" };
            // foreach (string name in MonsterManager.GenerateMonsterList("B"))
            foreach (string name in monNames)
                {
                Monster monster = MonsterManager.GetMonsterByName(name);
                if (monster == null)
                    continue;

                monster.Initialize(Content.Load<Texture2D>("Graphics\\Monsters\\" + monster.Name), true);
                monster.scene = sceneTwo;
                sceneTwoMonsters.Add(monster);
            }
            sceneTwo.PopulateScene(sceneTwoMonsters);

            // Graphics
            gameBackground = Content.Load<Texture2D>("Graphics\\GameArea");
            font = Content.Load<SpriteFont>("Graphics/Font");
            
            // Text Manager
            Texture2D[] textures = 
            {
                Content.Load<Texture2D>("Graphics\\ActorBox"),
                Content.Load<Texture2D>("Graphics\\DmgHitBox"),
                Content.Load<Texture2D>("Graphics\\ActorBox"),
                Content.Load<Texture2D>("Graphics\\DmgHitBox"),
                Content.Load<Texture2D>("Graphics\\ResultsBox")
            };
            TextManager.LoadContent(textures.ToArray(), font);

            // Threading
            combatThread.Start();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            sceneOne.UpdateSceneText(1);
            sceneTwo.UpdateSceneText(2);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(gameBackground, new Vector2(), Color.White);
            sceneOne.Draw(spriteBatch);
            sceneTwo.Draw(spriteBatch);
            TextManager.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        ////////////////
        // Game Logic //
        ////////////////

        private void CombatLoop()
        {
            // TODO: Sanity check. Ensure that battles don't run on forever. Enforce a round limit
            // Perhaps 100-200? If monsters heal their targets more than they can damage, gotta stop it.

            SoundManager.PlayBattleMusic();
            Thread.Sleep(2000);

            while (true)
            {
                round++;
                Debug.WriteLine("Round: " + round);
                turn = 0;
                foreach (Action action in GetSortedActionArray())
                {
                    turn++;
                    Debug.WriteLine("\nTurn: " + turn);

                    // If an actor was killed before it's turn, ignore the turn
                    if (action.Actor == null || action.Actor.IsDead())
                        continue;

                    foreach (Monster target in action.Targets)
                    {
                        // Ignore invalid target and spells against the dead
                        if (target == null)
                            continue;

                        if (action.Spell != null && target.IsDead())
                            continue;

                        TextManager.SetActorText(action.Actor.Name);
                        Thread.Sleep(gameTick);

                        if (action.Nothing)
                        {
                            TextManager.SetResultsText("Nothing");
                            continue;
                        }

                        TextManager.SetTargetText(target.Name);
                        Thread.Sleep(gameTick);

                        if (action.Physical)
                        {
                            // Physical attacks can target the dead, but are ineffective
                            if (target.IsDead())
                            {
                                TextManager.SetResultsText("Ineffective");
                                continue;
                            }

                            // Apply the attack and display the results
                            AttackResult atkRes = AttackManager.AttackMonster(action.Actor, target);

                            if (string.Equals(atkRes.DamageMessage, "Miss"))
                            {
                                TextManager.SetDamageText("Miss");
                                continue;
                            }

                            TextManager.SetHitsText(atkRes.HitsMessage);
                            Thread.Sleep(gameTick);

                            TextManager.SetDamageText(atkRes.DamageMessage);
                            Thread.Sleep(gameTick);

                            // Display each result, tearing down existing results as needed
                            for (int i = 0; i < atkRes.Results.Count; i++)
                            {
                                string res = atkRes.Results[i];
                                TextManager.SetResultsText(res);
                                if (i < atkRes.Results.Count - 1)
                                {
                                    Thread.Sleep(gameTick * 2);
                                    TextManager.TearDownResults();
                                    Thread.Sleep(teardownTick);
                                }
                            }
                            
                            break;
                        }
                        else
                        {
                            // Casting a spell
                            TextManager.SetHitsText(action.Spell.Name + " " + action.SpellLevel);
                            Thread.Sleep(gameTick);

                            // Cast the spell and display the results
                            SpellResult spellRes = SpellManager.CastSpell(action.Actor, target, action.Spell, action.SpellLevel, action.Targets.Count > 1); // TODO: Multi check

                            if (spellRes.Damage >= 0)
                            {
                                TextManager.SetDamageText(spellRes.Damage.ToString());
                                Thread.Sleep(gameTick);
                            }

                            // Display each result, tearing down existing results as needed
                            for (int i = 0; i < spellRes.Results.Count; i++)
                            {
                                string res = spellRes.Results[i];
                                TextManager.SetResultsText(res);
                                if (i < spellRes.Results.Count - 1)
                                {
                                    Thread.Sleep(gameTick * 2);
                                    TextManager.TearDownResults();
                                    Thread.Sleep(teardownTick);
                                }
                            }

                            // Tear down between each target
                            Thread.Sleep(gameTick * 2);
                            while (TextManager.TearDownText())
                                Thread.Sleep(teardownTick);
                        }
                    }
                    // Turn end, clean up text display
                    Thread.Sleep(gameTick * 2);
                    while (TextManager.TearDownText())
                        Thread.Sleep(teardownTick);

                    // Check for end of battle
                    if (!sceneOne.HasLivingMonsters() || !sceneTwo.HasLivingMonsters())
                    {
                        Debug.WriteLine("Battle over");
                        SoundManager.PlayVictoryMusic();

                        // TODO: Write battle results data to a file, or database, or something
                        // Just log it and determine how to store said data.

                        return;
                    }
                }
                Debug.WriteLine("Round end");
            }
        }

        /////////////
        // Helpers //
        /////////////

        /// <summary>
        /// Get and sort both scene's actions and return it
        /// </summary>
        /// <returns>Both Scene's actions, sorted by actor's Init</returns>
        private Action[] GetSortedActionArray()
        {
            // Build a list of all actions generated by both scenes
            List<Action> actList = new List<Action>();
            foreach (Action act in sceneOne.GetMonsterActions(sceneTwo))
                actList.Add(act);
            foreach (Action act in sceneTwo.GetMonsterActions(sceneOne))
                actList.Add(act);

            // Sort by actor's Init rolls and return the new array
            return actList.OrderBy(act => act.Actor.Init).ToArray();
        }
    }
}
