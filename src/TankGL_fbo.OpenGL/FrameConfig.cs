namespace TankGL_fbo.OpenGL;

public readonly struct FrameConfig
{
    public int Width { get; }
    public int Height { get; }
    public float Left { get; }
    public float Right { get; }
    public float Bottom { get; }
    public float Top { get; }

    public FrameConfig(int width, int height)
    {
        Width = width;
        Height = height;
        Left = -width / 2f;
        Right = width / 2f;
        Bottom = -height / 2f;
        Top = height / 2f;
    }
}