using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns.Decorators;
using TankGL_fbo.Core.Systems;
using TankGL_fbo.WPF.Systems;

using Vector2 = TankGL_fbo.Core.Contracts.Vector2;

namespace TankGL_fbo.WPF;

public partial class MainWindow : Window
{
    private GameLoop _gameLoop = null!;
    private AssetManager _assets = null!;
    private Shader _shader = null!;
    private GLControl _glControl = null!;

    private readonly Dictionary<int, HashSet<PlayerAction>> _activeInputs = new() { [0] = new(), [1] = new() };
    private readonly Dictionary<Key, (int playerId, PlayerAction action)> _keyMap = new();

    private readonly List<IRenderable> _renderQueue = new();
    private Matrix4 _projection;
    private readonly Stopwatch _stopwatch = new();
    private readonly DispatcherTimer _timer = new();

    public MainWindow()
    {
        InitializeComponent();
        SetupInputMap();
    }

    private void SetupInputMap()
    {
        _keyMap[Key.W] = (0, PlayerAction.MoveUp);
        _keyMap[Key.S] = (0, PlayerAction.MoveDown);
        _keyMap[Key.A] = (0, PlayerAction.RotateLeft);
        _keyMap[Key.D] = (0, PlayerAction.RotateRight);
        _keyMap[Key.Space] = (0, PlayerAction.Fire);

        _keyMap[Key.Up] = (1, PlayerAction.MoveUp);
        _keyMap[Key.Down] = (1, PlayerAction.MoveDown);
        _keyMap[Key.Left] = (1, PlayerAction.RotateLeft);
        _keyMap[Key.Right] = (1, PlayerAction.RotateRight);
        _keyMap[Key.Enter] = (1, PlayerAction.Fire);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _glControl = new GLControl();
        _glControl.Load += GlControl_Load;
        _glControl.Paint += GlControl_Paint;
        _glControl.Resize += GlControl_Resize;
        _glControl.Dock = System.Windows.Forms.DockStyle.Fill;

        string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
        _assets = new AssetManager(assetsPath);
        _assets.Init();
        _shader = _assets.GetShader("default");

        var tanks = new List<Tank>
        {
            new(new Vector2(-250, 0), "tank_red.png", new BaseStats()),
            new(new Vector2(250, 0), "tank_blue.png", new BaseStats())
        };

        var bullets = new List<Bullet>();
        var walls = new List<Wall>
        {
            new(new Vector2(0, 0), new Vector2(20, 100)),
            new(new Vector2(-150, 150), new Vector2(60, 20)),
            new(new Vector2(150, -150), new Vector2(60, 20))
        };
        var bonuses = new List<Bonus>();

        _gameLoop = new GameLoop(tanks, bullets, walls, bonuses);
        _gameLoop.RenderReady += OnRenderReady;

        Host.Child = _glControl;

        _stopwatch.Start();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += GameTimer_Tick;
        _timer.Start();
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        if (_shader == null || _assets == null) return;

        float dt = (float)_stopwatch.Elapsed.TotalSeconds;
        _stopwatch.Restart();

        _gameLoop.Tick(_activeInputs, dt);
        _glControl.Invalidate();

        if (_gameLoop.Tanks.Count >= 2)
        {
            TxtP1HP.Text = $"P1 HP: {(int)_gameLoop.Tanks[0].HP}";
            TxtP2HP.Text = $"P2 HP: {(int)_gameLoop.Tanks[1].HP}";
        }
    }

    private void OnRenderReady(IEnumerable<IRenderable> entities)
    {
        _renderQueue.Clear();
        _renderQueue.AddRange(entities);
    }

    private void GlControl_Load(object? sender, EventArgs e)
    {
        GL.ClearColor(0.15f, 0.15f, 0.15f, 1.0f);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private void GlControl_Resize(object? sender, EventArgs e)
    {
        GL.Viewport(0, 0, _glControl.Width, _glControl.Height);

        _projection = Matrix4.CreateOrthographic(_glControl.Width, _glControl.Height, -1, 1);
    }

    private void GlControl_Paint(object? sender, System.Windows.Forms.PaintEventArgs e)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _shader.Use();
        _shader.SetMatrix4("uProjection", _projection);

        foreach (var entity in _renderQueue.OrderBy(x => x.ZIndex))
        {
            var tex = _assets.LoadTexture(entity.TexturePath);
            tex.Use();

            var model = Matrix4.CreateScale(tex.Width * entity.Scale, tex.Height * entity.Scale, 1.0f) *
                        Matrix4.CreateRotationZ(entity.Rotation) *
                        Matrix4.CreateTranslation(entity.Position.X, entity.Position.Y, 0);

            _shader.SetMatrix4("uModel", model);
            _assets.Quad.Bind();
            _assets.Quad.Draw();
        }

        _glControl.SwapBuffers();
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (_keyMap.TryGetValue(e.Key, out var map))
            _activeInputs[map.playerId].Add(map.action);
    }

    private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (_keyMap.TryGetValue(e.Key, out var map))
            _activeInputs[map.playerId].Remove(map.action);
    }
}