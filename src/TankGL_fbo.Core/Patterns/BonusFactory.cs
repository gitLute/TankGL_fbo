namespace TankGL_fbo.Core.Patterns;

using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;

public static class BonusFactory
{
    public static Entities.Bonus Create(BonusType type, Vector2 position) => type switch
    {
        BonusType.SpeedUp     => new Entities.Bonus(position, BonusType.SpeedUp, 10f),
        BonusType.Shield      => new Entities.Bonus(position, BonusType.Shield, 8f),
        BonusType.DamageBoost => new Entities.Bonus(position, BonusType.DamageBoost, 7f),
        BonusType.AmmoRefill  => new Entities.Bonus(position, BonusType.AmmoRefill, 5f),
        BonusType.FuelCan     => new Entities.Bonus(position, BonusType.FuelCan, 5f),
        _ => throw new System.ArgumentOutOfRangeException(nameof(type), type, null)
    };
}