using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        
        private static string sceneOneText, sceneTwoText;
        private static string roundText, turnText;

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
            turnText = "Turn: 0";
            roundText = "Round: 0";

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

            spriteBatch.DrawString(spriteFont, roundText, new Vector2(0, 65), Color.White);
            spriteBatch.DrawString(spriteFont, turnText, new Vector2(0, 80), Color.White);

            ParseDrawSceneText(sceneOneText, 14, 444, spriteBatch);
            ParseDrawSceneText(sceneTwoText, 662, 444, spriteBatch);
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
            sceneOneText = sceneTwoText = "";
            roundText = "Round: 0";
            turnText = "Turn: 0";
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

        public static void SetRoundText(int round)
        {
            roundText = "Round: " + round.ToString();
        }

        public static void SetTurnText(int turn)
        {
            turnText = "Turn: " + turn.ToString();
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
            // Ensure the textbox is removed from the stack?
            textbox.IsVisible = false;
            textbox.Text = "";
        }

        /// <summary>
        /// Parse scene text to apply color throughout the string. 
        /// Expected format: "{{White}}Hello, {{Red}}World. \n{{White}}Newline {{Red}}test."
        /// TODO: Error checkin'. Color carry-over for newlines. Newline handling. etc. etc.
        /// </summary>
        private static void ParseDrawSceneText(string sceneText, int x, int y, SpriteBatch spriteBatch)
        {
            int newlineOffset = 0;
            string[] newLineSplit = sceneText.Split(new string[] { "\n" }, StringSplitOptions.None);
            for (int i = 0; i < newLineSplit.Length; i++)
            {
                string[] stringSplit = newLineSplit[i].Split(new string[] { "{{" }, StringSplitOptions.None);
                
                foreach (string str in stringSplit)
                {
                    // Ignore empty splits
                    if (string.IsNullOrEmpty(str))
                        continue;

                    // Split the string on "}}", use the [0] value for the color, and apply it to the [1] text
                    string[] nestedSplit = str.Split(new string[] { "}}" }, StringSplitOptions.None);

                    Color color = Color.White;
                    switch (nestedSplit[0].ToUpper())
                    {
                        case "WHITE":
                            color = Color.White;
                            break;
                        case "YELLOW":
                            color = Color.Yellow;
                            break;
                        case "RED":
                            color = Color.Red;
                            break;
                        default:
                            Debug.WriteLine("Unsupported color: " + nestedSplit[0]);
                            break;
                    }
                    spriteBatch.DrawString(spriteFont, nestedSplit[1], new Vector2(x, y + newlineOffset), color);
                    newlineOffset += 18;
                }
            }
        }
    }
}
