using System.Collections.Generic;
using System.Linq;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns;
using TankGL_fbo.Core.Patterns.Decorators;
using TankGL_fbo.Core.Systems;

namespace TankGL_fbo.Core.Scenes;

public sealed class GameScene : IScene
{
    private readonly List<Tank> _tanks = [];
    private readonly List<Bullet> _bullets = [];
    private readonly List<Wall> _walls = [];
    private readonly List<Bonus> _bonuses = [];
    private readonly List<Background> _backgrounds = [];

    private InputSystem _inputSystem = null!;
    private CollisionSystem _collisionSystem = null!;
    private SpawnSystem _spawnSystem = null!;

    private double _accumulator;
    private const double FixedDt = 1.0 / 60.0;

    public IReadOnlyList<Tank> Tanks => _tanks;

    public void OnEnter()
    {

        var background = new Background(new Vector2(0, 0), new Vector2(400, 300), "tile.png");
        _backgrounds.Add(background);

        _walls.AddRange(CreateOuterWalls(background));
        _walls.Add(new Wall(new Vector2(-150, 150), new Vector2(60, 20)));
        _walls.Add(new Wall(new Vector2(150, -150), new Vector2(60, 20)));

        _tanks.Add(new Tank(new Vector2(-250, 0), "tank_red.png", new BaseStats()));
        _tanks.Add(new Tank(new Vector2(250, 0), "tank_blue.png", new BaseStats()));

        _inputSystem = new InputSystem(_tanks, _bullets);
        _collisionSystem = new CollisionSystem(_tanks, _walls, _bonuses, _bullets);
        _spawnSystem = new SpawnSystem(_bonuses, _walls, _tanks);

        _accumulator = 0;
        _bullets.Clear();
        _bonuses.Clear();
    }

    public void OnExit()
    {
        _tanks.Clear();
        _bullets.Clear();
        _walls.Clear();
        _bonuses.Clear();
        _backgrounds.Clear();
    }

    public void Update(float deltaTime, Dictionary<int, HashSet<PlayerAction>> inputs)
    {

        _accumulator += deltaTime;
        while (_accumulator >= FixedDt)
        {
            float fixedDt = (float)FixedDt;

            foreach (var t in _tanks) t.Update(fixedDt);
            foreach (var b in _bullets) b.Update(fixedDt);
            foreach (var b in _bonuses) b.Update(fixedDt);

            _inputSystem.Process(inputs, fixedDt);
            _spawnSystem.Update(fixedDt);
            _collisionSystem.Resolve();

            _bullets.RemoveAll(b => b.IsExpired);
            _bonuses.RemoveAll(b => b.IsExpired);

            _accumulator -= FixedDt;
        }
    }

    public void CollectRenderables(List<IRenderable> renderables)
    {
        renderables.AddRange(_backgrounds);
        renderables.AddRange(_walls);
        renderables.AddRange(_tanks.Where(t => !t.IsDestroyed));
        renderables.AddRange(_bonuses);
        renderables.AddRange(_bullets);
    }

    public void ApplyBonus(int tankIndex, BonusType type)
    {
        if (tankIndex >= 0 && tankIndex < _tanks.Count)
        {
            var tank = _tanks[tankIndex];
            if (!tank.IsDestroyed)
            {
                _collisionSystem.ApplyBonus(tank, type);
            }
        }
    }

    private static List<Wall> CreateOuterWalls(Background bg, float thickness = 20f, float outerExtent = 10000f)
    {
        var walls = new List<Wall>();
        var half = bg.Bounds.HalfSize;
        var center = bg.Position;
        float t2 = thickness / 2f;
        float huge = outerExtent / 2f;

        walls.Add(new Wall(
            new Vector2(center.X, center.Y - half.Y - t2 - huge),
            new Vector2(half.X + thickness + huge, huge + t2)
        ));

        walls.Add(new Wall(
            new Vector2(center.X, center.Y + half.Y + t2 + huge),
            new Vector2(half.X + thickness + huge, huge + t2)
        ));

        walls.Add(new Wall(
            new Vector2(center.X - half.X - t2 - huge, center.Y),
            new Vector2(huge + t2, half.Y)
        ));

        walls.Add(new Wall(
            new Vector2(center.X + half.X + t2 + huge, center.Y),
            new Vector2(huge + t2, half.Y)
        ));

        return walls;
    }
}