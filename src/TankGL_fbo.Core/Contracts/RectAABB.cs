namespace TankGL_fbo.Core.Contracts;

public readonly struct RectAABB
{
    public Vector2 Center { get; }
    public Vector2 HalfSize { get; }

    public RectAABB(Vector2 center, Vector2 halfSize)
    {
        Center = center;
        HalfSize = halfSize;
    }

    public bool Intersects(RectAABB other)
    {
        return MathF.Abs(Center.X - other.Center.X) < (HalfSize.X + other.HalfSize.X) && MathF.Abs(Center.Y - other.Center.Y) < (HalfSize.Y + other.HalfSize.Y);
    }
}