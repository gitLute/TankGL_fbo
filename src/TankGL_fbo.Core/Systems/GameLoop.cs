using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns;

namespace TankGL_fbo.Core.Systems;

public sealed class GameLoop
{
    private readonly List<Tank> _tanks;
    public IReadOnlyList<Tank> Tanks => _tanks;
    private readonly List<Bullet> _bullets;
    private readonly List<Wall> _walls;
    private readonly List<Bonus> _bonuses;
    private readonly List<Background> _backgrounds;

    private readonly InputSystem _input;
    private readonly CollisionSystem _collision;
    private readonly SpawnSystem _spawn;

    private double _accumulator = 0;
    private const double FixedDt = 1.0 / 60.0;

    public event Action<IEnumerable<IRenderable>>? RenderReady;

    public GameLoop(List<Tank> tanks, List<Bullet> bullets, List<Wall> walls, List<Bonus> bonuses, List<Background> backgrounds)
    {
        _tanks = tanks;
        _bullets = bullets;
        _walls = walls;
        _bonuses = bonuses;
        _backgrounds = backgrounds;

        _input = new InputSystem(_tanks, _bullets);
        _collision = new CollisionSystem(_tanks, _walls, _bonuses, _bullets);
        _spawn = new SpawnSystem(_bonuses, _walls, _tanks);
    }

    public void Tick(Dictionary<int, HashSet<PlayerAction>> playerInputs, float deltaTime)
    {
        _accumulator += deltaTime;

        while (_accumulator >= FixedDt)
        {
            float fixedDt = (float)FixedDt;

            foreach (var t in _tanks) t.Update(fixedDt);
            foreach (var b in _bonuses) b.Update(fixedDt);
            foreach (var b in _bullets) b.Update(fixedDt);

            _input.Process(playerInputs, fixedDt);

            _spawn.Update(fixedDt);

            _collision.Resolve();

            _bullets.RemoveAll(b => b.IsExpired);
            _bonuses.RemoveAll(b => b.IsExpired);

            _accumulator -= FixedDt;
        }

        var renderables = new List<IRenderable>(_tanks.Count + _bullets.Count + _walls.Count + _bonuses.Count + _backgrounds.Count);

        renderables.AddRange(_backgrounds);
        renderables.AddRange(_walls);
        renderables.AddRange(_tanks.Where(t => !t.IsDestroyed));
        renderables.AddRange(_bonuses);
        renderables.AddRange(_bullets);

        RenderReady?.Invoke(renderables);
    }

    public void ApplyBonus(int tankIndex, BonusType type)
    {
        if (tankIndex >= 0 && tankIndex < _tanks.Count)
        {
            var tank = _tanks[tankIndex];
            if (!tank.IsDestroyed)
            {
                _collision.ApplyBonus(tank, type);
            }
        }
    }
}