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
            //Cursor = C1;
        }
        

        private void MotionController_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(MotionControllerAground);
                Console.WriteLine($"마우스 위치: X = {position.X}, Y = {position.Y}");

                // 마우스 위치에 따라 작은 원의 위치를 설정합니다.
                ControllerMotion.Visibility = Visibility.Visible;
                Canvas.SetLeft(ControllerMotion, position.X - ControllerMotion.Width / 2);
                Canvas.SetTop(ControllerMotion, position.Y - ControllerMotion.Height / 2);
                Cursor = Cursors.None;
            }
            else
            {
                // 마우스 프레스가 풀릴 때 작은 원을 중앙으로 이동합니다.
                double centerX = MotionControllerAground.ActualWidth / 2 - ControllerMotion.Width / 2;
                double centerY = MotionControllerAground.ActualHeight / 2 - ControllerMotion.Height / 2;
                Canvas.SetLeft(ControllerMotion, centerX);
                Canvas.SetTop(ControllerMotion, centerY);
                Cursor = C1;
            }
        }

        private void ScrewDriverController_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(ScrewDriverControllerAground);
                //Console.WriteLine($"마우스 위치: Y = {position.Y}");
                if(position.Y < -5)
                {
                    Console.WriteLine("상승 합니다");
                }
                else if (position.Y > 5)
                {
                    Console.WriteLine("하강 합니다");
                }

                ControllerScrewDriver.Visibility = Visibility.Visible;
                Canvas.SetTop(ControllerScrewDriver, position.Y - ControllerScrewDriver.Height / 50);
                Cursor = Cursors.None;
            }
            else
            {
                double centerY = ScrewDriverControllerAground.ActualHeight / 2 - ControllerScrewDriver.Height / 2;
                Canvas.SetTop(ControllerScrewDriver, centerY);
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
