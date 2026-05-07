using System;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns.Decorators;

namespace TankGL_fbo.Core.Scenes;

public sealed class Level2Scene : LevelScene
{
    public Level2Scene(Action<IScene>? requestSceneChange = null) : base(requestSceneChange) { }

    protected override void SetupLevel()
    {
        Walls.Add(new Wall(new Vector2(-200, 0), new Vector2(20, 100)));
        Walls.Add(new Wall(new Vector2(200, 0), new Vector2(20, 100)));
        Walls.Add(new Wall(new Vector2(0, 150), new Vector2(100, 20)));
        Walls.Add(new Wall(new Vector2(0, -150), new Vector2(100, 20)));

        Tanks.Add(new Tank(new Vector2(-300, 200), "tank_red.png", new BaseStats()));
        Tanks.Add(new Tank(new Vector2(300, -200), "tank_blue.png", new BaseStats()));
    }

    protected override IScene? CreateNextLevel()
    {
        return new Level3Scene(RequestSceneChange);
    }
}