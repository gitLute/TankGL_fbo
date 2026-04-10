namespace TankGL_fbo.Core.Entities;

using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;

public sealed class Bullet : IUpdatable, IRenderable
{
    public Vector2 Position { get; private set; }
    public float Rotation { get; private set; }
    public float Scale => 0.4f;
    public int ZIndex => 4;
    public string TexturePath => "bullet.png";

    public int OwnerId { get; }

    public float Lifetime { get; internal set; }
    public float Damage { get; }
    public RectAABB Bounds => new(Position, new Vector2(4f, 4f));
    public bool IsExpired => Lifetime <= 0;

    private readonly Vector2 _velocity;

    public Bullet(Vector2 startPos, float directionAngle, float speed, float damage, float lifetime, int ownerId)
    {
        Position = startPos;
        Rotation = directionAngle;
        Damage = damage;
        Lifetime = lifetime;

        OwnerId = ownerId;

        _velocity = new Vector2(MathF.Cos(directionAngle), MathF.Sin(directionAngle)) * speed;
    }

    public void Update(float deltaTime)
    {
        Position += _velocity * deltaTime;
        Lifetime -= deltaTime;
    }
}