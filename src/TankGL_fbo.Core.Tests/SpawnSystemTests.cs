using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Patterns;
using TankGL_fbo.Core.Patterns.Decorators;
using TankGL_fbo.Core.Systems;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class SpawnSystemTests
{
    [Fact]
    public void Update_DoesNotSpawnImmediately()
    {
        var bonuses = new List<Bonus>();
        var system = new SpawnSystem(bonuses, new List<Wall>(), new List<Tank>());
        system.Update(0.1f);
        Assert.Empty(bonuses); // Минимальный интервал 4 сек
    }

    [Fact]
    public void Update_RemovesExpiredBonuses()
    {
        var bonus = new Bonus(new Vector2(0, 0), BonusType.SpeedUp, 0.5f);
        var bonuses = new List<Bonus> { bonus };
        var system = new SpawnSystem(bonuses, new List<Wall>(), new List<Tank>());

        system.Update(1.0f);

        Assert.Empty(bonuses);
    }

    [Fact]
    public void Update_SpawnsBonus_AfterMaxInterval()
    {
        var bonuses = new List<Bonus>();
        var tanks = new List<Tank> { new Tank(new Vector2(1000, 1000), "tank.png", new BaseStats()) }; // Далеко от зоны спавна
        var system = new SpawnSystem(bonuses, new List<Wall>(), tanks);

        system.Update(11f); // Макс. интервал = 10 сек

        Assert.NotEmpty(bonuses);
        var spawned = bonuses[0];
        Assert.InRange(spawned.Position.X, -380f, 380f);
        Assert.InRange(spawned.Position.Y, -280f, 280f);
    }
}