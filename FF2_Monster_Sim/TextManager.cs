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
        private Textbox[] textboxes;

        public TextManager()
        {
            //
        }

        public void Initialize(int x, int y)
        {
            this.x = x;
            this.y = y;

            actorBox = new Textbox();
            hitsBox = new Textbox();
            targetsBox = new Textbox();
            dmgBox = new Textbox();
            resultsBox = new Textbox();
            textboxes = new Textbox[]{ actorBox, hitsBox, targetsBox, dmgBox, resultsBox };

            for (int i = 0; i < 5; i++)
                textboxes[i].Initialize(new Vector2(x, y + (i * 50)));
        }

        public void LoadContent(Texture2D[] textures, SpriteFont font)
        {
            for (int i = 0; i < 5; i++)
                textboxes[i].LoadContent(textures[i], font);
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
