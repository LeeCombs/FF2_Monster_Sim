using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace FF2_Monster_Sim
{
    public struct AttackResult
    {
        public int Hits;
        public int Damage;
        public List<string> Results;

        public AttackResult(int hits, int damage, List<string> results)
        {
            Hits = Utils.EnforceStatCap(hits);
            Damage = damage;
            Results = results ?? new List<string>();
        }
    }

    public class AttackManager
    {
        private static Random rnd;
        private const int CRIT_RATE = 5;

        /// <summary>
        /// Helper to determine which spells to cast based on Status Touch effect
        /// </summary>
        private static Dictionary<string, string> touchToSpellMap = new Dictionary<string, string>
        {
            { "Drain HP", "DRAN" },
            { "Drain MP", "ASPL" },
            { "Poison", "" }, // TODO: There's no relevant spell for poisoning. Figure it out.
            { "Sleep", "SLEP" },
            { "Mute", "MUTE" },
            { "Mini", "MINI" },
            { "Paralysis", "STOP" },
            { "Confusion", "CHRM" },
            { "Blind", "BLND" },
            { "Envenom", "" }, // TODO: There's no relevant spell for poisoning. Figure it out.
            { "Curse", "CURS" },
            { "Amensia", "FOG" },
            { "Toad", "TOAD" },
            { "Petrify", "BRAK" },
            { "Death", "DETH" },
        };

        /**
         * TODO: 
         * 
         * Critical Hits
         * - Bonus damage equal to attack score, not affected by defense
         * - Unknown crit rate?
         * 
         * Status Effects
         * - Can inflict multiple Temp or Perm statuses, but not both types
         * - If attack connects, chance to apply status, regardless of damage or resistances
         * - If an attack causes any ailment, it causes all of them (i.e. Malboros)
         * 
         * Drain Effects
         * - Each hit, regardless of damage, apply normal drain logic
         */

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
                // Get damage and add critical bonus damage if rolled
                damage += rnd.Next(attackScore, attackScore * 2 + 1) - target.Defense;
                if (rnd.Next(100) < CRIT_RATE)
                {
                    damage += attackScore;
                    results.Add("Critical Hit!");
                }
            }

            if (actor.AttackEffects.Count > 0)
            {
                // Apply attack effects to the target
                // TODO: I'm unsure of the logic behind status-touching, and will revisit later.
                foreach (string effect in actor.AttackEffects)
                {
                    if (string.Equals(effect, "Drain HP"))
                    {
                        Spell drainHPSpell = SpellManager.GetSpellByName("DRAN");
                        SpellResult drainRes = SpellManager.CastSpell(actor, target, drainHPSpell, totalHits);
                    }

                    if (effect == "Drain MP")
                    {
                        Spell drainMPSpell = SpellManager.GetSpellByName("ASPL");
                        SpellResult drainRes = SpellManager.CastSpell(actor, target, drainMPSpell, totalHits);
                    }

                    if (Enum.TryParse<PermStatus>(effect, out PermStatus permStat))
                    {
                        Spell pStatSpell = SpellManager.GetSpellByName(touchToSpellMap[effect]);
                        SpellResult pStatRes = SpellManager.CastSpell(actor, target, pStatSpell, totalHits);

                        // If one status spell hits, they are all applied. TODO: This is ugly. Un-ugly it
                        if (pStatRes.Results[0] != "Ineffective")
                            foreach (string effectNested in actor.AttackEffects)
                                if (effect != effectNested)
                                    target.AddPermStatus(permStat);
                    }

                    if (Enum.TryParse<TempStatus>(effect, out TempStatus tempStat))
                    {
                        Spell tStatSpell = SpellManager.GetSpellByName(touchToSpellMap[effect]);
                        SpellResult tStatRes = SpellManager.CastSpell(actor, target, tStatSpell, totalHits);

                        // If one status spell hits, they are all applied. TODO: This is ugly. Un-ugly it
                        if (tStatRes.Results[0] != "Ineffective")
                            foreach (string effectNested in actor.AttackEffects)
                                if (effect != effectNested)
                                    target.AddTempStatus(tempStat);
                    }
                }
            }

            // Apply the damage and return the overall results
            target.DamageHP(damage);
            return new AttackResult(totalHits, damage, results);
        }

        /// <summary>
        /// Return the total number of successful hits by the acting monster against a target
        /// </summary>
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
