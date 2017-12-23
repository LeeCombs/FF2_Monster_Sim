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

        private int power;
        public int Power
        {
            get { return power; }
            set { power = Utils.EnforceNumCap(value, 255); }
        }

        private int accuracy;
        public int Accuracy
        {
            get { return accuracy; }
            set
            {
                // Accuracy for spells is explicitly not capped
                accuracy = value < 0 ? 0 : value;
            }
        }
        
        public string Status;
        public Element Element;
        public int Price;
        public int Value;
        public string SuccessMessage;
    }
}
