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
        private static float volume = 0.05f; // 0.1f
        public static float SoundVolume = 0.1f;

        // Songs
        private static Song battleSong, bossSong, victorySong, defeatSong, menuSong;
        private static SoundEffect physicalSnd, deathSnd, attackSnd, healSnd, buffSnd, debuffSnd;

        public enum Sound
        {
            Physical, Death, AttackSpell, Heal, Buff, Debuff
        }

        public enum Music
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
            physicalSnd = content.Load<SoundEffect>("Sounds\\PhysicalHit");
            deathSnd = content.Load<SoundEffect>("Sounds\\Death");
            attackSnd = content.Load<SoundEffect>("Sounds\\AttackSpell");
            healSnd = content.Load<SoundEffect>("Sounds\\Heal");
            buffSnd = content.Load<SoundEffect>("Sounds\\Buff");
            debuffSnd = content.Load<SoundEffect>("Sounds\\Debuff");
        }

        public static void Update(GameTime gameTime)
        {
            //
        }

        /////////////
        // Publics //
        /////////////

        public static void PlaySound(Sound sound)
        {
            switch (sound)
            {
                case Sound.AttackSpell:
                    HandlePlaySound(attackSnd);
                    break;
                case Sound.Buff:
                    HandlePlaySound(buffSnd);
                    break;
                case Sound.Death:
                    HandlePlaySound(deathSnd);
                    break;
                case Sound.Debuff:
                    HandlePlaySound(debuffSnd);
                    break;
                case Sound.Heal:
                    HandlePlaySound(healSnd);
                    break;
                case Sound.Physical:
                    HandlePlaySound(physicalSnd);
                    break;
                default:
                    Debug.WriteLine("PlaySound sound not caught: " + sound.ToString());
                    break;
            }
        }

        public static void PlayMusic(Music music)
        {
            switch (music)
            {
                case Music.Battle:
                    MediaPlayer.Play(battleSong);
                    break;
                case Music.Boss:
                    MediaPlayer.Play(bossSong);
                    break;
                case Music.Defeat:
                    MediaPlayer.Play(defeatSong);
                    break;
                case Music.Menu:
                    MediaPlayer.Play(menuSong);
                    break;
                case Music.Victory:
                    MediaPlayer.Play(victorySong);
                    break;
                default:
                    Debug.WriteLine("PlayMusic music not caught: " + music.ToString());
                    break;
            }
        }
        
        /////////////
        // Helpers //
        /////////////

        private static void HandlePlaySound(SoundEffect se)
        {
            try
            {
                SoundEffectInstance sei = se.CreateInstance();
                sei.Volume = SoundVolume;
                sei.Play();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error playing sound " + se.ToString() + ": " + e);
            }
        }
    }
}
