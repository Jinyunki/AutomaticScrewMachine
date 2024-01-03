
using GalaSoft.MvvmLight.Messaging;
using System.ComponentModel;

namespace AutomaticScrewMachine.Bases {
    public static class StaticControllerSignal
    {
        /*private static readonly int ThreadSignalTimer = 10;
        private static DispatcherTimer _dispatcherTimer ;*/
        private static BackgroundWorker _joystickWorker ;

        public static bool ControlRock = false;

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

        // KeyDownEvent
        public static readonly string IO_SERVO_X = "F1";
        public static readonly string IO_SERVO_Y = "F2";
        public static readonly string IO_SERVO_Z = "F3";
        #endregion

        public static void ControllerSignalView(string clickBorderName)
        {
            IsPress = true;
            IsViewName = clickBorderName;
            /*_dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(ThreadSignalTimer)
            };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();*/


            _joystickWorker = new BackgroundWorker {
                WorkerSupportsCancellation = true
            };
            _joystickWorker.DoWork += JoyStick_DoWork;
            //_joystickWorker.RunWorkerCompleted += JoyStick_RunWorkerCompleted;
            _joystickWorker.RunWorkerAsync();

        }

        private static void JoyStick_DoWork (object sender, DoWorkEventArgs e) {
            while (!_joystickWorker.CancellationPending) {
                Messenger.Default.Send(new SignalMessage(IsViewName, IsPress));
            }
        }

        public static void StopControllerSignalView()
        {
            IsPress = false;
            Messenger.Default.Send(new SignalMessage(IsViewName, IsPress));
            if (_joystickWorker != null) {
                _joystickWorker.CancelAsync();
            }
            //_dispatcherTimer.Stop();
        }

        /*public static void DispatcherTimer_Tick(object sender, EventArgs e)
        {

            Messenger.Default.Send(new SignalMessage(IsViewName, IsPress));
        }*/
    }
}
