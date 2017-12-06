using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FF2_Monster_Sim
{
    class BattleScene
    {
        /**
         * Notes:
         * 
         *   A   B   C   D
         * +---+---+---+---+
         * | 1 | 2 | 3 | 4 |
         * +---+---+---+---+
         * | 5 | 6 | 7 | 8 |
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

        public BattleScene()
        {
            for (int i = 0; i < 4; i++)
            {
                Monster[] monsters = { null, null };
                monsterSlots[i] = monsters;
            }
        }
    }
}
