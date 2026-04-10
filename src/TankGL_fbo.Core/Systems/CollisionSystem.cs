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

            Vector2 currentPos = tank.Position;
            Vector2 prevPos = tank.PreviousPosition;
            Vector2 delta = currentPos - prevPos;

            Vector2 posAfterX = new Vector2(prevPos.X + delta.X, prevPos.Y);
            var boundsAfterX = new RectAABB(posAfterX, tank.Bounds.HalfSize);
            bool hitX = false;
            foreach (var wall in _walls)
            {
                if (boundsAfterX.Intersects(wall.Bounds)) { hitX = true; break; }
            }
            Vector2 resolvedPos = hitX ? new Vector2(prevPos.X, prevPos.Y) : posAfterX;

            Vector2 posAfterY = new Vector2(resolvedPos.X, prevPos.Y + delta.Y);
            var boundsAfterY = new RectAABB(posAfterY, tank.Bounds.HalfSize);
            bool hitY = false;
            foreach (var wall in _walls)
            {
                if (boundsAfterY.Intersects(wall.Bounds)) { hitY = true; break; }
            }
            resolvedPos = hitY ? new Vector2(resolvedPos.X, prevPos.Y) : posAfterY;

            tank.Position = resolvedPos;
        }

        for (int i = 0; i < _tanks.Count; i++)
        {
            var tankA = _tanks[i];
            if (tankA.IsDestroyed) continue;

            for (int j = i + 1; j < _tanks.Count; j++)
            {
                var tankB = _tanks[j];
                if (tankB.IsDestroyed) continue;

                if (tankA.Bounds.Intersects(tankB.Bounds))
                {
                    float dx = tankB.Position.X - tankA.Position.X;
                    float dy = tankB.Position.Y - tankA.Position.Y;

                    float overlapX = (tankA.Bounds.HalfSize.X + tankB.Bounds.HalfSize.X) - MathF.Abs(dx);
                    float overlapY = (tankA.Bounds.HalfSize.Y + tankB.Bounds.HalfSize.Y) - MathF.Abs(dy);

                    if (overlapX < overlapY)
                    {
                        float shift = overlapX / 2f;
                        if (dx > 0)
                        {
                            tankA.Position = new Vector2(tankA.Position.X - shift, tankA.Position.Y);
                            tankB.Position = new Vector2(tankB.Position.X + shift, tankB.Position.Y);
                        }
                        else
                        {
                            tankA.Position = new Vector2(tankA.Position.X + shift, tankA.Position.Y);
                            tankB.Position = new Vector2(tankB.Position.X - shift, tankB.Position.Y);
                        }
                    }
                    else
                    {
                        float shift = overlapY / 2f;
                        if (dy > 0)
                        {
                            tankA.Position = new Vector2(tankA.Position.X, tankA.Position.Y - shift);
                            tankB.Position = new Vector2(tankB.Position.X, tankB.Position.Y + shift);
                        }
                        else
                        {
                            tankA.Position = new Vector2(tankA.Position.X, tankA.Position.Y + shift);
                            tankB.Position = new Vector2(tankB.Position.X, tankB.Position.Y - shift);
                        }
                    }
                }
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