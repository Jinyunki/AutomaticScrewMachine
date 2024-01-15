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
using System.Windows.Input;
using System.Windows.Media;

namespace AutomaticScrewMachine.CurrentList._3.IO.ViewModel {
    public class IOMapViewModel : IOData {
        private BackgroundWorker _DIOWorker;
        private StatusReciver STATUS_Instance = StatusReciver.Instance;

        public ICommand NGBoxCommand { get; set; }
        public IOMapViewModel () {
            ControlSetsOutput = new ObservableCollection<ControlSetOutput>();
            STATUS_Instance.StartStatusRead();
            ReadStatusIO();
            AddContorl();
            WriteOrder();
        }
        private void AddCtr (int DOindex, uint statusValue, string textValue) {
            ControlSetsOutput.Add(new ControlSetOutput {
                ButtonBackground = Brushes.Gray,
                ButtonCommand = new RelayCommand(() => DIOWrite(DOindex, statusValue == 0 ? 1u : 0)),
                ButtonText = $"{DOindex} {textValue}"
            });
        }
        private void AddContorl () {
            //AddCtr((int)DO_Index.STARTBTN, STATUS_Instance.OUTPORT_START_LEFT_BUTTON, "START_L");
            //AddCtr((int)DO_Index.RESETBTN, STATUS_Instance.OUTPORT_RESET_BUTTON, "RESET");
            ControlSetsOutput.Add(new ControlSetOutput {
                ButtonBackground = Brushes.Gray,
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.STARTBTN, STATUS_Instance.OUTPORT_START_LEFT_BUTTON == 0 ? 1u : 0)),
                ButtonText = "(0) START_L"
            });

            ControlSetsOutput.Add(new ControlSetOutput {
                ButtonBackground = Brushes.Gray,
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.RESETBTN, STATUS_Instance.OUTPORT_RESET_BUTTON == 0 ? 1u : 0)),
                ButtonText = "(1) RESET"
            });

            ControlSetsOutput.Add(new ControlSetOutput {
                ButtonBackground = Brushes.Gray,
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.EMGBTN, STATUS_Instance.OUTPORT_EMG_BUTTON == 0 ? 1u : 0)),
                ButtonText = "(2) EMG"
            });

            ControlSetsOutput.Add(new ControlSetOutput {
                ButtonBackground = Brushes.Gray,
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.START2BTN, STATUS_Instance.OUTPORT_START_RIGHT_BUTTON == 0 ? 1u : 0)),
                ButtonText = "(3) START_R"
            });

            ControlSetsOutput.Add(new ControlSetOutput {
                ButtonBackground = Brushes.Gray,
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.TORQUE_DRIVER, STATUS_Instance.OUTPORT_START_TORQUE_DRIVER == 0 ? 1u : 0)),
                ButtonText = "(4) TORQU"
            });

            ControlSetsOutput.Add(new ControlSetOutput {
                ButtonBackground = Brushes.Gray,
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.PRESET1, STATUS_Instance.OUTPORT_PRESET_ONE == 0 ? 1u : 0)),
                ButtonText = "(5) PRESET1"
            });

            ControlSetsOutput.Add(new ControlSetOutput {
                ButtonBackground = Brushes.Gray,
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.PRESET2, STATUS_Instance.OUTPORT_PRESET_TWO == 0 ? 1u : 0)),
                ButtonText = "(6) PRESET2"
            });

            ControlSetsOutput.Add(new ControlSetOutput {
                ButtonBackground = Brushes.Gray,
                ButtonCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NGBOX, STATUS_Instance.OUTPORT_NGBOX == 1 ? 0 : 1u)),
                ButtonText = "(7) NGBOX"
            });

        }
        private void DIOWrite (int axis, uint value) {
            CAXD.AxdoWriteOutport(axis, value);
        }
        private void WriteOrder () {
            NGBoxCommand = new RelayCommand(()=> STATUS_Instance.DOWrite((int)DO_Index.NGBOX, STATUS_Instance.OUTPORT_NGBOX == 0 ? 1u:0));
        }

        ~IOMapViewModel () {
            _DIOWorker?.CancelAsync();
            STATUS_Instance = StatusReciver.ClearInstance();
        }
        private Brush SetColor (uint Status) {
            return Status == 0 ? Brushes.Gray : Brushes.DarkBlue;
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
                STATUS_Instance.Delay(100);
                // 수동버튼
                //SelfStartButton = SetColor(STATUS_Instance.INPORT_START_LEFT_BUTTON);
                ControlSetsOutput[0].ButtonBackground = SetColor(STATUS_Instance.OUTPORT_START_LEFT_BUTTON);
                ControlSetsOutput[1].ButtonBackground = SetColor(STATUS_Instance.OUTPORT_RESET_BUTTON);
                /*ControlSetsOutput[2].ButtonBackground = SetColor(STATUS_Instance.OUTPORT_EMG_BUTTON);
                ControlSetsOutput[3].ButtonBackground = SetColor(STATUS_Instance.OUTPORT_START_RIGHT_BUTTON);
                ControlSetsOutput[4].ButtonBackground = SetColor(STATUS_Instance.OUTPORT_START_TORQUE_DRIVER);

                ControlSetsOutput[7].ButtonBackground = SetColor(STATUS_Instance.OUTPORT_NGBOX);*/
                //Console.WriteLine("SetColor(STATUS_Instance.INPORT_START_LEFT_BUTTON) ????" + SetColor(STATUS_Instance.INPORT_START_LEFT_BUTTON));
                //SelfResetButton = STATUS_Instance.INPORT_RESET_BUTTON == 0 ? Brushes.Gray : Brushes.DarkBlue;
                //SelfEmgButton = STATUS_Instance.INPORT_EMG_BUTTON == 0 ? Brushes.Gray : Brushes.DarkBlue;
                //SelfStart2Button = STATUS_Instance.INPORT_START_RIGHT_BUTTON == 0 ? Brushes.Gray : Brushes.DarkBlue;
                //TorquButton = STATUS_Instance.INPORT_START_RIGHT_BUTTON == 0 ? Brushes.Gray : Brushes.DarkBlue;

                //NGBOX = STATUS_Instance.INPORT_NGBOX_OFF == 1 ? Brushes.Gray : Brushes.DarkBlue;

                /*// 시퀀스 시작 트리거
                if (SelfStartButton == Brushes.Green && SelfStart2Button == Brushes.Green) {
                    CommandSequenceStart();
                }

                //비상정지
                EmgLine = SetInportBind(19);

                //JIG 내부 센서
                JigP1 = SetInportBind(13);
                JigP2 = SetInportBind(14);
                JigP3 = SetInportBind(15);
                JigP4 = SetInportBind(16);
                JigP5 = SetInportBind(17);

                ScrewSupplyOnoff = STATUS_Instance.INPORT_SUPPLY_SCREW_SENSOR == 1 ? Brushes.Red : Brushes.Gray;
                ScrewSupplyINOUT = STATUS_Instance.INPORT_SUPPLY_SCREW_SENSOR == 1 ? Brushes.Green : Brushes.Gray;

                NGBOX = SetOutportBind((int)DIOIndex.NGBOX);

                DriverBuzzer = SetOutportBind((int)DIOIndex.DRIVER_SYLINDER);
                DepthBuzzer = SetOutportBind((int)DIOIndex.DEPTH_SYLINDER);
                VacuumBuzzer = SetOutportBind((int)DIOIndex.VACUUM);

                TorqBuzzer = SetOutportBind((int)DIOIndex.TORQUE_DRIVER);

                // 머신 상단 알람 부저
                BuzzerAlarmOK = SetOutportBind((int)DIOIndex.LED_BUZZER_GREEN);
                BuzzerAlarmERR = SetOutportBind((int)DIOIndex.LED_BUZZER_YELLOW);
                BuzzerAlarmNG = SetOutportBind((int)DIOIndex.LED_BUZZER_RED);

                P1_OK = SetOutportBind((int)DIOIndex.OK_LED_PORT1);
                P2_OK = SetOutportBind((int)DIOIndex.OK_LED_PORT2);
                P3_OK = SetOutportBind((int)DIOIndex.OK_LED_PORT3);
                P4_OK = SetOutportBind((int)DIOIndex.OK_LED_PORT4);
                P5_OK = SetOutportBind((int)DIOIndex.OK_LED_PORT5);

                P1_NG = SetOutportBind((int)DIOIndex.NG_LED_PORT1);
                P2_NG = SetOutportBind((int)DIOIndex.NG_LED_PORT2);
                P3_NG = SetOutportBind((int)DIOIndex.NG_LED_PORT3);
                P4_NG = SetOutportBind((int)DIOIndex.NG_LED_PORT4);
                P5_NG = SetOutportBind((int)DIOIndex.NG_LED_PORT5);*/
            }
        }
    }
}

