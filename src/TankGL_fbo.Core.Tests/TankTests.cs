using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Patterns.Decorators;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class TankTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        var stats = new BaseStats();
        var tank = new Tank(new Vector2(100, 100), "tank_red.png", stats);
        Assert.Equal(100f, tank.Position.X);
        Assert.Equal(100f, tank.Position.Y);
        Assert.Equal(100f, tank.HP);

        Assert.False(tank.IsDestroyed);
    }

    [Fact]
    public void TakeDamage_ReducesHP_ByNetDamage()
    {
        var stats = new BaseStats();
        var tank = new Tank(Vector2.Zero, "tank.png", stats);
        tank.TakeDamage(30f);
        Assert.Equal(80f, tank.HP);
    }

    [Fact]
    public void TakeDamage_DoesNotReduceHP_IfDamageLessThanArmor()
    {
        var stats = new BaseStats();
        var tank = new Tank(Vector2.Zero, "tank.png", stats);
        tank.TakeDamage(5f);
        Assert.Equal(100f, tank.HP);
    }

    [Fact]
    public void TakeDamage_ClampsHP_AtZero()
    {
        var stats = new BaseStats();
        var tank = new Tank(Vector2.Zero, "tank.png", stats);
        tank.TakeDamage(200f);
        Assert.Equal(0f, tank.HP);
        Assert.True(tank.IsDestroyed);
    }

    [Fact]
    public void TryFire_Fails_WhenOnCooldown()
    {
        var stats = new BaseStats();
        var tank = new Tank(Vector2.Zero, "tank.png", stats);
        tank.TryFire();
        Assert.False(tank.TryFire());
    }

    [Fact]
    public void Move_UpdatesPosition_AndConsumesFuel()
    {
        var stats = new BaseStats();
        var tank = new Tank(Vector2.Zero, "tank.png", stats);
        tank.Move(new Vector2(1, 0), 1.0f);
        Assert.Equal(110f, tank.Position.X);
        Assert.True(stats.Fuel < 100f);
    }

    [Fact]
    public void Update_RemovesExpiredDecorators()
    {
        var baseStats = new BaseStats();
        var speedDec = new SpeedDecorator(baseStats, 2.0f, 0.5f);
        var tank = new Tank(Vector2.Zero, "tank.png", speedDec);

        Assert.IsType<SpeedDecorator>(tank.Stats);
        tank.Update(0.6f);
        Assert.IsType<BaseStats>(tank.Stats);
    }
}