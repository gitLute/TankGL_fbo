using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Patterns;
using TankGL_fbo.Core.Patterns.Decorators;
using TankGL_fbo.Core.Systems;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class CollisionSystemTests
{
    [Fact]
    public void Resolve_PreventsTankFromMovingThroughWall()
    {
        var tank = new Tank(new Vector2(-40, 0), "tank.png", new BaseStats());
        tank.Update(0.01f);

        tank.Move(new Vector2(1, 0), 0.05f);

        var wall = new Wall(new Vector2(0, 0), new Vector2(10, 50));
        var system = new CollisionSystem(new List<Tank> { tank }, new List<Wall> { wall }, new List<Bonus>(), new List<Bullet>());

        system.Resolve();

        Assert.Equal(-36.25f, tank.Position.X, 0.01f);
    }

    [Fact]
    public void Resolve_BulletDamagesEnemyTank()
    {
        var tank0 = new Tank(new Vector2(-50, 0), "tank.png", new BaseStats());
        var tank1 = new Tank(new Vector2(50, 0), "tank.png", new BaseStats());
        // Пуля от игрока 0 летит вправо и попадает в танк 1
        var bullet = new Bullet(new Vector2(40, 0), 0f, 100f, 30f, 2f, 0);

        var system = new CollisionSystem(new List<Tank> { tank0, tank1 }, new List<Wall>(), new List<Bonus>(), new List<Bullet> { bullet });
        system.Resolve();

        Assert.True(tank1.HP < 100f);
        Assert.True(bullet.IsExpired);
    }

    [Fact]
    public void Resolve_BulletDoesNotDamageOwner()
    {
        var tank = new Tank(new Vector2(50, 0), "tank.png", new BaseStats());
        var bullet = new Bullet(new Vector2(40, 0), 0f, 100f, 30f, 2f, 0); // OwnerId == 0 (индекс танка)
        var system = new CollisionSystem(new List<Tank> { tank }, new List<Wall>(), new List<Bonus>(), new List<Bullet> { bullet });
        system.Resolve();

        Assert.Equal(100f, tank.HP);
        Assert.False(bullet.IsExpired); // Пуля не должна исчезнуть от столкновения с владельцем
    }

    [Fact]
    public void Resolve_TankPicksUpBonus()
    {
        var tank = new Tank(new Vector2(0, 0), "tank.png", new BaseStats());
        var bonus = new Bonus(new Vector2(5, 0), BonusType.AmmoRefill, 10f);
        var system = new CollisionSystem(new List<Tank> { tank }, new List<Wall>(), new List<Bonus> { bonus }, new List<Bullet>());

        system.Resolve();

        Assert.True(bonus.IsExpired); // Lifetime принудительно ставится в 0
        Assert.Equal(40, tank.Stats.Ammo); // Base 35 + 15 = 50, но кап на 40
    }

    [Fact]
    public void Resolve_BulletHitsWall_Expires()
    {
        var wall = new Wall(new Vector2(100, 0), new Vector2(10, 50));
        var bullet = new Bullet(new Vector2(90, 0), 0f, 100f, 20f, 5f, 0);
        var system = new CollisionSystem(new List<Tank>(), new List<Wall> { wall }, new List<Bonus>(), new List<Bullet> { bullet });

        system.Resolve();

        Assert.True(bullet.IsExpired);
    }
}