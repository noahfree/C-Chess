using System.Windows.Shapes;

namespace Chess
{
    // class Square: used to create the game board, holds an x & y value and a Rectangle object
    internal class Square
    {
        private Rectangle tile;
        private int x;
        private int y;

        public Square(Rectangle tile, int x, int y)
        {
            this.tile = tile;
            this.x = x;
            this.y = y;
        }

        public Rectangle Tile
        {
            get
            {
                return tile;
            }
        }

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
        }
    }
}