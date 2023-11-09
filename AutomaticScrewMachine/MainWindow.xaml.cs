using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;

namespace AutomaticScrewMachine
{
    public partial class MainWindow : Window
    {
        static Cursor C1 = new Cursor(Application.GetResourceStream(new Uri("MousePointer.cur", UriKind.Relative)).Stream);
        public MainWindow()
        {
            InitializeComponent();
            Cursor = C1;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                DragMove();
            }
        }


        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = C1;
        }
    }
}
