using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{
    public class MonoMonster : Monster
    {
        public bool IsVisible = true;
        private List<StatusSprite> statusSprites;
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

        public MonoMonster()
        {
            statusSprites = new List<StatusSprite>();
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
                spriteBatch.Draw(texture, Position, null, Color.White, 0f, Vector2.Zero, 1f, s, 0f);
        }

        public void Update()
        {
            //
        }

        /////////////
        // Publics //
        /////////////

        public override void Kill()
        {
            ClearStatusSprites();
            base.Kill();
        }

        public new bool AddTempStatus(TempStatus tempStatus)
        {
            // If the status is not an instant kill, add the status sprite
            if (tempStatus != TempStatus.Mini)
            {
                StatusSprite statSpr = StatusSpriteManager.GetStatusSprite();
                SetStatusSprite(statSpr, tempStat: tempStatus);
            }
            return base.AddTempStatus(tempStatus);
        }

        public new bool RemoveTempStatus(TempStatus tempStatus)
        {
            // TODO: Remove the appropriate StatusSprite
            return base.RemoveTempStatus(tempStatus);
        }

        public new bool AddPermStatus(PermStatus permStatus)
        {
            // If the status is not an instant kill, add the status sprite
            PermStatus[] killStatuses = new PermStatus[] { PermStatus.KO, PermStatus.Stone, PermStatus.Toad };
            if (!killStatuses.Contains(permStatus))
            {
                StatusSprite statSpr = StatusSpriteManager.GetStatusSprite();
                SetStatusSprite(statSpr, permStat: permStatus);
            }

            return base.AddPermStatus(permStatus);
        }

        public new bool RemovePermStatus(PermStatus permStatus)
        {
            // TODO: Remove the appropriate StatusSprite
            return base.RemovePermStatus(permStatus);
        }

        /////////////
        // Helpers //
        /////////////
        private void ClearStatusSprites()
        {
            foreach (StatusSprite s in statusSprites)
                s.Visible = false;
        }

        private void SetStatusSprite(StatusSprite statSpr, PermStatus? permStat = null, TempStatus? tempStat = null)
        {
            if (permStat == null && tempStat == null)
                return;

            if (permStat != null)
                switch (permStat)
                {
                    case PermStatus.Amnesia:
                        statSpr.SetAnimation(StatusSprite.StatusAnimation.Amnesia);
                        break;
                    case PermStatus.Curse:
                        // TODO: Amnesia -> Curse once animation exists
                        statSpr.SetAnimation(StatusSprite.StatusAnimation.Amnesia);
                        break;
                    case PermStatus.Darkness:
                        // TODO: Amnesia -> Darkness once animation exists
                        statSpr.SetAnimation(StatusSprite.StatusAnimation.Amnesia);
                        break;
                    case PermStatus.Poison:
                        statSpr.SetAnimation(StatusSprite.StatusAnimation.Poison);
                        break;
                }

            if (tempStat != null)
                switch (tempStat)
                {
                    case TempStatus.Confuse:
                        // TODO: Amnesia -> Confuse once animation exists
                        statSpr.SetAnimation(StatusSprite.StatusAnimation.Amnesia);
                        break;
                    case TempStatus.Mute:
                        statSpr.SetAnimation(StatusSprite.StatusAnimation.Mute);
                        break;
                    case TempStatus.Paralysis:
                        // TODO: Amnesia -> Paralysis once animation exists
                        statSpr.SetAnimation(StatusSprite.StatusAnimation.Amnesia);
                        break;
                    case TempStatus.Sleep:
                        statSpr.SetAnimation(StatusSprite.StatusAnimation.Sleep);
                        break;
                    case TempStatus.Venom:
                        statSpr.SetAnimation(StatusSprite.StatusAnimation.Poison);
                        break;
                }


            float xPos = Position.X + (26 * (statusSprites.Count % 3));
            float yPos = Position.Y + (16 * (statusSprites.Count / 3));

            statSpr.SetPosition(new Vector2(xPos, yPos));
            statSpr.Visible = true;

            statusSprites.Add(statSpr);
        }
    }
}
