using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FF2_Monster_Sim
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        // Monster stuff
        BattleScene sceneOne, sceneTwo;
        List<string> sceneOneNames = new List<string> {
            "Goblin", "Goblin", "LegEater", "LegEater", "VmpThorn", "VmpThorn", "Hornet", "Hornet"
        };
        List<string> sceneTwoNames = new List<string> {
            "Molbor", "Sargeant", "Sargeant", "Sargeant", "General"
        };

        // Turn Logic
        int turn = 0, round = 0;
        Thread combatThread;
        private int gameTick = 250;

        // Graphics
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Manager(s)
        TextManager textManager;
        SpriteFont font;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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

            textManager = new TextManager();

            sceneOne = new BattleScene();
            sceneTwo = new BattleScene(type: "B", flipped: true);

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

            // Init Managers
            MonsterManager.LoadContent();
            SpellManager.LoadContent();

            // TODO: use this.Content to load your game content here

            List<Monster> sceneOneMonsters = new List<Monster>();
            for (int i = 0; i < sceneOneNames.Count; i++)
            {
                Monster monster = MonsterManager.GetMonsterByName(sceneOneNames[i]);
                if (monster == null)
                    continue;

                monster.Initialize(Content.Load<Texture2D>("Graphics\\Monsters\\" + monster.Name));
                monster.scene = sceneOne;
                sceneOneMonsters.Add(monster);
            }
            sceneOne.PopulateScene(sceneOneMonsters);

            List<Monster> sceneTwoMonsters = new List<Monster>();
            for (int i = 0; i < sceneTwoNames.Count; i++)
            {
                Monster monster = MonsterManager.GetMonsterByName(sceneTwoNames[i]);
                if (monster == null)
                    continue;

                monster.Initialize(Content.Load<Texture2D>("Graphics\\Monsters\\" + monster.Name), true);
                monster.scene = sceneTwo;
                sceneTwoMonsters.Add(monster);
            }
            sceneTwo.PopulateScene(sceneTwoMonsters);
            foreach (Monster m in sceneTwo.GetAllTargets())
                m.Position.X += 500;

            // Font
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
            textManager.Initialize(0, 250);
            textManager.LoadContent(textures.ToArray(), font);

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
            sceneOne.Draw(spriteBatch);
            sceneTwo.Draw(spriteBatch);
            textManager.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void CombatLoop()
        {
            while (true)
            {
                round++;
                Debug.WriteLine("Round: " + round);
                turn = 0;
                foreach (Action action in GetSortedActionArray())
                {
                    turn++;
                    Debug.WriteLine("\nTurn: " + turn);

                    if (action.Actor == null || action.Actor.IsDead())
                        continue;

                    foreach (Monster target in action.Targets)
                    {
                        if (target == null)
                        {
                            Debug.WriteLine("null target");
                            continue;
                        }

                        textManager.SetActorText(action.Actor.Name);
                        Thread.Sleep(gameTick);

                        if (action.Nothing)
                        {
                            textManager.SetResultsText("Nothing");
                            continue;
                        }

                        textManager.SetTargetText(target.Name);
                        Thread.Sleep(gameTick);

                        if (action.Physical)
                        {
                            if (target.IsDead())
                            {
                                textManager.SetResultsText("Ineffective");
                                continue;
                            }

                            AttackResult atkRes = AttackManager.AttackMonster(action.Actor, target);

                            if (string.Equals(atkRes.DamageMessage, "Miss"))
                            {
                                textManager.SetDamageText("Miss");
                                continue;
                            }

                            textManager.SetHitsText(atkRes.HitsMessage);
                            Thread.Sleep(gameTick);


                            textManager.SetDamageText(atkRes.DamageMessage);
                            Thread.Sleep(gameTick);

                            foreach (string res in atkRes.Results)
                            {
                                textManager.SetResultsText(res);
                                Thread.Sleep(gameTick);
                            }
                            break;
                        }
                        else
                        {
                            // Casting a spell
                            textManager.SetHitsText("Spell");
                            Thread.Sleep(gameTick);
                        }
                    }
                    // Turn end
                    Thread.Sleep(gameTick*2);
                    textManager.SetActorText("");
                    textManager.SetTargetText("");
                    textManager.SetHitsText("");
                    textManager.SetDamageText("");
                    textManager.SetResultsText("");
                    Thread.Sleep(gameTick);
                }
                Debug.WriteLine("Round end");


                if (sceneOne.GetLiveCount() == 0)
                {
                    Debug.WriteLine("Scene two wins!");
                    break;
                }

                if (sceneTwo.GetLiveCount() == 0)
                {
                    Debug.WriteLine("Scene one wins!");
                    break;
                }
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
