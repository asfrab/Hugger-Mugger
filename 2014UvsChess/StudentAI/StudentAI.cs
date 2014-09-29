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
            get { return "Hugger-Mugger (Debug)"; }
#else
            get { return "Hugger-Mugger"; }
#endif
        }

        #region Main Methods
        string ModifiedFen = "rnbqkbnrpppppppp________________________________PPPPPPPPRNBQKBNR";
        const char EMPTY_SPACE = '_';

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
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
            string fen = BoardToModifiedFen(board);
            List<ChessMove> possibleMoves = getPossibleMoves(fen, myColor);
            foreach (ChessMove move in possibleMoves)
            {
                var movedBoard = MakeMove(fen, move);
                List<ChessMove> opponentMoves = getPossibleMoves(movedBoard, myColor == ChessColor.Black ? ChessColor.White : ChessColor.Black);


                if (opponentMoves.Count == 0)
                {
                    if (move.Flag == ChessFlag.Check)
                    {
                        move.ValueOfMove = 100000;
                        move.Flag = ChessFlag.Checkmate;
                    }
                    else
                    {
                        move.ValueOfMove = -100;
                        move.Flag = ChessFlag.Stalemate;
                    }
                }
                else
                {
                    int opponentBest = int.MinValue;
                    foreach (var opMove in opponentMoves)
                    {
                        if (opMove.ValueOfMove > opponentBest)
                        {
                            opponentBest = opMove.ValueOfMove;
                        }
                    }
                    move.ValueOfMove = -opponentBest;

                }
            }
            ChessMove moveToMake;
            ChessPiece pieceToMove;

            // If there are moves to be made choose one at random
            if (possibleMoves.Count > 0)
            {
                possibleMoves.Sort((a, b) => b.ValueOfMove.CompareTo(a.ValueOfMove));
                int highestVal = int.MinValue;
                int pos = 0;
                foreach (var move in possibleMoves)
                {
                    if (move.ValueOfMove > highestVal)
                    {
                        highestVal = move.ValueOfMove;
                    }
                    else if (move.ValueOfMove < highestVal)
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
                //pieceToMove = myPieces[moveToMake.From];

                //if ((pieceToMove == ChessPiece.WhitePawn || pieceToMove == ChessPiece.BlackPawn) && (moveToMake.To.Y == 0 || moveToMake.To.Y == 7)) {
                //    pieceToMove = myColor == ChessColor.Black ? ChessPiece.BlackQueen : ChessPiece.WhiteQueen;
                //}
                //myPieces.Add(moveToMake.To, pieceToMove);
                //myPieces.Remove(moveToMake.From);

                //// If we attacked their piece, remove it from collection
                //if (theirPieces.TryGetValue(moveToMake.To, out pieceToMove)) {
                //    theirPieces.Remove(moveToMake.To);
                //}
            }
            else
            { // No moves left.  Declare stalemate
                moveToMake = new ChessMove(null, null, ChessFlag.Stalemate);
            }

            if (moveToMake.From != null)
                ModifiedFen = MakeMove(ModifiedFen, moveToMake);

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
            string fen = BoardToModifiedFen(boardBeforeMove);
            List<ChessMove> possibleMoves = getPossibleMoves(fen, colorOfPlayerMoving);
            foreach (ChessMove move in possibleMoves)
            {
                if (move.Flag == ChessFlag.Check)
                {
                    var movedBoard = MakeMove(fen, move);
                    List<ChessMove> opponentMoves = getPossibleMoves(movedBoard, colorOfPlayerMoving == ChessColor.Black ? ChessColor.White : ChessColor.Black);
                    if (opponentMoves.Count == 0)
                    {
                        move.Flag = ChessFlag.Checkmate;
                        move.ValueOfMove = int.MaxValue;
                    }
                }
            }
            var tempDict = myPieces;
            myPieces = theirPieces;
            theirPieces = tempDict;
            myColorForDict = colorOfPlayerMoving;

            if (possibleMoves.Contains(moveToCheck))
            {
                // Change the position of the opponents piece in local collection 
                //if (myPieces != null)
                //{
                //    ChessPiece temp = theirPieces[moveToCheck.From];
                //    theirPieces.Add(moveToCheck.To, temp);
                //    theirPieces.Remove(moveToCheck.From);

                //    // If they attacked our piece, remove it from local collection
                //    if (myPieces.TryGetValue(moveToCheck.To, out temp))
                //    {
                //        myPieces.Remove(moveToCheck.To);
                //    }
                //}
                ModifiedFen = MakeMove(ModifiedFen, moveToCheck);

                return true;
            }
            return false;
        }

        private List<ChessMove> getPossibleMoves(string board, ChessColor myColor)
        {
            List<ChessMove> possibleMoves = new List<ChessMove>();
            if (myColor == ChessColor.White)
            {
                for (int i = 0; i < 64; ++i)
                {
                    if (board[i] != '_' && char.IsUpper(board[i]))
                    {
                        switch (board[i])
                        {
                            case 'P': // Pawn
                                possibleMoves.AddRange(PawnMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                            case 'R': // Rook
                                possibleMoves.AddRange(RookMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                            case 'N': // Knight
                                possibleMoves.AddRange(KnightMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                            case 'B': // Bishop
                                possibleMoves.AddRange(BishopMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                            case 'Q': // Queen
                                possibleMoves.AddRange(QueenMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                            case 'K': // King
                                possibleMoves.AddRange(KingMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 64; ++i)
                {
                    if (board[i] != '_' && char.IsLower(board[i]))
                    {
                        switch (board[i])
                        {
                            case 'p': // Pawn
                                possibleMoves.AddRange(PawnMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                            case 'r': // Rook
                                possibleMoves.AddRange(RookMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                            case 'n': // Knight
                                possibleMoves.AddRange(KnightMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                            case 'b': // Bishop
                                possibleMoves.AddRange(BishopMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                            case 'q': // Queen
                                possibleMoves.AddRange(QueenMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                            case 'k': // King
                                possibleMoves.AddRange(KingMoves(board, new ChessLocation(i % 8, i / 8), myColor));
                                break;
                        }
                    }
                }

            }
            return possibleMoves;
        }
        #endregion

        #region Fen Methods
        public string MakeMove(string fen, ChessMove move)
        {
            StringBuilder newFen = new StringBuilder(fen);
            int fromIndex = move.From.X % 8 + move.From.Y * 8;
            int toIndex = move.To.X % 8 + move.To.Y * 8;
            newFen[toIndex] = newFen[fromIndex];
            newFen[fromIndex] = '_';
            if (toIndex < 8 && newFen[toIndex] == 'P')
            {
                newFen[toIndex] = 'Q';
            }
            else if (toIndex >= 56 && newFen[toIndex] == 'p')
            {
                newFen[toIndex] = 'q';
            }
            //this.Log(newFen.ToString());
            return newFen.ToString();
        }

        public string BoardToModifiedFen(ChessBoard board)
        {
            StringBuilder fen = new StringBuilder(64);
            board.ToPartialFenBoard();
            for (int y = 0; y < 8; ++y)
            {
                for (int x = 0; x < 8; ++x)
                {
                    switch (board[x, y])
                    {
                        case ChessPiece.WhitePawn:
                            fen.Append('P');
                            break;
                        case ChessPiece.WhiteRook:
                            fen.Append('R');
                            break;
                        case ChessPiece.WhiteKnight:
                            fen.Append('N');
                            break;
                        case ChessPiece.WhiteBishop:
                            fen.Append('B');
                            break;
                        case ChessPiece.WhiteQueen:
                            fen.Append('Q');
                            break;
                        case ChessPiece.WhiteKing:
                            fen.Append('K');
                            break;
                        case ChessPiece.BlackPawn:
                            fen.Append('p');
                            break;
                        case ChessPiece.BlackRook:
                            fen.Append('r');
                            break;
                        case ChessPiece.BlackKnight:
                            fen.Append('n');
                            break;
                        case ChessPiece.BlackBishop:
                            fen.Append('b');
                            break;
                        case ChessPiece.BlackQueen:
                            fen.Append('q');
                            break;
                        case ChessPiece.BlackKing:
                            fen.Append('k');
                            break;
                        case ChessPiece.Empty:
                            fen.Append('_');
                            continue;
                    }

                }
            }
            return fen.ToString();
        }
        #endregion

        #region Pawn Moves
        public List<ChessMove> PawnMoves(string board, ChessLocation location, ChessColor color)
        {
            List<ChessMove> moves = new List<ChessMove>();
            ChessMove newMove;
            int X = location.X;
            int Y = location.Y;
            if (color == ChessColor.White)
            {
                if (X == 0)
                {
                    if (board[(X + 1) % 8 + (Y - 1) * 8] > EMPTY_SPACE) //take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(X + 1, Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                    }
                    if (board[X % 8 + (Y - 1) * 8] == EMPTY_SPACE)
                    {
                        newMove = new ChessMove(location, new ChessLocation(X, Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                        if (Y == 6) // pawn is in starting position
                        {
                            if (board[X % 8 + (Y - 2) * 8] == EMPTY_SPACE)
                            {
                                newMove = new ChessMove(location, new ChessLocation(X, Y - 2));
                                if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                }
                else if (X == 7)
                {
                    if (board[(X - 1) % 8 + (Y - 1) * 8] > EMPTY_SPACE) //take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(X - 1, Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                    }
                    if (board[(X) % 8 + (Y - 1) * 8] == EMPTY_SPACE)
                    {
                        newMove = new ChessMove(location, new ChessLocation(X, Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                        if (Y == 6) // pawn is in starting position
                        {
                            if (board[(X) % 8 + (Y - 2) * 8] == EMPTY_SPACE)
                            {
                                newMove = new ChessMove(location, new ChessLocation(X, Y - 2));
                                if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                }
                else
                {
                    if (board[(X - 1) % 8 + (Y - 1) * 8] > EMPTY_SPACE)// take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(X - 1, Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                    }
                    if (board[(X) % 8 + (Y - 1) * 8] == EMPTY_SPACE)
                    {
                        newMove = new ChessMove(location, new ChessLocation(X, Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                        if (Y == 6) // pawn is in starting position
                        {
                            if (board[(X) % 8 + (Y - 2) * 8] == EMPTY_SPACE)
                            {
                                newMove = new ChessMove(location, new ChessLocation(X, Y - 2));
                                if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                    if (board[(X + 1) % 8 + (Y - 1) * 8] > EMPTY_SPACE) // take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(X + 1, Y - 1));
                        if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }
                    }
                }
            }
            else // if color is black
            {
                if (X == 0)
                {
                    if (board[(X + 1) % 8 + (Y + 1) * 8] < EMPTY_SPACE) //take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(X + 1, Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                    }
                    if (board[(X) % 8 + (Y + 1) * 8] == EMPTY_SPACE)
                    {
                        newMove = new ChessMove(location, new ChessLocation(X, Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                        if (Y == 1) // pawn is in starting position
                        {
                            if (board[(X) % 8 + (Y + 2) * 8] == EMPTY_SPACE)
                            {
                                newMove = new ChessMove(location, new ChessLocation(X, Y + 2));
                                if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                }
                else if (X == 7)
                {
                    if (board[(X - 1) % 8 + (Y + 1) * 8] < EMPTY_SPACE) //take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(X - 1, Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                    }
                    if (board[(X) % 8 + (Y + 1) * 8] == EMPTY_SPACE)
                    {
                        newMove = new ChessMove(location, new ChessLocation(X, Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                        if (Y == 1) // pawn is in starting position
                        {
                            if (board[(X) % 8 + (Y + 2) * 8] == EMPTY_SPACE)
                            {
                                newMove = new ChessMove(location, new ChessLocation(X, Y + 2));
                                if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                }
                else
                {
                    if (board[(X - 1) % 8 + (Y + 1) * 8] < EMPTY_SPACE)// take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(X - 1, Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                    }
                    if (board[(X) % 8 + (Y + 1) * 8] == EMPTY_SPACE)
                    {
                        newMove = new ChessMove(location, new ChessLocation(X, Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                        if (Y == 1) // pawn is in starting position
                        {
                            if (board[(X) % 8 + (Y + 2) * 8] == EMPTY_SPACE)
                            {
                                newMove = new ChessMove(location, new ChessLocation(X, Y + 2));
                                if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                            }
                        }
                    }
                    if (board[(X + 1) % 8 + (Y + 1) * 8] < EMPTY_SPACE) // take the enemy piece
                    {
                        newMove = new ChessMove(location, new ChessLocation(X + 1, Y + 1));
                        if (isCheck(board, newMove, ChessColor.Black) >= 0) { moves.Add(newMove); }
                    }
                }
            }
            return moves;
        }

        #endregion

        #region King Moves
        public List<ChessMove> KingMoves(string board, ChessLocation location, ChessColor color)
        {
            List<ChessMove> moves = new List<ChessMove>();
            ChessMove newMove;
            int X = location.X;
            int Y = location.Y;
            if (color == ChessColor.White)
            {
                for (int i = (X - 1); i < (X + 2); i++)
                {
                    for (int j = (Y - 1); j < (Y + 2); j++)
                    {
                        if (i < 0 || i > 7 || j < 0 || j > 7) { continue; } // do nothing
                        if (i == X && j == Y) { continue; } //also, do nothing
                        if (board[i % 8 + j * 8] >= EMPTY_SPACE)
                        {
                            newMove = new ChessMove(location, new ChessLocation(i, j));
                            if (isCheck(board, newMove, ChessColor.White) >= 0) { moves.Add(newMove); }

                        }
                    }
                }
            }
            else // the color is black
            {
                for (int i = (X - 1); i < (X + 2); i++)
                {
                    for (int j = (Y - 1); j < (Y + 2); j++)
                    {
                        if (i < 0 || i > 7 || j < 0 || j > 7) { continue; } // do nothing
                        if (i == X && j == Y) { continue; } //also, do nothing
                        if (board[i % 8 + j * 8] <= EMPTY_SPACE)
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
        public List<ChessMove> RookMoves(string board, ChessLocation position, ChessColor color)
        {
            int orig_X = position.X;
            int orig_Y = position.Y;
            ChessMove move;

            List<ChessMove> movelist = new List<ChessMove>();
            if (color == ChessColor.Black)
            {
                for (int x = orig_X + 1; x < 8; ++x)
                {
                    if (board[x % 8 + orig_Y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + orig_Y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, orig_Y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(x, orig_Y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
                }
                for (int x = orig_X - 1; x >= 0; --x)
                {
                    if (board[x % 8 + orig_Y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + orig_Y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, orig_Y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(x, orig_Y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
                }
                for (int y = orig_Y + 1; y < 8; ++y)
                {
                    if (board[orig_X % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[orig_X % 8 + y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(orig_X, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(orig_X, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
                }
                for (int y = orig_Y - 1; y >= 0; --y)
                {
                    if (board[orig_X % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[orig_X % 8 + y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(orig_X, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(orig_X, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
                }
            }
            else // This is white
            {
                for (int x = orig_X + 1; x < 8; ++x)
                {
                    int pos = x % 8 + orig_Y * 8;
                    char boardspace = board[pos];
                    if (board[x % 8 + orig_Y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + orig_Y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, orig_Y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(x, orig_Y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
                }
                for (int x = orig_X - 1; x >= 0; --x)
                {
                    int pos = x % 8 + orig_Y * 8;
                    char boardspace = board[pos];
                    if (board[x % 8 + orig_Y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + orig_Y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, orig_Y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(x, orig_Y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
                }
                for (int y = orig_Y + 1; y < 8; ++y)
                {
                    int pos = orig_X % 8 + y * 8;
                    char boardspace = board[pos];
                    if (board[orig_X % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[orig_X % 8 + y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(orig_X, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(orig_X, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
                }
                for (int y = orig_Y - 1; y >= 0; --y)
                {
                    int pos = orig_X % 8 + y * 8;
                    char boardspace = board[pos];
                    if (board[orig_X % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[orig_X % 8 + y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(orig_X, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(orig_X, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
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
        public List<ChessMove> BishopMoves(string board, ChessLocation position, ChessColor color)
        {
            List<ChessMove> movelist = new List<ChessMove>();

            int orig_X = position.X;
            int orig_Y = position.Y;
            ChessMove move;

            if (color == ChessColor.Black)
            {
                int x = orig_X + 1;
                int y = orig_Y + 1;


                for (; x < 8 && y < 8; )
                {
                    if (board[x % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }

                    move = new ChessMove(position, new ChessLocation(x, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }

                    ++x;
                    ++y;
                }

                x = orig_X + 1;
                y = orig_Y - 1;
                for (; x < 8 && y >= 0; )
                {
                    if (board[x % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(x, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }

                    ++x;
                    --y;
                }

                x = orig_X - 1;
                y = orig_Y + 1;
                for (; x >= 0 && y < 8; )
                {
                    if (board[x % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }

                    move = new ChessMove(position, new ChessLocation(x, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }

                    --x;
                    ++y;
                }

                x = orig_X - 1;
                y = orig_Y - 1;
                for (; x >= 0 && y >= 0; )
                {
                    if (board[x % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(x, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }

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
                    if (board[x % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }

                    move = new ChessMove(position, new ChessLocation(x, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }

                    ++x;
                    ++y;
                }

                x = orig_X + 1;
                y = orig_Y - 1;
                for (; x < 8 && y >= 0; )
                {
                    if (board[x % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(x, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }

                    ++x;
                    --y;
                }

                x = orig_X - 1;
                y = orig_Y + 1;
                for (; x >= 0 && y < 8; )
                {
                    if (board[x % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }

                    move = new ChessMove(position, new ChessLocation(x, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }

                    --x;
                    ++y;
                }

                x = orig_X - 1;
                y = orig_Y - 1;
                for (; y >= 0 && x >= 0; )
                {
                    if (board[x % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[x % 8 + y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(x, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }

                    move = new ChessMove(position, new ChessLocation(x, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }

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
        public List<ChessMove> QueenMoves(string board, ChessLocation position, ChessColor color)
        {
            List<ChessMove> movelist = new List<ChessMove>();

            int orig_X = position.X;
            int orig_Y = position.Y;
            ChessMove move;

            if (color == ChessColor.Black)
            {
                #region Handling Up and Down Movements
                for (int y = orig_Y + 1; y < 8; ++y)
                {
                    if (board[orig_X % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[orig_X % 8 + y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(orig_X, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(orig_X, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
                }
                for (int y = orig_Y - 1; y >= 0; --y)
                {
                    if (board[orig_X % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[orig_X % 8 + y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(orig_X, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(orig_X, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
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
                        if (board[x % 8 + orig_Y * 8] != EMPTY_SPACE)
                        {
                            if (board[x % 8 + orig_Y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                            {
                                move = new ChessMove(position, new ChessLocation(x, orig_Y));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
                            }
                            HFlag = false;
                        }
                        else
                        {
                            move = new ChessMove(position, new ChessLocation(x, orig_Y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                    }
                    if (UpFlag)
                    {
                        ++yUp;
                        if (yUp < 8)
                        {
                            if (board[x % 8 + yUp * 8] != EMPTY_SPACE)
                            {
                                if (board[x % 8 + yUp * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                                {
                                    move = new ChessMove(position, new ChessLocation(x, yUp));
                                    if (isCheck(board, move, color) >= 0)
                                    {
                                        movelist.Add(move);
                                    }
                                }
                                UpFlag = false;
                            }
                            else
                            {
                                move = new ChessMove(position, new ChessLocation(x, yUp));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
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

                            if (board[x % 8 + yDown * 8] != EMPTY_SPACE)
                            {
                                if (board[x % 8 + yDown * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                                {
                                    move = new ChessMove(position, new ChessLocation(x, yDown));
                                    if (isCheck(board, move, color) >= 0)
                                    {
                                        movelist.Add(move);
                                    }
                                }
                                DownFlag = false;
                            }
                            else
                            {
                                move = new ChessMove(position, new ChessLocation(x, yDown));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
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
                        if (board[x % 8 + orig_Y * 8] != EMPTY_SPACE)
                        {
                            if (board[x % 8 + orig_Y * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                            {
                                move = new ChessMove(position, new ChessLocation(x, orig_Y));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
                            }
                            HFlag = false;
                        }
                        else
                        {
                            move = new ChessMove(position, new ChessLocation(x, orig_Y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                    }
                    if (UpFlag)
                    {
                        ++yUp;
                        if (yUp < 8)
                        {
                            if (board[x % 8 + yUp * 8] != EMPTY_SPACE)
                            {
                                if (board[x % 8 + yUp * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                                {
                                    move = new ChessMove(position, new ChessLocation(x, yUp));
                                    if (isCheck(board, move, color) >= 0)
                                    {
                                        movelist.Add(move);
                                    }
                                }
                                UpFlag = false;
                            }
                            else
                            {
                                move = new ChessMove(position, new ChessLocation(x, yUp));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
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

                            if (board[x % 8 + yDown * 8] != EMPTY_SPACE)
                            {
                                if (board[x % 8 + yDown * 8] < EMPTY_SPACE) // < empty = black | > empty = white
                                {
                                    move = new ChessMove(position, new ChessLocation(x, yDown));
                                    if (isCheck(board, move, color) >= 0)
                                    {
                                        movelist.Add(move);
                                    }
                                }
                                DownFlag = false;
                            }
                            else
                            {
                                move = new ChessMove(position, new ChessLocation(x, yDown));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
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
                    if (board[orig_X % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[orig_X % 8 + y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(orig_X, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(orig_X, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
                }
                for (int y = orig_Y - 1; y >= 0; --y)
                {
                    if (board[orig_X % 8 + y * 8] != EMPTY_SPACE)
                    {
                        if (board[orig_X % 8 + y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                        {
                            move = new ChessMove(position, new ChessLocation(orig_X, y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                        break;
                    }
                    move = new ChessMove(position, new ChessLocation(orig_X, y));
                    if (isCheck(board, move, color) >= 0)
                    {
                        movelist.Add(move);
                    }
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
                        if (board[x % 8 + orig_Y * 8] != EMPTY_SPACE)
                        {
                            if (board[x % 8 + orig_Y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                            {
                                move = new ChessMove(position, new ChessLocation(x, orig_Y));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
                            }
                            HFlag = false;
                        }
                        else
                        {
                            move = new ChessMove(position, new ChessLocation(x, orig_Y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                    }
                    if (UpFlag)
                    {
                        ++yUp;
                        if (yUp < 8)
                        {
                            if (board[x % 8 + yUp * 8] != EMPTY_SPACE)
                            {
                                if (board[x % 8 + yUp * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                                {
                                    move = new ChessMove(position, new ChessLocation(x, yUp));
                                    if (isCheck(board, move, color) >= 0)
                                    {
                                        movelist.Add(move);
                                    }
                                }
                                UpFlag = false;
                            }
                            else
                            {
                                move = new ChessMove(position, new ChessLocation(x, yUp));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
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

                            if (board[x % 8 + yDown * 8] != EMPTY_SPACE)
                            {
                                if (board[x % 8 + yDown * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                                {
                                    move = new ChessMove(position, new ChessLocation(x, yDown));
                                    if (isCheck(board, move, color) >= 0)
                                    {
                                        movelist.Add(move);
                                    }
                                }
                                DownFlag = false;
                            }
                            else
                            {
                                move = new ChessMove(position, new ChessLocation(x, yDown));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
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
                        if (board[x % 8 + orig_Y * 8] != EMPTY_SPACE)
                        {
                            if (board[x % 8 + orig_Y * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                            {
                                move = new ChessMove(position, new ChessLocation(x, orig_Y));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
                            }
                            HFlag = false;
                        }
                        else
                        {
                            move = new ChessMove(position, new ChessLocation(x, orig_Y));
                            if (isCheck(board, move, color) >= 0)
                            {
                                movelist.Add(move);
                            }
                        }
                    }
                    if (UpFlag)
                    {
                        ++yUp;
                        if (yUp < 8)
                        {
                            if (board[x % 8 + yUp * 8] != EMPTY_SPACE)
                            {
                                if (board[x % 8 + yUp * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                                {
                                    move = new ChessMove(position, new ChessLocation(x, yUp));
                                    if (isCheck(board, move, color) >= 0)
                                    {
                                        movelist.Add(move);
                                    }
                                }
                                UpFlag = false;
                            }
                            else
                            {
                                move = new ChessMove(position, new ChessLocation(x, yUp));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
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

                            if (board[x % 8 + yDown * 8] != EMPTY_SPACE)
                            {
                                if (board[x % 8 + yDown * 8] > EMPTY_SPACE) // < empty = black | > empty = white
                                {
                                    move = new ChessMove(position, new ChessLocation(x, yDown));
                                    if (isCheck(board, move, color) >= 0)
                                    {
                                        movelist.Add(move);
                                    }
                                }
                                DownFlag = false;
                            }
                            else
                            {
                                move = new ChessMove(position, new ChessLocation(x, yDown));
                                if (isCheck(board, move, color) >= 0)
                                {
                                    movelist.Add(move);
                                }
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
        public List<ChessMove> KnightMoves(string board, ChessLocation position, ChessColor color)
        {
            List<ChessMove> moves = new List<ChessMove>();
            int positionX = position.X;
            int positionY = position.Y;

            Predicate<char> isMyPiece;
            if (color == ChessColor.White)
                isMyPiece = char.IsUpper;
            else
                isMyPiece = char.IsLower;

            if (positionX > 0)
            { // Can I move to the left?
                if (positionX > 1)
                { // Can I move 2 to the left?
                    if (positionY > 0)
                    { // Can I move up 1?
                        if (!isMyPiece(board[(positionX - 2) % 8 + (positionY - 1) * 8]))
                        { // Checks if it is the opponents piece
                            ChessMove move = new ChessMove(position, new ChessLocation(positionX - 2, positionY - 1));
                            if (isCheck(board, move, color) >= 0)
                            {
                                moves.Add(move);
                            }
                        }
                    }

                    if (positionY < 7)
                    { // Can I move down 1?
                        if (!isMyPiece(board[(positionX - 2) % 8 + (positionY + 1) * 8]))
                        { // Checks if it is the opponents piece
                            ChessMove move = new ChessMove(position, new ChessLocation(positionX - 2, positionY + 1));
                            if (isCheck(board, move, color) >= 0)
                            {
                                moves.Add(move);
                            }
                        }
                    }
                }

                if (positionY > 1)
                { // Can I move up 2?
                    if (!isMyPiece(board[(positionX - 1) % 8 + (positionY - 2) * 8]))
                    { // Checks if it is the opponents piece
                        ChessMove move = new ChessMove(position, new ChessLocation(positionX - 1, positionY - 2));
                        if (isCheck(board, move, color) >= 0)
                        {
                            moves.Add(move);
                        }
                    }
                }

                if (positionY < 6)
                { // Can I move down 2?
                    if (!isMyPiece(board[(positionX - 1) % 8 + (positionY + 2) * 8]))
                    { // Checks if it is the opponents piece
                        ChessMove move = new ChessMove(position, new ChessLocation(positionX - 1, positionY + 2));
                        if (isCheck(board, move, color) >= 0)
                        {
                            moves.Add(move);
                        }
                    }
                }
            }

            if (positionX < 7)
            { // Can I move to the right?
                if (positionX < 6)
                { // Can I move 2 to the right?
                    if (positionY > 0)
                    { // Can I move up 1?
                        if (!isMyPiece(board[(positionX + 2) % 8 + (positionY - 1) * 8]))
                        { // Checks if it is the opponents piece
                            ChessMove move = new ChessMove(position, new ChessLocation(positionX + 2, positionY - 1));
                            if (isCheck(board, move, color) >= 0)
                            {
                                moves.Add(move);
                            }
                        }
                    }

                    if (positionY < 7)
                    { // Can I move down 1?
                        if (!isMyPiece(board[(positionX + 2) % 8 + (positionY + 1) * 8]))
                        { // Checks if it is the opponents piece
                            ChessMove move = new ChessMove(position, new ChessLocation(positionX + 2, positionY + 1));
                            if (isCheck(board, move, color) >= 0)
                            {
                                moves.Add(move);
                            }
                        }
                    }
                }

                if (positionY > 1)
                { // Can I move up 2?
                    if (!isMyPiece(board[(positionX + 1) % 8 + (positionY - 2) * 8]))
                    { // Checks if it is the opponents piece
                        ChessMove move = new ChessMove(position, new ChessLocation(positionX + 1, positionY - 2));
                        if (isCheck(board, move, color) >= 0)
                        {
                            moves.Add(move);
                        }
                    }
                }

                if (positionY < 6)
                { // Can I move down 2?
                    if (!isMyPiece(board[(positionX + 1) % 8 + (positionY + 2) * 8]))
                    { // Checks if it is the opponents piece
                        ChessMove move = new ChessMove(position, new ChessLocation(positionX + 1, positionY + 2));
                        if (isCheck(board, move, color) >= 0)
                        {
                            moves.Add(move);
                        }
                    }
                }
            }

            return moves;
        }
        #endregion

        #region Flag functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="before"></param>
        /// <param name="move"></param>
        /// <param name="color"></param>
        /// <returns> 0 if no check. -1 if check against color of move, 1 if check for player of move.</returns>
        public int isCheck(string before, ChessMove move, ChessColor color)
        {
            return isCheckHelper(MakeMove(before, move), color, move);
        }

        public int isCheckHelper(string before, ChessColor color, ChessMove move)
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
                    char piece = before[x % 8 + y * 8];
                    bool check = false;
                    switch (piece)
                    {
                        case 'K':
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
                                    while (tempy >= 0 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving up!
                                        tempy--;
                                    }
                                    if (tempy >= 0)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking > EMPTY_SPACE)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case 'r':
                                                case 'q':
                                                    check = true;
                                                    break;
                                                case 'k':
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
                                    while (tempy < 8 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving down
                                        tempy++;
                                    }
                                    if (tempy < 8)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking > EMPTY_SPACE)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case 'r':
                                                case 'q':
                                                    check = true;
                                                    break;
                                                case 'k':
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
                                    while (tempx >= 0 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving left
                                        tempx--;
                                    }
                                    if (tempx >= 0)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking > EMPTY_SPACE)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case 'r':
                                                case 'q':
                                                    check = true;
                                                    break;
                                                case 'k':
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
                                    while (tempx < 8 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving right
                                        tempx++;
                                    }
                                    if (tempx < 8)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking > EMPTY_SPACE)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case 'r':
                                                case 'q':
                                                    check = true;
                                                    break;
                                                case 'k':
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
                                    while (tempx >= 0 && tempy >= 0 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving up left
                                        tempy--;
                                        tempx--;
                                    }
                                    if (tempx >= 0 && tempy >= 0)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking > EMPTY_SPACE)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case 'b':
                                                case 'q':
                                                    check = true;
                                                    break;
                                                case 'p':
                                                case 'k':
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
                                    while (tempx < 8 && tempy >= 0 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving up right
                                        tempy--;
                                        tempx++;
                                    }
                                    if (tempx < 8 && tempy >= 0)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking > EMPTY_SPACE)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case 'b':
                                                case 'q':
                                                    check = true;
                                                    break;
                                                case 'p':
                                                case 'k':
                                                    if (tempx - x == 1 && y - tempy == 1)
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
                                    while (tempx >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving down left
                                        tempy++;
                                        tempx--;
                                    }
                                    if (tempx >= 0 && tempy < 8)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking > EMPTY_SPACE)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case 'b':
                                                case 'q':
                                                    check = true;
                                                    break;
                                                case 'k':
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
                                    while (tempx < 8 && tempy < 8 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving  down right
                                        tempy++;
                                        tempx++;
                                    }
                                    if (tempx < 8 && tempy < 8)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking > EMPTY_SPACE)
                                        {
                                            //this is a black piece.
                                            switch (attacking)
                                            {
                                                case 'b':
                                                case 'q':
                                                    check = true;
                                                    break;
                                                case 'k':
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
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'n')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 2;
                                    tempy = y - 1;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'n')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 2;
                                    tempy = y + 1;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'n')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 2;
                                    tempy = y - 1;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'n')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 1;
                                    tempy = y + 2;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'n')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 1;
                                    tempy = y + 2;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'n')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 1;
                                    tempy = y - 2;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'n')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 1;
                                    tempy = y - 2;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'n')
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
                        case 'k':
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
                                    while (tempy >= 0 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving up!
                                        tempy--;
                                    }
                                    if (tempy >= 0)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking < EMPTY_SPACE)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case 'R':
                                                case 'Q':
                                                    check = true;
                                                    break;
                                                case 'K':
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
                                    while (tempy < 8 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving down
                                        tempy++;
                                    }
                                    if (tempy < 8)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking < EMPTY_SPACE)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case 'R':
                                                case 'Q':
                                                    check = true;
                                                    break;
                                                case 'K':
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
                                    while (tempx >= 0 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving left
                                        tempx--;
                                    }
                                    if (tempx >= 0)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking < EMPTY_SPACE)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case 'R':
                                                case 'Q':
                                                    check = true;
                                                    break;
                                                case 'K':
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
                                    while (tempx < 8 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving right
                                        tempx++;
                                    }
                                    if (tempx < 8)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking < EMPTY_SPACE)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case 'R':
                                                case 'Q':
                                                    check = true;
                                                    break;
                                                case 'K':
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
                                    while (tempx >= 0 && tempy >= 0 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving up left
                                        tempy--;
                                        tempx--;
                                    }
                                    if (tempx >= 0 && tempy >= 0)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking < EMPTY_SPACE)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case 'B':
                                                case 'Q':
                                                    check = true;
                                                    break;
                                                case 'K':
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
                                    while (tempx < 8 && tempy >= 0 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving up right
                                        tempy--;
                                        tempx++;
                                    }
                                    if (tempx < 8 && tempy >= 0)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking < EMPTY_SPACE)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case 'B':
                                                case 'Q':
                                                    check = true;
                                                    break;
                                                case 'K':
                                                    if (tempx - x == 1 && tempy - y == 1)
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
                                    while (tempx >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving down left
                                        tempy++;
                                        tempx--;
                                    }
                                    if (tempx >= 0 && tempy < 8)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking < EMPTY_SPACE)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case 'B':
                                                case 'Q':
                                                    check = true;
                                                    break;
                                                case 'P':
                                                case 'K':
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
                                    while (tempx < 8 && tempy < 8 && before[tempx % 8 + tempy * 8] == EMPTY_SPACE)
                                    {
                                        //keep moving  down right
                                        tempy++;
                                        tempx++;
                                    }
                                    if (tempx < 8 && tempy < 8)
                                    {
                                        var attacking = before[tempx % 8 + tempy * 8];
                                        if (attacking < EMPTY_SPACE)
                                        {
                                            //this is a white piece.
                                            switch (attacking)
                                            {
                                                case 'B':
                                                case 'Q':
                                                    check = true;
                                                    break;
                                                case 'P':
                                                case 'K':
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
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'N')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 2;
                                    tempy = y - 1;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'N')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 2;
                                    tempy = y + 1;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'N')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 2;
                                    tempy = y - 1;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'N')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 1;
                                    tempy = y + 2;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'N')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 1;
                                    tempy = y + 2;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'N')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x + 1;
                                    tempy = y - 2;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'N')
                                    {
                                        check = true;
                                        break;
                                    }
                                    tempx = x - 1;
                                    tempy = y - 2;
                                    if (tempx >= 0 && tempx < 8 && tempy >= 0 && tempy < 8 && before[tempx % 8 + tempy * 8] == 'N')
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
                        case 'b':
                            blackTotal += 4;
                            break;
                        case 'n':
                            blackTotal += 3;
                            break;
                        case 'p':
                            blackTotal += 1;
                            break;
                        case 'q':
                            blackTotal += 9;
                            break;
                        case 'r':
                            blackTotal += 5;
                            break;
                        case 'B':
                            whiteTotal += 4;
                            break;
                        case 'N':
                            whiteTotal += 3;
                            break;
                        case 'P':
                            whiteTotal += 1;
                            break;
                        case 'Q':
                            whiteTotal += 9;
                            break;
                        case 'R':
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
