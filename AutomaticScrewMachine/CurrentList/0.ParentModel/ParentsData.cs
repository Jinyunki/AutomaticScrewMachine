using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;

namespace AutomaticScrewMachine.CurrentList._0.ParentModel {
    public class ParentsData : ViewModelBase {
        public enum DO_Index {
            STARTBTN_L = 0,
            RESETBTN = 1,
            EMGBTN = 2,
            STARTBTN_R = 3,

            TORQUE_DRIVER = 4, // Torqu 시작 P25 (4)
            PRESET1 = 5, // Preset선택 P25 (1)
            PRESET2 = 6, // Preset선택 P25 (2)

            NGBOX = 7,

            DRIVER_SYLINDER = 8,
            DEPTH_SYLINDER = 9,
            VACUUM = 10,

            OK_LED_PORT1 = 11,
            OK_LED_PORT2 = 12,
            OK_LED_PORT3 = 13,
            OK_LED_PORT4 = 14,
            OK_LED_PORT5 = 15,

            NG_LED_PORT1 = 16,
            NG_LED_PORT2 = 17,
            NG_LED_PORT3 = 18,
            NG_LED_PORT4 = 19,
            NG_LED_PORT5 = 20,

            LED_BUZZER_NG = 21,
            LED_BUZZER_ERR = 22,
            LED_BUZZER_OK = 23,
            SOUND_BUZZER = 24,

            /*IDK25 = 25, // NULL 비어있는 Index
            IDK26 = 26, // NULL 비어있는 Index
            IDK27 = 27, // NULL 비어있는 Index
            IDK28 = 28, // NULL 비어있는 Index
            IDK29 = 29, // NULL 비어있는 Index
            IDK30 = 30, // NULL 비어있는 Index
            IDK31 = 31  // NULL 비어있는 Index*/
        }
        public enum DI_Index {
            STARTBTN_LEFT = 0,
            RESETBTN = 1,
            EMGBTN = 2,
            STARTBTN_RIGHT = 3,

            // 4 NG BOX 오픈 중에 깜빡

            NGBOX_OFF = 5,
            SCREW_DRIVER_UP = 6,
            SCREW_DRIVER_DOWN = 7,

            INPORT_SCREW_DRIVER_VACUUM_SENSOR = 10,

            JIG_SENSOR_PORT1 = 13,
            JIG_SENSOR_PORT2 = 14,
            JIG_SENSOR_PORT3 = 15,
            JIG_SENSOR_PORT4 = 16,
            JIG_SENSOR_PORT5 = 17,

            SUPPLY_SCREW_SENSOR = 18,
            EMERGENCY_SENSOR = 19,

            TORQU_DRIVER_OK = 20,
            TORQU_DRIVER_START = 21,
            TORQU_DRIVER_READY = 22,
            TORQU_DRIVER_NG = 23,

            NGBOX_IN_SENSOR = 24,

            /*IDK25 = 25, // NULL 비어있는 Index
            IDK26 = 26, // NULL 비어있는 Index
            IDK27 = 27, // NULL 비어있는 Index
            IDK28 = 28, // NULL 비어있는 Index
            IDK29 = 29, // NULL 비어있는 Index
            IDK30 = 30, // NULL 비어있는 Index
            IDK31 = 31  // NULL 비어있는 Index*/
        }
        public enum Servo_Index {
            SERVO_Y = 0,
            SERVO_X = 1,
            SERVO_Z = 2
        }


        public static readonly string isFolderName = "Data";
        public static readonly string isFileName = "JogData.xlsx";
        public const uint SIGNAL_ON = 1u;
        public const uint SIGNAL_OFF = 0;

    }
}
