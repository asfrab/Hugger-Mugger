using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentAI;
using UvsChess;
using System.Collections.Generic;
using System.Diagnostics;

namespace StudentAI {
    [TestClass]
    public class UnitTest1 {
        const string EMPTY_BOARD = "8/8/8/8/8/8/8/8 w KQkq - 0 1";
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
        [TestMethod]
        public void TestKnightMoves() {
            StudentAI derp = new StudentAI();

            List<ChessMove> expected = new List<ChessMove>();
            List<ChessMove> moves = new List<ChessMove>();

            ChessBoard empty;
            ChessLocation locToTest;

            #region BlankBoard
            foreach (ChessColor color in (ChessColor[])Enum.GetValues(typeof(ChessColor))) {
                empty = new ChessBoard(EMPTY_BOARD);

                #region Corners
                locToTest = new ChessLocation(0, 0);
                moves = derp.KnightMoves(empty, locToTest, color);
                expected.Add(CreateMove(locToTest, 2, 1));
                expected.Add(CreateMove(locToTest, 1, 2));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(7, 0);
                moves = derp.KnightMoves(empty, locToTest, color);
                expected.Add(CreateMove(locToTest, 5, 1));
                expected.Add(CreateMove(locToTest, 6, 2));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(0, 7);
                moves = derp.KnightMoves(empty, locToTest, color);
                expected.Add(CreateMove(locToTest, 2, 6));
                expected.Add(CreateMove(locToTest, 1, 5));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(7, 7);
                moves = derp.KnightMoves(empty, locToTest, color);
                expected.Add(CreateMove(locToTest, 6, 5));
                expected.Add(CreateMove(locToTest, 5, 6));
                CompareMoveLists(expected, moves);
                #endregion

                #region Center
                locToTest = new ChessLocation(4, 4);
                moves = derp.KnightMoves(empty, locToTest, color);
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


        [TestMethod]
        public void TestPawnMoves() {
        }

        [TestMethod]
        public void TestKingMoves() {
            StudentAI derp = new StudentAI();

            List<ChessMove> expected = new List<ChessMove>();
            List<ChessMove> moves = new List<ChessMove>();

            ChessBoard empty;
            ChessLocation locToTest;

            #region BlankBoard
            foreach (ChessColor color in (ChessColor[])Enum.GetValues(typeof(ChessColor))) {
                empty = new ChessBoard(EMPTY_BOARD);

                #region Corners
                locToTest = new ChessLocation(0, 0);
                moves = derp.KingMoves(empty, locToTest, color);
                expected.Add(CreateMove(locToTest, 1, 1));
                expected.Add(CreateMove(locToTest, 0, 1));
                expected.Add(CreateMove(locToTest, 1, 0));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(7, 0);
                moves = derp.KingMoves(empty, locToTest, color);
                expected.Add(CreateMove(locToTest, 6, 1));
                expected.Add(CreateMove(locToTest, 7, 1));
                expected.Add(CreateMove(locToTest, 6, 0));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(0, 7);
                moves = derp.KingMoves(empty, locToTest, color);
                expected.Add(CreateMove(locToTest, 1, 6));
                expected.Add(CreateMove(locToTest, 0, 6));
                expected.Add(CreateMove(locToTest, 1, 7));
                CompareMoveLists(expected, moves);

                locToTest = new ChessLocation(7, 7);
                moves = derp.KingMoves(empty, locToTest, color);
                expected.Add(CreateMove(locToTest, 6, 6));
                expected.Add(CreateMove(locToTest, 7, 6));
                expected.Add(CreateMove(locToTest, 6, 7));
                CompareMoveLists(expected, moves);
                #endregion

                #region Center
                locToTest = new ChessLocation(4, 4);
                moves = derp.KingMoves(empty, locToTest, color);
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

        [TestMethod]
        public void TestRookMoves() {
        }

        [TestMethod]
        public void TestQueenMoves() {
        }

        [TestMethod]
        public void TestBishopMoves() {
        }

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
                    throw new Exception(string.Format("Could not find move: [{0},{1}] to [{2},{3}]", expectedMove.To.X, expectedMove.To.Y, expectedMove.From.Y, expectedMove.From.Y));
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