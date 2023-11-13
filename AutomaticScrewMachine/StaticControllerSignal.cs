using GalaSoft.MvvmLight.Messaging;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AutomaticScrewMachine
{
    public static class StaticControllerSignal
    {
        private static int ThreadSignalTimer = 100;
        private static DispatcherTimer _dispatcherTimer  ;

        public static bool IsPress = false;
        public static bool BuzzerSignal_X = false;
        public static string IsViewName = "";

        #region JOG Btn Const
        // STOP
        public static readonly string JOG_STOP = "JogStopBtn";

        // Y
        public static readonly string JOG_STRAIGHT = "JogStraightBtn";
        public static readonly string JOG_BACK = "JogBackBtn";
        
        // X
        public static readonly string JOG_RIGHT = "JogRightBtn";
        public static readonly string JOG_LEFT = "JogLeftBtn";

        // Z
        public static readonly string JOG_UP = "JogUpBtn";
        public static readonly string JOG_DOWN = "JogDownBtn";

        // Rollback
        public static readonly string JOG_RETURN = "JogReturnBtn";
        #endregion

        public static void ControllerSignalView(string clickBorderName, Border xBuzzer, Border yBuzzer, Border zBuzzer)
        {
            _dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(ThreadSignalTimer) // 0.5초
            };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            IsPress = true;
            _dispatcherTimer.Start();


            switch (clickBorderName)
            {
                case string n when n == JOG_STOP:
                    IsViewName = clickBorderName;
                    break;


                case string n when n == JOG_STRAIGHT:
                    IsViewName = clickBorderName;
                    yBuzzer.Background = Brushes.Green;
                    break;
                case string n when n == JOG_BACK:
                    IsViewName = clickBorderName;
                    yBuzzer.Background = Brushes.Green;
                    break;                    


                case string n when n == JOG_RIGHT:
                    IsViewName = clickBorderName;
                    xBuzzer.Background = Brushes.Green;
                    break;
                case string n when n == JOG_LEFT:
                    IsViewName = clickBorderName;
                    xBuzzer.Background = Brushes.Green;
                    break;
                    

                case string n when n == JOG_UP:
                    IsViewName = clickBorderName;
                    zBuzzer.Background = Brushes.Green;
                    break;
                case string n when n == JOG_DOWN:
                    IsViewName = clickBorderName;
                    zBuzzer.Background = Brushes.Green;
                    break;
                    

                case string n when n == JOG_RETURN:
                    IsViewName = clickBorderName;
                    break;

                default:
                    break;
            }
        }

        public static void StopControllerSignalView()
        {
            IsPress = false;
            _dispatcherTimer.Tick -= DispatcherTimer_Tick;
            _dispatcherTimer.Stop();
            Console.WriteLine(IsViewName + " => Timer Stopped!!!!");
        }

        public static void BuzzerOff(string clickNoderName, Border xBuzzer, Border yBuzzer, Border zBuzzer)
        {
            switch (clickNoderName)
            {
                case string n when n == JOG_LEFT:
                    xBuzzer.Background = Brushes.Gray;
                    break;
                case string n when n == JOG_RIGHT:
                    xBuzzer.Background = Brushes.Gray;
                    break;


                case string n when n == JOG_STRAIGHT:
                    yBuzzer.Background = Brushes.Gray;
                    break;
                case string n when n == JOG_BACK:
                    yBuzzer.Background = Brushes.Gray;
                    break;


                case string n when n == JOG_UP:
                    zBuzzer.Background = Brushes.Gray;
                    break;
                case string n when n == JOG_DOWN:
                    zBuzzer.Background = Brushes.Gray;
                    break;
            }
        }

        public static void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Messenger.Default.Send(new SignalMessage(IsViewName, IsPress));
            
        }
    }
}
