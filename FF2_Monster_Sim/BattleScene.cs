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
         *   A   B   C   D
         * +---+---+---+---+
         * | 1 | 3 | 5 | 7 |
         * +---+---+---+---+
         * | 2 | 4 | 6 | 8 |
         * +---+---+---+---+
         * 
         * 1-8 Monsters. A 2x4 grid.
         * 3 sizes? (1x1, 1x2, 2x4)
         * 
         * For columns (a, b, c, d)
         * Only front two columns (c,d) can be targetted by and use physical attacks
         * If column d is removed, then (b,c), etc.
         * It's the front-most active row, and the one behind it, even if it second one is empty
         * 
         * Monsters do not make a special effort to be effective from back row
         * If they choose to attack, they simply do "Nothing"
         */

        Dictionary<int, Monster[]> monsterSlots = new Dictionary<int, Monster[]>();
        Dictionary<int, Vector2[]> slotPositions = new Dictionary<int, Vector2[]>();


        public BattleScene()
        {
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
                monsterSlots[col / 2][row % 2] = monster;
                monster.Position = slotPositions[col / 2][row % 2];
                row++;
                col++;
            }
        }
    }
}
