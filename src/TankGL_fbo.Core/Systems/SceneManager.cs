using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;

namespace TankGL_fbo.Core.Systems;

public class SceneManager
{
    private IScene? _currentScene;
    private IScene? _pendingScene;

    private float _transitionTimer;
    private float _transitionDelay;
    private bool _isTransitioning;

    public IScene? CurrentScene => _currentScene;

    public bool IsTransitioning => _isTransitioning;
    public float TransitionProgress => _transitionDelay > 0 ? 1.0f - (_transitionTimer / _transitionDelay) : 1.0f;

    public void ChangeScene(IScene newScene, float delay = 0f)
    {
        _pendingScene = newScene;

        if (delay > 0f)
        {
            _transitionDelay = delay;
            _transitionTimer = delay;
            _isTransitioning = true;
        }
        else
        {
            _transitionDelay = 0f;
            _transitionTimer = 0f;
            _isTransitioning = false;
        }
    }

    public void Update(float deltaTime, Dictionary<int, HashSet<PlayerAction>> inputs)
    {

        if (_isTransitioning)
        {
            _transitionTimer -= deltaTime;

            _currentScene?.Update(deltaTime, inputs);

            if (_transitionTimer <= 0f)
            {
                _isTransitioning = false;
                PerformSceneSwitch();
            }
            return;
        }

        if (_pendingScene != null)
        {
            PerformSceneSwitch();
        }

        _currentScene?.Update(deltaTime, inputs);
    }

    private void PerformSceneSwitch()
    {
        if (_pendingScene == null) return;

        _currentScene?.OnExit();
        _currentScene = _pendingScene;
        _pendingScene = null;
        _currentScene.OnEnter();
    }

    public void CollectRenderables(List<IRenderable> renderables)
    {
        _currentScene?.CollectRenderables(renderables);
    }
}