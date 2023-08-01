using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    double[] pieceValues = { 0, 1, 3, 3.10, 5, 9, 100 };
    int total = 0;
    Move bestMove;
    public Move Think(Board board, Timer timer)
    {
        double eval = 0;
        int maxDepth = 5;
        bestMove = Move.NullMove;
        Console.WriteLine("Time To Use: " + timer.MillisecondsRemaining / 30);
        eval =  moveEvaluater(board, timer, maxDepth, -1000000, 1000000, 0);
        /*for(int depth = 1; depth < 100; depth++){ //NOT WORKING RIGHT
            maxDepth = depth;
            eval = moveEvaluater(board, timer, depth, -1000000, 1000000, 0);
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)
                break;
        }*/
        Console.WriteLine("Time Used: " + timer.MillisecondsElapsedThisTurn + " Max Depth: " + maxDepth);
        Console.WriteLine("Eval: " + eval);
        Move[] moves = board.GetLegalMoves();
        Console.WriteLine(bestMove);
        return bestMove == Move.NullMove ? moves[0] : bestMove;
    }
    private double moveEvaluater(Board board, Timer timer, double depth, double alpha, double beta, int ply){
        
        total++;
        if(board.IsDraw()){
            return 0;
        }
        if(board.IsInCheckmate()){
            return 1000000 + depth;
        }
        Move[] moves = board.GetLegalMoves();
        List<Move> checks = new List<Move>();
        List<Move> captures = new List<Move>();
        List<Move> other = new List<Move>();
        foreach (Move move in moves) {
            if(move.IsCapture){
                captures.Add(move);
            }
            else{
                board.MakeMove(move);
                if(board.IsInCheck()){
                    checks.Add(move);
                }
                else{
                    other.Add(move);
                }
                board.UndoMove(move);
            }
        }
        double best = positionEvaluator(board);
        if(depth <= 0 || moves.Length == 0){
            return best;
        }
        best = -30000;
        for(int i = 0; i < moves.Length; i++) {
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 3000000;
            Move move = getMove(checks, captures, other, i);
            //Console.WriteLine(move);
            
            board.MakeMove(move);
            double score = -moveEvaluater(board, timer, depth-1, -beta, -alpha, ply + 1);
            board.UndoMove(move);
            if (score > best){
                best = score;
                //bestMove = move;
                if (ply == 0) bestMove = move;
            }

            alpha = Math.Max(alpha, best);
            if (alpha >= beta) break;
        }
        return best;
    }
    private Move getMove(List<Move> checks, List<Move> captures, List<Move> others, int index){
        if(index < checks.Count){
            return checks[index];
        }
        else if(index < checks.Count + captures.Count){
            return captures[index-checks.Count];
        }
        else{
            return others[index-(checks.Count + captures.Count)];
        }
    }
    private int materialDifference(Board board){
        PieceList[] list = board.GetAllPieceLists();
        int total = list[0].Count + 3*(list[1].Count +list[2].Count) + 5*list[3].Count + 9*list[4].Count - (list[6].Count + 3*(list[7].Count +list[8].Count) + 5*list[9].Count + 9*list[10].Count);
        return total;
    }
    private double positionEvaluator(Board board){
        Move[] moves = board.GetLegalMoves();
        double pos = Math.Cbrt(moves.Length - 15.0) * board.PlyCount / 3000;
        if(board.IsInCheck()){
            pos++;
        }
        pos += materialDifference(board);
        pos += getSpaceAdvantage(board) / 10;
        if((board.GetPieceBitboard(PieceType.Pawn, true) & 103481868288) != 0){
            pos += 0.5;
        }
        if((board.GetPieceBitboard(PieceType.Pawn, false) & (ulong)103481868288) != 0){
            pos-= 0.5;
        }
        return board.IsWhiteToMove ? pos : - pos;
    }
    private int getSpaceAdvantage(Board board){           
        return getSpaceAdvantageSide(board, true) - getSpaceAdvantageSide(board, false);
    }
    public int getSpaceAdvantageSide(Board board, bool white){
        PieceList[] pieces = board.GetAllPieceLists();
        int whiteSquaresAttacked = 0;
        int listLoc;
        listLoc = white ? 0 : 6;
        foreach(Piece piece in pieces[listLoc++]){
            whiteSquaresAttacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPawnAttacks(piece.Square, white));
        }
        foreach(Piece piece in pieces[listLoc++]){
            whiteSquaresAttacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPawnAttacks(piece.Square, white));
        }
        foreach(Piece piece in pieces[listLoc++]){
            whiteSquaresAttacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPawnAttacks(piece.Square, white));
        }
        foreach(Piece piece in pieces[listLoc++]){
            whiteSquaresAttacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPawnAttacks(piece.Square, white));
        }
        foreach(Piece piece in pieces[listLoc++]){
            whiteSquaresAttacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPawnAttacks(piece.Square, white));
        }
        return whiteSquaresAttacked;
    }
}