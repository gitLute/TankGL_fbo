using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns;

namespace TankGL_fbo.Core.Entities;

public sealed class Bonus : IUpdatable, IRenderable
{
    public Vector2 Position { get; private set; }
    public BonusType Type { get; }
    public float Lifetime { get; private set; }

    public string TexturePath => Type switch
    {
        BonusType.SpeedUp => "bonus_speed.png",
        BonusType.Shield => "bonus_shield.png",
        BonusType.DamageBoost => "bonus_damage.png",
        BonusType.AmmoRefill => "bonus_ammo.png",
        BonusType.FuelCan => "bonus_fuel.png",
        _ => "error.png"
    };

    public float Rotation => 0;
    public float Scale => 0.8f;
    public int ZIndex => 1;

    public Bonus(Vector2 pos, BonusType type, float lifetime)
    {
        Position = pos;
        Type = type;
        Lifetime = lifetime;
    }

    public void Update(float deltaTime) => Lifetime -= deltaTime;
    public bool IsExpired => Lifetime <= 0;

    Vector2 IRenderable.Position => throw new NotImplementedException();
}