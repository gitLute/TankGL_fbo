namespace TankGL_fbo.Core.Contracts;

public class GameConfig
{
    public bool DebugMode { get; set; } = false;
    public bool ShowColliderBounds { get; set; } = false;
    public int ResolutionWidth { get; set; } = 1280;
    public int ResolutionHeight { get; set; } = 720;
    public int MenuFontSize { get; set; } = 30;
}