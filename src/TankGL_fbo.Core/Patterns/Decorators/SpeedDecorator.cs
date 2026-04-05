namespace TankGL_fbo.Core.Patterns.Decorators;

public sealed class SpeedDecorator : StatDecorator
{
    private readonly float _multiplier;
    public SpeedDecorator(Interfaces.ICombatStats wrapped, float multiplier, float duration) : base(wrapped, duration) => _multiplier = multiplier;
    public override float Speed => _wrapped.Speed * _multiplier;
}