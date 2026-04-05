namespace TankGL_fbo.Core.Entities;

using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns.Decorators;

public sealed class Tank : IUpdatable, IRenderable
{
    public Vector2 Position { get; private set; }
    public float Rotation { get; private set; }
    public float Scale => 1.0f;
    public int ZIndex => 10;
    public string TexturePath { get; }

    public ICombatStats Stats { get; private set; }
    public float HP { get; private set; }
    public float CooldownTimer { get; private set; }


    public RectAABB Bounds => new(Position, new Vector2(18f, 24f));
    public bool IsDestroyed => HP <= 0;

    private const float MaxHP = 100f;
    private const float FireCooldown = 0.5f;

    public Tank(Vector2 startPos, string texturePath, ICombatStats stats)
    {
        Position = startPos;
        TexturePath = texturePath;
        Stats = stats;
        HP = MaxHP;
        CooldownTimer = 0f;
    }

    public void Update(float deltaTime)
    {
        if (Stats is IUpdatable updatable) updatable.Update(deltaTime);

        if (CooldownTimer > 0) CooldownTimer -= deltaTime;
    }

    public void Move(Vector2 direction, float deltaTime)
    {
        if (Stats.Fuel <= 0 || IsDestroyed) return;

        Vector2 displacement = direction.Normalized() * Stats.Speed * deltaTime;
        Position += displacement;
        Stats.Fuel -= MathF.Abs(displacement.Length()) * 0.05f;
        if (Stats.Fuel < 0) Stats.Fuel = 0;
    }

    public void Rotate(float angleDelta)
    {
        Rotation += angleDelta;
    }

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
}