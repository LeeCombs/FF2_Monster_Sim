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
    public class MagicSprite
    {
        public enum MagicAnimation
        {
            Attack, Heal, Buff, Debuff
        }

        public bool Visible = false;

        Texture2D activeGraphic;
        Texture2D atkGraphic, healGraphic, buffGraphic, debuffGraphic;

        public const int SPRITE_WIDTH = 16, SPRITE_HEIGHT = 16;
        const float ANIMATION_INTERVAL = 100f;

        float timer = 0f, delaytimer = 0f, lifespan = 1000f;
        int currentFrame = 0;
        int totalFrames = 0;
        Vector2 position, origin;
        Rectangle sourceRect;

        public MagicSprite()
        {

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
                atkGraphic = content.Load<Texture2D>("Graphics\\Magic\\Attack");
                healGraphic = content.Load<Texture2D>("Graphics\\Magic\\Heal");
                buffGraphic = content.Load<Texture2D>("Graphics\\Magic\\Buff");
                debuffGraphic = content.Load<Texture2D>("Graphics\\Magic\\Debuff");
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

            delaytimer -= timer;

            if (lifespan > 0f) {
                lifespan -= gameTime.ElapsedGameTime.Milliseconds;
                if (lifespan <= 0f)
                    Visible = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Visible && delaytimer <= 0f)
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
        public void SetAnimation(MagicAnimation anim, float delaytimer = 0f)
        {
            Debug.WriteLine("Setanim " + anim.ToString() + ", " + delaytimer);

            this.delaytimer = delaytimer;
            lifespan = 1000f;
            Visible = true;

            // awk
            switch (anim)
            {
                case MagicAnimation.Attack:
                    activeGraphic = atkGraphic;
                    break;
                case MagicAnimation.Heal:
                    activeGraphic = healGraphic;
                    break;
                case MagicAnimation.Buff:
                    activeGraphic = buffGraphic;
                    break;
                case MagicAnimation.Debuff:
                    activeGraphic = debuffGraphic;
                    break;
                default:
                    Debug.WriteLine("Uncaught magic animation: " + anim.ToString());
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
