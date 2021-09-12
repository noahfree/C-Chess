using System.Collections.Generic;
using System.Windows;

namespace Chess
{
    // class ComputerPiece: used to store the coordinates and the valid moves for each piece that the computer controls
    internal class ComputerPiece
    {
        private Piece piece;
        private List<Point> validMoves;
        private int x;
        private int y;

        public ComputerPiece(Piece piece)
        {
            this.piece = piece;
            validMoves = new List<Point>();
        }

        public Piece Piece
        {
            get
            {
                return piece;
            }
        }

        public List<Point> ValidMoves
        {
            get
            {
                return validMoves;
            }
            set
            {
                validMoves = value;
            }
        }

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }
    }
}