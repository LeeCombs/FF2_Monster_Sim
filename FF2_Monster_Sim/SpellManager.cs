using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;

namespace FF2_Monster_Sim
{
    public enum SpellType
    {
        White,
        Black,
        Special
    }

    public struct SpellResult
    {
        public int Damage;
        public List<string> Results;

        public SpellResult(int damage, List<string> results)
        {
            Damage = damage;
            Results = results;
        }

        public SpellResult(int damage)
        {
            Damage = damage;
            Results = new List<string>();
        }

        public SpellResult(List<string> results)
        {
            Damage = -1;
            Results = results;
        }
    }

    public class SpellManager
    {
        private static Random rnd;
        private static dynamic spellData;

        public SpellManager()
        {
            //
        }

        public static void Initialize()
        {
            rnd = new Random();
        }

        public static void LoadContent()
        {
            // Read SpellData.json and load it
            Debug.WriteLine("MonsterManager LoadContent");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "\\Content\\Data\\FF2_SpellData.json");
            spellData = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@"Content\\Data\\FF2_SpellData.json"));

            Monster monster = MonsterManager.GetMonsterByName("Wizard");
        }

        /// <summary>
        /// Retrieve a Spell object by name. Returns null if invalid.
        /// </summary>
        /// <returns>The Spell object if found, null if error.</returns>
        public static Spell GetSpellByName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                Debug.WriteLine("Null or Empty name supplied");
                return null;
            }

            foreach (dynamic data in spellData)
            {
                if (String.Equals(name, (string)data.name, StringComparison.OrdinalIgnoreCase))
                {
                    Spell spl = new Spell();
                    spl.Name = (string)data.name;
                    spl.SpellType = (SpellType)Enum.Parse(typeof(SpellType), (string)data.type);
                    spl.Effect = (string)data.effect;
                    spl.Power = (int)data.power;
                    spl.Accuracy = (int)data.accuracy;
                    spl.Status = (string)data.status;
                    spl.Element = (Element)Enum.Parse(typeof(Element), (string)data.element);
                    spl.Price = (int)data.price;
                    spl.Value = (int)data.value;
                    spl.SuccessMessage = (string)data.successMessage;
                    return spl;
                }
            }

            // Return null if no match was found
            Debug.WriteLine("No spell found by name: " + name);
            return null;
        }

        /// <summary>
        /// Cast a spell against a target. Applies the effects and returns the result.
        /// </summary>
        /// <param name="multiTarget">Whether the spell being cast is targeting multiple monsters. Halves accuracy and quarters power.</param>
        /// <returns>Result of casting the spell</returns>
        public static SpellResult CastSpell(Monster caster, Monster target, Spell spell, int level, bool multiTarget = false)
        {
            Debug.WriteLine("Casting spell: " + spell.Name + " " + level);

            // Helpers
            SpellResult failedResult = new SpellResult(new List<string> { "Ineffective" });
            SpellResult statusSuccessResult = new SpellResult(new List<string> { spell.SuccessMessage });

            if (level <= target.GetBuffStacks(Buff.Wall))
            {
                Debug.WriteLine("Wall stacks exceed spell level");
                return failedResult;
            }

            // Reduce accuracy and power if multi-targetting
            int adjustedAccuracy = spell.Accuracy;
            if (multiTarget) adjustedAccuracy = adjustedAccuracy / 2;
            int adjustedPower = spell.Power;
            if (multiTarget) adjustedPower = adjustedPower / 4;

            // Check for absorption. No effect except HP gain. All spells calculate damage.
            if (target.IsAbsorbantTo(spell.Element))
            {
                int totalHeal = GetDamage(adjustedAccuracy, level);
                Debug.WriteLine("Healing: " + totalHeal);
                target.HealHP(totalHeal);
                return new SpellResult(new List<string> { "HP up!" });
            }
            
            //// Notes of below
            // Resistances: Success fail. Damage spells achieve minimum successes (level) and halve damage
            // Weaknesses: Success is automatic. Damage spells achieve perfect successes (lvl*2) and double damage
            // If resisted and weak, successes fail, damage is not halved or doubled
            // Strictly positive effects not subject to Magic Resistance. (e.g. CURE, BLINK, etc.)
            switch (spell.Effect)
            {
                case "Damage":
                case "Damage_2": // TODO: Is this just normal damage?
                case "Damage_3": // TODO: Is this just normal damage?
                    if (target.IsResistantTo(spell.Element))
                    {
                        return HandleDamageSpellResult(target, GetDamage(adjustedPower, level) / 2);
                    }
                    if (target.IsWeakTo(spell.Element))
                    {
                        return HandleDamageSpellResult(target, GetDamage(adjustedPower, level  *2) * 2);
                    }
                    if (target.IsWeakTo(spell.Element) && target.IsResistantTo(spell.Element))
                    {
                        // Damage as usual. Best guess at hits: level - blocks
                        int rwHits = level - GetMagicBlocks(target);
                        return HandleDamageSpellResult(target, GetDamage(adjustedPower, rwHits));
                    }
                    // Normal logic
                    int hits = level + GetSuccesses(level, adjustedAccuracy) - GetMagicBlocks(target);
                    return HandleDamageSpellResult(target, GetDamage(adjustedPower, hits));
                case "Damage_Ultima":
                    // TODO: Ultima damage bug
                    break;
                case "Heal":
                    // This is CURE, not HEAL. Damage undead, heal otherwise
                    if (target.Families.Contains(MonsterFamily.Undead))
                    {
                        int healHits = GetHitsAgainstTarget(level, adjustedAccuracy, target);
                        return HandleDamageSpellResult(target, GetDamage(adjustedPower, healHits));
                    }
                    int totalHeal = GetDamage(adjustedPower, GetSuccesses(level, adjustedAccuracy));
                    target.HealHP(totalHeal);
                    return new SpellResult(new List<string> { "HP up!" });
                case "Revive":
                    // Chance to kill undead, otherwise fail. Fails if multi-targeting.
                    if (multiTarget) return failedResult;
                    if (target.Families.Contains(MonsterFamily.Undead))
                    {
                        if (GetHitsAgainstTarget(level, adjustedAccuracy, target) > 0)
                        {
                            target.Kill();
                            return new SpellResult(new List<string> { target.Name + " fell", "Collapsed" });
                        }
                    }
                    break;
                case "Buff":
                    // Autohits, ignores magic resistance rolls
                    Buff buff = (Buff)Enum.Parse(typeof(Buff), spell.Status);
                    int buffHits = GetSuccesses(level, adjustedAccuracy);
                    if (buffHits == 0) return failedResult;
                    target.AddBuff(buff, buffHits);

                    // AURA and BARR have their unique results based on # of successes. Messages are displayed highest to lowest.
                    switch (spell.Name)
                    {
                        case "AURA":
                            string[] auraMessages = { "White", "Yellow", "Green", "Black", "Blue", "Orange", "Red" };
                            List<string> auraMsgList = new List<string>();
                            buffHits--;
                            for (int i = buffHits > 6 ? 6 : buffHits; i >= 0; i--)
                            {
                                auraMsgList.Add(auraMessages[i] + " Aura");
                            }
                            return new SpellResult(auraMsgList);
                        case "BARR":
                            string[] barrMessages = { "Ice", "Critical Hit!", "Poison", "Death", "Bolt", "Soul", "Fire" };
                            List<string> barrMsgList = new List<string>();
                            buffHits--;
                            for (int i = buffHits > 6 ? 6 : buffHits; i >= 0; i--)
                            {
                                barrMsgList.Add(barrMessages[i] + " Df");
                            }
                            return new SpellResult(barrMsgList);
                        default:
                            return statusSuccessResult;
                    }
                case "Debuff":
                    if (target.IsResistantTo(spell.Element)) return failedResult;
                    if (target.IsWeakTo(spell.Element))
                    {
                        Debuff debuff = (Debuff)Enum.Parse(typeof(Debuff), spell.Status);
                        target.AddDebuff(debuff, level); // TODO: level + successes?
                        // TODO: DSPL has unique results messages based on # of successes
                        return statusSuccessResult;
                    }
                    // Normal logic
                    int debuffHits = GetHitsAgainstTarget(level, adjustedAccuracy, target);
                    if (debuffHits > 0)
                    {
                        Debuff debuff = (Debuff)Enum.Parse(typeof(Debuff), spell.Status);
                        target.AddDebuff(debuff, debuffHits);
                        // TODO: DSPL has unique results messages based on # of successes
                        return statusSuccessResult;
                    }
                    break;
                case "TempStatus":
                    if (target.IsResistantTo(spell.Element)) return new SpellResult(new List<string> { "Ineffective" });
                    if (target.IsWeakTo(spell.Element))
                    {
                        TempStatus tempStatus = (TempStatus)Enum.Parse(typeof(TempStatus), spell.Status);
                        target.AddTempStatus(tempStatus);
                        return statusSuccessResult;
                    }
                    // Normal logic. A single hit = success
                    if (GetHitsAgainstTarget(level, adjustedAccuracy, target) > 0)
                    {
                        TempStatus tempStatus = (TempStatus)Enum.Parse(typeof(TempStatus), spell.Status);
                        target.AddTempStatus(tempStatus);
                        return statusSuccessResult;
                    }
                    break;
                case "PermStatus":
                    if (target.IsResistantTo(spell.Element)) return failedResult;
                    if (target.IsWeakTo(spell.Element))
                    {
                        PermStatus permStatus = (PermStatus)Enum.Parse(typeof(PermStatus), spell.Status);
                        target.AddPermStatus(permStatus);
                        return statusSuccessResult;
                    }
                    // Normal logic. A single hit = success
                    if (GetHitsAgainstTarget(level, adjustedAccuracy, target) > 0)
                    {
                        PermStatus permStatus = (PermStatus)Enum.Parse(typeof(PermStatus), spell.Status);
                        target.AddPermStatus(permStatus);
                        return statusSuccessResult;
                    }
                    break;
                case "CureTempStatus":
                    // This is PEEP
                    // 1 = Venom & Sleep, then one more per level, up to 5
                    TempStatus[] tempCureOrder = { TempStatus.Venom, TempStatus.Sleep, TempStatus.Mini, TempStatus.Mute, TempStatus.Paralysis, TempStatus.Confuse };
                    String[] peepMsgOrder = { "Devenomed", "Scared", "Grew", "Can speak", "Can move", "Normal" };
                    target.RemoveTempStatus(TempStatus.Venom);
                    List<string> peepMsgs = new List<string>{ "Devenomed" };
                    for (int i = 1; i < level; i++)
                    {
                        if (i >= tempCureOrder.Length) break;
                        target.RemoveTempStatus(tempCureOrder[i]);
                        peepMsgs.Add(peepMsgOrder[i]);
                    }
                    // TODO: If nothing is cured, is ineffective returned?
                    return new SpellResult(peepMsgs); ;
                case "CurePermStatus":
                    // This is HEAL. Cure everything up to and including level.
                    PermStatus[] permCureOrder = { PermStatus.Darkness, PermStatus.Poison, PermStatus.Curse, PermStatus.Amnesia, PermStatus.Toad, PermStatus.Stone, PermStatus.KO };
                    String[] healMsgOrder = { "Can see", "Devenomed", "Uncursed", "Remembers", "Regained form", "Normal body", "" };
                    
                    List<string> healMsgs = new List<string>();
                    for (int i = 0; i < level; i++)
                    {
                        if (i >= permCureOrder.Length) break;
                        target.RemovePermStatus(permCureOrder[i]);
                        healMsgs.Add(healMsgOrder[i]);
                    }
                    // TODO: If nothing is cured, is ineffective returned?
                    return new SpellResult(healMsgs);
                case "Special":
                    // TODO: HP Drain, MP Drain, Swap, Halve MP
                    switch (spell.Name.ToUpper())
                    {
                        case "DRAN": // Drain HP
                            // Each hit target loses 1/16 (round down) of Max HP. Caster gains that amount
                            int hpDrainAmt = target.HPMax / 16; // NOTE: Int division
                            int dranHits = GetHitsAgainstTarget(level, adjustedAccuracy, target);
                            if (dranHits > 0)
                            {
                                int dranAmt = hpDrainAmt * dranHits;
                                // Opposite effect vs. Undead
                                if (target.Families.Contains(MonsterFamily.Undead))
                                {
                                    caster.DamageHP(dranAmt);
                                    target.HealHP(dranAmt);
                                }
                                else
                                {
                                    caster.HealHP(dranAmt);
                                    target.DamageHP(dranAmt);
                                }
                                return statusSuccessResult;
                            }
                            break;
                        case "ASPL": // Drain MP
                            // Each hit target loses 1/16 (round down) of Max MP. Caster gains that amount
                            int mpDrainAmt = target.MPMax / 16; // NOTE: Int division
                            int asplHits = GetHitsAgainstTarget(level, adjustedAccuracy, target);
                            if (asplHits > 0)
                            {
                                int asplAmt = mpDrainAmt * asplHits;
                                // Opposite effect vs. Undead
                                if (target.Families.Contains(MonsterFamily.Undead))
                                {
                                    caster.DamageMP(asplAmt);
                                    target.HealMP(asplAmt);
                                }
                                else
                                {
                                    caster.HealMP(asplAmt);
                                    target.DamageMP(asplAmt);
                                }
                                return statusSuccessResult;
                            }
                            break;
                        case "ANTI": // Halve MP
                            if (target.IsResistantTo(spell.Element)) return failedResult;
                            if (GetHitsAgainstTarget(level, adjustedAccuracy, target) > 0)
                            {
                                // TODO: Target loses half of MP
                                // NES_BUG: This only effects the first byte of the MP value
                                return statusSuccessResult;
                            }
                            break;
                        case "CHNG": // Caster and Target swap HP and MP
                            if (target.IsResistantTo(spell.Element)) return failedResult;
                            if (GetHitsAgainstTarget(level, adjustedAccuracy, target) > 0)
                            {
                                int casterHP = caster.HP;
                                int casterMP = caster.MP;
                                caster.HP = target.HP;
                                caster.MP = target.MP;
                                target.HP = casterHP;
                                target.MP = casterMP;
                                // caster.MP -= MPConsumption
                                return statusSuccessResult;
                            }
                            break;
                        case "BLAST":
                            // Bomb's Explosion Spell. It's kinda dumb...
                            // Fails if HP is full. Acts like a physical attack...
                            // Deals ((20...40) - Defense) * level
                            if (caster.HP == caster.HPMax) return failedResult;
                            break;
                        default:
                            Debug.WriteLine("Invalid spell found at speical: " + spell.Name);
                            break;
                    }
                    break;
                default:
                    Debug.WriteLine("Invalid spell effect found: " + spell.Effect);
                    break;
            }

            return failedResult;
        }


        /////////////
        // Helpers //
        /////////////


        /// <summary>
        /// Calculate how many successes were found (e.g. 5 rolls at 50% accuracy chance)
        /// </summary>
        private static int GetSuccesses(int rolls, int accuracy)
        {
            int successes = 0;
            for (int i = 0; i < rolls; i++)
            {
                if (rnd.Next(0, 100) < accuracy) successes++;
            }
            return successes;
        }

        /// <summary>
        /// Calculate how many amgic blocks a monster was able to perform. (e.g. 5 blocks at 50% evasion chance)
        /// </summary>
        private static int GetMagicBlocks(Monster monster)
        {
            int successes = 0;
            for (int i = 0; i < monster.MagicBlocks; i++)
            {
                if (rnd.Next(0, 100) < monster.MagicEvasion) successes++;
            }
            return successes;
        }

        /// <summary>
        /// Dumb little helper to help readability
        /// </summary>
        private static int GetHitsAgainstTarget(int level, int accuracy, Monster target)
        {
            return GetSuccesses(level, accuracy) - GetMagicBlocks(target);
        }

        /// <summary>
        /// Get the sum of damage rolls (power...2*power per hit)
        /// </summary>
        private static int GetDamage(int power, int hits)
        {
            int sum = 0;
            for (int i = 0 ; i < hits ; i++)
            {
                sum += rnd.Next(power, 2 * power);
            }
            return sum;
        }

        /// <summary>
        /// Deal damage to a target, and build and return the result.
        /// </summary>
        private static SpellResult HandleDamageSpellResult(Monster target, int damage)
        {
            Debug.WriteLine("Damaging " + target.Name + " for " + damage);
            SpellResult res = new SpellResult(damage);
            target.DamageHP(damage);
            if (target.IsDead()) res.Results.Add(target.Name + " fell");
            return res;
        }
    }
}
