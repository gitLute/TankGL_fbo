using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns.Decorators;
using TankGL_fbo.Core.Systems;
using TankGL_fbo.WPF.Systems;
using TankGL_fbo.Core.Patterns;

using Vector2 = TankGL_fbo.Core.Contracts.Vector2;

namespace TankGL_fbo.WPF
{
    public partial class MainWindow : Window
    {
        private GameLoop _gameLoop = null!;
        private AssetManager _assets = null!;
        private Shader _shader = null!;
        private GLControl _glControl = null!;
        private bool _isInitialized = false;
        private OpenGlTextRenderer? _textRenderer;

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

        [Obsolete]
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = new GLControlSettings
            {
                Profile = ContextProfile.Compatability,
                API = ContextAPI.OpenGL,
                APIVersion = new Version(3, 3)
            };

            _glControl = new GLControl(settings);
            _glControl.Load += GlControl_Load;
            _glControl.Paint += GlControl_Paint;
            _glControl.Resize += GlControl_Resize;
            _glControl.Dock = System.Windows.Forms.DockStyle.Fill;


            Host.Child = _glControl;

            _stopwatch.Start();
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += GameTimer_Tick;
            _timer.Start();
        }

        private void GlControl_Load(object? sender, EventArgs e)
        {
            if (_glControl == null) return;
            _glControl.MakeCurrent();

            string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
            _assets = new AssetManager(assetsPath);

            try
            {
                _assets.Init();
                _shader = _assets.GetShader("default");
                _textRenderer = new OpenGlTextRenderer();

                GL.ClearColor(0.15f, 0.15f, 0.15f, 1.0f);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                var tanks = new List<Tank>
                {
                    new(new Vector2(-250, 0), "tank_red.png", new BaseStats()),
                    new(new Vector2(250, 0), "tank_blue.png", new BaseStats())
                };

                _gameLoop = new GameLoop(tanks, new List<Bullet>(), new List<Wall>
                {
                    new(new Vector2(0, 0), new Vector2(20, 100)),
                    new(new Vector2(-150, 150), new Vector2(60, 20)),
                    new(new Vector2(150, -150), new Vector2(60, 20))
                }, new List<Bonus>());

                _gameLoop.RenderReady += OnRenderReady;
                _isInitialized = true;

                GlControl_Resize(_glControl, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка инициализации OpenGL: {ex.Message}");
                _isInitialized = false;
            }
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isInitialized || _gameLoop == null) return;

            float dt = (float)_stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();

            _gameLoop.Tick(_activeInputs, Math.Min(dt, 0.1f));

            _glControl.Invalidate();
        }

        private void UpdateHud()
        {
            if (_gameLoop != null && _gameLoop.Tanks.Count >= 2 && _textRenderer != null)
            {
                var tank1 = _gameLoop.Tanks[0];
                var tank2 = _gameLoop.Tanks[1];

                static string GetBonusInfo(Tank tank)
                {
                    if (tank.Stats is TankGL_fbo.Core.Patterns.Decorators.StatDecorator dec && !dec.IsExpired)
                    {
                        string typeName = dec.GetType().Name.Replace("Decorator", "");
                        return $"Bonus: {typeName} ({dec.DurationLeft:F1}s)";
                    }
                    return "Bonus: None";
                }

                string statsPl1 = $"P1 HP: {(int)Math.Max(0, tank1.HP)}\n" +
                                $"Ammo: {tank1.Stats.Ammo}\n" +
                                $"Fuel: {(int)tank1.Stats.Fuel}\n" +
                                $"Spd: {(int)tank1.Stats.Speed}\n" +
                                $"Arm: {(int)tank1.Stats.Armor}\n" +
                                $"Dmg: {(int)tank1.Stats.Damage}\n" +
                                $"{GetBonusInfo(tank1)}";

                string statsPl2 = $"P2 HP: {(int)Math.Max(0, tank2.HP)}\n" +
                                $"Ammo: {tank2.Stats.Ammo}\n" +
                                $"Fuel: {(int)tank2.Stats.Fuel}\n" +
                                $"Spd: {(int)tank2.Stats.Speed}\n" +
                                $"Arm: {(int)tank2.Stats.Armor}\n" +
                                $"Dmg: {(int)tank2.Stats.Damage}\n" +
                                $"{GetBonusInfo(tank2)}";

                _textRenderer.DrawText(statsPl1, 50, 50, 16, Host.Child.Width, Host.Child.Height);

                _textRenderer.DrawText(statsPl2, Host.Child.Width - 250, 50, 16, Host.Child.Width, Host.Child.Height);
            }
        }

        private void OnRenderReady(IEnumerable<IRenderable> entities)
        {
            _renderQueue.Clear();
            _renderQueue.AddRange(entities);
        }

        private void GlControl_Resize(object? sender, EventArgs e)
        {
            if (!_isInitialized || _glControl == null || _glControl.ClientSize.Width <= 0) return;

            _glControl.MakeCurrent();
            GL.Viewport(0, 0, _glControl.ClientSize.Width, _glControl.ClientSize.Height);
            _projection = Matrix4.CreateOrthographic(_glControl.ClientSize.Width, _glControl.ClientSize.Height, -1, 1);
        }

        [Obsolete]
        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            if (!_isInitialized || _shader == null || _glControl == null) return;
            _glControl.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            _shader.Use();
            _shader.SetMatrix4("uProjection", _projection);
            
            var renderList = _renderQueue.OrderBy(x => x.ZIndex).ToList();
            
            
            const float TileSize = 50f; 

            foreach (var entity in renderList)
            {
                var tex = _assets.LoadTexture(entity.TexturePath);
                if (tex == null) continue;
                
                float renderWidth = entity.Bounds.HalfSize.X * 2f;
                float renderHeight = entity.Bounds.HalfSize.Y * 2f;
                
                
                OpenTK.Mathematics.Vector2 uvScale;
                if (entity is TankGL_fbo.Core.Entities.Wall)
                {
                    
                    float aspect = (float)tex.Width / (float)tex.Height;
                    uvScale = new OpenTK.Mathematics.Vector2(
                        renderWidth / TileSize, 
                        renderHeight * aspect / TileSize
                    );
                }
                else
                {
                    
                    uvScale = new OpenTK.Mathematics.Vector2(1f, 1f);
                }
                
                _shader.SetVector2("uUvScale", uvScale); 
                
                var model = Matrix4.CreateScale(renderWidth, renderHeight, 1.0f) *
                            Matrix4.CreateRotationZ(entity.Rotation) *
                            Matrix4.CreateTranslation(entity.Position.X, entity.Position.Y, 0);
                _shader.SetMatrix4("uModel", model);
                
                tex.Use();
                _assets.Quad.Bind();
                _assets.Quad.Draw();
            }

            
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            foreach (var entity in renderList)
            {
                var boxWidth = entity.Bounds.HalfSize.X * 2;
                var boxHeight = entity.Bounds.HalfSize.Y * 2;
                var debugModel = Matrix4.CreateScale(boxWidth, boxHeight, 1.0f) * 
                                Matrix4.CreateTranslation(entity.Bounds.Center.X, entity.Bounds.Center.Y, 0);
                _shader.SetMatrix4("uModel", debugModel);
                _assets.Quad.Draw();
            }
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            
            GL.UseProgram(0);
            UpdateHud();
            _glControl.SwapBuffers();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_keyMap.TryGetValue(e.Key, out var map))
            {
                _activeInputs[map.playerId].Add(map.action);
                return;
            }

            
            if (_gameLoop != null)
            {
                BonusType? bonus = e.Key switch
                {
                    Key.D1 or Key.NumPad1 => BonusType.SpeedUp,
                    Key.D2 or Key.NumPad2 => BonusType.Shield,
                    Key.D3 or Key.NumPad3 => BonusType.DamageBoost,
                    Key.D4 or Key.NumPad4 => BonusType.AmmoRefill,
                    Key.D5 or Key.NumPad5 => BonusType.FuelCan,
                    _ => null
                };

                if (bonus.HasValue)
                {
                    
                    int tankIndex = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? 1 : 0;
                    _gameLoop.ApplyBonus(tankIndex, bonus.Value);
                }
            }
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_keyMap.TryGetValue(e.Key, out var map)) _activeInputs[map.playerId].Remove(map.action);
        }
    }
}