using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MySql.Data.MySqlClient;

namespace FF2_Monster_Sim
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        // MonoMonster stuff
        BattleScene sceneOne, sceneTwo;

        // Turn Logic
        public bool SpeedUp = true;
        private const int FANFARE_TIMER = 17000; // 17000 for one loop, 30000 for two
        private const int INTERLUDE_TIMER = 1350; // ~6500 per loop
        private const int GAME_TICK = 150;
        private const int TEARDOWN_TICK = 100;
        private const int ROUND_LIMIT = 100;
        private const int SPELL_ANIMATION_DELAY = 100; // 700
        private int turn = 0, turnTotal = 0, round = 0, gameTick = 0, teardownTick = 0;
        private Thread combatThread;

        // Graphics
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D gameBackground;
        private SpriteFont font;

        private Texture2D bg1, bg2;
        string[] bgs = new string[] {
                "Castle", "Cave", "Cyclone", "Desert", "Dreadnought", "Forest",
                "Leviathan", "MysidianTower", "Pandaemonium1", "Pandaemonium2",
                "Plains", "Sea", "Snow", "SnowCavern", "Swamp", "TropicalIsle"
            };

        // Teams
        private TeamManager.Team teamOne, teamTwo;

        // Saving battle results
        private const string RESULTS_PATH = @".\Content\Data\FF2_BattleResults.txt";

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
            MonsterManager.Initialize();
            SpellManager.Initialize();
            AttackManager.Initialize();
            SoundManager.Initialize();
            TextManager.Initialize(360, 413);
            TeamManager.Initialize();

            sceneOne = new BattleScene(1, 50, 130); // y was 139
            sceneOne.Initialize();

            sceneTwo = new BattleScene(2, 665, 130, true);
            sceneTwo.Initialize();

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

            // Graphics
            gameBackground = Content.Load<Texture2D>("Graphics\\GameArea");
            font = Content.Load<SpriteFont>("Graphics\\Font");

            // Managers
            MonsterManager.LoadContent();
            SpellManager.LoadContent();
            SoundManager.LoadContent(Content);
            TextManager.LoadContent(Content, font);
            TeamManager.LoadContent();
            StatusSpriteManager.LoadContent(Content);
            MagicSpriteManager.LoadContent(Content);

            // Populate the scenes with random monsters
            PopulateScenes();

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
            
            sceneOne.UpdateSceneText();
            sceneTwo.UpdateSceneText();
            sceneOne.Update(gameTime);
            sceneTwo.Update(gameTime);

            // Update Managers
            StatusSpriteManager.Update(gameTime);
            MagicSpriteManager.Update(gameTime);
            TeamManager.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            spriteBatch.Begin();
            spriteBatch.Draw(gameBackground, new Vector2(), Color.White);
            spriteBatch.Draw(bg1, new Vector2(), Color.White);
            spriteBatch.Draw(bg2, new Vector2(512, 0), Color.White);
            sceneOne.Draw(spriteBatch);
            sceneTwo.Draw(spriteBatch);

            TextManager.Draw(spriteBatch);
            StatusSpriteManager.Draw(spriteBatch);
            MagicSpriteManager.Draw(spriteBatch);

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
                // Setup the match, execute the rounds, then record the results
                SetupMatch();
                ExecuteRounds();
                WriteBattleResults();

                // Display results info and play some music. If a boss won, play different music.
                if (sceneOne.SceneType == SceneType.C && sceneOne.HasLivingMonsters())
                    SoundManager.PlayMusic(SoundManager.Music.Defeat);
                if (sceneTwo.SceneType == SceneType.C && sceneTwo.HasLivingMonsters())
                    SoundManager.PlayMusic(SoundManager.Music.Defeat);
                else
                    SoundManager.PlayMusic(SoundManager.Music.Victory);

                // Build and display the text of which team won and their stats
                String infoText = "";
                if (sceneOne.HasLivingMonsters() && sceneTwo.HasLivingMonsters())
                {
                    infoText = "Tie! Everyone loses.";
                }
                else if (sceneOne.HasLivingMonsters())
                {
                    infoText = "Team " + teamOne.TeamName + "\nwas victorious!!\n\n";
                    infoText += TeamManager.GetTeamInfo(teamOne);
                }
                else if (sceneTwo.HasLivingMonsters())
                {
                    infoText = "Team " + teamTwo.TeamName + "\nwas victorious!!\n\n";
                    infoText += TeamManager.GetTeamInfo(teamTwo);
                }
                TextManager.SetInfoText(infoText);
                

                if (SpeedUp)
                    Thread.Sleep(100);
                else
                    Thread.Sleep(FANFARE_TIMER);

                // Cleanup and setup for the next battle
                round = turn = turnTotal = 0;
                TextManager.Clear();
                PopulateScenes();
            }
        }

        /////////////
        // Helpers //
        /////////////

        /// <summary>
        /// Setup the two teams and battle scenes for the next battle
        /// </summary>
        private void PopulateScenes()
        {
            // Cleanup scenes
            sceneOne.ClearScene();
            sceneTwo.ClearScene();

            // Set the background image randomly
            string bgstr = bgs[Globals.rnd.Next(bgs.Length)];
            bg1 = Content.Load<Texture2D>("Graphics\\Backdrops\\" + bgstr);
            bg2 = Content.Load<Texture2D>("Graphics\\Backdrops\\" + bgstr);

            // Team one is always a pre-defined team
            teamOne = TeamManager.GetRandomTeam();
            sceneOne.PopulateScene(teamOne.TeamString, Content);
            TextManager.SetTeamName(1, teamOne.TeamName);

            // Sometimes pick a pre-defined team for scene two, but usually just generate one
            if (Globals.rnd.Next(100) < 20)
            {
                // Don't pit a team against itself
                do
                    teamTwo = TeamManager.GetRandomTeam();
                while (teamOne.TeamIndex == teamTwo.TeamIndex);
            }
            else
            {
                // Grab the "random" team and overwrite its team string
                teamTwo = TeamManager.GetTeamByIndex(0);
                teamTwo.TeamString = GenerateRandomSceneString();
            }

            sceneTwo.PopulateScene(teamTwo.TeamString, Content);
            TextManager.SetTeamName(2, teamTwo.TeamName);
        }

        /// <summary>
        /// Generate and retrieve a random Scene string
        /// </summary>
        private string GenerateRandomSceneString()
        {
            string[] alph = new string[] { "A", "A", "B", "B", "B", "B", "C" };
            string rndchar = alph[Globals.rnd.Next(0, alph.Length)];
            return rndchar + ";" + String.Join("-", MonsterManager.GenerateMonsterList(rndchar));
        }

        /// <summary>
        /// Write results of the battle and update team data
        /// </summary>
        private void WriteBattleResults()
        {
            // Build the match results string and save it
            // Example results string: "1,9,31,C;Emperor_2,B;BigHorn-Mantis-Icicle-VmpGirl-Gigas"
            int winner = sceneOne.HasLivingMonsters() ? 1 : 2;
            if (round >= ROUND_LIMIT)
                winner = 0;
            
            // Build and save the match results
            string matchResults = winner.ToString() + "," + round.ToString() + "," + turnTotal.ToString() + "," + sceneOne.SceneString + "," + sceneTwo.SceneString;
            using (StreamWriter file = new StreamWriter(RESULTS_PATH, true))
                file.WriteLine(matchResults);

            // Write battle results to db
            var db = DBConnection.Instance();
            db.DatabaseName = "ff2_monster_sim";
            db.Password = Environment.GetEnvironmentVariable("FFSIM_MYSQL_PASSWORD", EnvironmentVariableTarget.User);
            if (db.IsConnected())
            {
                MySqlConnection conn = db.Connection;

                string sql = "INSERT INTO battle_results (winning_team, num_of_rounds, num_of_turns, team_one_string, team_two_string, entry_date) " +
                "VALUES (@winning_team, @num_of_rounds, @num_of_turns, @team_one_string, @team_two_string, @entry_date);";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@winning_team", winner);
                cmd.Parameters.AddWithValue("@num_of_rounds", round);
                cmd.Parameters.AddWithValue("@num_of_turns", turnTotal);
                cmd.Parameters.AddWithValue("@team_one_string", sceneOne.SceneString);
                cmd.Parameters.AddWithValue("@team_two_string", sceneTwo.SceneString);
                cmd.Parameters.AddWithValue("@entry_date", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
            
            // Update team data
            // Wtf is this?
            int winStateOne, winStateTwo;
            if (winner == 0)
                winStateOne = winStateTwo = 2;
            else if (winner == 1)
            {
                winStateOne = 1;
                winStateTwo = 0;
            }
            else
            {
                winStateOne = 0;
                winStateTwo = 1;
            }
            TeamManager.UpdateTeamData(teamOne, round, winStateOne);
            TeamManager.UpdateTeamData(teamTwo, round, winStateTwo);
            TeamManager.WriteTeamData();

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
        

        private void SetupMatch()
        {
            // Reset speeds
            if (SpeedUp)
            {
                gameTick = 10;
                teardownTick = 10;
            }
            else
            {
                gameTick = GAME_TICK;
                teardownTick = TEARDOWN_TICK;
            }

            // Display pre-game and countdown
            SoundManager.PlayMusic(SoundManager.Music.Menu);
            for (int i = 0; i <= INTERLUDE_TIMER; i += 1000)
            {
                TextManager.SetInfoText("Starting in " + (INTERLUDE_TIMER - i) / 1000 + "...");
                Thread.Sleep(10); // 1000
            }

            // Clean up the text and play some music
            while (TextManager.TearDownText()) ;
            if (sceneOne.SceneType == SceneType.C || sceneTwo.SceneType == SceneType.C)
                SoundManager.PlayMusic(SoundManager.Music.Boss);
            else
                SoundManager.PlayMusic(SoundManager.Music.Battle);
        }

        private void ExecuteRounds()
        {
            while (true)
            {
                // Update and display round number
                TextManager.SetRoundText(++round);

                // Speed up the battle every round
                if (gameTick >= 10)
                    gameTick -= 10;
                if (teardownTick >= 15)
                    teardownTick -= 15;

                // Apply every action
                while (ExecuteActions());
                
                // Check for end of battle
                if (!sceneOne.HasLivingMonsters() || !sceneTwo.HasLivingMonsters() || round >= ROUND_LIMIT)
                    break;

                // Check if any monster recovers from temporary status effects
                foreach (MonoMonster mon in sceneOne.GetAllLiveMonsters())
                    mon.RollTempStatusRecovery();
                foreach (MonoMonster mon in sceneTwo.GetAllLiveMonsters())
                    mon.RollTempStatusRecovery();
            }
        }

        private bool ExecuteActions()
        {
            turn = 0;
            foreach (Action action in GetSortedActionArray())
            {
                // Update and display turn number
                turn++;
                turnTotal++;
                TextManager.SetTurnText(turn);

                // If an actor was killed before its turn, ignore the turn
                if (action.Actor == null || action.Actor.IsDead())
                    continue;

                // Apply the action to each target
                foreach (MonoMonster target in action.Targets)
                {
                    // Ignore invalid target and spells against the dead
                    if (target == null || (action.Spell != null && target.IsDead()))
                        continue;

                    TextManager.SetActorText(action.Actor.Name);
                    Thread.Sleep(gameTick);

                    // If the action is nothing, display and bail out
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

                        // On a miss, display and bail out
                        if (string.Equals(atkRes.DamageMessage, "Miss"))
                        {
                            TextManager.SetDamageText("Miss");
                            continue;
                        }

                        // Flicker sprite
                        // TODO: Different sounds and animations need to play based on the attack type
                        if (gameTick > 30)
                        {
                            SoundManager.PlaySound(SoundManager.Sound.Physical);
                            target.Flicker(16);
                        }

                        TextManager.SetHitsText(atkRes.HitsMessage);
                        Thread.Sleep(gameTick);

                        TextManager.SetDamageText(atkRes.DamageMessage);
                        Thread.Sleep(gameTick);
                        
                        // Fade the monster if dead
                        // TODO: This is awkward here
                        if (target.IsDead())
                        {
                            target.StartDeath();
                            Thread.Sleep(30); // 300
                        }

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
                        SpellResult spellRes = SpellManager.CastSpell(action.Actor, target, action.Spell, action.SpellLevel, action.Targets.Count > 1);
                        
                        // Set the spell animation and sound based on the spell's effect (kinda). This is awkward but fine for now.
                        MagicSprite.MagicAnimation magicAnim = MagicSprite.MagicAnimation.Attack;
                        SoundManager.Sound magicSnd = SoundManager.Sound.AttackSpell;
                        if (action.Spell.Effect == "Buff")
                        {
                            magicAnim = MagicSprite.MagicAnimation.Buff;
                            magicSnd = SoundManager.Sound.Buff;
                        }
                        if (action.Spell.Effect == "Debuff" || action.Spell.Effect == "TempStatus" || action.Spell.Effect == "PermStatus")
                        {
                            magicAnim = MagicSprite.MagicAnimation.Debuff;
                            magicSnd = SoundManager.Sound.Debuff;

                        }
                        if (action.Spell.Effect == "Heal" || action.Spell.Effect == "Revive" || action.Spell.Effect == "ItemFullRestore")
                        {
                            magicAnim = MagicSprite.MagicAnimation.Heal;
                            magicSnd = SoundManager.Sound.Heal;
                        }

                        MagicSpriteManager.GenerateSpellBurst((int)target.Position.X, (int)target.Position.Y, target.Width, target.Height, magicAnim);
                        SoundManager.PlaySound(magicSnd);
                        Thread.Sleep(SPELL_ANIMATION_DELAY);

                        if (spellRes.Damage >= 0)
                        {
                            TextManager.SetDamageText(spellRes.Damage.ToString());
                            Thread.Sleep(gameTick);
                        }

                        // Kill the target if it's dead.
                        // TODO: This is weird here, should be handled by the monster itself?
                        if (target.IsDead())
                        {
                            target.StartDeath();
                            Thread.Sleep(300);
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
                        Thread.Sleep(gameTick * 5);
                        while (TextManager.TearDownText())
                            Thread.Sleep(teardownTick);
                    }

                    // TODO: If both scenes only contain "Soul" enemies, they cannot kill eachother
                    if (!sceneOne.HasLivingMonsters() || !sceneTwo.HasLivingMonsters() || round >= ROUND_LIMIT)
                        break;
                }

                // Turn end, clean up text display
                Thread.Sleep(gameTick * 5);
                while (TextManager.TearDownText())
                    Thread.Sleep(teardownTick);

                if (!sceneOne.HasLivingMonsters() || !sceneTwo.HasLivingMonsters() || round >= ROUND_LIMIT)
                    break;
            }

            return false;
        }
    }
}
