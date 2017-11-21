using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;


namespace FF2_Monster_Sim
{
    public class Monster
    {
        // Sprite Stuff
        public Texture2D MonsterTexture;
        public Vector2 Position;
        public int Width
        {
            get { return MonsterTexture.Width; }
        }
        public int Height
        {
            get { return MonsterTexture.Height; }
        }

        // Stats
        public string Name { get; set; }
        public List<string> AlternateNames { get; set; }

        private int hp;
        public int HP
        {
            get
            {
                return hp;
            }
            set
            {
                hp = value;
                if (hp < 0) hp = 0;
                if (hp > HPMax) hp = HPMax;
            }
        }

        public int HPMax { get; set; }

        private int mp;
        public int MP
        {
            get
            {
                return mp;
            }
            set
            {
                mp = value;
                if (mp < 0) mp = 0;
                if (mp > MPMax) mp = MPMax;
            }
        }

        public int MPMax { get; set; }
        public int Level { get; set; }

        private int strength;
        public int Strength
        {
            get
            {
                // TODO: Determine calculation order for Curse and Berserk
                int totalStrength = strength;

                // Halve strength if afflicted with Curse. Note: Int division
                if (PermStatuses.Contains(PermStatus.Curse)) totalStrength = totalStrength / 2;

                // Add Berserk stacks * Berserk Power (5)
                int totalBuff = 0;
                if (Buffs.ContainsKey(Buff.Berserk)) totalBuff += (Buffs[Buff.Berserk] * 5);
                totalStrength += (totalBuff % 256); // Overflow
                return (totalStrength < 255) ? totalStrength : 255; // Cap at 255
            }
            set
            {
                strength = value;
                if (strength < 0) strength = 0;
                if (strength > 255) strength = 255;
            }
        }

        private int hits;
        public int Hits
        {
            get
            {
                // TODO: Determine what happens between Haste and Slow if they both exist
                int totalHits = hits;

                // Add Haste stacks to total hits
                int totalStacks = 0;
                if (Buffs.ContainsKey(Buff.Haste)) totalStacks = Buffs[Buff.Haste];
                if (totalStacks > 16) totalStacks = 16; // 16-stack maximum for Haste
                totalHits += totalStacks;

                // Remove Slow stacks from total hits
                if (Debuffs.ContainsKey(Debuff.Slow)) totalHits -= Debuffs[Debuff.Slow];

                // Always a 1 hit minimum
                if (totalHits < 1) totalHits = 1;
                return totalHits;
            }
            set
            {
                hits = value;
                if (hits < 1) hits = 1;
                if (hits > 16) hits = 16;
            }
        }

        private int accuracy;
        public int Accuracy
        {
            get
            {
                // Halve accuracy if afflicted with darkness. Note: Int division
                int totalAccuracy = accuracy;
                if (PermStatuses.Contains(PermStatus.Darkness)) totalAccuracy = totalAccuracy / 2;
                return totalAccuracy;
            }
            set
            {
                accuracy = value;
                if (accuracy < 0) accuracy = 0;
                if (accuracy > 99) accuracy = 99;
            }
        }

        private int defense;
        public int Defense
        {
            get
            {
                // TODO: Determine calculation order for Curse and Protect
                int totalDefense = defense;

                // Halve defense if afflicted with Curse. Note: Int division
                if (PermStatuses.Contains(PermStatus.Curse)) totalDefense = totalDefense / 2;

                // TODO: Adds 1/4 caster's spirit per Protect stack. Spirit doesn't exist on Monsters...
                if (Buffs.ContainsKey(Buff.Protect))
                {
                    // totalDefense += (Buffs[Buff.Protect] * (spirit/4));
                }

                return totalDefense;
            }
            set
            {
                defense = value;
                if (defense < 0) defense = 0;
                if (defense > 255) defense = 255;
            }
        }

        private int blocks;
        public int Blocks
        {
            get
            {
                // Add number of Blink stacks to blocks
                int totalBlocks = blocks;
                if (Buffs.ContainsKey(Buff.Blink)) totalBlocks += Buffs[Buff.Blink];
                return totalBlocks;
            }
            set
            {
                blocks = value;
                if (blocks < 0) blocks = 0;
                if (blocks > 255) blocks = 255;
            }
        }

        private int evasion;
        public int Evasion
        {
            get
            {
                // Cannot evade if afflicted by sleep or paralysis
                int totalEvasion = evasion;
                if (TempStatuses.Contains(TempStatus.Sleep) || TempStatuses.Contains(TempStatus.Paralysis)) totalEvasion = 0;
                return totalEvasion;
            }
            set
            {
                evasion = value;
                if (evasion < 0) evasion = 0;
                if (evasion > 99) evasion = 99;
            }
        }

        private int magicBlocks;
        public int MagicBlocks
        {
            get
            {
                // Add number of Shell stacks to magic blocks
                int totalMagicBlocks = magicBlocks;
                if (Buffs.ContainsKey(Buff.Shell)) totalMagicBlocks += Buffs[Buff.Shell];
                return totalMagicBlocks;
            }
            set
            {
                magicBlocks = value;
                if (magicBlocks < 0) magicBlocks = 0;
                if (magicBlocks > 255) magicBlocks = 255;
            }
        }

        private int magicEvasion;
        public int MagicEvasion
        {
            get
            {
                return magicEvasion;
            }
            set
            {
                magicEvasion = value;
                if (magicEvasion < 0) magicEvasion = 0;
                if (magicEvasion > 99) magicEvasion = 99;
            }
        }

        private int fear;
        public int Fear
        {
            get
            {
                // Add Fear stacks * Fear Power (20), with debuff overflow check
                int totalFear = fear;
                int totalDebuff = 0;
                if (Debuffs.ContainsKey(Debuff.Fear)) totalDebuff += (Debuffs[Debuff.Fear] * 20);
                return totalFear + (totalDebuff % 256);
            }
            set
            {
                fear = value;
                if (fear < 0) fear = 0;
                if (fear > 255) fear = 255;
            }
        }

        public List<string> AttackEffects { get; set; }
        public List<string> SpecialAttacks { get; set; }
        public List<string> Families { get; set; }
        public List<string> Weaknesses { get; set; }
        public List<string> Resistances { get; set; }
        public List<string> Absorbs { get; set; }

        public List<string> GilDrops { get; set; }
        public List<string> ItemDrops { get; set; }

        // In-battle effects
        public Dictionary<Buff, int> Buffs { get; set; }
        public Dictionary<Debuff, int> Debuffs { get; set; }
        public List<TempStatus> TempStatuses { get; set; }
        public List<PermStatus> PermStatuses { get; set; }

        public Monster()
        {
            Buffs = new Dictionary<Buff, int>();
            Debuffs = new Dictionary<Debuff, int>();
            TempStatuses = new List<TempStatus>();
            PermStatuses = new List<PermStatus>();
        }
        
        public void Initialize(Texture2D texture, Vector2 position)
        {
            MonsterTexture = texture;
            Position = position;
        }

        public void Update()
        {
            //
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(MonsterTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        ////////////////////
        // Stat Functions //
        ////////////////////

        public bool IsWeakTo(string element)
        {
            if (String.IsNullOrEmpty(element)) return false;
            return Weaknesses.Contains(element.ToUpper());
        }

        public bool IsResistantTo(string element)
        {
            if (String.IsNullOrEmpty(element)) return false;
            return Resistances.Contains(element.ToUpper());
        }

        public bool IsAbsorbantTo(string element)
        {
            if (String.IsNullOrEmpty(element)) return false;
            return Absorbs.Contains(element.ToUpper());
        }

        //////////////////////
        // Battle Functions //
        //////////////////////

        public string GetAction()
        {
            // Noting this here... Curse halves magic power, but that's not a base stat

            // Step through action logic, determine action, and return it
            return "";
        }

        ////////////////////////////
        // (De)Buffs and Statuses //
        ////////////////////////////

        /// <summary>
        /// Attempt to add a Buff to the Monster
        /// </summary>
        /// <param name="buff">The Buff to add</param>
        /// <param name="stacks">How many times to add the Buff</param>
        /// <returns>Whether or not the Buff was successfully added</returns>
        public bool AddBuff(Buff buff, int stacks)
        {
            // There are three types of Buffs: Stacking, Non-stackng, and HighestStack

            // Deal with non-stacking buffs first
            Buff[] nonStackingBuffs = new Buff[] { Buff.Intelligence, Buff.Spirit };
            if (nonStackingBuffs.Contains(buff))
            {
                // Add the buff only if it doesn't exist currently
                if (Buffs.ContainsKey(buff)) return false;
                Buffs.Add(buff, 0);
                return true;
            }

            // Deal with highest stack buffs
            Buff[] highestStack = new Buff[] { Buff.Haste, Buff.Aura, Buff.Barrier, Buff.Wall };
            if (highestStack.Contains(buff))
            {
                // If the buff exists, set it's value to the highest between itself and stacks
                // Otherwise add and set the buff
                if (Buffs.TryGetValue(buff, out int value))
                {
                    if (value < stacks) Buffs[buff] = stacks;
                }
                else Buffs.Add(buff, stacks);

                // Haste clears the Slow debuff
                if (buff == Buff.Haste) RemoveDebuff(Debuff.Slow);

                // TODO: Determine what bool should be given if stacks is lower than the current value
                return true;
            }

            // Deal with stacking buffs
            Buff[] stackingBuffs = new Buff[] { Buff.Berserk, Buff.Blink, Buff.Protect, Buff.Shell, Buff.Imbibe };
            if (stackingBuffs.Contains(buff))
            {
                // If the buff exists, add success to it's value
                // Otherwise add and set the buff
                if (Buffs.TryGetValue(buff, out int value))
                {
                    // Not doing anything with 'value'
                    Buffs[buff] += stacks;
                }
                else Buffs.Add(buff, stacks);

                // Stacking buffs always "succeed" adding themselves
                return true;
            }

            // The buff wasn't caught above
            Debug.WriteLine("Buff not caught: " + buff);
            return false;
        }
        
        /// <summary>
        /// Remove a Buff from the Monster
        /// </summary>
        /// <param name="buff">The buff to remove</param>
        /// <returns>Whether or not the Buff existed before removal</returns>
        public bool RemoveBuff(Buff buff)
        {
            bool retBool = Buffs.ContainsKey(buff);
            Buffs.Remove(buff);
            return retBool;
        }

        /// <summary>
        /// Check if a Buff exists on the monster. Returns stack value of the given buff, 0 if not found.
        /// </summary>
        /// <param name="buff">The Buff to check for</param>
        /// <returns>Stack value of the buff, 0 if not found.</returns>
        public int HasBuff(Buff buff)
        {
            if (Buffs.TryGetValue(buff, out int value)) return value;
            return 0;
        }

        /// <summary>
        /// Attempt to add a Debuff to the Monster
        /// </summary>
        /// <param name="debuff">The Debuff to add</param>
        /// <param name="stacks">How many times to add the Debuff</param>
        /// <returns>Whether or not the debuff was successfully added</returns>
        public bool AddDebuff(Debuff debuff, int stacks)
        {
            // Slow doesn't stack
            if (debuff == Debuff.Slow)
            {
                // The highest stacked slow takes over
                if (Debuffs.TryGetValue(debuff, out int value))
                {
                    if (value < stacks) Debuffs[debuff] = stacks;
                }
                else Debuffs.Add(debuff, stacks);

                // Slow clears the Haste buff
                RemoveBuff(Buff.Haste);

                // TODO: Determine what bool to return is stacks is lower than current value
                return true;
            }

            // Fear stacks
            if (debuff == Debuff.Fear)
            {
                if (Debuffs.TryGetValue(debuff, out int value))
                {
                    Debuffs[debuff] += stacks;
                }
                else Debuffs[debuff] = stacks;
                return true;
            }

            // The debuff wasn't caught above
            Debug.WriteLine("Debuff not caught: " + debuff);
            return false;
        }

        /// <summary>
        /// Remove a Debuff from the Monster
        /// </summary>
        /// <param name="debuff">The debuff to remove</param>
        /// <returns>Whether or not the Debuff existed before removal</returns>
        public bool RemoveDebuff(Debuff debuff)
        {
            bool retBool = Debuffs.ContainsKey(debuff);
            Debuffs.Remove(debuff);
            return retBool;
        }
        
        /// <summary>
        /// Attempt to add a temporary status to the monster
        /// </summary>
        /// <param name="tempStatus">The TempStatus to add</param>
        /// <returns>Whether or not the status was successfully added</returns>
        public bool AddTempStatus(TempStatus tempStatus)
        {
            // TODO: If successful, Mini kills monster immediately

            if (!TempStatuses.Contains(tempStatus))
            {
                TempStatuses.Add(tempStatus);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a tempStatus from the Monster
        /// </summary>
        /// <param name="tempStatus">The TempStatus to remove</param>
        /// <returns>Whether or not the status existed prior to removal</returns>
        public bool RemoveTempStatus(TempStatus tempStatus)
        {
            return TempStatuses.Remove(tempStatus);
        }

        /// <summary>
        /// Attempt to add a permanent status to the monster
        /// </summary>
        /// <param name="permStatus">the PermStatus to add</param>
        /// <returns>Whether or not the status was successfully added</returns>
        public bool AddPermStatus(PermStatus permStatus)
        {
            // TODO: If successful, KO, Stone, Toad kills monster immediately

            if (!PermStatuses.Contains(permStatus))
            {
                PermStatuses.Add(permStatus);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a PermStatus from the Monster
        /// </summary>
        /// <param name="permStatus">The PermStatus to remove</param>
        /// <returns>Whether or not the status existed prior to removal</returns>
        public bool RemovePermStatus(PermStatus permStatus)
        {
            return PermStatuses.Remove(permStatus);
        }
    }
}
