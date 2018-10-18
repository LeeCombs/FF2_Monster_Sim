using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{
    public class MonoMonster : Monster
    {
        // Graphics
        float alpha = 1f;
        public bool IsVisible = true, IsFading = false;
        internal bool flipped;
        private Texture2D texture;
        public Vector2 Position;
        public int Width
        {
            get { return texture.Width; }
        }
        public int Height
        {
            get { return texture.Height; }
        }

        // Status sprites
        Dictionary<dynamic, StatusSprite> statSprites;

        // Helper to map animations to specific statuses
        Dictionary<dynamic, StatusSprite.StatusAnimation> StatSpritAnimMap = new Dictionary<dynamic, StatusSprite.StatusAnimation>() {
            { PermStatus.Amnesia, StatusSprite.StatusAnimation.Amnesia },
            { PermStatus.Curse, StatusSprite.StatusAnimation.Curse },
            { PermStatus.Darkness, StatusSprite.StatusAnimation.Blind },
            { PermStatus.Poison, StatusSprite.StatusAnimation.Poison },
            { TempStatus.Confuse, StatusSprite.StatusAnimation.Amnesia },
            { TempStatus.Mute, StatusSprite.StatusAnimation.Mute },
            { TempStatus.Paralysis, StatusSprite.StatusAnimation.Paralysis },
            { TempStatus.Sleep, StatusSprite.StatusAnimation.Sleep },
            { TempStatus.Venom, StatusSprite.StatusAnimation.Poison }
        };


        public MonoMonster()
        {
            statSprites = new Dictionary<dynamic, StatusSprite>();
        }
        
        //////////////
        // Monogame //
        //////////////

        public void Initialize(Texture2D texture, bool flipped = false)
        {
            Position = new Vector2();
            this.flipped = flipped;
            this.texture = texture;
        }

        public void LoadContent()
        {
            //
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            SpriteEffects s = SpriteEffects.None;
            if (flipped)
                s = SpriteEffects.FlipHorizontally;
            if (IsVisible)
                spriteBatch.Draw(texture, Position, null, Color.White * alpha, 0f, Vector2.Zero, 1f, s, 0f);
        }

        public void Update(GameTime gameTime)
        {
            if (IsFading)
            {
                alpha -= 0.05f;
                if (alpha <= 0f)
                    IsFading = false;
            }
        }

        /////////////
        // Publics //
        /////////////

        public override void Kill()
        {
            ClearStatusSprites();
            base.Kill();
        }

        public void Flicker(int cycles)
        {
            for (int i = 0; i < cycles; i++)
            {
                if (i % 2 == 0)
                    IsVisible = false;
                else
                    IsVisible = true;
                Thread.Sleep(25);
            }

            // Ensure the monster always ends up visible
            IsVisible = true;
        }

        public void StartDeath()
        {
            SoundManager.PlaySound(SoundManager.Sound.Death);
            IsFading = true;
        }
        
        public override bool AddTempStatus(TempStatus tempStatus)
        {
            bool ret = base.AddTempStatus(tempStatus);

            // If the status is not an instant kill, add the status sprite
            if (ret)
            {
                if (tempStatus != TempStatus.Mini)
                {
                    StatusSprite statSpr = StatusSpriteManager.GetStatusSprite();
                    SetStatusSprite(statSpr, tempStat: tempStatus);
                }
            }
            return ret;
        }

        public override bool RemoveTempStatus(TempStatus tempStatus)
        {
            // TODO: Remove the appropriate StatusSprite
            if (statSprites.ContainsKey(tempStatus))
            {
                statSprites[tempStatus].Visible = false;
                statSprites.Remove(tempStatus);
            }

            return base.RemoveTempStatus(tempStatus);
        }

        public override bool AddPermStatus(PermStatus permStatus)
        {
            bool ret = base.AddPermStatus(permStatus);
            if (ret)
            {
                // If the status is not an instant kill, add the status sprite
                PermStatus[] killStatuses = new PermStatus[] { PermStatus.KO, PermStatus.Stone, PermStatus.Toad };
                if (!killStatuses.Contains(permStatus))
                {
                    StatusSprite statSpr = StatusSpriteManager.GetStatusSprite();
                    SetStatusSprite(statSpr, permStat: permStatus);
                }
            }


            return ret;
        }

        public override bool RemovePermStatus(PermStatus permStatus)
        {
            // Remove the appropriate StatusSprite
            if (statSprites.ContainsKey(permStatus))
            {
                statSprites[permStatus].Visible = false;
                statSprites.Remove(permStatus);
            }

            return base.RemovePermStatus(permStatus);
        }

        /////////////
        // Helpers //
        /////////////
        private void ClearStatusSprites()
        {
            foreach (KeyValuePair<dynamic, StatusSprite> entry in statSprites)
                entry.Value.Visible = false;
            statSprites.Clear();
        }

        private void SetStatusSprite(StatusSprite statSpr, PermStatus? permStat = null, TempStatus? tempStat = null)
        {
            // Don't bother displaying more than three sprites
            if (statSprites.Count >= 3)
                return;

            dynamic stat;
            if (permStat != null)
                stat = permStat;
            else if (tempStat != null)
                stat = tempStat;
            else
                return;
            
            float xPos = Position.X + (26 * (statSprites.Count % 3));
            float yPos = Position.Y - 20; // 20 for bubble
            statSpr.SetPosition(new Vector2(xPos, yPos));
            statSpr.SetAnimation(StatSpritAnimMap[stat]);
            statSpr.Visible = true;

            statSprites.Add(stat, statSpr);
        }
    }
}
