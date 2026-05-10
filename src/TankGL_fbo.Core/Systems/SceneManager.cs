using System;
using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;

namespace TankGL_fbo.Core.Systems;

/// <summary>
/// Менеджер сцен, управляющий жизненным циклом, переходами и обновлениями игровых сцен.
/// Поддерживает плавные переходы с настраиваемой задержкой.
/// </summary>
public class SceneManager
{
    /// <summary>Текущая активная сцена.</summary>
    private IScene? _currentScene;
    /// <summary>Сцена, ожидающая переключения.</summary>
    private IScene? _pendingScene;
    /// <summary>Таймер обратного отсчета для перехода.</summary>
    private float _transitionTimer;
    /// <summary>Общая длительность задержки перехода.</summary>
    private float _transitionDelay;
    /// <summary>Флаг активного процесса перехода между сценами.</summary>
    private bool _isTransitioning;

    /// <summary>Событие, вызываемое после успешного переключения на новую сцену.</summary>
    public event EventHandler<IScene>? SceneChanged;
    /// <summary>Событие-запрос на смену сцены (используется сценами для навигации).</summary>
    public event EventHandler<IScene>? SceneChangeRequested;

    /// <summary>Возвращает текущую активную сцену.</summary>
    public IScene? CurrentScene => _currentScene;
    /// <summary>Указывает, находится ли менеджер в процессе перехода.</summary>
    public bool IsTransitioning => _isTransitioning;
    /// <summary>Прогресс перехода от 0.0 до 1.0.</summary>
    public float TransitionProgress => _transitionDelay > 0 ? 1.0f - (_transitionTimer / _transitionDelay) : 1.0f;

    /// <summary>
    /// Инициирует смену сцены. Может быть выполнена мгновенно или с задержкой.
    /// </summary>
    /// <param name="newScene">Новая сцена для активации.</param>
    /// <param name="delay">Время задержки перед переключением (в секундах). 0 для мгновенного перехода.</param>
    public void ChangeScene(IScene newScene, float delay = 0f)
    {
        _pendingScene = newScene;
        if (delay > 0f)
        {
            _transitionDelay = delay;
            _transitionTimer = delay;
            _isTransitioning = true;
        }
        else
        {
            _transitionDelay = 0f;
            _transitionTimer = 0f;
            _isTransitioning = false;
        }
    }

    /// <summary>
    /// Отправляет запрос на смену сцены через событие <see cref="SceneChangeRequested"/>.
    /// </summary>
    /// <param name="scene">Целевая сцена.</param>
    public void RequestSceneChange(IScene scene)
    {
        SceneChangeRequested?.Invoke(this, scene);
    }

    /// <summary>
    /// Обновляет состояние менеджера сцен и текущей сцены.
    /// Обрабатывает таймеры переходов и делегирует обновление активной сцене.
    /// </summary>
    /// <param name="deltaTime">Время, прошедшее с последнего кадра.</param>
    /// <param name="inputs">Словарь активных действий игроков.</param>
    public void Update(float deltaTime, Dictionary<int, HashSet<PlayerAction>> inputs)
    {
        if (_isTransitioning)
        {
            _transitionTimer -= deltaTime;
            _currentScene?.Update(deltaTime, inputs);
            if (_transitionTimer <= 0f)
            {
                _isTransitioning = false;
                PerformSceneSwitch();
            }
            return;
        }

        if (_pendingScene != null)
        {
            PerformSceneSwitch();
        }

        _currentScene?.Update(deltaTime, inputs);
    }

    /// <summary>
    /// Выполняет непосредственное переключение сцен: вызывает OnExit у старой,
    /// заменяет ссылку, вызывает OnEnter у новой и уведомляет подписчиков.
    /// </summary>
    private void PerformSceneSwitch()
    {
        if (_pendingScene == null) return;
        _currentScene?.OnExit();
        _currentScene = _pendingScene;
        _pendingScene = null;
        _currentScene.OnEnter();
        SceneChanged?.Invoke(this, _currentScene);
    }

    /// <summary>
    /// Собирает все отрисовываемые объекты из текущей сцены в указанный список.
    /// </summary>
    /// <param name="renderables">Список для добавления объектов рендеринга.</param>
    public void CollectRenderables(List<IRenderable> renderables)
    {
        _currentScene?.CollectRenderables(renderables);
    }
}