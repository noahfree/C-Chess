using System;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    // class Piece: inherited by each specific piece's class in order to utilize polymorphism
    [Serializable]
    public abstract class Piece
    {
        private bool black;
        private bool white;
        private int x;
        private int y;
        private string filePath;
        private double cost;

        public Piece(bool black, int x, int y)
        {
            this.black = black;
            this.white = !black;
            this.x = x;
            this.y = y;
        }

        // each class implements the function VerifyMove(), which verifies that the piece is able to move to coordinate (x, y) taking into
        // account only the path of the piece and not anything else happening on the board, which is verified using other algorithm found
        // in the main class
        public abstract bool VerifyMove(int x, int y);

        public bool Black
        {
            get
            {
                return black;
            }
        }

        public bool White
        {
            get
            {
                return white;
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

        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
            }
        }

        public double Cost
        {
            get
            {
                return cost;
            }
            set
            {
                cost = value;
            }
        }
    }

    // Note: The Kaufman Piece Values, established by Larry Kaufman in 1999, are utilized in order to make 
    // the single-player computer somewhat intelligent: http://www.chessfornovices.com/chesspiecevalues.html

    [Serializable]
    public class Pawn : Piece
    {
        public Pawn(bool black, int x, int y) : base(black, x, y)
        {
            if (black) this.FilePath = @"pack://siteoforigin:,,,/pieces/BP.png";
            else FilePath = @"pack://siteoforigin:,,,/pieces/WP.png";
            this.Cost = 1;
        }

        // function VerifyMove() returns true if a pawn can move to coordinate (x, y)
        public override bool VerifyMove(int x, int y)
        {
            bool toggle = false;
            if (this.Black)
            {
                if (this.Y == 1)
                {
                    if (y == this.Y + 2) toggle = true;
                }
                return x == this.X && (toggle || y == this.Y + 1);
            }
            else
            {
                if (this.Y == 6)
                {
                    if (y == this.Y - 2) toggle = true;
                }
                return x == this.X && (toggle || y == this.Y - 1);
            }
        }
    }

    [Serializable]
    public class Rook : Piece
    {
        public Rook(bool black, int x, int y) : base(black, x, y)
        {
            if (black) this.FilePath = @"pack://siteoforigin:,,,/pieces/BR.png";
            else FilePath = @"pack://siteoforigin:,,,/pieces/WR.png";
            this.Cost = 5;
        }

        // function VerifyMove() returns true if a rook can move to coordinate (x, y)
        public override bool VerifyMove(int x, int y)
        {
            return (this.X == x && this.Y != y) || (this.X != x && this.Y == y);
        }
    }

    [Serializable]
    public class Knight : Piece
    {
        public Knight(bool black, int x, int y) : base(black, x, y)
        {
            if (black) this.FilePath = @"pack://siteoforigin:,,,/pieces/BH.png";
            else FilePath = @"pack://siteoforigin:,,,/pieces/WH.png";
            this.Cost = 3.25;
        }

        // function VerifyMove() returns true if a knight can move to coordinate (x, y)
        public override bool VerifyMove(int x, int y)
        {
            return ((this.Y + 2 == y) && (this.X + 1 == x)) ||
                   ((this.Y + 1 == y) && (this.X + 2 == x)) ||
                   ((this.Y - 2 == y) && (this.X + 1 == x)) ||
                   ((this.Y - 1 == y) && (this.X + 2 == x)) ||
                   ((this.Y + 2 == y) && (this.X - 1 == x)) ||
                   ((this.Y + 1 == y) && (this.X - 2 == x)) ||
                   ((this.Y - 2 == y) && (this.X - 1 == x)) ||
                   ((this.Y - 1 == y) && (this.X - 2 == x));
        }
    }

    [Serializable]
    public class Bishop : Piece
    {
        public Bishop(bool black, int x, int y) : base(black, x, y)
        {
            if (black) this.FilePath = @"pack://siteoforigin:,,,/pieces/BB.png";
            else FilePath = @"pack://siteoforigin:,,,/pieces/WB.png";
            this.Cost = 3.25;
        }

        // function VerifyMove() returns true if a bishop can move to coordinate (x, y)
        public override bool VerifyMove(int x, int y)
        {
            if (this.X == x || this.Y == y) return false;

            int i = this.X;
            int j = this.Y;
            if (this.X < x && this.Y < y)
            {
                while (i >= 0 && j >= 0 && i <= 7 && j <= 7)
                {
                    i++;
                    j++;
                    if (i == x && j == y) return true;
                }
                return false;
            }
            if (this.X > x && this.Y < y)
            {
                while (i >= 0 && j >= 0 && i <= 7 && j <= 7)
                {
                    i--;
                    j++;
                    if (i == x && j == y) return true;
                }
                return false;
            }
            if (this.X > x && this.Y > y)
            {
                while (i >= 0 && j >= 0 && i <= 7 && j <= 7)
                {
                    i--;
                    j--;
                    if (i == x && j == y) return true;
                }
                return false;
            }
            if (this.X < x && this.Y > y)
            {
                while (i >= 0 && j >= 0 && i <= 7 && j <= 7)
                {
                    i++;
                    j--;
                    if (i == x && j == y) return true;
                }
                return false;
            }
            return false;
        }
    }

    [Serializable]
    public class Queen : Piece
    {
        public Queen(bool black, int x, int y) : base(black, x, y)
        {
            if (black) this.FilePath = @"pack://siteoforigin:,,,/pieces/BQ.png";
            else FilePath = @"pack://siteoforigin:,,,/pieces/WQ.png";
            this.Cost = 9.75;
        }

        // function VerifyMove() returns true if a queen can move to coordinate (x, y)
        public override bool VerifyMove(int x, int y)
        {
            Rook rook = new Rook(true, this.X, this.Y);
            if (rook.VerifyMove(x, y)) return true;
            Bishop bishop = new Bishop(true, this.X, this.Y);
            return bishop.VerifyMove(x, y);
        }
    }

    [Serializable]
    public class King : Piece
    {
        private bool castle = true;
        public King(bool black, int x, int y) : base(black, x, y)
        {
            if (black) this.FilePath = @"pack://siteoforigin:,,,/pieces/BK.png";
            else FilePath = @"pack://siteoforigin:,,,/pieces/WK.png";
            this.Cost = 3;
        }

        // function VerifyMove() returns true if a king can move to coordinate (x, y)
        public override bool VerifyMove(int x, int y)
        {
            if (this.X == x && this.Y == y) return false;

            return (x == this.X - 1 || x == this.X + 1 || x == this.X) && (y == this.Y - 1 || y == this.Y + 1|| y == this.Y) || (y == this.Y && (x == this.X - 2 || x == this.X + 2));
        }

        // castle is set to false whenever the king object takes its first move, meaning the king is no longer able to castle
        public bool Castle
        {
            get
            {
                return castle;
            }
            set
            {
                castle = value;
            }
        }
    }
}