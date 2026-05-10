using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Patterns.Decorators;
using TankGL_fbo.Core.Systems;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class InputSystemTests
{
    [Fact]
    public void Process_RotatesTankLeft()
    {
        var tanks = new List<Tank> { new Tank(Vector2.Zero, "tank.png", new BaseStats()) };
        var bullets = new List<Bullet>();
        var input = new InputSystem(tanks, bullets);
        var actions = new Dictionary<int, HashSet<PlayerAction>>
        {
            [0] = new HashSet<PlayerAction> { PlayerAction.RotateLeft }
        };
        float initialRotation = tanks[0].Rotation;
        input.Process(actions, 1.0f);
        Assert.True(tanks[0].Rotation > initialRotation);
    }

    [Fact]
    public void Process_MovesTankForward()
    {
        var tanks = new List<Tank> { new Tank(Vector2.Zero, "tank.png", new BaseStats()) };
        var bullets = new List<Bullet>();
        var input = new InputSystem(tanks, bullets);
        var actions = new Dictionary<int, HashSet<PlayerAction>>
        {
            [0] = new HashSet<PlayerAction> { PlayerAction.MoveUp }
        };
        input.Process(actions, 1.0f);
        Assert.NotEqual(Vector2.Zero, tanks[0].Position);
        Assert.True(tanks[0].Position.Length() > 0);
    }

    [Fact]
    public void Process_FiresBullet_WhenFireActionPressed()
    {
        var tanks = new List<Tank> { new Tank(Vector2.Zero, "tank.png", new BaseStats()) };
        var bullets = new List<Bullet>();
        tanks[0].Update(0.6f); // Сбрасываем начальный кулдаун 0.5f

        var input = new InputSystem(tanks, bullets);
        var actions = new Dictionary<int, HashSet<PlayerAction>>
        {
            [0] = new HashSet<PlayerAction> { PlayerAction.Fire }
        };
        input.Process(actions, 0.1f);
        Assert.Single(bullets);
        Assert.Equal(0, bullets[0].OwnerId);
    }

    [Fact]
    public void Process_DoesNotFire_WhenOnCooldown()
    {
        var tanks = new List<Tank> { new Tank(Vector2.Zero, "tank.png", new BaseStats()) };
        var bullets = new List<Bullet>();
        tanks[0].Update(0.6f); // Сбрасываем начальный кулдаун

        var input = new InputSystem(tanks, bullets);
        var actions = new Dictionary<int, HashSet<PlayerAction>>
        {
            [0] = new HashSet<PlayerAction> { PlayerAction.Fire }
        };
        input.Process(actions, 0.1f); // Первый выстрел
        Assert.Single(bullets);

        input.Process(actions, 0.1f); // Попытка второго выстрела сразу
        Assert.Single(bullets); // Кулдаун 0.5f не должен дать выстрелить снова
    }

    [Fact]
    public void Process_IgnoresDestroyedTank()
    {
        var tank = new Tank(Vector2.Zero, "tank.png", new BaseStats());
        tank.TakeDamage(200f);
        var tanks = new List<Tank> { tank };
        var bullets = new List<Bullet>();
        var input = new InputSystem(tanks, bullets);
        var actions = new Dictionary<int, HashSet<PlayerAction>>
        {
            [0] = new HashSet<PlayerAction> { PlayerAction.MoveUp, PlayerAction.Fire }
        };
        input.Process(actions, 1.0f);
        Assert.Equal(Vector2.Zero, tank.Position);
        Assert.Empty(bullets);
    }
}