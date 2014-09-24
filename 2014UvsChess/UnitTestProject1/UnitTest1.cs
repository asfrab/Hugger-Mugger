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

            moves = derp.PawnMoves(new ChessBoard("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"),new ChessLocation(7,6), ChessColor.White);
            //Debug.Assert(moves.Contains(new ChessMove(new ChessLocation(1,7),  new ChessLocation(0, 5))),
            //                            new ChessMove(new ChessLocation(1,7),  new ChessLocation(2, 5))};
        }

        [TestMethod]
        public void GetPositionsTest() {

            StudentAI derp = new StudentAI();
                ChessMove move;
                ChessBoard board = new ChessBoard("rnbqkbnr/8/8/8/8/2p5/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
                ChessColor color = ChessColor.Black;
            int i = 0;

            do { 
                move = derp.GetNextMove(board, color);
                board.MakeMove(move);
                //ChessPiece temp = board[move.From];
                //board[move.From] = board[move.To];
                //board[move.To] = temp;
                color = color == ChessColor.Black ? ChessColor.White : ChessColor.Black;
            } while (move.Flag != ChessFlag.Stalemate);
            }
    }
}
