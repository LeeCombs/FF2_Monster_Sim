using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace FF2_Monster_Sim
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        // Monster stuff
        BattleScene sceneOne, sceneTwo;
        List<string> sceneOneNames = new List<string> { "Imp", "Eagle", "Goblin", "Balloon", "Bomb", "Brain", "Coctrice", "DesAngel" };
        List<string> sceneTwoNames = new List<string> { "Molbor", "IceLiz", "G.Toad", "GrOgre", "Gigas", };

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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

            sceneOne = new BattleScene();
            sceneTwo = new BattleScene(type: "B", flipped: true);

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
                if (monster == null) continue;
                monster.Initialize(Content.Load<Texture2D>("Graphics\\Monsters\\" + monster.Name));
                sceneOneMonsters.Add(monster);
            }
            sceneOne.PopulateScene(sceneOneMonsters);

            List<Monster> sceneTwoMonsters = new List<Monster>();
            for (int i = 0; i < sceneTwoNames.Count; i++)
            {
                Monster monster = MonsterManager.GetMonsterByName(sceneTwoNames[i]);
                if (monster == null) continue;

                monster.Initialize(Content.Load<Texture2D>("Graphics\\Monsters\\" + monster.Name), true);
                sceneTwoMonsters.Add(monster);
            }
            sceneTwo.PopulateScene(sceneTwoMonsters);
            foreach (Monster m in sceneTwo.GetAllTargets())
                m.Position.X += 500;
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
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
