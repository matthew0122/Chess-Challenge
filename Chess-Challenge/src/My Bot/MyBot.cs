using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    double[] pieceValues = { 0, 1, 3, 3.10, 5, 9, 100 };
    int total = 0;
    Move move;
    public Move Think(Board board, Timer timer)
    {
        Console.WriteLine(moveEvaluater(board, 4, -1000000, 1000000, board.IsWhiteToMove));
        //BitboardHelper.VisualizeBitboard(board.WhitePiecesBitboard);
        Console.WriteLine(positionEvaluator(board, board.IsWhiteToMove));
        //Console.WriteLine(total);
        return move;
    }
    private double moveEvaluater(Board board, double depth, double alpha, double beta, bool white){
        
        total++;
        
        /*      
        if(board.PlyCount < 4 && move.MovePieceType == PieceType.Pawn){
            if(white){
                pos += 0.3;
            }
            else{
                pos -= 0.3;
            }
        }
        else if(board.PlyCount < 10 && (move.MovePieceType == PieceType.King || move.MovePieceType == PieceType.Pawn || move.MovePieceType == PieceType.Queen)){
            if(white){
                pos -= 0.3;
            }
            else{
                pos += 0.3;
            }
        }
        if(move.IsCastles){
            if(white){
                pos += 0.8;
            }
            else{
                pos -= 0.8;
            }
        }
        if(move.IsCapture){
            PieceType captured = move.CapturePieceType;
            PieceType capturer = move.MovePieceType;
            if(pieceValues[(int)captured] > pieceValues[(int)capturer]){
                if(!white){
                    pos += pieceValues[(int)captured] - pieceValues[(int)capturer];
                }
                else{
                    pos -= pieceValues[(int)captured] - pieceValues[(int)capturer];
                }
                
            }
        }
        */
        if(board.IsDraw()){
            return 0;
        }
        if(board.IsInCheckmate()){
            return white ? -1000000 - depth : 1000000 + depth;
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
        int amtOfChecks = checks.Count;
        int index = 0;
        if(depth <= 0 || moves.Length == 0){
            double returnVal = positionEvaluator(board, white);
            return returnVal;
        }
        
        double value;
        if(white){
            value = -10000000;
            for(int i = 0; i < moves.Length; i++){
                Move currentMove = getMove(checks, captures, other, i);
                board.MakeMove(currentMove);
                double eval = moveEvaluater(board, depth - 1, alpha, beta, false);
                if(eval > value){
                    value = eval;
                    index = i;
                }
                board.UndoMove(currentMove);
                if(value > beta){
                    break;
                }
                alpha = Math.Max(alpha, value);
            }
            if(depth == 4){
                move = getMove(checks, captures, other, index);
            }
            return value;
        }
        else{
            value = 10000000;
            for(int i = 0; i < moves.Length; i++){
                Move currentMove = getMove(checks, captures, other, i);
                board.MakeMove(currentMove);
                double eval = moveEvaluater(board, depth - 1, alpha, beta, true);
                if(eval < value){
                    value = eval;
                    index = i;
                }
                board.UndoMove(currentMove);
                if(value < alpha){
                    break;
                }
                beta = Math.Min(beta, value);
            }
            if(depth == 4){
                move = getMove(checks, captures, other, index);
            }
            return value;
        }
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
    private double positionEvaluator(Board board, bool white){
        Move[] moves = board.GetLegalMoves();
        double pos = 0;
        if(board.IsInCheck()){
            pos = white ? pos+1 : pos-1;
        }
        pos = (white) ? Math.Cbrt(moves.Length - 15.0) * board.PlyCount / 3000 : Math.Cbrt(moves.Length - 15.0) * board.PlyCount / -3000;
        pos += materialDifference(board);
        pos += getSpaceAdvantage(board) / 10;
        if((board.GetPieceBitboard(PieceType.Pawn, true) & 103481868288) != 0){
            pos += 0.5;
        }
        else if((board.GetPieceBitboard(PieceType.Pawn, false) & 103481868288) != 0){
            pos-= 0.5;
        }
        return pos;
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