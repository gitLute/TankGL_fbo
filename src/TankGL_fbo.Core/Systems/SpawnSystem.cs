namespace TankGL_fbo.Core.Systems;

using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Patterns;

public sealed class SpawnSystem
{
    private readonly List<Bonus> _bonuses;
    private readonly List<Wall> _walls;
    private readonly List<Tank> _tanks;
    private readonly Random _rng = new();
    private float _spawnTimer = 0f;

    private const float MinX = -380f, MaxX = 380f;
    private const float MinY = -280f, MaxY = 280f;

    public SpawnSystem(List<Bonus> bonuses, List<Wall> walls, List<Tank> tanks)
    {
        _bonuses = bonuses;
        _walls = walls;
        _tanks = tanks;
        _spawnTimer = GetRandomInterval();
    }

    public void Update(float deltaTime)
    {
        foreach (var bonus in _bonuses) bonus.Update(deltaTime);

        _bonuses.RemoveAll(b => b.IsExpired);

        _spawnTimer -= deltaTime;
        if (_spawnTimer <= 0f)
        {
            TrySpawnBonus();
            _spawnTimer = GetRandomInterval();
        }
    }

    private void TrySpawnBonus()
    {
        BonusType type = (BonusType)_rng.Next(0, Enum.GetValues<BonusType>().Length);
        Vector2 pos;
        int attempts = 0;

        do
        {
            pos = new Vector2(_rng.NextSingle() * (MaxX - MinX) + MinX, _rng.NextSingle() * (MaxY - MinY) + MinY);
            attempts++;
        } while (!IsPositionFree(pos) && attempts < 50);

        if (attempts < 50)
        {
            _bonuses.Add(BonusFactory.Create(type, pos));
        }
    }

    private bool IsPositionFree(Vector2 pos)
    {
        var bonusBounds = new RectAABB(pos, new Vector2(16f, 16f));
        foreach (var wall in _walls) if (bonusBounds.Intersects(wall.Bounds)) return false;
        foreach (var tank in _tanks) if (!tank.IsDestroyed && bonusBounds.Intersects(tank.Bounds)) return false;
        return true;
    }

    private float GetRandomInterval() => 4f + _rng.NextSingle() * 6f;
}