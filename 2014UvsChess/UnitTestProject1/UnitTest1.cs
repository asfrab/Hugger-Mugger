using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentAI;
using UvsChess;
using System.Collections.Generic;
using System.Diagnostics;

namespace StudentAI {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void TestMethod1() {
            StudentAI derp = new StudentAI();

            List<ChessMove> moves = new List<ChessMove>();

            moves = derp.BishopMoves(new ChessBoard("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"),new ChessLocation(1,4), ChessColor.White);
            //Debug.Assert(moves.Contains(new ChessMove(new ChessLocation(1,7),  new ChessLocation(0, 5))),
            //                            new ChessMove(new ChessLocation(1,7),  new ChessLocation(2, 5))};
        }

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
                if (derp2.IsValidMove(board, move, color))
                {
                    board.MakeMove(move);
                }
                else
                {
                    Assert.Fail("move made was detected as invalid");
                }
                color = (color == ChessColor.White) ? ChessColor.Black : ChessColor.White;
                move = derp2.GetNextMove(board, color);
                if (derp.IsValidMove(board, move, color))
                {
                    board.MakeMove(move);
                }
                else
                {
                    Assert.Fail("move made was detected as invalid");
                }
                color = (color == ChessColor.White) ? ChessColor.Black : ChessColor.White;
            } while (move.Flag != ChessFlag.Stalemate);
            }
    }
}
