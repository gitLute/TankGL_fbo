namespace TankGL_fbo.Core.Patterns.Decorators;

public sealed class BaseStats : Interfaces.ICombatStats
{
    public float Speed { get; } = 150f;
    public float Armor { get; } = 10f;
    public float Damage { get; } = 15f;
    public int Ammo { get; set; } = 35;
    public float Fuel { get; set; } = 100f;
}