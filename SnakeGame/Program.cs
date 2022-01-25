using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Console;

namespace SnakeGame
{
    class Game
    {
        public Random rand = new Random();
        public ConsoleKeyInfo keypress = new ConsoleKeyInfo();

        int m;
        int n;
        int foodForWin;
        int foodInGame;
        int SnakeX;
        int SnakeY;
        string path;
        StreamReader sr;


        int score, steps, HeadX, HeadY, fruitX, fruitY, nTail;
        int[] TailX = new int[100];
        int[] TailY = new int[100];

        char[,] levelMap;
        string[,] Map;
        int[,] WallCompare;

        int width;
        int height;

        int totalScore = 0;
        int lives = 3;
        int level = 1;
        int deaths = 0;


        bool gameOver, reset, isprinted, horizontal, vertical;
        string dir, pre_dir;

        void LevelInfo()
        {
            if (level == 1)
            {
                path = @"D:\Level1.txt";

            }
            if (level == 2)
            {
                path = @"D:\Level2.txt";

            }
            if (level == 3)
            {
                path = @"D:\Level3.txt";

            }
            if (level > 3)
            {
                CreateWinWindow();
            }
            sr = new StreamReader(path);

            m = int.Parse(sr.ReadLine());
            n = int.Parse(sr.ReadLine());
            foodForWin = int.Parse(sr.ReadLine());
            foodInGame = int.Parse(sr.ReadLine());
            SnakeX = int.Parse(sr.ReadLine());
            SnakeY = int.Parse(sr.ReadLine());
            levelLoad();
        }

        void ShowBanner()
        {
            LevelInfo();
            height = m;
            width = n;
            Console.Clear();
            int origWidth = Console.WindowWidth;
            int origHeight = Console.WindowHeight;
            Console.SetBufferSize(origWidth, origHeight);
            Console.SetWindowSize(n + 10, m + 10);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.CursorVisible = false;

            Console.WriteLine("||=====================================================||");
            Console.WriteLine("||-----------------------------------------------------||");
            Console.WriteLine("||------------------ Welcome to Snake Game ------------||");
            Console.WriteLine("||-----------------------------------------------------||");
            Console.WriteLine("||=====================================================||");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("                     PRESS ANY KEY TO PLAY               ");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("  Controller:       - Use Arrow or WASD to move the snake");
            Console.WriteLine("                    - Press E to Pause                   ");
            Console.WriteLine("                    - Press R to reset game              ");
            Console.WriteLine("                    - Press ESC to quit game             ");

            keypress = Console.ReadKey(true);
            if (keypress.Key == ConsoleKey.Escape)
            {
                Environment.Exit(0);
            }
        } // ShowBanner()

        void Setup()
        {
            dir = "Right";
            pre_dir = "";
            score = 0;
            nTail = 0;
            steps = 0;

            gameOver = false;
            reset = false;
            isprinted = false;

            HeadX = SnakeX;
            HeadY = SnakeY;

            GenerateFood();
        } // Setup()

        void CheckInput()
        {
            while (Console.KeyAvailable)
            {
                keypress = Console.ReadKey(true);
                if (keypress.Key == ConsoleKey.Escape)
                    Environment.Exit(0);

                if (keypress.Key == ConsoleKey.E)
                {
                    pre_dir = dir;
                    dir = "Stop";
                }
                if (keypress.Key == ConsoleKey.LeftArrow || keypress.Key == ConsoleKey.A)
                {
                    pre_dir = dir;
                    dir = "Left";
                }
                if (keypress.Key == ConsoleKey.RightArrow || keypress.Key == ConsoleKey.D)
                {
                    pre_dir = dir;
                    dir = "Right";
                }
                if (keypress.Key == ConsoleKey.UpArrow || keypress.Key == ConsoleKey.W)
                {
                    pre_dir = dir;
                    dir = "Up";
                }
                if (keypress.Key == ConsoleKey.DownArrow || keypress.Key == ConsoleKey.S)
                {
                    pre_dir = dir;
                    dir = "Down";
                }
            }
        } // CheckInput()

        void Logic()
        {
            int preX = TailX[0];
            int preY = TailY[0];
            int tempX, tempY;
            // Проверка на нажатие кнопки "пауза"
            if (dir != "Stop")
            {
                TailX[0] = HeadX;
                TailY[0] = HeadY;
                for (int i = 1; i < nTail; i++)
                {
                    tempX = TailX[i];
                    tempY = TailY[i];
                    TailX[i] = preX;
                    TailY[i] = preY;
                    preX = tempX;
                    preY = tempY;
                }
            }
            ChangeDirection();

            // Если голова столкнулась со стеной
            if (WallCompare[HeadY, HeadX] == 1)
            {
                //KillSnake();
                if (lives != 0)
                {
                    KillSnake();
                }
                if (lives == 0)
                {
                    gameOver = true;
                }
            }
            else
            {
                gameOver = false;
            }
            if (IsSnakeEated())
            {
                IncreaseScore();
            }
            DetectSnakeDirection();

            if (CheckTailCollision())
            {
                // Если направление получено
                if (horizontal || vertical)
                {
                    gameOver = false;
                }
                // Если напрвления нет и остались жизни
                if (!horizontal || !vertical && lives != 0)
                {
                    KillSnake();
                }
                if (lives == 0)
                {
                    level = 1;
                    gameOver = true;
                }
                GenerateFood();
            }
        } // Logic()

        void KillSnake()
        {
            lives--;
            deaths++;
            Reset();
        }

        void ChangeDirection()
        {
            switch (dir)
            {
                case "Right":
                    HeadX++;
                    break;
                case "Left":
                    HeadX--;
                    break;
                case "Up":
                    HeadY--;
                    break;
                case "Down":
                    HeadY++;
                    break;
                case "Stop":
                    ShowPauseMenu();
                    dir = pre_dir;
                    break;
            }
        }

        void ShowPauseMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.CursorVisible = false;
                Console.WriteLine("Game Paused");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("  - Press E to resume game");
                Console.WriteLine("  - Press R to reset game");
                Console.WriteLine("  - Press ESC to quit game");
                keypress = Console.ReadKey(true);
                if (keypress.Key == ConsoleKey.Escape)
                    Environment.Exit(0);
                if (keypress.Key == ConsoleKey.R)
                {
                    reset = true;
                    break;
                }
                if (keypress.Key == ConsoleKey.E)
                    break;
            }
        }

        bool CheckTailCollision()
        {
            for (int i = 1; i < nTail; i++)
            {
                if (TailX[i] == HeadX && TailY[i] == HeadY)
                {
                    return true;
                }

            }
            return false;
        }

        void DetectSnakeDirection()
        {
            if (((dir == "Left" && pre_dir != "Up") && (dir == "Left" && pre_dir != "Down"))
               || ((dir == "Right" && pre_dir != "Up") && (dir == "Right" && pre_dir != "Down")))
            {
                horizontal = true;
            }
            else
            {
                horizontal = false;
            }

            if (((dir == "Up" && pre_dir != "Left") && (dir == "Up" && pre_dir != "Right"))
                || ((dir == "Down" && pre_dir != "Left") && (dir == "Down" && pre_dir != "Right")))
            {
                vertical = true;
            }
            else
            {
                vertical = false;
            }
        }
        void IncreaseScore()
        {
            score++;
            totalScore++;
            if (score == foodForWin)
            {
                NewLevel();
            }
            nTail++;

            GenerateFood();
        }

        bool IsSnakeEated()
        {
            return HeadX == fruitX && HeadY == fruitY;
        }

        void GenerateFood()
        {
            while (true)
            {
                fruitX = rand.Next(1, width - 1);
                fruitY = rand.Next(1, height - 1);
                if (levelMap[fruitY, fruitX].ToString() != "#")
                {
                    return;
                }
            }
        }

        bool IsWallCell(int i, int j)
        {
            return (levelMap[i, j].ToString() == "#");
        }

        bool IsFruitCell(int i, int j)
        {
            return (j == fruitX && i == fruitY);
        }

        bool IsHeadCell(int i, int j)
        {
            return (j == HeadX && i == HeadY);
        }

        bool IsTailCell(int i, int j, int k)
        {
            return (TailX[k] == j && TailY[k] == i);
        }

        void ShowResults()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Level № " + level);
            Console.WriteLine("Your lives " + lives);
            Console.WriteLine("Your food score " + score);
            Console.WriteLine($"Food score for win {foodForWin}");
        }
        
        void Render()
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Проверка на стену
                    if (IsWallCell(i, j))
                    {
                        Console.Write("#");
                    }
                    // Если стена
                    else if (IsFruitCell(i, j))
                    {
                        Console.Write("s");
                    }
                    else if (IsHeadCell(i, j))
                    {
                        Console.Write("O");
                    }
                    else
                    {
                        isprinted = false;
                        for (int k = 0; k < nTail; k++)
                        {
                            if (IsTailCell(i, j, k))
                            {
                                Console.Write("o");
                                isprinted = true;
                            }
                        }
                        if (!isprinted)
                            Console.Write(".");
                    }
                }
                Console.WriteLine();
            }
            ShowResults();
        } // Render()

        void NewLevel()
        {
            lives = 3;
            level++;
            LevelInfo();
            Reset();
        }

        void CreateWinWindow()
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            Console.Clear();
            Console.WriteLine("You Win");
            Console.WriteLine("Yor did " + steps + " steps");
            Console.WriteLine("you died " + deaths + " times");
            Console.WriteLine("Your total food score " + totalScore);
            Console.WriteLine("Press R to reset game");
            Console.WriteLine("Press ESC to quit game");
            deaths = 0;
            totalScore = 0;
            while (true)
            {
                keypress = Console.ReadKey(true);
                if (keypress.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
                if (keypress.Key == ConsoleKey.R)
                {
                    level = 1;
                    LevelInfo();
                    Reset();
                    break;
                }
            }
        } // CreateWinWindow()

        void CreateloseWindow()
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            Console.Clear();
            Console.WriteLine("You Died");
            Console.WriteLine("Press R to reset game");
            Console.WriteLine("Press ESC to quit game");
            while (true)
            {
                keypress = Console.ReadKey(true);
                if (keypress.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
                if (keypress.Key == ConsoleKey.R)
                {
                    level = 1;
                    LevelInfo();
                    Reset();
                    break;
                }
            }
        } // CreateloseWindow()

        void Reset()
        {
            if (lives == 0)
            {
                lives = 3;
                CreateloseWindow();
            }
            else
            {
                reset = true;
                Setup();
                Update();
            }
        } // Reset()

        void Update()
        {
            while (!gameOver)
            {
                while (Console.KeyAvailable) // Пошаговое управление
                {

                    CheckInput();
                    Logic();
                    Render();
                    steps++;

                    if (reset == true)
                        break;
                }
            }
            if (gameOver)
                CreateloseWindow();
        } // Update()


        void levelLoad()
        {
            levelMap = new char[m, n];
            Map = new string[m, n];
            WallCompare = new int[m, n];

            //Посимвольная запись путём разбиения каждой i'той строки на j символов
            for (int i = 0; i < m; i++)
            {
                string temp = sr.ReadLine();
                string[] line = temp.Split(new[] { ' ' });
                for (int j = 0; j < n; j++)
                {
                    levelMap[i, j] = char.Parse(line[j]);
                    if (levelMap[i, j].ToString() == "#")
                    {
                        Map[i, j] = levelMap[i, j].ToString();
                        WallCompare[i, j] = 1;
                    }
                    else
                    {
                        Map[i, j] = levelMap[i, j].ToString();
                        WallCompare[i, j] = 0;
                    }
                }

            }
        }// levelLoad()


        static void Main(string[] args)
        {
            Game snake = new Game();
            snake.ShowBanner();
            while (true)
            {
                snake.Setup();
                snake.Update();
                Console.Clear();
            }
        }
    }
}
