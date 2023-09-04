using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class NeuralNetwork
{
    private int inputSize = 64;
    private int hiddenSize = 64;
    private int outputSize = 1;
    private double[,] weightsInputHidden;
    private double[,] weightsHiddenOutput;
    private double[] biasHidden;
    private double[] biasOutput;

    public NeuralNetwork()
    {
        // Initialize weights and biases with random values
        weightsInputHidden = new double[inputSize, hiddenSize];
        weightsHiddenOutput = new double[hiddenSize, outputSize];
        biasHidden = new double[] {
            0.2981, -0.2535,  0.2771,  0.2300,  0.2028,  0.2985,  0.2444,  0.5654,
            -0.2777,  0.2984,  0.3115,  0.2199,  0.1980,  0.1995,  0.2738,  0.2300,
            0.1942,  0.1575,  0.2979,  0.2617,  0.2117,  0.1617,  0.1926,  0.2007,
            0.5081,  0.3370,  0.3412,  0.3197,  0.3914,  0.3916,  0.3435,  0.3391,
            0.3439,  0.3429,  0.2739,  0.2295,  0.3802,  0.3430,  0.3672,  0.0864,
            0.2769,  0.2971,  0.3434,  0.1902,  0.2634,  0.2990,  0.1442,  0.2593,
            0.3430,  0.2753,  0.3429,  0.2453,  0.1929,  0.2990,  0.2155,  0.3210,
            0.2914,  0.2405,  0.3446,  0.3702,  0.1718,  0.3544,  0.1748,  0.1808 };
        biasOutput = new double[] { -0.0297 };
        RandomizeWeightsAndBiases();
    }
    public double ReLU(double x)
    {
        return Math.Max(0, x);
    }
    private void RandomizeWeightsAndBiases()
    {
        var random = new Random(0); // Use a fixed seed for reproducibility

        for (int i = 0; i < inputSize; i++)
        {
            for (int j = 0; j < hiddenSize; j++)
            {
                weightsInputHidden[i, j] = random.NextDouble() - 0.5;
            }
        }

        for (int i = 0; i < hiddenSize; i++)
        {
            for (int j = 0; j < outputSize; j++)
            {
                weightsHiddenOutput[i, j] = random.NextDouble() - 0.5;
            }
        }
    }

    public double FeedForward(double[] input)
    {
        if (input.Length != inputSize)
        {
            throw new ArgumentException("Input size does not match the network's input size.");
        }

        // Calculate hidden layer output
        var hiddenLayer = new double[hiddenSize];
        for (int i = 0; i < hiddenSize; i++)
        {
            hiddenLayer[i] = biasHidden[i] + Enumerable.Range(0, inputSize)
                .Sum(j => input[j] * weightsInputHidden[j, i]);
            hiddenLayer[i] = ReLU(hiddenLayer[i]);
        }

        // Calculate output layer output
        var output = biasOutput[0] + Enumerable.Range(0, hiddenSize)
            .Sum(j => hiddenLayer[j] * weightsHiddenOutput[j, 0]);
        return output;
    }
}

public class MyBot : IChessBot
{
    private NeuralNetwork neuralNetwork = new NeuralNetwork();

    public Move Think(Board board, Timer timer)
    {
        // Initialize search parameters (e.g., depth, time control)
        int depth = 4;
        double maxTime = timer.MillisecondsRemaining / 1000.0; // Convert to seconds
        double startTime = timer.GameStartTimeMilliseconds / 1000.0;

        // Perform iterative deepening search
        Move[] legalMoves = board.GetLegalMoves();
        Array.Sort(legalMoves, (a, b) => a.IsCapture.CompareTo(b.IsCapture) + a.IsPromotion.CompareTo(b.IsPromotion) + a.IsPromotion.CompareTo(b.IsPromotion));
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
                if (board.IsInCheckmate())
                {
                    return move;
                }
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
        Array.Sort(legalMoves, (a, b) => a.IsCapture.CompareTo(b.IsCapture) + a.IsPromotion.CompareTo(b.IsPromotion) + a.IsPromotion.CompareTo(b.IsPromotion));
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
    public static double[] FenToBoardVector(string fen)
    {
        Console.WriteLine(fen);
        string[] piecePlacement = fen.Split(' ')[0].Split('/');
        // string activeColor = parts[1];
        // string castlingRights = parts[2];
        // string enPassant = parts[3];
        // int halfmoveClock = int.Parse(parts[4]);
        // int fullmoveClock = int.Parse(parts[5]);

        double[] boardVector = new double[64];

        Dictionary<char, double> pieceToValue = new Dictionary<char, double>
        {
            { 'R', 0.5 },
            { 'N', 0.3 },
            { 'B', 0.35 },
            { 'Q', 0.9 },
            { 'K', 1.0 },
            { 'P', 0.1 },
            { 'p', -0.1 },
            { 'k', -1.0 },
            { 'q', -0.9},
            { 'b', -0.35 },
            { 'n', -0.3 },
            { 'r', -0.5 }
        };
        for (int r = 0; r < piecePlacement.Length; r++)
        {
            int c = 0;
            foreach (char piece in piecePlacement[r])
            {
                if (pieceToValue.ContainsKey(piece))
                {
                    boardVector[r * 8 + c] = pieceToValue[piece];
                    c++;
                }
                else
                {
                    c += int.Parse(piece.ToString());
                }
            }
        }
        return boardVector;
    }
    public double Evaluate(Board board)
    {
        // Implement a position evaluation function
        // Consider factors like material, piece activity, pawn structure, king safety, etc.
        return neuralNetwork.FeedForward(FenToBoardVector(board.GetFenString()));
    }
}
