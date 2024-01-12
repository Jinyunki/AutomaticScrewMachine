using AutomaticScrewMachine.Bases;
using AutomaticScrewMachine.CurrentList._1.Jog.Model;
using AutomaticScrewMachine.CurrentList._3.IO.Model;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
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
            STATUS_Instance.StartStatusRead();
            ReadStatusIO();
            WriteOrder();
        }

        private void WriteOrder () {
            NGBoxCommand = new RelayCommand(()=> STATUS_Instance.DOWrite((int)DO_Index.NGBOX, STATUS_Instance.OUTPORT_NGBOX == 0 ? 1u:0));
        }

        ~IOMapViewModel () {
            _DIOWorker?.CancelAsync();
            STATUS_Instance = StatusReciver.ClearInstance();
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
                SelfStartButton = STATUS_Instance.INPORT_START_LEFT_BUTTON == 0 ? Brushes.Gray : Brushes.DarkBlue;
                SelfResetButton = STATUS_Instance.INPORT_RESET_BUTTON == 0 ? Brushes.Gray : Brushes.DarkBlue;
                SelfEmgButton = STATUS_Instance.INPORT_EMG_BUTTON == 0 ? Brushes.Gray : Brushes.DarkBlue;
                SelfStart2Button = STATUS_Instance.INPORT_START_RIGHT_BUTTON == 0 ? Brushes.Gray : Brushes.DarkBlue;
                NGBOX = STATUS_Instance.INPORT_NGBOX_OFF == 1 ? Brushes.Gray : Brushes.DarkBlue;

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

