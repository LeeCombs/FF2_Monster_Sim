using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;


namespace FF2_Monster_Sim
{
    public class SoundManager
    {
        private static float volume = 0f; // 0.1f

        // Songs
        private static Song battleSong;
        private static Song bossSong;
        private static Song victorySong;
        private static Song defeatSong;
        private static Song menuSong;
        private static SoundEffect physicalHit;

        // Sound

        public SoundManager()
        {
            //
        }

        //////////////
        // Monogame //
        //////////////

        public static void Initialize()
        {
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = volume;
        }

        public static void LoadContent(ContentManager content)
        {
            // Songs
            battleSong = content.Load<Song>("Music\\BattleScene");
            bossSong = content.Load<Song>("Music\\BattleScene2");
            victorySong = content.Load<Song>("Music\\Victory");
            defeatSong = content.Load<Song>("Music\\Dead");
            menuSong = content.Load<Song>("Music\\Chocobo");

            // Sounds
            physicalHit = content.Load<SoundEffect>("Sounds\\Physical_Hit");
        }

        public static void Update(GameTime gameTime)
        {
            //
        }

        /////////////
        // Publics //
        /////////////

        public static void PlayBattleMusic()
        {
            MediaPlayer.Play(battleSong);
        }
        
        public static void PlayBossMusic()
        {
            MediaPlayer.Play(bossSong);
        }
        
        public static void PlayVictoryMusic()
        {
            MediaPlayer.Play(victorySong);
        }
        
        public static void PlayDefeatMusic()
        {
            MediaPlayer.Play(defeatSong);
        }
        
        public static void PlayMenuMusic()
        {
            MediaPlayer.Play(menuSong);
        }

        public static void PlayPhysicalHitSound()
        {
            SoundEffectInstance ph = physicalHit.CreateInstance();
            ph.Volume = volume;
            ph.Play();
        }

        /////////////
        // Helpers //
        /////////////
    }
}
