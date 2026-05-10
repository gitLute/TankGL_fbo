using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Patterns;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class BonusTests
{
    [Fact]
    public void Update_ReducesLifetime()
    {
        var bonus = new Bonus(new Vector2(0, 0), BonusType.SpeedUp, 10f);
        bonus.Update(3f);
        Assert.Equal(7f, bonus.Lifetime);
    }

    [Fact]
    public void IsExpired_ReturnsTrue_WhenLifetimeDepleted()
    {
        var bonus = new Bonus(new Vector2(0, 0), BonusType.Shield, 5f);
        bonus.Update(5.1f);
        Assert.True(bonus.IsExpired);
    }

    [Theory]
    [InlineData(BonusType.SpeedUp, "bonus_speed.png")]
    [InlineData(BonusType.Shield, "bonus_shield.png")]
    [InlineData(BonusType.DamageBoost, "bonus_damage.png")]
    [InlineData(BonusType.AmmoRefill, "bonus_ammo.png")]
    [InlineData(BonusType.FuelCan, "bonus_fuel.png")]
    [InlineData(BonusType.SpeedDown, "penalty_speed.png")]
    [InlineData(BonusType.ArmorBreak, "penalty_armor.png")]
    [InlineData(BonusType.DamageDown, "penalty_damage.png")]
    public void TexturePath_ReturnsCorrectTexture(BonusType type, string expectedTexture)
    {
        var bonus = new Bonus(Vector2.Zero, type, 10f);
        Assert.Equal(expectedTexture, bonus.TexturePath);
    }
}