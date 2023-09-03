using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        foreach (Move move in moves)
        {
            if (move.IsCapture)
            {
                return move;
            }
            else if (move.IsPromotion)
            {
                return move;
            }
            else if (move.IsCastles)
            {
                return move;
            }
            else if (move.IsEnPassant)
            {
                return move;
            }
        }

        // Pick a random move to play if nothing better is found
        Random rng = new();
        Move moveToPlay = moves[rng.Next(moves.Length)];
        return moveToPlay;

    }
}