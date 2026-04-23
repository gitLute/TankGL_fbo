using System;
using System.Collections.Generic;
using System.Linq;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns;
using TankGL_fbo.Core.Systems;

namespace TankGL_fbo.Core.Scenes;

public abstract class LevelScene : IScene
{
    protected readonly List<Tank> Tanks = [];
    protected readonly List<Bullet> Bullets = [];
    protected readonly List<Wall> Walls = [];
    protected readonly List<Bonus> Bonuses = [];
    protected readonly List<Background> Backgrounds = [];

    protected Action<IScene>? RequestSceneChange => _requestSceneChange;

    private InputSystem _inputSystem = null!;
    private CollisionSystem _collisionSystem = null!;
    private SpawnSystem _spawnSystem = null!;
    private double _accumulator;
    private const double FixedDt = 1.0 / 60.0;
    private readonly Action<IScene>? _requestSceneChange;
    private bool _levelCompleted;

    private float _hudUpdateTimer = 0f;
    private const float HudUpdateInterval = 0.1f;

    public IReadOnlyList<Tank> PublicTanks => Tanks;

    public event EventHandler<(string p1Stats, string p2Stats)>? HudDataUpdated;


    protected LevelScene(Action<IScene>? requestSceneChange = null)
    {
        _requestSceneChange = requestSceneChange;
    }

    public virtual void OnEnter()
    {
        var background = new Background(new Vector2(0, 0), new Vector2(400, 300), "tile.png");
        Backgrounds.Add(background);
        Walls.AddRange(CreateOuterWalls(background));
        SetupLevel();

        _inputSystem = new InputSystem(Tanks, Bullets);
        _collisionSystem = new CollisionSystem(Tanks, Walls, Bonuses, Bullets);
        _spawnSystem = new SpawnSystem(Bonuses, Walls, Tanks);

        _accumulator = 0;
        _levelCompleted = false;
        Bullets.Clear();
        Bonuses.Clear();

        RaiseHudUpdate();
    }

    public virtual void OnExit()
    {
        Tanks.Clear();
        Bullets.Clear();
        Walls.Clear();
        Bonuses.Clear();
        Backgrounds.Clear();
    }

    public virtual void Update(float deltaTime, Dictionary<int, HashSet<PlayerAction>> inputs)
    {
        if (_levelCompleted)
        {
            RaiseHudUpdate();
            return;
        }

        _accumulator += deltaTime;
        while (_accumulator >= FixedDt)
        {
            float fixedDt = (float)FixedDt;
            foreach (var t in Tanks) t.Update(fixedDt);
            foreach (var b in Bullets) b.Update(fixedDt);
            foreach (var b in Bonuses) b.Update(fixedDt);

            _inputSystem.Process(inputs, fixedDt);
            _spawnSystem.Update(fixedDt);
            _collisionSystem.Resolve();

            Bullets.RemoveAll(b => b.IsExpired);
            Bonuses.RemoveAll(b => b.IsExpired);

            if (Tanks.Any(t => t.IsDestroyed))
            {
                _levelCompleted = true;
                var nextLevel = CreateNextLevel();
                if (nextLevel != null)
                {
                    _requestSceneChange?.Invoke(nextLevel);
                }
                break;
            }

            _accumulator -= FixedDt;
        }

        _hudUpdateTimer += deltaTime;
        if (_hudUpdateTimer >= HudUpdateInterval)
        {
            RaiseHudUpdate();
            _hudUpdateTimer = 0f;
        }
    }

    public virtual void CollectRenderables(List<IRenderable> renderables)
    {
        renderables.AddRange(Backgrounds);
        renderables.AddRange(Walls);
        renderables.AddRange(Tanks.Where(t => !t.IsDestroyed));
        renderables.AddRange(Bonuses);
        renderables.AddRange(Bullets);
    }

    public void ApplyBonus(int tankIndex, BonusType type)
    {
        if (tankIndex >= 0 && tankIndex < Tanks.Count)
        {
            var tank = Tanks[tankIndex];
            if (!tank.IsDestroyed)
            {
                _collisionSystem.ApplyBonus(tank, type);

                RaiseHudUpdate();
            }
        }
    }

    public void RequestReturnToMenu()
    {
        _requestSceneChange?.Invoke(new MenuScene(_requestSceneChange));
    }

    private void RaiseHudUpdate()
    {
        // string p1 = string.Empty;
        // string p2 = string.Empty;

        string p1, p2;

        var t1 = Tanks[0];
        var t2 = Tanks[1];

        if (!t1.IsDestroyed)
        {
            p1 = $"P1:\nHP {t1.HP:F0}\nAMMO {t1.Stats.Ammo}\nFUEL {t1.Stats.Fuel:F0}";
        }
        else
        {
            p1 = "P1:\nDESTROYED";
        }

        if (!t2.IsDestroyed)
        {
            p2 = $"P2:\nHP {t2.HP:F0}\nAMMO {t2.Stats.Ammo}\nFUEL {t2.Stats.Fuel:F0}";
        }
        else
        {
            p2 = "P2:\nDESTROYED";
        }

        HudDataUpdated?.Invoke(this, (p1, p2));
    }


    protected abstract void SetupLevel();
    protected virtual IScene? CreateNextLevel() => null;

    protected static List<Wall> CreateOuterWalls(Background bg, float thickness = 20f, float outerExtent = 10000f)
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