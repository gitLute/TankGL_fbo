using System;
using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;

namespace TankGL_fbo.Core.Scenes;

public sealed class MenuScene : IScene
{
    private readonly Action<IScene>? _requestSceneChange;
    private readonly List<Background> _backgrounds = [];
    private int _selectedIndex = 0;
    private float _inputCooldown = 0f;
    private const float CooldownTime = 0.2f;

    private static readonly string[] MenuItems = { "Start Game", "Options", "Exit" };

    public MenuScene(Action<IScene>? requestSceneChange = null)
    {
        _requestSceneChange = requestSceneChange;
    }

    public void OnEnter()
    {
        _backgrounds.Add(new Background(new Vector2(0, 0), new Vector2(640, 360), "tile.png"));
        _selectedIndex = 0;
        _inputCooldown = 0f;
    }

    public void OnExit()
    {
        _backgrounds.Clear();
    }

    public void Update(float deltaTime, Dictionary<int, HashSet<PlayerAction>> inputs)
    {
        _inputCooldown -= deltaTime;
        if (_inputCooldown < 0) _inputCooldown = 0;

        if (inputs.TryGetValue(0, out var actions))
        {

            if (actions.Contains(PlayerAction.MoveUp) && _inputCooldown <= 0)
            {
                _selectedIndex = (_selectedIndex - 1 + MenuItems.Length) % MenuItems.Length;
                _inputCooldown = CooldownTime;
            }

            if (actions.Contains(PlayerAction.MoveDown) && _inputCooldown <= 0)
            {
                _selectedIndex = (_selectedIndex + 1) % MenuItems.Length;
                _inputCooldown = CooldownTime;
            }

            if (actions.Contains(PlayerAction.Fire) && _inputCooldown <= 0)
            {
                SelectItem();
                _inputCooldown = CooldownTime;
            }
        }
    }

    private void SelectItem()
    {
        switch (_selectedIndex)
        {
            case 0:
                _requestSceneChange?.Invoke(new Level1Scene(_requestSceneChange));
                break;
            case 1:

                break;
            case 2:

                break;
        }
    }

    public void CollectRenderables(List<IRenderable> renderables)
    {
        renderables.AddRange(_backgrounds);
    }

    public (string[] items, int selectedIndex) GetMenuState() => (MenuItems, _selectedIndex);
}