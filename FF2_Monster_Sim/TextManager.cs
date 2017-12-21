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
        private Textbox actorBox, hitsBox, targetBox, dmgBox, resultsBox;
        private Textbox[] textboxes;
        private Vector2[] positions;

        private Stack<Textbox> textboxStack;

        public TextManager()
        {
            //
        }

        //////////////
        // Monogame //
        //////////////

        public void Initialize(int x, int y)
        {
            this.x = x;
            this.y = y;

            positions = new Vector2[] {
                new Vector2(x, y),
                new Vector2(x + 144, y),
                new Vector2(x, y + 50),
                new Vector2(x + 144, y + 50),
                new Vector2(x, y + 100)
            };

            actorBox = new Textbox();
            hitsBox = new Textbox();
            targetBox = new Textbox();
            dmgBox = new Textbox();
            resultsBox = new Textbox();
            textboxes = new Textbox[]{ actorBox, hitsBox, targetBox, dmgBox, resultsBox };

            for (int i = 0; i < 5; i++)
                textboxes[i].Initialize(positions[i]);

            textboxStack = new Stack<Textbox>();
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
            targetBox.Draw(spriteBatch);
            dmgBox.Draw(spriteBatch);
            resultsBox.Draw(spriteBatch);
        }

        /////////////
        // Publics //
        /////////////

        public void SetActorText(string text)
        {
            SetTextBox(actorBox, text);
        }

        public void SetTargetText(string text)
        {
            SetTextBox(targetBox, text);
        }

        public void SetHitsText(string text)
        {
            SetTextBox(hitsBox, text);
        }

        public void SetDamageText(string text)
        {
            SetTextBox(dmgBox, text);
        }

        public void SetResultsText(string text)
        {
            SetTextBox(resultsBox, text);
        }
        
        /// <summary>
        /// Removes and hides the results textbox
        /// </summary>
        public void TearDownResults()
        {
            TearDownTextBox(resultsBox);
        }

        /// <summary>
        /// Returns whether there is text to tear down, and tears it down
        /// </summary>
        public bool TearDownText()
        {
            if (textboxStack.Count == 0)
                return false;

            TearDownTextBox(textboxStack.Pop());
            return true;
        }
        
        /////////////
        // Helpers //
        /////////////
        
        /// <summary>
        /// Set a textbox's text, make it visuble, and add it to the stack
        /// </summary>
        private void SetTextBox(Textbox textbox, string text)
        {
            textbox.IsVisible = true;
            textbox.Text = text;
            textboxStack.Push(textbox);
        }

        /// <summary>
        /// Remove the textbox's text and make it invisible
        /// </summary>
        private void TearDownTextBox(Textbox textbox)
        {
            // Animate?
            textbox.IsVisible = false;
            textbox.Text = "";
        }
    }
}
