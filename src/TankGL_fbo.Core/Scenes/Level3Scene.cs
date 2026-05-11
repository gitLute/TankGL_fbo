using System;
using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns.Decorators;

namespace TankGL_fbo.Core.Scenes;

public sealed class Level3Scene : LevelScene
{
    private const float halfX = 400f;
    private const float halfY = 300f;
    private const float CellSize = 50f;
    private const float genChance = 0.1f;

    private const int GridCols = (int)(halfX * 2 / CellSize);
    private const int GridRows = (int)(halfY * 2 / CellSize);
    private const float WallHalfSize = CellSize / 2;

    private readonly Random _rng = new();

    public Level3Scene(Action<IScene>? requestSceneChange = null) : base(requestSceneChange) { }

    protected override void SetupLevel()
    {
        bool[,] grid;
        Vector2 p1Spawn, p2Spawn;
        int attempts = 0;

        do
        {
            grid = GenerateRandomGrid();
            p1Spawn = GetRandomSpawn(grid, isLeft: true);
            p2Spawn = GetRandomSpawn(grid, isLeft: false);
            attempts++;
        } while (!IsPathExists(grid, p1Spawn, p2Spawn) && attempts < 100);

        for (int c = 0; c < GridCols; c++)
        {
            for (int r = 0; r < GridRows; r++)
            {
                if (grid[c, r])
                {
                    Vector2 worldPos = GridToWorld(c, r);
                    Walls.Add(new Wall(worldPos, new Vector2(WallHalfSize, WallHalfSize)));
                }
            }
        }


        Tanks.Add(new Tank(p1Spawn, "tank_red.png", new BaseStats()));
        Tanks.Add(new Tank(p2Spawn, "tank_blue.png", new BaseStats()));
    }

    protected override IScene? CreateNextLevel()
    {
        return new Level4Scene(RequestSceneChange);
    }

    private bool[,] GenerateRandomGrid()
    {
        bool[,] grid = new bool[GridCols, GridRows];
        float wallChance = genChance + (float)_rng.NextDouble() * 0.15f;

        for (int c = 0; c < GridCols; c++)
        {
            for (int r = 0; r < GridRows; r++)
            {

                bool isCenter = c == GridCols / 2 - 1 || c == GridCols / 2;
                grid[c, r] = _rng.NextDouble() < (isCenter ? wallChance * 0.4f : wallChance);
            }
        }
        return grid;
    }

    private Vector2 GridToWorld(int col, int row)
    {
        float x = -halfX + col * CellSize + CellSize / 2f;
        float y = -halfY + row * CellSize + CellSize / 2f;
        return new Vector2(x, y);
    }

    private (int c, int r) WorldToGrid(Vector2 pos)
    {
        int c = (int)MathF.Round((pos.X + halfX - CellSize / 2f) / CellSize);
        int r = (int)MathF.Round((pos.Y + halfY - CellSize / 2f) / CellSize);
        c = Math.Clamp(c, 0, GridCols - 1);
        r = Math.Clamp(r, 0, GridRows - 1);
        return (c, r);
    }

    private Vector2 GetRandomSpawn(bool[,] grid, bool isLeft)
    {
        List<(int c, int r)> validCells = new();
        int colStart = isLeft ? 0 : GridCols / 2;
        int colEnd = isLeft ? GridCols / 2 - 1 : GridCols - 1;

        for (int c = colStart; c <= colEnd; c++)
        {
            for (int r = 0; r < GridRows; r++)
            {
                if (!grid[c, r]) validCells.Add((c, r));
            }
        }

        if (validCells.Count == 0)
            return isLeft ? new Vector2(-250, 0) : new Vector2(250, 0);

        var cell = validCells[_rng.Next(validCells.Count)];
        return GridToWorld(cell.c, cell.r);
    }

    private bool IsPathExists(bool[,] grid, Vector2 start, Vector2 end)
    {
        var (sc, sr) = WorldToGrid(start);
        var (ec, er) = WorldToGrid(end);

        bool[,] visited = new bool[GridCols, GridRows];
        Queue<(int c, int r)> q = new();
        q.Enqueue((sc, sr));
        visited[sc, sr] = true;

        int[] dc = { 1, -1, 0, 0 };
        int[] dr = { 0, 0, 1, -1 };

        while (q.Count > 0)
        {
            var (c, r) = q.Dequeue();
            if (c == ec && r == er) return true;

            for (int i = 0; i < 4; i++)
            {
                int nc = c + dc[i];
                int nr = r + dr[i];
                if (nc >= 0 && nc < GridCols && nr >= 0 && nr < GridRows &&
                    !visited[nc, nr] && !grid[nc, nr])
                {
                    visited[nc, nr] = true;
                    q.Enqueue((nc, nr));
                }
            }
        }
        return false;
    }
}