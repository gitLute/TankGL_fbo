using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Systems;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class MockScene : IScene
{
    public bool EnterCalled, ExitCalled, UpdateCalled;
    public void OnEnter() => EnterCalled = true;
    public void OnExit() => ExitCalled = true;
    public void Update(float deltaTime, Dictionary<int, HashSet<PlayerAction>> inputs) => UpdateCalled = true;
    public void CollectRenderables(List<IRenderable> renderables) { }
}

public class SceneManagerTests
{
    [Fact]
    public void ChangeScene_WithoutDelay_SwitchesImmediatelyOnUpdate()
    {
        var manager = new SceneManager();
        var scene1 = new MockScene();
        var scene2 = new MockScene();

        manager.ChangeScene(scene1);
        manager.Update(0.1f, new Dictionary<int, HashSet<PlayerAction>>());
        Assert.True(scene1.EnterCalled);

        manager.ChangeScene(scene2);
        manager.Update(0.1f, new Dictionary<int, HashSet<PlayerAction>>());
        Assert.True(scene1.ExitCalled);
        Assert.True(scene2.EnterCalled);
        Assert.Same(scene2, manager.CurrentScene);
    }

    [Fact]
    public void Update_CallsCurrentSceneUpdate()
    {
        var manager = new SceneManager();
        var scene = new MockScene();
        manager.ChangeScene(scene);
        manager.Update(0.1f, new Dictionary<int, HashSet<PlayerAction>>());
        manager.Update(0.1f, new Dictionary<int, HashSet<PlayerAction>>());
        Assert.True(scene.UpdateCalled);
    }
}