namespace TankGL_fbo.Core.Systems;

public static class SessionState
{
    public static int Player1Wins { get; private set; }
    public static int Player2Wins { get; private set; }

    public static void RecordWin(int playerIndex)
    {
        if (playerIndex == 0) Player1Wins++;
        else if (playerIndex == 1) Player2Wins++;
    }

    public static void Reset()
    {
        Player1Wins = 0;
        Player2Wins = 0;
    }
}