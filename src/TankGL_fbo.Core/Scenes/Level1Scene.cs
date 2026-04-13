using System;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns.Decorators;

namespace TankGL_fbo.Core.Scenes;

public sealed class Level1Scene : LevelScene
{
    public Level1Scene(Action<IScene>? requestSceneChange = null) : base(requestSceneChange) { }

    protected override void SetupLevel()
    {
        Walls.Add(new Wall(new Vector2(-150, 150), new Vector2(60, 20)));
        Walls.Add(new Wall(new Vector2(150, -150), new Vector2(60, 20)));
        Walls.Add(new Wall(new Vector2(0, 0), new Vector2(40, 40)));

        Tanks.Add(new Tank(new Vector2(-250, 0), "tank_red.png", new BaseStats()));
        Tanks.Add(new Tank(new Vector2(250, 0), "tank_blue.png", new BaseStats()));
    }

    protected override IScene? CreateNextLevel()
    {
        return new Level2Scene(RequestSceneChange);
    }
}