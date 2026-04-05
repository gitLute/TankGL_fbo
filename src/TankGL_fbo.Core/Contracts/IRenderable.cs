namespace TankGL_fbo.Core.Contracts;

public interface IRenderable
{
    string TexturePath { get; }
    Vector2 Position { get; }
    float Rotation { get; }
    float Scale { get; }
    int ZIndex { get; }
}