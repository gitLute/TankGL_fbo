﻿using System;
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
using TankGL_fbo.Core.Patterns;
using TankGL_fbo.Core.Patterns.Decorators;
using TankGL_fbo.Core.Scenes;
using TankGL_fbo.Core.Systems;
using TankGL_fbo.WPF.Systems;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using Vector2 = TankGL_fbo.Core.Contracts.Vector2;

namespace TankGL_fbo.WPF
{
    public partial class MainWindow : Window
    {
        private SceneManager _sceneManager = null!;
        private AssetManager _assets = null!;
        private Shader _shader = null!;
        private GLControl _glControl = null!;
        private OpenGlTextRenderer? _textRenderer;
        private bool _isInitialized;
        private Matrix4 _projection;
        private readonly Stopwatch _stopwatch = new();
        private readonly DispatcherTimer _timer = new();
        private readonly List<IRenderable> _renderQueue = new();
        private readonly Dictionary<int, HashSet<PlayerAction>> _activeInputs = new() { [0] = new(), [1] = new() };
        private readonly Dictionary<Key, (int playerId, PlayerAction action)> _keyMap = new();
        private string _hudPlayer1Stats = string.Empty;
        private string _hudPlayer2Stats = string.Empty;
        private string[] _menuItems = Array.Empty<string>();
        private int _menuSelectedIndex;
        private enum SceneState { Menu, Info, Level, Options, Other }
        private SceneState _currentSceneState = SceneState.Other;
        private float VirtualWidth => ConfigManager.Config.ResolutionWidth;
        private float VirtualHeight => ConfigManager.Config.ResolutionHeight;
        private int _vpX, _vpY, _vpW, _vpH;

        private int _cachedMenuFontSize = 24;
        private int _cachedStatsFontSize = 16;

        public MainWindow()
        {
            InitializeComponent();
            SetupInputMap();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeOpenGL();
            InitializeGameSystems();
            SubscribeToEvents();
            StartGameLoop();
        }

        private void InitializeOpenGL()
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
        }

        private void InitializeGameSystems()
        {
            ConfigManager.Load();

            _cachedMenuFontSize = ConfigManager.Config.MenuFontSize;
            _cachedStatsFontSize = _cachedMenuFontSize * 2 / 3;
            ConfigManager.MenuFontSizeChanged += OnMenuFontSizeChanged;

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
                _sceneManager = new SceneManager();
                var initialScene = new InfoScene(_sceneManager.RequestSceneChange);
                _sceneManager.ChangeScene(initialScene);
                _isInitialized = true;
                ConfigManager.ConfigSaved += OnConfigSaved;
                GlControl_Resize(_glControl, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации OpenGL: {ex.Message}");
                _isInitialized = false;
            }
        }

        private void OnMenuFontSizeChanged(int newSize)
        {
            _cachedMenuFontSize = newSize;
            _cachedStatsFontSize = newSize * 2 / 3;
        }

        private void OnConfigSaved()
        {
            if (!_isInitialized) return;
            Dispatcher.Invoke(() => GlControl_Resize(_glControl, EventArgs.Empty));
        }

        private void StartGameLoop()
        {
            _stopwatch.Start();
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += GameTimer_Tick;
            _timer.Start();
        }

        private void SubscribeToEvents()
        {
            _sceneManager.SceneChanged += OnSceneChanged;
            _sceneManager.SceneChangeRequested += OnSceneChangeRequested;
            SubscribeToCurrentSceneEvents();
        }

        private void SubscribeToCurrentSceneEvents()
        {
            if (_sceneManager.CurrentScene is LevelScene level)
            {
                level.HudDataUpdated += OnHudDataUpdated;
            }
            else if (_sceneManager.CurrentScene is MenuSceneBase menu)
            {
                menu.MenuStateChanged += OnMenuStateChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_sceneManager != null)
            {
                _sceneManager.SceneChanged -= OnSceneChanged;
                _sceneManager.SceneChangeRequested -= OnSceneChangeRequested;
            }
            UnsubscribeFromCurrentSceneEvents();
        }

        private void UnsubscribeFromCurrentSceneEvents()
        {
            if (_sceneManager?.CurrentScene is LevelScene level)
            {
                level.HudDataUpdated -= OnHudDataUpdated;
            }
            else if (_sceneManager?.CurrentScene is MenuSceneBase menu)
            {
                menu.MenuStateChanged -= OnMenuStateChanged;
            }
        }

        private void OnSceneChanged(object? sender, IScene newScene)
        {
            UnsubscribeFromCurrentSceneEvents();
            SubscribeToCurrentSceneEvents();
            _currentSceneState = newScene switch
            {
                MenuScene => SceneState.Menu,
                InfoScene => SceneState.Info,
                LevelScene => SceneState.Level,
                OptionsScene => SceneState.Options,
                _ => SceneState.Other
            };
            if (newScene is MenuSceneBase menuScene)
            {
                menuScene.RequestMenuStateUpdate();
            }
            foreach (var set in _activeInputs.Values) set.Clear();
        }

        private void OnSceneChangeRequested(object? sender, IScene requestedScene)
        {
            _sceneManager.ChangeScene(requestedScene, 0.3f);
        }

        private void OnHudDataUpdated(object? sender, (string p1Stats, string p2Stats) hudData)
        {
            _hudPlayer1Stats = hudData.p1Stats;
            _hudPlayer2Stats = hudData.p2Stats;
        }

        private void OnMenuStateChanged(object? sender, (string[] items, int selectedIndex) menuState)
        {
            _menuItems = menuState.items;
            _menuSelectedIndex = menuState.selectedIndex;
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isInitialized) return;
            float dt = (float)_stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();
            _sceneManager.Update(Math.Min(dt, 0.1f), _activeInputs);
            _glControl.Invalidate();
        }

        private void GlControl_Load(object? sender, EventArgs e) { }

        private void GlControl_Resize(object? sender, EventArgs e)
        {
            if (!_isInitialized || _glControl == null || _glControl.ClientSize.Width <= 0 || _glControl.ClientSize.Height <= 0) return;
            _glControl.MakeCurrent();
            int actualW = _glControl.ClientSize.Width;
            int actualH = _glControl.ClientSize.Height;
            var vp = new Viewport(VirtualWidth, VirtualHeight, actualW, actualH);
            float scale = vp.ScaleFactor;
            var offset = vp.Offset;
            _vpX = (int)MathF.Round(offset.X);
            _vpY = (int)MathF.Round(offset.Y);
            _vpW = (int)MathF.Round(VirtualWidth * scale);
            _vpH = (int)MathF.Round(VirtualHeight * scale);
            GL.Viewport(_vpX, _vpY, _vpW, _vpH);
            _projection = Matrix4.CreateOrthographic(VirtualWidth, VirtualHeight, -1, 1);
        }

        private void GlControl_Paint(object? sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (!_isInitialized || _shader == null || _glControl == null) return;
            _glControl.MakeCurrent();
            GL.Viewport(0, 0, _glControl.ClientSize.Width, _glControl.ClientSize.Height);
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(_vpX, _vpY, _vpW, _vpH);
            GL.ClearColor(0.15f, 0.15f, 0.15f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            RenderScene();
            RenderDebugBounds();
            GL.UseProgram(0);
            DrawHud();
            _glControl.SwapBuffers();
        }

        private void RenderScene()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            _shader.Use();
            _shader.SetMatrix4("uProjection", _projection);
            _renderQueue.Clear();
            _sceneManager.CollectRenderables(_renderQueue);
            var renderList = _renderQueue.OrderBy(x => x.ZIndex).ToList();
            const float TileSize = 50f;
            foreach (var entity in renderList)
            {
                var tex = _assets.LoadTexture(entity.TexturePath);
                if (tex == null) continue;
                float renderWidth = entity.Bounds.HalfSize.X * 2f;
                float renderHeight = entity.Bounds.HalfSize.Y * 2f;
                OpenTK.Mathematics.Vector2 uvScale;
                if (entity is Wall || (entity is Background bg && bg.Tile))
                {
                    float aspect = (float)tex.Width / tex.Height;
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
        }

        private void RenderDebugBounds()
        {
            if (!ConfigManager.Config.ShowColliderBounds) return;
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            foreach (var entity in _renderQueue)
            {
                var boxWidth = entity.Bounds.HalfSize.X * 2;
                var boxHeight = entity.Bounds.HalfSize.Y * 2;
                var debugModel = Matrix4.CreateScale(boxWidth, boxHeight, 1.0f) *
                                 Matrix4.CreateTranslation(entity.Bounds.Center.X, entity.Bounds.Center.Y, 0);
                _shader.SetMatrix4("uModel", debugModel);
                _assets.Quad.Draw();
            }
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        private void DrawHud()
        {
            if (_textRenderer == null) return;
            int width = (int)VirtualWidth;
            int height = (int)VirtualHeight;

            int menuFontSize = _cachedMenuFontSize;
            int statsFontSize = _cachedStatsFontSize;

            float menuFontProcentage = menuFontSize / height;
            float statFontProcentage = statsFontSize / height;
            float menuX = (int)(width / (10 + menuFontProcentage));
            float menuY = (int)(height / 3);
            float statsX_PL1 = (int)(width / (15 + statFontProcentage));
            float statsX_PL2 = (int)(width * 13 / (15 + statFontProcentage));
            float statsY = (int)(height / 15);
            float statsCenterX = (int)(width / (2 + statFontProcentage));
            float buildX = (int)(width / (10 + statFontProcentage));
            float buildY = (int)(height / 10);

            switch (_currentSceneState)
            {
                case SceneState.Level:
                    if (!string.IsNullOrEmpty(_hudPlayer1Stats))
                        _textRenderer.DrawText(_hudPlayer1Stats, statsX_PL1, statsY, statsFontSize, width, height);
                    if (!string.IsNullOrEmpty(_hudPlayer2Stats))
                        _textRenderer.DrawText(_hudPlayer2Stats, statsX_PL2, statsY, statsFontSize, width, height);
                    if (ConfigManager.Config.ShowColliderBounds)
                        _textRenderer.DrawText("[DEBUG: COLLIDERS ON]", statsCenterX, statsY, statsFontSize, width, height, Color.Coral);
                    break;
                case SceneState.Menu:
                    for (int i = 0; i < _menuItems.Length; i++)
                    {
                        string text = i == _menuSelectedIndex ? $"> {_menuItems[i]} <" : _menuItems[i];
                        _textRenderer.DrawText(text, menuX, menuY + i * menuFontSize, menuFontSize, width, height);
                    }
                    break;
                case SceneState.Info:
                    float infoX = menuX;
                    float infoY =(int)(height / 10);

                    // foreach (var line in InfoScene.Instructions)
                    // {
                    //     _textRenderer.DrawText(line, infoX, infoY, menuFontSize, width, height, Color.White);
                    //     infoY += menuFontSize * 1.2f;
                    // }

                    for (int i = 0; i < InfoScene.Instructions.Length; i++)
                    {
                        string text = InfoScene.Instructions[i];
                        _textRenderer.DrawText(text, infoX, infoY + i * menuFontSize, menuFontSize, width, height);
                    }
                    break;
                case SceneState.Options:
                    for (int i = 0; i < _menuItems.Length; i++)
                    {
                        string text = i == _menuSelectedIndex ? $"> {_menuItems[i]} <" : _menuItems[i];
                        _textRenderer.DrawText(text, menuX, menuY + i * menuFontSize, menuFontSize, width, height);
                    }
                    string buildInfo = $"Build: {TankGL_fbo.Core.GitBuildInfo.BuildDate}\nGit: {TankGL_fbo.Core.GitBuildInfo.Branch}@{TankGL_fbo.Core.GitBuildInfo.Commit}";
                    _textRenderer.DrawText(buildInfo, buildX, buildY, statsFontSize, width, height, Color.Coral);
                    break;
                default:
                    break;
            }
        }

        private void SetupInputMap()
        {
            _keyMap[Key.E] = (0, PlayerAction.Confirm);
            _keyMap[Key.W] = (0, PlayerAction.MoveUp);
            _keyMap[Key.S] = (0, PlayerAction.MoveDown);
            _keyMap[Key.A] = (0, PlayerAction.RotateLeft);
            _keyMap[Key.D] = (0, PlayerAction.RotateRight);
            _keyMap[Key.Space] = (0, PlayerAction.Fire);
            _keyMap[Key.LeftCtrl] = (0, PlayerAction.Fire);
            _keyMap[Key.Up] = (1, PlayerAction.MoveUp);
            _keyMap[Key.Down] = (1, PlayerAction.MoveDown);
            _keyMap[Key.Left] = (1, PlayerAction.RotateLeft);
            _keyMap[Key.Right] = (1, PlayerAction.RotateRight);
            _keyMap[Key.RightShift] = (1, PlayerAction.Fire);
            _keyMap[Key.RightCtrl] = (1, PlayerAction.Fire);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleGlobalShortcuts(e);
            if (_keyMap.TryGetValue(e.Key, out var map))
            {
                _activeInputs[map.playerId].Add(map.action);
                e.Handled = true;
                return;
            }
            HandleDebugLevelSwitch(e);
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (_keyMap.TryGetValue(e.Key, out var map))
            {
                _activeInputs[map.playerId].Remove(map.action);
                e.Handled = true;
            }
        }

        private void HandleGlobalShortcuts(KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                _sceneManager.ChangeScene(new MenuScene(_sceneManager.RequestSceneChange));
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F1)
            {
                _sceneManager.ChangeScene(new InfoScene(_sceneManager.RequestSceneChange));
                e.Handled = true;
                return;
            }
        }

        private void HandleDebugLevelSwitch(KeyEventArgs e)
        {
            if (!ConfigManager.Config.DebugMode) return;

            IScene? targetScene = e.Key switch
            {
                Key.D1 => new Level1Scene(_sceneManager.RequestSceneChange),
                Key.D2 => new Level2Scene(_sceneManager.RequestSceneChange),
                Key.D3 => new Level3Scene(_sceneManager.RequestSceneChange),
                Key.D4 => new Level4Scene(_sceneManager.RequestSceneChange),
                _ => null
            };

            if (targetScene != null)
            {
                _sceneManager.ChangeScene(targetScene);
                e.Handled = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            UnsubscribeFromEvents();
            ConfigManager.ConfigSaved -= OnConfigSaved;
            ConfigManager.MenuFontSizeChanged -= OnMenuFontSizeChanged;
            _timer.Stop();
            _timer.Tick -= GameTimer_Tick;
            _assets?.Dispose();
            _shader?.Dispose();
            _textRenderer?.Dispose();
            base.OnClosed(e);
        }
    }
}