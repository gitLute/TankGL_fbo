using TankGL_fbo.Core.Contracts;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class RectAABBTests
{
    [Fact]
    public void Intersects_OverlappingBoxes_ReturnsTrue()
    {
        var a = new RectAABB(new Vector2(0, 0), new Vector2(10, 10));
        var b = new RectAABB(new Vector2(5, 5), new Vector2(10, 10));
        Assert.True(a.Intersects(b));
    }

    [Fact]
    public void Intersects_SeparatedBoxes_ReturnsFalse()
    {
        var a = new RectAABB(new Vector2(0, 0), new Vector2(5, 5));
        var b = new RectAABB(new Vector2(20, 20), new Vector2(5, 5));
        Assert.False(a.Intersects(b));
    }

    [Fact]
    public void Intersects_TouchingEdges_ReturnsFalse()
    {
        var a = new RectAABB(new Vector2(0, 0), new Vector2(10, 10));
        var b = new RectAABB(new Vector2(20, 0), new Vector2(10, 10));
        Assert.False(a.Intersects(b));
    }
}