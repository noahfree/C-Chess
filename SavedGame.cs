using System;
using System.Collections.Generic;

namespace Chess
{
    // class SavedGame: used to serialize the game so that the board can be loaded when the program next opens
    [Serializable]
    internal class SavedGame
    {
        private Piece[,] piecesArray;
        private List<Piece> piecesList;
        private bool turnSwitch;
        private bool computer;

        public SavedGame(Piece[,] piecesArray, List<Piece> piecesList, bool turnSwitch, bool computer)
        {
            this.piecesArray = piecesArray;
            this.piecesList = piecesList;
            this.turnSwitch = turnSwitch;
            this.computer = computer;
        }

        public Piece[,] PiecesArray
        {
            get
            {
                return piecesArray;
            }
        }

        public List<Piece> PiecesList
        {
            get
            {
                return piecesList;
            }
        }

        public bool TurnSwitch
        {
            get
            {
                return turnSwitch;
            }
        }

        public bool Computer
        {
            get
            {
                return computer;
            }
        }
    }
}