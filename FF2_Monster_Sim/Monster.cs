using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;


namespace FF2_Monster_Sim
{
    public enum MonsterFamily
    {
        Air,
        Water,
        Earth,
        Giant,
        Magic,
        Dragon,
        Were,
        Undead
    }

    public struct MonsterAction
    {
        public string Name;
        public int Level;
        public int Accuracy;
        public int MPCost;
        public string Target;

        public MonsterAction(string name, int level, int acc, int mpCost, string target)
        {
            Name = name;
            Level = level;
            Accuracy = acc;
            MPCost = mpCost;
            Target = target;
        }
    }

    public class Monster
    {
        private Random rnd;
        private bool flipped;

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

        public int HPMax { get; set; } = -1;
        private int hp;
        public int HP
        {
            get { return hp; }
            set
            {
                if (HPMax == -1) HPMax = Utils.EnforceStatCap(value, 65535);
                hp = Utils.EnforceStatCap(value, HPMax);
            }
        }

        public int MPMax { get; set; } = -1;
        private int mp;
        public int MP
        {
            get { return mp; }
            set
            {
                if (MPMax == -1) MPMax = Utils.EnforceStatCap(value, 65535);
                mp = Utils.EnforceStatCap(value, MPMax);
            }
        }

        private int strength;
        public int Strength
        {
            get
            {
                // TODO: Determine calculation order for Curse and Berserk
                int totalStrength = strength;

                // Halve strength if afflicted with Curse. Note: Int division
                if (PermStatuses.Contains(PermStatus.Curse)) totalStrength = totalStrength / 2;

                // Add Berserk stacks (5 per) and Imbibe stacks (10 per)
                int totalBuff = 0;
                if (Buffs.ContainsKey(Buff.Berserk)) totalBuff += (Buffs[Buff.Berserk] * 5);
                if (Buffs.ContainsKey(Buff.Imbibe)) totalBuff += (Buffs[Buff.Imbibe] * 10);
                totalStrength += (totalBuff % 256); // Overflow

                return (totalStrength > 255) ? 255 : totalStrength; // Cap at 255
            }
            set { strength = Utils.EnforceStatCap(value); }
        }

        private int hits;
        public int Hits
        {
            get
            {
                // TODO: Determine what happens between Haste and Slow if they both exist. They shouldn't.
                int totalHits = hits;

                // Add Haste stacks to total hits, or remove slow stacks
                int totalStacks = 0;
                if (Buffs.ContainsKey(Buff.Haste)) totalStacks = Buffs[Buff.Haste];
                if (totalStacks > 16) totalStacks = 16; // 16-stack maximum for Haste
                totalHits += totalStacks;
                
                if (Debuffs.ContainsKey(Debuff.Slow)) totalHits -= Debuffs[Debuff.Slow];
                if (totalHits < 1) totalHits = 1; // Always a 1 hit minimum

                return totalHits;
            }
            set { hits = Utils.EnforceStatCap(value, min: 1, max: 16); }
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
            set { accuracy = Utils.EnforceStatCap(value, 99); }
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
            set { defense = Utils.EnforceStatCap(value); }
        }

        private int blocks;
        public int Blocks
        {
            get
            {
                // Add Blink stacks to blocks
                int totalBlocks = blocks;
                int totalBuff = 0;
                if (Buffs.ContainsKey(Buff.Blink)) totalBuff = Buffs[Buff.Blink];
                totalBlocks += (totalBuff % 256); // Overflow
                return (totalBlocks > 255) ? 255 : totalBlocks; // Cap at 255
            }
            set { blocks = Utils.EnforceStatCap(value); }
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
            set { evasion = Utils.EnforceStatCap(value, 99); }
        }

        private int magicBlocks;
        public int MagicBlocks
        {
            get
            {
                // Add Shell stacks to magic blocks
                int totalMagicBlocks = magicBlocks;
                int totalBuff = 0;
                if (Buffs.ContainsKey(Buff.Shell)) totalBuff = Buffs[Buff.Shell];
                totalMagicBlocks += (totalBuff % 256); // Overflow
                return (totalMagicBlocks > 255) ? 255 : totalMagicBlocks; // Cap at 255
            }
            set { magicBlocks = Utils.EnforceStatCap(value); }
        }

        private int magicEvasion;
        public int MagicEvasion
        {
            get { return magicEvasion; }
            set { magicEvasion = Utils.EnforceStatCap(value, 99); }
        }

        private int fear;
        public int Fear
        {
            get
            {
                // Add Fear stacks * Fear Power (20), with debuff overflow check
                int totalFear = fear;
                int totalDebuff = 0;
                if (Debuffs.ContainsKey(Debuff.Fear)) totalDebuff = (Debuffs[Debuff.Fear] * 20);
                totalFear += (totalDebuff % 256); // Overflow
                return (totalFear > 255) ? 255 : totalFear; // Cap at 255
            }
            set { fear = Utils.EnforceStatCap(value); }
        }

        public List<MonsterAction> ActionList { get; set; }
        public HashSet<string> AttackEffects { get; set; }
        public HashSet<MonsterFamily> Families { get; set; }
        public HashSet<Element> Weaknesses { get; set; } // TODO: Some sort of setter that disallows Element.None
        public HashSet<Element> Resistances { get; set; } // TODO: Some sort of setter that disallows Element.None
        public HashSet<Element> Absorbs { get; set; } // TODO: Some sort of setter that disallows Element.None

        // The below may or may not be implemented
        // public int Level { get; set; }
        // public List<string> GilDrops { get; set; }
        // public List<string> ItemDrops { get; set; }

        // In-battle effects
        public int Init = 0;
        public Dictionary<Buff, int> Buffs { get; set; }
        public Dictionary<Debuff, int> Debuffs { get; set; } 
        public HashSet<TempStatus> TempStatuses { get; set; }
        public HashSet<PermStatus> PermStatuses { get; set; }

        public Monster()
        {
            ActionList = new List<MonsterAction>();
            AttackEffects = new HashSet<string>();
            Families = new HashSet<MonsterFamily>();
            Weaknesses = new HashSet<Element>();
            Resistances = new HashSet<Element>();
            Absorbs = new HashSet<Element>();

            Buffs = new Dictionary<Buff, int>();
            Debuffs = new Dictionary<Debuff, int>();
            TempStatuses = new HashSet<TempStatus>();
            PermStatuses = new HashSet<PermStatus>();

            rnd = new Random();
        }

        public void Initialize(Texture2D texture, bool flip = false)
        {
            MonsterTexture = texture;
            flipped = flip;
            Position = new Vector2();
        }

        public void Update()
        {
            //
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            SpriteEffects s = SpriteEffects.None;
            if (flipped) s = SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(MonsterTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, s, 0f);
        }

        ////////////////////
        // Stat Functions //
        ////////////////////

        public bool IsWeakTo(Element element)
        {
            return Weaknesses.Contains(element);
        }

        public bool IsResistantTo(Element element)
        {
            Element[] barrResists = { Element.Dimension, Element.Fire, Element.Mind, Element.Lightning, Element.Death, Element.Poison, Element.Body, Element.Ice };
            for (int i = 0; i < GetBuffStacks(Buff.Barrier); i++)
            {
                // Ignore Ice unless bug fixes are in effect
                if (!Globals.BUG_FIXES && i == 7) break;
                if (barrResists[i] == element) return true;
            }

            return Resistances.Contains(element);
        }

        public bool IsAbsorbantTo(Element element)
        {
            return Absorbs.Contains(element);
        }

        //////////////////////
        // Battle Functions //
        //////////////////////

        /// <summary>
        /// Get the monster's current action.
        /// </summary>
        public MonsterAction GetAction()
        {
            // Noting this here... Curse halves magic power, but that's not a base stat
            
            // Odds per slot: 20, 20, 20, 10, 10, 10, 5, 5
            int[] slotOdds = { 20, 40, 60, 70, 80, 90, 95, 100 };
            int rndRoll = rnd.Next(0, 100);
            for (int i = 0; i < slotOdds.Length; i++)
            {
                if (slotOdds[i] > rndRoll)
                {
                    return ActionList[i];
                }
            }

            Debug.WriteLine("Error retrieving action");
            return new MonsterAction();
        }
        
        public void HealHP(int amount)
        {
            if (amount < 0) Debug.WriteLine("Cannot heal a negative amount: " + amount);
            else HP += amount;
        }

        public void HealMP(int amount)
        {
            if (amount < 0) Debug.WriteLine("Cannot heal a negative amount: " + amount);
            else MP += amount;
        }

        public void DamageHP(int amount)
        {
            if (amount < 0) Debug.WriteLine("Cannot damage a negative amount: " + amount);
            else HP -= amount;
        }

        public void DamageMP(int amount)
        {
            if (amount < 0) Debug.WriteLine("Cannot damage a negative amount: " + amount);
            else MP -= amount;
        }

        public void Kill()
        {
            HP = 0;
            // TODO: Animation
            // TODO: Removal
        }

        public bool IsDead()
        {
            // TODO: Add KO status checks here?
            return HP <= 0;
        }

        ///////////
        // Buffs //
       ////////////

        /// <summary>
        /// Attempt to add a Buff to the Monster
        /// </summary>
        /// <param name="buff">The Buff to add</param>
        /// <param name="stacks">How many times to add the Buff (1 to 16)</param>
        /// <returns>Whether or not the Buff was successfully added</returns>
        public bool AddBuff(Buff buff, int stacks)
        {
            if (stacks < 1 || stacks > 16)
            {
                Debug.WriteLine("Stacks must be within range 1 - 16. Found: " + stacks);
                return false;
            }

            /* Notes TODO
             * 
             * Aura - Grants family-killing properties to main weapon
             * 1 - Magic Beast
             * 2 - Aquatic
             * 3 - Earth
             * 4 - Giants
             * 5 - Spellcaster
             * 6 - Dragons
             * 7 - Were
             * 8+ - Undead (Doesn't work)
             * 
             * Wall - Negates ALL spells up to it's level 
             * If wall exists and an instant KO spell is used, it succeeds
             */

            // There are three types of Buffs: Stacking, Non-stackng, and HighestStack

            // Non-stacking
            Buff[] nonStackingBuffs = new Buff[] { Buff.Intelligence, Buff.Spirit };
            if (nonStackingBuffs.Contains(buff))
            {
                // Add the buff only if it doesn't exist currently
                if (Buffs.ContainsKey(buff)) return false;
                Buffs.Add(buff, 0);
                return true;
            }

            // Highest-stack
            Buff[] highestStack = new Buff[] { Buff.Haste, Buff.Aura, Buff.Barrier, Buff.Wall };
            if (highestStack.Contains(buff))
            {
                // Set the buff to the highest value between current and stacks
                if (Buffs.TryGetValue(buff, out int value))
                {
                    if (value < stacks) Buffs[buff] = stacks;
                }
                else Buffs.Add(buff, stacks);

                // Haste clears the Slow debuff
                if (buff == Buff.Haste) RemoveDebuff(Debuff.Slow);

                // Enforce max stacks of 8 for Aura and Barrier
                if ((buff == Buff.Aura || buff == Buff.Barrier) && Buffs[buff] > 8) Buffs[buff] = 8;

                // TODO: Determine what bool should be given if stacks is lower than the current value
                return true;
            }

            // Stacking
            Buff[] stackingBuffs = new Buff[] { Buff.Berserk, Buff.Blink, Buff.Protect, Buff.Shell, Buff.Imbibe };
            if (stackingBuffs.Contains(buff))
            {
                // Add successes to existing buffs, else set it
                if (Buffs.TryGetValue(buff, out int value))
                {
                    Buffs[buff] += stacks;
                }
                else Buffs.Add(buff, stacks);
                Buffs[buff] = (Buffs[buff] % 256); // Overflow
                return true;
            }
            
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
        /// Check if a Buff exists on the monster. Returns stack value of the buff, 0 if not found.
        /// </summary>
        /// <param name="buff">The Buff to check for</param>
        /// <returns>Stack value of the buff, 0 if not found.</returns>
        public int GetBuffStacks(Buff buff)
        {
            if (Buffs.TryGetValue(buff, out int value)) return value;
            return 0;
        }

        ////////////
        // Debuff //
        ////////////

        /// <summary>
        /// Attempt to add a Debuff to the Monster
        /// </summary>
        /// <param name="debuff">The Debuff to add</param>
        /// <param name="stacks">How many times to add the Debuff (1 to 16)</param>
        /// <returns>Whether or not the debuff was successfully added</returns>
        public bool AddDebuff(Debuff debuff, int stacks)
        {
            if (stacks < 1 || stacks > 16)
            {
                Debug.WriteLine("Stacks must be within range 1 - 16. Found: " + stacks);
                return false;
            }

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
            
            Debug.WriteLine("Debuff not caught: " + debuff);
            return false;
        }

        /// <summary>
        /// Remove a Debuff from the Monster
        /// </summary>
        /// <returns>Whether or not the Debuff existed before removal</returns>
        public bool RemoveDebuff(Debuff debuff)
        {
            bool retBool = Debuffs.ContainsKey(debuff);
            Debuffs.Remove(debuff);
            return retBool;
        }

        /// <summary>
        /// Check if a Debuff exists on the monster. Returns stack value of the debuff, 0 if not found.
        /// </summary>
        /// <returns>Stack value of the debuff, 0 if not found.</returns>
        public int GetDebuffStacks(Debuff debuff)
        {
            if (Debuffs.TryGetValue(debuff, out int value)) return value;
            return 0;
        }

        /////////////////
        // Temp Status //
        /////////////////

        /// <summary>
        /// Attempt to add a temporary status to the monster. 
        /// If the status is Mini, the monster is instead killed.
        /// </summary>
        /// <returns>Whether or not the status was successfully added</returns>
        public bool AddTempStatus(TempStatus tempStatus)
        {
            if (tempStatus == TempStatus.Mini)
            {
                // TODO: Mini should kill the monster after it's added
            }
            return TempStatuses.Add(tempStatus);
        }
        
        public bool RemoveTempStatus(TempStatus tempStatus)
        {
            return TempStatuses.Remove(tempStatus);
        }

        public bool HasTempStatus(TempStatus tempStatus)
        {
            return TempStatuses.Contains(tempStatus);
        }

        /////////////////
        // Perm Status //
        /////////////////

        /// <summary>
        /// Attempt to add a permanent status to the monster. 
        /// If the status is KO, Stone, or Toad, the monster is instead killed.
        /// </summary>
        /// <returns>Whether or not the status was successfully added</returns>
        public bool AddPermStatus(PermStatus permStatus)
        {
            if (permStatus == PermStatus.KO || permStatus == PermStatus.Stone || permStatus == PermStatus.Toad)
            {
                // TODO: KO, Stone, and Toad should kill the monster after it's added
            }
            return PermStatuses.Add(permStatus);
        }

        public bool RemovePermStatus(PermStatus permStatus)
        {
            return PermStatuses.Remove(permStatus);
        }

        public bool HasPermStatus(PermStatus permStatus)
        {
            return PermStatuses.Contains(permStatus);
        }
    }
}
