using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Patterns;
using TankGL_fbo.Core.Patterns.Decorators;

namespace TankGL_fbo.Core.Systems;

/// <summary>
/// Система обработки и разрешения столкновений между игровыми сущностями.
/// Отвечает за проверку пересечений танков со стенами и другими танками,
/// обработку попаданий снарядов, подбор бонусов и применение их эффектов.
/// </summary>
public sealed class CollisionSystem
{
    /// <summary>Список танков на уровне.</summary>
    private readonly List<Tank> _tanks;
    /// <summary>Список препятствий (стен).</summary>
    private readonly List<Wall> _walls;
    /// <summary>Список активных бонусов.</summary>
    private readonly List<Bonus> _bonuses;
    /// <summary>Список активных снарядов.</summary>
    private readonly List<Bullet> _bullets;

    /// <summary>
    /// Инициализирует новый экземпляр системы столкновений.
    /// </summary>
    /// <param name="tanks">Коллекция танков для проверки коллизий.</param>
    /// <param name="walls">Коллекция стен на уровне.</param>
    /// <param name="bonuses">Коллекция появившихся бонусов.</param>
    /// <param name="bullets">Коллекция выпущенных снарядов.</param>
    public CollisionSystem(List<Tank> tanks, List<Wall> walls, List<Bonus> bonuses, List<Bullet> bullets)
    {
        _tanks = tanks;
        _walls = walls;
        _bonuses = bonuses;
        _bullets = bullets;
    }

    /// <summary>
    /// Выполняет полный цикл проверки и разрешения столкновений для всех сущностей.
    /// Включает: ограничение движения танков стенами и другими танками,
    /// подбор бонусов, уничтожение пуль о стены и нанесение урона танкам.
    /// </summary>
    public void Resolve()
    {
        // Разрешение коллизий движения танков (по осям X и Y раздельно)
        foreach (var tank in _tanks)
        {
            if (tank.IsDestroyed) continue;
            Vector2 currentPos = tank.Position;
            Vector2 prevPos = tank.PreviousPosition;
            Vector2 delta = currentPos - prevPos;

            // Проверка по оси X
            Vector2 posAfterX = new Vector2(prevPos.X + delta.X, prevPos.Y);
            var boundsAfterX = new RectAABB(posAfterX, tank.Bounds.HalfSize);
            bool hitX = false;
            foreach (var wall in _walls)
                if (boundsAfterX.Intersects(wall.Bounds)) { hitX = true; break; }
            foreach (var otherTank in _tanks)
            {
                if (otherTank.IsDestroyed || otherTank == tank) continue;
                if (boundsAfterX.Intersects(otherTank.Bounds)) { hitX = true; break; }
            }
            Vector2 resolvedPos = hitX ? new Vector2(prevPos.X, prevPos.Y) : posAfterX;

            // Проверка по оси Y
            Vector2 posAfterY = new Vector2(resolvedPos.X, prevPos.Y + delta.Y);
            var boundsAfterY = new RectAABB(posAfterY, tank.Bounds.HalfSize);
            bool hitY = false;
            foreach (var wall in _walls)
                if (boundsAfterY.Intersects(wall.Bounds)) { hitY = true; break; }
            foreach (var otherTank in _tanks)
            {
                if (otherTank.IsDestroyed || otherTank == tank) continue;
                if (boundsAfterY.Intersects(otherTank.Bounds)) { hitY = true; break; }
            }
            resolvedPos = hitY ? new Vector2(resolvedPos.X, prevPos.Y) : posAfterY;
            tank.Position = resolvedPos;

            // === НОВОЕ: Замедление танка при столкновении ===
            if (hitX || hitY)
            {
                tank.ApplyCollisionSlowdown();
            }
        }

        // Обработка подбора бонусов
        for (int i = _bonuses.Count - 1; i >= 0; i--)
        {
            var bonus = _bonuses[i];
            foreach (var tank in _tanks)
            {
                if (tank.IsDestroyed) continue;
                if (bonus.Bounds.Intersects(tank.Bounds))
                {
                    ApplyBonus(tank, bonus.Type);
                    bonus.Lifetime = 0;
                    break;
                }
            }
        }

        // Обработка столкновений пуль
        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            var bullet = _bullets[i];
            if (bullet.IsExpired) continue;

            // Столкновение со стенами
            foreach (var wall in _walls)
            {
                if (bullet.Bounds.Intersects(wall.Bounds))
                {
                    bullet.Lifetime = 0;
                    break;
                }
            }
            if (bullet.IsExpired) continue;

            // Столкновение с танками (исключая владельца)
            for (int j = 0; j < _tanks.Count; j++)
            {
                var tank = _tanks[j];
                if (tank.IsDestroyed || j == bullet.OwnerId) continue;
                if (bullet.Bounds.Intersects(tank.Bounds))
                {
                    tank.TakeDamage(bullet.Damage);
                    bullet.Lifetime = 0;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Применяет эффект указанного типа бонуса к танку.
    /// Использует паттерн "Декоратор" для временного изменения характеристик
    /// или напрямую пополняет ресурсы (патроны, топливо).
    /// </summary>
    /// <param name="tank">Танк, подбирающий бонус.</param>
    /// <param name="type">Тип подобранного бонуса.</param>
    public void ApplyBonus(Tank tank, BonusType type)
    {
        switch (type)
        {
            case BonusType.SpeedUp:
                {
                    var existing = StatDecorator.FindInChain<SpeedDecorator>(tank.Stats);
                    if (existing != null) existing.Refresh(10f);
                    else tank.Stats = new SpeedDecorator(tank.Stats, 1.5f, 10f);
                    break;
                }
            case BonusType.Shield:
                {
                    var existing = StatDecorator.FindInChain<ArmorDecorator>(tank.Stats);
                    if (existing != null) existing.Refresh(8f);
                    else tank.Stats = new ArmorDecorator(tank.Stats, 20f, 8f);
                    break;
                }
            case BonusType.DamageBoost:
                {
                    var existing = StatDecorator.FindInChain<DamageDecorator>(tank.Stats);
                    if (existing != null) existing.Refresh(7f);
                    else tank.Stats = new DamageDecorator(tank.Stats, 1.8f, 7f);
                    break;
                }
            case BonusType.AmmoRefill:
                tank.Stats.Ammo = Math.Min(tank.Stats.Ammo + 15, 40);
                break;
            case BonusType.FuelCan:
                tank.Stats.Fuel = Math.Min(tank.Stats.Fuel + 40f, 100f);
                break;
            case BonusType.SpeedDown:
                {
                    var existing = StatDecorator.FindInChain<SpeedDecorator>(tank.Stats);
                    if (existing != null) existing.Refresh(8f);
                    else tank.Stats = new SpeedDecorator(tank.Stats, 0.5f, 8f);
                    break;
                }
            case BonusType.ArmorBreak:
                {
                    var existing = StatDecorator.FindInChain<ArmorDecorator>(tank.Stats);
                    if (existing != null) existing.Refresh(10f);
                    else tank.Stats = new ArmorDecorator(tank.Stats, -20f, 10f);
                    break;
                }
            case BonusType.DamageDown:
                {
                    var existing = StatDecorator.FindInChain<DamageDecorator>(tank.Stats);
                    if (existing != null) existing.Refresh(9f);
                    else tank.Stats = new DamageDecorator(tank.Stats, 0.4f, 9f);
                    break;
                }
        }
    }
}