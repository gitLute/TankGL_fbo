using System;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Systems;

namespace TankGL_fbo.Core.Scenes;

public sealed class OptionsScene : MenuSceneBase
{
    private readonly string[] _menuItems = new string[5];
    private bool _unsavedChanges = false;

    public OptionsScene(Action<IScene>? requestSceneChange = null) : base(requestSceneChange) { }

    public override void OnEnter()
    {
        base.OnEnter();
        ConfigManager.Load();
        _unsavedChanges = false;
    }

    public override void OnExit()
    {
        if (_unsavedChanges)
        {
            ConfigManager.Save();
        }
        base.OnExit();
    }

    protected override string[] GetMenuItems()
    {
        _menuItems[0] = $"Debug Mode: {(ConfigManager.Config.DebugMode ? "ON" : "OFF")}";
        _menuItems[1] = $"";
        _menuItems[2] = $"";
        _menuItems[3] = "Reset to Default";
        _menuItems[4] = "Back";
        return _menuItems;
    }

    protected override void OnItemSelected(int index)
    {
        switch (index)
        {
            case 0:
                ConfigManager.Config.DebugMode = !ConfigManager.Config.DebugMode;
                _unsavedChanges = true;
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                ConfigManager.Config = new GameConfig();
                _unsavedChanges = true;
                break;
            case 4:
                RequestSceneChange?.Invoke(new MenuScene(RequestSceneChange));
                break;
        }
    }
}