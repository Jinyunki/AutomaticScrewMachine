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
        

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(canvas);
                Console.WriteLine($"마우스 위치: X = {position.X}, Y = {position.Y}");

                // 마우스 위치에 따라 작은 원의 위치를 설정합니다.
                mouseTracker.Visibility = Visibility.Visible;
                Canvas.SetLeft(mouseTracker, position.X - mouseTracker.Width / 2);
                Canvas.SetTop(mouseTracker, position.Y - mouseTracker.Height / 2);
                Cursor = Cursors.None;
            }
            else
            {
                // 마우스 프레스가 풀릴 때 작은 원을 중앙으로 이동합니다.
                double centerX = canvas.ActualWidth / 2 - mouseTracker.Width / 2;
                double centerY = canvas.ActualHeight / 2 - mouseTracker.Height / 2;
                Canvas.SetLeft(mouseTracker, centerX);
                Canvas.SetTop(mouseTracker, centerY);
                Cursor = C1;
            }
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
