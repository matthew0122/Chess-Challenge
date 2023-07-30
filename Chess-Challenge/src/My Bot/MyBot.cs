using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    double[] pieceValues = { 0, 1, 3, 3.01, 5, 9, 100 };
    int total = 0;
    Move move;
    public Move Think(Board board, Timer timer)
    {
        Console.WriteLine(moveEvaluater(board, 4, -1000000, 1000000, board.IsWhiteToMove));
        return move;
    }
    private double moveEvaluater(Board board, double depth, double alpha, double beta, bool white){
        
        total++;
        
        /*
        if(BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard) < 8){
            if(movesMade > 3){
                return eval;
            }
        }
        else if(movesMade > 3){
            return eval;
        }
        
        board.MakeMove(move);
        if(board.IsInCheckmate() && white){
            board.UndoMove(move);
            Console.WriteLine(move);
            return 1000000;
        }
        else if(board.IsInCheckmate()){
            board.UndoMove(move);
            return -1000000;
        }
        pos = positionEvaluator(board, white);
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
        if(board.IsInCheck()){
            if(white){
                pos += 0.3;
            }
            else{
                pos -= 0.3;
            }
        }
        */
        Move[] moves = board.GetLegalMoves();
        int index = 0;
        if(depth <= 0 || moves.Length == 0){
            double returnVal = positionEvaluator(board, white);
            return returnVal;
        }
        
        double value;
        if(white){
            value = -10000000;
            for(int i = 0; i < moves.Length; i++){
                board.MakeMove(moves[i]);
                double eval = moveEvaluater(board, depth - 1, alpha, beta, false);
                if(eval > value){
                    value = eval;
                    index = i;
                }
                board.UndoMove(moves[i]);
                if(value > beta){
                    break;
                }
                alpha = Math.Max(alpha, value);
            }
            if(depth == 4){
                move = moves[index];
            }
            return value;
        }
        else{
            value = 10000000;
            for(int i = 0; i < moves.Length; i++){
                board.MakeMove(moves[i]);
                double eval = moveEvaluater(board, depth - 1, alpha, beta, true);
                if(eval < value){
                    value = eval;
                    index = i;
                }
                board.UndoMove(moves[i]);
                if(value < alpha){
                    break;
                }
                beta = Math.Min(beta, value);
            }
            if(depth == 4){
                move = moves[index];
            }
            return value;
        }
    }
    private int materialDifference(Board board){
        PieceList[] list = board.GetAllPieceLists();
        return list[0].Count + 3*(list[1].Count +list[2].Count) + 5*list[3].Count + 9*list[4].Count - (list[6].Count + 3*(list[7].Count +list[8].Count) + 5*list[9].Count + 9*list[10].Count);
    }
    private double positionEvaluator(Board board, bool white){
        if(board.IsDraw()){
            return 0;
        }
        if(board.IsInCheckmate() && white){
            return -1000000;
        }
        else if(board.IsInCheckmate()){
            return 1000000;
        }
        Move[] moves = board.GetLegalMoves();
        double pos = 0;
        if(board.IsInCheck()){
            if(white){
                pos += 1;
            }
            else{
                pos -=1;
            }
        }
        if(white){
            pos = Math.Cbrt(moves.Length - 15.0) * board.PlyCount / 3000;
        }
        else{
            pos = Math.Cbrt(moves.Length - 15.0) * board.PlyCount / -3000;
        }
        pos += materialDifference(board);
        pos += getSpaceAdvantage(board) / 10;
        return pos;
    }
    private int getSpaceAdvantage(Board board){           
        return getSpaceAdvantageSide(board, true) - getSpaceAdvantageSide(board, false);
    }
    public int getSpaceAdvantageSide(Board board, bool white){
        PieceList[] pieces = board.GetAllPieceLists();
        int whiteSquaresAttacked = 0;
        int listLoc = 0;
        if(!white){
            listLoc += 6;
        }
        PieceList pieceList = pieces[listLoc];
        for(int j=0; j < pieceList.Count; j++){
            Piece piece = pieceList.GetPiece(j);
            whiteSquaresAttacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetPawnAttacks(piece.Square, white));
        }
        pieceList = pieces[++listLoc];
        for(int j=0; j < pieceList.Count; j++){
            Piece piece = pieceList.GetPiece(j);
            whiteSquaresAttacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetKnightAttacks(piece.Square));
        }
        pieceList = pieces[++listLoc];
        for(int j=0; j < pieceList.Count; j++){
            Piece piece = pieceList.GetPiece(j);
            whiteSquaresAttacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Bishop, piece.Square, board));
        }
        pieceList = pieces[++listLoc];
        for(int j=0; j < pieceList.Count; j++){
            Piece piece = pieceList.GetPiece(j);
            whiteSquaresAttacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Rook, piece.Square, board));
        }
        pieceList = pieces[++listLoc];
        for(int j=0; j < pieceList.Count; j++){
            Piece piece = pieceList.GetPiece(j);
            whiteSquaresAttacked += BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(PieceType.Queen, piece.Square, board));
        }
        return whiteSquaresAttacked;
    }
}