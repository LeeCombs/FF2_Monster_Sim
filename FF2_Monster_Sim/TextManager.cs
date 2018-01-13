using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{

    public static class TextManager
    {
        private static int X, Y;
        private static Textbox actorBox, hitsBox, targetBox, dmgBox, resultsBox;
        private static Textbox[] textboxes;
        private static Vector2[] positions;

        private static Stack<Textbox> textboxStack;
        
        private static string sceneOneText;
        private static string sceneTwoText;

        private static SpriteFont spriteFont;
        
        //////////////
        // Monogame //
        //////////////

        public static void Initialize(int x, int y)
        {
            // Inits
            actorBox = new Textbox();
            hitsBox = new Textbox();
            targetBox = new Textbox();
            dmgBox = new Textbox();
            resultsBox = new Textbox();
            textboxes = new Textbox[]{ actorBox, hitsBox, targetBox, dmgBox, resultsBox };
            textboxStack = new Stack<Textbox>();

            // Setup positioning
            X = x;
            Y = y;

            positions = new Vector2[] {
                new Vector2(x, y),
                new Vector2(x + 144, y),
                new Vector2(x, y + 64),
                new Vector2(x + 144, y + 64),
                new Vector2(x, y + 128)
            };

            for (int i = 0; i < 5; i++)
                textboxes[i].Initialize(positions[i]);

            // Testing
            sceneOneText = sceneTwoText = 
                "MonsName -- 65535 -- 65535\n" +
                "MonsName -- 65535 -- 65535\n" +
                "MonsName -- 65535 -- 65535\n" +
                "MonsName -- 65535 -- 65535\n" +
                "MonsName -- 65535 -- 65535\n" +
                "MonsName -- 65535 -- 65535\n" +
                "MonsName -- 65535 -- 65535\n" +
                "MonsName -- 65535 -- 65535";
        }

        public static void LoadContent(Texture2D[] textures, SpriteFont font)
        {
            for (int i = 0; i < 5; i++)
                textboxes[i].LoadContent(textures[i], font);
            spriteFont = font;
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            actorBox.Draw(spriteBatch);
            hitsBox.Draw(spriteBatch);
            targetBox.Draw(spriteBatch);
            dmgBox.Draw(spriteBatch);
            resultsBox.Draw(spriteBatch);
            spriteBatch.DrawString(spriteFont, sceneOneText, new Vector2(14, 444), Color.White);
            spriteBatch.DrawString(spriteFont, sceneTwoText, new Vector2(662, 444), Color.White);
        }

        /////////////
        // Publics //
        /////////////

        /// <summary>
        /// Remove and hide all text objects
        /// </summary>
        public static void Clear()
        {
            foreach (Textbox box in textboxes)
                TearDownTextBox(box);
            sceneOneText = "";
            sceneTwoText = "";
            textboxStack.Clear();
        }

        public static void SetSceneText(int sceneNum, string str)
        {
            if (sceneNum == 1)
                sceneOneText = str;
            else
                sceneTwoText = str;
        }

        public static void SetActorText(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Invalid actor string supplied");
            SetTextBox(actorBox, text);
        }

        public static void SetTargetText(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Invalid target string supplied");
            SetTextBox(targetBox, text);
        }

        public static void SetHitsText(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Invalid hits string supplied");
            SetTextBox(hitsBox, text);
        }

        public static void SetDamageText(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Invalid damage string supplied");
            SetTextBox(dmgBox, text);
        }

        public static void SetResultsText(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Invalid results string supplied");
            SetTextBox(resultsBox, text);
        }
        
        /// <summary>
        /// Removes and hides the results textbox
        /// </summary>
        public static void TearDownResults()
        {
            // Just pop the stack, results should always be on the top
            TearDownTextBox(textboxStack.Pop());
        }

        /// <summary>
        /// Returns whether there is text to tear down, and tears it down
        /// </summary>
        public static bool TearDownText()
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
        private static void SetTextBox(Textbox textbox, string text)
        {
            textbox.IsVisible = true;
            textbox.Text = text;
            textboxStack.Push(textbox);
        }

        /// <summary>
        /// Remove the textbox's text and make it invisible
        /// </summary>
        private static void TearDownTextBox(Textbox textbox)
        {
            // Animate?
            // Ensure the texbtox is removed from the stack?
            textbox.IsVisible = false;
            textbox.Text = "";
        }
    }
}
