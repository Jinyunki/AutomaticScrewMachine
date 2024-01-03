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

            if (e.IsRepeat) {
                return;
            }

            // Left Ctr + @
            if (Keyboard.Modifiers == ModifierKeys.Control) {
                // Ctrl 키를 누른 상태에서 다른 키를 처리
                switch (e.Key) {
                    case Key.Up:
                        StaticControllerSignal.ControllerSignalView("JogUpBtn");
                        break;
                    case Key.Down:
                        StaticControllerSignal.ControllerSignalView("JogDownBtn");
                        break;
                        
                    default :
                        break;
                }

                // 단독 key event
            } else {
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
                    case Key.F1:
                        StaticControllerSignal.ControllerSignalView("F1");
                        break;
                    case Key.F2:
                        StaticControllerSignal.ControllerSignalView("F2");
                        break;
                    case Key.F3:
                        StaticControllerSignal.ControllerSignalView("F3");
                        break;
                    default:
                        break;
                }
            }
        }

        private void Jog_KeyUp (object sender, KeyEventArgs e) {
            StaticControllerSignal.StopControllerSignalView();
        }
    }
}
