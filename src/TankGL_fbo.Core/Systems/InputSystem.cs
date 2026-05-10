using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;

namespace TankGL_fbo.Core.Systems;

/// <summary>
/// Система обработки ввода игроков.
/// Преобразует нажатия клавиш в действия танков: поворот, движение и стрельба.
/// </summary>
public sealed class InputSystem
{
    /// <summary>Список танков, привязанных к индексам игроков.</summary>
    private readonly List<Tank> _tanks;
    /// <summary>Список снарядов для добавления новых выстрелов.</summary>
    private readonly List<Bullet> _bullets;

    /// <summary>Скорость поворота танка (радиан в секунду).</summary>
    private const float RotateSpeed = 2.5f;
    /// <summary>Смещение точки спавна пули относительно центра танка.</summary>
    private const float BulletSpawnOffset = 22f;

    /// <summary>
    /// Инициализирует новый экземпляр системы ввода.
    /// </summary>
    /// <param name="tanks">Коллекция танков для управления.</param>
    /// <param name="bullets">Коллекция снарядов для регистрации выстрелов.</param>
    public InputSystem(List<Tank> tanks, List<Bullet> bullets)
    {
        _tanks = tanks;
        _bullets = bullets;
    }

    /// <summary>
    /// Обрабатывает активные действия игроков за текущий кадр.
    /// Обновляет поворот, перемещение и инициирует стрельбу, если это возможно.
    /// </summary>
    /// <param name="activeInputs">Словарь активных действий для каждого игрока.</param>
    /// <param name="deltaTime">Время, прошедшее с последнего кадра.</param>
    public void Process(Dictionary<int, HashSet<PlayerAction>> activeInputs, float deltaTime)
    {
        for (int i = 0; i < _tanks.Count; i++)
        {
            var tank = _tanks[i];
            if (tank.IsDestroyed) continue;
            if (!activeInputs.TryGetValue(i, out var actions)) continue;

            // Обработка поворота
            if (actions.Contains(PlayerAction.RotateLeft)) tank.Rotate(RotateSpeed * deltaTime);
            if (actions.Contains(PlayerAction.RotateRight)) tank.Rotate(-RotateSpeed * deltaTime);

            // Расчет направления движения
            Vector2 moveDir = Vector2.Zero;
            float cos = MathF.Cos(tank.Rotation);
            float sin = MathF.Sin(tank.Rotation);
            if (actions.Contains(PlayerAction.MoveUp)) moveDir += new Vector2(cos, sin);
            if (actions.Contains(PlayerAction.MoveDown)) moveDir -= new Vector2(cos, sin);

            // Применение движения
            if (moveDir.Length() > 0f) tank.Move(moveDir, deltaTime);

            // Обработка стрельбы
            if (actions.Contains(PlayerAction.Fire) && tank.TryFire())
            {
                Vector2 spawnPos = tank.Position + new Vector2(cos, sin) * BulletSpawnOffset;
                _bullets.Add(new Bullet(spawnPos, tank.Rotation, 450f, tank.Stats.Damage, 3.0f, i));
            }
        }
    }
}