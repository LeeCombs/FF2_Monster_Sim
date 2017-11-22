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
    public enum MonsterFamily
    {
        Aquatic,
        Dragon,
        Earth,
        Giant,
        MagicBeast,
        Spellcaster,
        Undead,
        Werebeast
    }

    public static class MonsterManager
    {
        private static dynamic monsterData;
        

        public static void Initialize()
        {
            //
            Debug.WriteLine("monster init");
        }

        public static void LoadContent()
        {
            // Read MonsterData.json and load it
            Debug.WriteLine("MonsterManager LoadContent");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "\\Content\\Data\\FF2_MonsterData.json");
            monsterData = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@"Content\\Data\\FF2_MonsterData.json"));

            Monster mon = GetMonsterByName("LegEater");
        }
        
        /// <summary>
        /// Retrieve a Monster object using the supplied name
        /// </summary>
        /// <param name="name">Any given name of the monster (ignores case)</param>
        /// <returns>The Monster, if any. Null if there's an error.</returns>
        public static Monster GetMonsterByName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                Debug.WriteLine("Null or Empty name supplied");
                return null;
            }

            // Look through monster data and pick the matching monster by name
            foreach (dynamic data in monsterData)
            {
                if (String.Equals(name, (string)data.name, StringComparison.OrdinalIgnoreCase))
                {
                    Monster mon = new Monster();

                    // Doing this by hand since generated .json naming isn't consistent, nor formatted 100%
                    mon.Name = data.name;
                    mon.HPMax = data.HP; // Set HPMax before HP
                    mon.HP = data.HP;
                    mon.MPMax = data.MP; // Set MPMax before MP
                    mon.MP = data.MP;
                    // mon.Level = data.skillLevel;
                    mon.Strength = data.strength;
                    mon.Hits = data.hits;
                    mon.Defense = data.defense;
                    mon.Blocks = data.blocks;
                    mon.Evasion = data.evade;
                    mon.MagicBlocks = data.mBlocks;
                    mon.Accuracy = data.accuracy;
                    mon.MagicEvasion = data.mEvade;
                    mon.Fear = data.cowardice;

                    // Currently arrays are represented by strings and must be converted
                    mon.AttackEffects = Utils.StringToStringList((string)data.attackEffect);
                    mon.SpecialAttacks = Utils.StringToStringList((string)data.attackList);

                    // TODO: Consider converting string lists into enum lists of their corresponding types
                    mon.Families = Utils.StringToStringList((string)data.race);
                    mon.Weaknesses = Utils.StringToStringList((string)data.weak);
                    mon.Resistances = Utils.StringToStringList((string)data.resist);
                    mon.Absorbs = Utils.StringToStringList((string)data.absorb);

                    mon.GilDrops = Utils.StringToStringList((string)data.gilDrops);
                    mon.ItemDrops = Utils.StringToStringList((string)data.itemDrops);

                    return mon;
                }
            }
            
            Debug.WriteLine("No monster found by name: " + name);
            return null;
        }
    }
}
