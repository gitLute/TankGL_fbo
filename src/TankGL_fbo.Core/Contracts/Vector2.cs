namespace TankGL_fbo.Core.Contracts;

public readonly struct Vector2
{
    public float X { get; }
    public float Y { get; }

    public Vector2(float x, float y) { X = x; Y = y; }

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 v, float scalar) => new(v.X * scalar, v.Y * scalar);

    public float Length() => MathF.Sqrt(X * X + Y * Y);

    public Vector2 Normalized()
    {
        float len = Length();
        return len > 0.0001f ? this * (1f / len) : Zero;
    }

    public static Vector2 Zero => new(0f, 0f);
    public static Vector2 Up => new(0f, 1f);
    public static Vector2 Right => new(1f, 0f);
}