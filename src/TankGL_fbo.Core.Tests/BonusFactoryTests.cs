using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Patterns;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class BonusFactoryTests
{
    [Theory]
    [InlineData(BonusType.SpeedUp, 30f)]
    [InlineData(BonusType.Shield, 18f)]
    [InlineData(BonusType.DamageBoost, 17f)]
    [InlineData(BonusType.AmmoRefill, 15f)]
    [InlineData(BonusType.FuelCan, 15f)]
    public void Create_ReturnsBonusWithCorrectTypeAndLifetime(BonusType type, float expectedLifetime)
    {
        var pos = new Vector2(100, 100);
        var bonus = BonusFactory.Create(type, pos);
        Assert.Equal(type, bonus.Type);
        Assert.Equal(expectedLifetime, bonus.Lifetime);
        Assert.Equal(100f, bonus.Position.X);
        Assert.Equal(100f, bonus.Position.Y);
    }
}