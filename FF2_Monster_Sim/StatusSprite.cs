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
    class StatusSprite
    {
        public enum StatusAnimation
        {
            Amnesia,
            Poison,
            Mute,
            Sleep
            // Curse
            // Darkness
            // Confuse
            // Paralysis
            // Venom
            // Ignore instant death statuses: Mini, Toad, KO, Stone
        }

        public bool Visible = false;

        Texture2D activeGraphic;
        Texture2D amnesiaGraphic, psnGraphic, muteGraphic, slpGraphic;
        // Texture2D curseGraphic, darkGraphic, confGraphic, paraGraphic, venomGraphic;

        float timer = 0f;
        float interval = 200f;
        int currentFrame = 0;
        int totalFrames = 0;
        int spriteWidth = 24; // 10 - 24
        int spriteHeight = 16;
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
            amnesiaGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Amnesia");
            psnGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Poison");
            muteGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Mute");
            slpGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Sleep");

            /*
            curseGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Curse");
            darkGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Darkness");
            confGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Confuse");
            paraGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Paralysis");
            venomGraphic = content.Load<Texture2D>("Graphics\\Statuses\\Venom");
            */
        }

        public void Update(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime.Milliseconds;
            if (timer >= interval)
            {
                timer -= interval;

                currentFrame++;
                if (currentFrame >= totalFrames)
                    currentFrame = 0;
                
                sourceRect = new Rectangle(currentFrame * spriteWidth, 0, spriteWidth, spriteHeight);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
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

            switch (anim)
            {
                case StatusAnimation.Amnesia:
                    spriteWidth = 10;
                    totalFrames = 2;
                    activeGraphic = amnesiaGraphic;
                    break;
                case StatusAnimation.Mute:
                    spriteWidth = 24;
                    totalFrames = 2;
                    activeGraphic = muteGraphic;
                    break;
                case StatusAnimation.Poison:
                    spriteWidth = 16;
                    totalFrames = 2;
                    activeGraphic = psnGraphic;
                    break;
                case StatusAnimation.Sleep:
                    spriteWidth = 12;
                    totalFrames = 4;
                    activeGraphic = slpGraphic;
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("Uncaught status animation: " + anim.ToString());
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
