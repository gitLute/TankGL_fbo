using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class BulletTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var bullet = new Bullet(new Vector2(0, 0), 0f, 100f, 25f, 2.0f, 0);
        Assert.Equal(0f, bullet.Position.X);
        Assert.Equal(0f, bullet.Position.Y);
        Assert.Equal(2.0f, bullet.Lifetime);
        Assert.Equal(25f, bullet.Damage);
        Assert.Equal(0, bullet.OwnerId);
        Assert.False(bullet.IsExpired);
    }

    [Fact]
    public void Update_MovesBullet_AndReducesLifetime()
    {
        var bullet = new Bullet(new Vector2(0, 0), 0f, 100f, 25f, 2.0f, 0);
        bullet.Update(0.5f);
        Assert.Equal(50f, bullet.Position.X);
        Assert.Equal(0f, bullet.Position.Y);
        Assert.Equal(1.5f, bullet.Lifetime);
    }

    [Fact]
    public void IsExpired_ReturnsTrue_WhenLifetimeDepleted()
    {
        var bullet = new Bullet(Vector2.Zero, 0f, 100f, 25f, 1.0f, 0);
        bullet.Update(1.1f);
        Assert.True(bullet.IsExpired);
    }
}