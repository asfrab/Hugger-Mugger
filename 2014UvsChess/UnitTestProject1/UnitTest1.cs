using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentAI;
using UvsChess;
using System.Collections.Generic;
using System.Diagnostics;

namespace StudentAI {
    [TestClass]
    public class UnitTest1 {
        const string EMPTY_BOARD = "8/8/8/1K6/8/8/8/8 w KQkq - 0 1";
        const string FRESH_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        [TestMethod]
        public void GetPositionsTest() {

            StudentAI derp = new StudentAI();
            StudentAI derp2 = new StudentAI();
            ChessMove move;
            ChessBoard board = new ChessBoard("rn1qk1nr/PPPPPPPP/4b1b1/8/8/B1B5/pppppppp/RN1QK1NR w KQkq - 0 1");
            ChessColor color = ChessColor.Black;
            int i = 0;

            do {
                move = derp.GetNextMove(board, color);
                if (derp2.IsValidMove(board, move, color)) {
                    board.MakeMove(move);
                }
                else {
                    Assert.Fail("move made was detected as invalid");
                }
                color = (color == ChessColor.White) ? ChessColor.Black : ChessColor.White;
                move = derp2.GetNextMove(board, color);
                if (derp.IsValidMove(board, move, color)) {
                    board.MakeMove(move);
                }
                else {
                    Assert.Fail("move made was detected as invalid");
                }
                color = (color == ChessColor.White) ? ChessColor.Black : ChessColor.White;
            } while (move.Flag != ChessFlag.Stalemate);
        }

        #region MoveTesting
        #region Knight Moves
        [TestMethod]
        public void TestKnightMoves() {
            StudentAI derp = new StudentAI();

            List<ChessMove> expected = new List<ChessMove>();
            List<ChessMove> moves = new List<ChessMove>();

            ChessBoard board;
            ChessLocation locToTest;

            #region BlankBoard
            foreach (ChessColor color in (ChessColor[])Enum.GetValues(typeof(ChessColor))) {
                board = new ChessBoard("8/3k4/8/1K6/8/8/8/8 w KQkq - 0 1");

                #region Corners
                locToTest = new ChessLocation(0, 0);
                moves = derp.KnightMoves(board, locToTest, color);
                expected.Add(CreateMove(locToTest, 2, 1));
                expected.Add(CreateMove(locToTest, 1, 2));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(7, 0);
                moves = derp.KnightMoves(board, locToTest, color);
                expected.Add(CreateMove(locToTest, 5, 1));
                expected.Add(CreateMove(locToTest, 6, 2));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(0, 7);
                moves = derp.KnightMoves(board, locToTest, color);
                expected.Add(CreateMove(locToTest, 2, 6));
                expected.Add(CreateMove(locToTest, 1, 5));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(7, 7);
                moves = derp.KnightMoves(board, locToTest, color);
                expected.Add(CreateMove(locToTest, 6, 5));
                expected.Add(CreateMove(locToTest, 5, 6));
                CompareMoveLists(expected, moves);
                #endregion

                #region Center
                locToTest = new ChessLocation(4, 4);
                moves = derp.KnightMoves(board, locToTest, color);
                expected.Add(CreateMove(locToTest, 6, 5));
                expected.Add(CreateMove(locToTest, 6, 3));
                expected.Add(CreateMove(locToTest, 5, 6));
                expected.Add(CreateMove(locToTest, 5, 2));
                expected.Add(CreateMove(locToTest, 3, 6));
                expected.Add(CreateMove(locToTest, 3, 2));
                expected.Add(CreateMove(locToTest, 2, 5));
                expected.Add(CreateMove(locToTest, 2, 3));
                CompareMoveLists(expected, moves);
                #endregion
            }
            #endregion

        } 
        #endregion
        
        #region Pawn Moves
        [TestMethod]
        public void TestPawnMoves() {
            StudentAI derp = new StudentAI();

            List<ChessMove> expected = new List<ChessMove>();
            List<ChessMove> moves = new List<ChessMove>();

            ChessBoard board;
            ChessLocation locToTest;

            #region BlankBoard
            board = new ChessBoard("1K2k3/8/8/8/8/8/8/8 w KQkq - 0 1");
            for (int y = 1; y < 7; ++y) {
                for (int x = 0; x < 8; ++x) {
                    locToTest = new ChessLocation(x, y);
                    moves = derp.PawnMoves(board, locToTest, ChessColor.Black);
                    if (y == 1) {
                        expected.Add(CreateMove(locToTest, x, y + 2));
                    }
                    expected.Add(CreateMove(locToTest, x, y + 1));
                    CompareMoveLists(expected, moves);
                }
            }

            board = new ChessBoard("8/8/8/8/8/8/8/1K2k3 w KQkq - 0 1");
            for (int y = 6; y < 0; --y) {
                for (int x = 0; x < 8; ++x) {
                    locToTest = new ChessLocation(x, y);
                    moves = derp.PawnMoves(board, locToTest, ChessColor.Black);
                    if (y == 6) {
                        expected.Add(CreateMove(locToTest, x, y - 2));
                    }
                    expected.Add(CreateMove(locToTest, x, y - 1));
                    CompareMoveLists(expected, moves);
                }
            }
            #endregion
        } 
        #endregion

        #region King Moves
        [TestMethod]
        public void TestKingMoves() {
            StudentAI derp = new StudentAI();

            List<ChessMove> expected = new List<ChessMove>();
            List<ChessMove> moves = new List<ChessMove>();

            ChessBoard board;
            ChessLocation locToTest;

            #region BlankBoard
            foreach (ChessColor color in (ChessColor[])Enum.GetValues(typeof(ChessColor))) {
                board = new ChessBoard("8/3k4/8/1K6/8/8/8/8 w KQkq - 0 1");

                #region Corners
                locToTest = new ChessLocation(0, 0);
                moves = derp.KingMoves(board, locToTest, color);
                expected.Add(CreateMove(locToTest, 1, 1));
                expected.Add(CreateMove(locToTest, 0, 1));
                expected.Add(CreateMove(locToTest, 1, 0));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(7, 0);
                moves = derp.KingMoves(board, locToTest, color);
                expected.Add(CreateMove(locToTest, 6, 1));
                expected.Add(CreateMove(locToTest, 7, 1));
                expected.Add(CreateMove(locToTest, 6, 0));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(0, 7);
                moves = derp.KingMoves(board, locToTest, color);
                expected.Add(CreateMove(locToTest, 1, 6));
                expected.Add(CreateMove(locToTest, 0, 6));
                expected.Add(CreateMove(locToTest, 1, 7));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(7, 7);
                moves = derp.KingMoves(board, locToTest, color);
                expected.Add(CreateMove(locToTest, 6, 6));
                expected.Add(CreateMove(locToTest, 7, 6));
                expected.Add(CreateMove(locToTest, 6, 7));
                CompareMoveLists(expected, moves);
                #endregion

                #region Center
                locToTest = new ChessLocation(4, 4);
                moves = derp.KingMoves(board, locToTest, color);
                expected.Add(CreateMove(locToTest, 5, 5));
                expected.Add(CreateMove(locToTest, 5, 4));
                expected.Add(CreateMove(locToTest, 5, 3));
                expected.Add(CreateMove(locToTest, 4, 5));
                expected.Add(CreateMove(locToTest, 4, 3));
                expected.Add(CreateMove(locToTest, 3, 5));
                expected.Add(CreateMove(locToTest, 3, 4));
                expected.Add(CreateMove(locToTest, 3, 3));
                CompareMoveLists(expected, moves);
                #endregion
            }
            #endregion
        } 
        #endregion

        #region Rook Moves
        [TestMethod]
        public void TestRookMoves() {
            StudentAI derp = new StudentAI();

            List<ChessMove> expected = new List<ChessMove>();
            List<ChessMove> moves = new List<ChessMove>();

            ChessBoard board;
            ChessLocation locToTest;

            #region BlankBoard
            foreach (ChessColor color in (ChessColor[])Enum.GetValues(typeof(ChessColor))) {
                board = new ChessBoard("8/3k4/8/1K6/8/8/8/8 w KQkq - 0 1");

                #region Corners
                locToTest = new ChessLocation(0, 0);
                moves = derp.RookMoves(board, locToTest, color);
                for (int x = locToTest.X + 1; x < 8; ++x) {
                    expected.Add(CreateMove(locToTest, x, locToTest.Y));
                }
                for (int y = locToTest.Y + 1; y < 8; ++y) {
                    expected.Add(CreateMove(locToTest, locToTest.X, y));
                }
                CompareMoveLists(expected, moves);


                locToTest = new ChessLocation(7, 0);
                moves = derp.RookMoves(board, locToTest, color);
                for (int x = locToTest.X - 1; x >= 0; --x) {
                    expected.Add(CreateMove(locToTest, x, locToTest.Y));
                }
                for (int y = locToTest.Y + 1; y < 8; ++y) {
                    expected.Add(CreateMove(locToTest, locToTest.X, y));
                }
                CompareMoveLists(expected, moves);


                locToTest = new ChessLocation(0, 7);
                moves = derp.RookMoves(board, locToTest, color);
                for (int x = locToTest.X + 1; x < 8; ++x) {
                    expected.Add(CreateMove(locToTest, x, locToTest.Y));
                }
                for (int y = locToTest.Y - 1; y >= 0; --y) {
                    expected.Add(CreateMove(locToTest, locToTest.X, y));
                }
                CompareMoveLists(expected, moves);


                locToTest = new ChessLocation(7, 7);
                moves = derp.RookMoves(board, locToTest, color);
                for (int x = locToTest.X - 1; x >= 0; --x) {
                    expected.Add(CreateMove(locToTest, x, locToTest.Y));
                }
                for (int y = locToTest.Y - 1; y >= 0; --y) {
                    expected.Add(CreateMove(locToTest, locToTest.X, y));
                }
                CompareMoveLists(expected, moves);
                #endregion

                #region Center
                locToTest = new ChessLocation(4, 4);
                moves = derp.RookMoves(board, locToTest, color);
                for (int x = 0; x < 8; ++x) {
                    if (x != locToTest.X)
                        expected.Add(CreateMove(locToTest, x, locToTest.Y));
                }
                for (int y = 0; y < 8; ++y) {
                    if (y != locToTest.Y)
                        expected.Add(CreateMove(locToTest, locToTest.X, y));
                }

                CompareMoveLists(expected, moves);
                #endregion
            }
            #endregion
        } 
        #endregion

        #region Queen Moves
        [TestMethod]
        public void TestQueenMoves() {
            StudentAI derp = new StudentAI();

            List<ChessMove> expected = new List<ChessMove>();
            List<ChessMove> moves = new List<ChessMove>();

            ChessBoard board;
            ChessLocation locToTest;

            #region BlankBoard
            foreach (ChessColor color in (ChessColor[])Enum.GetValues(typeof(ChessColor))) {
                board = new ChessBoard("8/3k4/8/1K6/8/8/8/8 w KQkq - 0 1");

                #region Corners
                board = new ChessBoard(color == ChessColor.White ? "8/8/7p/1K5k/8/8/8/8 w KQkq - 0 1" : "8/8/7P/1k5K/8/8/8/8 w KQkq");
                locToTest = new ChessLocation(0, 0);
                moves = derp.QueenMoves(board, locToTest, color);
                for (int i = 1; i < 8; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y + i));
                    expected.Add(CreateMove(locToTest, locToTest.X, locToTest.Y + i));
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y));
                }
                CompareMoveLists(expected, moves);

                board = new ChessBoard(color == ChessColor.White ? "8/8/p7/k5K1/8/8/8/8 w KQkq - 0 1" : "8/8/P7/K5k1/8/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(7, 0);
                moves = derp.QueenMoves(board, locToTest, color);
                for (int i = 1; i < 8; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y + i));
                    expected.Add(CreateMove(locToTest, locToTest.X, locToTest.Y + i));
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y));
                }
                CompareMoveLists(expected, moves);

                board = new ChessBoard(color == ChessColor.White ? "8/8/1K5k/7p/8/8/8/8 w KQkq - 0 1" : "8/8/1k5K/7P/8/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(0, 7);
                moves = derp.QueenMoves(board, locToTest, color);
                for (int i = 1; i < 8; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y - i));
                    expected.Add(CreateMove(locToTest, locToTest.X, locToTest.Y - i));
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y));
                }
                CompareMoveLists(expected, moves);

                board = new ChessBoard(color == ChessColor.White ? "8/8/k5K1/p7/8/8/8/8 w KQkq - 0 1" : "8/8/K5k1/P7/8/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(7, 7);
                moves = derp.QueenMoves(board, locToTest, color);
                for (int i = 1; i < 8; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y - i));
                    expected.Add(CreateMove(locToTest, locToTest.X, locToTest.Y - i));
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y));
                }
                CompareMoveLists(expected, moves);
                #endregion

                #region Center
                board = new ChessBoard(color == ChessColor.White ? "1rkp2K1/2pp4/8/8/4Q3/8/8/8 w KQkq - 0 1" : "1RKP2k1/2PP4/8/8/4q3/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(4, 4);
                moves = derp.QueenMoves(board, locToTest, color);
                for (int i = 1; i < 4; ++i) {
                    if (locToTest.X - i == 1 && locToTest.Y - i == 1)
                        expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y - i, ChessFlag.Check));
                    else
                        expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y - i));
                    expected.Add(CreateMove(locToTest, locToTest.X, locToTest.Y - i));
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y - i));
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y));
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y + i));
                    expected.Add(CreateMove(locToTest, locToTest.X, locToTest.Y + i));
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y + i));
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y));
                }
                expected.Add(CreateMove(locToTest, 0, 0));
                expected.Add(CreateMove(locToTest, 0, locToTest.Y));
                expected.Add(CreateMove(locToTest, locToTest.X, 0));
                CompareMoveLists(expected, moves);
                #endregion
            }
            #endregion
        } 
        #endregion

        #region Bishop Moves
        [TestMethod]
        public void TestBishopMoves() {
            StudentAI derp = new StudentAI();

            List<ChessMove> expected = new List<ChessMove>();
            List<ChessMove> moves = new List<ChessMove>();

            ChessBoard board;
            ChessLocation locToTest;

            #region BlankBoard
            foreach (ChessColor color in (ChessColor[])Enum.GetValues(typeof(ChessColor))) {
                board = new ChessBoard("8/3k4/8/1K6/8/8/8/8 w KQkq - 0 1");

                #region Corners
                board = new ChessBoard("8/2k3K1/8/8/8/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(0, 0);
                moves = derp.BishopMoves(board, locToTest, color);
                for (int i = 1; i < 8; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y + i));
                }
                CompareMoveLists(expected, moves);

                board = new ChessBoard("8/1k3K1/8/8/8/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(7, 0);
                moves = derp.BishopMoves(board, locToTest, color);
                for (int i = 1; i < 8; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y + i));
                }
                CompareMoveLists(expected, moves);

                board = new ChessBoard("8/1k3K1/8/8/8/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(0, 7);
                moves = derp.BishopMoves(board, locToTest, color);
                for (int i = 1; i < 8; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y - i));
                }
                CompareMoveLists(expected, moves);

                board = new ChessBoard("8/2k3K1/8/8/8/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(7, 7);
                moves = derp.BishopMoves(board, locToTest, color);
                for (int i = 1; i < 8; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y - i));
                }
                CompareMoveLists(expected, moves);
                #endregion

                #region Sides
                board = new ChessBoard("8/8/8/1K3k2/8/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(0, 3);
                moves = derp.BishopMoves(board, locToTest, color);
                for (int i = 1; i < 4; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y + i));
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y - i));
                }
                expected.Add(CreateMove(locToTest, 4, 7));
                CompareMoveLists(expected, moves);

                board = new ChessBoard("8/8/8/2k3K1/8/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(4, 0);
                moves = derp.BishopMoves(board, locToTest, color);
                for (int i = 1; i < 4; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y + i));
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y + i));
                }
                expected.Add(CreateMove(locToTest, 0, 4));
                CompareMoveLists(expected, moves);

                board = new ChessBoard("8/8/8/1K3k2/8/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(7, 4);
                moves = derp.BishopMoves(board, locToTest, color);
                for (int i = 1; i < 4; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y + i));
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y - i));
                }
                expected.Add(CreateMove(locToTest, 3, 0));
                CompareMoveLists(expected, moves);

                board = new ChessBoard("8/8/8/2k3K1/8/8/8/8 w KQkq - 0 1");
                locToTest = new ChessLocation(3, 7);
                moves = derp.BishopMoves(board, locToTest, color);
                for (int i = 1; i < 4; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y - i));
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y - i));
                }
                expected.Add(CreateMove(locToTest, 7, 3));
                CompareMoveLists(expected, moves);
                #endregion

                #region Center
                board = new ChessBoard("8/4k3/8/8/8/8/8/4K3 w KQkq - 0 1");
                locToTest = new ChessLocation(4, 4);
                moves = derp.BishopMoves(board, locToTest, color);
                for (int i = 1; i < 4; ++i) {
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y - i));
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y - i));
                    expected.Add(CreateMove(locToTest, locToTest.X + i, locToTest.Y + i));
                    expected.Add(CreateMove(locToTest, locToTest.X - i, locToTest.Y + i));
                }
                expected.Add(CreateMove(locToTest, 0, 0));
                CompareMoveLists(expected, moves);
                #endregion
            }
            #endregion
        } 
        #endregion

        public ChessMove CreateMove(ChessLocation from, int toX, int toY, ChessFlag flag = ChessFlag.NoFlag) {
            return CreateMove(from.X, from.Y, toX, toY, flag);
        }

        public ChessMove CreateMove(int fromX, int fromY, int toX, int toY, ChessFlag flag = ChessFlag.NoFlag) {
            return new ChessMove(new ChessLocation(fromX, fromY), new ChessLocation(toX, toY), flag);
        }

        public bool CompareMoveLists(List<ChessMove> expectedMoves, List<ChessMove> actual) {
            List<ChessMove> expected = new List<ChessMove>(expectedMoves);
            expectedMoves.Clear();
            if (expected.Count != actual.Count) {
                throw new Exception(string.Format("Expected # of moves: {0}; Actual # of Moves: {1}", expected.Count, actual.Count));
            }

            foreach (ChessMove expectedMove in expected) {
                if (!actual.Contains(expectedMove))
                    throw new Exception(string.Format("Could not find move: [{0},{1}] to [{2},{3}]", expectedMove.From.X, expectedMove.From.Y, expectedMove.To.Y, expectedMove.To.Y));
            }

            return true;
        }
        #endregion


        [TestMethod]
        public void TestMethod1() {
            StudentAI derp = new StudentAI();

            List<ChessMove> moves = new List<ChessMove>();

            moves = derp.BishopMoves(new ChessBoard("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"), new ChessLocation(1, 4), ChessColor.White);
            //Debug.Assert(moves.Contains(new ChessMove(new ChessLocation(1,7),  new ChessLocation(0, 5))),
            //                            new ChessMove(new ChessLocation(1,7),  new ChessLocation(2, 5))};
        }
    }
}