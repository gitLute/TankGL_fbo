namespace TankGL_fbo.Core.Patterns.Decorators;

public sealed class DamageDecorator : StatDecorator
{
    private readonly float _multiplier;
    public DamageDecorator(Interfaces.ICombatStats wrapped, float multiplier, float duration) : base(wrapped, duration) => _multiplier = multiplier;
    public override float Damage => _wrapped.Damage * _multiplier;
}