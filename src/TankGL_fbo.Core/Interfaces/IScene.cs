using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;

namespace TankGL_fbo.Core.Interfaces;




public interface IScene
{
    void OnEnter();

    void OnExit();

    void Update(float deltaTime, Dictionary<int, HashSet<PlayerAction>> inputs);

    void CollectRenderables(List<IRenderable> renderables);
}