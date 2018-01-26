﻿using System;
using System.IO;
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
        private const int ROUND_LIMIT = 100;
        private int turn = 0, turnTotal = 0, round = 0;
        private Thread combatThread;
        private int gameTick = 100, teardownTick = 150;
        private const int FANFARE_TIMER = 17000; // 17000 for one loop, 30000 for two
        private const int INTERLUDE_TIMER = 6500; // ~6500 per loop

        // Graphics
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D gameBackground;
        private SpriteFont font;

        // Teams
        private string teamOneName, teamTwoName;
        private int teamOneIndex, teamTwoIndex;

        // Saving battle results
        private const string RESULTS_FILE_PATH = @".\Content\Data\FF2_BattleResults.txt";

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1008;
            graphics.PreferredBackBufferHeight = 604;
            graphics.ApplyChanges();
        }
        
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

            sceneOne = new BattleScene(1, 50, 139);
            sceneTwo = new BattleScene(2, 665, 139, true);

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

            // Graphics
            gameBackground = Content.Load<Texture2D>("Graphics\\GameArea");
            font = Content.Load<SpriteFont>("Graphics/Font");
            
            // Text Manager
            // TODO: This should be moved into TextManager
            Texture2D[] textures = 
            {
                Content.Load<Texture2D>("Graphics\\ActorBox"),
                Content.Load<Texture2D>("Graphics\\DmgHitBox"),
                Content.Load<Texture2D>("Graphics\\ActorBox"),
                Content.Load<Texture2D>("Graphics\\DmgHitBox"),
                Content.Load<Texture2D>("Graphics\\ResultsBox"),
                Content.Load<Texture2D>("Graphics\\InfoBox")
            };
            TextManager.LoadContent(textures.ToArray(), font);
            
            // Populate the scenes with random monsters
            string[] teamOne = PickRandomTeam().Split(',');
            teamOneName = teamOne[1];
            teamOneIndex = int.Parse(teamOne[0]);
            TextManager.SetTeamName(1, teamOneName);
            sceneOne.PopulateScene(teamOne[2], Content, true);

            teamTwoName = "Random";
            TextManager.SetTeamName(2, teamTwoName);
            sceneTwo.PopulateScene(GenerateRandomSceneString(), Content);
            
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
            
            // TODO: Only drawing the text every frame for now. Should be redrawn when monsters take/heal damage.
            // sceneOne.UpdateSceneText();
            // sceneTwo.UpdateSceneText();

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
            while (true)
            {
                SoundManager.PlayMenuMusic();
                TextManager.SetInfoText("Starting in 14...");
                for (int i = 0; i <= INTERLUDE_TIMER; i += 1000)
                {
                    TextManager.SetInfoText("Starting in " + (INTERLUDE_TIMER - i)/1000 + "...");
                    Thread.Sleep(1000);
                }
                TextManager.TearDownText();

                if (sceneOne.SceneType == SceneType.C || sceneTwo.SceneType == SceneType.C)
                    SoundManager.PlayBossMusic();
                else
                    SoundManager.PlayBattleMusic();

                while (true)
                {
                    // Update and display round number
                    round++;
                    TextManager.SetRoundText(round);

                    turn = 0;
                    foreach (Action action in GetSortedActionArray())
                    {
                        // Update and display turn number
                        turn++;
                        turnTotal++;
                        TextManager.SetTurnText(turn);

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

                                // Testin
                                sceneOne.UpdateSceneText();
                                sceneTwo.UpdateSceneText();

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

                                // Testin
                                sceneOne.UpdateSceneText();
                                sceneTwo.UpdateSceneText();
                            }

                            // TODO: If both scenes only contain "Soul" enemies, they cannot kill eachother
                            if (!sceneOne.HasLivingMonsters() || !sceneTwo.HasLivingMonsters() || round >= ROUND_LIMIT)
                                break;
                        }
                        // Turn end, clean up text display
                        Thread.Sleep(gameTick * 2);
                        while (TextManager.TearDownText())
                            Thread.Sleep(teardownTick);
                        
                        if (!sceneOne.HasLivingMonsters() || !sceneTwo.HasLivingMonsters() || round >= ROUND_LIMIT)
                            break;
                    }

                    // End of round
                    
                    // Check for end of battle
                    if (!sceneOne.HasLivingMonsters() || !sceneTwo.HasLivingMonsters() || round >= ROUND_LIMIT)
                        break;

                    // Check if any monster recovers from temporary status effects
                    foreach (Monster mon in sceneOne.GetAllLiveMonsters())
                        MonsterManager.RollTempStatusRecovery(mon);
                    foreach (Monster mon in sceneTwo.GetAllLiveMonsters())
                        MonsterManager.RollTempStatusRecovery(mon);
                }

                // Record the battle info
                WriteBattleResults();

                // Play music based on who won or lost, and based on scene type
                if (!sceneOne.HasLivingMonsters() && sceneTwo.SceneType == SceneType.C)
                {
                    SoundManager.PlayDefeatMusic();
                    TextManager.SetInfoText("Team " + teamOneName + "\nwas defeated...");
                }
                else if (!sceneTwo.HasLivingMonsters() && sceneOne.SceneType == SceneType.C)
                {
                    SoundManager.PlayDefeatMusic();
                    TextManager.SetInfoText("Team " + teamTwoName + "\nwas defeated...");
                }
                else
                {
                    SoundManager.PlayVictoryMusic();
                    if (sceneOne.HasLivingMonsters())
                        TextManager.SetInfoText("Team " + teamOneName + "\nwas victorious!!");
                    else
                        TextManager.SetInfoText("Team " + teamTwoName + "\nwas victorious!!");
                }
                Thread.Sleep(FANFARE_TIMER);

                // Cleanup scenes and setup for next battle
                sceneOne.ClearScene();
                sceneTwo.ClearScene();
                round = turn = turnTotal = 0;
                TextManager.Clear();

                // Repopulate scenes
                string[] teamOne = PickRandomTeam().Split(',');
                teamOneName = teamOne[1];
                teamOneIndex = int.Parse(teamOne[0]);
                TextManager.SetTeamName(1, teamOneName);
                sceneOne.PopulateScene(teamOne[2], Content, true);

                teamTwoName = "Random";
                TextManager.SetTeamName(2, teamTwoName);
                sceneTwo.PopulateScene(GenerateRandomSceneString(), Content);
            }
        }

        /////////////
        // Helpers //
        /////////////

        private string GenerateRandomSceneString()
        {
            string[] alph = new string[] { "A", "A", "B", "B", "B", "B", "C" };
            string rndchar = alph[Globals.rnd.Next(0, alph.Length)];
            return rndchar + ";" + String.Join("-", MonsterManager.GenerateMonsterList(rndchar));
        }

        private string PickRandomTeam()
        {
            string path = @".\Content\Data\FF2_TeamData.csv";
            List<string> teamData = File.ReadAllLines(path).ToList();
            return teamData[Globals.rnd.Next(1, teamData.Count)];

            /*
            string[] str2 = str[1].Split(',');
            str[1] = string.Join(",", str2);
            File.WriteAllLines(path, str);
            */
        }

        private void WriteBattleResults()
        {
            // Build the output string and save it. 
            int winner = sceneOne.HasLivingMonsters() ? 1 : 2;
            if (round >= ROUND_LIMIT)
                winner = 0;
            string outstr = winner.ToString() + "," + round.ToString() + "," + turnTotal.ToString() + "," + sceneOne.SceneString + "," + sceneTwo.SceneString;

            Debug.WriteLine(outstr);
            using (StreamWriter file = new StreamWriter(RESULTS_FILE_PATH, true))
            {
                file.WriteLine(outstr);
            }

            string path = @".\Content\Data\FF2_TeamData.csv";
            List<string> teamData = File.ReadAllLines(path).ToList();
            if (sceneOne.IsTeam)
            {
                for (int i = 0; i < teamData.Count; i++)
                {
                    string data = teamData[i];
                    string[] dataSplit = data.Split(',');
                    if (string.Equals(teamOneIndex.ToString(), dataSplit[0]))
                    {
                        Debug.WriteLine(string.Join(",", dataSplit));

                        // TODO: This is fugly, fix it
                        if (sceneOne.HasLivingMonsters())
                        {
                            int wins = int.Parse(dataSplit[3]);
                            wins++;
                            dataSplit[3] = wins.ToString();
                        }
                        else if (sceneTwo.HasLivingMonsters())
                        {
                            int losses = int.Parse(dataSplit[4]);
                            losses++;
                            dataSplit[4] = losses.ToString();
                        }
                        else if (round >= ROUND_LIMIT)
                        {
                            int ties = int.Parse(dataSplit[5]);
                            ties++;
                            dataSplit[5] = ties.ToString();
                        }

                        int rounds = int.Parse(dataSplit[6]);
                        rounds += round;
                        dataSplit[6] = rounds.ToString();

                        Debug.WriteLine(string.Join(",", dataSplit));
                        teamData[i] = string.Join(",", dataSplit);
                        break;
                    }
                }
                File.WriteAllLines(path, teamData);
            }


        }

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
