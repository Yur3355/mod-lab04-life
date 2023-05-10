using Microsoft.VisualStudio.TestTools.UnitTesting;
using cli_life;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Test
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void CreateBoardAndCells()
        {
            Board board = new Board(10, 10, 1);

            Assert.AreEqual(10, board.Columns);
            Assert.AreEqual(10, board.Rows);
            Assert.AreEqual(10, board.Width);
            Assert.AreEqual(10, board.Height);

            for (int x = 0; x < board.Columns; x++)
            {
                for (int y = 0; y < board.Rows; y++)
                {
                    Cell cell = board.Cells[x, y];
                    Assert.IsNotNull(cell);
                    Assert.AreEqual(x, cell.X);
                    Assert.AreEqual(y, cell.Y);
                }
            }
        }

        [TestMethod]
        public void TestAdvance()
        {
            int width = 50;
            int height = 20;
            int cellSize = 1;
            double liveDensity = 0.5;

            Board board = new Board(width, height, cellSize, liveDensity);

            int initialCount = board.CC();

            board.Advance();

            int currentCount = board.CC();
            Assert.AreNotEqual(initialCount, currentCount);
        }

        [TestMethod]
        public void DetermineNextLiveState_CellIsNotAliveWithThreeLiveNeighbors_IsAliveNext()
        {
            var cell = new Cell();
            cell.IsAlive = false;
            cell.neighbors.AddRange(new[]
            {
            new Cell { IsAlive = true },
            new Cell { IsAlive = true },
            new Cell { IsAlive = true },
            new Cell { IsAlive = false },
            new Cell { IsAlive = false },
            new Cell { IsAlive = false },
            new Cell { IsAlive = false },
            new Cell { IsAlive = false }
        });

            cell.DetermineNextLiveState();

            Assert.IsTrue(cell.IsAliveNext);
        }

        [TestMethod]
        public void DetermineNextLiveState_CellIsAliveWithTwoLiveNeighbors_IsAliveNext()
        {
            var cell = new Cell();
            cell.IsAlive = true;
            cell.neighbors.AddRange(new[]
            {
            new Cell { IsAlive = true },
            new Cell { IsAlive = true },
            new Cell { IsAlive = false },
            new Cell { IsAlive = false },
            new Cell { IsAlive = false },
            new Cell { IsAlive = false },
            new Cell { IsAlive = false },
            new Cell { IsAlive = false }
        });

            cell.DetermineNextLiveState();
            Assert.IsTrue(cell.IsAliveNext);
        }

        [TestMethod]
        public void Board_Advance_Should_Update_Board_State()
        {
            Board board = new Board(50, 50, 5, 0.1);
            board.Advance();
            Assert.AreEqual(1, board.generationCount);
        }
    }
}