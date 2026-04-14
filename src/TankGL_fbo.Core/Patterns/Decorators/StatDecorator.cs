using TankGL_fbo.Core.Interfaces;

namespace TankGL_fbo.Core.Patterns.Decorators;

public abstract class StatDecorator : Interfaces.ICombatStats, Interfaces.IUpdatable
{
    protected readonly Interfaces.ICombatStats _wrapped;
    protected float _durationLeft;

    public Interfaces.ICombatStats Wrapped => _wrapped;

    public StatDecorator(Interfaces.ICombatStats wrapped, float duration)
    {
        _wrapped = wrapped;
        _durationLeft = duration;
    }

    public bool IsExpired => _durationLeft <= 0;
    public float DurationLeft => _durationLeft;

    public void Refresh(float duration)
    {
        _durationLeft = duration;
    }

    public virtual float Speed => _wrapped.Speed;
    public virtual float Armor => _wrapped.Armor;
    public virtual float Damage => _wrapped.Damage;
    public virtual int Ammo { get => _wrapped.Ammo; set => _wrapped.Ammo = value; }
    public virtual float Fuel { get => _wrapped.Fuel; set => _wrapped.Fuel = value; }

    public void Update(float deltaTime)
    {
        _durationLeft -= deltaTime;
        if (_durationLeft < 0) _durationLeft = 0;


        if (_wrapped is IUpdatable updatable)
        {
            updatable.Update(deltaTime);
        }
    }

    public static T? FindInChain<T>(Interfaces.ICombatStats stats) where T : StatDecorator
    {
        if (stats is T found) return found;
        if (stats is StatDecorator dec) return FindInChain<T>(dec._wrapped);
        return null;
    }
}