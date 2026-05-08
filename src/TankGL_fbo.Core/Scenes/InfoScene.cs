using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;

namespace TankGL_fbo.Core.Scenes;

public sealed class InfoScene : IScene
{
    private readonly Action<IScene>? _requestSceneChange;
    private readonly List<Background> _backgrounds = [];

    public static readonly string[] Instructions = new[]
    {
        "CONTROLS:",
        "",
        "Player 1:",
        " W/A/S/D - Move",
        " Space/LCtrl - Shoot",
        "",
        "Player 2:",
        " Arrow Keys - Move",
        " RShift/RCtrl - Shoot",
        "",
        "Menu:",
        " W/S - Navigate",
        " E - Select",
        " F1 - Show info screen",
        " F12 - Exit to Menu",
        "",
        "Press E to continue"
    };

    public InfoScene(Action<IScene>? requestSceneChange = null)
    {
        _requestSceneChange = requestSceneChange;
    }

    public void OnEnter()
    {
        _backgrounds.Add(new Background(new Vector2(0, 0), new Vector2(640, 360), "blank.png", tile: true));

        // _backgrounds.Add(new Background(
        //     position: new Vector2(0, 0),
        //     halfSize: new Vector2(400, 300),
        //     texturePath: "info.png",
        //     tile: false
        // ));
    }

    public void OnExit()
    {
        _backgrounds.Clear();
    }

    public void Update(float deltaTime, Dictionary<int, HashSet<PlayerAction>> inputs)
    {
        if (inputs.TryGetValue(0, out var actions) && actions.Contains(PlayerAction.Confirm))
        {
            _requestSceneChange?.Invoke(new MenuScene(_requestSceneChange));
        }
    }

    public void CollectRenderables(List<IRenderable> renderables)
    {
        renderables.AddRange(_backgrounds);
    }
}