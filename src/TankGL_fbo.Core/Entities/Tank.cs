using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns.Decorators;

namespace TankGL_fbo.Core.Entities;

public sealed class Tank : IUpdatable, IRenderable
{
    public Vector2 Position { get; internal set; }
    public Vector2 PreviousPosition { get; internal set; }
    public float Rotation { get; private set; }
    public float Scale => 1.0f;
    public int ZIndex => 10;
    public string TexturePath { get; }

    public ICombatStats Stats { get; internal set; }
    public float HP { get; private set; }
    public float CooldownTimer { get; private set; }

    public RectAABB Bounds => new(Position, new Vector2(24f, 24f));
    public bool IsDestroyed => HP <= 0;

    private const float MaxHP = 100f;
    private const float FireCooldown = 0.5f;
    private float _collisionSlowdownTimer = 0f;

    public Tank(Vector2 startPos, string texturePath, ICombatStats stats)
    {
        Position = startPos;
        PreviousPosition = startPos;
        TexturePath = texturePath;
        Stats = stats;
        HP = MaxHP;
        CooldownTimer = 0.5f;
        Rotation = (float)(new Random().NextDouble() * 2 * MathF.PI);
    }

    public void Update(float deltaTime)
    {
        PreviousPosition = Position;

        if (Stats is IUpdatable updatable)
        {
            updatable.Update(deltaTime);
        }

        while (Stats is Patterns.Decorators.StatDecorator dec && dec.IsExpired)
        {
            Stats = dec.Wrapped;
        }
        if (CooldownTimer > 0) CooldownTimer -= deltaTime;

        if (_collisionSlowdownTimer > 0)
            _collisionSlowdownTimer -= deltaTime;
    }

    public void Move(Vector2 direction, float deltaTime)
    {
        if (Stats.Fuel <= 0 || IsDestroyed) return;

        float currentSpeed = Stats.Speed;
        if (_collisionSlowdownTimer > 0)
            currentSpeed *= 0.35f;

        Vector2 displacement = direction.Normalized() * currentSpeed * deltaTime;
        Position += displacement;
        Stats.Fuel -= MathF.Abs(displacement.Length()) * 0.01f;
        if (Stats.Fuel < 0) Stats.Fuel = 0;
    }

    public void Rotate(float angleDelta) => Rotation += angleDelta;

    public void TakeDamage(float incomingDamage)
    {
        float netDamage = incomingDamage - Stats.Armor;
        if (netDamage > 0) HP -= netDamage;
        if (HP < 0) HP = 0;
    }

    public bool TryFire()
    {
        if (CooldownTimer > 0 || Stats.Ammo <= 0 || IsDestroyed) return false;
        CooldownTimer = FireCooldown;
        Stats.Ammo--;
        return true;
    }

    /// <summary>
    /// Активирует замедление скорости на 50% на 1 секунду.
    /// Вызывается системой коллизий при ударе о стену или другой танк.
    /// </summary>
    public void ApplyCollisionSlowdown()
    {
        _collisionSlowdownTimer = 0.05f; // Длительность замедления
    }
}