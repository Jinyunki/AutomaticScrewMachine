using AutomaticScrewMachine.Bases;
using AutomaticScrewMachine.CurrentList._1.Jog.Model;
using AutomaticScrewMachine.Utiles;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections.ObjectModel;
using static OfficeOpenXml.ExcelErrorValue;

namespace AutomaticScrewMachine.CurrentList._1.Jog.ViewModel {
    public class JogViewModel : JogData {
        private BackgroundWorker _DIOWorker;
        private BackgroundWorker _motionWorker;
        private BackgroundWorker _seqWorker;
        private BackgroundWorker _homeReturnWorker;
        private StatusReciver STATUS_Instance = StatusReciver.Instance;

        public JogViewModel () {
            DefaultSet(); // 기본

            ThreadWorker();
            // EMERGENCY
            EmergencyStopCommand = new RelayCommand(EmergencyStop);
            // HOME
            HomeCommand = new RelayCommand(CmdHomeReturn);
            // Button TriggerEvent
            SetButtonEvent();
        }
        ~ JogViewModel () {
            _DIOWorker?.CancelAsync();
            _motionWorker?.CancelAsync();
            _seqWorker?.CancelAsync();
            _homeReturnWorker?.CancelAsync();
            STATUS_Instance = StatusReciver.ClearInstance();
        }
        public void DefaultSet () {
            // 컨트롤락
            StaticControllerSignal.ControlRock = false;
            // 조그 이동 속도 기본 설정
            JogMoveSpeed = 1;

        }


        public void ThreadWorker () {
            if (_homeReturnWorker == null) {
                Messenger.Default.Register<SignalMessage>(this, HandleSignalMessage); // 이벤트 관장 핸들러 메신저 Jog 컨트롤
            }

            _motionWorker = new BackgroundWorker {
                WorkerSupportsCancellation = true
            };
            _motionWorker.DoWork += Motion_DoWork;
            _motionWorker.RunWorkerCompleted += Motion_RunWorkerCompleted;
            _motionWorker.RunWorkerAsync();


            _DIOWorker = new BackgroundWorker {
                WorkerSupportsCancellation = true
            };
            _DIOWorker.DoWork += DIO_DoWork;
            _DIOWorker.RunWorkerCompleted += DIO_RunWorkerCompleted;
            _DIOWorker.RunWorkerAsync();

        }

        private void DIO_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e) {
            Console.WriteLine("DIO Worker 종료");
        }
        private Brush SetInportBind (int indexNum) {
            Brush brush;
            switch (indexNum) {
                case 0:
                    brush = STATUS_Instance.InportStatus((int)DI_Index.STARTBTN_LEFT) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green;
                    OutportInput(indexNum, STATUS_Instance.InportStatus((int)DI_Index.STARTBTN_LEFT));
                    break;
                case 1: // 리셋버튼
                    brush = STATUS_Instance.InportStatus((int)DI_Index.RESETBTN) == SIGNAL_OFF ? Brushes.Gray : Brushes.Orange;
                    OutportInput(indexNum, STATUS_Instance.InportStatus((int)DI_Index.RESETBTN));
                    if (STATUS_Instance.InportStatus((int)DI_Index.RESETBTN) == SIGNAL_ON) {
                        CmdHomeReturn();
                    }
                    break;
                case 2:
                    brush = STATUS_Instance.InportStatus((int)DI_Index.EMGBTN) == SIGNAL_OFF ? Brushes.Gray : Brushes.Red;
                    OutportInput(indexNum, STATUS_Instance.InportStatus((int)DI_Index.EMGBTN));
                    break;
                case 3:
                    brush = STATUS_Instance.InportStatus((int)DI_Index.STARTBTN_RIGHT) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green;
                    OutportInput(indexNum, STATUS_Instance.InportStatus((int)DI_Index.STARTBTN_RIGHT));
                    break;


                case 13:
                    brush = STATUS_Instance.InportStatus((int)DI_Index.JIG_SENSOR_PORT1) == SIGNAL_OFF ? Brushes.Transparent : Brushes.Black;
                    break;
                case 14:
                    brush = STATUS_Instance.InportStatus((int)DI_Index.JIG_SENSOR_PORT2) == SIGNAL_OFF ? Brushes.Transparent : Brushes.Black;
                    break;
                case 15:
                    brush = STATUS_Instance.InportStatus((int)DI_Index.JIG_SENSOR_PORT3) == SIGNAL_OFF ? Brushes.Transparent : Brushes.Black;
                    break;
                case 16:
                    brush = STATUS_Instance.InportStatus((int)DI_Index.JIG_SENSOR_PORT4) == SIGNAL_OFF ? Brushes.Transparent : Brushes.Black;
                    break;
                case 17:
                    brush = STATUS_Instance.InportStatus((int)DI_Index.JIG_SENSOR_PORT5) == SIGNAL_OFF ? Brushes.Transparent : Brushes.Black;
                    break;

                case 19:
                    brush = STATUS_Instance.InportStatus((int)DI_Index.EMERGENCY_SENSOR) == SIGNAL_OFF ? Brushes.Red : Brushes.Transparent; // EMG 센서는 반대로 임
                    if (brush == Brushes.Red && PositionValueY < 330000) {
                        //Console.WriteLine("CCCCCCCCCCCCC");
                        EmergencyStop();
                    }
                    break;

                default:
                    brush = Brushes.Red;
                    break;
            }

            return brush;
        }

        private void OutportInput (int index, uint value) {
            CAXD.AxdoWriteOutport(index, value);
        }

        private void DIO_DoWork (object sender, DoWorkEventArgs e) {
            while (!_DIOWorker.CancellationPending) {
                Delay(100);

                // 수동버튼
                SelfStartButton = SetInportBind(0);
                SelfResetButton = SetInportBind(1);
                SelfEmgButton = SetInportBind(2);
                SelfStart2Button = SetInportBind(3);

                // 시퀀스 시작 트리거
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

                ScrewSupplyOnoff = STATUS_Instance.InportStatus((int)DI_Index.SUPPLY_SCREW_SENSOR) == SIGNAL_OFF ? Brushes.Gray : Brushes.Red ;
                ScrewSupplyINOUT = STATUS_Instance.InportStatus((int)DI_Index.SUPPLY_SCREW_SENSOR) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green ;

                NGBOX = SetOutportBind((int)DO_Index.NGBOX);
                
                DriverBuzzer = SetOutportBind((int)DO_Index.DRIVER_SYLINDER);
                DepthBuzzer = SetOutportBind((int)DO_Index.DEPTH_SYLINDER);
                VacuumBuzzer = SetOutportBind((int)DO_Index.VACUUM);

                TorqDriverCtr = SetOutportBind((int)DO_Index.TORQUE_DRIVER);
                ReversTorqDriverCtr = SetOutportBind((int)DO_Index.TORQUE_REVERSED);


                // 머신 상단 알람 부저
                BuzzerAlarmOK = SetOutportBind((int)DO_Index.LED_BUZZER_OK);
                BuzzerAlarmERR = SetOutportBind((int)DO_Index.LED_BUZZER_ERR);
                BuzzerAlarmNG = SetOutportBind((int)DO_Index.LED_BUZZER_NG);

                P1_OK = SetOutportBind((int)DO_Index.OK_LED_PORT1);
                P2_OK = SetOutportBind((int)DO_Index.OK_LED_PORT2);
                P3_OK = SetOutportBind((int)DO_Index.OK_LED_PORT3);
                P4_OK = SetOutportBind((int)DO_Index.OK_LED_PORT4);
                P5_OK = SetOutportBind((int)DO_Index.OK_LED_PORT5);

                P1_NG = SetOutportBind((int)DO_Index.NG_LED_PORT1);
                P2_NG = SetOutportBind((int)DO_Index.NG_LED_PORT2);
                P3_NG = SetOutportBind((int)DO_Index.NG_LED_PORT3);
                P4_NG = SetOutportBind((int)DO_Index.NG_LED_PORT4);
                P5_NG = SetOutportBind((int)DO_Index.NG_LED_PORT5);
            }
        }

        private void Motion_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e) {
            Console.WriteLine("Motion Worker 종료");
        }

        private void Motion_DoWork (object sender, DoWorkEventArgs e) {
            // BackgroundWorker가 캔슬신호를 받기전까지 무한루프.
            /*var box = sender as BackgroundWorker;
            while (!box.CancellationPending) {*/
            while (!_motionWorker.CancellationPending) {
                ServoStatusX = RecevieSignalColor(STATUS_Instance.ServoSignalStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_X));
                ServoStatusY = RecevieSignalColor(STATUS_Instance.ServoSignalStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Y));
                ServoStatusZ = RecevieSignalColor(STATUS_Instance.ServoSignalStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Z));

                ServoMoveCheckX = RecevieSignalColor(STATUS_Instance.ServoMovingStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_X));
                ServoMoveCheckY = RecevieSignalColor(STATUS_Instance.ServoMovingStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Y));
                ServoMoveCheckZ = RecevieSignalColor(STATUS_Instance.ServoMovingStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Z));

                PositionValueX = STATUS_Instance.ServoPositionValue((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_X);
                PositionValueY = STATUS_Instance.ServoPositionValue((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Y);
                PositionValueZ = STATUS_Instance.ServoPositionValue((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Z);

                DriverPosList = new Thickness(PositionValueX * 0.00155, PositionValueY * 0.00165, 0, 0);
                ScrewMCForcus = STATUS_Instance.ServoPositionValue((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Z);
            }
        }
        
        private Brush SetOutportBind (int indexIONumber) {
            Dictionary<int, Func<Brush>> brushFunctions = new Dictionary<int, Func<Brush>> {
                { (int)DO_Index.NGBOX, () => STATUS_Instance.InportStatus((int) DI_Index.NGBOX_OFF) == SIGNAL_OFF ? Brushes.Transparent : Brushes.Gray },
                { (int)DO_Index.DRIVER_SYLINDER, () => STATUS_Instance.OutportStatus((int) DO_Index.DRIVER_SYLINDER) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green },
                { (int)DO_Index.DEPTH_SYLINDER, () => STATUS_Instance.OutportStatus((int) DO_Index.DEPTH_SYLINDER) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green },
                { (int)DO_Index.VACUUM, () => STATUS_Instance.OutportStatus((int) DO_Index.VACUUM) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green },

                { (int)DO_Index.TORQUE_DRIVER, () => STATUS_Instance.OutportStatus((int)DO_Index.TORQUE_DRIVER) == SIGNAL_OFF ? Brushes.Gray : Brushes.Blue },
                { (int)DO_Index.TORQUE_REVERSED, () => STATUS_Instance.OutportStatus((int)DO_Index.TORQUE_REVERSED) == SIGNAL_OFF ? Brushes.Gray : Brushes.Red },

                { (int)DO_Index.OK_LED_PORT1, () => STATUS_Instance.OutportStatus((int)DO_Index.OK_LED_PORT1) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green },
                { (int)DO_Index.OK_LED_PORT2, () => STATUS_Instance.OutportStatus((int)DO_Index.OK_LED_PORT2) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green },
                { (int)DO_Index.OK_LED_PORT3, () => STATUS_Instance.OutportStatus((int)DO_Index.OK_LED_PORT3) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green },
                { (int)DO_Index.OK_LED_PORT4, () => STATUS_Instance.OutportStatus((int)DO_Index.OK_LED_PORT4) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green },
                { (int)DO_Index.OK_LED_PORT5, () => STATUS_Instance.OutportStatus((int)DO_Index.OK_LED_PORT5) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green },
                
                { (int)DO_Index.NG_LED_PORT1, () => STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT1) == SIGNAL_OFF ? Brushes.Gray : Brushes.Red },
                { (int)DO_Index.NG_LED_PORT2, () => STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT2) == SIGNAL_OFF ? Brushes.Gray : Brushes.Red },
                { (int)DO_Index.NG_LED_PORT3, () => STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT3) == SIGNAL_OFF ? Brushes.Gray : Brushes.Red },
                { (int)DO_Index.NG_LED_PORT4, () => STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT4) == SIGNAL_OFF ? Brushes.Gray : Brushes.Red },
                { (int)DO_Index.NG_LED_PORT5, () => STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT5) == SIGNAL_OFF ? Brushes.Gray : Brushes.Red },

                { (int)DO_Index.LED_BUZZER_NG, () => { CAXD.AxdoWriteOutport((int)DO_Index.SOUND_BUZZER, STATUS_Instance.OutportStatus((int) DO_Index.LED_BUZZER_NG)); return STATUS_Instance.OutportStatus((int) DO_Index.LED_BUZZER_NG) == SIGNAL_OFF ? Brushes.Gray : Brushes.Red; } },
                { (int)DO_Index.LED_BUZZER_ERR, () => STATUS_Instance.OutportStatus((int) DO_Index.LED_BUZZER_ERR) == SIGNAL_OFF ? Brushes.Gray : Brushes.Orange },
                { (int)DO_Index.LED_BUZZER_OK, () => STATUS_Instance.OutportStatus((int) DO_Index.LED_BUZZER_OK) == SIGNAL_OFF ? Brushes.Gray : Brushes.Green },

            };

            return brushFunctions.TryGetValue(indexIONumber, out var func) ? func() : Brushes.Black;
            
        }
        


        private void SetButtonEvent () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                if (!StaticControllerSignal.ControlRock) {

                    //SEQ BTN
                    AddPosition = new RelayCommand(AddPos); // Seq 추가
                    RemoveSequenceCommand = new RelayCommand(RemoveSelectedSequenceListItem); // SEQ삭제
                    RemovePositionCommand = new RelayCommand(RemoveSelectedPositionListItem); // POS삭제

                    // 자동화 시퀀스 시작
                    SequenceStart = new RelayCommand(CommandSequenceStart); // 시퀀스 시작

                    //NGBOX ONOFF
                    NGBoxCommand = new RelayCommand(() => DIOWrite((int)DO_Index.NGBOX, STATUS_Instance.InportStatus((int)DI_Index.NGBOX_OFF) == SIGNAL_OFF ? 1u : 0));

                    // Servo OnOff
                    ServoCheckX = new RelayCommand(() => ServoDIOWrite((int)Servo_Index.SERVO_X, STATUS_Instance.ServoSignalStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_X) == SIGNAL_OFF ? 1u : 0));
                    ServoCheckY = new RelayCommand(() => ServoDIOWrite((int)Servo_Index.SERVO_Y, STATUS_Instance.ServoSignalStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Y) == SIGNAL_OFF ? 1u : 0));
                    ServoCheckZ = new RelayCommand(() => ServoDIOWrite((int)Servo_Index.SERVO_Z, STATUS_Instance.ServoSignalStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Z) == SIGNAL_OFF ? 1u : 0));

                    // Torq OnOff
                    TorqDriverIO = new RelayCommand(() => DIOWrite((int)DO_Index.TORQUE_DRIVER, STATUS_Instance.OutportStatus((int)DO_Index.TORQUE_DRIVER) == SIGNAL_OFF ? 1u : 0));
                    TorqDriverReversIO = new RelayCommand(ReversTorque);

                    // Sylinder IO
                    DriverIO = new RelayCommand(() => DIOWrite((int)DO_Index.DRIVER_SYLINDER, STATUS_Instance.OutportStatus((int)DO_Index.DRIVER_SYLINDER) == SIGNAL_OFF ? 1u : 0));
                    DepthIO = new RelayCommand(() => DIOWrite((int)DO_Index.DEPTH_SYLINDER, STATUS_Instance.OutportStatus((int)DO_Index.DEPTH_SYLINDER) == SIGNAL_OFF ? 1u : 0));
                    VacuumIO = new RelayCommand(() => DIOWrite((int)DO_Index.VACUUM, STATUS_Instance.OutportStatus((int)DO_Index.VACUUM) == SIGNAL_OFF ? 1u : 0));

                    // ExcelIO
                    ReadRecipe = new RelayCommand(ReadRecipeCommand);
                    AddRecipe = new RelayCommand(AddRecipeCommand);
                    SavePosDataRecipe = new RelayCommand(SaveRecipeCommand);

                    // GetMovePosition
                    MovePosition1 = new RelayCommand(() => CommandMoveJIG(0));
                    MovePosition2 = new RelayCommand(() => CommandMoveJIG(GetPortInterval));
                    MovePosition3 = new RelayCommand(() => CommandMoveJIG(GetPortInterval * 2));
                    MovePosition4 = new RelayCommand(() => CommandMoveJIG(GetPortInterval * 3));
                    MovePosition5 = new RelayCommand(() => CommandMoveJIG(GetPortInterval * 4));
                    MovePositionSupply = new RelayCommand(() => MoveMultiPos_XY(SupplyPosition));

                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }


        }

        private void ReversTorque () {
            DIOWrite((int)DO_Index.TORQUE_REVERSED, STATUS_Instance.OutportStatus((int)DO_Index.TORQUE_REVERSED) == SIGNAL_OFF ? 1u : 0);
            Delay(10);
            //DIOWrite((int)DO_Index.TORQUE_DRIVER, STATUS_Instance.OutportStatus((int)DO_Index.TORQUE_DRIVER) == SIGNAL_OFF ? 1u : 0);
        }

        private void CommandMoveJIG (double intaval) {
            if (TabCnt == 1) {
                Interval = intaval;
                Port1Position = new double[2] { StartPortXPos + Interval, StartPortYPos };
                MoveMultiPos_XY(Port1Position);
                TabCnt++;
            } else {
                Interval = intaval;
                Port1Position = new double[2] { StartPortXPos + Interval + GetTabInterval, StartPortYPos };
                MoveMultiPos_XY(Port1Position);
                TabCnt--;
            }
            

        }

        private void AddRecipeCommand () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {

                List<List<string>> totlaList = new List<List<string>>();
                for (int i = 0; i < PositionDataList.Count; i++) {

                    List<string> itemList = new List<string> {
                    PositionDataList[i].Name,
                    PositionDataList[i].X.ToString(),
                    PositionDataList[i].Y.ToString(),
                    PositionDataList[i].Z.ToString(),
                    PositionDataList[i].Driver_IO.ToString(),
                    PositionDataList[i].Depth_IO.ToString(),
                };

                    totlaList.Add(itemList);
                }

                string newWorksheetName = "Recipe#" + SequenceDataList.Count;
                ExcelAdapter.Add(isFolderName, isFileName, newWorksheetName, totlaList);

                SequenceDataList.Add(new SequenceData {
                    Name = "Recipe#" + (SequenceDataList.Count + 1)
                });
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void SaveRecipeCommand () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                if (SelectedSequenceItem != null) {
                    if (SelectedPositionItem != null) {
                        ExcelAdapter.Save(isFolderName, isFileName, SequenceDataList.IndexOf(SelectedSequenceItem), PositionDataList.IndexOf(SelectedPositionItem), PositionDataList);
                    } else {
                        MessageBox.Show("선택 된 추가 할 Position List를 Select 해주세요");
                    }

                } else {
                    MessageBox.Show("선택 된 Sequnce List 가 없습니다");
                }

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void ReadRecipeCommand () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                SequenceDataList?.Clear();
                ExcelAdapter.Connect(isFolderName, isFileName, 0);
                for (int i = 0; i < ExcelAdapter.WorkSheetNameList.Count; i++) {
                    SequenceData seq = new SequenceData {
                        Name = ExcelAdapter.WorkSheetNameList[i].ToString(),
                        SequenceListStart = new RelayCommand(CommandSequenceStart)
                    };

                    SequenceDataList.Add(seq);
                }

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }
        }

        private void ServoDIOWrite (int axis, uint value) {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                CAXM.AxmSignalServoOn(axis, value);

                CAXM.AxmSignalServoAlarmReset(axis, 1);
                //SetServoAlarm(axis, value);
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }
        }

        private void HandleSignalMessage (SignalMessage message) {
            try {
                if (StaticControllerSignal.ControlRock == false) {
                    var isPress = message.IsPress;
                    var isViewName = message.IsViewName;
                    switch (isViewName) {
                        // y 전후방
                        case string n when n == StaticControllerSignal.JOG_STRAIGHT:
                            GetMove((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Y, -MC_JogSpeed);
                            break;
                        case string n when n == StaticControllerSignal.JOG_BACK:
                            GetMove((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Y, MC_JogSpeed);
                            break;


                        // x 좌우
                        case string n when n == StaticControllerSignal.JOG_LEFT:
                            GetMove((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_X, -MC_JogSpeed);
                            break;
                        case string n when n == StaticControllerSignal.JOG_RIGHT:
                            GetMove((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_X, MC_JogSpeed);
                            break;


                        // z 위아래
                        case string n when n == StaticControllerSignal.JOG_UP:
                            GetMove((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Z, -MC_JogSpeed);
                            break;

                        case string n when n == StaticControllerSignal.JOG_DOWN:
                            GetMove((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Z, MC_JogSpeed);
                            break;

                        case string n when n == StaticControllerSignal.IO_SERVO_X:
                            ServoDIOWrite((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_X, STATUS_Instance.ServoMovingStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_X));
                            break;
                            
                        case string n when n == StaticControllerSignal.IO_SERVO_Y:
                            ServoDIOWrite((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Y, STATUS_Instance.ServoMovingStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Y));
                            break;
                            
                        case string n when n == StaticControllerSignal.IO_SERVO_Z:
                            ServoDIOWrite((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Z, STATUS_Instance.ServoMovingStatus((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Z));
                            break;

                        default:
                            break;
                    }

                    if (!isPress) {
                        CAXM.AxmMoveSStop((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Y);
                        CAXM.AxmMoveSStop((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_X);
                        CAXM.AxmMoveSStop((int)_0.ParentModel.ParentsData.Servo_Index.SERVO_Z);
                    }
                } 
                


            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }
        private void GetMove (int index, double direction) {
            CAXM.AxmMoveVel(index, direction, MC_JogAcl, MC_JogDcl);

        }

        #region ListBoxConf

        private void UpdateSelectedPosData (int index) {

            PositionDataList[index].X = PositionValueX;
            PositionDataList[index].Y = PositionValueY;
            PositionDataList[index].Z = PositionValueZ;
            PositionDataList[index].Driver_IO = STATUS_Instance.OutportStatus((int)DO_Index.DRIVER_SYLINDER);
            PositionDataList[index].Depth_IO = STATUS_Instance.OutportStatus((int)DO_Index.DEPTH_SYLINDER);
        }
        public void GetReadingData (int workSheetIndex) {
            PositionDataList?.Clear();
            ObservableCollection<List<string>> GetJogDataList;

            GetJogDataList = ExcelAdapter.GetReadData(isFolderName, isFileName, workSheetIndex);
            if (GetJogDataList != null) {
                for (int j = 1; j < GetJogDataList.Count; j++) // j = 0 CategoryList 그래서 1부터 시작
                {
                    PosData posData = new PosData {
                        Name = GetJogDataList[j][0].ToString(),
                        X = double.Parse(GetJogDataList[j][1]),
                        Y = double.Parse(GetJogDataList[j][2]),
                        Z = double.Parse(GetJogDataList[j][3]),
                        Driver_IO = uint.Parse(GetJogDataList[j][4]),
                        Depth_IO = uint.Parse(GetJogDataList[j][5]),
                        ChangePositionDataBtn = new RelayCommand(()=> UpdateSelectedPosData(SelectedPositionIndex)),
                        //MoveCheckPositionXY = new RelayCommand(()=> MoveJIGPos(new double[2] { double.Parse(GetJogDataList[j][1]), double.Parse(GetJogDataList[j][2]) }))
                        
                        // 해당 부분이 누락된 업데이트
                        MoveCheckPositionXY = new RelayCommand(()=> GetSelectedListPositionMove(PositionDataList[SelectedPositionIndex].X, PositionDataList[SelectedPositionIndex].Y, PositionDataList[SelectedPositionIndex].Z))
                    };

                    PositionDataList.Add(posData);
                }
            }
        }

        private void GetSelectedListPositionMove (double x, double y, double z) {
            double[] doubles = new double[2] {x,y };
            MoveMultiPos_XY(doubles);
            MoveDownPos(doubles, z); // Thread가 물리나 ?
        }

        private SequenceData _selectedSequenceItem;
        public SequenceData SelectedSequenceItem {
            get { return _selectedSequenceItem; }
            set {
                if (_selectedSequenceItem != value) {
                    _selectedSequenceItem = value;
                    RaisePropertyChanged(nameof(_selectedSequenceItem));

                    // 선택된 항목에 대한 처리 수행
                    if (_selectedSequenceItem != null) {
                        SelectedSequenceIndex = SequenceDataList.IndexOf(_selectedSequenceItem);
                        GetReadingData(SelectedSequenceIndex);
                    } else {
                        PositionDataList.Clear();
                    }
                }
            }
        }

        private void AddPos () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                TitleName = (PositionDataList.Count + 1).ToString();

                PosData posData = new PosData() {
                    Name = TitleName,
                    X = PositionValueX,
                    Y = PositionValueY,
                    Z = PositionValueZ,
                    Driver_IO = STATUS_Instance.OutportStatus((int)DO_Index.DRIVER_SYLINDER),
                    Depth_IO = STATUS_Instance.OutportStatus((int)DO_Index.DEPTH_SYLINDER)
                };

                PositionDataList.Add(posData);
                TitleName = "";

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }

        private void RemoveSelectedSequenceListItem () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                if (SelectedSequenceItem != null) {
                    ExcelAdapter.RemoveWorkSheet(isFolderName, isFileName, RemoveAndGetIndex(SelectedSequenceItem));
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }
        }
        private void RemoveSelectedPositionListItem () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                if (SelectedPositionItem != null) {
                    ExcelAdapter.RemovePositionDataList(isFolderName, isFileName, SelectedSequenceIndex, SelectedPositionIndex);
                    GetReadingData(SelectedSequenceIndex);
                    //ReadRecipeCommand();
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }
        }
        /// <summary>
        /// 선택된 SELECTED INDEX를 삭제 한다.(Sequnce)
        /// </summary>
        /// <param name="itemToRemove"></param>
        /// <returns></returns>
        public int RemoveAndGetIndex (SequenceData itemToRemove) {
            int removedIndex = -1;
            // CollectionChanged 이벤트 핸들러 등록
            SequenceDataList.CollectionChanged += (sender, e) => {
                if (e.Action == NotifyCollectionChangedAction.Remove) {
                    // 제거된 항목의 인덱스를 가져옴
                    removedIndex = e.OldStartingIndex;
                }
            };

            // 항목 제거
            SequenceDataList.Remove(itemToRemove);

            // 이벤트 핸들러 제거
            SequenceDataList.CollectionChanged -= null;

            return removedIndex;
        }
        private void CommandSequenceStart () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {

                _seqWorker = new BackgroundWorker {
                    WorkerSupportsCancellation = true
                };
                _seqWorker.DoWork += SEQ_DoWork;
                _seqWorker.RunWorkerCompleted += SEQ_RunWorkerCompleted;
                _seqWorker.RunWorkerAsync();


            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }
        }

        private void SEQ_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e) {

            Delay(500); // 시퀀스 작업이 모두 종료후 0.5초동안 잠시 대기 후

            MoveMultiPos_XY(SupplyPosition); // 시퀀스 종료 후, 

            if (STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT1) == SIGNAL_ON || STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT2) == SIGNAL_ON || STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT3) == SIGNAL_ON || STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT4) == SIGNAL_ON || STATUS_Instance.OutportStatus((int)DO_Index.NG_LED_PORT5) == SIGNAL_ON) {
                DIOWrite((int)DO_Index.LED_BUZZER_NG, SIGNAL_ON);
            } else {
                DIOWrite((int)DO_Index.LED_BUZZER_OK, SIGNAL_ON);
            }
        }
        private void DIOWrite (int axis, uint value) {
            CAXD.AxdoWriteOutport(axis, value);
        }

        private void SEQ_DoWork (object sender, DoWorkEventArgs e) {
            for (int i = 1; i <= PositionDataList.Count; i++) {
                double[] SeqXYHoldPos = new double[2] { PositionDataList[i-1].X, PositionDataList[i-1].Y };

                MoveMultiPos_XY(SupplyPosition); //xy 공급기

                MoveDownPos(SupplyPosition, SupplyScrewDownPos); // z 스크류 공급

                //GetScrewCommand_WriteOutport(true); // io 스크류 체득
                DIOWrite((int)DO_Index.DRIVER_SYLINDER, SIGNAL_ON);
                DIOWrite((int)DO_Index.VACUUM, SIGNAL_ON);
                Delay(500);

                MoveUpPos(10000); // z 이동을위한 업

                MoveJIGPos(SeqXYHoldPos); // xy 지그 위치로 이동

                MoveDownPos(SeqXYHoldPos, TorqReadyZposition); // 지그 구멍에 맞춰 체결할곳으로 하강
                //Delay(700); // 해당 딜레이 => 조건이 충족하면 ? 하기 토크 작동되는 것으로 변경
                            // ex) 위치가 해당위치가 맞고, INPORT_SCREW_DRIVER_VACUUM_SENSOR의 Status가, ON이면, 
                TorqDriverWrite((int)DO_Index.TORQUE_DRIVER,i); // 토크 드라이버 작동 (메서드화 해야함. INPORT_TORQU_DRIVER_OK Status가 ON이 아니면 while Delay)
                 
                MoveUpPos(10000);

                //GetScrewCommand_WriteOutport(false); driver,vacuum Off

                DIOWrite((int)DO_Index.DRIVER_SYLINDER, SIGNAL_OFF);
                DIOWrite((int)DO_Index.VACUUM, SIGNAL_OFF);

                Console.WriteLine("원사이클");
            }

        }

        private void TorqDriverWrite (int axis,int positionListIndex) {
            double checkerValue = (double)positionListIndex / 2;
            while (STATUS_Instance.InportStatus((int)DI_Index.SCREW_DRIVER_UP) != SIGNAL_OFF) {
                Delay(10);

                Console.WriteLine("AA");
            }
            CAXD.AxdoWriteOutport(axis, SIGNAL_ON);
            Delay(500);
            while (STATUS_Instance.InportStatus((int)DI_Index.TORQU_DRIVER_START) == SIGNAL_ON) {
                Delay(10);
                Console.WriteLine("CC");
            }
            Delay(300);
            if (STATUS_Instance.InportStatus((int)DI_Index.TORQU_DRIVER_NG) == SIGNAL_ON) {
                if (checkerValue <= 1) {//12
                    DIOWrite((int)DO_Index.NG_LED_PORT1, SIGNAL_ON);
                } else if (1 < checkerValue && checkerValue <= 2) { //34
                    DIOWrite((int)DO_Index.NG_LED_PORT2, SIGNAL_ON);
                } else if (2 < checkerValue && checkerValue <= 3) { //56
                    DIOWrite((int)DO_Index.NG_LED_PORT3, SIGNAL_ON);
                } else if (3 < checkerValue && checkerValue <= 4) { //78
                    DIOWrite((int)DO_Index.NG_LED_PORT4, SIGNAL_ON);
                } else if (4 < checkerValue && checkerValue <= 5) { //910
                    DIOWrite((int)DO_Index.NG_LED_PORT5, SIGNAL_ON);
                }
            } else {
                DIOWrite((int)DO_Index.OK_LED_PORT1, SIGNAL_ON);
            }
            Delay(100);
            CAXD.AxdoWriteOutport(axis, SIGNAL_OFF);
            
        }
        private bool ArePositionsEqual (double[] position1, double[] position2) {
            return ((int)position1[0] == (int)position2[0] && (int)position1[1] == (int)position2[1]);
        }

        private void WaitUntilPositionsEqual (double[] targetPosition) {
            while (!ArePositionsEqual(targetPosition, new double[] { PositionValueX, PositionValueY })) {
                Delay(100);
            }
        }

        private void MoveJIGPos (double[] recipePosition) {
            if (!ArePositionsEqual(recipePosition, new double[] { PositionValueX, PositionValueY })) {
                MultiMovePos(recipePosition, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
            }

            WaitUntilPositionsEqual(recipePosition);

        }

        private void MoveDownPos (double[] xyStatus, double downPos) {
            while (!((int)xyStatus[0] == (int)PositionValueX && (int)xyStatus[1] == (int)PositionValueY)) { // XY 이동이 종료 될때까지
                Delay(100);
            }

            if ((int)xyStatus[0] == (int)PositionValueX && (int)xyStatus[1] == (int)PositionValueY) { // XY 가 다시한번 정상인것을 확인 후 하강
                CAXM.AxmMoveStartPos((int)Servo_Index.SERVO_Z, downPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
            }

            while ((int)PositionValueZ != (int)downPos) {
                Delay(100);
                Console.WriteLine("CHECKER2222222");
            }

        }
        private void MoveUpPos (double downPos) {
            CAXM.AxmMoveStartPos((int)Servo_Index.SERVO_Z, downPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
            while ((int)PositionValueZ != downPos) {
                Delay(100);
            }
        }

        private void MoveMultiPos_XY (double[] XYHoldPos) {
            double _moveAbleZPosition = 10000; //마지노선
            if (STATUS_Instance.OutportStatus((int)DO_Index.DRIVER_SYLINDER) != 0 && STATUS_Instance.OutportStatus((int)DO_Index.VACUUM) != 0) {
                Crash_IO_OnOff(false);
            }
            if (PositionValueZ > _moveAbleZPosition) {
                CAXM.AxmMovePos((int)Servo_Index.SERVO_Z, _moveAbleZPosition, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                MultiMovePos(XYHoldPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
            } else {
                MultiMovePos(XYHoldPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
            }
        }

        private void MultiMovePos (double[] positionList, double jogSpeed, double jogAcl, double jogDcl) {
            int[] jogList = { (int)Servo_Index.SERVO_X, (int)Servo_Index.SERVO_Y }; // X,Y
            double[] speedList = { jogSpeed, jogSpeed };
            double[] aclList = { jogAcl, jogAcl };
            double[] dclList = { jogDcl, jogDcl };
            CAXM.AxmMoveStartMultiPos(2, jogList, positionList, speedList, aclList, dclList); //AxmMoveMultiPos
        }

        #endregion

        private void EmergencyStop () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                _seqWorker?.CancelAsync();
                //PositionDataList?.Clear();
                CAXM.AxmMoveEStop((int)Servo_Index.SERVO_Y);
                CAXM.AxmMoveEStop((int)Servo_Index.SERVO_X);
                CAXM.AxmMoveEStop((int)Servo_Index.SERVO_Z);

                CAXM.AxmSignalServoOn((int)Servo_Index.SERVO_Z, SIGNAL_OFF);
                CAXM.AxmSignalServoOn((int)Servo_Index.SERVO_X, SIGNAL_OFF);
                CAXM.AxmSignalServoOn((int)Servo_Index.SERVO_Y, SIGNAL_OFF);

                Crash_IO_OnOff(false);

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }


        /// <summary>
        /// Z축에 영향을 주는 DI/O 를 모두 종료(UP-Sylinder) 시킵니다
        /// </summary>
        /// <param name="OnOff"> True = On, False = Off </param>
        private void Crash_IO_OnOff (bool OnOff) {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {

                uint returnOnof = OnOff ? 1u : 0u;
                CAXD.AxdoWriteOutport((int)DO_Index.DRIVER_SYLINDER, returnOnof);
                CAXD.AxdoWriteOutport((int)DO_Index.DEPTH_SYLINDER, returnOnof);
                CAXD.AxdoWriteOutport((int)DO_Index.VACUUM, returnOnof);

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }
        /// <summary>
        /// Home원점 복귀 시 속도를 지정합니다.
        /// </summary>
        /// <param name="moveSpeed"></param>
        private void SetHomeReturnSpeed (double moveSpeed) {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                CAXM.AxmHomeSetVel(2, moveSpeed, moveSpeed / 4, moveSpeed / 8, moveSpeed / 16, moveSpeed * 10, moveSpeed * 10);
                CAXM.AxmHomeSetVel(0, moveSpeed, moveSpeed / 4, moveSpeed / 8, moveSpeed / 16, moveSpeed * 10, moveSpeed * 10);
                CAXM.AxmHomeSetVel(1, moveSpeed, moveSpeed / 4, moveSpeed / 8, moveSpeed / 16, moveSpeed * 10, moveSpeed * 10);
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }
        private void CmdHomeReturn () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                StaticControllerSignal.ControlRock = true;
                
                CAXM.AxmSignalServoOn((int)Servo_Index.SERVO_Z, SIGNAL_ON);
                CAXM.AxmSignalServoOn((int)Servo_Index.SERVO_X, SIGNAL_ON);
                CAXM.AxmSignalServoOn((int)Servo_Index.SERVO_Y, SIGNAL_ON);
                
                Crash_IO_OnOff(false);
                SetHomeReturnSpeed(100000);

                if (PositionValueZ != 0 || PositionValueY != 0 || PositionValueX != 0) {
                    
                    _homeReturnWorker = new BackgroundWorker {
                        WorkerSupportsCancellation = true
                    };
                    _homeReturnWorker.DoWork += HomeReturn_DoWork;
                    _homeReturnWorker.RunWorkerCompleted += HomeReturn_RunWorkerCompleted;
                    _homeReturnWorker.RunWorkerAsync();
                } else {
                    Console.WriteLine("홈이 이미 잡혀있습니다");
                    StaticControllerSignal.ControlRock = false;
                    _homeReturnWorker?.CancelAsync();
                    _homeReturnWorker = null;
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }

        private void HomeReturn_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e) {
            StaticControllerSignal.ControlRock = false;
            _homeReturnWorker?.CancelAsync();
            _homeReturnWorker = null;
            Console.WriteLine("홈셋완료");
        }

        private void HomeReturn_DoWork (object sender, DoWorkEventArgs e) {
            _seqWorker?.CancelAsync();
            
            CAXM.AxmHomeSetStart((int)Servo_Index.SERVO_Z); // Z
            Delay(1000);

            CAXM.AxmHomeSetStart((int)Servo_Index.SERVO_Y); // Y
            CAXM.AxmHomeSetStart((int)Servo_Index.SERVO_X); // X

            while (HomeReturnFinish((int)Servo_Index.SERVO_Z) != true && HomeReturnFinish((int)Servo_Index.SERVO_Y) != true && HomeReturnFinish((int)Servo_Index.SERVO_X) != true) {
                StaticControllerSignal.ControlRock = true;
            }
        }

        /// <summary>
        /// HOMESET이 완료되었는지 값을 반환 받습니다 True = 완료, False = 미완료
        /// </summary>
        /// <param name="MotionIndexNum"></param>
        /// <returns></returns>
        public bool HomeReturnFinish (int MotionIndexNum) {
            uint posStateValue = 9;
            CAXM.AxmHomeGetResult(MotionIndexNum, ref posStateValue);
            if (posStateValue != 1) {
                return false;
            } else {
                return true;
            }
            
        }
    }
}
