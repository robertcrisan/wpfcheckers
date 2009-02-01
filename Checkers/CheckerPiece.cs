using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Checkers
{
    public delegate void Captured();

    public abstract class CheckerPiece : UserControl
    {

        public event Captured OnCaptured;

        
        public int col { get; set; }
        public int row { get; set; }
    }
}
