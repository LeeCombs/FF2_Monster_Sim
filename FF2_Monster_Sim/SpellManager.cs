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
        private Random rnd;
        private dynamic spellData;

        public SpellManager()
        {
            //
        }

        public void Initialize()
        {
            //
            rnd = new Random();
        }

        public void LoadContent()
        {
            // Read SpellData.json and load it
            Debug.WriteLine("MonsterManager LoadContent");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "\\Content\\Data\\FF2_SpellData.json");
            spellData = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@"Content\\Data\\FF2_SpellData.json"));

            Spell spl = GetSpellByName("CURE");
        }

        public Spell GetSpellByName(string name)
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

                    spl.name = (string)data.name;
                    spl.spellType = (SpellType)Enum.Parse(typeof(SpellType), (string)data.type);
                    spl.effect = (string)data.effect;
                    spl.power = (int)data.power;
                    spl.accuracy = (int)data.accuracy;
                    spl.status = (string)data.status;
                    spl.element = (string)data.element;
                    spl.price = (int)data.price;
                    spl.value = (int)data.value;

                    return spl;
                }
            }

            // Return null if no match was found
            Debug.WriteLine("No spell found by name: " + name);
            return null;
        }

        public string CastSpell(Monster caster, Monster target, Spell spell, int level, bool multiTarget = false)
        {
            // Reduce accuracy and power if multi-targetting
            int adjustedAccuracy = spell.accuracy;
            if (multiTarget) adjustedAccuracy = adjustedAccuracy / 2;

            int power = spell.power;
            if (multiTarget) power = power / 4;

            // Damage/Heal Spells spells
            // Get number of "hits" level + successes
            // Hits deal (power...2*power)
            // Halved after sum on resist, doubles on weakness
            
            //// Elemental considerations 

            // Absorb
            // No effect except HP gain
            // Damage = Heal. Non-damage -> Heal calculated as if they dealt damage
            // Ignores weakness/resistance
            // No MagicDefense roll

            // Resistance
            // Success-based properties fail, making them immune to status ailments caused by spells
            // Damage spells achieve minimum successes (level), then halve after calculation
            // If resist and weak, spells will wail, damage not be halved or doubled

            // Weakness
            // All spells achieve perfect success, status ailments auto-hit
            // Damage spells achieve successes equal to level. Damage doubled after calculation.

            // Strictly positive effects not subject to Magic Resistance. (e.g. CURE, BLINK, etc.)

            switch (spell.effect)
            {
                case "Damage":
                    break;
                case "Damage_2":
                    break;
                case "Damage_3":
                    break;
                case "Damage_Ultima":
                    break;
                case "Heal":
                    break;
                case "Revive":
                    break;
                case "Buff":
                    Buff buff = (Buff)Enum.Parse(typeof(Buff), spell.name);
                    target.AddBuff(buff, GetSuccesses(level, spell.accuracy));
                    break;
                case "Debuff":
                    break;
                case "TempStatus":
                    break;
                case "PermStatus":
                    break;
                case "CureTempStatus":
                    break;
                case "CurePermStatus":
                    break;
                case "Special":
                    // Drains, Swap, Half MP
                    break;
                default:
                    Debug.WriteLine("Invalid spell effect found: " + spell.effect);
                    break;
            }

            return "Whatever";
        }

        private int GetSuccesses(int rolls, int accuracy)
        {
            int successes = 0;
            for (int i = 0; i < rolls; i++)
            {
                if (rnd.Next(0, 100) < accuracy) successes++;
            }
            return successes;
        }

        private int GetDamage(int power, int hits)
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
