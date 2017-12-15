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
            HitsMessage = hits > 0 ? hits.ToString() + "xHit" : "Miss";
            DamageMessage = damage.ToString() + " DMG";
            Results = results ?? new List<string>();
        }
    }

    public class AttackManager
    {
        private static Random rnd;
        private const int CRIT_RATE = 5;
        private const int AURA_BONUS = 20;
        private const string CRIT_MESSAGE = "Critical Hit!";

        /// <summary>
        /// Helper to determine which spells to cast based on Status Touch effect
        /// </summary>
        private static readonly Dictionary<string, string> touchToSpellMap = new Dictionary<string, string>
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
            for (int i = 0; i < totalHits; i++)
            {
                // Get damage and add critical bonus damage if rolled
                damage += rnd.Next(attackScore, attackScore * 2 + 1) - target.Defense;
                if (rnd.Next(100) < CRIT_RATE)
                {
                    damage += attackScore;
                    results.Add(CRIT_MESSAGE);
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
            int totalHits = actor.RollHits() - target.RollBlocks();
            return totalHits > 0 ? totalHits : 0; 
        }
    }
}
