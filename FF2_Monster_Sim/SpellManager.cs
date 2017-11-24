﻿using System;
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
                target.Heal(totalHeal);
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
                        // TODO: Damage as usual, determine how hits are determined.
                        // Best Guess: level - blocks
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
                    target.Heal(totalHeal);
                    // TODO: If undead, treat as damage
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
                    // Normal logic
                    // TODO: A single hit = success?
                    int tsHits = GetSuccesses(level, adjustedAccuracy) - GetMagicBlocks(target);
                    if (tsHits > 0)
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
                        // Auto-success
                        PermStatus permStatus = (PermStatus)Enum.Parse(typeof(PermStatus), spell.Status);
                        target.AddPermStatus(permStatus);
                        return "success";
                    }
                    // Normal logic
                    // TODO: A single hit = success?
                    int psHits = GetSuccesses(level, adjustedAccuracy) - GetMagicBlocks(target);
                    if (psHits > 0)
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
                    return HandleSpecialSpell(spell);
                    break;
                default:
                    Debug.WriteLine("Invalid spell effect found: " + spell.Effect);
                    break;
            }

            return "Fail";
        }

        /// <summary>
        /// Helper for handling special spells HP/MP drain, Swap, and Halve-MP
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        private static string HandleSpecialSpell(Spell spell)
        {
            switch (spell.Name.ToUpper())
            {
                case "DRAN":
                    // Drain HP
                    break;
                case "ASPL":
                    // Drain MP
                    // TODO: Deducts MP cost after the first target?
                    break;
                case "ANTI":
                    // Halve MP
                    // BUG: Only effects first byte of MP
                    break;
                case "CHNG":
                    // Swap HP and MP
                    // TODO: Deducts MP cost after the first target?
                    break;
                default:
                    break;
            }
            return "";
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
