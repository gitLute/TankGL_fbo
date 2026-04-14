using System;
using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;

namespace TankGL_fbo.Core.Scenes;

public abstract class MenuSceneBase : IScene
{
    protected readonly Action<IScene>? RequestSceneChange;
    protected int SelectedIndex { get; set; }
    protected float InputCooldown { get; set; }
    protected const float CooldownTime = 0.2f;

    private readonly List<Background> _backgrounds = [];

    protected MenuSceneBase(Action<IScene>? requestSceneChange = null)
    {
        RequestSceneChange = requestSceneChange;
    }

    protected abstract string[] GetMenuItems();

    protected abstract void OnItemSelected(int index);

    protected virtual Background CreateBackground() => new Background(new Vector2(0, 0), new Vector2(640, 360), "tile.png");

    public virtual void OnEnter()
    {
        _backgrounds.Add(CreateBackground());
        SelectedIndex = 0;
        InputCooldown = 0.5f;
    }

    public virtual void OnExit()
    {
        _backgrounds.Clear();
    }

    public virtual void Update(float deltaTime, Dictionary<int, HashSet<PlayerAction>> inputs)
    {
        InputCooldown -= deltaTime;
        if (InputCooldown < 0) InputCooldown = 0;

        if (inputs.TryGetValue(0, out var actions))
        {
            var items = GetMenuItems();

            if (actions.Contains(PlayerAction.MoveUp) && InputCooldown <= 0)
            {
                SelectedIndex = (SelectedIndex - 1 + items.Length) % items.Length;
                InputCooldown = CooldownTime;
            }

            if (actions.Contains(PlayerAction.MoveDown) && InputCooldown <= 0)
            {
                SelectedIndex = (SelectedIndex + 1) % items.Length;
                InputCooldown = CooldownTime;
            }

            if (actions.Contains(PlayerAction.Confirm) && InputCooldown <= 0)
            {
                OnItemSelected(SelectedIndex);
                InputCooldown = CooldownTime;
            }
        }
    }

    public virtual void CollectRenderables(List<IRenderable> renderables)
    {
        renderables.AddRange(_backgrounds);
    }

    public (string[] items, int selectedIndex) GetMenuState() => (GetMenuItems(), SelectedIndex);
}