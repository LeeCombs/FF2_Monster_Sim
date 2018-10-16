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
using System.Diagnostics;


namespace FF2_Monster_Sim
{
    public class SoundManager
    {
        private static float volume = 0f; // 0.1f
        public static float SoundVolume = 1f;

        // Songs
        private static Song battleSong, bossSong, victorySong, defeatSong, menuSong;
        private static SoundEffect physicalHit, death;

        enum Sound
        {
            Physical, Death, AttackSpell, Heal, Buff, Debuff
        }

        enum Music
        {
            Battle, Boss, Victory, Defeat, Menu
        }

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
            death = content.Load<SoundEffect>("Sounds\\Death");
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
            // Create and play the physical hit sound. Ignore it if there's an issue creating the instance.
            try
            {
                SoundEffectInstance ph = physicalHit.CreateInstance();
                ph.Volume = SoundVolume;
                ph.Play();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error creating physical hit sound: " + e);
            }
        }

        public static void PlayDeathSound()
        {
            // Create and play the physical hit sound. Ignore it if there's an issue creating the instance.
            try
            {
                SoundEffectInstance dth = death.CreateInstance();
                dth.Volume = SoundVolume;
                dth.Play();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error creating death sound: " + e);
            }
        }

        /////////////
        // Helpers //
        /////////////
    }
}
