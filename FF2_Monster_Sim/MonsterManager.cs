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
    public static class MonsterManager
    {
        private static dynamic monsterData;

        //////////////
        // Monogame //
        //////////////

        public static void Initialize()
        {
            //
        }

        public static void LoadContent()
        {
            // Read MonsterData.json and load it
            Debug.WriteLine("MonsterManager LoadContent");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "\\Content\\Data\\FF2_MonsterData.json");
            monsterData = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@"Content\\Data\\FF2_MonsterData.json"));
            Debug.WriteLine("Loaded: " + GetMonsterNames().Count + " monsters");
        }

        /////////////
        // Publics //
        /////////////

        /// <summary>
        /// Retrieve a Monster object using the supplied name
        /// </summary>
        /// <param name="name">Any given name of the monster (ignores case)</param>
        /// <returns>The Monster, if any. Null if there's an error.</returns>
        public static Monster GetMonsterByName(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Invalid monster name supplied");

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
                    mon.Strength = data.strength;
                    mon.Hits = data.hits;
                    mon.Defense = data.defense;
                    mon.Blocks = data.blocks;
                    mon.Evasion = data.evade;
                    mon.MagicBlocks = data.mBlocks;
                    mon.Accuracy = data.accuracy;
                    mon.MagicEvasion = data.mEvade;
                    mon.Fear = data.cowardice;
                    mon.AttackEffects = data.attackEffect.ToObject<HashSet<String>>();
                    mon.ActionList = data.attackList.ToObject<List<MonsterAction>>();
                    mon.Families = data.race.ToObject<HashSet<MonsterFamily>>();
                    mon.Weaknesses = data.weak.ToObject<HashSet<Element>>();
                    mon.Resistances = data.resist.ToObject<HashSet<Element>>();
                    mon.Absorbs = data.absorb.ToObject<HashSet<Element>>();

                    // Below may or may not be implemented
                    // mon.Level = data.skillLevel;
                    // mon.GilDrops = data.gilDrops.ToObject<List<String>>();
                    // mon.ItemDrops = data.itemDrops.ToObject<List<String>>();

                    return mon;
                }
            }

            throw new Exception("No monster found by name: " + name);
        }

        /// <summary>
        /// Return a HashSet of all the monsters names within monster data
        /// </summary>
        public static HashSet<string> GetMonsterNames()
        {
            HashSet<string> nameSet = new HashSet<string>();
            foreach (dynamic data in monsterData)
                nameSet.Add((string)data.name);
            return nameSet;
        }
    }
}
