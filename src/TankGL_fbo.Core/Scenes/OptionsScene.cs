using System;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Systems;

namespace TankGL_fbo.Core.Scenes;

public sealed class OptionsScene : MenuSceneBase
{
    private readonly string[] _menuItems = new string[4];
    private bool _unsavedChanges = false;

    private static readonly (int w, int h)[] Presets =
    {
        (1280, 720), (1920, 1080), (2560, 1440), (800, 600)
    };

    public OptionsScene(Action<IScene>? requestSceneChange = null) : base(requestSceneChange) { }

    public override void OnEnter()
    {
        base.OnEnter();
        ConfigManager.Load();
        _unsavedChanges = false;
    }

    public override void OnExit()
    {
        if (_unsavedChanges) ConfigManager.Save();
        base.OnExit();
    }

    private int GetCurrentPresetIndex()
    {
        for (int i = 0; i < Presets.Length; i++)
            if (Presets[i].w == ConfigManager.Config.ResolutionWidth &&
                Presets[i].h == ConfigManager.Config.ResolutionHeight)
                return i;
        return 0;
    }

    protected override string[] GetMenuItems()
    {
        _menuItems[0] = $"Resolution: {ConfigManager.Config.ResolutionWidth}x{ConfigManager.Config.ResolutionHeight}";
        _menuItems[1] = $"Collider Borders: {(ConfigManager.Config.ShowColliderBounds ? "ON" : "OFF")}";
        _menuItems[2] = "Reset to Default";
        _menuItems[3] = "Back";
        return _menuItems;
    }

    protected override void OnItemSelected(int index)
    {
        switch (index)
        {
            case 0:
                int next = (GetCurrentPresetIndex() + 1) % Presets.Length;
                ConfigManager.Config.ResolutionWidth = Presets[next].w;
                ConfigManager.Config.ResolutionHeight = Presets[next].h;
                _unsavedChanges = true;
                break;
            case 1:
                ConfigManager.Config.ShowColliderBounds = !ConfigManager.Config.ShowColliderBounds;
                _unsavedChanges = true;
                break;
            case 2:
                ConfigManager.Config = new GameConfig();
                _unsavedChanges = true;
                break;
            case 3:
                RequestSceneChange?.Invoke(new MenuScene(RequestSceneChange));
                break;
        }
    }
}