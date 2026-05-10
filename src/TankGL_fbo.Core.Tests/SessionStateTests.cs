using TankGL_fbo.Core.Systems;
using Xunit;

namespace TankGL_fbo.Core.Tests;

public class SessionStateTests
{
    [Fact]
    public void RecordWin_IncrementsCorrectPlayer()
    {
        SessionState.Reset();
        SessionState.RecordWin(0);
        Assert.Equal(1, SessionState.Player1Wins);
        Assert.Equal(0, SessionState.Player2Wins);

        SessionState.RecordWin(1);
        Assert.Equal(1, SessionState.Player1Wins);
        Assert.Equal(1, SessionState.Player2Wins);
    }

    [Fact]
    public void Reset_ClearsWins()
    {
        SessionState.RecordWin(0);
        SessionState.RecordWin(1);
        SessionState.Reset();
        Assert.Equal(0, SessionState.Player1Wins);
        Assert.Equal(0, SessionState.Player2Wins);
    }
}