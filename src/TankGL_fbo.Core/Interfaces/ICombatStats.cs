namespace TankGL_fbo.Core.Interfaces;

public interface ICombatStats
{
    float Speed { get; }
    float Armor { get; }
    float Damage { get; }
    int Ammo { get; set; }
    float Fuel { get; set; }
}