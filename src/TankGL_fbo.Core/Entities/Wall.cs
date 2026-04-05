namespace TankGL_fbo.Core.Entities;

using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;

public sealed class Wall : IRenderable
{
    public Vector2 Position { get; }
    public float Rotation => 0f;
    public float Scale => 1.0f;
    public int ZIndex => 2;
    public string TexturePath => "wall.png";

    public RectAABB Bounds { get; }

    public Wall(Vector2 position, Vector2 halfSize)
    {
        Position = position;
        Bounds = new RectAABB(position, halfSize);
    }
}