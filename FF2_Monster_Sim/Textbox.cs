using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{
    public class Textbox
    {
        private Vector2 position;
        private Texture2D graphic;
        private SpriteFont font;

        public bool IsVisible = false;
        public string Text = "";

        public Textbox()
        {
            //
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
            if (IsVisible)
            {
                spriteBatch.Draw(graphic, position, Color.White);
                Vector2 pos = new Vector2(position.X + 15, position.Y + 30);
                spriteBatch.DrawString(font, Text, pos, Color.White);
            }
        }

        public void Update(GameTime gameTime)
        {
            //
        }

        /////////////
        // Publics //
        /////////////

        /////////////
        // Helpers //
        /////////////
    }
}
