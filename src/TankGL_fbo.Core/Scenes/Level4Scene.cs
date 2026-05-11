using System;
using System.Collections.Generic;
using TankGL_fbo.Core.Contracts;
using TankGL_fbo.Core.Entities;
using TankGL_fbo.Core.Interfaces;
using TankGL_fbo.Core.Patterns.Decorators;

namespace TankGL_fbo.Core.Scenes;

public sealed class Level4Scene : LevelScene
{

    private const float halfX = 400f;
    private const float halfY = 300f;
    private const float CellSize = 50f;

    private const int GridCols = (int)(halfX * 2 / CellSize);
    private const int GridRows = (int)(halfY * 2 / CellSize);
    private const float WallHalfSize = CellSize / 2;

    private readonly Random _rng = new();

    public Level4Scene(Action<IScene>? requestSceneChange = null) : base(requestSceneChange) { }

    protected override void SetupLevel()
    {
        bool[,] grid;
        Vector2 p1Spawn, p2Spawn;
        int attempts = 0;

        do
        {
            grid = GenerateLineBasedGrid();
            p1Spawn = GetRandomSpawn(grid, isLeft: true);
            p2Spawn = GetRandomSpawn(grid, isLeft: false);
            attempts++;
        } while (!IsPathExists(grid, p1Spawn, p2Spawn) && attempts < 50);

        for (int c = 0; c < GridCols; c++)
        {
            for (int r = 0; r < GridRows; r++)
            {
                if (grid[c, r])
                {
                    Walls.Add(new Wall(GridToWorld(c, r), new Vector2(WallHalfSize, WallHalfSize)));
                }
            }
        }

        Tanks.Add(new Tank(p1Spawn, "tank_red.png", new BaseStats()));
        Tanks.Add(new Tank(p2Spawn, "tank_blue.png", new BaseStats()));
    }

    protected override IScene? CreateNextLevel() => new Level3Scene(RequestSceneChange);

    private bool[,] GenerateLineBasedGrid()
    {
        bool[,] grid = new bool[GridCols, GridRows];
        int strokes = _rng.Next(6, 8);

        for (int i = 0; i < strokes; i++)
        {
            float x = _rng.NextSingle() * 600f - 300f;
            float y = _rng.NextSingle() * 400f - 200f;

            float angle = _rng.NextSingle() * MathF.PI * 2f;
            float length = _rng.NextSingle() * 350f + 500f;
            float curvature = (_rng.NextSingle() - 0.3f) * 0.06f;
            float step = CellSize * 0.1f;
            int steps = (int)(length / step);

            for (int s = 0; s < steps; s++)
            {
                int c = WorldToGridCol(x);
                int r = WorldToGridRow(y);

                if (c >= 0 && c < GridCols && r >= 0 && r < GridRows)
                {

                    if (c > 1 && c < GridCols - 2)
                        grid[c, r] = true;
                }


                x += MathF.Cos(angle) * step;
                y += MathF.Sin(angle) * step;
                angle += curvature;
            }
        }

        return grid;
    }

    private int WorldToGridCol(float x) => Math.Clamp((int)MathF.Round((x + halfX) / CellSize - 0.5f), 0, GridCols - 1);
    private int WorldToGridRow(float y) => Math.Clamp((int)MathF.Round((y + halfY) / CellSize - 0.5f), 0, GridRows - 1);

    private Vector2 GridToWorld(int col, int row)
    {
        float x = -halfX + (col + 0.5f) * CellSize;
        float y = -halfY + (row + 0.5f) * CellSize;
        return new Vector2(x, y);
    }

    private Vector2 GetRandomSpawn(bool[,] grid, bool isLeft)
    {
        var validCells = new List<(int c, int r)>();
        int colStart = isLeft ? 0 : GridCols / 2;
        int colEnd = isLeft ? GridCols / 2 - 1 : GridCols - 1;

        for (int c = colStart; c <= colEnd; c++)
            for (int r = 0; r < GridRows; r++)
                if (!grid[c, r]) validCells.Add((c, r));

        if (validCells.Count == 0)
            return isLeft ? new Vector2(-300, 0) : new Vector2(300, 0);

        var cell = validCells[_rng.Next(validCells.Count)];
        return GridToWorld(cell.c, cell.r);
    }

    private bool IsPathExists(bool[,] grid, Vector2 start, Vector2 end)
    {
        int sc = WorldToGridCol(start.X);
        int sr = WorldToGridRow(start.Y);
        int ec = WorldToGridCol(end.X);
        int er = WorldToGridRow(end.Y);

        bool[,] visited = new bool[GridCols, GridRows];
        var q = new Queue<(int c, int r)>();
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