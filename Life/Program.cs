using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace cli_life
{
    public class FileManager
    {
        public static void CF()
        {
            string[] fileNames = { "figure1.txt", "figure2.txt", "figure3.txt" };

            string figure1 = "0 1 0\n0 1 0\n0 1 0";
            string figure2 = "1 1 1\n1 0 1\n1 1 1";
            string figure3 = "0 0 1\n1 1 0\n0 1 0";

            string[] figures = { figure1, figure2, figure3 };

            for (int i = 0; i < fileNames.Length; i++)
            {
                string fileName = fileNames[i];
                string figure = figures[i];

                File.WriteAllText(fileName, figure);
            }
        }
    }
    public class Settings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int CellSize { get; set; }
        public double LiveDensity { get; set; }
    }
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        public bool IsAliveNext;
        public int X { get; set; }
        public int Y { get; set; }
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }

    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;
        public int generationCount = 0;
        private int generationCount1 = 0;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }
        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                {
                    Cells[x, y] = new Cell
                    {
                        X = x,
                        Y = y
                    };
                }

            ConnectNeighbors();
            Randomize(liveDensity);
        }
        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }
        public void Advance()
        {
            generationCount++;
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
            if (IsStable())
            {
                generationCount1++;
            }
            if (generationCount1 == 8)
            {
                generationCount1 = 0;
                Console.WriteLine($"Stable phase reached after {generationCount} generations.");
            }
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
        public void SB(string f)
        {
            using (StreamWriter writer = new StreamWriter(f))
            {
                for (int x = 0; x < Columns; x++)
                {
                    for (int y = 0; y < Rows; y++)
                    {
                        var cell = Cells[x, y];
                        writer.WriteLine($"{x},{y},{cell.IsAlive}");
                    }
                }
            }
        }
        public void LB(string f)
        {
            if (!File.Exists(f))
                return;

            ClB();

            using (StreamReader reader = new StreamReader(f))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 3 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y) && bool.TryParse(parts[2], out bool isAlive))
                    {
                        if (x >= 0 && x < Columns && y >= 0 && y < Rows)
                            Cells[x, y].IsAlive = isAlive;
                    }
                }
            }
        }
        private void ClB()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    Cells[x, y].IsAlive = false;
                }
            }
        }
        public void LFFF(string fileName, int startX, int startY)
        {
            string[] lines = File.ReadAllLines(fileName);
            int rows = lines.Length;
            int cols = lines[0].Split(' ').Length;

            for (int y = 0; y < rows; y++)
            {
                string[] cells = lines[y].Split(' ');
                for (int x = 0; x < cols; x++)
                {
                    bool isAlive = cells[x] == "1";
                    Cells[startX + x, startY + y].IsAlive = isAlive;
                }
            }
        }
        public int CC()
        {
            int count = 0;
            foreach (var cell in Cells)
            {
                if (cell.IsAlive)
                {
                    count++;
                }
            }
            return count;
        }
        public int CComb()
        {
            int count = 0;
            foreach (var cell in Cells)
            {
                if (cell.IsAlive)
                {
                    count += cell.neighbors.Count(x => x.IsAlive);
                }
            }
            return count;
        }
        private bool IsStable()
        {
            foreach (var cell in Cells)
            {
                if (cell.IsAlive != cell.IsAliveNext)
                    return false;
            }
            return true;
        }
        public int CountDifferentCombinations()
        {
            HashSet<string> combinations = new HashSet<string>();

            foreach (var cell in Cells)
            {
                if (cell.IsAlive)
                {
                    string combination = GetCombination(cell);
                    combinations.Add(combination);
                }
            }

            return combinations.Count;
        }
        private string GetCombination(Cell cell)
        {
            int x = GetCellColumn(cell);
            int y = GetCellRow(cell);

            List<int> neighborStates = new List<int>();

            foreach (var neighbor in cell.neighbors)
            {
                int neighborState = neighbor.IsAlive ? 1 : 0;
                neighborStates.Add(neighborState);
            }

            neighborStates.Sort();

            string combination = $"{x},{y}:{string.Join("", neighborStates)}";
            return combination;
        }
        private int GetCellColumn(Cell cell)
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (Cells[x, y] == cell)
                        return x;
                }
            }

            return -1;
        }
        private int GetCellRow(Cell cell)
        {
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    if (Cells[x, y] == cell)
                        return y;
                }
            }

            return -1;
        }
        public int CountSymmetricPieces()
        {
            int count = 0;
            HashSet<Cell> visitedCells = new HashSet<Cell>();

            foreach (var cell in Cells)
            {
                if (!visitedCells.Contains(cell) && cell.IsAlive)
                {
                    List<Cell> symmetricPiece = new List<Cell>();
                    FindSymmetricPiece(cell, symmetricPiece);

                    bool isSymmetric = CheckSymmetry(symmetricPiece);
                    if (isSymmetric)
                    {
                        count++;
                        foreach (var symmetricCell in symmetricPiece)
                        {
                            visitedCells.Add(symmetricCell);
                        }
                    }
                }
            }

            return count;
        }
        private void FindSymmetricPiece(Cell cell, List<Cell> symmetricPiece)
        {
            if (cell == null || symmetricPiece.Contains(cell) || !cell.IsAlive)
                return;

            symmetricPiece.Add(cell);

            foreach (var neighbor in cell.neighbors)
            {
                FindSymmetricPiece(neighbor, symmetricPiece);
            }
        }
        private bool CheckSymmetry(List<Cell> symmetricPiece)
        {
            int minX = symmetricPiece.Min(c => c.X);
            int maxX = symmetricPiece.Max(c => c.X);
            int minY = symmetricPiece.Min(c => c.Y);
            int maxY = symmetricPiece.Max(c => c.Y);
            for (int i = 0; i < symmetricPiece.Count; i++)
            {
                var cell1 = symmetricPiece[i];
                var cell2 = symmetricPiece[symmetricPiece.Count - 1 - i];
                int x1 = cell1.X - minX;
                int y1 = cell1.Y - minY;
                int x2 = cell2.X - minX;
                int y2 = cell2.Y - minY;
                if (x1 != x2 || y1 != y2)
                    return false;
            }
            return true;
        }
    }
    class Program
    {
        static Board board;
        static private void Reset()
        {
            Settings settings;
            if (File.Exists("setboard.json"))
            {
                string json = File.ReadAllText("setboard.json");
                settings = JsonConvert.DeserializeObject<Settings>(json);
            }
            else
            {
                settings = new Settings
                {
                    Width = 50,
                    Height = 20,
                    CellSize = 1,
                    LiveDensity = 0.5
                };
            }
            board = new Board(
                width: settings.Width,
                height: settings.Height,
                cellSize: settings.CellSize,
                liveDensity: settings.LiveDensity);
        }
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                        Console.Write('*');
                    else
                        Console.Write(' ');
                }
                Console.Write('\n');
            }
            Console.WriteLine("Cell Count: " + board.CC());
            Console.WriteLine("Combination Count: " + board.CComb());
        }
        static void Main(string[] args)
        {
            FileManager.CF();
            Reset();
            while (true)
            {
                Console.Clear();
                Render();
                board.Advance();
                board.LFFF("figure1.txt", 10, 5);
                board.LFFF("figure2.txt", 20, 10);
                board.LFFF("figure3.txt", 30, 15);
                int combinationCount = board.CountDifferentCombinations();
                Console.WriteLine($"Different combinations: {combinationCount}");
                int symmetricPiecesCount = board.CountSymmetricPieces();
                Console.WriteLine($"Symmetric Pieces: {symmetricPiecesCount}");
                Console.WriteLine("S - Save, L - Load");
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                    if (keyInfo.Key == ConsoleKey.S)
                    {
                        board.SB("board.txt");
                        Console.WriteLine("Saved");
                    }
                    else if (keyInfo.Key == ConsoleKey.L)
                    {
                        board.LB("board.txt");
                        Console.WriteLine("Loaded");
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}