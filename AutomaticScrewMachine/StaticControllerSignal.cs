using GalaSoft.MvvmLight.Messaging;
using System;
using System.Windows.Threading;

namespace AutomaticScrewMachine
{
    public static class StaticControllerSignal
    {
        private static int ThreadSignalTimer = 10;
        private static DispatcherTimer _dispatcherTimer ;

        public static bool IsPress = false;
        public static string IsViewName = "";
        public static double IsPosValue = 0;

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

        public static void ControllerSignalView(string clickBorderName)
        {
            IsPress = true;
            IsViewName = clickBorderName;
            _dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(ThreadSignalTimer)
            };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        public static void StopControllerSignalView()
        {
            IsPress = false;
            Messenger.Default.Send(new SignalMessage(IsViewName, IsPress));
            _dispatcherTimer.Stop();
        }

        public static void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            Messenger.Default.Send(new SignalMessage(IsViewName, IsPress));
            
        }
    }
}
