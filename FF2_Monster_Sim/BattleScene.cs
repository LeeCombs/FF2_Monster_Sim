using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{
    public enum SceneType
    {
        A, B, C
    }

    public struct Action
    {
        public Monster Actor;
        public List<Monster> Targets;
        public bool Physical;
        public bool Nothing;
        public Spell Spell;
        public int SpellLevel;

        public Action(Monster actor)
        {
            Actor = actor;
            Targets = new List<Monster>();
            Physical = false;
            Nothing = false;
            Spell = null;
            SpellLevel = 0;
        }
    }

    public class BattleScene
    {
        /**
         * Notes:
         * 
         * Type A: Smalls
         * 
         *   0   1   2   3
         * +---+---+---+---+
         * | s | s | s | s | 0
         * +---+---+---+---+
         * | s | s | s | s | 1
         * +---+---+---+---+
         * 
         * Type B: Mediums/Talls
         * 
         *   0      1     2           0     1     2
         * +-----+-----+-----+     +-----+-----+-----+
         * |  m  |  m  |  m  | 0   |  t  |  t  |  m  | 0
         * +-----+-----+-----+     |     |     +-----+
         * |  m  |  m  |  m  | 1   |     |     |  m  | 1
         * +-----+-----+-----+     +-----+-----+-----+
         * 
         * Type C: Large/Boss
         * 
         *         0
         * +---------------+
         * |               |
         * |       L       |
         * |               |
         * +---------------+
         * 
         * For columns (a, b, c, d)
         * Only front two columns (c,d) can be targetted by and use physical attacks
         * If column d is removed, then (b,c), etc.
         * It's the front-most active row, and the one behind it, even if it second one is empty
         * 
         * Monsters do not make a special effort to be effective from back row
         * If they choose to attack, they simply do "Nothing"
         */

        public SceneType SceneType { get; private set; }
        private int sceneNum;
        private Random rnd;
        private int width = 300, height = 200;
        private string type;
        public int X = 0, Y = 0;
        
        private Dictionary<int, Monster[]> monsterSlots = new Dictionary<int, Monster[]>();
        private Dictionary<int, Vector2[]> slotPositions = new Dictionary<int, Vector2[]>();
        

        public BattleScene(int sceneNum, int x, int y, string type = "A", bool flipped = false)
        {
            rnd = new Random();

            this.sceneNum = sceneNum;
            X = x;
            Y = y;

            // TODO: flipped should swap column positions

            switch(type)
            {
                case "A": // 4 x 2 Slots
                    SceneType = SceneType.A;
                    for (int i = 0; i < 4; i++)
                    {
                        Monster[] monsters = { null, null };
                        monsterSlots[i] = monsters;

                        Vector2[] positions = { new Vector2(X + 75 * i, Y + 0), new Vector2(X + 75 * i, Y + 100) };
                        slotPositions[i] = positions;
                    }
                    break;
                case "B": // 3 x 2 Slots
                    SceneType = SceneType.B;
                    for (int i = 0; i < 3; i++)
                    {
                        Monster[] monsters = { null, null };
                        monsterSlots[i] = monsters;

                        Vector2[] positions = { new Vector2(X + 100 * i, Y + 0), new Vector2(X + 100 * i, Y + 100) };
                        slotPositions[i] = positions;
                    }
                    break;
                case "C": // Only one slot
                    SceneType = SceneType.C;
                    monsterSlots[0] = new Monster[]{ null };
                    slotPositions[0] = new Vector2[]{ new Vector2(X, Y) };
                    break;
                default:
                    Debug.WriteLine("Scene type must be A, B, or C");
                    return;
            }
            if (flipped)
                Utils.ReverseNumberedDictValues(ref slotPositions);
            this.type = type;
        }

        //////////////
        // Monogame //
        //////////////

        public void Initialize()
        {
            //
        }

        public void LoadContent()
        {
            //
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (KeyValuePair<int, Monster[]> entry in monsterSlots)
            {
                foreach (Monster m in entry.Value)
                {
                    if (m == null || m.IsDead())
                        continue;

                    m.Draw(spriteBatch);
                }
            }
        }
        
        /////////////
        // Publics //
        /////////////

        public void PopulateScene(List<Monster> monsters)
        {
            // TODO: This needs to have some checks in place
            // Length check on monsters
            // Size checks for monsters
            // - A must be small. B must be medium/tall. C must be large.
            // Talls must be in row 0, and will ignore the next input~
            // Maybe have unique layouts for combinations of medium/tall?

            int col = 0, row = 0;
            foreach (Monster monster in monsters)
            {
                monsterSlots[col / 2][row % 2] = monster;
                monster.Position = slotPositions[col / 2][row % 2];
                row++;
                col++;
                // Tall monsters skip the next slot
                if (string.Equals(monster.size.ToUpper(), "TALL"))
                {
                    row++;
                    col++;
                }
            }
        }

        public void ClearScene()
        {
            // TODO: Ensure this actually cleans up properly
            foreach (KeyValuePair<int, Monster[]> entry in monsterSlots)
                for (int i = 0; i < entry.Value.Length; i++)
                    entry.Value[i] = null;
        }

        /// <summary>
        /// Remove a single monster from this scene
        /// TODO: Ensure memory cleanup
        /// </summary>
        /// <param name="monster"></param>
        public void RemoveMonster(Monster monster)
        {
            foreach (KeyValuePair<int, Monster[]> entry in monsterSlots)
                for (int i = 0; i < entry.Value.Length; i++)
                    if (entry.Value[i] != null)
                        if (Object.Equals(monster, entry.Value[i]))
                        {
                            entry.Value[i] = null;
                            return;
                        }
        }

        /// <summary>
        /// Generate and return a list of actions from active monsters
        /// </summary>
        /// <param name="sceneRef">The target scene to target monsters from</param>
        public List<Action> GetMonsterActions(BattleScene sceneRef)
        {
            // Get the monster's action at a given position
            // If monster attacks and is in back row, return "nothing"
            List<Action> actList = new List<Action>();

            foreach (Monster mon in GetAllLiveMonsters())
            {
                if (mon == null)
                    continue;

                mon.Init = mon.Evasion += rnd.Next(0, 50);
                Action action = new Action(mon);
                MonsterAction monAct = mon.GetAction();

                if (monAct.Name == "Attack")
                {
                    // If monster is in back row, it will instead return 'nothing'
                    if (MonsterIsBackRow(mon))
                    {
                        action.Nothing = true;
                        action.Targets.Add(mon);
                        actList.Add(action);
                        continue;
                    }

                    action.Physical = true;
                    action.Targets.Add(sceneRef.GetFrontRowTarget());
                    actList.Add(action);
                    continue;
                }
                else
                {
                    // Get Spell
                    // Spell spell = SpellManager.GetSpellByName(monAct.Name);
                    Spell spell = SpellManager.GetSpellByName(monAct.Name);
                    spell.Accuracy = monAct.Accuracy;
                    action.Spell = spell;
                    action.SpellLevel = monAct.Level;

                    switch (monAct.Target)
                    {
                        case "Self":
                            action.Targets.Add(mon);
                            break;
                        case "SingleTarget":
                            action.Targets.Add(sceneRef.GetAnySingleTarget());
                            break;
                        case "EnemyParty":
                            action.Targets = sceneRef.GetAllLiveMonsters().ToList();
                            break;
                        case "CasterParty":
                            action.Targets = this.GetAllLiveMonsters().ToList();
                            break;
                        default:
                            Debug.WriteLine("Invalid monAct target: " + monAct.Target);
                            continue;
                    }
                    actList.Add(action);
                }
            }

            return actList;
        }
        
        /// <summary>
        /// Retrieve an array of all live monsters
        /// </summary>
        public Monster[] GetAllLiveMonsters()
        {
            List<Monster> liveMonsters = new List<Monster>();
            foreach (Monster mon in GetAllMonsters())
                if (mon.IsAlive())
                    liveMonsters.Add(mon);
            return liveMonsters.ToArray();
        }

        /// <summary>
        /// Return an array of monsters within the scene, including the dead
        /// </summary>
        /// <returns></returns>
        public Monster[] GetAllMonsters()
        {
            List<Monster> activeList = new List<Monster>();
            foreach (KeyValuePair<int, Monster[]> entry in monsterSlots)
                foreach (Monster m in entry.Value)
                    if (m != null)
                        activeList.Add(m);
            return activeList.ToArray();
        }

        /// <summary>
        /// Return whether the scene has living creatures
        /// </summary>
        public bool HasLivingMonsters()
        {
            return GetAllLiveMonsters().Length > 0;
        }

        /// <summary>
        /// Retrieve a single random Monster from any slot
        /// </summary>
        public Monster GetAnySingleTarget()
        {
            if (monsterSlots.Count == 0)
                throw new Exception("Monster Slots must be populated before they can return anything");

            // Build a list of current monsters, choose one randomly and return it
            Monster[] activeList = GetAllLiveMonsters();
            int slotRoll = rnd.Next(0, activeList.Length);
            return activeList[slotRoll];
        }

        /// <summary>
        /// Retrieve a single random monster from the front rows
        /// </summary>
        public Monster GetFrontRowTarget()
        {
            if (monsterSlots.Count == 0)
                throw new Exception("Monster Slots must be populated before they can return anything");

            // Build a list of viable targets based on which two rows are front rows
            List<Monster> monsterList = new List<Monster>();

            switch (SceneType)
            {
                case SceneType.A:
                    // 4 columns, check front two for emptiness
                    if (!ColumnIsEmpty(3))
                    {
                        monsterList.AddRange(GetLiveMonstersFromCol(3));
                        monsterList.AddRange(GetLiveMonstersFromCol(2));
                    }
                    if (!ColumnIsEmpty(2))
                    {
                        monsterList.AddRange(GetLiveMonstersFromCol(2));
                        monsterList.AddRange(GetLiveMonstersFromCol(1));
                    }
                    else
                    {
                        monsterList.AddRange(GetLiveMonstersFromCol(1));
                        monsterList.AddRange(GetLiveMonstersFromCol(0));
                    }
                    break;
                case SceneType.B:
                    // 3 columns, only check front-most for emptiness
                    if (!ColumnIsEmpty(2))
                    {
                        monsterList.AddRange(GetLiveMonstersFromCol(2));
                        monsterList.AddRange(GetLiveMonstersFromCol(1));
                    }
                    else
                    {
                        monsterList.AddRange(GetLiveMonstersFromCol(1));
                        monsterList.AddRange(GetLiveMonstersFromCol(0));
                    }
                    break;
                case SceneType.C:
                    // Only one column
                    monsterList.AddRange(GetLiveMonstersFromCol(0));
                    break;
            }

            // Roll a random monster from the list and return it
            int slotRoll = rnd.Next(0, monsterList.Count);
            return monsterList[slotRoll];
        }

    
        /// <summary>
        /// Iterate through all monsters and build the display text based on their current stats
        /// TODO: Try to only call this when monsters in this scene take/heal damage
        /// </summary>
        public void UpdateSceneText()
        {
            string displayText = "";
            foreach (Monster monster in GetAllMonsters())
            {
                // Set line color based on Monster's current HP. Red = dead, Yellow = half or less.
                if (monster.IsDead())
                    displayText += "{{Red}}";
                else if (monster.IsCritical())
                    displayText += "{{White}}";
                else
                    displayText += "{{Yellow}}";

                displayText += monster.Name.PadRight(9) + " - ";
                displayText += monster.HP.ToString().PadLeft(6) + " - ";
                displayText += monster.MP.ToString().PadLeft(6) + "\n";
            }
            TextManager.SetSceneText(sceneNum, displayText);
        }
        
        /////////////
        // Helpers //
        /////////////

        private Monster[] GetLiveMonstersFromCol(int col)
        {
            List<Monster> monList = new List<Monster>();
            foreach (Monster m in monsterSlots[col])
                if (m != null)
                    if (m.IsAlive())
                        monList.Add(m);
            return monList.ToArray();
        }

        private bool MonsterIsBackRow(Monster monster)
        {
            int col = GetMonsterColumn(monster);

            // Col 2 and 3 cannot be back rows
            if (col == 2 || col == 3)
                return false;
            
            // Check columns 2+ ahead of monster for emptiness
            switch (SceneType)
            {
                case SceneType.A:
                    if (col == 1)
                        return !ColumnIsEmpty(3);
                    return !ColumnIsEmpty(3) || !ColumnIsEmpty(2);
                case SceneType.B:
                    // Three rows max, only check column 1
                    return ColumnIsEmpty(1);
                case SceneType.C:
                    // Since there's only one slot, it cannot be a back row
                    return false;
            }

            throw new Exception("Uncaught sceneType: " + SceneType);
        }

        /// <summary>
        /// Return the column number a given monster is in
        /// </summary>
        private int GetMonsterColumn(Monster monster)
        {
            foreach (KeyValuePair<int, Monster[]> entry in monsterSlots)
                foreach (Monster m in entry.Value)
                    if (Object.Equals(m, monster))
                        return entry.Key;
            return -1;
        }
        
        /// <summary>
        /// Check whether or not a column contains active monsters
        /// </summary>
        private bool ColumnIsEmpty(int col)
        {
            // If col is out of bounds, it's empty
            if (col < 0 || col >= monsterSlots.Count)
                return true;

            foreach (Monster m in monsterSlots[col])
                if (m.IsAlive())
                    return false;

            return true;
        }
    }
}
