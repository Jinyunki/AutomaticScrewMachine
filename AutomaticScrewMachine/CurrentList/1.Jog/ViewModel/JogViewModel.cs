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

namespace AutomaticScrewMachine.CurrentList._1.Jog.ViewModel {
    public class JogViewModel : JogData {
        private BackgroundWorker _DIOWorker;
        private BackgroundWorker _motionWorker;
        private BackgroundWorker _seqWorker;
        private BackgroundWorker _homeReturnWorker;

        public JogViewModel () {
            StatusReciver.StartStatusRead();
            //SerialPortAdapter.ConnectedSerial();
            //SerialPortAdapter.WriteTorqSerial();
            DefaultSet(); // 기본

            ThreadWorker();
            // EMERGENCY
            EmergencyStopCommand = new RelayCommand(EmergencyStop);
            // HOME
            HomeCommand = new RelayCommand(CmdHomeReturn);
            // Button TriggerEvent
            SetButtonEvent();
        }
        public void DefaultSet () {
            // 컨트롤락
            StaticControllerSignal.ControlRock = false;
            // 조그 이동 속도 기본 설정
            JogMoveSpeed = 1;

        }


        public void ThreadWorker () {
            Messenger.Default.Register<SignalMessage>(this, HandleSignalMessage); // 이벤트 관장 핸들러 메신저 Jog 컨트롤

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
                    brush = StatusReciver.INPORT_START_LEFT_BUTTON == 0 ? Brushes.Gray : Brushes.Green;
                    OutportInput(indexNum, StatusReciver.INPORT_START_LEFT_BUTTON);
                    break;
                case 1: // 리셋버튼
                    brush = StatusReciver.INPORT_RESET_BUTTON == 0 ? Brushes.Gray : Brushes.Orange;
                    OutportInput(indexNum, StatusReciver.INPORT_RESET_BUTTON);
                    if (StatusReciver.INPORT_RESET_BUTTON == 1) {
                        CmdHomeReturn();
                    }
                    break;
                case 2:
                    brush = StatusReciver.INPORT_EMG_BUTTON == 0 ? Brushes.Gray : Brushes.Red;
                    OutportInput(indexNum, StatusReciver.INPORT_EMG_BUTTON);
                    break;
                case 3:
                    brush = StatusReciver.INPORT_START_RIGHT_BUTTON == 0 ? Brushes.Gray : Brushes.Green;
                    OutportInput(indexNum, StatusReciver.INPORT_START_RIGHT_BUTTON);
                    break;


                case 13:
                    brush = StatusReciver.INPORT_JIG_PORT1 == 0 ? Brushes.Transparent : Brushes.Black;
                    break;
                case 14:
                    brush = StatusReciver.INPORT_JIG_PORT2 == 0 ? Brushes.Transparent : Brushes.Black;
                    break;
                case 15:
                    brush = StatusReciver.INPORT_JIG_PORT3 == 0 ? Brushes.Transparent : Brushes.Black;
                    break;
                case 16:
                    brush = StatusReciver.INPORT_JIG_PORT4 == 0 ? Brushes.Transparent : Brushes.Black;
                    break;
                case 17:
                    brush = StatusReciver.INPORT_JIG_PORT5 == 0 ? Brushes.Transparent : Brushes.Black;
                    break;

                case 19:
                    brush = StatusReciver.INPORT_EMERGENCY_SENSOR == 0 ? Brushes.Red : Brushes.Transparent; // EMG 센서는 반대로 임
                    if (StatusReciver.INPORT_EMERGENCY_SENSOR == 0 && PositionValueY < 330000) {
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

                SelfStartButton = SetInportBind(0);
                SelfResetButton = SetInportBind(1);
                SelfEmgButton = SetInportBind(2);
                SelfStart2Button = SetInportBind(3);

                if (SelfStartButton == Brushes.Green && SelfStart2Button == Brushes.Green) {
                    GetSequenceStart();
                }

                if (SelfEmgButton == Brushes.Red) {
                    EmergencyStop();
                } 

                //비상정지
                EmgLine = SetInportBind(19);

                //JIG 내부 센서
                JigP1 = SetInportBind(13);
                JigP2 = SetInportBind(14);
                JigP3 = SetInportBind(15);
                JigP4 = SetInportBind(16);
                JigP5 = SetInportBind(17);

                ScrewSupplyOnoff = StatusReciver.INPORT_SUPPLY_SCREW_SENSOR == 1 ? Brushes.Red : Brushes.Gray;
                ScrewSupplyINOUT = StatusReciver.INPORT_SUPPLY_SCREW_SENSOR == 1 ? Brushes.Green : Brushes.Gray;

                DriverBuzzer = SetOutportBind(8);
                DepthBuzzer = SetOutportBind(9);
                VacuumBuzzer = SetOutportBind(10);

                // 머신 상단 알람 부저
                BuzzerAlarmOK = SetOutportBind(23);
                BuzzerAlarmERR = SetOutportBind(22);
                BuzzerAlarmNG = SetOutportBind(21);

                P1_OK = SetOutportBind(11);
                P2_OK = SetOutportBind(12);
                P3_OK = SetOutportBind(13);
                P4_OK = SetOutportBind(14);
                P5_OK = SetOutportBind(15);

                P1_NG = SetOutportBind(16);
                P2_NG = SetOutportBind(17);
                P3_NG = SetOutportBind(18);
                P4_NG = SetOutportBind(19);
                P5_NG = SetOutportBind(20);

                NGBOX = SetOutportBind(7);
            }
        }

        private void Motion_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e) {
            Console.WriteLine("Motion Worker 종료");
        }

        private void Motion_DoWork (object sender, DoWorkEventArgs e) {
            // BackgroundWorker가 캔슬신호를 받기전까지 무한루프.
            while (!_motionWorker.CancellationPending) {
                ServoX = RecevieSignalColor(StatusReciver.SERVO_ONOFF_SIGNAL_X);
                ServoY = RecevieSignalColor(StatusReciver.SERVO_ONOFF_SIGNAL_Y);
                ServoZ = RecevieSignalColor(StatusReciver.SERVO_ONOFF_SIGNAL_Z);

                BuzzerX = RecevieSignalColor(StatusReciver.SERVO_MOVE_STATUS_X);
                BuzzerY = RecevieSignalColor(StatusReciver.SERVO_MOVE_STATUS_Y);
                BuzzerZ = RecevieSignalColor(StatusReciver.SERVO_MOVE_STATUS_Z);

                PositionValueX = StatusReciver.SERVO_POSITION_VALUE_X;
                PositionValueY = StatusReciver.SERVO_POSITION_VALUE_Y;
                PositionValueZ = StatusReciver.SERVO_POSITION_VALUE_Z;

                DriverPosList = new Thickness(PositionValueX * 0.00155, PositionValueY * 0.00165, 0, 0);
                ScrewMCForcus = PositionValueZ;
            }
        }
        private Brush SetOutportBind (int indexIONumber) {
            Brush returnBrs;
            switch (indexIONumber) {
                case 7:
                    returnBrs = StatusReciver.INPORT_NGBOX_OFF == 0 ? Brushes.Transparent : Brushes.Gray;
                    break;
                case 8:
                    //DriverBuzzerSignal = StatusReciver.OUTPORT_SCREW_DRIVER;
                    returnBrs = StatusReciver.OUTPORT_SCREW_DRIVER == 0 ? Brushes.Gray : Brushes.Green;
                    break;
                case 9:
                    //DepthBuzzerSignal = StatusReciver.OUTPORT_DEPTH_CHECKER;
                    returnBrs = StatusReciver.OUTPORT_DEPTH_CHECKER == 0 ? Brushes.Gray : Brushes.Green;
                    break;
                case 10:
                    //VacuumBuzzerSignal = StatusReciver.OUTPORT_SCREW_VACUUM;
                    returnBrs = StatusReciver.OUTPORT_SCREW_VACUUM == 0 ? Brushes.Gray : Brushes.Green;
                    break;
                case 11:
                    returnBrs = StatusReciver.OUTPORT_LED_OK1 == 0 ? Brushes.Gray : Brushes.Green;
                    break;
                case 12:
                    returnBrs = StatusReciver.OUTPORT_LED_OK2 == 0 ? Brushes.Gray : Brushes.Green;
                    break;
                case 13:
                    returnBrs = StatusReciver.OUTPORT_LED_OK3 == 0 ? Brushes.Gray : Brushes.Green;
                    break;
                case 14:
                    returnBrs = StatusReciver.OUTPORT_LED_OK4 == 0 ? Brushes.Gray : Brushes.Green;
                    break;
                case 15:
                    returnBrs = StatusReciver.OUTPORT_LED_OK5 == 0 ? Brushes.Gray : Brushes.Green;
                    break;
                case 21:
                    returnBrs = StatusReciver.OUTPORT_BUZZER_NG == 0 ? Brushes.Gray : Brushes.Red;
                    CAXD.AxdoWriteOutport(24, StatusReciver.OUTPORT_BUZZER_NG);
                    break;
                case 22:
                    returnBrs = StatusReciver.OUTPORT_BUZZER_ERROR == 0 ? Brushes.Gray : Brushes.Orange;
                    break;
                case 23:
                    returnBrs = StatusReciver.OUTPORT_BUZZER_OK == 0 ? Brushes.Gray : Brushes.Green;
                    break;
                case 16:
                    returnBrs = StatusReciver.OUTPORT_LED_NG1 == 0 ? Brushes.Gray : Brushes.Red;
                    break;
                case 17:
                    returnBrs = StatusReciver.OUTPORT_LED_NG2 == 0 ? Brushes.Gray : Brushes.Red;
                    break;
                case 18:
                    returnBrs = StatusReciver.OUTPORT_LED_NG3 == 0 ? Brushes.Gray : Brushes.Red;
                    break;
                case 19:
                    returnBrs = StatusReciver.OUTPORT_LED_NG4 == 0 ? Brushes.Gray : Brushes.Red;
                    break;
                case 20:
                    returnBrs = StatusReciver.OUTPORT_LED_NG5 == 0 ? Brushes.Gray : Brushes.Red;
                    break;
                default:
                    returnBrs = Brushes.Black;
                    break;
            }

            return returnBrs;
        }


        private void SetButtonEvent () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                if (!StaticControllerSignal.ControlRock) {

                    //SEQ BTN
                    AddPosition = new RelayCommand(AddPos); // Seq 추가
                    RemoveSequenceCommand = new RelayCommand(RemoveSelectedSequenceListItem); // SEQ삭제
                    RemovePositionCommand = new RelayCommand(RemoveSelectedPositionListItem); // POS삭제
                    SequenceStart = new RelayCommand(GetSequenceStart);

                    //NGBOX ONOFF
                    NGBoxCommand = new RelayCommand(() => CAXD.AxdoWriteOutport(7, StatusReciver.INPORT_NGBOX_OFF == 0 ? 1u : 0));

                    // Servo OnOff
                    ServoCheckX = new RelayCommand(() => SetServoState((int)ServoIndex.XPOSITION, StatusReciver.SERVO_ONOFF_SIGNAL_X));
                    ServoCheckY = new RelayCommand(() => SetServoState((int)ServoIndex.YPOSITION, StatusReciver.SERVO_ONOFF_SIGNAL_Y));
                    ServoCheckZ = new RelayCommand(() => SetServoState((int)ServoIndex.ZPOSITION, StatusReciver.SERVO_ONOFF_SIGNAL_Z));
                    
                    // Sylinder IO
                    DriverIO = new RelayCommand(() => DIO_WriteOutport((int)DIOIndex.DRIVER, StatusReciver.OUTPORT_SCREW_DRIVER));
                    DepthIO = new RelayCommand(() => DIO_WriteOutport((int)DIOIndex.DEPTH, StatusReciver.OUTPORT_DEPTH_CHECKER));
                    VacuumIO = new RelayCommand(() => DIO_WriteOutport((int)DIOIndex.VACUUM, StatusReciver.OUTPORT_SCREW_VACUUM));
                    
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
                    MovePositionSupply = new RelayCommand(() => GetMoveMultiPosition(SupplyPosition));

                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }


        }

        //TODO : TABCOUNT 구조 변경해야함
        private int _tabCnt = 1;
        public int TabCnt {
            get { return _tabCnt; }
            set {
                _tabCnt = value;
                RaisePropertyChanged(nameof(TabCnt));
            }
        }
        private void CommandMoveJIG (double intaval) {
            if (TabCnt == 1) {
                Interval = intaval;
                Port1Position = new double[2] { StartPortXPos + Interval, StartPortYPos };
                GetMoveMultiPosition(Port1Position);
                TabCnt++;
            } else {
                Interval = intaval;
                Port1Position = new double[2] { StartPortXPos + Interval + GetTabInterval, StartPortYPos };
                GetMoveMultiPosition(Port1Position);
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
                        SequenceListStart = new RelayCommand(GetSequenceStart)
                    };

                    SequenceDataList.Add(seq);
                }

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }
        }

        private void SetServoState (int axis, uint value) {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                CAXM.AxmSignalServoOn(axis, (uint)(value == 0 ? 1 : 0));
                SetServoAlarm(axis, value);
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }
        private void SetServoAlarm (int axis, uint value) {
            if (value > 1) {
                switch (axis) {
                    case 0:
                        BuzzerY = RecevieSignalColor(value);
                        break;
                    case 1:
                        BuzzerX = RecevieSignalColor(value);
                        break;
                    case 2:
                        BuzzerZ = RecevieSignalColor(value);
                        break;
                }
                CAXM.AxmSignalServoAlarmReset(axis, 1);
            }
        }
        private void DIO_WriteOutport (int axis, uint value) {
            CAXD.AxdoWriteOutport(axis, value == 0 ? 1u : 0);
        }

        private void HandleSignalMessage (SignalMessage message) {
            try {
                if (StaticControllerSignal.ControlRock == false) {
                    var isPress = message.IsPress;
                    var isViewName = message.IsViewName;
                    switch (isViewName) {
                        // y 전후방
                        case string n when n == StaticControllerSignal.JOG_STRAIGHT:
                            CAXM.AxmMoveVel(0, -MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                            break;
                        case string n when n == StaticControllerSignal.JOG_BACK:
                            CAXM.AxmMoveVel(0, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                            break;


                        // x 좌우
                        case string n when n == StaticControllerSignal.JOG_LEFT:
                            CAXM.AxmMoveVel(1, -MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                            break;
                        case string n when n == StaticControllerSignal.JOG_RIGHT:
                            CAXM.AxmMoveVel(1, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                            break;


                        // z 위아래
                        case string n when n == StaticControllerSignal.JOG_UP:
                            CAXM.AxmMoveVel(2, -MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);
                            break;

                        case string n when n == StaticControllerSignal.JOG_DOWN:
                            CAXM.AxmMoveVel(2, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);
                            break;

                        case string n when n == StaticControllerSignal.IO_SERVO_X:
                            SetServoState((int)ServoIndex.XPOSITION, StatusReciver.SERVO_MOVE_STATUS_X);
                            break;
                            
                        case string n when n == StaticControllerSignal.IO_SERVO_Y:
                            SetServoState((int)ServoIndex.YPOSITION, StatusReciver.SERVO_MOVE_STATUS_Y);
                            break;
                            
                        case string n when n == StaticControllerSignal.IO_SERVO_Z:
                            SetServoState((int)ServoIndex.ZPOSITION, StatusReciver.SERVO_MOVE_STATUS_Z);
                            break;

                        default:
                            break;
                    }

                    if (!isPress) {
                        CAXM.AxmMoveSStop(0);
                        CAXM.AxmMoveSStop(1);
                        CAXM.AxmMoveSStop(2);
                    }
                } 
                


            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }

        #region ListBoxConf
        private void AddPos () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                TitleName = (PositionDataList.Count + 1).ToString();

                PosData posData = new PosData() {
                    Name = TitleName,
                    X = PositionValueX,
                    Y = PositionValueY,
                    Z = PositionValueZ,
                    Driver_IO = StatusReciver.OUTPORT_SCREW_DRIVER,
                    Depth_IO = StatusReciver.OUTPORT_DEPTH_CHECKER
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
        private void GetSequenceStart () {
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
            GetMoveMultiPosition(SupplyPosition);
        }

        private void SEQ_DoWork (object sender, DoWorkEventArgs e) {

            for (int i = 0; i < PositionDataList.Count; i++) {
                double[] SeqXYHoldPos = new double[2] { PositionDataList[i].X, PositionDataList[i].Y };

                GetMoveMultiPosition(SupplyPosition); //xy 공급기

                MoveDownPos(SupplyPosition, 50000); // z 스크류 공급

                WriteOutport(true); // io 스크류 체득

                MoveUpPos(10000); // z 이동을위한 업

                MoveJIGPos(SeqXYHoldPos); // xy 지그 위치로 이동

                MoveDownPos(SeqXYHoldPos, 30000); // 지그 구멍에 맞춰 체결할곳으로 하강

                Delay(700); // 차후 토크 컨트롤임

                MoveUpPos(10000);

                WriteOutport(false);

            }

            Delay(500); // 시퀀스 작업이 모두 종료후 0.5초동안 잠시 대기 후
        }

        private void MoveJIGPos (double[] recipePosition) {

            if (!((int)recipePosition[0] == (int)PositionValueX && (int)recipePosition[1] == (int)PositionValueY)) {
                MultiMovePos(recipePosition, MC_JogSpeed, MC_JogAcl, MC_JogDcl); // 포지션 위치 로 무빙 시작
            }
            while (!((int)recipePosition[0] == (int)PositionValueX && (int)recipePosition[1] == (int)PositionValueY)) { // XY 이동이 종료 될때까지
                Delay(100);
            }

        }

        private void WriteOutport (bool onOff) {
            uint IO_onOff = onOff ? 1u : 0u;
            CAXD.AxdoWriteOutport((int)DIOIndex.DRIVER, IO_onOff); // Driver Down
            CAXD.AxdoWriteOutport((int)DIOIndex.VACUUM, IO_onOff); // Vaccum ON
            Delay(500);
        }

        private void MoveDownPos (double[] xyStatus, double downPos) {
            while (!((int)xyStatus[0] == (int)PositionValueX && (int)xyStatus[1] == (int)PositionValueY)) { // XY 이동이 종료 될때까지
                Delay(100);
            }

            if ((int)xyStatus[0] == (int)PositionValueX && (int)xyStatus[1] == (int)PositionValueY) { // XY 가 다시한번 정상인것을 확인 후 하강
                CAXM.AxmMoveStartPos(2, downPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
            }

            while ((int)PositionValueZ != downPos) {
                Delay(100);
            }

        }
        private void MoveUpPos (double downPos) {
            CAXM.AxmMoveStartPos(2, downPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
            while ((int)PositionValueZ != downPos) {
                Delay(100);
            }
        }

        private void GetMoveMultiPosition (double[] XYHoldPos) {
            double _moveAbleZPosition = 10000; //마지노선
            if (StatusReciver.OUTPORT_SCREW_DRIVER != 0 && StatusReciver.OUTPORT_SCREW_VACUUM != 0) {
                Crash_IO_OnOff(false);
            }
            if (PositionValueZ > _moveAbleZPosition) {
                CAXM.AxmMovePos((int)ServoIndex.ZPOSITION, _moveAbleZPosition, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                MultiMovePos(XYHoldPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
            } else {
                MultiMovePos(XYHoldPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
            }
        }

        private void MultiMovePos (double[] positionList, double jogSpeed, double jogAcl, double jogDcl) {
            int[] jogList = { 1, 0 }; // X,Y
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

                CAXM.AxmMoveEStop(0);
                CAXM.AxmMoveEStop(1);
                CAXM.AxmMoveEStop(2);

                CAXM.AxmSignalServoOn(2, 0);
                CAXM.AxmSignalServoOn(1, 0);
                CAXM.AxmSignalServoOn(0, 0);

                Crash_IO_OnOff(false);


            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }


        /// <summary>
        /// DI/O 를 모두 종료 시킵니다
        /// </summary>
        /// <param name="OnOff"> True = On, False = Off </param>
        private void Crash_IO_OnOff (bool OnOff) {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {

                uint returnOnof = OnOff ? 1u : 0u;
                CAXD.AxdoWriteOutport((int)DIOIndex.DRIVER, returnOnof);
                CAXD.AxdoWriteOutport((int)DIOIndex.DEPTH, returnOnof);
                CAXD.AxdoWriteOutport((int)DIOIndex.VACUUM, returnOnof);

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

                CAXM.AxmSignalServoOn(2, 1);
                CAXM.AxmSignalServoOn(1, 1);
                CAXM.AxmSignalServoOn(0, 1);

                Crash_IO_OnOff(false);
                SetHomeReturnSpeed(100000);

                CAXM.AxmHomeSetStart(2); // Z
                Delay(1000);
                CAXM.AxmHomeSetStart(0); // Y
                CAXM.AxmHomeSetStart(1); // X

                if (GetHomeReturnStateValue((int)ServoIndex.ZPOSITION) != 1 || GetHomeReturnStateValue((int)ServoIndex.YPOSITION) != 1 || GetHomeReturnStateValue((int)ServoIndex.XPOSITION) != 1) {
                    _homeReturnWorker = new BackgroundWorker {
                        WorkerSupportsCancellation = true
                    };
                    _homeReturnWorker.DoWork += HomeReturn_DoWork;
                    _homeReturnWorker.RunWorkerCompleted += HomeReturn_RunWorkerCompleted;
                    _homeReturnWorker.RunWorkerAsync();
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }

        private void HomeReturn_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e) {
            StaticControllerSignal.ControlRock = false;
            _homeReturnWorker.CancelAsync();
        }

        private void HomeReturn_DoWork (object sender, DoWorkEventArgs e) {
            _seqWorker?.CancelAsync();
            while (GetHomeReturnStateValue((int)ServoIndex.ZPOSITION) != 1 && GetHomeReturnStateValue((int)ServoIndex.YPOSITION) != 1 && GetHomeReturnStateValue((int)ServoIndex.XPOSITION) != 1) {
                StaticControllerSignal.ControlRock = true;
            }
        }

        /// <summary>
        /// 반환값 : 1이면 정상적으로 홈포지션 성공
        /// </summary>
        /// <param name="MotionIndexNum"></param>
        /// <returns></returns>
        public uint GetHomeReturnStateValue (int MotionIndexNum) {
            uint posStateValue = 9;
            CAXM.AxmHomeGetResult(MotionIndexNum, ref posStateValue);
            return posStateValue;
        }
    }
}
