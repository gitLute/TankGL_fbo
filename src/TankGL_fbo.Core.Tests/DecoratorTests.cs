using TankGL_fbo.Core.Patterns.Decorators;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class DecoratorTests
{
    [Fact]
    public void SpeedDecorator_MultipliesSpeed()
    {
        var baseStats = new BaseStats(); // Speed = 150
        var decorated = new SpeedDecorator(baseStats, 1.5f, 10f);
        Assert.Equal(225f, decorated.Speed);
        Assert.Equal(150f, baseStats.Speed);
    }

    [Fact]
    public void ArmorDecorator_AddsArmor()
    {
        var baseStats = new BaseStats(); // Armor = 10
        var decorated = new ArmorDecorator(baseStats, 20f, 8f);
        Assert.Equal(30f, decorated.Armor);
    }

    [Fact]
    public void DamageDecorator_MultipliesDamage()
    {
        var baseStats = new BaseStats(); // Damage = 25
        var decorated = new DamageDecorator(baseStats, 1.8f, 7f);
        Assert.Equal(45f, decorated.Damage);
    }

    [Fact]
    public void StatDecorator_ExpiresAfterDuration()
    {
        var dec = new SpeedDecorator(new BaseStats(), 2.0f, 1.0f);
        Assert.False(dec.IsExpired);
        dec.Update(0.5f);
        Assert.False(dec.IsExpired);
        dec.Update(0.6f);
        Assert.True(dec.IsExpired);
    }

    [Fact]
    public void FindInChain_FindsNestedDecorator()
    {
        var baseStats = new BaseStats();
        var speedDec = new SpeedDecorator(baseStats, 1.5f, 10f);
        var armorDec = new ArmorDecorator(speedDec, 20f, 8f);

        var found = StatDecorator.FindInChain<SpeedDecorator>(armorDec);
        Assert.NotNull(found);
        Assert.Same(speedDec, found);
    }
}