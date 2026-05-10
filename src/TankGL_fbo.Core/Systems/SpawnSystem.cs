namespace TankGL_fbo.Core.Systems;

using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Patterns;

/// <summary>
/// Система спавна бонусов на игровом поле.
/// Отвечает за периодическое появление случайных бонусов в свободных от стен и танков местах.
/// </summary>
public sealed class SpawnSystem
{
    /// <summary>Список активных бонусов для добавления новых и очистки просроченных.</summary>
    private readonly List<Bonus> _bonuses;
    /// <summary>Список стен для проверки занятости позиции.</summary>
    private readonly List<Wall> _walls;
    /// <summary>Список танков для проверки занятости позиции.</summary>
    private readonly List<Tank> _tanks;
    /// <summary>Генератор случайных чисел для выбора типа и координат бонуса.</summary>
    private readonly Random _rng = new();
    /// <summary>Таймер до следующего спавна.</summary>
    private float _spawnTimer = 0f;

    /// <summary>Границы области спавна по оси X.</summary>
    private const float MinX = -380f, MaxX = 380f;
    /// <summary>Границы области спавна по оси Y.</summary>
    private const float MinY = -280f, MaxY = 280f;

    /// <summary>
    /// Инициализирует новый экземпляр системы спавна.
    /// </summary>
    /// <param name="bonuses">Коллекция бонусов для управления.</param>
    /// <param name="walls">Коллекция стен для проверки коллизий при спавне.</param>
    /// <param name="tanks">Коллекция танков для проверки коллизий при спавне.</param>
    public SpawnSystem(List<Bonus> bonuses, List<Wall> walls, List<Tank> tanks)
    {
        _bonuses = bonuses;
        _walls = walls;
        _tanks = tanks;
        _spawnTimer = GetRandomInterval();
    }

    /// <summary>
    /// Обновляет состояние системы: уменьшает таймер спавна, обновляет существующие бонусы
    /// и создает новый бонус, если таймер истек и найдено свободное место.
    /// </summary>
    /// <param name="deltaTime">Время, прошедшее с последнего кадра.</param>
    public void Update(float deltaTime)
    {
        foreach (var bonus in _bonuses) bonus.Update(deltaTime);
        _bonuses.RemoveAll(b => b.IsExpired);

        _spawnTimer -= deltaTime;
        if (_spawnTimer <= 0f)
        {
            TrySpawnBonus();
            _spawnTimer = GetRandomInterval();
        }
    }

    /// <summary>
    /// Пытается создать и добавить новый бонус в случайную свободную позицию.
    /// Выполняет до 50 попыток поиска места, не пересекающегося со стенами и танками.
    /// </summary>
    private void TrySpawnBonus()
    {
        BonusType type = (BonusType)_rng.Next(0, Enum.GetValues<BonusType>().Length);
        Vector2 pos;
        int attempts = 0;
        do
        {
            pos = new Vector2(_rng.NextSingle() * (MaxX - MinX) + MinX, _rng.NextSingle() * (MaxY - MinY) + MinY);
            attempts++;
        } while (!IsPositionFree(pos) && attempts < 50);

        if (attempts < 50)
        {
            _bonuses.Add(BonusFactory.Create(type, pos));
        }
    }

    /// <summary>
    /// Проверяет, свободна ли указанная позиция для спавна бонуса.
    /// Позиция считается занятой, если пересекается со стеной или живым танком.
    /// </summary>
    /// <param name="pos">Проверяемые координаты.</param>
    /// <returns>True, если позиция свободна; иначе False.</returns>
    private bool IsPositionFree(Vector2 pos)
    {
        var bonusBounds = new RectAABB(pos, new Vector2(16f, 16f));
        foreach (var wall in _walls) if (bonusBounds.Intersects(wall.Bounds)) return false;
        foreach (var tank in _tanks) if (!tank.IsDestroyed && bonusBounds.Intersects(tank.Bounds)) return false;
        return true;
    }

    /// <summary>
    /// Генерирует случайный интервал времени до следующего спавна.
    /// </summary>
    /// <returns>Случайное значение от 4 до 10 секунд.</returns>
    private float GetRandomInterval() => 4f + _rng.NextSingle() * 6f;
}