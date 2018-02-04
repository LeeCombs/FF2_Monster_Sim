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
        // Misc vars

        public BattleScene scene;
        public string size = "";
        public bool IsVisible = true;

        private const int FEAR_VALUE = 20;
        private const int BERSERK_VALUE = 5;
        private const int IMBIBE_VALUE = 10;
        private const int PROTECT_VALUE = 0;
        
        // Monogame vars

        private bool flipped;
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
        
        // Combat vars

        public string Name { get; set; }
        public List<string> AlternateNames { get; set; }

        public int HPMax { get; set; } = -1;
        private int hp;
        public int HP
        {
            get { return hp; }
            set
            {
                // Set HPMax if not already set
                if (HPMax == -1)
                    HPMax = Utils.EnforceNumCap(value, 65535);
                hp = Utils.EnforceNumCap(value, HPMax);
            }
        }

        public int MPMax { get; set; } = -1;
        private int mp;
        public int MP
        {
            get { return mp; }
            set
            {
                // Set MPMax if not already set
                if (MPMax == -1)
                    MPMax = Utils.EnforceNumCap(value, 65535);
                mp = Utils.EnforceNumCap(value, 0, MPMax);
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
                if (PermStatuses.Contains(PermStatus.Curse))
                    totalStrength = totalStrength / 2;

                // Add Berserk stacks (5 per) and Imbibe stacks (10 per)
                int totalBuff = 0;
                totalBuff += (GetBuffStacks(Buff.Berserk) * BERSERK_VALUE);
                totalBuff += (GetBuffStacks(Buff.Imbibe) * IMBIBE_VALUE);
                totalStrength += (totalBuff % 256); // Overflow
                return Utils.EnforceNumCap(totalStrength, 255);
            }
            set { strength = Utils.EnforceNumCap(value, 255); }
        }

        private int hits;
        public int Hits
        {
            get
            {
                // TODO: Determine what happens between Haste and Slow if they both exist. They shouldn't.
                // Add haste stacks and remove slow
                int totalHits = hits;
                totalHits += GetBuffStacks(Buff.Haste);
                totalHits -= GetDebuffStacks(Debuff.Slow);
                return Utils.EnforceNumCap(totalHits, 1, 32);
            }
            set { hits = Utils.EnforceNumCap(value, 1, 16); }
        }

        private int accuracy;
        public int Accuracy
        {
            get
            {
                // Halve accuracy if afflicted with darkness. Note: Int division
                int totalAccuracy = accuracy;
                if (HasPermStatus(PermStatus.Darkness))
                    totalAccuracy = totalAccuracy / 2;
                return totalAccuracy;
            }
            set { accuracy = Utils.EnforceNumCap(value, 99); }
        }

        private int defense;
        public int Defense
        {
            get
            {
                // TODO: Determine calculation order for Curse and Protect
                // TODO: Adds 1/4 caster's spirit per Protect stack. Spirit doesn't exist on Monsters...
                // Halve defense if afflicted with Curse. Note: Int division
                int totalDefense = defense;
                if (HasPermStatus(PermStatus.Curse))
                    totalDefense = totalDefense / 2;
                totalDefense += GetBuffStacks(Buff.Protect) * PROTECT_VALUE;
                return Utils.EnforceNumCap(totalDefense, 255);
            }
            set { defense = Utils.EnforceNumCap(value, 255); }
        }

        private int blocks;
        public int Blocks
        {
            get
            {
                // Add Blink stacks to blocks
                int totalBlocks = blocks;
                totalBlocks += GetBuffStacks(Buff.Blink);
                return Utils.EnforceNumCap(totalBlocks, 255);
            }
            set { blocks = Utils.EnforceNumCap(value, 255); }
        }

        private int evasion;
        public int Evasion
        {
            get
            {
                // Cannot evade if afflicted by sleep or paralysis
                int totalEvasion = evasion;
                if (HasTempStatus(TempStatus.Sleep) || HasTempStatus(TempStatus.Paralysis))
                    totalEvasion = 0;
                return totalEvasion;
            }
            set { evasion = Utils.EnforceNumCap(value, 99); }
        }

        private int magicBlocks;
        public int MagicBlocks
        {
            get
            {
                // Add Shell stacks to magic blocks
                int totalMagicBlocks = magicBlocks;
                totalMagicBlocks += GetBuffStacks(Buff.Shell);
                return Utils.EnforceNumCap(totalMagicBlocks, 255);
            }
            set { magicBlocks = Utils.EnforceNumCap(value, 255); }
        }

        private int magicEvasion;
        public int MagicEvasion
        {
            get { return magicEvasion; }
            set { magicEvasion = Utils.EnforceNumCap(value, 99); }
        }

        private int fear;
        public int Fear
        {
            get
            {
                // Add Fear stacks * Fear Power (20), with debuff overflow check
                int totalFear = fear;
                int totalDebuff = GetDebuffStacks(Debuff.Fear) * FEAR_VALUE;
                totalFear += (totalDebuff % 256); // Overflow
                return Utils.EnforceNumCap(totalFear, 255);
            }
            set { fear = Utils.EnforceNumCap(value, 255); }
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
        
        // Battle vars

        public int Init = 0;
        public Dictionary<Buff, int> Buffs { get; set; }
        public Dictionary<Debuff, int> Debuffs { get; set; } 
        public HashSet<TempStatus> TempStatuses { get; set; }
        public HashSet<PermStatus> PermStatuses { get; set; }

        private List<int[]> actionSlotOdds = new List<int[]>
        {
            new int[] { 0, 20 },
            new int[] { 1, 40 },
            new int[] { 2, 60 },
            new int[] { 3, 70 },
            new int[] { 4, 80 },
            new int[] { 5, 90 },
            new int[] { 6, 95 },
            new int[] { 7, 100 }
        };

        //////////////////
        // Actual Class //
        //////////////////


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
                if (!Globals.BUG_FIXES && i == 7)
                    break;

                if (barrResists[i] == element)
                    return true;
            }

            return Resistances.Contains(element);
        }

        public bool IsAbsorbentTo(Element element)
        {
            return Absorbs.Contains(element);
        }

        //////////////////////
        // Battle Functions //
        //////////////////////

        /// <summary>
        /// Get the monster's current action.
        /// Will return a basic attack if it cannot perform any normal actions.
        /// </summary>
        public MonsterAction GetAction()
        {
            /**
             * Considerations
             * 
             * Monster will never choose an ability it can't use
             * If mp == 0 || inflicted by amnesia/mute, won't try to cast a spell
             * if mp > 0, it can attempt to cast a spell that it doesn't have enough mp for
             * if it cannot perform any listed action, it will attack, even if attack doesn't exist in their abilities
             */

            // If the monster cannot cast a spell, it'll simply attack
            if (MP <= 0 || HasPermStatus(PermStatus.Amnesia) || HasTempStatus(TempStatus.Mute) || HasTempStatus(TempStatus.Confuse))
                new MonsterAction("Attack", 0, 0, 0, "SingleTarget");
            
            // Roll a random action and return it
            List<int[]> slotVals = actionSlotOdds;
            int rndRoll = Globals.rnd.Next(0, 100);
            for (int i = 0; i < actionSlotOdds.Count; i++)
                if (actionSlotOdds[i][1] > rndRoll)
                    return ActionList[actionSlotOdds[i][0]];
            
            // Default to basic attack if unable to perform any of the normal actions
            return new MonsterAction("Attack", 0, 0, 0, "SingleTarget");
        }

        /// <summary>
        /// Roll and return total number of successful hits
        /// </summary>
        public int RollHits()
        {
            int hits = 0;
            for (int i = 0; i < Hits; i++)
                if (Globals.rnd.Next(0, 100) < Accuracy)
                    hits++;
            return hits;
        }

        /// <summary>
        /// Roll and return total number of successful blocks
        /// </summary>
        public int RollBlocks()
        {
            int blocks = 0;
            for (int i = 0; i < Blocks; i++)
                if (Globals.rnd.Next(100) < Evasion)
                    blocks++;
            return blocks;
        }

        /// <summary>
        /// Roll and return total number of successful magic blocks
        /// </summary>
        public int RollMagicBlocks()
        {
            int mBlocks = 0;
            for (int i = 0; i < MagicBlocks; i++)
                if (Globals.rnd.Next(100) < MagicEvasion)
                    mBlocks++;
            return mBlocks;
        }
        
        public void HealHP(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Cannot heal a negative amount: " + amount);
            HP += amount;
        }

        public void HealMP(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Cannot heal a negative amount: " + amount);
            MP += amount;
        }

        public void DamageHP(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Cannot damage a negative amount: " + amount);
            HP -= amount;

            if (HP <= 0)
                Kill();
        }

        public void DamageMP(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Cannot damage a negative amount: " + amount);
            MP -= amount;
        }

        public void Kill()
        {
            HP = 0;
            
            // TODO: Animation
        }

        public bool IsAlive()
        {
            return HP > 0;
        }

        public bool IsDead()
        {
            return HP <= 0;
        }

        public bool IsCritical()
        {
            // HP is at half or less
            if (HP <= 0)
                return false;
            return HPMax / HP <= 2;
        }

        ///////////
        // Buffs //
        ///////////

        /// <summary>
        /// Attempt to add a Buff to the Monster
        /// </summary>
        /// <param name="buff">The Buff to add</param>
        /// <param name="stacks">How many times to add the Buff (1 to 16)</param>
        /// <returns>Whether or not the Buff was successfully added</returns>
        public bool AddBuff(Buff buff, int stacks)
        {
            if (stacks < 1 || stacks > 16)
                throw new ArgumentOutOfRangeException("Stacks must be within range 1 - 16. Found: " + stacks);

            // Ignore Spirit and Intelligence for now as they're irrelevant
            if (buff == Buff.Spirit || buff == Buff.Intelligence)
                return false;

            /**
             * Notes TODO
             * 
             * If wall exists and an instant KO spell is used, it succeeds
             */

            // There are three types of Buffs: Stacking, Non-stackng, and HighestStack

            // Non-stacking
            Buff[] nonStackingBuffs = new Buff[] { Buff.Intelligence, Buff.Spirit };
            if (nonStackingBuffs.Contains(buff))
            {
                // Add the buff only if it doesn't exist currently
                if (Buffs.ContainsKey(buff))
                    return false;

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
                    if (value < stacks)
                        Buffs[buff] = stacks;
                }
                else
                    Buffs.Add(buff, stacks);

                // Haste clears the Slow debuff
                if (buff == Buff.Haste)
                {
                    RemoveDebuff(Debuff.Slow);
                    if (Buffs[buff] > 16)
                        Buffs[buff] = 16;
                }

                // Enforce max stacks of 8 for Aura and Barrier
                if ((buff == Buff.Aura || buff == Buff.Barrier) && Buffs[buff] > 8)
                    Buffs[buff] = 8;

                // TODO: Determine what bool should be given if stacks is lower than the current value
                return true;
            }

            // Stacking
            Buff[] stackingBuffs = new Buff[] { Buff.Berserk, Buff.Blink, Buff.Protect, Buff.Shell, Buff.Imbibe };
            if (stackingBuffs.Contains(buff))
            {
                // Add successes to existing buffs, else set it
                if (Buffs.TryGetValue(buff, out int value))
                    Buffs[buff] += stacks;
                else
                    Buffs.Add(buff, stacks);

                Buffs[buff] = (Buffs[buff] % 256); // Overflow
                return true;
            }

            throw new Exception("Invalid buff supplied: " + buff);
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
            if (Buffs.TryGetValue(buff, out int value))
                return value % 256; // Overflow
            return 0;
        }

        public bool HasBuff(Buff buff)
        {
            return Buffs.ContainsKey(buff);
        }

        /////////////
        // Debuffs //
        /////////////

        /// <summary>
        /// Attempt to add a Debuff to the Monster
        /// </summary>
        /// <param name="debuff">The Debuff to add</param>
        /// <param name="stacks">How many times to add the Debuff (1 to 16)</param>
        /// <returns>Whether or not the debuff was successfully added</returns>
        public bool AddDebuff(Debuff debuff, int stacks)
        {
            if (stacks < 1 || stacks > 16)
                throw new ArgumentOutOfRangeException("Stacks must be within range 1 - 16. Found: " + stacks);

            // Slow doesn't stack
            if (debuff == Debuff.Slow)
            {
                // The highest stacked slow takes over
                if (Debuffs.TryGetValue(debuff, out int value))
                {
                    if (value < stacks)
                        Debuffs[debuff] = stacks;
                }
                else
                    Debuffs.Add(debuff, stacks);

                // Slow clears the Haste buff
                RemoveBuff(Buff.Haste);

                // TODO: Determine what bool to return is stacks is lower than current value
                return true;
            }

            // Fear stacks
            if (debuff == Debuff.Fear)
            {
                if (Debuffs.TryGetValue(debuff, out int value))
                    Debuffs[debuff] += stacks;
                else
                    Debuffs[debuff] = stacks;
                return true;
            }
            
            throw new Exception("Invalid debuff supplied: " + debuff);
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
            if (Debuffs.TryGetValue(debuff, out int value))
                return value % 256; // Overflow
            return 0;
        }

        public bool HasDebuff(Debuff debuff)
        {
            return Debuffs.ContainsKey(debuff);
        }

        ///////////////////
        // Temp Statuses //
        ///////////////////

        /// <summary>
        /// Attempt to add a temporary status to the monster. 
        /// If the status is Mini, the monster is instead killed.
        /// </summary>
        /// <returns>Whether or not the status was successfully added</returns>
        public bool AddTempStatus(TempStatus tempStatus)
        {
            if (tempStatus == TempStatus.Mini)
            {
                // Animation
                Kill();
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

        ///////////////////
        // Perm Statuses //
        ///////////////////

        /// <summary>
        /// Attempt to add a permanent status to the monster. 
        /// If the status is KO, Stone, or Toad, the monster is instead killed.
        /// </summary>
        /// <returns>Whether or not the status was successfully added</returns>
        public bool AddPermStatus(PermStatus permStatus)
        {
            if (permStatus == PermStatus.KO || permStatus == PermStatus.Stone || permStatus == PermStatus.Toad)
            {
                // Animation
                Kill();
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
        
        /////////////
        // Helpers //
        /////////////
    }
}
