using System;
using TankGL_fbo.Core.Interfaces;

namespace TankGL_fbo.Core.Scenes;

public sealed class MenuScene : MenuSceneBase
{
    private static readonly string[] MenuItems = { "Start Game", "Options", "Exit" };

    public MenuScene(Action<IScene>? requestSceneChange = null) : base(requestSceneChange) { }

    protected override string[] GetMenuItems() => MenuItems;

    protected override void OnItemSelected(int index)
    {
        switch (index)
        {
            case 0:
                RequestSceneChange?.Invoke(new Level1Scene(RequestSceneChange));
                break;
            case 1:
                RequestSceneChange?.Invoke(new OptionsScene(RequestSceneChange));
                break;
            case 2:
                Environment.Exit(0);
                break;
        }
    }
}