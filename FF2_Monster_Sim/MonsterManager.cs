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

        public static List<string> MonsterNames;
        private static List<string> smallMonsterNames;
        private static List<string> mediumMonsterNames;
        private static List<string> tallMonsterNames;
        private static List<string> largeMonsterNames;

        //////////////
        // Monogame //
        //////////////

        public static void Initialize()
        {
            MonsterNames = new List<string>();
            smallMonsterNames = new List<string>();
            mediumMonsterNames = new List<string>();
            tallMonsterNames = new List<string>();
            largeMonsterNames = new List<string>();
        }

        public static void LoadContent()
        {
            // Read MonsterData.json and load it
            Debug.WriteLine("MonsterManager LoadContent");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "\\Content\\Data\\FF2_MonsterData.json");
            monsterData = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@"Content\\Data\\FF2_MonsterData.json"));
            Debug.WriteLine("Loaded " + GetMonsterNames().Count + " monsters");
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

                    // TODO: Grab the monster's size and append it's name to a size list
                    /*
                    switch (data.size)
                    {
                        case "SMALL":
                            smallMonsterNames.Add((string)data.name);
                            break;
                        case "MEDIUM":
                            smallMonsterNames.Add((string)data.name);
                            break;
                        case "TALL":
                            smallMonsterNames.Add((string)data.name);
                            break;
                        case "LARGE":
                            smallMonsterNames.Add((string)data.name);
                            break;
                        default:
                            throw new Exception("Invalid monster size: " + data.size + ", found on " + data.name);
                    }
                    */

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

        /// <summary>
        /// Returns a list of monster names suitable for filling a given scene type
        /// </summary>
        /// <param name="sceneType">Type of scene to be filled. Must be "A", "B", or "C"</param>
        public static List<string> GenerateMonsterList(string sceneType)
        {
            // TODO: x/100 chance to grab a pre-made team from a file and return that

            switch (sceneType.ToUpper())
            {
                case "A":
                    // Generate a list with 8 small monsters
                    break;
                case "B":
                    // Determine which slots will be "tall", if any
                    // Grab corresponding medium and tall monsters
                    break;
                case "C":
                    // Grab a single large enemy
                    break;
                default:
                    throw new ArgumentException("Invalid sceneType supplied: " + sceneType);
            }

            return new List<string>();
        }
    }
}
