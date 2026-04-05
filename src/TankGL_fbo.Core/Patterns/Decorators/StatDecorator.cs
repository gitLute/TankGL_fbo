namespace TankGL_fbo.Core.Patterns.Decorators;

public abstract class StatDecorator : Interfaces.ICombatStats, Interfaces.IUpdatable
{
    protected readonly Interfaces.ICombatStats _wrapped;
    protected float _durationLeft;

    public StatDecorator(Interfaces.ICombatStats wrapped, float duration)
    {
        _wrapped = wrapped;
        _durationLeft = duration;
    }

    public bool IsExpired => _durationLeft <= 0;

    public virtual float Speed => _wrapped.Speed;
    public virtual float Armor => _wrapped.Armor;
    public virtual float Damage => _wrapped.Damage;
    public virtual int Ammo { get => _wrapped.Ammo; set => _wrapped.Ammo = value; }
    public virtual float Fuel { get => _wrapped.Fuel; set => _wrapped.Fuel = value; }

    public void Update(float deltaTime)
    {
        _durationLeft -= deltaTime;
        if (_durationLeft < 0) _durationLeft = 0;
    }
}