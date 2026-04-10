using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Patterns;
using TankGL_fbo.Core.Patterns.Decorators;

namespace TankGL_fbo.Core.Systems;

public sealed class CollisionSystem
{
    private readonly List<Tank> _tanks;
    private readonly List<Wall> _walls;
    private readonly List<Bonus> _bonuses;
    private readonly List<Bullet> _bullets;

    public CollisionSystem(List<Tank> tanks, List<Wall> walls, List<Bonus> bonuses, List<Bullet> bullets)
    {
        _tanks = tanks;
        _walls = walls;
        _bonuses = bonuses;
        _bullets = bullets;
    }

    public void Resolve()
    {
        foreach (var tank in _tanks)
        {
            if (tank.IsDestroyed) continue;
            foreach (var wall in _walls)
            {
                if (tank.Bounds.Intersects(wall.Bounds)) tank.Position = tank.PreviousPosition;
            }
        }

        for (int i = _bonuses.Count - 1; i >= 0; i--)
        {
            var bonus = _bonuses[i];
            foreach (var tank in _tanks)
            {
                if (tank.IsDestroyed) continue;
                if (bonus.Bounds.Intersects(tank.Bounds))
                {
                    ApplyBonus(tank, bonus.Type);
                    bonus.Lifetime = 0;
                    break;
                }
            }
        }

        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            var bullet = _bullets[i];
            if (bullet.IsExpired) continue;

            foreach (var wall in _walls)
            {
                if (bullet.Bounds.Intersects(wall.Bounds))
                {
                    bullet.Lifetime = 0;
                    break;
                }
            }
            if (bullet.IsExpired) continue;

            for (int j = 0; j < _tanks.Count; j++)
            {
                var tank = _tanks[j];
                if (tank.IsDestroyed) continue;

                if (j == bullet.OwnerId) continue;

                if (bullet.Bounds.Intersects(tank.Bounds))
                {
                    tank.TakeDamage(bullet.Damage);
                    bullet.Lifetime = 0;
                    break;
                }
            }
        }
    }

    public void ApplyBonus(Tank tank, BonusType type)
    {
        switch (type)
        {
            case BonusType.SpeedUp:
                tank.Stats = new SpeedDecorator(tank.Stats, 1.5f, 10f);
                break;
            case BonusType.Shield:
                tank.Stats = new ArmorDecorator(tank.Stats, 20f, 8f);
                break;
            case BonusType.DamageBoost:
                tank.Stats = new DamageDecorator(tank.Stats, 1.8f, 7f);
                break;
            case BonusType.AmmoRefill:
                tank.Stats.Ammo = Math.Min(tank.Stats.Ammo + 15, 40);
                break;
            case BonusType.FuelCan:
                tank.Stats.Fuel = Math.Min(tank.Stats.Fuel + 40f, 100f);
                break;
        }
    }
}