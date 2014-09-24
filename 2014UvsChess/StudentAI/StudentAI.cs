using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;

namespace StudentAI
{
    public class StudentAI : IChessAI
    {
        #region IChessAI Members that are implemented by the Student

        /// <summary>
        /// The name of your AI
        /// </summary>
        public string Name
        {
#if DEBUG
            get { return "Its a Trap! (Debug)"; }
#else
            get { return "Its a Trap!"; }
#endif
        }

        #region Main Methods
        Dictionary<ChessLocation, ChessPiece> myPieces;
        Dictionary<ChessLocation, ChessPiece> theirPieces;
        ChessColor myColorForDict;

        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board</param>
        /// <param name="yourColor">Your color</param>
        /// <returns> Returns the best chess move the player has for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor) {

            List<ChessMove> possibleMoves = getPossibleMoves(board, myColor);
            ChessMove moveToMake;
            ChessPiece pieceToMove;

            // If there are moves to be made choose one at random
            if (possibleMoves.Count > 0) {
                possibleMoves.Sort((a, b) => b.ValueOfMove.CompareTo(a.ValueOfMove));
                int highestVal = int.MinValue;
                int pos = 0;
                foreach(var move in possibleMoves)
                {
                    if(move.ValueOfMove > highestVal)
                    {
                        highestVal = move.ValueOfMove;
                    }
                    else if(move.ValueOfMove < highestVal)
                    {
                        break;
                    }
                    pos++;
                }
                if (pos > 0)
                {
                Random rand = new Random();
                    int indexOfMove = rand.Next(pos);
                moveToMake = possibleMoves[indexOfMove];
                }
                else
                {
                    moveToMake = possibleMoves[0];
                }
                // Change position of our piec in local collection
                pieceToMove = myPieces[moveToMake.From];
                
                if ((pieceToMove == ChessPiece.WhitePawn || pieceToMove == ChessPiece.BlackPawn) && (moveToMake.To.Y == 0 || moveToMake.To.Y == 7)) {
                    pieceToMove = myColor == ChessColor.Black ? ChessPiece.BlackQueen : ChessPiece.WhiteQueen;
                }
                myPieces.Add(moveToMake.To, pieceToMove);
                myPieces.Remove(moveToMake.From);

                // If we attacked their piece, remove it from collection
                if (theirPieces.TryGetValue(moveToMake.To, out pieceToMove)) {
                    theirPieces.Remove(moveToMake.To);
                }
            }
            else { // No moves left.  Declare stalemate
                moveToMake = new ChessMove(new ChessLocation(1, 1), new ChessLocation(1, 1), ChessFlag.Stalemate);
            }

            return moveToMake;
        }

        /// <summary>
        /// Validates a move. The framework uses this to validate the opponents move.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        public bool IsValidMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            List<ChessMove> possibleMoves = getPossibleMoves(boardBeforeMove, colorOfPlayerMoving);            
            var tempDict = myPieces;
            myPieces = theirPieces;
            theirPieces = tempDict;
            myColorForDict = colorOfPlayerMoving;
            if (possibleMoves.Contains(moveToCheck))
            {
                // Change the position of the opponents piece in local collection 
                if (myPieces != null)
                {
                    ChessPiece temp = theirPieces[moveToCheck.From];
                    theirPieces.Add(moveToCheck.To, temp);
                    theirPieces.Remove(moveToCheck.From);

                    // If they attacked our piece, remove it from local collection
                    if (myPieces.TryGetValue(moveToCheck.To, out temp))
                    {
                        myPieces.Remove(moveToCheck.To);
                    }
                }
                return true;
            }
            return false;
        }

        private List<ChessMove> getPossibleMoves(ChessBoard board, ChessColor myColor)
        {
            myPieces = null;
            if (myPieces == null)
            {
                myColorForDict = myColor;
                myPieces = new Dictionary<ChessLocation, ChessPiece>();
                theirPieces = new Dictionary<ChessLocation, ChessPiece>();
                for (int x = 0; x < 8; ++x)
                {
                    for (int y = 0; y < 8; ++y)
                    {
                        if (board[x, y] != ChessPiece.Empty)
                        {
                            if ((board[x, y] - ChessPiece.Empty) * ((int)myColor * 2 - 1) > 0)
                            {
                                theirPieces.Add(new ChessLocation(x, y), board[x, y]);
                            }
                            else
                            {
                                myPieces.Add(new ChessLocation(x, y), board[x, y]);
                            }
                        }
                    }
                }
            }
            if (myColor != myColorForDict)
            {
                //it's getting the move set for the other color.  Switch the saved pieces.
                var temp = myPieces;
                myPieces = theirPieces;
                theirPieces = temp;
                myColorForDict = myColor;
            }
            List<ChessMove> possibleMoves = new List<ChessMove>();

            foreach (var piece in myPieces)
            {
                switch ((int)(piece.Value + 1) % 7)
                {
                    case 1: // Pawn
                        possibleMoves.AddRange(PawnMoves(board, piece.Key, myColor));
                        break;
                    case 2: // Rook
                        possibleMoves.AddRange(RookMoves(board, piece.Key, myColor));
                        break;
                    case 3: // Knight
                        possibleMoves.AddRange(KnightMoves(board, piece.Key, myColor));
                        break;
                    case 4: // Bishop
                        possibleMoves.AddRange(BishopMoves(board, piece.Key, myColor));
                        break;
                    case 5: // Queen
                        possibleMoves.AddRange(QueenMoves(board, piece.Key, myColor));
                        break;
                    case 6: // King
                        possibleMoves.AddRange(KingMoves(board, piece.Key, myColor));
                        break;
                }
            }
            foreach(ChessMove move in possibleMoves)
            {
                if(move.Flag == ChessFlag.Check)
                {
                    var movedBoard = board.Clone();
                    movedBoard.MakeMove(move);
                    List<ChessMove> opponentMoves = getPossibleMoves(board, myColor == ChessColor.Black ? ChessColor.White : ChessColor.Black);
                    if(opponentMoves.Count == 0)
                    {
                        move.Flag = ChessFlag.Checkmate;
                        move.ValueOfMove = int.MaxValue;
                    }
                }
            }
            return possibleMoves;
        }
        #endregion

        #region Pawn Moves

        public List<ChessMove> PawnMoves(ChessBoard board, ChessLocation location, ChessColor color)
        {
            List<ChessMove> moves = new List<ChessMove>();
            ChessMove newMove;
            if (color == ChessColor.White)
            {
                if (location.X == 0)
                {
                    if (board[location.X + 1, location.Y - 1] < ChessPiece.Empty) //take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X + 1, location.Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                    }
                    if (board[location.X, location.Y - 1] == ChessPiece.Empty)
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X, location.Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                        if (location.Y == 6) // pawn is in starting position
                        {
                            if (board[location.X, location.Y - 2] == ChessPiece.Empty)
                            {
                                newMove = new ChessMove(location, new ChessLocation(location.X, location.Y - 2));
                                if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                }
                else if (location.X == 7)
                {
                    if (board[location.X - 1, location.Y - 1] < ChessPiece.Empty) //take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X - 1, location.Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                    }
                    if (board[location.X, location.Y - 1] == ChessPiece.Empty)
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X, location.Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                        if (location.Y == 6) // pawn is in starting position
                        {
                            if (board[location.X, location.Y - 2] == ChessPiece.Empty)
                            {
                                newMove = new ChessMove(location, new ChessLocation(location.X, location.Y - 2));
                                if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                }
                else
                {
                    if (board[location.X - 1, location.Y - 1] < ChessPiece.Empty)// take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X - 1, location.Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                    }
                    if (board[location.X, location.Y - 1] == ChessPiece.Empty)
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X, location.Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                        if (location.Y == 6) // pawn is in starting position
                        {
                            if (board[location.X, location.Y - 2] == ChessPiece.Empty)
                            {
                                newMove = new ChessMove(location, new ChessLocation(location.X, location.Y - 2));
                                if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                    if (board[location.X + 1, location.Y - 1] < ChessPiece.Empty) // take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X + 1, location.Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                    }
                }
            }
            else // if color is black
            {
                if (location.X == 0)
                {
                    if (board[location.X + 1, location.Y + 1] > ChessPiece.Empty) //take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X + 1, location.Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                    }
                    if (board[location.X, location.Y + 1] == ChessPiece.Empty)
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X, location.Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                        if (location.Y == 1) // pawn is in starting position
                        {
                            if (board[location.X, location.Y + 2] == ChessPiece.Empty)
                            {
                                newMove = new ChessMove(location, new ChessLocation(location.X, location.Y + 2));
                                if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                }
                else if (location.X == 7)
                {
                    if (board[location.X - 1, location.Y + 1] > ChessPiece.Empty) //take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X - 1, location.Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                    }
                    if (board[location.X, location.Y + 1] == ChessPiece.Empty)
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X, location.Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                        if (location.Y == 1) // pawn is in starting position
                        {
                            if (board[location.X, location.Y + 2] == ChessPiece.Empty)
                            {
                                newMove = new ChessMove(location, new ChessLocation(location.X, location.Y + 2));
                                if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                }
                else
                {
                    if (board[location.X - 1, location.Y + 1] > ChessPiece.Empty)// take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X - 1, location.Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                    }
                    if (board[location.X, location.Y + 1] == ChessPiece.Empty)
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X, location.Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                        if (location.Y == 1) // pawn is in starting position
                        {
                            if (board[location.X, location.Y + 2] == ChessPiece.Empty)
                            {
                                newMove = new ChessMove(location, new ChessLocation(location.X, location.Y + 2));
                                if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                    if (board[location.X + 1, location.Y + 1] > ChessPiece.Empty) // take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(location.X + 1, location.Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                    }
                }
            }
            return moves;
        }

        #endregion

        #region King Moves

        public List<ChessMove> KingMoves(ChessBoard board, ChessLocation location, ChessColor color)
        {
            List<ChessMove> moves = new List<ChessMove>();
            ChessMove newMove;
            if (color == ChessColor.White)
            {
                for (int i = (location.X - 1); i < (location.X + 2); i++)
                {
                    for (int j = (location.Y - 1); j < (location.Y + 2); j++)
                    {
                        if (i < 0 || i > 7 || j < 0 || j > 7) { continue; } // do nothing
                        if (i == location.X && j == location.Y) { continue; } //also, do nothing
                        if (board[i, j] <= ChessPiece.Empty)
                        {
                            newMove = new ChessMove(location, new ChessLocation(i, j));
                            if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }

                        }
                    }
                }
            }
            else // the color is black
            {
                for (int i = (location.X - 1); i < (location.X + 2); i++)
                {
                    for (int j = (location.Y - 1); j < (location.Y + 2); j++)
                    {
                        if (i < 0 || i > 7 || j < 0 || j > 7) { continue; } // do nothing
                        if (i == location.X && j == location.Y) { continue; } //also, do nothing
                        if (board[i, j] >= ChessPiece.Empty)
                        {
                            newMove = new ChessMove(location, new ChessLocation(i, j));
                            if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                        }
                    }
                }
            }
            return moves;
        }

        #endregion

        #region Rook Moves
        /// <summary> Gather the list of valid moves for the Rook </summary>
        /// <param name="board">Current board state</param>
        /// <param name="position">Current position of the rook</param>
        /// <param name="color">What color is this rook</param>
        /// <returns>List of all possible, valid, chess moves this rook can make</returns>
        public List<ChessMove> RookMoves(ChessBoard board, ChessLocation position, ChessColor color)
        {
            int orig_X = position.X;
            int orig_Y = position.Y;

            List<ChessMove> movelist = new List<ChessMove>();
            if (color == ChessColor.Black)
            {
                for (int x = orig_X + 1; x < 8; ++x)
                {
                    if (board[x, orig_Y] != ChessPiece.Empty)
                    {
                        if (board[x, orig_Y] > ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                }
                for (int x = orig_X - 1; x >= 0; --x)
                {
                    if (board[x, orig_Y] != ChessPiece.Empty)
                    {
                        if (board[x, orig_Y] > ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                }
                for (int y = orig_Y + 1; y < 8; ++y)
                {
                    if (board[orig_X, y] != ChessPiece.Empty)
                    {
                        if (board[orig_X, y] > ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                }
                for (int y = orig_Y - 1; y >= 0; --y)
                {
                    if (board[orig_X, y] != ChessPiece.Empty)
                    {
                        if (board[orig_X, y] > ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                }
            }
            else // This is white
            {
                for (int x = orig_X + 1; x < 8; ++x)
                {
                    if (board[x, orig_Y] != ChessPiece.Empty)
                    {
                        if (board[x, orig_Y] < ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                }
                for (int x = orig_X - 1; x >= 0; --x)
                {
                    if (board[x, orig_Y] != ChessPiece.Empty)
                    {
                        if (board[x, orig_Y] < ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                }
                for (int y = orig_Y + 1; y < 8; ++y)
                {
                    if (board[orig_X, y] != ChessPiece.Empty)
                    {
                        if (board[orig_X, y] < ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                }
                for (int y = orig_Y - 1; y >= 0; --y)
                {
                    if (board[orig_X, y] != ChessPiece.Empty)
                    {
                        if (board[orig_X, y] < ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                }
            }
            return movelist;
        }
        #endregion

        #region Bishop Moves
        /// <summary> Gather the list of valid moves for the Bishop </summary>
        /// <param name="board">Current board state</param>
        /// <param name="position">Current position of the bishop</param>
        /// <param name="color">What color is this bishop</param>
        /// <returns>List of all possible, valid, chess moves this bishop can make</returns>
        public List<ChessMove> BishopMoves(ChessBoard board, ChessLocation position, ChessColor color)
        {
            List<ChessMove> movelist = new List<ChessMove>();

            int orig_X = position.X;
            int orig_Y = position.Y;

            if (color == ChessColor.Black)
            {
                int x = orig_X + 1;
                int y = orig_Y + 1;
                for (; x < 8 && y < 8; )
                {
                    if (board[x, y] != ChessPiece.Empty)
                    {
                        if (board[x, y] > ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, y)));
                        }
                        break;
                    }

                    movelist.Add(new ChessMove(position, new ChessLocation(x, y)));

                    ++x;
                    ++y;
                }

                x = orig_X + 1;
                y = orig_Y - 1;
                for (; x < 8 && y >= 0; )
                {
                    if (board[x, y] != ChessPiece.Empty)
                    {
                        if (board[x, y] > ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, y)));
                        }
                        break;
                    }

                    movelist.Add(new ChessMove(position, new ChessLocation(x, y)));

                    ++x;
                    --y;
                }

                x = orig_X - 1;
                y = orig_Y + 1;
                for (; x >= 0 && y < 8; )
                {
                    if (board[x, y] != ChessPiece.Empty)
                    {
                        if (board[x, y] > ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, y)));
                        }
                        break;
                    }

                    movelist.Add(new ChessMove(position, new ChessLocation(x, y)));

                    --x;
                    ++y;
                }

                x = orig_X - 1;
                y = orig_Y - 1;
                for (; x >= 0 && y >= 0; )
                {
                    if (board[x, y] != ChessPiece.Empty)
                    {
                        if (board[x, y] > ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, y)));
                        }
                        break;
                    }

                    movelist.Add(new ChessMove(position, new ChessLocation(x, y)));

                    --x;
                    --y;
                }
            }
            else // This is white
            {
                int x = orig_X + 1;
                int y = orig_Y + 1;
                for (; x < 8 && y < 8; )
                {
                    if (board[x, y] != ChessPiece.Empty)
                    {
                        if (board[x, y] < ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, y)));
                        }
                        break;
                    }

                    movelist.Add(new ChessMove(position, new ChessLocation(x, y)));

                    ++x;
                    ++y;
                }

                x = orig_X + 1;
                y = orig_Y - 1;
                for (; x < 8 && y >= 0; )
                {
                    if (board[x, y] != ChessPiece.Empty)
                    {
                        if (board[x, y] < ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, y)));
                        }
                        break;
                    }

                    movelist.Add(new ChessMove(position, new ChessLocation(x, y)));

                    ++x;
                    --y;
                }

                x = orig_X - 1;
                y = orig_Y + 1;
                for (; x >= 0 && y < 8; )
                {
                    if (board[x, y] != ChessPiece.Empty)
                    {
                        if (board[x, y] < ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, y)));
                        }
                        break;
                    }

                    movelist.Add(new ChessMove(position, new ChessLocation(x, y)));

                    --x;
                    ++y;
                }

                x = orig_X - 1;
                y = orig_Y - 1;
                for (; y >= 0 && x >= 0; )
                {
                    if (board[x, y] != ChessPiece.Empty)
                    {
                        if (board[x, y] < ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, y)));
                        }
                        break;
                    }

                    movelist.Add(new ChessMove(position, new ChessLocation(x, y)));

                    --x;
                    --y;
                }
            }
            return movelist;
        }
        #endregion

        #region Queen Moves
        /// <summary> Gather the list of valid moves for the Queen </summary>
        /// <param name="board">Current board state</param>
        /// <param name="position">Current position of the queen</param>
        /// <param name="color">What color is this queen</param>
        /// <returns>List of all possible, valid, chess moves this queen can make</returns>
        public List<ChessMove> QueenMoves(ChessBoard board, ChessLocation position, ChessColor color)
        {
            List<ChessMove> movelist = new List<ChessMove>();

            int orig_X = position.X;
            int orig_Y = position.Y;

            if (color == ChessColor.Black)
            {
                #region Handling Up and Down Movements
                for (int y = orig_Y + 1; y < 8; ++y)
                {
                    if (board[orig_X, y] != ChessPiece.Empty)
                    {
                        if (board[orig_X, y] > ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                }
                for (int y = orig_Y - 1; y >= 0; --y)
                {
                    if (board[orig_X, y] != ChessPiece.Empty)
                    {
                        if (board[orig_X, y] > ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                }
                #endregion

                #region Handling Movement to the Right
                int yUp = orig_Y;
                int yDown = orig_Y;
                bool UpFlag = true; // Flags set true means we can continue this direction
                bool DownFlag = true;
                bool HFlag = true; // Horizontal movement flag
                for (int x = orig_X + 1; x < 8; ++x)
                {
                    if (HFlag == false && UpFlag == false && DownFlag == false)
                    {
                        break;
                    }

                    if (HFlag)
                    {
                        if (board[x, orig_Y] != ChessPiece.Empty)
                        {
                            if (board[x, orig_Y] > ChessPiece.Empty) // < empty = black | > empty = white
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                            }
                            HFlag = false;
                        }
                        else
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                        }
                    }
                    if (UpFlag)
                    {
                        ++yUp;
                        if (yUp < 8)
                        {
                            if (board[x, yUp] != ChessPiece.Empty)
                            {
                                if (board[x, yUp] > ChessPiece.Empty) // < empty = black | > empty = white
                                {
                                    movelist.Add(new ChessMove(position, new ChessLocation(x, yUp)));
                                }
                                UpFlag = false;
                            }
                            else
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, yUp)));
                            }
                        }
                        else
                        {
                            UpFlag = false;
                        }
                    }
                    if (DownFlag)
                    {
                        --yDown;
                        if (yDown >= 0)
                        {

                            if (board[x, yDown] != ChessPiece.Empty)
                            {
                                if (board[x, yDown] > ChessPiece.Empty) // < empty = black | > empty = white
                                {
                                    movelist.Add(new ChessMove(position, new ChessLocation(x, yDown)));
                                }
                                DownFlag = false;
                            }
                            else
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, yDown)));
                            }
                        }
                        else
                        {
                            DownFlag = false;
                        }
                    }
                }
                #endregion

                #region Handling Movement to the Left
                yUp = orig_Y;
                yDown = orig_Y;
                UpFlag = true; // Flags set true means we can continue this direction
                DownFlag = true;
                HFlag = true; // Horizontal movement flag
                for (int x = orig_X - 1; x >= 0; --x)
                {
                    if (HFlag == false && UpFlag == false && DownFlag == false)
                    {
                        break;
                    }

                    if (HFlag)
                    {
                        if (board[x, orig_Y] != ChessPiece.Empty)
                        {
                            if (board[x, orig_Y] > ChessPiece.Empty) // < empty = black | > empty = white
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                            }
                            HFlag = false;
                        }
                        else
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                        }
                    }
                    if (UpFlag)
                    {
                        ++yUp;
                        if (yUp < 8)
                        {
                            if (board[x, yUp] != ChessPiece.Empty)
                            {
                                if (board[x, yUp] > ChessPiece.Empty) // < empty = black | > empty = white
                                {
                                    movelist.Add(new ChessMove(position, new ChessLocation(x, yUp)));
                                }
                                UpFlag = false;
                            }
                            else
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, yUp)));
                            }
                        }
                        else
                        {
                            UpFlag = false;
                        }
                    }
                    if (DownFlag)
                    {
                        --yDown;
                        if (yDown >= 0)
                        {

                            if (board[x, yDown] != ChessPiece.Empty)
                            {
                                if (board[x, yDown] > ChessPiece.Empty) // < empty = black | > empty = white
                                {
                                    movelist.Add(new ChessMove(position, new ChessLocation(x, yDown)));
                                }
                                DownFlag = false;
                            }
                            else
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, yDown)));
                            }
                        }
                        else
                        {
                            DownFlag = false;
                        }
                    }
                }
                #endregion
            }
            else // This is white
            {
                #region Handling Up and Down Movements
                for (int y = orig_Y + 1; y < 8; ++y)
                {
                    if (board[orig_X, y] != ChessPiece.Empty)
                    {
                        if (board[orig_X, y] < ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                }
                for (int y = orig_Y - 1; y >= 0; --y)
                {
                    if (board[orig_X, y] != ChessPiece.Empty)
                    {
                        if (board[orig_X, y] < ChessPiece.Empty) // < empty = black | > empty = white
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                        }
                        break;
                    }
                    movelist.Add(new ChessMove(position, new ChessLocation(orig_X, y)));
                }
                #endregion

                #region Handling Movement to the Right
                int yUp = orig_Y;
                int yDown = orig_Y;
                bool UpFlag = true; // Flags set true means we can continue this direction
                bool DownFlag = true;
                bool HFlag = true; // Horizontal movement flag
                for (int x = orig_X + 1; x < 8; ++x)
                {
                    if (HFlag == false && UpFlag == false && DownFlag == false)
                    {
                        break;
                    }

                    if (HFlag)
                    {
                        if (board[x, orig_Y] != ChessPiece.Empty)
                        {
                            if (board[x, orig_Y] < ChessPiece.Empty) // < empty = black | > empty = white
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                            }
                            HFlag = false;
                        }
                        else
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                        }
                    }
                    if (UpFlag)
                    {
                        ++yUp;
                        if (yUp < 8)
                        {
                            if (board[x, yUp] != ChessPiece.Empty)
                            {
                                if (board[x, yUp] < ChessPiece.Empty) // < empty = black | > empty = white
                                {
                                    movelist.Add(new ChessMove(position, new ChessLocation(x, yUp)));
                                }
                                UpFlag = false;
                            }
                            else
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, yUp)));
                            }
                        }
                        else
                        {
                            UpFlag = false;
                        }
                    }
                    if (DownFlag)
                    {
                        --yDown;
                        if (yDown >= 0)
                        {

                            if (board[x, yDown] != ChessPiece.Empty)
                            {
                                if (board[x, yDown] < ChessPiece.Empty) // < empty = black | > empty = white
                                {
                                    movelist.Add(new ChessMove(position, new ChessLocation(x, yDown)));
                                }
                                DownFlag = false;
                            }
                            else
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, yDown)));
                            }
                        }
                        else
                        {
                            DownFlag = false;
                        }
                    }
                }
                #endregion

                #region Handling Movement to the Left
                yUp = orig_Y;
                yDown = orig_Y;
                UpFlag = true; // Flags set true means we can continue this direction
                DownFlag = true;
                HFlag = true; // Horizontal movement flag
                for (int x = orig_X - 1; x >= 0; --x)
                {
                    if (HFlag == false && UpFlag == false && DownFlag == false)
                    {
                        break;
                    }

                    if (HFlag)
                    {
                        if (board[x, orig_Y] != ChessPiece.Empty)
                        {
                            if (board[x, orig_Y] < ChessPiece.Empty) // < empty = black | > empty = white
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                            }
                            HFlag = false;
                        }
                        else
                        {
                            movelist.Add(new ChessMove(position, new ChessLocation(x, orig_Y)));
                        }
                    }
                    if (UpFlag)
                    {
                        ++yUp;
                        if (yUp < 8)
                        {
                            if (board[x, yUp] != ChessPiece.Empty)
                            {
                                if (board[x, yUp] < ChessPiece.Empty) // < empty = black | > empty = white
                                {
                                    movelist.Add(new ChessMove(position, new ChessLocation(x, yUp)));
                                }
                                UpFlag = false;
                            }
                            else
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, yUp)));
                            }
                        }
                        else
                        {
                            UpFlag = false;
                        }
                    }
                    if (DownFlag)
                    {
                        --yDown;
                        if (yDown >= 0)
                        {

                            if (board[x, yDown] != ChessPiece.Empty)
                            {
                                if (board[x, yDown] < ChessPiece.Empty) // < empty = black | > empty = white
                                {
                                    movelist.Add(new ChessMove(position, new ChessLocation(x, yDown)));
                                }
                                DownFlag = false;
                            }
                            else
                            {
                                movelist.Add(new ChessMove(position, new ChessLocation(x, yDown)));
                            }
                        }
                        else
                        {
                            DownFlag = false;
                        }
                    }
                }
                #endregion
            }

            return movelist;
        }
        #endregion

        #region Knight Moves
        /// <summary>Returns all valid moves for a knight from a particular position on the board.
        ///  The possible moves are broken up into columns.  2 to left; 1 to left; 1 to right; 2 to right</summary>
        public List<ChessMove> KnightMoves(ChessBoard board, ChessLocation position, ChessColor color) {
            List<ChessMove> moves = new List<ChessMove>();
            if (position.X > 0) { // Can I move to the left?
                if (position.X > 1) { // Can I move 2 to the left?
                    if (position.Y > 0) { // Can I move up 1?
                        if ((board[position.X - 2, position.Y - 1] - ChessPiece.Empty) * ((int)color * 2 - 1) >= 0) { // Checks if it is the opponents piece
                            ChessMove move = new ChessMove(position, new ChessLocation(position.X - 2, position.Y - 1));
                            if (isCheck(board, move, color) >= 0) {
                            moves.Add(move);
                            }
                        }
                    }

                    if (position.Y < 7) { // Can I move down 1?
                        if ((board[position.X - 2, position.Y + 1] - ChessPiece.Empty) * ((int)color * 2 - 1) >= 0) { // Checks if it is the opponents piece
                            ChessMove move = new ChessMove(position, new ChessLocation(position.X - 2, position.Y + 1));
                            if (isCheck(board, move, color) >= 0) {
                            moves.Add(move);
                            }
                        }
                    }
                }

                if (position.Y > 1) { // Can I move up 2?
                    if ((board[position.X - 1, position.Y - 2] - ChessPiece.Empty) * ((int)color * 2 - 1) >= 0) { // Checks if it is the opponents piece
                        ChessMove move = new ChessMove(position, new ChessLocation(position.X - 1, position.Y - 2));
                        if (isCheck(board, move, color) >= 0) {
                        moves.Add(move);
                        }
                    }
                }

                if (position.Y < 6) { // Can I move down 2?
                    if ((board[position.X - 1, position.Y + 2] - ChessPiece.Empty) * ((int)color * 2 - 1) >= 0) { // Checks if it is the opponents piece
                        ChessMove move = new ChessMove(position, new ChessLocation(position.X - 1, position.Y + 2));
                        if (isCheck(board, move, color) >= 0) {
                        moves.Add(move);
                        }
                    }
                }
            }

            if (position.X < 7) { // Can I move to the right?
                if (position.X < 6) { // Can I move 2 to the right?
                    if (position.Y > 0) { // Can I move up 1?
                        if ((board[position.X + 2, position.Y - 1] - ChessPiece.Empty) * ((int)color * 2 - 1) >= 0) { // Checks if it is the opponents piece
                            ChessMove move = new ChessMove(position, new ChessLocation(position.X + 2, position.Y - 1));
                            if (isCheck(board, move, color) >= 0) {
                            moves.Add(move);
                            }
                        }
                    }

                    if (position.Y < 7) { // Can I move down 1?
                        if ((board[position.X + 2, position.Y + 1] - ChessPiece.Empty) * ((int)color * 2 - 1) >= 0) { // Checks if it is the opponents piece
                            ChessMove move = new ChessMove(position, new ChessLocation(position.X + 2, position.Y + 1));
                            if (isCheck(board, move, color) >= 0) {
                                moves.Add(move);
                            }
                        }
                    }
                }

                if (position.Y > 1) { // Can I move up 2?
                    if ((board[position.X + 1, position.Y - 2] - ChessPiece.Empty) * ((int)color * 2 - 1) >= 0) { // Checks if it is the opponents piece
                        ChessMove move = new ChessMove(position, new ChessLocation(position.X + 1, position.Y - 2));
                        if (isCheck(board, move, color) >= 0) {
                        moves.Add(move);
                        }
                    }
                }

                if (position.Y < 6) { // Can I move down 2?
                    if ((board[position.X + 1, position.Y + 2] - ChessPiece.Empty) * ((int)color * 2 - 1) >= 0) { // Checks if it is the opponents piece
                        ChessMove move = new ChessMove(position, new ChessLocation(position.X + 1, position.Y + 2));
                        if (isCheck(board, move, color) >= 0) {
                        moves.Add(move);
                        }
                    }
                }
            }

            return moves;
        } 
        #endregion

        #region Pawn Moves
        //This is the old version of the pawn moves.
        //public List<ChessMove> PawnMoves(ChessBoard board, ChessLocation location, ChessColor color) {
        //    List<ChessMove> moves = new List<ChessMove>();
        //    ChessMove newMove;
        //    if (color == ChessColor.White) {
        //        if (location.X == 0) {
        //            if (board[location.X + 1, location.Y - 1] < ChessPiece.Empty) //take the enemy piece
        //            {
        //                newMove = new ChessMove(location, new ChessLocation(location.X + 1, location.Y - 1));
        //                if (isCheck(board, newMove, ChessColor.White) == 0) { moves.Add(newMove); }
        //                if (isCheck(board, newMove, ChessColor.White) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //            }
        //        }
        //        else if (location.X == 7) {
        //            if (board[location.X - 1, location.Y - 1] < ChessPiece.Empty) //take the enemy piece
        //            {
        //                newMove = new ChessMove(location, new ChessLocation(location.X - 1, location.Y - 1));
        //                if (isCheck(board, newMove, ChessColor.White) == 0) { moves.Add(newMove); }
        //                if (isCheck(board, newMove, ChessColor.White) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //            }
        //        }
        //        else {
        //            if (board[location.X - 1, location.Y - 1] < ChessPiece.Empty)// take the enemy piece
        //            {
        //                newMove = new ChessMove(location, new ChessLocation(location.X - 1, location.Y - 1));
        //                if (isCheck(board, newMove, ChessColor.White) == 0) { moves.Add(newMove); }
        //                if (isCheck(board, newMove, ChessColor.White) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //            }
        //            if (board[location.X, location.Y - 1] == ChessPiece.Empty) {
        //                newMove = new ChessMove(location, new ChessLocation(location.X, location.Y - 1));
        //                if (isCheck(board, newMove, ChessColor.White) == 0) { moves.Add(newMove); }
        //                if (isCheck(board, newMove, ChessColor.White) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //                if (location.Y == 6) // pawn is in starting position
        //                {
        //                    if (board[location.X, location.Y - 2] == ChessPiece.Empty) {
        //                        newMove = new ChessMove(location, new ChessLocation(location.X, location.Y - 2));
        //                        if (isCheck(board, newMove, ChessColor.White) == 0) { moves.Add(newMove); }
        //                        if (isCheck(board, newMove, ChessColor.White) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //                    }
        //                }
        //            }
        //            if (board[location.X + 1, location.Y - 1] < ChessPiece.Empty) // take the enemy piece
        //            {
        //                newMove = new ChessMove(location, new ChessLocation(location.X + 1, location.Y - 1));
        //                if (isCheck(board, newMove, ChessColor.White) == 0) { moves.Add(newMove); }
        //                if (isCheck(board, newMove, ChessColor.White) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //            }
        //        }
        //    }
        //    else // if color is black
        //    {
        //        if (location.X == 0) {
        //            if (board[location.X + 1, location.Y + 1] < ChessPiece.Empty) //take the enemy piece
        //            {
        //                newMove = new ChessMove(location, new ChessLocation(location.X + 1, location.Y + 1));
        //                if (isCheck(board, newMove, ChessColor.Black) == 0) { moves.Add(newMove); }
        //                if (isCheck(board, newMove, ChessColor.Black) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //            }
        //        }
        //        else if (location.X == 7) {
        //            if (board[location.X - 1, location.Y + 1] < ChessPiece.Empty) //take the enemy piece
        //            {
        //                newMove = new ChessMove(location, new ChessLocation(location.X - 1, location.Y + 1));
        //                if (isCheck(board, newMove, ChessColor.Black) == 0) { moves.Add(newMove); }
        //                if (isCheck(board, newMove, ChessColor.Black) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //            }
        //        }
        //        else {
        //            if (board[location.X - 1, location.Y + 1] < ChessPiece.Empty)// take the enemy piece
        //            {
        //                newMove = new ChessMove(location, new ChessLocation(location.X - 1, location.Y + 1));
        //                if (isCheck(board, newMove, ChessColor.Black) == 0) { moves.Add(newMove); }
        //                if (isCheck(board, newMove, ChessColor.Black) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //            }
        //            if (board[location.X, location.Y + 1] == ChessPiece.Empty) {
        //                newMove = new ChessMove(location, new ChessLocation(location.X, location.Y + 1));
        //                if (isCheck(board, newMove, ChessColor.Black) == 0) { moves.Add(newMove); }
        //                if (isCheck(board, newMove, ChessColor.Black) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //                if (location.Y == 1) // pawn is in starting position
        //                {
        //                    if (board[location.X, location.Y + 2] == ChessPiece.Empty) {
        //                        newMove = new ChessMove(location, new ChessLocation(location.X, location.Y + 2));
        //                        if (isCheck(board, newMove, ChessColor.Black) == 0) { moves.Add(newMove); }
        //                        if (isCheck(board, newMove, ChessColor.Black) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //                    }
        //                }
        //            }
        //            if (board[location.X + 1, location.Y + 1] < ChessPiece.Empty) // take the enemy piece
        //            {
        //                newMove = new ChessMove(location, new ChessLocation(location.X + 1, location.Y + 1));
        //                if (isCheck(board, newMove, ChessColor.Black) == 0) { moves.Add(newMove); }
        //                if (isCheck(board, newMove, ChessColor.Black) == 1) { newMove.Flag = ChessFlag.Check; moves.Add(newMove); }
        //            }
        //        }
        //    }
        //    return moves;
        //} 
        #endregion

        #region Flag functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="before"></param>
        /// <param name="move"></param>
        /// <param name="color"></param>
        /// <returns> 0 if no check. -1 if check against color of move, 1 if check for player of move.</returns>
        public int isCheck(ChessBoard before, ChessMove move, ChessColor color)
        {
            before = before.Clone();
            before.MakeMove(move);
            return isCheckHelper(before, color, move);
        }
        
        public int isCheckHelper(ChessBoard before, ChessColor color, ChessMove move)
        {
            
            int x = 0;
            int y = 0;
            int checkValue = 0;
            bool checkedBlack = false;
            bool checkedWhite = false;
            int whiteTotal = 0;
            int blackTotal = 0;
            while (x < 8)//&&  (!checkedBlack || !checkedWhite))
            {
                while (y < 8)//&& (!checkedBlack || !checkedWhite))
                {
                    var piece = before[x, y];
                    bool check = false;
                    switch (piece)
                    {
                        case ChessPiece.WhiteKing:
                            whiteTotal += 100;
                            checkedWhite = true;
                            do
                            {
                                int tempx = x;
                                int tempy = y;
                                //pretend the piece is a queen.  If it can attack any black piece, check if that piece can attack it, if so, it's in check.
                                {
                                    //there are 8 directions a queen can move.                                    
                                    //up
                                    tempx = x;
                                    tempy = y - 1;
                                    while (tempy >= 0 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving up!
                                        tempy--;
                                    }
                                    if (tempy >= 0)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.BlackRook:
                                                case ChessPiece.BlackQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.BlackKing:
                                                    if (y - tempy == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //down
                                    tempx = x;
                                    tempy = y + 1;
                                    while (tempy < 8 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving down
                                        tempy++;
                                    }
                                    if (tempy < 8)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.BlackRook:
                                                case ChessPiece.BlackQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.BlackKing:
                                                    if (tempy - y == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //left
                                    tempx = x - 1;
                                    tempy = y;
                                    while (tempx >= 0 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving left
                                        tempx--;
                                    }
                                    if (tempx >= 0)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.BlackRook:
                                                case ChessPiece.BlackQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.BlackKing:
                                                    if (x - tempx == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //right
                                    tempx = x + 1;
                                    tempy = y;
                                    while (tempx < 8 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving right
                                        tempx++;
                                    }
                                    if (tempx < 8)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.BlackRook:
                                                case ChessPiece.BlackQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.BlackKing:
                                                    if (tempx - x == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //upleft
                                    tempx = x - 1;
                                    tempy = y - 1;
                                    while (tempx >= 0 && tempy >= 0 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving up left
                                        tempy--;
                                        tempx--;
                                    }
                                    if (tempx >= 0 && tempy >= 0)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.BlackBishop:
                                                case ChessPiece.BlackQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.BlackPawn:
                                                case ChessPiece.BlackKing:
                                                    if (x - tempx == 1 && y - tempy == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //upright
                                    tempx = x + 1;
                                    tempy = y - 1;
                                    while (tempx < 8 && tempy >= 0 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving up right
                                        tempy--;
                                        tempx++;
                                    }
                                    if (tempx < 8 && tempy >= 0)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.BlackBishop:
                                                case ChessPiece.BlackQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.BlackPawn:
                                                case ChessPiece.BlackKing:
                                                    if (tempx - x == 1 && tempy - x == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //downleft
                                    tempx = x - 1;
                                    tempy = y + 1;
                                    while (tempx >= 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving down left
                                        tempy++;
                                        tempx--;
                                    }
                                    if (tempx >= 0 && tempy < 8)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.BlackBishop:
                                                case ChessPiece.BlackQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.BlackKing:
                                                    if (x - tempx == tempy - y && x - tempx == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //downright
                                    tempx = x + 1;
                                    tempy = y + 1;
                                    while (tempx < 8 && tempy < 8 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving  down right
                                        tempy++;
                                        tempx++;
                                    }
                                    if (tempx < 8 && tempy < 8)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.BlackBishop:
                                                case ChessPiece.BlackQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.BlackKing:
                                                    if (tempx - x == tempy - y && tempx - x == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }

                                //pretend the piece is a knight.  If it can attack any black knight, it's in check.
                                if (!check)
                                {
                                    //there are 8 moves a knight can make.
                                    tempx = x + 2;
                                    tempy = y + 1;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.BlackKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 2;
                                    tempy = y - 1;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.BlackKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 2;
                                    tempy = y + 1;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.BlackKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 2;
                                    tempy = y - 1;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.BlackKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 1;
                                    tempy = y + 2;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.BlackKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 1;
                                    tempy = y + 2;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.BlackKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 1;
                                    tempy = y - 2;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.BlackKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 1;
                                    tempy = y - 2;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.BlackKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                }
                            }
                            while (false);  //only repeats it once, however, I can leave the code at any time with a break;

                            if (check && checkValue >= 0)
                            {
                                checkValue = color == ChessColor.White ? -1 : 1;
                            }
                            break;
                        case ChessPiece.BlackKing:
                            blackTotal += 100;
                            checkedBlack = true;
                            do
                            {
                                int tempx = x;
                                int tempy = y;
                                //pretend the piece is a queen.  If it can attack any white piece, check if that piece can attack it, if so, it's in check.
                                {
                                    //there are 8 directions a queen can move.                                    
                                    //up
                                    tempx = x;
                                    tempy = y - 1;
                                    while (tempy >= 0 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving up!
                                        tempy--;
                                    }
                                    if (tempy >= 0)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking > ChessPiece.Empty)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.WhiteRook:
                                                case ChessPiece.WhiteQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.WhiteKing:
                                                    if (y - tempy == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //down
                                    tempx = x;
                                    tempy = y + 1;
                                    while (tempy < 8 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving down
                                        tempy++;
                                    }
                                    if (tempy < 8)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.WhiteRook:
                                                case ChessPiece.WhiteQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.WhiteKing:
                                                    if (tempy - y == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //left
                                    tempx = x - 1;
                                    tempy = y;
                                    while (tempx >= 0 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving left
                                        tempx--;
                                    }
                                    if (tempx >= 0)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.WhiteRook:
                                                case ChessPiece.WhiteQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.WhiteKing:
                                                    if (x - tempx == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //right
                                    tempx = x + 1;
                                    tempy = y;
                                    while (tempx < 8 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving right
                                        tempx++;
                                    }
                                    if (tempx < 8)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.WhiteRook:
                                                case ChessPiece.WhiteQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.WhiteKing:
                                                    if (tempx - x == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //upleft
                                    tempx = x - 1;
                                    tempy = y - 1;
                                    while (tempx >= 0 && tempy >= 0 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving up left
                                        tempy--;
                                        tempx--;
                                    }
                                    if (tempx >= 0 && tempy >= 0)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.WhiteBishop:
                                                case ChessPiece.WhiteQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.WhiteKing:
                                                    if (x - tempx == 1 && y - tempy == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //upright
                                    tempx = x + 1;
                                    tempy = y - 1;
                                    while (tempx < 8 && tempy >= 0 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving up right
                                        tempy--;
                                        tempx++;
                                    }
                                    if (tempx < 8 && tempy >= 0)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.WhiteBishop:
                                                case ChessPiece.WhiteQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.WhiteKing:
                                                    if (tempx - x == 1 && tempy - x == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //downleft
                                    tempx = x - 1;
                                    tempy = y + 1;
                                    while (tempx >= 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving down left
                                        tempy++;
                                        tempx--;
                                    }
                                    if (tempx >= 0 && tempy < 8)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.WhiteBishop:
                                                case ChessPiece.WhiteQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.WhitePawn:
                                                case ChessPiece.WhiteKing:
                                                    if (x - tempx == tempy - y && x - tempx == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    //downright
                                    tempx = x + 1;
                                    tempy = y + 1;
                                    while (tempx < 8 && tempy < 8 && before[tempx, tempy] == ChessPiece.Empty)
                                    {
                                        //keep moving  down right
                                        tempy++;
                                        tempx++;
                                    }
                                    if (tempx < 8 && tempy < 8)
                                    {
                                        var attacking = before[tempx, tempy];
                                        if (attacking < ChessPiece.Empty)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case ChessPiece.WhiteBishop:
                                                case ChessPiece.WhiteQueen:
                                                    check = true;
                                                    break;
                                                case ChessPiece.WhitePawn:
                                                case ChessPiece.WhiteKing:
                                                    if (tempx - x == tempy - y && tempx - x == 1)
                                                    {
                                                        check = true;
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (check)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }

                                //pretend the piece is a knight.  If it can attack any white knight, it's in check.
                                if (!check)
                                {
                                    //there are 8 moves a knight can make.
                                    tempx = x + 2;
                                    tempy = y + 1;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.WhiteKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 2;
                                    tempy = y - 1;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.WhiteKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 2;
                                    tempy = y + 1;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.WhiteKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 2;
                                    tempy = y - 1;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.WhiteKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 1;
                                    tempy = y + 2;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.WhiteKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 1;
                                    tempy = y + 2;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.WhiteKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 1;
                                    tempy = y - 2;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.WhiteKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 1;
                                    tempy = y - 2;
                                    if (tempx > 0 && tempx < 8 && tempy > 0 && tempy < 8 && before[tempx, tempy] == ChessPiece.WhiteKnight)
                                    {
                                        check = true;
                                        break;
                                    }
                                }
                            }
                            while (false);  //only repeats it once, however, I can leave the code at any time with a break;
                            if (check && checkValue >= 0)
                            {
                                checkValue = color == ChessColor.Black ? -1 : 1;

                            }
                            break;
                        case ChessPiece.BlackBishop:
                            blackTotal += 4;
                            break;
                        case ChessPiece.BlackKnight:
                            blackTotal += 3;
                            break;
                        case ChessPiece.BlackPawn:
                            blackTotal += 1;
                            break;
                        case ChessPiece.BlackQueen:
                            blackTotal += 9;
                            break;
                        case ChessPiece.BlackRook:
                            blackTotal += 5;
                            break;
                        case ChessPiece.WhiteBishop:
                            whiteTotal += 4;
                            break;
                        case ChessPiece.WhiteKnight:
                            whiteTotal += 3;
                            break;
                        case ChessPiece.WhitePawn:
                            whiteTotal += 1;
                            break;
                        case ChessPiece.WhiteQueen:
                            whiteTotal += 9;
                            break;
                        case ChessPiece.WhiteRook:
                            whiteTotal += 5;
                            break;
                        default:
                            break;
                    }
                    y++;
                }
                y = 0;
                x++;
            }
            if (!checkedBlack)
            {
                //the black king is dead.
                if (color == ChessColor.Black)
                {
                    checkValue = -1;
                }
                else
                {
                    checkValue = 1;
                }
            }
            if (!checkedWhite)
            {
                //the white king is dead.
                if (color == ChessColor.White)
                {
                    checkValue = -1;
                }
                else
                {
                    checkValue = 1;
                }
            }
            move.ValueOfMove = color == ChessColor.Black ? blackTotal - whiteTotal : whiteTotal - blackTotal;
            if (checkValue > 0)
            {
                move.Flag = ChessFlag.Check;
                move.ValueOfMove += 30;  //30 points due to the king being in check.
            }

            return checkValue;
        }
        #endregion
        #endregion














        #region IChessAI Members that should be implemented as automatic properties and should NEVER be touched by students.
        /// <summary>
        /// This will return false when the framework starts running your AI. When the AI's time has run out,
        /// then this method will return true. Once this method returns true, your AI should return a 
        /// move immediately.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        public AIIsMyTurnOverCallback IsMyTurnOver { get; set; }

        /// <summary>
        /// Call this method to print out debug information. The framework subscribes to this event
        /// and will provide a log window for your debug messages.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AILoggerCallback Log { get; set; }

        /// <summary>
        /// Call this method to catch profiling information. The framework subscribes to this event
        /// and will print out the profiling stats in your log window.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="key"></param>
        public AIProfiler Profiler { get; set; }

        /// <summary>
        /// Call this method to tell the framework what decision print out debug information. The framework subscribes to this event
        /// and will provide a debug window for your decision tree.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AISetDecisionTreeCallback SetDecisionTree { get; set; }
        #endregion
    }
}
