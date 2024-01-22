using AutomaticScrewMachine.Bases;
using AutomaticScrewMachine.CurrentList._3.IO.Model;
using GalaSoft.MvvmLight.Command;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using static OfficeOpenXml.ExcelErrorValue;

namespace AutomaticScrewMachine.CurrentList._3.IO.ViewModel {
    public class IOMapViewModel : IOData {
        private BackgroundWorker _DIOWorker;
        private StatusReciver STATUS_Instance = StatusReciver.Instance;
        

        public IOMapViewModel () {
            ReadStatusIO();
            AddStatusOutput();
            AddStatusInput();
        }
        ~IOMapViewModel () {
            _DIOWorker?.CancelAsync();
            _DIOWorker?.Dispose();
            STATUS_Instance = StatusReciver.ClearInstance();
        }
        
        private void AddStatusInput () {
            for (int i = 0; i <= 24; i++) {
                InputList.Add(new ControlSetOutput { ButtonText = $"({i})"});
                
            }
        }
        private void AddStatusOutput () {
            for (int i = 0; i <= 24; i++) {
                int currentIndex = i;
                OutputList.Add(new ControlSetOutput { 
                    ButtonText = $"({currentIndex})"
                });
            }

            AddButtonEvent();
        }
        private void AddButtonEvent () {
            #region Button Event
            // H/W Button
            OutputList[(int)DO_Index.STARTBTN_L].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.STARTBTN_L, STATUS_Instance.OutportStatus((int)DO_Index.STARTBTN_L)));
            OutputList[(int)DO_Index.RESETBTN].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.RESETBTN, STATUS_Instance.OutportStatus((int)DO_Index.RESETBTN)));
            OutputList[(int)DO_Index.EMGBTN].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.EMGBTN, STATUS_Instance.OutportStatus((int)DO_Index.EMGBTN)));
            OutputList[(int)DO_Index.STARTBTN_R].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.STARTBTN_R, STATUS_Instance.OutportStatus((int)DO_Index.STARTBTN_R)));
            
            // TorqControl
            OutputList[(int)DO_Index.TORQUE_DRIVER].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.TORQUE_DRIVER, STATUS_Instance.OutportStatus((int)DO_Index.TORQUE_DRIVER)));

            // Preset
            OutputList[(int)DO_Index.PRESET1].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.PRESET1, STATUS_Instance.OutportStatus((int)DO_Index.PRESET1)));
            OutputList[(int)DO_Index.PRESET2].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.PRESET2, STATUS_Instance.OutportStatus((int)DO_Index.PRESET2)));

            // NGBox
            OutputList[(int)DO_Index.NGBOX].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NGBOX, STATUS_Instance.OutportStatus((int)DO_Index.NGBOX))); //

            // Servo Sylinder
            OutputList[(int)DO_Index.DRIVER_SYLINDER].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.DRIVER_SYLINDER, STATUS_Instance.OutportStatus((int)DO_Index.DRIVER_SYLINDER)));
            OutputList[(int)DO_Index.DEPTH_SYLINDER].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.DEPTH_SYLINDER, STATUS_Instance.OutportStatus((int)DO_Index.DEPTH_SYLINDER)));
            OutputList[(int)DO_Index.VACUUM].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.VACUUM, STATUS_Instance.OutportStatus((int)DO_Index.VACUUM)));

            // OK SIGNAL PORT
            OutputList[(int)DO_Index.OK_LED_PORT1].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.OK_LED_PORT1, STATUS_Instance.OutportStatus((int)DO_Index.OK_LED_PORT1)));
            OutputList[(int)DO_Index.OK_LED_PORT2].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.OK_LED_PORT2, STATUS_Instance.OutportStatus((int)DO_Index.OK_LED_PORT2)));
            OutputList[(int)DO_Index.OK_LED_PORT3].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.OK_LED_PORT3, STATUS_Instance.OutportStatus((int)DO_Index.OK_LED_PORT3)));
            OutputList[(int)DO_Index.OK_LED_PORT4].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.OK_LED_PORT4, STATUS_Instance.OutportStatus((int)DO_Index.OK_LED_PORT4)));
            OutputList[(int)DO_Index.OK_LED_PORT5].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.OK_LED_PORT5, STATUS_Instance.OutportStatus((int)DO_Index.OK_LED_PORT5)));
            // NG SIGNAL PORT
            OutputList[(int)DO_Index.NG_LED_PORT1].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NG_LED_PORT1, STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT1)));
            OutputList[(int)DO_Index.NG_LED_PORT2].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NG_LED_PORT2, STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT2)));
            OutputList[(int)DO_Index.NG_LED_PORT3].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NG_LED_PORT3, STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT3)));
            OutputList[(int)DO_Index.NG_LED_PORT4].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NG_LED_PORT4, STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT4)));
            OutputList[(int)DO_Index.NG_LED_PORT5].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NG_LED_PORT5, STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT5)));

            // BUZZER
            OutputList[(int)DO_Index.LED_BUZZER_NG].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.LED_BUZZER_NG, STATUS_Instance.OutportStatus((int)DO_Index.LED_BUZZER_NG)));
            OutputList[(int)DO_Index.LED_BUZZER_ERR].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.LED_BUZZER_ERR, STATUS_Instance.OutportStatus((int)DO_Index.LED_BUZZER_ERR)));
            OutputList[(int)DO_Index.LED_BUZZER_OK].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.LED_BUZZER_OK, STATUS_Instance.OutportStatus((int)DO_Index.LED_BUZZER_OK)));
            OutputList[(int)DO_Index.SOUND_BUZZER].ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.SOUND_BUZZER, STATUS_Instance.OutportStatus((int)DO_Index.SOUND_BUZZER)));
            #endregion
        }


        private void DIOWrite (int axis, uint value) {
            if (axis == 7) {
                CAXD.AxdoWriteOutport(axis, value == SIGNAL_ON ? 0 : 1u);
            } else {
                CAXD.AxdoWriteOutport(axis, value == SIGNAL_OFF ? 1u : 0);
            }
        }

        private Brush SetOutputColor (uint Status) {
            return Status == SIGNAL_OFF ? Brushes.Gray : Brushes.DarkBlue;
        }
        private Brush SetInputColor (uint Status) {
            return Status == SIGNAL_OFF ? Brushes.Gray : Brushes.DarkGreen;
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

