namespace TankGL_fbo.Core.Entities;

using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;

public sealed class Background : IRenderable
{
    public Vector2 Position { get; }
    public float Rotation => 0f;
    public float Scale => 1.0f;
    // if (entity is TankGL_fbo.Core.Entities.Wall)
    // {
    //     float aspect = (float)tex.Width / (float)tex.Height;
    //     uvScale = new OpenTK.Mathematics.Vector2(
    //         renderWidth / TileSize,
    //         renderHeight * aspect / TileSize
    //     );
    // }
    public int ZIndex => -100;

    public string TexturePath { get; }
    public RectAABB Bounds { get; }

    public Background(Vector2 position, Vector2 halfSize, string texturePath)
    {
        Position = position;
        TexturePath = texturePath;
        Bounds = new RectAABB(position, halfSize);
    }
}