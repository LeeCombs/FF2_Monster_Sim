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
        private static int TALL_CHANCE = 10;

        private static dynamic monsterData;

        public static List<string> MonsterNames { get; private set; }
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
            PopulateNameData();

            Debug.WriteLine("Loaded " + MonsterNames.Count + " monsters");
            Debug.WriteLine("\t" + smallMonsterNames.Count + " small");
            Debug.WriteLine("\t" + mediumMonsterNames.Count + " medium");
            Debug.WriteLine("\t" + tallMonsterNames.Count + " tall");
            Debug.WriteLine("\t" + largeMonsterNames.Count + " large");
        }

        /////////////
        // Publics //
        /////////////
        
        /// <summary>
        /// Retrieve a MonoMonster object using the supplied name
        /// </summary>
        /// <param name="name">Any given name of the monster (ignores case)</param>
        /// <returns>The MonoMonster, if any. Null if there's an error.</returns>
        public static Monster GetMonsterByName(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Invalid monster name supplied");

            // Look through monster data and pick the matching monster by name
            foreach (dynamic data in monsterData)
            {
                if (String.Equals(name, (string)data.name, StringComparison.OrdinalIgnoreCase))
                {
                    MonoMonster mon = new MonoMonster();
                    mon.size = data.size;
                    
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
        /// Returns a list of monster names suitable for filling a given scene type
        /// </summary>
        /// <param name="sceneType">Type of scene to be filled. Must be "A", "B", or "C"</param>
        public static List<string> GenerateMonsterList(string sceneType)
        {
            if (String.IsNullOrEmpty(sceneType))
                throw new ArgumentException("Invalid sceneType supplied");

            // TODO: x/100 chance to grab a pre-made team from a file and return that?

            List<string> nameList = new List<string>();
            int roll;

            switch (sceneType.ToUpper())
            {
                case "A":
                    // Generate a list with 8 small monsters
                    for (int i = 0; i < 8; i++)
                    {
                        roll = Globals.rnd.Next(smallMonsterNames.Count);
                        nameList.Add(smallMonsterNames[roll]);
                    }
                    break;
                case "B":
                    // Determine which slots will be designated for tall or medium enemies
                    // Per slot, add one tall enemy or two mediums
                    for (int i = 0; i < 3; i++)
                    {
                        if (Globals.rnd.Next(100) < TALL_CHANCE)
                            nameList.Add(tallMonsterNames[Globals.rnd.Next(tallMonsterNames.Count)]);
                        else
                        { 
                            nameList.Add(mediumMonsterNames[Globals.rnd.Next(mediumMonsterNames.Count)]);
                            nameList.Add(mediumMonsterNames[Globals.rnd.Next(mediumMonsterNames.Count)]);
                        }
                    }
                    break;
                case "C":
                    // Grab a single large enemy
                    roll = Globals.rnd.Next(largeMonsterNames.Count);
                    nameList.Add(largeMonsterNames[roll]);
                    break;
                default:
                    throw new ArgumentException("Invalid sceneType supplied: " + sceneType);
            }

            return nameList;
        }

        /////////////
        // Helpers //
        /////////////

        private static void PopulateNameData()
        {
            foreach (dynamic data in monsterData)
            {
                // Grab the monster's size and append it's name to a size list
                string size = (string)data.size;
                switch (size.ToUpper())
                {
                    case "SMALL":
                        smallMonsterNames.Add((string)data.name);
                        break;
                    case "MEDIUM":
                        mediumMonsterNames.Add((string)data.name);
                        break;
                    case "TALL":
                        tallMonsterNames.Add((string)data.name);
                        break;
                    case "LARGE":
                        largeMonsterNames.Add((string)data.name);
                        break;
                    default:
                        throw new Exception("Invalid monster size: " + data.size + ", found on " + data.name);
                }
                MonsterNames.Add((string)data.name);
            }
        }
    }
}
