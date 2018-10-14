using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{
    public static class MagicSpriteManager
    {
        static List<MagicSprite> magicSprites = new List<MagicSprite>();
        static ContentManager Content;

        //////////////
        // Monogame //
        //////////////

        public static void Initialize()
        {
            //
        }

        public static void LoadContent(ContentManager content)
        {
            Content = content;
        }

        public static void Update(GameTime gameTime)
        {
            foreach (MagicSprite s in magicSprites)
                s.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < magicSprites.Count; i++)
                if (magicSprites[i] != null)
                    magicSprites[i].Draw(spriteBatch);
        }

        /////////////
        // Publics //
        /////////////

        public static MagicSprite GetMagicSprite()
        {
            Debug.WriteLine("GetMagicSprite");

            // Check for an available status sprite. If none are available, 
            // make and return a new one
            for (int i = 0; i < magicSprites.Count; i++)
            {
                MagicSprite statSpr = magicSprites[i];
                if (!statSpr.Visible)
                    return statSpr;
            }

            MagicSprite newStatSpr = new MagicSprite();
            newStatSpr.LoadContent(Content); // Revisit
            magicSprites.Add(newStatSpr);
            return newStatSpr;
        }

        public static void GenerateSpellBurst(int x, int y, int width, int height, MagicSprite.MagicAnimation anim)
        {
            // Create ~6 spell sprites within bounds randomly and play them
            Debug.WriteLine("Generate burst " + x + ", " + y + ", " + width + ", " + height + ", " + anim.ToString());

            for (int i = 0; i < 6; i++)
            {
                int xPos = Globals.rnd.Next(x, x + width - MagicSprite.SPRITE_WIDTH);
                int yPos = Globals.rnd.Next(y, y + height - MagicSprite.SPRITE_HEIGHT);
                float delay = (float)Globals.rnd.NextDouble() * 100; // 0 - 100 ms


                MagicSprite mspr = GetMagicSprite();
                mspr.SetAnimation(anim, delay);
                mspr.SetPosition(new Vector2(xPos, yPos));
            }
        }

        public static void ClearSprites()
        {
            for (int i = 0; i < magicSprites.Count; i++)
                magicSprites[i].Visible = false;
        }

        /////////////
        // Helpers //
        /////////////
    }
}
