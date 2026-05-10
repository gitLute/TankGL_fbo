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
    /// <summary>
    /// Главное окно приложения WPF.
    /// Инициализирует контекст OpenGL, управляет игровым циклом, обрабатывает пользовательский ввод,
    /// подписывается на события сцен и отрисовывает игровой мир вместе с интерфейсом (HUD/Меню).
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>Менеджер сцен, управляющий переходами и обновлениями игровых состояний.</summary>
        private SceneManager _sceneManager = null!;
        /// <summary>Менеджер ресурсов для загрузки и кэширования шейдеров и текстур.</summary>
        private AssetManager _assets = null!;
        /// <summary>Активная шейдерная программа для рендеринга спрайтов.</summary>
        private Shader _shader = null!;
        /// <summary>Элемент управления OpenGL, встроенный в WPF через WindowsFormsHost.</summary>
        private GLControl _glControl = null!;
        /// <summary>Рендерер текста для отрисовки HUD, меню и отладочной информации.</summary>
        private OpenGlTextRenderer? _textRenderer;
        /// <summary>Флаг успешной инициализации графической подсистемы и ресурсов.</summary>
        private bool _isInitialized;
        /// <summary>Матрица ортографической проекции для текущих виртуальных размеров экрана.</summary>
        private Matrix4 _projection;
        /// <summary>Секундомер для точного измерения времени между кадрами (delta time).</summary>
        private readonly Stopwatch _stopwatch = new();
        /// <summary>Таймер диспетчера WPF, запускающий тики игрового цикла.</summary>
        private readonly DispatcherTimer _timer = new();
        /// <summary>Очередь объектов, подлежащих отрисовке в текущем кадре.</summary>
        private readonly List<IRenderable> _renderQueue = new();
        /// <summary>Словарь активных действий игроков, обновляемый при вводе с клавиатуры.</summary>
        private readonly Dictionary<int, HashSet<PlayerAction>> _activeInputs = new() { [0] = new(), [1] = new() };
        /// <summary>Карта соответствия клавиш WPF действиям конкретных игроков.</summary>
        private readonly Dictionary<Key, (int playerId, PlayerAction action)> _keyMap = new();

        /// <summary>Строка статистики первого игрока для отображения в HUD.</summary>
        private string _hudPlayer1Stats = string.Empty;
        /// <summary>Строка статистики второго игрока для отображения в HUD.</summary>
        private string _hudPlayer2Stats = string.Empty;
        /// <summary>Массив пунктов текущего меню.</summary>
        private string[] _menuItems = Array.Empty<string>();
        /// <summary>Индекс выбранного пункта меню.</summary>
        private int _menuSelectedIndex;

        /// <summary>Перечисление состояний текущей сцены для корректной логики отрисовки интерфейса.</summary>
        private enum SceneState { Menu, Info, Level, Options, Other }
        /// <summary>Текущее состояние сцены, определяющее способ отрисовки HUD.</summary>
        private SceneState _currentSceneState = SceneState.Other;

        /// <summary>Виртуальная ширина экрана, полученная из глобальной конфигурации.</summary>
        private float VirtualWidth => ConfigManager.Config.ResolutionWidth;
        /// <summary>Виртуальная высота экрана, полученная из глобальной конфигурации.</summary>
        private float VirtualHeight => ConfigManager.Config.ResolutionHeight;

        /// <summary>Координаты и размеры рассчитанного вьюпорта для сохранения пропорций (Letterboxing).</summary>
        private int _vpX, _vpY, _vpW, _vpH;
        /// <summary>Кэшированный размер шрифта меню.</summary>
        private int _cachedMenuFontSize = 24;
        /// <summary>Кэшированный размер шрифта статистики.</summary>
        private int _cachedStatsFontSize = 16;

        /// <summary>
        /// Инициализирует компоненты окна и настраивает карту ввода.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            SetupInputMap();
        }

        /// <summary>
        /// Обработчик события загрузки окна. Запускает последовательную инициализацию подсистем.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeOpenGL();
            InitializeGameSystems();
            SubscribeToEvents();
            StartGameLoop();
        }

        /// <summary>
        /// Создает и настраивает элемент управления GLControl с профилем совместимости OpenGL 3.3.
        /// Внедряет его в WindowsFormsHost и подписывается на события жизненного цикла GL.
        /// </summary>
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

        /// <summary>
        /// Загружает конфигурацию, инициализирует менеджер ресурсов, шейдеры, рендерер текста и менеджер сцен.
        /// Устанавливает начальную сцену (InfoScene) и настраивает состояние OpenGL.
        /// </summary>
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

        /// <summary>
        /// Обновляет кэшированные размеры шрифтов при изменении настроек в меню опций.
        /// </summary>
        /// <param name="newSize">Новый размер шрифта меню.</param>
        private void OnMenuFontSizeChanged(int newSize)
        {
            _cachedMenuFontSize = newSize;
            _cachedStatsFontSize = newSize * 2 / 3;
        }

        /// <summary>
        /// Пересчитывает параметры вьюпорта при сохранении новой конфигурации (например, смене разрешения).
        /// </summary>
        private void OnConfigSaved()
        {
            if (!_isInitialized) return;
            Dispatcher.Invoke(() => GlControl_Resize(_glControl, EventArgs.Empty));
        }

        /// <summary>
        /// Запускает секундомер и таймер диспетчера для начала выполнения игрового цикла.
        /// </summary>
        private void StartGameLoop()
        {
            _stopwatch.Start();
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += GameTimer_Tick;
            _timer.Start();
        }

        /// <summary>
        /// Подписывается на глобальные события менеджера сцен и события текущей активной сцены.
        /// </summary>
        private void SubscribeToEvents()
        {
            _sceneManager.SceneChanged += OnSceneChanged;
            _sceneManager.SceneChangeRequested += OnSceneChangeRequested;
            SubscribeToCurrentSceneEvents();
        }

        /// <summary>
        /// Подписывается на специфичные события текущей сцены (обновление HUD уровня или состояния меню).
        /// </summary>
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

        /// <summary>
        /// Отписывается от глобальных событий менеджера сцен для предотвращения утечек памяти.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (_sceneManager != null)
            {
                _sceneManager.SceneChanged -= OnSceneChanged;
                _sceneManager.SceneChangeRequested -= OnSceneChangeRequested;
            }
            UnsubscribeFromCurrentSceneEvents();
        }

        /// <summary>
        /// Отписывается от событий текущей сцены при её смене или закрытии окна.
        /// </summary>
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

        /// <summary>
        /// Обработчик события смены сцены. Обновляет внутреннее состояние интерфейса,
        /// переподписывается на события новой сцены и очищает словарь активного ввода.
        /// </summary>
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

        /// <summary>
        /// Обработчик запроса на смену сцены от игровых систем. Запускает переход с задержкой 0.3с.
        /// </summary>
        private void OnSceneChangeRequested(object? sender, IScene requestedScene)
        {
            _sceneManager.ChangeScene(requestedScene, 0.3f);
        }

        /// <summary>
        /// Обновляет строки статистики игроков при получении данных от текущего уровня.
        /// </summary>
        private void OnHudDataUpdated(object? sender, (string p1Stats, string p2Stats) hudData)
        {
            _hudPlayer1Stats = hudData.p1Stats;
            _hudPlayer2Stats = hudData.p2Stats;
        }

        /// <summary>
        /// Обновляет пункты меню и выбранный индекс при изменении состояния меню.
        /// </summary>
        private void OnMenuStateChanged(object? sender, (string[] items, int selectedIndex) menuState)
        {
            _menuItems = menuState.items;
            _menuSelectedIndex = menuState.selectedIndex;
        }

        /// <summary>
        /// Основной тик игрового цикла. Вычисляет delta time, ограничивает его,
        /// обновляет менеджер сцен и инициирует перерисовку GLControl.
        /// </summary>
        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isInitialized) return;
            float dt = (float)_stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();
            _sceneManager.Update(Math.Min(dt, 0.1f), _activeInputs);
            _glControl.Invalidate();
        }

        /// <summary>Заглушка для события загрузки GLControl.</summary>
        private void GlControl_Load(object? sender, EventArgs e) { }

        /// <summary>
        /// Пересчитывает матрицу проекции и параметры вьюпорта при изменении размера окна.
        /// Реализует логику Letterboxing для сохранения виртуальных пропорций экрана.
        /// </summary>
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

        /// <summary>
        /// Основной метод отрисовки кадра. Очищает буферы, рендерит игровую сцену,
        /// отладочные границы коллизий и интерфейс (HUD), затем меняет буферы.
        /// </summary>
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

        /// <summary>
        /// Отрисовывает все объекты текущей сцены с применением шейдеров, текстур,
        /// UV-масштабирования и матриц трансформации (масштаб, поворот, перемещение).
        /// </summary>
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

        /// <summary>
        /// Отрисовывает отладочные рамки (wireframe) коллизий для всех объектов сцены,
        /// если включен соответствующий режим в конфигурации.
        /// </summary>
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

        /// <summary>
        /// Отрисовывает интерфейс пользователя (HUD, меню, настройки, справку)
        /// в зависимости от текущего состояния сцены с использованием текстурного шрифта.
        /// </summary>
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
            string debugMessage = string.Empty;
            switch (_currentSceneState)
            {
                case SceneState.Level:
                    if (ConfigManager.Config.ShowColliderBounds) debugMessage += "COLLIDERS ENABLED\n";
                    if (ConfigManager.Config.DebugMode) debugMessage += "DEBUG SHORTCUTS ENABLED\n";
                    if (!string.IsNullOrEmpty(_hudPlayer1Stats))
                        _textRenderer.DrawText(_hudPlayer1Stats, statsX_PL1, statsY, statsFontSize, width, height);
                    if (!string.IsNullOrEmpty(_hudPlayer2Stats))
                        _textRenderer.DrawText(_hudPlayer2Stats, statsX_PL2, statsY, statsFontSize, width, height);
                    if (!string.IsNullOrEmpty(_hudPlayer2Stats))
                        _textRenderer.DrawText(debugMessage, 0, 0, statsFontSize, width, height, Color.Coral);
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
                    float infoY = (int)(height / 10);
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

        /// <summary>
        /// Инициализирует словарь соответствия клавиш клавиатуры действиям игроков и навигации по меню.
        /// </summary>
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

        /// <summary>
        /// Обрабатывает нажатия клавиш: регистрирует действия игроков, проверяет глобальные
        /// и отладочные сочетания клавиш. Помечает событие как обработанное.
        /// </summary>
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

        /// <summary>
        /// Обрабатывает отпускания клавиш: удаляет соответствующие действия из словаря активного ввода.
        /// </summary>
        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (_keyMap.TryGetValue(e.Key, out var map))
            {
                _activeInputs[map.playerId].Remove(map.action);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обрабатывает глобальные горячие клавиши, доступные в любом состоянии приложения.
        /// F12 - возврат в главное меню, F1 - экран справки.
        /// </summary>
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
            if (e.Key == Key.Oem3)
            {
                _sceneManager.ChangeScene(new OptionsScene(_sceneManager.RequestSceneChange));
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// Обрабатывает переключение уровней по клавишам D1-D4.
        /// Работает только при включенном отладочном режиме в конфигурации.
        /// </summary>
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

        /// <summary>
        /// Освобождает неуправляемые ресурсы OpenGL, отписывается от всех событий,
        /// останавливает таймеры и вызывает базовую реализацию закрытия окна.
        /// </summary>
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