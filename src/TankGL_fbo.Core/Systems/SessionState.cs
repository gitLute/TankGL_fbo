namespace TankGL_fbo.Core.Systems;

/// <summary>
/// Хранит глобальное состояние текущей игровой сессии.
/// Используется для отслеживания статистики побед игроков между уровнями.
/// </summary>
public static class SessionState
{
    /// <summary>Количество побед первого игрока в текущей сессии.</summary>
    public static int Player1Wins { get; private set; }
    /// <summary>Количество побед второго игрока в текущей сессии.</summary>
    public static int Player2Wins { get; private set; }

    /// <summary>
    /// Регистрирует победу указанного игрока.
    /// </summary>
    /// <param name="playerIndex">Индекс победившего игрока (0 или 1).</param>
    public static void RecordWin(int playerIndex)
    {
        if (playerIndex == 0) Player1Wins++;
        else if (playerIndex == 1) Player2Wins++;
    }

    /// <summary>
    /// Сбрасывает статистику побед, начиная новую игровую сессию.
    /// </summary>
    public static void Reset()
    {
        Player1Wins = 0;
        Player2Wins = 0;
    }
}