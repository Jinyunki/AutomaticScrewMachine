using AutomaticScrewMachine.Bases;
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

        private void Jog_KeyDown (object sender, KeyEventArgs e) {
            if (e.IsRepeat)
                return;
            switch (e.Key) {
                case Key.Left:
                    StaticControllerSignal.ControllerSignalView("JogLeftBtn");
                    break;
                case Key.Right:
                    StaticControllerSignal.ControllerSignalView("JogRightBtn");
                    break;
                case Key.Up:
                    StaticControllerSignal.ControllerSignalView("JogStraightBtn");
                    break;
                case Key.Down:
                    StaticControllerSignal.ControllerSignalView("JogBackBtn");
                    break;
                default:
                    break;
            }


        }

        private void Jog_KeyUp (object sender, KeyEventArgs e) {
            StaticControllerSignal.StopControllerSignalView();
        }
    }
}
