using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 301, 311, 500, 900, 100000 };
    int total = 0;
    Move depthMove;
    Move bestMove;
    struct TTEntry {
        public ulong key;
        public Move move;
        public int depth, bound;
        public int score;
        public TTEntry(ulong _key, Move _move, int _depth, int _score, int _bound) {
            key = _key; move = _move; depth = _depth; score = _score; bound = _bound;
        }
    }

    const int entries = (1 << 20);
    TTEntry[] transpositionTable = new TTEntry[entries];

    public Move Think(Board board, Timer timer)
    {
        int maxDepth = 1;
        bestMove = Move.NullMove;
        for(int depth = 1; depth < 100; depth++){
            maxDepth = depth;
            moveEvaluater(board, timer, depth, -1000000, 1000000, 0);
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30)
                break;
            bestMove = depthMove;
        }
        Move[] moves = board.GetLegalMoves();
        return bestMove == Move.NullMove ? moves[0] : bestMove;
    }
    private int moveEvaluater(Board board, Timer timer, int depth, int alpha, int beta, int ply){
        if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 0;
        ulong zobristKey = board.ZobristKey;
        bool notRoot = ply > 0;
        if(notRoot && board.IsRepeatedPosition()) return 0;

        TTEntry entry = transpositionTable[zobristKey % entries];

        // TT cutoffs
        if(notRoot && entry.key == zobristKey && entry.depth >= depth && (
            entry.bound == 3 // exact score
                || entry.bound == 2 && entry.score >= beta // lower bound, fail high
                || entry.bound == 1 && entry.score <= alpha // upper bound, fail low
        )) return entry.score;
        total++;
        Move[] moves = board.GetLegalMoves();
        if(moves.Length == 0) return board.IsInCheck() ? -30000 + ply : 0;
        int best = positionEvaluator(board);
        if(depth <= 0) return best;
        
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
        double origAlpha = alpha;
        best = -3000000;
        for(int i = 0; i < moves.Length; i++) {     
            if(timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30) return 0;       
            Move move = getMove(checks, captures, other, i);
            //Console.WriteLine(move);
            board.MakeMove(move);
            int score = -moveEvaluater(board, timer, depth - 1, -beta, -alpha, ply + 1);
            board.UndoMove(move);
            if(score > best) {
                best = score;
                //bestMove = move;
                if(ply == 0) depthMove = move;
                
                // Improve alpha
                alpha = Math.Max(alpha, score);

                // Fail-high
                if(alpha >= beta) break;

            }
            
        }
        int bound = best >= beta ? 2 : best > origAlpha ? 3 : 1;
        transpositionTable[zobristKey % entries] = new TTEntry(zobristKey, depthMove, depth, best, bound);
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
        return list[0].Count + 3*(list[1].Count +list[2].Count) + 5*list[3].Count + 9*list[4].Count - (list[6].Count + 3*(list[7].Count +list[8].Count) + 5*list[9].Count + 9*list[10].Count);
    }
    private int positionEvaluator(Board board){
        Move[] moves = board.GetLegalMoves();
        int pos = materialDifference(board);
        pos += getSpaceAdvantage(board) / 10;
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