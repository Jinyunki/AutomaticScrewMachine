﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static OfficeOpenXml.ExcelErrorValue;

namespace AutomaticScrewMachine.Bases {
    public static class StatusReciver {

        #region OUTPORT Status Literal
        public static uint OUTPORT_START_LEFT_BUTTON; // 0
        public static uint OUTPORT_RESET_BUTTON; // 1
        public static uint OUTPORT_EMG_BUTTON; // 2
        public static uint OUTPORT_START_RIGHT_BUTTON; // 3
                                                       //456 무엇인지 모르는 상황
        public static uint OUTPORT_NGBOX; // 7

        public static uint OUTPORT_SCREW_DRIVER; // 8
        public static uint OUTPORT_DEPTH_CHECKER; // 9
        public static uint OUTPORT_SCREW_VACUUM; // 10

        public static uint OUTPORT_LED_OK1; // 11
        public static uint OUTPORT_LED_OK2; // 12
        public static uint OUTPORT_LED_OK3; // 13
        public static uint OUTPORT_LED_OK4; // 14
        public static uint OUTPORT_LED_OK5; // 15

        public static uint OUTPORT_LED_NG1; // 16
        public static uint OUTPORT_LED_NG2; // 17
        public static uint OUTPORT_LED_NG3; // 18
        public static uint OUTPORT_LED_NG4; // 19
        public static uint OUTPORT_LED_NG5; // 20

        public static uint OUTPORT_BUZZER_NG; // 21
        public static uint OUTPORT_BUZZER_ERROR; // 22
        public static uint OUTPORT_BUZZER_OK; // 23
        public static uint OUTPORT_BUZZER_SOUND; // 24
                                                 // 25~31 분석 필요
        #endregion

        #region INPORT Status Literal
        public static uint INPORT_START_LEFT_BUTTON; // 0
        public static uint INPORT_RESET_BUTTON; // 1
        public static uint INPORT_EMG_BUTTON; // 2
        public static uint INPORT_START_RIGHT_BUTTON; // 3

        // 4 => NGBOX OPNE 시 깜빡임
        public static uint INPORT_NGBOX_OFF; // 5
        public static uint INPORT_SCREW_DRIVER_UP ; // 6
        public static uint INPORT_SCREW_DRIVER_DOWN ; // 7

        // 8, 9 ??
        public static uint INPORT_SCREW_DRIVER_VACUUM_SENSOR ; // 10

        // 11, 12 ??
        public static uint INPORT_JIG_PORT1 ; // 13
        public static uint INPORT_JIG_PORT2 ; // 14
        public static uint INPORT_JIG_PORT3 ; // 15
        public static uint INPORT_JIG_PORT4 ; // 16
        public static uint INPORT_JIG_PORT5 ; // 17

        // 20, 21, 22, 23 ??
        public static uint INPORT_SUPPLY_SCREW_SENSOR; // 18
        public static uint INPORT_EMERGENCY_SENSOR; // 19
        public static uint INPORT_NGBOX_IN_SENSOR; // 24
                                                   // 25, 26, 27, 28, 29 ,30, 31 ??

        #endregion
        
        #region Servo Status Literal
        public static uint SERVO_ONOFF_SIGNAL_Y; // 0
        public static uint SERVO_ONOFF_SIGNAL_X; // 1
        public static uint SERVO_ONOFF_SIGNAL_Z; // 2

        public static uint SERVO_MOVE_STATUS_Y; // 0
        public static uint SERVO_MOVE_STATUS_X; // 1
        public static uint SERVO_MOVE_STATUS_Z; // 2

        public static double SERVO_POSITION_VALUE_Y; // 0
        public static double SERVO_POSITION_VALUE_X; // 1
        public static double SERVO_POSITION_VALUE_Z; // 2

        public static double SERVO_ALARM_VALUE_Y; // 0
        public static double SERVO_ALARM_VALUE_X; // 1
        public static double SERVO_ALARM_VALUE_Z; // 2
        #endregion

        #region Thread Worker List
        private static BackgroundWorker _digitalStatusWorker;
        private static BackgroundWorker _servoStatusWorker;
        #endregion
        public static void StartStatusRead () {
            //IO Status worker
            _digitalStatusWorker = new BackgroundWorker {
                WorkerSupportsCancellation = true
            };
            _digitalStatusWorker.DoWork += IOReadWorker_DoWork;
            _digitalStatusWorker.RunWorkerCompleted += IOReadWorker_RunWorkerCompleted;
            _digitalStatusWorker.RunWorkerAsync();

            //Servo Status worker
            _servoStatusWorker = new BackgroundWorker {
                WorkerSupportsCancellation = true
            };
            _servoStatusWorker.DoWork += ServoStatusReadWorker_DoWork;
            _servoStatusWorker.RunWorkerCompleted += ServoStatusReadWorke_RunWorkerCompleted;
            _servoStatusWorker.RunWorkerAsync();
        }

        private static void ServoStatusReadWorke_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e) {
            throw new NotImplementedException();
        }

        private static void ServoStatusReadWorker_DoWork (object sender, DoWorkEventArgs e) {
            while (!_servoStatusWorker.CancellationPending) {

                SERVO_ONOFF_SIGNAL_Y = ServoSignalStatus(0);
                SERVO_ONOFF_SIGNAL_X = ServoSignalStatus(1);
                SERVO_ONOFF_SIGNAL_Z = ServoSignalStatus(2);

                SERVO_MOVE_STATUS_Y = ServoMovingStatus(0);
                SERVO_MOVE_STATUS_X = ServoMovingStatus(1);
                SERVO_MOVE_STATUS_Z = ServoMovingStatus(2);

                SERVO_POSITION_VALUE_Y = ServoPositionValue(0);
                SERVO_POSITION_VALUE_X = ServoPositionValue(1);
                SERVO_POSITION_VALUE_Z = ServoPositionValue(2);

                SERVO_ALARM_VALUE_Y = ServoAlarmStatus(0);
                SERVO_ALARM_VALUE_X = ServoAlarmStatus(1);
                SERVO_ALARM_VALUE_Z = ServoAlarmStatus(2);
            }
        }

        public static DateTime Delay (int MS) {

            DateTime thisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime afterMoment = thisMoment.Add(duration);

            while (afterMoment >= thisMoment) {
                Application.Current?.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                thisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }
        private static void IOReadWorker_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e) {
            throw new NotImplementedException();
        }

        private static void IOReadWorker_DoWork (object sender, DoWorkEventArgs e) {
            while (!_digitalStatusWorker.CancellationPending) {
                Delay(100);
                OUTPORT_START_LEFT_BUTTON = OutportStatus(0);
                OUTPORT_RESET_BUTTON = OutportStatus(1);
                OUTPORT_EMG_BUTTON = OutportStatus(2);
                OUTPORT_START_RIGHT_BUTTON = OutportStatus(3);

                OUTPORT_NGBOX = OutportStatus(7);

                OUTPORT_SCREW_DRIVER = OutportStatus(8);
                OUTPORT_DEPTH_CHECKER = OutportStatus(9);
                OUTPORT_SCREW_VACUUM = OutportStatus(10);

                OUTPORT_LED_OK1 = OutportStatus(11);
                OUTPORT_LED_OK2 = OutportStatus(12);
                OUTPORT_LED_OK3 = OutportStatus(13);
                OUTPORT_LED_OK4 = OutportStatus(14);
                OUTPORT_LED_OK5 = OutportStatus(15);

                OUTPORT_LED_NG1 = OutportStatus(16);
                OUTPORT_LED_NG2 = OutportStatus(17);
                OUTPORT_LED_NG3 = OutportStatus(18);
                OUTPORT_LED_NG4 = OutportStatus(19);
                OUTPORT_LED_NG5 = OutportStatus(20);

                OUTPORT_BUZZER_NG = OutportStatus(21);
                OUTPORT_BUZZER_ERROR = OutportStatus(22);
                OUTPORT_BUZZER_OK = OutportStatus(23);
                OUTPORT_BUZZER_SOUND = OutportStatus(24);
                
                INPORT_START_LEFT_BUTTON = InportStatus(0);
                INPORT_RESET_BUTTON = InportStatus(1);
                INPORT_EMG_BUTTON = InportStatus(2);
                INPORT_START_RIGHT_BUTTON  = InportStatus(3);

                INPORT_NGBOX_OFF = InportStatus(5);
                INPORT_SCREW_DRIVER_UP = InportStatus(6);
                INPORT_SCREW_DRIVER_DOWN = InportStatus(7);

                INPORT_SCREW_DRIVER_VACUUM_SENSOR = InportStatus(10);

                INPORT_JIG_PORT1 = InportStatus(13);
                INPORT_JIG_PORT2 = InportStatus(14);
                INPORT_JIG_PORT3 = InportStatus(15);
                INPORT_JIG_PORT4 = InportStatus(16);
                INPORT_JIG_PORT5 = InportStatus(17);

                INPORT_SUPPLY_SCREW_SENSOR = InportStatus(18);
                INPORT_EMERGENCY_SENSOR = InportStatus(19);
                INPORT_NGBOX_IN_SENSOR = InportStatus(24);
            }
        }


        private static double ServoPositionValue (int indexNum) {
            double value = 99;
            CAXM.AxmStatusGetCmdPos(indexNum, ref value);
            return value;
        }
        private static uint ServoMovingStatus (int indexNum) {
            uint value = 99;
            CAXM.AxmStatusReadInMotion(indexNum, ref value);
            return value;
        }
        private static uint ServoSignalStatus (int indexNum) {
            uint value = 99;
            CAXM.AxmSignalIsServoOn(indexNum, ref value);
            return value;
        }

        private static uint OutportStatus (int indexNum) {
            uint value = 99;
            CAXD.AxdoReadOutport(indexNum, ref value);
            return value;
        }
        private static uint InportStatus (int indexNum) {
            uint value = 99;
            CAXD.AxdiReadInport(indexNum, ref value);
            return value;
        }
        private static uint ServoAlarmStatus (int indexNum) {
            uint value = 99;
            CAXM.AxmStatusReadServoAlarm(indexNum, 0,ref value);
            return value;
        }

    }
}
