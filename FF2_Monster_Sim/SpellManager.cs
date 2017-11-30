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
        /// <param name="name"></param>
        /// <returns></returns>
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
                    // Load spell
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

                    return spl;
                }
            }

            // Return null if no match was found
            Debug.WriteLine("No spell found by name: " + name);
            return null;
        }

        public static string CastSpell(Monster caster, Monster target, Spell spell, int level, bool multiTarget = false)
        {
            Debug.WriteLine("Casting spell: " + spell.Name + " " + level);

            // Reduce accuracy and power if multi-targetting
            int adjustedAccuracy = spell.Accuracy;
            if (multiTarget) adjustedAccuracy = adjustedAccuracy / 2;
            int adjustedPower = spell.Power;
            if (multiTarget) adjustedPower = adjustedPower / 4;

            // Check for absorption
            // No effect except HP gain. Damaging and non-damaging spells calculate for healing
            // Ignore weakness, resistance, and magic defense rolls
            if (target.IsAbsorbantTo(spell.Element))
            {
                int totalHeal = GetDamage(adjustedAccuracy, level);
                Debug.WriteLine("Healing: " + totalHeal);
                target.HealHP(totalHeal);
                return "Absorbed";
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
                        Debug.WriteLine("Resist! Damaging: " + GetDamage(adjustedPower, level) / 2);
                        return "resist";
                    }
                    if (target.IsWeakTo(spell.Element))
                    {
                        Debug.WriteLine("Weak! Damaging: " + GetDamage(adjustedPower, level * 2) * 2);
                        return "weak";
                    }
                    if (target.IsWeakTo(spell.Element) && target.IsResistantTo(spell.Element))
                    {
                        // Damage as usual. Best guess at hits: level - blocks
                        int rwHits = level - GetMagicBlocks(target);
                        Debug.WriteLine("Resist and Weak! " + rwHits + " hits. Damaging: " + GetDamage(adjustedPower, rwHits));
                        return "weak";
                    }
                    // Normal logic
                    int hits = level + GetSuccesses(level, adjustedAccuracy) - GetMagicBlocks(target);
                    Debug.WriteLine(hits + " hits. Damaging: " + GetDamage(adjustedPower, hits));
                    break;
                case "Damage_Ultima":
                    // TODO: Ultima damage bug
                    break;
                case "Heal":
                    // Roll damage like usual, except heal
                    int totalHeal = GetDamage(adjustedPower, GetSuccesses(level, adjustedAccuracy));
                    Debug.WriteLine("Healing: " + totalHeal);
                    target.HealHP(totalHeal);
                    // TODO: If undead, treat as damage. Do they roll to resist?
                    break;
                case "Revive":
                    // TODO: Life is effective against undead, useless otherwise
                    break;
                case "Buff":
                    // Autohits, ignores magic resistance rolls
                    Buff buff = (Buff)Enum.Parse(typeof(Buff), spell.Status);
                    target.AddBuff(buff, GetSuccesses(level, adjustedAccuracy));
                    break;
                case "Debuff":
                    if (target.IsResistantTo(spell.Element)) return "ineffective";
                    if (target.IsWeakTo(spell.Element))
                    {
                        Debuff debuff = (Debuff)Enum.Parse(typeof(Debuff), spell.Status);
                        target.AddDebuff(debuff, level); // TODO: level + successes?
                        return "success";
                    }
                    // Normal logic
                    int debuffHits = GetHitsAgainstTarget(level, adjustedAccuracy, target);
                    if (debuffHits > 0)
                    {
                        Debuff debuff = (Debuff)Enum.Parse(typeof(Debuff), spell.Status);
                        target.AddDebuff(debuff, level);
                    }
                    break;
                case "TempStatus":
                    if (target.IsResistantTo(spell.Element)) return "ineffective";
                    if (target.IsWeakTo(spell.Element))
                    {
                        TempStatus tempStatus = (TempStatus)Enum.Parse(typeof(TempStatus), spell.Status);
                        target.AddTempStatus(tempStatus);
                        return "success";
                    }
                    // Normal logic. A single hit = success
                    if (GetHitsAgainstTarget(level, adjustedAccuracy, target) > 0)
                    {
                        TempStatus tempStatus = (TempStatus)Enum.Parse(typeof(TempStatus), spell.Status);
                        target.AddTempStatus(tempStatus);
                        return "success";
                    }
                    break;
                case "PermStatus":
                    if (target.IsResistantTo(spell.Element)) return "ineffective";
                    if (target.IsWeakTo(spell.Element))
                    {
                        PermStatus permStatus = (PermStatus)Enum.Parse(typeof(PermStatus), spell.Status);
                        target.AddPermStatus(permStatus);
                        return "success";
                    }
                    // Normal logic. A single hit = success
                    if (GetHitsAgainstTarget(level, adjustedAccuracy, target) > 0)
                    {
                        PermStatus permStatus = (PermStatus)Enum.Parse(typeof(PermStatus), spell.Status);
                        target.AddPermStatus(permStatus);
                        return "success";
                    }
                    break;
                case "CureTempStatus":
                    // This is PEEP/Basuna
                    // 1 = Venom & Sleep, then one more per level, up to 5
                    TempStatus[] tempCureOrder = { TempStatus.Venom, TempStatus.Sleep, TempStatus.Mini, TempStatus.Mute, TempStatus.Paralysis, TempStatus.Confuse };
                    target.RemoveTempStatus(TempStatus.Venom);
                    for (int i = 1; i < level; i++)
                    {
                        if (i >= tempCureOrder.Length) break;
                        target.RemoveTempStatus(tempCureOrder[i]);
                    }
                    break;
                case "CurePermStatus":
                    // This is HEAL/Esuna. Cure everything up to and including level.
                    PermStatus[] permCureOrder = { PermStatus.Darkness, PermStatus.Poison, PermStatus.Curse, PermStatus.Amnesia, PermStatus.Toad, PermStatus.Stone, PermStatus.KO };
                    for (int i = 0; i < level; i++)
                    {
                        if (i >= permCureOrder.Length) break;
                        target.RemovePermStatus(permCureOrder[i]);
                    }
                    break;
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
                                return "success";
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
                                return "success";
                            }
                            break;
                        case "ANTI": // Halve MP
                            if (target.IsResistantTo(spell.Element)) return "failed";
                            if (GetHitsAgainstTarget(level, adjustedAccuracy, target) > 0)
                            {
                                // Target loses half of MP
                                // NES_BUG: This only effects the first byte of the MP value
                                return "success";
                            }
                            break;
                        case "CHNG": // Caster and Target swap HP and MP
                            if (target.IsResistantTo(spell.Element)) return "failed";
                            if (GetHitsAgainstTarget(level, adjustedAccuracy, target) > 0)
                            {
                                int casterHP = caster.HP;
                                int casterMP = caster.MP;
                                caster.HP = target.HP;
                                caster.MP = target.MP;
                                target.HP = casterHP;
                                target.MP = casterMP;
                                // caster.MP -= MPConsumption
                                return "success";
                            }
                            break;
                        case "BLAST":
                            // Bomb's Explosion Spell
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

            return "Fail";
        }

        /// <summary>
        /// Calculate how many successes were found (e.g. 5 rolls at 50% accuracy chance)
        /// </summary>
        /// <param name="rolls"></param>
        /// <param name="accuracy"></param>
        /// <returns></returns>
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
        /// <param name="monster"></param>
        /// <returns></returns>
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
        /// <param name="power"></param>
        /// <param name="hits"></param>
        /// <returns></returns>
        private static int GetDamage(int power, int hits)
        {
            int sum = 0;
            for (int i = 0 ; i < hits ; i++)
            {
                sum += rnd.Next(power, 2 * power);
            }
            return sum;
        }
    }
}
