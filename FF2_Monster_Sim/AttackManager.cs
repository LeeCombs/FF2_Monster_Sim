using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace FF2_Monster_Sim
{
    public struct AttackResult
    {
        public string HitsMessage;
        public string DamageMessage;
        public List<string> Results;

        public AttackResult(int hits, int damage, List<string> results)
        {
            hits = Utils.EnforceStatCap(hits, 16);
            if (hits == 0)
            {
                HitsMessage = null;
                DamageMessage = "Miss";
            }
            else
            {
                HitsMessage = hits > 0 ? hits.ToString() + "xHit" : "Miss";
                DamageMessage = damage.ToString() + " DMG";
            }
            Results = results ?? new List<string>();
        }
    }

    public class AttackManager
    {
        private static Random rnd;
        private const int CRIT_RATE = 5;
        private const int AURA_BONUS = 20;
        private const string CRIT_MESSAGE = "Critical Hit!";
        
        public AttackManager()
        {
            //
        }

        public static void Initialize()
        {
            rnd = new Random();
        }

        /// <summary>
        /// Get the result of one monster attacking another
        /// </summary>
        public static AttackResult AttackMonster(Monster actor, Monster target)
        {
            // Catch them errors first and foremost
            if (actor == null)
                throw new ArgumentNullException("Invalid actor supplied");
            if (target == null)
                throw new ArgumentNullException("Invalid target supplied");
            
            // Get overall attack score. If actor has beneficial AURA stacks, add +20 to attack
            int attackScore = actor.Strength;
            for (int i = 0; i < actor.GetBuffStacks(Buff.Aura); i++)
            {
                // Ignore Undead unless bug fixed
                if (!Globals.BUG_FIXES && i >= 7)
                    break;

                // Attack bonus only applies once for a family match
                if (target.Families.Contains((MonsterFamily)i))
                {
                    attackScore += AURA_BONUS;
                    break;
                }
            }

            // Determine total successful hits and overall damage. Apply status effects if necessary.
            List<string> results = new List<string>();
            int totalHits = GetTotalHits(actor, target);
            if (totalHits == 0)
                return new AttackResult(0, 0, new List<string>());

            int damage = 0;
            bool critFlag = false;
            for (int i = 0; i < totalHits; i++)
            {
                // Get damage and add critical bonus damage if rolled
                int dmgRoll = rnd.Next(attackScore, attackScore * 2 + 1) - target.Defense;
                damage += dmgRoll > 0 ? dmgRoll : 0;
                if (rnd.Next(100) < CRIT_RATE)
                {
                    damage += attackScore;
                    // Only add the crit message once
                    if (!critFlag)
                    {
                        critFlag = true;
                        results.Add(CRIT_MESSAGE);
                    }
                }
            }

            if (actor.AttackEffects.Count > 0)
            {
                int statusHits = totalHits - target.RollMagicBlocks();
                if (statusHits > 0)
                {
                    // Apply attack effects to the target
                    // TODO: I'm unsure of the logic behind status-touching, and will revisit later.
                    foreach (string effect in actor.AttackEffects)
                    {
                        // TODO: Figure out about message returns
                        if (string.Equals(effect, "Drain HP"))
                        {
                            int hpAmt = target.HPMax / 16;
                            int dranAmt = hpAmt * statusHits;

                            // Opposite effect vs. Undead
                            if (target.Families.Contains(MonsterFamily.Undead))
                            {
                                actor.DamageHP(dranAmt);
                                target.HealHP(dranAmt);
                            }
                            else
                            {
                                actor.HealHP(dranAmt);
                                target.DamageHP(dranAmt);
                            }
                            break;
                        }

                        // TODO: Figure out about message returns
                        if (string.Equals(effect, "Drain MP"))
                        {
                            int mpAmt = target.MPMax / 16;
                            int asplAmt = mpAmt * statusHits;

                            // Opposite effect vs. Undead
                            if (target.Families.Contains(MonsterFamily.Undead))
                            {
                                actor.DamageMP(asplAmt);
                                target.HealMP(asplAmt);
                            }
                            else
                            {
                                actor.HealMP(asplAmt);
                                target.DamageMP(asplAmt);
                            }
                            break;
                        }

                        if (Enum.TryParse<PermStatus>(effect, out PermStatus permStat))
                        {
                            target.AddPermStatus(permStat);
                            continue;
                        }

                        if (Enum.TryParse<TempStatus>(effect, out TempStatus tempStat))
                        {
                            target.AddTempStatus(tempStat);
                            continue;
                        }
                    }
                } // End statusHits > 0
            } // End attackEffects.Count > 0

            // Apply the damage and return the overall results
            target.DamageHP(damage);
            if (target.IsDead())
                results.Add(target.Name + " fell");
            return new AttackResult(totalHits, damage, results);
        }

        /////////////
        // Helpers //
        /////////////

        /// <summary>
        /// Return the total number of successful hits by the acting monster against a target
        /// </summary>
        private static int GetTotalHits(Monster actor, Monster target)
        {
            int totalHits = actor.RollHits() - target.RollBlocks();
            return totalHits > 0 ? totalHits : 0; 
        }
    }
}
