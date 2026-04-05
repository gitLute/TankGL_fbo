namespace TankGL_fbo.Core.Contracts;

public readonly struct Viewport
{
    public float VirtualWidth { get; }
    public float VirtualHeight { get; }
    public float ActualWidth { get; }
    public float ActualHeight { get; }

    public Viewport(float virtualW, float virtualH, float actualW, float actualH)
    {
        VirtualWidth = virtualW;
        VirtualHeight = virtualH;
        ActualWidth = actualW;
        ActualHeight = actualH;
    }

    public float ScaleFactor => MathF.Min(ActualWidth / VirtualWidth, ActualHeight / VirtualHeight);

    public Vector2 Offset => new((ActualWidth - VirtualWidth * ScaleFactor) / 2f, (ActualHeight - VirtualHeight * ScaleFactor) / 2f);
}