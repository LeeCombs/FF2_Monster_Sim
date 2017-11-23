using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF2_Monster_Sim
{
    public class Spell
    {
        public const int MAX_SPELL_LEVEL = 16;

        // Stats
        public string Name;
        public SpellType SpellType;
        public string Effect;
        public int Power;
        public int Accuracy;
        public string Status;
        public Element Element;
        public int Price;
        public int Value;
    }
}
