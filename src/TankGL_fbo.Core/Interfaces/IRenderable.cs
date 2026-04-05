using TankGL_fbo.Core.Contracts;

namespace TankGL_fbo.Core.Interfaces;

public interface IRenderable
{
    string TexturePath { get; }
    Vector2 Position { get; }
    float Rotation { get; }
    float Scale { get; }
    int ZIndex { get; }
}