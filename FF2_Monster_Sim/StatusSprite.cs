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
    public class StatusSprite
    {
        public enum StatusAnimation
        {
            Amnesia, // and Confuse
            Poison, // and Venom
            Mute,
            Sleep,
            Curse,
            Blind,
            Paralysis
            // Ignore instant death statuses: Mini, Toad, KO, Stone
        }

        public bool Visible = false;

        Texture2D activeGraphic;
        Texture2D amnesiaGraphic, psnGraphic, muteGraphic, slpGraphic, curseGraphic, blndGraphic, paraGraphic; // Why is this like this?

        const int SPRITE_WIDTH = 24;
        const int SPRITE_HEIGHT = 16;
        const float ANIMATION_INTERVAL = 200f;

        float timer = 0f;
        int currentFrame = 0;
        int totalFrames = 0;
        Vector2 position, origin;
        Rectangle sourceRect;


        public StatusSprite()
        {
            //
        }

        //////////////
        // Monogame //
        //////////////

        public void Initialize()
        {
            //
        }

        public void LoadContent(ContentManager content)
        {
            // Whatever
            if (content != null)
            {
                amnesiaGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Amnesia");
                psnGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Poison");
                muteGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Mute");
                slpGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Sleep");
                curseGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Cursed");
                blndGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Blind");
                paraGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Paralysis");

                /*
                confGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Confuse");
                venomGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Venom");
                */
            }

        }

        public void Update(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime.Milliseconds;
            if (timer >= ANIMATION_INTERVAL)
            {
                timer -= ANIMATION_INTERVAL;

                currentFrame++;
                if (currentFrame >= totalFrames)
                    currentFrame = 0;
                
                sourceRect = new Rectangle(currentFrame * SPRITE_WIDTH, 0, SPRITE_WIDTH, SPRITE_HEIGHT);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
                spriteBatch.Draw(activeGraphic, position, sourceRect, Color.White, 0f, origin, 1.0f, SpriteEffects.None, 0);
        }

        /////////////
        // Publics //
        /////////////

        public void SetPosition(Vector2 position)
        {
            this.position = position;
        }

        /// <summary>
        /// Load a given animation graphic, and start animating
        /// </summary>
        public void SetAnimation(StatusAnimation anim)
        {
            currentFrame = 0;
            totalFrames = 2;

            // awk
            switch (anim)
            {
                case StatusAnimation.Amnesia:
                    activeGraphic = amnesiaGraphic;
                    break;
                case StatusAnimation.Mute:
                    activeGraphic = muteGraphic;
                    break;
                case StatusAnimation.Poison:
                    activeGraphic = psnGraphic;
                    break;
                case StatusAnimation.Sleep:
                    totalFrames = 4;
                    activeGraphic = slpGraphic;
                    break;
                case StatusAnimation.Curse:
                    activeGraphic = curseGraphic;
                    break;
                case StatusAnimation.Blind:
                    activeGraphic = blndGraphic;
                    break;
                case StatusAnimation.Paralysis:
                    activeGraphic = paraGraphic;
                    break;
                default:
                    Debug.WriteLine("Uncaught status animation: " + anim.ToString());
                    break;
            }
        }

        /////////////
        // Helpers //
        /////////////

        private void HandleAnimation(GameTime gameTime)
        {
            // 
        }
    }
}
