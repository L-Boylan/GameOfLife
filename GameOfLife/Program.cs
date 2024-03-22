using Newtonsoft.Json;
using Raylib_cs;

namespace GameOfLife;

class Program
{
    private const int CellWidth = 10;
    private const int CellHeight = 10;
    private const int DefaultScreenWidth = 1920;
    private const int DefaultScreenHeight = 1080;
    private const int GridSpacing = 12;

    private enum GameScreen
    {
        MainMenu,
        GameOfLife
    }

    public static void Main()
    {
        var config = ReadConfig("../../../Resources/config.json");
        Raylib.InitWindow(config.ScreenWidth, config.ScreenHeight, "The Game of Life");

        var grid = GenerateGrid(config);
        var currentScreen = GameScreen.MainMenu;
        
        Raylib.SetTargetFPS(120);
        
        while (!Raylib.WindowShouldClose())
        {
            switch (currentScreen)
            {
                case GameScreen.MainMenu:
                    if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                        currentScreen = GameScreen.GameOfLife;
                    break;
                case GameScreen.GameOfLife:
                    if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                        currentScreen = GameScreen.MainMenu;
                    break;
            }
            
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.LightGray);
            Raylib.DrawFPS(0,0);

            switch (currentScreen)
            {
                case GameScreen.MainMenu:
                    Raylib.DrawText("MAIN MENU", (config.ScreenWidth / 2 - 100), (config.ScreenHeight / 2 - 20), 40, Color.Black);
                    Raylib.DrawText("press enter to start", (config.ScreenWidth / 2 - 100), (config.ScreenHeight / 2 + 40), 20, Color.Black);
                    break;
                case GameScreen.GameOfLife:
                    DrawCurrentGrid(grid);
                    grid = UpdateGrid(grid);
                    break;
            }
            
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static Config ReadConfig(string filename)
    {
        var jsonString = File.ReadAllText(filename);
        var config = JsonConvert.DeserializeObject<Config>(jsonString);
        if (config == null)
        {
            config = new Config
            {
                ScreenWidth = DefaultScreenWidth,
                ScreenHeight = DefaultScreenHeight
            };
        }

        return config;
    }

    static bool DeadCell(List<bool> surroundingCells)
    {
        var counter = 0;
        var isLive = false;
        
        foreach (bool cell in surroundingCells)
        {
            if (cell)
                counter++;
        }

        if (counter == 3)
            isLive = true;

        return isLive;
    }

    static bool LiveCell(List<bool> surroundingCells)
    {
        var counter = 0;
        var isLive = false;
        
        foreach (bool cell in surroundingCells)
        {
            if (cell)
                counter++;
        }

        if (counter == 2 || counter == 3)
        {
            isLive = true;
        }

        return isLive;
    }

    static bool[,] GenerateGrid(Config config)
    {
        var mBA = new List<MultidimensionalBoolArray>();
        
        for (int i = 0; i < config.ScreenHeight; i++)
        {
            var dimensionPosition = (i + 1) * GridSpacing;
            if (dimensionPosition < config.ScreenHeight)
            {
                mBA.Add(new MultidimensionalBoolArray
                {
                    Dimension = i,
                    Elements = new List<bool>()
                });
                for (int j = 0; j < config.ScreenWidth; j++)
                {
                    var elementPosition = (j + 1) * GridSpacing;
                    var randomElement = new Random();
                    if (elementPosition < config.ScreenWidth)
                    {
                        mBA[i].Elements.Add(randomElement.Next(2) == 1);
                        continue;
                    }
                    break;
                }
            }
            else
            {
                break;
            }
        }
        
        bool[,] grid = new bool[mBA.Count, mBA[0].Elements.Count];
        for (int i = 0; i < mBA.Count; i++)
        {
            for (int j = 0; j < mBA[i].Elements.Count; j++)
            {
                grid[i, j] = mBA[i].Elements[j];
            }
        }

        return grid;
    }

    static void DrawCurrentGrid(bool[,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                var posX = (j + 1) * GridSpacing;
                var posY = (i + 1) * GridSpacing;
                
                if (grid[i, j])
                {
                    Raylib.DrawRectangle(posX, posY, CellWidth, CellHeight, Color.Yellow);
                    continue;
                }
                Raylib.DrawRectangle(posX, posY, CellWidth, CellHeight, Color.Gray);
            }
        }
    }

    static bool[,] UpdateGrid(bool[,] grid)
    {
        var gridDimensions = grid.GetLength(0) - 1;
        var gridElements = grid.GetLength(1) - 1;
        
        for (int i = 0; i <= gridDimensions; i++)
        {
            for (int j = 0; j <= gridElements; j++)
            {
                var isFirstElement = j == 0;
                var isLastElement = j == gridElements;
                var isFirstDimension = i == 0;
                var isLastDimension = i == gridDimensions;
                var surroundingCells = new List<bool>();

                if (isFirstDimension && isFirstElement)
                {
                    surroundingCells.Add(grid[i, j + 1]);
                    surroundingCells.Add(grid[i + 1, j]);
                    surroundingCells.Add(grid[i + 1, j + 1]);
                    
                    if (!grid[i, j])
                    {
                        grid[i, j] = DeadCell(surroundingCells);
                        continue;
                    }
                    grid[i, j] = LiveCell(surroundingCells);
                    continue;
                }
                if (isLastDimension && isFirstElement)
                {
                    surroundingCells.Add(grid[i, j + 1]);
                    surroundingCells.Add(grid[i - 1, j]);
                    surroundingCells.Add(grid[i - 1, j + 1]);
                    
                    if (!grid[i, j])
                    {
                        grid[i, j] = DeadCell(surroundingCells);
                        continue;
                    }
                    grid[i, j] = LiveCell(surroundingCells);
                    continue;
                }
                if (isFirstDimension && isLastElement)
                {
                    surroundingCells.Add(grid[i, j - 1]);
                    surroundingCells.Add(grid[i + 1, j]);
                    surroundingCells.Add(grid[i + 1, j - 1]);
                    
                    if (!grid[i, j])
                    {
                        grid[i, j] = DeadCell(surroundingCells);
                        continue;
                    }
                    grid[i, j] = LiveCell(surroundingCells);
                    continue;
                }
                if (isLastDimension && isLastElement)
                {
                    surroundingCells.Add(grid[i, j - 1]);
                    surroundingCells.Add(grid[i - 1, j]);
                    surroundingCells.Add(grid[i - 1, j - 1]);
                    
                    if (!grid[i, j])
                    {
                        grid[i, j] = DeadCell(surroundingCells);
                        continue;
                    }
                    grid[i, j] = LiveCell(surroundingCells);
                    continue;
                }
                if (isFirstDimension && !isFirstElement || isFirstDimension && !isLastElement)
                {
                    surroundingCells.Add(grid[i, j + 1]);
                    surroundingCells.Add(grid[i, j - 1]);
                    surroundingCells.Add(grid[i + 1, j]);
                    surroundingCells.Add(grid[i + 1, j + 1]);
                    surroundingCells.Add(grid[i + 1, j - 1]);
                    
                    if (!grid[i, j])
                    {
                        grid[i, j] = DeadCell(surroundingCells);
                        continue;
                    }
                    grid[i, j] = LiveCell(surroundingCells);
                    continue;
                } 
                if (isLastDimension && !isFirstElement || isLastDimension && !isLastElement)
                {
                    surroundingCells.Add(grid[i, j + 1]);
                    surroundingCells.Add(grid[i, j - 1]);
                    surroundingCells.Add(grid[i - 1, j]);
                    surroundingCells.Add(grid[i - 1, j + 1]);
                    surroundingCells.Add(grid[i - 1, j - 1]);
                    
                    if (!grid[i, j])
                    {
                        grid[i, j] = DeadCell(surroundingCells);
                        continue;
                    }
                    grid[i, j] = LiveCell(surroundingCells);
                    continue;
                }
                if (isFirstElement && !isFirstDimension || isFirstElement && !isLastDimension)
                {
                    surroundingCells.Add(grid[i, j + 1]);
                    surroundingCells.Add(grid[i - 1, j]);
                    surroundingCells.Add(grid[i - 1, j + 1]);
                    surroundingCells.Add(grid[i + 1, j]);
                    surroundingCells.Add(grid[i + 1, j + 1]);
                    
                    if (!grid[i, j])
                    {
                        grid[i, j] = DeadCell(surroundingCells);
                        continue;
                    }
                    grid[i, j] = LiveCell(surroundingCells);
                    continue;
                }
                if (isLastElement && !isFirstDimension || isLastElement && !isLastDimension)
                {
                    surroundingCells.Add(grid[i, j - 1]);
                    surroundingCells.Add(grid[i - 1, j]);
                    surroundingCells.Add(grid[i - 1, j - 1]);
                    surroundingCells.Add(grid[i + 1, j]);
                    surroundingCells.Add(grid[i + 1, j - 1]);
                    
                    if (!grid[i, j])
                    {
                        grid[i, j] = DeadCell(surroundingCells);
                        continue;
                    }
                    grid[i, j] = LiveCell(surroundingCells);
                    continue;
                }
                
                surroundingCells.Add(grid[i, j + 1]);
                surroundingCells.Add(grid[i, j - 1]);
                surroundingCells.Add(grid[i - 1, j]);
                surroundingCells.Add(grid[i - 1, j - 1]);
                surroundingCells.Add(grid[i - 1, j + 1]);
                surroundingCells.Add(grid[i + 1, j]);
                surroundingCells.Add(grid[i + 1, j - 1]);
                surroundingCells.Add(grid[i + 1, j + 1]);
                
                if (!grid[i, j])
                {
                    grid[i, j] = DeadCell(surroundingCells);
                    continue;
                }
                grid[i, j] = LiveCell(surroundingCells);
            }
        }

        return grid;
    }
}
