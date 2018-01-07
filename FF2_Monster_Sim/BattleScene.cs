using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{
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

        private Random rnd;
        private int width = 300, height = 200;
        private string type;
        public int X = 0, Y = 0;
        
        Dictionary<int, Monster[]> monsterSlots = new Dictionary<int, Monster[]>();
        Dictionary<int, Vector2[]> slotPositions = new Dictionary<int, Vector2[]>();

        public BattleScene(int x = 0, int y = 0, string type = "A", bool flipped = false)
        {
            rnd = new Random();

            X = x;
            Y = y;

            // TODO: flipped should swap column positions

            switch(type)
            {
                case "A": // 4 x 2 Slots
                    for (int i = 0; i < 4; i++)
                    {
                        Monster[] monsters = { null, null };
                        monsterSlots[i] = monsters;

                        Vector2[] positions = { new Vector2(X + 75 * i, Y + 0), new Vector2(X + 75 * i, Y + 100) };
                        slotPositions[i] = positions;
                    }
                    break;
                case "B": // 3 x 2 Slots
                    for (int i = 0; i < 3; i++)
                    {
                        Monster[] monsters = { null, null };
                        monsterSlots[i] = monsters;

                        Vector2[] positions = { new Vector2(X + 100 * i, Y + 0), new Vector2(X + 100 * i, Y + 100) };
                        slotPositions[i] = positions;
                    }
                    break;
                case "C": // Only one slot
                    monsterSlots[0] = new Monster[]{ null };
                    slotPositions[0] = new Vector2[]{ new Vector2(X, Y) };
                    break;
                default:
                    Debug.WriteLine("Scene type must be A, B, or C");
                    return;
            }
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
                    if (m == null)
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
            }
        }

        public void ClearScene()
        {
            // TODO: Ensure this actually cleans up properly
            foreach (KeyValuePair<int, Monster[]> entry in monsterSlots)
                for (int i = 0; i < entry.Value.Length; i++)
                    entry.Value[i] = null;
        }

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

            foreach (Monster mon in GetAllTargets())
            {
                if (mon != null)
                {
                    mon.Init = mon.Evasion += rnd.Next(0, 50);
                    Action action = new Action(mon);
                    MonsterAction monAct = mon.GetAction();

                    if (monAct.Name == "Attack")
                    {
                        // TODO: If monster is in back row, it will instead return 'nothing'
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
                                action.Targets = sceneRef.GetAllTargets().ToList();
                                break;
                            case "CasterParty":
                                action.Targets = this.GetAllTargets().ToList();
                                break;
                            default:
                                Debug.WriteLine("Invalid monAct target: " + monAct.Target);
                                continue;
                        }
                        actList.Add(action);
                    }
                }
            }

            return actList;
        }
        
        /// <summary>
        /// Retrieve an array of all active monsters
        /// </summary>
        public Monster[] GetAllTargets()
        {
            List<Monster> activeList = new List<Monster>();
            foreach (KeyValuePair<int, Monster[]> entry in monsterSlots)
                foreach (Monster m in entry.Value)
                    if (m != null)
                        activeList.Add(m);
            return activeList.ToArray();
        }

        public int GetLiveCount()
        {
            // ?
            return GetAllTargets().Length;
        }

        /// <summary>
        /// Retrieve a single random Monster from any slot
        /// </summary>
        public Monster GetAnySingleTarget()
        {
            if (monsterSlots.Count == 0)
            {
                Debug.WriteLine("Monster Slots must be populated before they can return anything");
                return null;
            }

            // Build a list of current monsters, choose one randomly and return it
            List<Monster> activeList = new List<Monster>();
            foreach (KeyValuePair<int, Monster[]> entry in monsterSlots)
                foreach (Monster m in entry.Value)
                    if (m != null)
                        activeList.Add(m);

            if (activeList.Count == 0)
                return null;

            int slotRoll = rnd.Next(0, activeList.Count);
            return activeList[slotRoll];
        }

        /// <summary>
        /// Retrieve a single random monster from the front rows
        /// </summary>
        public Monster GetFrontRowTarget()
        {
            if (monsterSlots.Count == 0)
            {
                Debug.WriteLine("Monster Slots must be populated before they can return anything");
                return null;
            }

            List<Monster> monsterList = new List<Monster>();

            if (!ColumnIsEmpty(3))
            {
                // Col 3 and 2 are front
                foreach (Monster m in monsterSlots[3])
                    if (m != null)
                        monsterList.Add(m);

                foreach (Monster m in monsterSlots[2])
                    if (m != null)
                        monsterList.Add(m);
            }
            else if (!ColumnIsEmpty(2))
            {
                // Col 2 and 1 are front
                foreach (Monster m in monsterSlots[2])
                    if (m != null)
                        monsterList.Add(m);

                foreach (Monster m in monsterSlots[1])
                    if (m != null)
                        monsterList.Add(m);
            }
            else
            {
                // Col 1 and 0 are front
                foreach (Monster m in monsterSlots[1])
                    if (m != null)
                        monsterList.Add(m);

                foreach (Monster m in monsterSlots[0])
                    if (m != null)
                        monsterList.Add(m);
            }

            if (monsterList.Count == 0)
                return null;

            int slotRoll = rnd.Next(0, monsterList.Count);
            return monsterList[slotRoll];
        }
        
        /////////////
        // Helpers //
        /////////////

        private bool MonsterIsBackRow(Monster monster)
        {
            // Columns 3 and 4 are always front row
            // If another column is found, check 2-3 columns ahead of it for emptiness
            Debug.WriteLine("Monsterisbackrow");
            int col = GetMonsterColumn(monster);
            Debug.WriteLine("col: " + col);

            if (col == 3 || col == 2)
                return false;
            if (col == 1 && ColumnIsEmpty(3))
                return false;
            if (col == 1 && ColumnIsEmpty(3) && ColumnIsEmpty(2))
                return false;
            return true;
        }

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
                if (m != null)
                    return false;

            return true;
        }
    }
}
