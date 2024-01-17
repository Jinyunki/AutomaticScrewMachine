using System;
using System.Windows;
using System.Windows.Threading;

namespace AutomaticScrewMachine.Bases {
    public class StatusReciver {
        private static StatusReciver _instance;
        private static readonly object LockObject = new object();

        private StatusReciver () { }
        public static StatusReciver Instance {
            get {
                lock (LockObject) {
                    if (_instance == null) {
                        _instance = new StatusReciver();
                    }
                    return _instance;
                }
            }
        }
        // 추가: 인스턴스 클리어 메서드
        public static StatusReciver ClearInstance () {
            lock (LockObject) {
                _instance = null;
            }
            return _instance;
        }
        
        public DateTime Delay (int MS) {

            DateTime thisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime afterMoment = thisMoment.Add(duration);

            while (afterMoment >= thisMoment) {
                Application.Current?.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                thisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }
        

        public void DOWrite (int axis, uint value) {
            if (axis == 7) {
                CAXD.AxdoWriteOutport(axis, value == 1u ? 0 : 1u);
            } else {
                CAXD.AxdoWriteOutport(axis, value == 0 ? 1u : 0);
            }
        }
        public double ServoPositionValue (int indexNum) {
            double value = 99;
            CAXM.AxmStatusGetCmdPos(indexNum, ref value);
            return value;
        }
        public uint ServoMovingStatus (int indexNum) {
            uint value = 99;
            CAXM.AxmStatusReadInMotion(indexNum, ref value);
            return value;
        }
        public uint ServoSignalStatus (int indexNum) {
            uint value = 99;
            CAXM.AxmSignalIsServoOn(indexNum, ref value);
            return value;
        }
        public uint ServoAlarmStatus (int indexNum) {
            uint value = 99;
            CAXM.AxmStatusReadServoAlarm(indexNum, 0, ref value);
            return value;
        }

        public uint OutportStatus (int indexNum) {
            uint value = 99;
            CAXD.AxdoReadOutport(indexNum, ref value);
            return value;
        }
        public uint InportStatus (int indexNum) {
            uint value = 99;
            CAXD.AxdiReadInport(indexNum, ref value);
            return value;
        }
    }
}
