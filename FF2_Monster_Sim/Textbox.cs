using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{
    class Textbox
    {
        private Vector2 position;
        private Texture2D graphic;
        private SpriteFont font;
        public string text = "";

        public bool isVisible = true;

        public Textbox()
        {
        }

        //////////////
        // Monogame //
        //////////////

        public void Initialize(Vector2 position)
        {
            this.position = position;
        }

        public void LoadContent(Texture2D graphic, SpriteFont font)
        {
            this.graphic = graphic;
            this.font = font;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(graphic, position, Color.White);
            spriteBatch.DrawString(font, text, position, Color.White);
        }

        public void Update(GameTime gameTime)
        {
            //
        }
    }
}
