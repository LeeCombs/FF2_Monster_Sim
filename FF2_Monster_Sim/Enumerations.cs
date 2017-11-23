// Enum definitions going here for now...

namespace FF2_Monster_Sim
{
    public enum PermStatus
    {
        Amnesia,
        Curse,
        Darkness,
        KO,
        Poison,
        Stone,
        Toad
    }

    public enum TempStatus
    {
        Confuse,
        Mini,
        Mute,
        Paralysis,
        Sleep,
        Venom
    }

    public enum Debuff
    {
        Slow,
        Fear
    }

    public enum Buff
    {
        Aura,
        Barrier,
        Berserk,
        Blink,
        Haste,
        Protect,
        Shell,
        Wall,
        Imbibe,
        Intelligence,
        Spirit
    }

    public enum Element
    {
        // Found naming has been inconsistent so far
        // e.g. Dimension = Time = Matter, Mind = Mental, Body = Physical
        Dimension,
        Mind,
        Body,
        Death,
        Fire,
        Lightning,
        Poison,
        Ice
    }
}
