using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{

    class TextManager
    {
        private int x, y;
        private Texture2D actorBox, hitsBox, targetsBox, dmgBox, resultsBox;

        public TextManager()
        {
            //
        }

        public void Initialize(int x, int y, Texture2D[] textures)
        {
            this.x = x;
            this.y = y;
            actorBox = textures[0];
            targetsBox = textures[2];
            hitsBox = textures[1];
            dmgBox = textures[3];
            resultsBox = textures[4];
        }

        public void LoadContent()
        {
            //
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            SpriteEffects s = SpriteEffects.None;
            spriteBatch.Draw(actorBox,   new Vector2(x, y), null, Color.White, 0f, Vector2.Zero, 1f, s, 0f);
            spriteBatch.Draw(hitsBox,    new Vector2(x, y + 50), null, Color.White, 0f, Vector2.Zero, 1f, s, 0f);
            spriteBatch.Draw(targetsBox, new Vector2(x, y + 100), null, Color.White, 0f, Vector2.Zero, 1f, s, 0f);
            spriteBatch.Draw(dmgBox,     new Vector2(x, y + 150), null, Color.White, 0f, Vector2.Zero, 1f, s, 0f);
            spriteBatch.Draw(resultsBox, new Vector2(x, y + 200), null, Color.White, 0f, Vector2.Zero, 1f, s, 0f);
        }
    }
}
