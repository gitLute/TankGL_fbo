using TankGL_fbo.Core.Contracts;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class Vector2Tests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var v = new Vector2(3f, 4f);
        Assert.Equal(3f, v.X);
        Assert.Equal(4f, v.Y);
    }

    [Fact]
    public void Addition_WorksCorrectly()
    {
        var a = new Vector2(1f, 2f);
        var b = new Vector2(3f, 4f);
        var result = a + b;
        Assert.Equal(4f, result.X);
        Assert.Equal(6f, result.Y);
    }

    [Fact]
    public void Length_CalculatesCorrectly()
    {
        var v = new Vector2(3f, 4f);
        Assert.Equal(5f, v.Length());
    }

    [Fact]
    public void Normalized_ReturnsUnitVector()
    {
        var v = new Vector2(3f, 4f);
        var n = v.Normalized();
        Assert.Equal(1f, n.Length(), 3);
        Assert.Equal(0.6f, n.X, 3);
        Assert.Equal(0.8f, n.Y, 3);
    }

    [Fact]
    public void Normalized_ZeroVector_ReturnsZero()
    {
        var v = Vector2.Zero;
        Assert.Equal(0f, v.Normalized().X);
        Assert.Equal(0f, v.Normalized().Y);
    }
}