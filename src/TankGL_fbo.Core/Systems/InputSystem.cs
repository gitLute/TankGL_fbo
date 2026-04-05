namespace TankGL_fbo.Core.Systems;

using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;

public sealed class InputSystem
{
    private readonly Dictionary<int, HashSet<PlayerAction>> _playerInputs;
    private readonly List<Tank> _tanks;
    private readonly List<Bullet> _bullets;


    private const float RotateSpeed = 2.5f;
    private const float BulletSpawnOffset = 22f;

    public InputSystem(Dictionary<int, HashSet<PlayerAction>> playerInputs, List<Tank> tanks, List<Bullet> bullets)
    {
        _playerInputs = playerInputs;
        _tanks = tanks;
        _bullets = bullets;
    }

    public void Process(float deltaTime)
    {
        for (int i = 0; i < _tanks.Count; i++)
        {
            var tank = _tanks[i];
            if (tank.IsDestroyed) continue;

            if (!_playerInputs.TryGetValue(i, out var actions)) continue;

            if (actions.Contains(PlayerAction.RotateLeft)) tank.Rotate(-RotateSpeed * deltaTime);
            if (actions.Contains(PlayerAction.RotateRight)) tank.Rotate(RotateSpeed * deltaTime);

            Vector2 moveDir = Vector2.Zero;
            float cos = MathF.Cos(tank.Rotation);
            float sin = MathF.Sin(tank.Rotation);

            if (actions.Contains(PlayerAction.MoveUp)) moveDir += new Vector2(cos, sin);
            if (actions.Contains(PlayerAction.MoveDown)) moveDir -= new Vector2(cos, sin);

            if (moveDir.Length() > 0f) tank.Move(moveDir, deltaTime);

            if (actions.Contains(PlayerAction.Fire) && tank.TryFire())
            {
                Vector2 spawnPos = tank.Position + new Vector2(cos, sin) * BulletSpawnOffset;
                _bullets.Add(new Bullet(spawnPos, tank.Rotation, 450f, tank.Stats.Damage, 3.0f));
            }
        }
    }
}