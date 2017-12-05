using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace FF2_Monster_Sim
{
    public struct AttackResult
    {
        int Hits;
        int Damage;
        List<string> Results;

        public AttackResult(int hits, int damage, List<string> results)
        {
            Hits = hits;
            Damage = damage;
            Results = results;
        }
    }

    class AttackManager
    {
        private static Random rnd;

        /**
         * Attack Score, Number of attacks, Accuracy
         * Defense, Evasion, Evasion Rate
         * Damage = (atk...2 * atk) - defense
         * 
         * Critical Hits
         * - Bonus damage equal to attack score, not affected by defense
         * - Unknown crit rate?
         * 
         * Attacking Family/Elemental Weakness
         * - +20 to attack score. Only applied once
         * - If target is absorbent, deal normal damage
         * - If target is weak and resistant, +20 bonus
         * 
         * Status Effects
         * - Can inflict multiple Temp or Perm statuses, but not both types
         * - If attack connects, chance to apply status, regardless of damage or resistances
         * - If an attack causes any ailment, it causes all of them (i.e. Malboros)
         * 
         * Drain Effects
         * - Each hit, regardless of damage, apply normal drain logic
         * 
         * Ripper Effect
         * - NES BUG: Deals normal damage. Should deal +20 with each hit. Irrelevant to monsters.
         * 
         * Healing Staff:
         * - Heals instead of harms. Some bugs. Irrelevant to monsters.
         */

        public AttackManager()
        {
            //
        }

        public static void Initialize()
        {
            rnd = new Random();

            Debug.WriteLine("Attack man");
            Monster monster = new Monster();
            monster.Name = "Non";
            monster.Families = new HashSet<MonsterFamily> { MonsterFamily.Air, MonsterFamily.Dragon };
            monster.AddBuff(Buff.Aura, 8);
            monster.Strength = 10;
            monster.Hits = 10;
            monster.Accuracy = 99;
            monster.AttackEffects = new HashSet<string> { "Poison", "Boobs", "Drain HP", "Venom", "Mini", "KO", "whatever", "Drain MP" };
            AttackResult res = AttackMonster(monster, monster);

        }

        public static AttackResult AttackMonster(Monster actor, Monster target)
        {
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
                    attackScore += 20;
                    break;
                }
            }

            // Determine total successful hits and overall damage. Apply status effects if necessary.
            List<string> results = new List<string>();
            int totalHits = GetTotalHits(actor, target);
            int damage = 0;
            for (int i = 0; i < totalHits; i++)
            {
                // TODO: Check for a critical hit
                damage += rnd.Next(attackScore, attackScore * 2 + 1);

                if (actor.AttackEffects.Count == 0)
                    continue;

                // Apply attack effects to the target
                foreach (string effect in actor.AttackEffects)
                {
                    if (effect == "Drain HP")
                        Debug.WriteLine("Drain HP");

                    if (effect == "Drain MP")
                        Debug.WriteLine("Drain MP");

                    if (Enum.TryParse<PermStatus>(effect, out PermStatus permStat))
                    {
                        // TODO: Attempt to apply the PermStatus
                        Debug.WriteLine("found permStat : " + permStat);
                    }

                    if (Enum.TryParse<TempStatus>(effect, out TempStatus tempStat))
                    {
                        // TODO: Attempt to apply the TempStatus
                        Debug.WriteLine("found tempStat : " + tempStat);
                    }
                }
            }

            return new AttackResult(totalHits, damage, results);
        }

        private static int GetTotalHits(Monster actor, Monster target)
        {
            int attacks = 0;
            for (int i = 0; i < actor.Hits; i++)
                if (rnd.Next(0, 100) < actor.Accuracy)
                    attacks++;

            int blocks = 0;
            for (int i = 0; i < target.Blocks; i++)
                if (rnd.Next(0, 100) < target.Evasion)
                    blocks++;

            int totalHits = attacks - blocks;
            return totalHits > 0 ? totalHits : 0; 
        }
    }
}
