using System;
using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Systems;

namespace TankGL_fbo.Core.Scenes;

/// <summary>
/// Базовый абстрактный класс для всех сцен меню.
/// Реализует общую логику навигации по пунктам меню, обработку ввода,
/// управление задержкой между нажатиями и отрисовку фона.
/// </summary>
public abstract class MenuSceneBase : IScene
{
    /// <summary>Делегат для запроса смены сцены у менеджера сцен.</summary>
    protected readonly Action<IScene>? RequestSceneChange;

    private int _selectedIndex;
    /// <summary>
    /// Индекс текущего выбранного пункта меню.
    /// При изменении автоматически вызывает событие обновления состояния меню.
    /// </summary>
    protected int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                RaiseMenuStateChanged();
            }
        }
    }

    /// <summary>Текущее время задержки между обработкой нажатий клавиш.</summary>
    protected float InputCooldown { get; set; }
    /// <summary>Базовое время задержки (в секундах) для предотвращения двойных нажатий.</summary>
    protected const float CooldownTime = 0.2f;

    /// <summary>Список фоновых элементов для отрисовки в меню.</summary>
    private readonly List<Background> _backgrounds = [];

    /// <summary>Событие, уведомляющее представление об изменении пунктов меню или выбранного индекса.</summary>
    public event EventHandler<(string[] items, int selectedIndex)>? MenuStateChanged;

    /// <summary>
    /// Принудительно запрашивает обновление состояния меню у подписчиков.
    /// </summary>
    public void RequestMenuStateUpdate()
    {
        RaiseMenuStateChanged();
    }

    /// <summary>
    /// Вызывает событие <see cref="MenuStateChanged"/> с текущими данными меню.
    /// </summary>
    protected void RaiseMenuStateChanged()
    {
        MenuStateChanged?.Invoke(this, GetMenuState());
    }

    /// <summary>
    /// Инициализирует новый экземпляр базового класса меню.
    /// </summary>
    /// <param name="requestSceneChange">Делегат для запроса перехода на другую сцену.</param>
    protected MenuSceneBase(Action<IScene>? requestSceneChange = null)
    {
        RequestSceneChange = requestSceneChange;
    }

    /// <summary>
    /// Возвращает массив строк, представляющих пункты текущего меню.
    /// Должен быть реализован в наследниках.
    /// </summary>
    /// <returns>Массив названий пунктов меню.</returns>
    protected abstract string[] GetMenuItems();

    /// <summary>
    /// Обрабатывает выбор пункта меню по указанному индексу.
    /// Должен быть реализован в наследниках.
    /// </summary>
    /// <param name="index">Индекс выбранного пункта.</param>
    protected abstract void OnItemSelected(int index);

    /// <summary>
    /// Создает фоновый объект для сцены меню. Может быть переопределен в наследниках.
    /// </summary>
    /// <returns>Экземпляр фона.</returns>
    protected virtual Background CreateBackground() => new Background(new Vector2(0, 0), new Vector2(640, 360), "tile.png");

    /// <summary>
    /// Вызывается при переходе в меню. Сбрасывает состояние сессии, инициализирует фон и выбирает первый пункт.
    /// </summary>
    public virtual void OnEnter()
    {
        SessionState.Reset();
        _backgrounds.Add(CreateBackground());
        SelectedIndex = 0;
        InputCooldown = 0.5f;
    }

    /// <summary>
    /// Вызывается при покидании меню. Очищает список фоновых элементов.
    /// </summary>
    public virtual void OnExit()
    {
        _backgrounds.Clear();
    }

    /// <summary>
    /// Обновляет логику меню: обрабатывает навигацию, выбор пунктов и задержку ввода.
    /// </summary>
    /// <param name="deltaTime">Время, прошедшее с последнего кадра.</param>
    /// <param name="inputs">Словарь активных действий игроков (обрабатывается только игрок 0).</param>
    public virtual void Update(float deltaTime, Dictionary<int, HashSet<PlayerAction>> inputs)
    {
        InputCooldown -= deltaTime;
        if (InputCooldown < 0) InputCooldown = 0;

        if (inputs.TryGetValue(0, out var actions))
        {
            var items = GetMenuItems();
            if (actions.Contains(PlayerAction.MoveUp) && InputCooldown <= 0)
            {
                SelectedIndex = (SelectedIndex - 1 + items.Length) % items.Length;
                InputCooldown = CooldownTime;
            }
            if (actions.Contains(PlayerAction.MoveDown) && InputCooldown <= 0)
            {
                SelectedIndex = (SelectedIndex + 1) % items.Length;
                InputCooldown = CooldownTime;
            }
            if (actions.Contains(PlayerAction.Confirm) && InputCooldown <= 0)
            {
                OnItemSelected(SelectedIndex);
                InputCooldown = CooldownTime;
            }
        }
    }

    /// <summary>
    /// Добавляет фоновые элементы в список объектов для отрисовки.
    /// </summary>
    /// <param name="renderables">Список для добавления отрисовываемых сущностей.</param>
    public virtual void CollectRenderables(List<IRenderable> renderables)
    {
        renderables.AddRange(_backgrounds);
    }

    /// <summary>
    /// Возвращает текущее состояние меню в виде кортежа.
    /// </summary>
    /// <returns>Кортеж, содержащий массив пунктов меню и индекс выбранного элемента.</returns>
    public (string[] items, int selectedIndex) GetMenuState() => (GetMenuItems(), SelectedIndex);
}