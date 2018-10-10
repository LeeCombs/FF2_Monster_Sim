using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{
    public static class StatusSpriteManager
    {
        static List<StatusSprite> statusSprites = new List<StatusSprite>();

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
            foreach (StatusSprite s in statusSprites)
                s.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < statusSprites.Count; i++)
                if (statusSprites[i] != null)
                    statusSprites[i].Draw(spriteBatch);
        }

        /////////////
        // Publics //
        /////////////

        public static StatusSprite GetStatusSprite()
        {
            // Check for an available status sprite. If none are available, 
            // make and return a new one
            for (int i = 0; i < statusSprites.Count; i++)
            {
                StatusSprite statSpr = statusSprites[i];
                if (!statSpr.Visible)
                    return statSpr;
            }
            StatusSprite newStatSpr = new StatusSprite();
            newStatSpr.LoadContent(Content); // Revisit
            statusSprites.Add(newStatSpr);
            return newStatSpr;
        }
        
        public static void ClearSprites()
        {
            for (int i = 0; i < statusSprites.Count; i++)
                statusSprites[i].Visible = false;
        }

        /////////////
        // Helpers //
        /////////////
    }
}
