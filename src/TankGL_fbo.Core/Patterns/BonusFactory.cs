namespace TankGL_fbo.Core.Patterns;

using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;

public abstract class BonusCreator
{
    public abstract Bonus Create(Vector2 position);
}

public class SpeedUpCreator : BonusCreator
{
    public override Bonus Create(Vector2 pos) => new Bonus(pos, BonusType.SpeedUp, 30f);
}

public class ShieldCreator : BonusCreator
{
    public override Bonus Create(Vector2 pos) => new Bonus(pos, BonusType.Shield, 18f);
}

public class DamageBoostCreator : BonusCreator
{
    public override Bonus Create(Vector2 pos) => new Bonus(pos, BonusType.DamageBoost, 17f);
}

public class AmmoRefillCreator : BonusCreator
{
    public override Bonus Create(Vector2 pos) => new Bonus(pos, BonusType.AmmoRefill, 15f);
}

public class FuelCanCreator : BonusCreator
{
    public override Bonus Create(Vector2 pos) => new Bonus(pos, BonusType.FuelCan, 15f);
}

public class SpeedDownCreator : BonusCreator
{
    public override Bonus Create(Vector2 pos) => new Bonus(pos, BonusType.SpeedDown, 25f);
}

public class ArmorBreakCreator : BonusCreator
{
    public override Bonus Create(Vector2 pos) => new Bonus(pos, BonusType.ArmorBreak, 20f);
}

public class DamageDownCreator : BonusCreator
{
    public override Bonus Create(Vector2 pos) => new Bonus(pos, BonusType.DamageDown, 18f);
}

public static class BonusFactory
{
    private static readonly Dictionary<BonusType, BonusCreator> _creators = new()
    {
        { BonusType.SpeedUp,     new SpeedUpCreator() },
        { BonusType.Shield,      new ShieldCreator() },
        { BonusType.DamageBoost, new DamageBoostCreator() },
        { BonusType.AmmoRefill,  new AmmoRefillCreator() },
        { BonusType.FuelCan,     new FuelCanCreator() },
        { BonusType.SpeedDown,   new SpeedDownCreator() },
        { BonusType.ArmorBreak,  new ArmorBreakCreator() },
        { BonusType.DamageDown,  new DamageDownCreator() }
    };

    public static Bonus Create(BonusType type, Vector2 position)
    {
        if (_creators.TryGetValue(type, out var creator))
            return creator.Create(position);

        throw new ArgumentOutOfRangeException(nameof(type), $"unknown type: {type}");
    }
}