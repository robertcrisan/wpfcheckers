﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections;
using System.Data;
using System.Resources;
using System.Windows.Media.Animation;

namespace Checkers
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
        private enum Turn
        {
            Red,
            Black
        }

        #region Globals
        
        private bool IsDragging;
        private Point _startPoint;
        private Point _endPoint;
        private CheckerPiece currentPiece;
        private CheckerPiece capturedPiece; // this is messy, but I can't think of a better way to pass the captured piece to the Completed event handler.
        private Turn currentTurn;

        #endregion

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            this.grdBoard.Drop += new DragEventHandler(grdBoard_Drop);
            this.grdBoard.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(grdBoard_PreviewMouseLeftButtonUp);            
        }
        
        /// <summary>
        /// Here, the source is the drop target which in this case is a Label. This is needed to get
        /// a reference to the underlying grid cell. That way we know the cell to which to add the new 
        /// image. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdBoard_Drop(object sender, DragEventArgs e)
        {
            // use the label in the cell to get the current row and column
            EmptySpace l = e.Source as EmptySpace;
            int r = Grid.GetRow((EmptySpace)e.Source);
            int c = Grid.GetColumn((EmptySpace)e.Source);
            bool okToMove = false;
            
            // Because both RedChecker and BlackChecker derive from CheckerPiece, we can use polymorphism
            // to create the correct piece. Get the correct piece and determine if the move is valid.
            // A valid move is one row forward to an unoccupied space.
            CheckerPiece checker;
            if (currentPiece is RedChecker)
            {
                if (currentTurn != Turn.Red)
                {
                    System.Windows.Forms.MessageBox.Show("It's not your turn");
                    return; // it's not your turn.
                }

                // It's red's turn...
                checker = new RedChecker();
                if (l.row == currentPiece.row + 1 && (l.col == currentPiece.col+1 || l.col==currentPiece.col-1))
                    okToMove = true;
                
                //now check to see if we captured anything
                BlackChecker opponentPiece ;
                if (c == currentPiece.col + 2)
                {
                    opponentPiece = grdBoard.Children.OfType<BlackChecker>().Where(p => p.row == currentPiece.row + 1 && (p.col == currentPiece.col + 1)).SingleOrDefault();
                }
                else
                {
                    opponentPiece = grdBoard.Children.OfType<BlackChecker>().Where(p => p.row == currentPiece.row + 1 && (p.col == currentPiece.col - 1)).SingleOrDefault();
                }

                if (opponentPiece != null && l.row - currentPiece.row == 2)
                {
                    int validCol = (opponentPiece.col > currentPiece.col) ? currentPiece.col + 2 : currentPiece.col - 2;
                    if (r == currentPiece.row + 2 && c == validCol)
                    {
                        Storyboard PieceCaptured = opponentPiece.Resources["PieceCaptured"] as Storyboard;
                        capturedPiece = opponentPiece;

                        if (PieceCaptured != null)
                        {
                            PieceCaptured.Completed += new EventHandler(RemovePiece);
                            PieceCaptured.Begin();
                        }
                        
                        okToMove = true;
                    }
                }
                if (okToMove)
                {                    
                    currentTurn = Turn.Black;
                }
            }
            else
            {
                if (currentTurn != Turn.Black)
                {
                    System.Windows.Forms.MessageBox.Show("It's not your turn");
                    return; // it's not your turn.
                }

                // It's black's turn...
                checker = new BlackChecker();
                if (l.row == currentPiece.row - 1 && (l.col == currentPiece.col + 1 || l.col == currentPiece.col - 1))
                    okToMove = true;

                RedChecker opponentPiece = null;
                if (c == currentPiece.col + 2)
                {
                    opponentPiece = grdBoard.Children.OfType<RedChecker>().Where(p => p.row == currentPiece.row - 1 && (p.col == currentPiece.col + 1)).SingleOrDefault();
                }
                else if (c== currentPiece.col - 2)
                {
                    opponentPiece = grdBoard.Children.OfType<RedChecker>().Where(p => p.row == currentPiece.row - 1 && (p.col == currentPiece.col - 1)).SingleOrDefault();
                }

                //FIXME: capturing a piece to the left isn't working
                if (opponentPiece != null && currentPiece.row -l.row == 2)
                {
                    int validCol = (opponentPiece.col > currentPiece.col) ? currentPiece.col + 2 : currentPiece.col - 2;
                    if (r == currentPiece.row - 2 && c == validCol)
                    {
                        capturedPiece = opponentPiece;
                        Storyboard PieceCaptured = opponentPiece.Resources["PieceCaptured"] as Storyboard;
                        if (PieceCaptured != null)
                        {
                            PieceCaptured.Completed += new EventHandler(RemovePiece);
                            PieceCaptured.Begin();
                        }
                        okToMove = true;
                    }
                }
                if (okToMove)
                    currentTurn = Turn.Red;
            }

            if (okToMove)
            {
                checker.col = c;
                checker.row = r;

                // bind the mouse events
                checker.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(grdBoard_PreviewMouseLeftButtonDown);
                checker.PreviewMouseMove += new MouseEventHandler(grdBoard_PreviewMouseMove);
                checker.Cursor = Cursors.Hand;
                checker.AllowDrop = false;

                // add the piece to the board
                Grid.SetRow(checker, r);
                Grid.SetColumn(checker, c);
                this.grdBoard.Children.Remove(currentPiece);
                grdBoard.Children.Add(checker);
                Storyboard DropPiece = checker.Resources["DropPiece"] as Storyboard;
                if (DropPiece != null)
                {
                    DropPiece.Begin();
                }
                
            }
        }

        void RemovePiece(object sender, EventArgs e)
        {
            if (capturedPiece != null)
            {
                capturedPiece.Visibility = Visibility.Hidden;

                if (capturedPiece is RedChecker)
                {
                    RedChecker deadman = new RedChecker();
                    Storyboard AddToGraveyard = deadman.Resources["AddToGraveyard"] as Storyboard;
                    this.pnlBlackGraveyard.Children.Add(deadman);
                    AddToGraveyard.Begin();
                }
                else
                {
                    BlackChecker deadman = new BlackChecker();
                    Storyboard AddToGraveyard = deadman.Resources["AddToGraveyard"] as Storyboard;
                    this.pnlRedGraveyard.Children.Add(deadman);
                    AddToGraveyard.Begin();
                }
                grdBoard.Children.Remove(capturedPiece);
            }
        }

		public Window1()
		{
			this.InitializeComponent();
            this.grdBoard.AllowDrop = true;
            ResetGame();
		}

        /// <summary>
        /// This function loads the game pieces into the grid cells and prepares
        /// </summary>
        private void ResetGame()
        {
            currentTurn = Turn.Red; // Red goes first.
            this.pnlBlackGraveyard.Children.Clear();
            this.pnlRedGraveyard.Children.Clear();

            int col = 0;
            for (int row = 0; row < grdBoard.RowDefinitions.Count; row++)
            {
                for (col = 0; col < grdBoard.ColumnDefinitions.Count; col ++)
                {
                    Border b = new Border();
                    b.BorderThickness = new Thickness(1, 1, 1, 1);
                    b.BorderBrush = Brushes.Black;
                    Grid.SetColumn(b, col);
                    Grid.SetRow(b, row);
                    this.grdBoard.Children.Add(b);
                }
            }

            for (int row = 0; row < grdBoard.RowDefinitions.Count; row++)
            {
                // put a piece in every other cell
                for (col = (col % 2 != 0 ? 0 : 1); col < grdBoard.ColumnDefinitions.Count; col += 2)
                {                    
                    EmptySpace l = new EmptySpace();
                    l.Margin = new Thickness(0, 0, 0, 0);
                    l.AllowDrop = true;
                    l.Background = Brushes.Black;
                    l.Name = "Label" + (row * col).ToString();
                    l.Drop += new DragEventHandler(grdBoard_Drop);
                    l.col = col;
                    l.row = row;

                    Grid.SetColumn(l, col);
                    Grid.SetRow(l, row);
                    this.grdBoard.Children.Add(l);

                    if (row < 3)
                    {
                        RedChecker redChecker = new RedChecker();
                        redChecker.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(grdBoard_PreviewMouseLeftButtonDown);
                        redChecker.PreviewMouseMove += new MouseEventHandler(grdBoard_PreviewMouseMove);
                        redChecker.Cursor = Cursors.Hand;
                        redChecker.AllowDrop = false;
                        redChecker.col = col;
                        redChecker.row = row;

                        Grid.SetColumn(redChecker, col);
                        Grid.SetRow(redChecker, row);
                        this.grdBoard.Children.Add(redChecker);
                    }
                    if (row >= grdBoard.RowDefinitions.Count - 3)
                    {
                        BlackChecker blackChecker = new BlackChecker();
                        blackChecker.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(grdBoard_PreviewMouseLeftButtonDown);
                        blackChecker.PreviewMouseMove += new MouseEventHandler(grdBoard_PreviewMouseMove);
                        blackChecker.Cursor = Cursors.Hand;
                        blackChecker.AllowDrop = false;
                        blackChecker.col = col;
                        blackChecker.row = row;

                        Grid.SetColumn(blackChecker, col);
                        Grid.SetRow(blackChecker, row);
                        this.grdBoard.Children.Add(blackChecker);
                    }
                }
            }
        }

        void grdBoard_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _endPoint = e.GetPosition(null);
        }
        void grdBoard_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void StartDrag(MouseEventArgs e)
        {
            IsDragging = true;
            CheckerPiece src;
            if (e.Source.GetType() == typeof(RedChecker))
                src = (RedChecker)e.Source;
            else
                src = (BlackChecker)e.Source;

            currentPiece = src;
            DataObject data = new DataObject(System.Windows.DataFormats.Text.ToString(), "abcd");
            DragDropEffects de = DragDrop.DoDragDrop(this.grdBoard, data, DragDropEffects.All);                       

            IsDragging = false;
        }

        void grdBoard_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !IsDragging)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    StartDrag(e);
                }
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            this.grdBoard.Children.Clear();
            this.ResetGame();
        }
    }
}