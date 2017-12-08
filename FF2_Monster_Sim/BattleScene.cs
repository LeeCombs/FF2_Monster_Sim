using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FF2_Monster_Sim
{
    class BattleScene
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

        private int width = 150, height = 100;
        public int X = 0, Y = 0;
        private string type;

        public List<Monster> MonsterList = new List<Monster>();
        Dictionary<int, Monster[]> monsterSlots = new Dictionary<int, Monster[]>();
        Dictionary<int, Vector2[]> slotPositions = new Dictionary<int, Vector2[]>();

        public BattleScene(int x = 0, int y = 0, string type = "A", bool flipped = false)
        {
            X = x;
            Y = y;

            switch(type)
            {
                case "A":
                    break;
                case "B":
                    break;
                case "C":
                    break;
                default:
                    Debug.WriteLine("Scene type must be A, B, or C");
                    return;
            }
            this.type = type;


            for (int i = 0; i < 4; i++)
            {
                Monster[] monsters = { null, null };
                monsterSlots[i] = monsters;

                Vector2[] positions = { new Vector2(100 * i, 0), new Vector2(100 * i, 100) };
                slotPositions[i] = positions;
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (KeyValuePair<int, Monster[]> entry in monsterSlots)
            {
                if (entry.Value[0] != null) entry.Value[0].Draw(spriteBatch);
                if (entry.Value[1] != null) entry.Value[1].Draw(spriteBatch);
            }
        }

        public void PopulateScene(List<Monster> monsters)
        {
            int col = 0, row = 0;
            foreach (Monster monster in monsters)
            {
                MonsterList.Add(monster);
                monsterSlots[col / 2][row % 2] = monster;
                monster.Position = slotPositions[col / 2][row % 2];
                row++;
                col++;
            }
        }

        private bool ColumnIsEmpty(int col)
        {
            // Check column number, return wheter monster slots are empty
            return false;
        }

        private bool MonsterIsBackRow(int col, int row)
        {
            switch (type)
            {
                case "A":
                    // Check col 3 empty
                    // Check col 2 empty
                    break;
                case "B":
                    // check col 3 empty
                    break;
                case "C": 
                    // Only one row, can't be considered a back row
                    return false;
            }
            return false;
        }

        public Monster GetMonsterAtPosition(int col, int row)
        {
            // Retrieve a monster from a given position
            return new Monster();
        }

        public MonsterAction GetMonsterAction(int col, int row)
        {
            // Get the monster's action at a given position
            return new MonsterAction();
        }

        public Monster GetFrontRowTarget()
        {
            // Find the 1-2 front rows
            // Roll rnd to choose a monster within the rows
            // Return it
            return new Monster();
        }

        public Monster GeAnyTarget()
        {
            // Roll rnd to choose a monster within the rows
            // Return it
            return new Monster();
        }

    }
}
