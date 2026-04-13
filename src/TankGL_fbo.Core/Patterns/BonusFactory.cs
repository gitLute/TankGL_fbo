namespace TankGL_fbo.Core.Patterns;

using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;

public static class BonusFactory
{
    public static Entities.Bonus Create(BonusType type, Vector2 position) => type switch
    {
        BonusType.SpeedUp     => new Entities.Bonus(position, BonusType.SpeedUp, 30f),
        BonusType.Shield      => new Entities.Bonus(position, BonusType.Shield, 18f),
        BonusType.DamageBoost => new Entities.Bonus(position, BonusType.DamageBoost, 17f),
        BonusType.AmmoRefill  => new Entities.Bonus(position, BonusType.AmmoRefill, 15f),
        BonusType.FuelCan     => new Entities.Bonus(position, BonusType.FuelCan, 15f),
        _ => throw new System.ArgumentOutOfRangeException(nameof(type), type, null)
    };
}