using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chess
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadGame();
            CreateBoard();
        }

        // GLOBAL VARIABLES:
        private Square[,] board; // matrix of Square objects created when the visual board is created
        private Rectangle[,] grid; // matrix of Rectangle objects used to show available moves
        private Image[,] images; // matrix of Image objects used to move pieces on the board
        private Piece[,] piecesArray; // matrix of Piece objects used to easily access each piece based on its coordinates (x & y)
        private List<Piece> piecesList; // List of Piece objects used to iterate through pieces on the board
        private int xQueue = -1; // integer used to store the x coordinate of the Piece about to be moves
        private int yQueue = -1; // integer used to store the y coordinate " "
        private bool turnSwitch = false; // true = Black's turn, false = White's turn
        private bool newGame = true; // initialized to true when the board needs to be reset
        private Piece castle = null; // initialized to a Rook when the King is about to castle
        private SavedGame save = null; // used to load the game from the previous run of the program
        private bool computer = false; // computer = true when single player mode is enabled
        private King computerKing; // set to the king of the Black side, used by the computer in single player mode


/* SERIALIZATION FUNCTIONS: */

        // function OnWindowClose() saved the current game using serialization when the application closes, so it can be loaded next time
        private void OnWindowClose(object sender, EventArgs e)
        {
            FileStream filestream = new FileStream("SavedGame.txt", FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            // if newGame equals true, then saving information would be pointless, so a SavedGame object is created using null & false values, else the current game is saved
            if (!newGame) bf.Serialize(filestream, new SavedGame(piecesArray, piecesList, turnSwitch, computer));
            else bf.Serialize(filestream, new SavedGame(null, null, false, false));
            filestream.Close();
        }

        // function LoadGame() deserializes the SavedGame object and uses its information to initialize the primary variables
        private void LoadGame()
        {
            try
            {
                FileStream filestream = new FileStream("SavedGame.txt", FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                save = (SavedGame)bf.Deserialize(filestream);
                filestream.Close();
                piecesArray = save.PiecesArray;
                piecesList = save.PiecesList;
                turnSwitch = save.TurnSwitch;
                computer = save.Computer;
                if (computer) MainPage.Title = "CHESS - Single Player";
                else MainPage.Title = "CHESS - Multiplayer";
            }
            catch (Exception)
            {
                save = null;
                return;
            }
        }
/* end of SERIALIZATION FUNCTIONS */


/* CREATING GAME BOARD FUNCTIONS: */

        // function CreateBoard() is called when the program opens or when a new game begins; the function created the board by adding Rectangle
        // objects to the Canvas, and then calls other functions to place the pieces on the board
        public void CreateBoard()
        {
            xQueue = yQueue = -1;
            // three 8x8 matrices are initialized to contain the main board, the grid showing available moves, and the pieces
            board = new Square[8, 8];
            grid = new Rectangle[8, 8];
            images = new Image[8, 8];
            // a counter is used to ensure each square is the correct color
            int counter = 0;
            // square & tile are temporary variables used only for initialization of the values in the matrices
            Rectangle square;
            Rectangle tile;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    square = new Rectangle();
                    tile = new Rectangle();
                    square.Width = square.Height = 80;
                    tile.Width = tile.Height = 70;
                    // a MoutButtonEventHandler is applied to each transparent square, which will be above the main grid
                    tile.MouseDown += new MouseButtonEventHandler(SquareClick);
                    square.Margin = new Thickness(80 * i, 80 * j, 0, 0);
                    tile.Margin = new Thickness(80 * i + 5, 80 * j + 5, 0, 0);
                    if (counter % 2 == 0) square.Fill = new SolidColorBrush(Colors.SandyBrown);
                    else square.Fill = new SolidColorBrush(Colors.SaddleBrown);
                    tile.Fill = new SolidColorBrush(Colors.Transparent);
                    counter++;

                    // the rectangles are added to the Canvas 
                    MainGrid.Children.Add(square);
                    MainGrid.Children.Add(tile);
                    // a Square object is created and added to the board matrix
                    board[i, j] = new Square(square, i, j);
                    // the transparent Rectangle is added to the grid matrix
                    grid[i, j] = tile;
                }
                // counter is incremented again to offset the colors on the board, making the checker pattern
                counter++;
            }

            // if statement detects whether or not there is an available saved game to load
            if (save != null && piecesArray != null & piecesList != null)
            {
                // if so, the ResumeGame() function is called followed by a return
                ResumeGame();
                return;
            }

            // piecesArray & piecesList are initialized
            piecesArray = new Piece[8, 8];
            piecesList = new List<Piece>();
            // the first for loop creates each Pawn and they are added to the board
            for (int i = 0; i < 8; i++)
            {
                MakePiece(new Pawn(true, i, 1));
                MakePiece(new Pawn(false, i, 6));
            }
            // the second for loop creates and places every other piece using a switch statement
            for (int j = 0; j <= 7; j++)
            {
                // a switch is utilized to efficiently place the remaining 16 pieces
                switch (j)
                {
                    case 0:
                    case 7:
                        MakePiece(new Rook(true, j, 0));
                        MakePiece(new Rook(false, j, 7));
                        break;
                    case 1:
                    case 6:
                        MakePiece(new Knight(true, j, 0));
                        MakePiece(new Knight(false, j, 7));
                        break;
                    case 2:
                    case 5:
                        MakePiece(new Bishop(true, j, 0));
                        MakePiece(new Bishop(false, j, 7));
                        break;
                    case 3:
                        MakePiece(new Queen(true, j, 0));
                        MakePiece(new Queen(false, j, 7));
                        break;
                    case 4:
                        computerKing = new King(true, j, 0);
                        MakePiece(computerKing);
                        MakePiece(new King(false, j, 7));
                        break;
                }
            }
        }

        // function ResumeGame() sets up the board from the previous game's save if the load was successful
        private void ResumeGame()
        {
            foreach (Piece piece in piecesList)
            {
                // each piece in the list is placed on the board and added to the images matrix
                images[piece.X, piece.Y] = PlacePiece(piece);
                if (computer && piece is King && piece.Black)
                {
                    computerKing = piece as King;
                }
            }

            save = null;
        }

        // function MakePiece() takes a Piece object and creates an image using the object, which is then placed on the board and
        // added to the images matrix
        public void MakePiece(Piece newPiece)
        {
            Image image = PlacePiece(newPiece);
            // the new piece is also added to piecesArray and piecesList
            piecesArray[newPiece.X, newPiece.Y] = newPiece;
            piecesList.Add(newPiece);
            images[newPiece.X, newPiece.Y] = image;
        }

        // function PlacePiece() creates an Image object based on the inputted piece using its x & y coordinates; the image's Cursor is
        // also set as well as putting a MouseButtonEventHandler on it and then finally adding it to the Canvas before returning it
        public Image PlacePiece(Piece piece)
        {
            Image image = new Image();
            image.Width = image.Height = 80;
            image.Margin = new Thickness(80 * piece.X, 80 * piece.Y, 0, 0);
            if (piece.White && !turnSwitch || piece.Black && turnSwitch) image.Cursor = Cursors.Hand;
            image.MouseDown += new MouseButtonEventHandler(SquareClick);
            image.Source = new BitmapImage(new Uri(piece.FilePath, UriKind.Absolute));
            MainGrid.Children.Add(image);
            return image;
        }
/* end of CREATING GAME BOARD FUNCTIONS */


/* EVENT HANDLER FUNCTIONS: */

        // function SquareClick is an event handler that is called every time a tile or a piece is clicked by the user; the function either
        // shows the available moves or performs a move, depending on if this is the first or second click of the turn
        public void SquareClick(object sender, MouseButtonEventArgs e)
        {
            // if xQueue is -1, then the valid moves need to be displayed if this is a valid click, meaning the sender was an image & not a rectangle
            if (xQueue == -1)
            {
                // if the sender is not an image, then the click was likely a misclick, so nothing happens
                if (sender is Image)
                {
                    Image piece = sender as Image;
                    // xQueue & yQueue are set to the coordinates of the piece clicked using the Margin property
                    xQueue = (int)piece.Margin.Left / 80;
                    yQueue = (int)piece.Margin.Top / 80;
                    // if statement ensure that the click was on the correct color; i.e. a white piece being click on black's turn does not do anything & vice versa
                    if (turnSwitch && piecesArray[xQueue, yQueue].White || !turnSwitch && piecesArray[xQueue, yQueue].Black) xQueue = yQueue = -1;
                    else if (ShowMoves(null)) // else, ShowMoves() is called which displays the valid moves of the selected piece
                    {
                        // if ShowMoves() returns true, then the piece selected has no available moves, so a message is displayed 
                        MessageBoxResult result = MessageBox.Show("Selected piece has no available moves.", "Unable to move this piece");
                        // SwitchTurn() is called to merely reset the Cursors of the images & tiles, not to actually switch the turn
                        SwitchTurn();
                        // xQueue & yQueue are reset to -1 to allow a new piece to be selected
                        xQueue = yQueue = -1;
                    }
                }
            }
            else // this else statement runs if a piece has already been selected, meaning the if ran previously and xQueue/yQueue are not -1
            {
                // newGame is set to false to ensure that a new game is only created if a King is taken on this turn
                newGame = false;
                int x, y;
                // sender is Rectangle means the click came from an emtpy tile
                if (sender is Rectangle)
                {
                    Rectangle tile = sender as Rectangle;
                    // x & y variables are initialized to the tile's coordinates using the Margin property
                    x = (int) tile.Margin.Left / 80;
                    y = (int) tile.Margin.Top / 80;
                    // if xQueue equals x & yQueue equals y, this means that the same piece selected initially was selected again, which
                    // in this program means that the user wants to move a different piece, so xQueue & yQueue are set to -1 and the grid
                    // is cleared
                    if (xQueue == x && yQueue == y)
                    {
                        xQueue = yQueue = -1;
                        ClearGrid();
                        return;
                    }
                    // instead of running the CheckAll() function again, we can now see if the chosen tile is a valid move by seeing whether
                    // or not the tile selected has a Hand as the cursor, if so then the move is valid
                    if (grid[x, y].Cursor == Cursors.Hand)
                    {
                        // the following if statement is one last check only for moving a King to an empty spot, and this call of CheckAll()
                        // enables kings to castle
                        if (piecesArray[xQueue, yQueue] is King) CheckAll(x, y);
                        // finally, MovePiece() is called to move the chosen piece, and then grid is then cleared
                        MovePiece(x, y);
                        ClearGrid();
                    }
                }
                // sender is Image means the click came from a tile with a piece on it
                else if (sender is Image)
                {
                    Image piece = sender as Image;
                    // x & y variables are set to the piece's coordinates using the Margin property
                    x = (int)piece.Margin.Left / 80;
                    y = (int)piece.Margin.Top / 80;
                    // if xQueue equals x & yQueue equals y, this means that the same piece selected initially was selected again, which
                    // in this program means that the user wants to move a different piece, so xQueue & yQueue are set to -1 and the grid
                    // is cleared
                    if (xQueue == x && yQueue == y)
                    {
                        xQueue = yQueue = -1;
                        ClearGrid();
                        return;
                    }
                    // instead of running the CheckAll() function again, we can now see if the chosen tile is a valid move by seeing whether
                    // or not the tile selected has a Hand as the cursor, if so then the move is valid
                    if (images[x, y].Cursor == Cursors.Hand)
                    {
                        // finally, MovePiece() is called to move the chosen piece, and then grid is then cleared
                        MovePiece(x, y);
                        ClearGrid();
                    }
                }
            }
        }

        // function ToSingle() is an event handler that switches the mode from multiplayer to singleplayer; if the mode is already single player, nothing happens
        private void ToSingle(object sender, EventArgs e)
        {
            // computer variable equals true if the game is already in single player mode
            if (computer) return;
            computer = true;
            MainPage.Title = "CHESS - Single Player";
            NewGame(sender, e);
        }

        // function ToMulti() is an event handler that switches the mode from singleplayer to multiplayer; if the mode is already multiplayer, nothing happens
        private void ToMulti(object sender, EventArgs e)
        {
            // computer variable equals false if the game is already in multiplayer mode
            if (!computer) return;
            computer = false;
            MainPage.Title = "CHESS - Multiplayer";
            NewGame(sender, e);
        }

        // function NewGame() is an event handler called when the New Game button is clicked; the function resets variables and calls CreateBoard()
        public void NewGame(object sender, EventArgs e)
        {
            turnSwitch = false;
            piecesArray = null;
            piecesList = null;
            CreateBoard();
        }

        // function ExitGame() is an event handler called when the Exit Game button is clicked; it caused the program to shut down
        public void ExitGame(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
/* end of EVENT HANDLERS */


/* MOVE FUNCTIONS: */

        // function ComputerMove() is called in single player mode to move the computer based on an algorithm analyzing the pieces on the board
        // in their current positions; the algorithm is lacking in some ways, primarily because it does not look ahead at all, so it can easily
        // be outsmarted. ideally, a chess AI would look ahead at least 4 moves and analyze every possible move taking into account every possible
        // outcome and choose the best one
        private void ComputerMove()
        {
            // the following variables are used to compute the most effective piece to be moves
            ComputerPiece selected;
            ComputerPiece bestPlay = null;
            double current, next;
            current = -1000;

            // every piece is iterated through, but a random number is generated to choose the starting point so that the computer does
            // not move the same way at the beginning of every game
            Random random = new Random();
            int query = random.Next(piecesList.Count);
            Piece piece;
            // for loop iterates the same number of times as the count of piecesList, a forreach loop would provide the same result
            for (int i = 0; i < piecesList.Count; i++)
            {
                // variable piece is set to the current piece being analyzed
                piece = piecesList[query];
                // the computer is always black, so is the Piece is White it is not analyzed
                if (piece.Black)
                {
                    xQueue = computerKing.X;
                    yQueue = computerKing.Y;
                    // this if statement will make the computer's king evade check if it is able to itself
                    if (IsCheck(false, computerKing.X, computerKing.Y) && !(piece is King) && !ShowMoves(null))
                    {
                        query++;
                        if (query == piecesList.Count) query = 0;
                        continue;
                    }
                    // a ComputerPiece object is created that stores a list of its valid moves
                    selected = new ComputerPiece(piece);
                    // as the ShowMoves function uses the xQueue & yQueue values to show which moves are available, these variables need to 
                    // be set to the coordinates of the current piece befor each call of ShowMoves()
                    xQueue = piece.X;
                    yQueue = piece.Y;
                    // ShowMoves() is called which adds Point objects to the list contained in the current ComputerPiece object
                    ShowMoves(selected.ValidMoves);
                    // a point value is awarded for each possible move in the object's ValidMoves list
                    foreach (Point move in selected.ValidMoves)
                    {
                        // each Pieces cost, which can be found in the class declaration for each piece, is used to calculate the value of the move
                        if (piecesArray[(int)move.X, (int)move.Y] == null)
                        {
                            next = 0 - piece.Cost / 4;
                        }
                        else if (piecesArray[(int)move.X, (int)move.Y] is King)
                        {
                            next = 1000;
                        }
                        else
                        {
                            next = piecesArray[(int)move.X, (int)move.Y].Cost - piece.Cost / 4;
                        }

                        if (IsCheck(false, (int)move.X, (int)move.Y))
                        {
                            next -= piece.Cost;
                        }

                        if (computerKing.VerifyMove(xQueue, yQueue))
                        {
                            next -= 0.15;
                        }

                        if (IsCheck(false, piece.X, piece.Y))
                        {
                            next += piece.Cost;
                        }

                        // if this move is better than the previous move, current and bestPlay are updated accordingly
                        if (next > current)
                        {
                            current = next;
                            bestPlay = selected;
                            bestPlay.X = (int)move.X;
                            bestPlay.Y = (int)move.Y;
                        }
                    }
                }
                // if the loop has reached the end of the piecesList, query is set back to 0
                query++;
                if (query == piecesList.Count) query = 0;
            }

            // finally, xQueue & yQueue are set to the coordinates of the best piece to move
            xQueue = bestPlay.Piece.X;
            yQueue = bestPlay.Piece.Y;
            if (piecesArray[xQueue, yQueue] is King)
            {
                computerKing.X = xQueue;
                computerKing.Y = yQueue;
            }
            // MovePiece is called to complete the move
            MovePiece(bestPlay.X, bestPlay.Y);
        }

        // function ShowMoves() uses the transparent matrix between the board and the pieces to show the available moves for the selected piece
        public bool ShowMoves(List<Point> validMoves)
        {
            // variable 'cannotMove' is returned by the function and tells whether or not the piece has any valid moves
            bool cannotMove = true;
            foreach (Square square in board)
            {
                // for every square on the board, the CheckAll() function is called which returns true if the move is valid
                if (CheckAll(square.X, square.Y))
                {
                    // the validMoves list is only used by the computer in single player mode, so in multiplayer mode it will
                    // be equal to null, and nothing will be added. if the list has been initialized, then a Point object is created
                    // for the given x & y and it is added to the list
                    if (validMoves != null) validMoves.Add(new Point(square.X, square.Y));
                    // switch statement is used to make the tiles of available moves different colors for each side (DimGray vs DarkGray)
                    switch (turnSwitch)
                    {
                        case true:
                            grid[square.X, square.Y].Fill = new SolidColorBrush(Colors.DimGray);
                            break;
                        case false:
                            grid[square.X, square.Y].Fill = new SolidColorBrush(Colors.DarkGray);
                            break;
                    }
                    // the Cursor for the image (piece) or rectangle (tile) is set to Hand so it is obvious to the user that it can
                    // be chosen. the Cursor property is also used in the Move function to detect whether a move has been validified 
                    if (piecesArray[square.X, square.Y] != null)
                    {
                        images[square.X, square.Y].Cursor = Cursors.Hand;
                    }
                    else grid[square.X, square.Y].Cursor = Cursors.Hand;
                    // cannotMove is set equal to false, since one possible move means that the piece is able to move
                    cannotMove = false;
                }
                else
                {
                    // if the location selected cannot be moved to, its Cursor is set to arrow to make that obvious
                    if (piecesArray[square.X, square.Y] != null)
                    {
                        images[square.X, square.Y].Cursor = Cursors.Arrow;
                    }
                    else grid[square.X, square.Y].Cursor = Cursors.Arrow;
                }
            }
            // cannotMove is returned
            return cannotMove;
        }

        // function MovePiece() assumes that the move is valid, and moves the piece at (xQueue, yQueue) to (x, y)
        public void MovePiece(int x, int y)
        {
            string title, caption;
            title = caption = "";
            newGame = false;
            // if (x, y) contains a piece, then it needs to be removed to make room for the piece moving there (which is currently at xQueue, yQueue)
            if (piecesArray[x, y] != null)
            {
                // the piece's image is removed from the grid, and the piece is removed from the piecesList
                MainGrid.Children.Remove(images[x, y]);
                piecesList.Remove(piecesArray[x, y]);
                // if the piece is a king, then the game as been won, so newGame is toggled to true and title/caption are initialized accordingly
                if (piecesArray[x, y] is King)
                {
                    newGame = true;
                    title = "Game is complete";
                    switch (piecesArray[x, y].Black)
                    {
                        case true:
                            caption = "WHITE has won the game.";
                            break;
                        case false:
                            caption = "BLACK has won the game.";
                            break;
                    }
                }
                // the piece in the array is also set to null
                piecesArray[x, y] = null;
            }
            // the following if statement is used to "Queen" a pawn if a pawn reaches the opposite side of the board; i.e. if a white Pawn reaches
            // y-coordinate 0 or if a black Pawn reaches y-coordinate 7
            if (piecesArray[xQueue, yQueue] is Pawn && (piecesArray[xQueue, yQueue].Black && y == 7 || !piecesArray[xQueue, yQueue].Black && y == 0))
            {
                // the next few lines of code basically just make a Queen object and replace the pawn with it
                Piece queen = new Queen(piecesArray[xQueue, yQueue].Black, xQueue, yQueue);
                MainGrid.Children.Remove(images[xQueue, yQueue]);
                images[xQueue, yQueue] = PlacePiece(queen);
                piecesList.Remove(piecesArray[xQueue, yQueue]);
                piecesArray[xQueue, yQueue] = queen;
                piecesList.Add(queen);
            }
            // the following if statement is used to castle, which means moving a rook on the other side of the king;
            // the castle variable will equal the instance of the Rook to be castle if the user has chosen to castle
            if (piecesArray[xQueue, yQueue] is King && castle != null)
            {
                // the rook has to be moved differently based on whether it is in a left corner or right corner
                // (castling kingside vs castling queenside)
                if (castle.X < piecesArray[xQueue, yQueue].X)
                {
                    // the Rook is moved to its new spot on the right side of the king, and then the image matrix and the pieces array
                    // are updated accordingly, along with the rook's new coordinates
                    images[castle.X, castle.Y].Margin = new Thickness(80 * (castle.X + 3), 80 * castle.Y, 0, 0);
                    images[castle.X + 3, castle.Y] = images[castle.X, castle.Y];
                    piecesArray[castle.X + 3, castle.Y] = castle;
                    castle.X = castle.X + 3;
                    images[castle.X - 3, castle.Y] = null;
                    piecesArray[castle.X - 3, castle.Y] = null;
                }
                else
                {
                    // the Rook is moved to its new spot on the left side of the king, and then the image matrix and the pieces array
                    // are updated accordingly, along with the rook's new coordinates
                    images[castle.X, castle.Y].Margin = new Thickness(80 * (castle.X - 2), 80 * castle.Y, 0, 0);
                    images[castle.X - 2, castle.Y] = images[castle.X, castle.Y];
                    piecesArray[castle.X - 2, castle.Y] = castle;
                    castle.X = castle.X - 2;
                    images[castle.X + 2, castle.Y] = null;
                    piecesArray[castle.X + 2, castle.Y] = null;
                }
                // castle is equal to null again
                castle = null;

                // the king will then be moved normally by the following lines of code
            }
            
            // the following lines of code move the selected piece to its new coordinate by changing the margin on the image, and then
            // the image matrix, the pieces array, and the piece's coordinates are updated
            images[xQueue, yQueue].Margin = new Thickness(80 * x, 80 * y, 0, 0);
            images[x, y] = images[xQueue, yQueue];
            images[xQueue, yQueue] = null;
            piecesArray[x, y] = piecesArray[xQueue, yQueue];
            piecesArray[xQueue, yQueue] = null;
            piecesArray[x, y].X = x;
            piecesArray[x, y].Y = y;
            // a King is not allowed to castle if it has already moved, so after a king moves its castle property is set to false
            if (piecesArray[x, y] is King) (piecesArray[x, y] as King).Castle = false;
            // xQueue & yQueue are reset to -1 and the turnSwitch is toggled
            xQueue = yQueue = -1;
            turnSwitch = !turnSwitch;
            // function SwitchTurn() is called to reset the board for the next turn
            SwitchTurn();
            // if variable newGame is true, the game has just ended, so the board and variables need to be reset; CreateBoard() is then called
            if (newGame)
            {
                foreach (Rectangle tile in grid) tile.Fill = new SolidColorBrush(Colors.Transparent);
                MessageBoxResult result = MessageBox.Show(caption, title);
                turnSwitch = false;
                piecesArray = null;
                piecesList = null;
                CreateBoard();
            }

            // if turnSwitch equals true, meaning it is Black's turn, and computer equals true, meaning single player mode is enabled and
            // the computer needs to take its turn, then function ComputerMove() is called
            if (turnSwitch && computer) ComputerMove();
        }

        

        // function SwitchTurn() causes the turn to be switched, meaning Cursors are reset based on whose turn it is
        public void SwitchTurn()
        {
            foreach (Piece piece in piecesList)
            {
                if (piece.Black && turnSwitch || piece.White && !turnSwitch)
                {
                    images[piece.X, piece.Y].Cursor = Cursors.Hand;
                }
                else
                {
                    images[piece.X, piece.Y].Cursor = Cursors.Arrow;
                }
            }
        }

        // function ClearGrid() simply makes the grid matrix fully transparent again, as to not show any available moves; each tile's cursor
        // is also set to 'Arrow', & SwitchTurn() is then called
        public void ClearGrid()
        {
            foreach (Rectangle tile in grid)
            {
                tile.Fill = new SolidColorBrush(Colors.Transparent);
                tile.Cursor = Cursors.Arrow;
            }
            SwitchTurn();
        }
/* end of MOVE FUNCTIONS */


/* CHECK FUNCTIONS: */

        // function CheckAll() does multiple checks on the given piece using the inputted x & y coordinates; the piece is known by the global
        // variables xQueue & yQueue
        public bool CheckAll(int x, int y)
        {
            // CheckSpace() is called to see if the space is occupied; toggle will equal -1 if it is not, 0 if occupied by White, & 1 if 
            // occupied by Black
            int toggle = CheckSpace(x, y);
            // since pawns move unlike any other piece on the board, as their movement is based both on their color and whether there is a
            // piece diagonal to them, these nested if statements are used to decipher if the Pawn is able to attack
            if (piecesArray[xQueue, yQueue] is Pawn && (toggle == 1 && piecesArray[xQueue, yQueue].White || toggle == 0 && piecesArray[xQueue, yQueue].Black))
            {
                // the outer if statement (above) checks that the space being moved to not occupied by a piece the same color as the pawn

                // the inner if statement (below) sees if the pawn is able to attack the piece occupying tile (x,y)
                if (!(piecesArray[xQueue, yQueue].Black && (x == xQueue - 1 && y == yQueue + 1 || x == xQueue + 1 && y == yQueue + 1)) &&
                    !(piecesArray[xQueue, yQueue].White && (x == xQueue - 1 && y == yQueue - 1 || x == xQueue + 1 && y == yQueue - 1)))
                    return false; // false is returned if the space the pawn is moving to is occupied but is not diagonal to the pawn, since
                                  // pawns cannot take out pieces that are directly in front of them
            }
            else
            {
                // each piece has a unique VerifyMove() function that verifies whether the move is valid based solely on the coordinates of
                // the piece and where it is trying to move; if VerifyMove() returns false, then the move is not valid, so false is returned
                bool valid = piecesArray[xQueue, yQueue].VerifyMove(x, y);
                if (!valid) return false;

                // the following if statement checks whether the piece occupying the tile that the selected piece is attempting to move to 
                // and the selected piece are the same color; if so, this is not valid so false is returned
                if (toggle == 1 && piecesArray[xQueue, yQueue].Black || toggle == 0 && piecesArray[xQueue, yQueue].White) return false;

                // the final function called to ensure the move is valid is FinalCheck(), which runs differently based on the type of the selected
                // piece, but essentially FinalCheck() ensures that there are not any other pieces on the board preventing this move from being
                // valid
                bool boolean = FinalCheck(piecesArray[xQueue, yQueue], x, y, x, y);
                if (!boolean) return false;
            }

            // if the function has not returned false, then the move is valid, so true is returned
            return true;
        }

        // function CheckSpace() returns 1 if space (x,y) is occupied by a black piece, 0 if white piece, and -1 if empty
        public int CheckSpace(int x, int y)
        {
            if (piecesArray[x, y] == null)
            {
                return -1;
            }
            else if (piecesArray[x, y].Black)
            {
                return 1;
            }
            else return 0;
        }

        // function FinalCheck() essentially checks if there are any pieces preventing the given move from being legal; depending on the type
        // of piece that is passed to the function, it may be called recusively, so parameters constX & constY are utilized to store the initial
        // x and y values; true is returned if the move is legal
        public bool FinalCheck(Piece piece, int x, int y, int constX, int constY)
        {
            // if the x equals the x coordinate of piece and y equals the y coordinate of piece, then the function has been called recursively
            // successfully and should now return true as it is a valid move
            if (piece.X == x && piece.Y == y) return true;

            if (piece is Pawn)
            {
                if (piecesArray[x, y] == null)
                {
                    // if statement ensures that a Pawn cannot jump over another piece on its first turn
                    if ((piece.Black && yQueue == 1 && y == 3 && piecesArray[xQueue, 2] != null) || (piece.White && yQueue == 6 && y == 4 && piecesArray[xQueue, 5] != null)) return false;
                    else return true;
                }
                else return false;
            }
            if (piece is Rook)
            {
                // the following lines of code essentially ensure that are are no pieces between the inputted x & y coordinates and the piece, as
                // rooks are not allowed to jump over any pieces
                if (piece.X < x)
                {
                    if (piecesArray[x, y] == null || x == constX && y == constY)
                    {
                        return FinalCheck(piece, x - 1, y, constX, constY);
                    }
                    return false;
                }
                else if (piece.X > x)
                {
                    if (piecesArray[x, y] == null || x == constX && y == constY)
                    {
                        return FinalCheck(piece, x + 1, y, constX, constY);
                    }
                    return false;
                }
                else if (piece.Y < y)
                {
                    if (piecesArray[x, y] == null || x == constX && y == constY)
                    {
                        return FinalCheck(piece, x, y - 1, constX, constY);
                    }
                    return false;
                }
                else
                {
                    if (piecesArray[x, y] == null || x == constX && y == constY)
                    {
                        return FinalCheck(piece, x, y + 1, constX, constY);
                    }
                    return false;
                }
            }
            if (piece is Knight)
            {
                // knights are allowed to go over pieces, so true is returned regardless
                return true;
            }
            if (piece is Bishop)
            {
                // the following lines of code essentially ensure that are are no pieces between the inputted x & y coordinates and the piece, as
                // bishops are not allowed to jump over any pieces
                if (piece.X < x && piece.Y < y)
                {
                    if (piecesArray[x, y] == null || x == constX && y == constY)
                    {
                        return FinalCheck(piece, x - 1, y - 1, constX, constY);
                    }
                    return false;
                }
                else if (piece.X > x && piece.Y > y)
                {
                    if (piecesArray[x, y] == null || x == constX && y == constY)
                    {
                        return FinalCheck(piece, x + 1, y + 1, constX, constY);
                    }
                    return false;
                }
                else if (piece.X > x && piece.Y < y)
                {
                    if (piecesArray[x, y] == null || x == constX && y == constY)
                    {
                        return FinalCheck(piece, x + 1, y - 1, constX, constY);
                    }
                    return false;
                }
                else
                {
                    if (piecesArray[x, y] == null || x == constX && y == constY)
                    {
                        return FinalCheck(piece, x - 1, y + 1, constX, constY);
                    }
                    return false;
                }
            }
            if (piece is Queen)
            {
                // as Queens are able to move like a rook or a bishop, depending on the coordinates of the Queen in relation to the
                // x and y values, the FinalCheck() function is called with either a Rook or a Bishop as the first parameters
                if (piece.X == x || piece.Y == y) return FinalCheck(new Rook(true, piece.X, piece.Y), x, y, constX, constY);
                else return FinalCheck(new Bishop(true, piece.X, piece.Y), x, y, constX, constY);
            }
            if (piece is King)
            {
                // the king's segment of this function tests whether the king is allowed to castle, and it also ensures that check will not
                // occur as a result of the king's move
                castle = null;

                // this if statement runs if castling cannot be attempted given the x and y coordinates, so the function IsCheck() is called
                // to ensure that moving to (x,y) will not put the king into check
                if (piece.Y != y || piece.X - 1 == x || piece.X + 1 == x)
                {
                    return !IsCheck(!piece.Black, x, y);
                }

                // this if statement funs if the king has not moved yet this game
                if (piece.X == 4 && ((piece.Y == 0 && piece.Black) || (piece.Y == 7 && piece.White)) && (piece as King).Castle)
                {
                    // if x is less than piece.X, then queenside castling must take place
                    if (x < piece.X)
                    {
                        // for loop ensures that there are no pieces occupying the spaces between the king and rook
                        for (int i = 3; i > 0; i--)
                        {
                            if (piecesArray[i, piece.Y] != null) return false;
                        }
                        // outer if statement ensures there is a rook of the correct color in the correct spot
                        if (piecesArray[0, piece.Y] != null && (piecesArray[0, piece.Y].Black && piecesArray[0, piece.Y] is Rook && piece.Black || piecesArray[0, piece.Y].White && piecesArray[0, piece.Y] is Rook && piece.White))
                        {
                            // inner if statement ensures that either of the spaces to the left of the king will not put him into check
                            if (!IsCheck(!piece.Black, 2, piece.Y) && !IsCheck(!piece.Black, 3, piece.Y)){
                                // variable castle is set to the rook
                                castle = piecesArray[0, piece.Y];
                                return true;
                            }
                        }
                    }
                    else // else, kingside castling must take place
                    {
                        // for loop ensures that there are no pieces occupying the spaces between the king and rook
                        for (int i = 5; i < 7; i++)
                        {
                            if (piecesArray[i, piece.Y] != null) return false;
                        }
                        // outer if statement ensures there is a rook of the correct color in the correct spot
                        if (piecesArray[7, piece.Y] != null && (piecesArray[7, piece.Y].Black && piecesArray[7, piece.Y] is Rook && piece.Black || piecesArray[7, piece.Y].White && piecesArray[7, piece.Y] is Rook && piece.White))
                        {
                            // inner if statement ensures that either of the spaces to the right of the king will not put him into check
                            if (!IsCheck(!piece.Black, 5, piece.Y) && !IsCheck(!piece.Black, 6, piece.Y)){
                                // variable castle is set to the rook
                                castle = piecesArray[7, piece.Y];
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        // function IsCheck() is designed for the king and returns true if the opposing side is able to get to the king at location (x,y)
        public bool IsCheck(bool isBlack, int x, int y)
        {
            // the current xQueue & yQueue are stored in tempX and tempY as to not lose the values
            int tempX = xQueue;
            int tempY = yQueue;

            // enable is returned at the end of the function
            bool enable = false;
            foreach (Piece piece in piecesList)
            {
                // variable 'isBlack' is the color of the opposing pieces, so if isBlack is true we want each piece to be Black, and
                // if isBlack is false we want each piece to be White
                if (piece.Black && isBlack || piece.White && !isBlack)
                {
                    // used the King's function VerifyMove() to see if the piece can move to spot (x, y)
                    if (piece is King && (piece as King).VerifyMove(x, y))
                    {
                        enable = true;
                        break;
                    }
                    else if (piece is King) continue; // if piece is a King and move is not verified, move onto next iteration

                    // since CheckAll() used xQueue & yQueue, the variables need to be set to the coordinates of the current piece
                    xQueue = piece.X;
                    yQueue = piece.Y;
                    // a pawn is a special case since pawn's can only move diagonal when taking a piece and vertical when not taking a piece
                    if (piece is Pawn)
                    {
                        // if coordinate (x, y) is in either diagonal of the pawn, enable is set to true
                        if (((piece.Black && isBlack && piece.Y + 1 == y) || (piece.White && !isBlack && piece.Y - 1 == y)) && (piece.X - 1 == x || piece.X + 1 == x))
                        {
                            enable = true;
                            break;
                        }
                    } // every other piece can use the function CheckAll() to check if it can move to coordinate (x, y)
                    else if (CheckAll(x, y))
                    {
                        enable = true;
                        break;
                    }
                }
            }

            // the xQueue & yQueue variables are set back to their original values, and enable is returned
            xQueue = tempX;
            yQueue = tempY;
            return enable;
        }
/* end of CHECK FUNCTIONS */
    }
}
