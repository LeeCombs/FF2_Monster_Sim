using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF2_Monster_Sim
{
    public static class MonoMonsterManager
    {
        public static MonoMonster GetMonoMonsterByName(string name)
        {
            return (MonoMonster)MonsterManager.GetMonsterByName(name);
        }
    }
}
