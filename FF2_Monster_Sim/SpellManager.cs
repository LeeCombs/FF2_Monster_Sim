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
                Debug.WriteLine("Healing: " + GetDamage(adjustedPower, level));
                return "Absorbs";
            }

            // Damage/Heal Spells spells
            // Get number of "hits" level + successes
            // Hits deal (power...2*power)
            // Halved after sum on resist, doubles on weakness
            
            //// Elemental considerations

            // Resistance
            // Success-based properties fail, making them immune to status ailments caused by spells
            // Damage spells achieve minimum successes (level), then halve after calculation
            // If resist and weak, spells will fail, damage not be halved or doubled

            // Weakness
            // All spells achieve perfect success, status ailments auto-hit
            // Damage spells achieve successes equal to level. Damage doubled after calculation.

            // Strictly positive effects not subject to Magic Resistance. (e.g. CURE, BLINK, etc.)

            switch (spell.Effect)
            {
                case "Damage":
                case "Damage_2": // TODO: Is this just normal damage?
                case "Damage_3": // TODO: Is this just normal damage?
                    if (target.IsResistantTo(spell.Element))
                    {
                        // Minimum hits (level), then halve damage
                        Debug.WriteLine("Resist! Damaging: " + GetDamage(adjustedPower, level) / 2);
                        return "resist";
                    }
                    if (target.IsWeakTo(spell.Element))
                    {
                        // Minimum hits (level * 2), then double damage
                        Debug.WriteLine("Weak! Damaging: " + GetDamage(adjustedPower, level * 2) * 2);
                        return "weak";
                    }
                    // TODO: Weak and Resist don't modify damage, spells fail. Is that successes?
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
                    target.Heal(totalHeal);
                    // TODO: If undead, treat as damage
                    break;
                case "Revive":
                    // TODO: Life is effective against undead
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
                        // Auto-success
                        Debuff debuff = (Debuff)Enum.Parse(typeof(Debuff), spell.Status);
                        target.AddDebuff(debuff, level); // TODO: level + successes?
                        return "success";
                    }
                    // TODO: Normal logic
                    break;
                case "TempStatus":
                    if (target.IsResistantTo(spell.Element)) return "ineffective";
                    if (target.IsWeakTo(spell.Element))
                    {
                        // Auto-success
                        TempStatus tempStatus = (TempStatus)Enum.Parse(typeof(TempStatus), spell.Status);
                        target.AddTempStatus(tempStatus);
                        return "success";
                    }
                    // TODO: Normal logic
                    // A single hit = success?
                    break;
                case "PermStatus":
                    if (target.IsResistantTo(spell.Element)) return "ineffective";
                    if (target.IsWeakTo(spell.Element))
                    {
                        // Auto-success
                        PermStatus permStatus = (PermStatus)Enum.Parse(typeof(PermStatus), spell.Status);
                        target.AddPermStatus(permStatus);
                        return "success";
                    }
                    // TODO: Normal logic
                    // A single hit = success?
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
                    break;
                default:
                    Debug.WriteLine("Invalid spell effect found: " + spell.Effect);
                    break;
            }

            return "Fail";
        }

        private static int GetSuccesses(int rolls, int accuracy)
        {
            int successes = 0;
            for (int i = 0; i < rolls; i++)
            {
                if (rnd.Next(0, 100) < accuracy) successes++;
            }
            return successes;
        }

        private static int GetDamage(int power, int hits)
        {
            int sum = 0;
            for (int i = 0 ; i < hits ; i++)
            {
                sum += rnd.Next(power, 2 * power);
            }
            return sum;
        }

        private static int GetMagicBlocks(Monster monster)
        {
            int successes = 0;
            for (int i = 0; i < monster.MagicBlocks; i++)
            {
                if (rnd.Next(0, 100) < monster.MagicEvasion) successes++;
            }
            return successes;
        }
    }
}
