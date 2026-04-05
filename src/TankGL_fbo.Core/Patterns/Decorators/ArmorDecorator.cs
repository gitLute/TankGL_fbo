namespace TankGL_fbo.Core.Patterns.Decorators;

public sealed class ArmorDecorator : StatDecorator
{
    private readonly float _bonusArmor;
    public ArmorDecorator(Interfaces.ICombatStats wrapped, float bonusArmor, float duration) : base(wrapped, duration) => _bonusArmor = bonusArmor;
    public override float Armor => _wrapped.Armor + _bonusArmor;
}