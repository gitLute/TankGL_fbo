using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;

namespace TankGL_fbo.Core.Systems;

public class SceneManager
{
    private IScene? _currentScene;
    private IScene? _pendingScene;

    public IScene? CurrentScene => _currentScene;

    public void ChangeScene(IScene newScene)
    {
        _pendingScene = newScene;
    }

    public void Update(float deltaTime, Dictionary<int, HashSet<PlayerAction>> inputs)
    {

        if (_pendingScene != null)
        {
            _currentScene?.OnExit();
            _currentScene = _pendingScene;
            _pendingScene = null;
            _currentScene.OnEnter();
        }

        _currentScene?.Update(deltaTime, inputs);
    }

    public void CollectRenderables(List<IRenderable> renderables)
    {
        _currentScene?.CollectRenderables(renderables);
    }
}