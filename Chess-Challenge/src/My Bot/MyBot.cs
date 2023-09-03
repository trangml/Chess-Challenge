using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        // Initialize search parameters (e.g., depth, time control)
        int depth = 4;
        double maxTime = timer.MillisecondsRemaining / 1000.0; // Convert to seconds
        double startTime = timer.GameStartTimeMilliseconds / 1000.0;

        // Perform iterative deepening search
        Move[] legalMoves = board.GetLegalMoves();
        Move bestMove = legalMoves[0];
        double bestScore = double.NegativeInfinity;
        double alpha = double.NegativeInfinity;
        double beta = double.PositiveInfinity;

        for (int currentDepth = 1; currentDepth <= depth; currentDepth++)
        {
            // Generate legal moves for the current position
            foreach (Move move in legalMoves)
            {
                board.MakeMove(move); // Make the move
                double score = -AlphaBeta(board, currentDepth - 1, -beta, -alpha);
                board.UndoMove(move); // Undo the move

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }

                alpha = Math.Max(alpha, bestScore);

                if (alpha >= beta)
                {
                    break; // Beta cutoff
                }
            }

            // Check if time is running out and decide whether to continue searching
            double elapsedTime = timer.MillisecondsElapsedThisTurn / 1000.0;
            if (startTime + elapsedTime >= maxTime)
            {
                break;
            }
        }

        // Return the best move found
        return bestMove;
    }

    public double AlphaBeta(Board board, int depth, double alpha, double beta)
    {
        if (depth == 0 || board.IsDraw() || board.IsInCheckmate())
        {
            return Evaluate(board);
        }

        Move[] legalMoves = board.GetLegalMoves();
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            double score = -AlphaBeta(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);

            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
            {
                break; // Beta cutoff
            }
        }

        return alpha;
    }
    public double Evaluate(Board board)
    {
        // Implement a position evaluation function
        // Consider factors like material, piece activity, pawn structure, king safety, etc.

        Random rng = new();
        double val = rng.Next(1); // Placeholder evaluation score
        Console.WriteLine(val);
        return val;
    }
}
