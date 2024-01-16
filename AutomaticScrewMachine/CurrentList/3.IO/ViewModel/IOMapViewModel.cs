using AutomaticScrewMachine.Bases;
using AutomaticScrewMachine.CurrentList._1.Jog.Model;
using AutomaticScrewMachine.CurrentList._3.IO.Model;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static OfficeOpenXml.ExcelErrorValue;

namespace AutomaticScrewMachine.CurrentList._3.IO.ViewModel {
    public class IOMapViewModel : IOData {
        private BackgroundWorker _DIOWorker;
        private StatusReciver STATUS_Instance = StatusReciver.Instance;

        public IOMapViewModel () {
            STATUS_Instance.StartStatusRead();
            ReadStatusIO();
            AddStatusOutput();
            AddStatusInput();
        }
        ~IOMapViewModel () {
            _DIOWorker?.CancelAsync();
            STATUS_Instance = StatusReciver.ClearInstance();
        }
        private void AddStatusOutput () {
            // H/W Button
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.STARTBTN_L, STATUS_Instance.OUTPORT_START_LEFT_BUTTON)),
                ButtonText = $"({(int)DO_Index.STARTBTN_L}) Start L"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.RESETBTN, STATUS_Instance.OUTPORT_RESET_BUTTON)),
                ButtonText = $"({(int)DO_Index.RESETBTN}) Reset"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.EMGBTN, STATUS_Instance.OUTPORT_EMG_BUTTON)),
                ButtonText = $"({(int)DO_Index.EMGBTN}) Emg"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.STARTBTN_R, STATUS_Instance.OUTPORT_START_RIGHT_BUTTON)),
                ButtonText = $"({(int)DO_Index.STARTBTN_R}) Start R"
            });

            // TorqControl
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.TORQUE_DRIVER, STATUS_Instance.OUTPORT_START_TORQUE_DRIVER)),
                ButtonText = $"({(int)DO_Index.TORQUE_DRIVER})Torqu"
            });

            // Preset
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.PRESET1, STATUS_Instance.OUTPORT_PRESET_ONE)),
                ButtonText = $"({(int)DO_Index.PRESET1}) Pre1"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.PRESET2, STATUS_Instance.OUTPORT_PRESET_TWO)),
                ButtonText = $"({(int)DO_Index.PRESET2}) Pre2"
            });

            // NGBox
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NGBOX, STATUS_Instance.OUTPORT_NGBOX)),
                ButtonText = $"({(int)DO_Index.NGBOX}) NgBox"
            });

            // Servo Sylinder
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.DRIVER_SYLINDER, STATUS_Instance.OUTPORT_DRIVER_SYLINDER)),
                ButtonText = $"({(int)DO_Index.DRIVER_SYLINDER}) Driver"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.DEPTH_SYLINDER, STATUS_Instance.OUTPORT_DEPTH_SYLINDER)),
                ButtonText = $"({(int)DO_Index.DEPTH_SYLINDER}) Depth"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.VACUUM, STATUS_Instance.OUTPORT_SCREW_VACUUM)),
                ButtonText = $"({(int)DO_Index.VACUUM}) Vacuum"
            });

            // OK SIGNAL PORT
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.OK_LED_PORT1, STATUS_Instance.OUTPORT_LED_OK1)),
                ButtonText = $"({(int)DO_Index.OK_LED_PORT1}) Port 1 OK"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.OK_LED_PORT2, STATUS_Instance.OUTPORT_LED_OK2)),
                ButtonText = $"({(int)DO_Index.OK_LED_PORT2}) Port 2 OK"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.OK_LED_PORT3, STATUS_Instance.OUTPORT_LED_OK3)),
                ButtonText = $"({(int)DO_Index.OK_LED_PORT3}) Port 3 OK"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.OK_LED_PORT4, STATUS_Instance.OUTPORT_LED_OK4)),
                ButtonText = $"({(int)DO_Index.OK_LED_PORT4}) Port 4 OK"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.OK_LED_PORT5, STATUS_Instance.OUTPORT_LED_OK5)),
                ButtonText = $"({(int)DO_Index.OK_LED_PORT5}) Port 5 OK"
            });

            // NG SIGNAL PORT
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NG_LED_PORT1, STATUS_Instance.OUTPORT_LED_NG1)),
                ButtonText = $"({(int)DO_Index.NG_LED_PORT1}) Port 1 NG"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NG_LED_PORT2, STATUS_Instance.OUTPORT_LED_NG2)),
                ButtonText = $"({(int)DO_Index.NG_LED_PORT2}) Port 2 NG"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NG_LED_PORT3, STATUS_Instance.OUTPORT_LED_NG3)),
                ButtonText = $"({(int)DO_Index.NG_LED_PORT3}) Port 3 NG"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NG_LED_PORT4, STATUS_Instance.OUTPORT_LED_NG4)),
                ButtonText = $"({(int)DO_Index.NG_LED_PORT4}) Port 4 NG"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NG_LED_PORT5, STATUS_Instance.OUTPORT_LED_NG5)),
                ButtonText = $"({(int)DO_Index.NG_LED_PORT5}) Port 5 NG"
            });

            // BUZZER
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.LED_BUZZER_RED, STATUS_Instance.OUTPORT_BUZZER_NG)),
                ButtonText = $"({(int)DO_Index.LED_BUZZER_RED}) Buzzer NG"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.LED_BUZZER_YELLOW, STATUS_Instance.OUTPORT_BUZZER_ERROR)),
                ButtonText = $"({(int)DO_Index.LED_BUZZER_YELLOW}) Buzzer Err"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.LED_BUZZER_GREEN, STATUS_Instance.OUTPORT_BUZZER_OK)),
                ButtonText = $"({(int)DO_Index.LED_BUZZER_GREEN}) Buzzer OK"
            });
            OutputList.Add(new ControlSetOutput {
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.SOUND_BUZZER, STATUS_Instance.OUTPORT_BUZZER_SOUND)),
                ButtonText = $"({(int)DO_Index.SOUND_BUZZER}) Buzzer Sound"
            });

        }
        private void AddStatusInput () {
            for (int i = 0; i <= 24; i++) {
                InputList.Add(new ControlSetOutput { ButtonText = $"({i})"});
            }
        }
        private void DIOWrite (int axis, uint value) {
            if (axis == 7) {
                CAXD.AxdoWriteOutport(axis, value == 1 ? 0 : 1u);
            } else {
                CAXD.AxdoWriteOutport(axis, value == 0 ? 1u : 0);
            }
        }

        private Brush SetOutputColor (uint Status) {
            return Status == 0 ? Brushes.Gray : Brushes.DarkBlue;
        }
        private Brush SetInputColor (uint Status) {
            return Status == 0 ? Brushes.Gray : Brushes.DarkGreen;
        }

        private void ReadStatusIO () {
            _DIOWorker = new BackgroundWorker {
                WorkerSupportsCancellation = true
            };
            _DIOWorker.DoWork += DIO_DoWork;
            _DIOWorker.RunWorkerCompleted += DIO_RunWorkerCompleted;
            _DIOWorker.RunWorkerAsync();
        }

        private void DIO_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e) {
            throw new NotImplementedException();
        }

        private void DIO_DoWork (object sender, DoWorkEventArgs e) {
            while (!_DIOWorker.CancellationPending) {
                STATUS_Instance.Delay(10);
                Console.WriteLine("STATUS_Instance.OUTPORT_PRESET_ONE : " + STATUS_Instance.OUTPORT_PRESET_ONE);
                Console.WriteLine("STATUS_Instance.OUTPORT_PRESET_TWO : " + STATUS_Instance.OUTPORT_PRESET_TWO);
                SetOutputBackground();
                SetInputBackground();
            }
        }

        private void SetOutputBackground () {
            for (int i = 0; i <= 24; i++) {
                OutputList[i].ButtonBackground = SetOutputColor(STATUS_Instance.OutportStatus(i));
            } 
        }private void SetInputBackground () {
            for (int i = 0; i <= 24; i++) {
                InputList[i].ButtonBackground = SetInputColor(STATUS_Instance.InportStatus(i));
            } 
        }
    }
}

