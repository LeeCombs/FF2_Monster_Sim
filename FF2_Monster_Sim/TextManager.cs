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
        private Textbox actorBox, hitsBox, targetsBox, dmgBox, resultsBox;

        public TextManager()
        {
            //
        }

        public void Initialize(int x, int y, Texture2D[] textures, SpriteFont font)
        {
            this.x = x;
            this.y = y;
            actorBox = new Textbox();
            hitsBox = new Textbox();
            targetsBox = new Textbox();
            dmgBox = new Textbox();
            resultsBox = new Textbox();

            actorBox.Initialize(new   Vector2(x, y), textures[0], font);
            hitsBox.Initialize(new    Vector2(x, y + 50), textures[1], font);
            targetsBox.Initialize(new Vector2(x, y + 100), textures[2], font);
            dmgBox.Initialize(new     Vector2(x, y + 150), textures[3], font);
            resultsBox.Initialize(new Vector2(x, y + 200), textures[4], font);
        }

        public void LoadContent()
        {
            //
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            actorBox.Draw(spriteBatch);
            hitsBox.Draw(spriteBatch);
            targetsBox.Draw(spriteBatch);
            dmgBox.Draw(spriteBatch);
            resultsBox.Draw(spriteBatch);
        }
    }
}
