using AutomaticScrewMachine.Bases;
using AutomaticScrewMachine.CurrentList._1.Jog.Model;
using AutomaticScrewMachine.Utiles;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media;
using static OfficeOpenXml.ExcelErrorValue;

namespace AutomaticScrewMachine.CurrentList._1.Jog.ViewModel {
    public class JogViewModel : JogData {
        private BackgroundWorker _DIOWorker ;
        private BackgroundWorker _motionWorker ;
        private BackgroundWorker _seqWorker ;

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
        public void DefaultSet () {
            // 컨트롤락
            ControlRock = false;
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

        private void DIO_DoWork (object sender, DoWorkEventArgs e) {
            while (!_DIOWorker.CancellationPending) {
                Delay(100);
                DriverBuzzer = DIO_Brush(8);
                DepthBuzzer = DIO_Brush(9);
                VacuumBuzzer = DIO_Brush(10);
            }
        }

        private void Motion_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e) {
            Console.WriteLine("Motion Worker 종료");
        }

        private void Motion_DoWork (object sender, DoWorkEventArgs e) {
            // BackgroundWorker가 캔슬신호를 받기전까지 무한루프.
            while (!_motionWorker.CancellationPending) {
                CAXM.AxmSignalIsServoOn(1, ref valueX);
                ServoX = RecevieSignalColor(ReturnServoState(1, valueX));
                CAXM.AxmSignalIsServoOn(0, ref valueY);
                ServoY = RecevieSignalColor(ReturnServoState(0, valueY));
                CAXM.AxmSignalIsServoOn(2, ref valueZ);
                ServoZ = RecevieSignalColor(ReturnServoState(2, valueZ));

                BuzzerX = RecevieSignalColor(ReturnMotionState(1));
                BuzzerY = RecevieSignalColor(ReturnMotionState(0));
                BuzzerZ = RecevieSignalColor(ReturnMotionState(2));

                PositionValueX = ReturnPosValue(1);
                PositionValueY = ReturnPosValue(0);
                PositionValueZ = ReturnPosValue(2);
                
                //DriverPosList = new Thickness(PositionValueX * 0.00098, PositionValueY * 0.0009, 0, 0);
                DriverPosList = new Thickness(PositionValueX * 0.00155, PositionValueY * 0.00165, 0, 0);
                ScrewMCForcus = PositionValueZ;


                // X나 Y축이 움직이고있으면
                if (BuzzerX.ToString() != "#FF808080" || BuzzerY.ToString() != "#FF808080") {
                    XYMoveState = true;
                    //Console.WriteLine("이동중");
                    // 멈춰있으면
                } else {
                    XYMoveState = false;
                    //Console.WriteLine("멈춰있습니다");
                }

                if (BuzzerZ.ToString() != "#FF808080") {
                    ZMoveState = true;
                } else {
                    ZMoveState = false;
                }


            }
        }
        public bool XYMoveState = false;
        public bool ZMoveState = false;
        private Brush DIO_Brush (int indexIONumber) {
            uint temp = 9;
            CAXD.AxdoReadOutport(indexIONumber, ref temp);
            return RecevieSignalColor(temp);
        }

        private void SetButtonEvent () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                if (!ControlRock) {

                    //SEQ BTN
                    AddPosition = new RelayCommand(AddPos); // Seq 추가
                    RemoveSequenceCommand = new RelayCommand(RemoveSelectedSequenceListItem); // SEQ삭제
                    RemovePositionCommand = new RelayCommand(RemoveSelectedPositionListItem); // POS삭제
                    SequenceStart = new RelayCommand(GetSequenceStart);
                    // Servo
                    ServoCheckX = new RelayCommand(() => SetServoState((int)ServoIndex.XPOSITION, valueX));
                    ServoCheckY = new RelayCommand(() => SetServoState((int)ServoIndex.YPOSITION, valueY));
                    ServoCheckZ = new RelayCommand(() => SetServoState((int)ServoIndex.ZPOSITION, valueZ));
                    // Sylinder IO
                    DriverIO = new RelayCommand(() => DIO_WriteOutport((int)DIOIndex.DRIVER, DriverBuzzerSignal));
                    DepthIO = new RelayCommand(() => DIO_WriteOutport((int)DIOIndex.DEPTH, DepthBuzzerSignal));
                    VacuumIO = new RelayCommand(() => DIO_WriteOutport((int)DIOIndex.VACUUM, VacuumBuzzerSignal));
                    // ExcelIO
                    ReadRecipe = new RelayCommand(ReadRecipeCommand);
                    AddRecipe = new RelayCommand(AddRecipeCommand);
                    SavePosDataRecipe = new RelayCommand(SaveRecipeCommand);
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }


        }

        private void AddRecipeCommand () {

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
                Name = "Recipe#" + (SequenceDataList.Count+1)
            });
        }

        private void SaveRecipeCommand () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                if (SelectedSequenceItem != null ) {
                    if (SelectedPositionItem != null)
                    {
                        ExcelAdapter.Save(isFolderName, isFileName, SequenceDataList.IndexOf(SelectedSequenceItem), PositionDataList.IndexOf(SelectedPositionItem), PositionDataList);
                    } else
                    {
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
            CAXM.AxmSignalServoOn(axis, (uint)(value == 0 ? 1 : 0));
            SetServoAlarm(axis, value);
        }
        private void SetServoAlarm (int axis, uint value) {
            CAXM.AxmStatusReadServoAlarm(axis, 0, ref value);
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
            CAXD.AxdoReadOutport(axis, ref value);
            CAXD.AxdoWriteOutport(axis, (uint)(value == 0 ? 1 : 0));
            switch (axis) {
                case 8 :
                    DriverBuzzer = RecevieSignalColor(value);
                    break;
                case 9 :
                    DepthBuzzer = RecevieSignalColor(value);
                    break;
                case 10 :
                    VacuumBuzzer = RecevieSignalColor(value);
                    break;
            }
        }

        private void HandleSignalMessage (SignalMessage message) {
            try {
                var isPress = message.IsPress;
                var isViewName = message.IsViewName;
                if (!ControlRock) {
                    /*if (PositionValueZ > 20000) {
                        SetOff_DIO();
                    }*/
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
                    Driver_IO = DriverBuzzerSignal,
                    Depth_IO = DepthBuzzerSignal
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
            Console.WriteLine("FINISH");
        }

        private void SEQ_DoWork (object sender, DoWorkEventArgs e) {

            for (int i = 0; i < PositionDataList.Count; i++) {
                double[] SeqXYHoldPos = new double[2] { PositionDataList[i].X, PositionDataList[i].Y };

                MoveScrewSupplyPos(SequenceReadyPosition); //xy 공급기

                MoveDownPos(SequenceReadyPosition, 50000); // z 스크류 공급

                WriteOutport(true); // io 스크류 체득

                MoveUpPos(10000); // z 이동을위한 업

                MoveJIGPos(SeqXYHoldPos); // xy 지그 위치로 이동

                MoveDownPos(SeqXYHoldPos, 30000); // 지그 구멍에 맞춰 체결할곳으로 하강
                
                Delay(700); // 차후 토크 컨트롤임

                MoveUpPos(10000);

                WriteOutport(false);


            }

            Delay(500); // 시퀀스 작업이 모두 종료후 0.5초동안 잠시 대기 후
            MoveScrewSupplyPos(SequenceReadyPosition);
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

            CAXD.AxdoReadOutport((int)DIOIndex.DRIVER, ref DriverBuzzerSignal); // IO Driver 값 수신
            CAXD.AxdoReadOutport((int)DIOIndex.VACUUM, ref VacuumBuzzerSignal); // IO Vaccum 값 수신
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

        private void MoveScrewSupplyPos (double[] XYHoldPos) {
            if (DriverBuzzerSignal != 0 && VacuumBuzzerSignal != 0) {
                DIO_OnOff(false);
            }
            if (PositionValueZ > MoveAbleZPosition) {
                CAXM.AxmMovePos((int)ServoIndex.ZPOSITION, MoveAbleZPosition, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
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


        private double ReturnPosValue(int index) {
            double dp = 9;
            CAXM.AxmStatusGetCmdPos(index, ref dp);
            return dp;
        }
        private uint ReturnMotionState (int index) {
            uint movingSignal = 9;
            CAXM.AxmStatusReadInMotion(index, ref movingSignal);
            return movingSignal;
        }
        private uint ReturnServoState (int index, uint value) {
            CAXM.AxmSignalIsServoOn(index, ref value);

            return value;
        }
        #endregion
        
        private void EmergencyStop () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try { 
                
                CAXM.AxmMoveEStop(0);
                CAXM.AxmMoveEStop(1);
                CAXM.AxmMoveEStop(2);
                
                CAXM.AxmSignalServoOn(2, (uint)(valueZ == 0 ? 1 : 0));
                CAXM.AxmSignalServoOn(1, (uint)(valueX == 0 ? 1 : 0));
                CAXM.AxmSignalServoOn(0, (uint)(valueY == 0 ? 1 : 0));

                DIO_OnOff(false);


            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }


        /// <summary>
        /// DI/O 를 모두 종료 시킵니다
        /// </summary>
        /// <param name="OnOff"> True = On, False = Off </param>
        private void DIO_OnOff (bool OnOff) {
            uint returnOnof = OnOff ? 1u : 0u;
            CAXD.AxdoWriteOutport((int)DIOIndex.DRIVER, returnOnof);
            CAXD.AxdoWriteOutport((int)DIOIndex.DEPTH, returnOnof);
            CAXD.AxdoWriteOutport((int)DIOIndex.VACUUM, returnOnof);
        }
        /// <summary>
        /// Home원점 복귀 시 속도를 지정합니다.
        /// </summary>
        /// <param name="moveSpeed"></param>
        private void SetHomeReturnSpeed (double moveSpeed) {
            CAXM.AxmHomeSetVel(2, moveSpeed, moveSpeed / 4, moveSpeed / 8, moveSpeed / 16, moveSpeed * 10, moveSpeed * 10);
            CAXM.AxmHomeSetVel(0, moveSpeed, moveSpeed / 4, moveSpeed / 8, moveSpeed / 16, moveSpeed * 10, moveSpeed * 10);
            CAXM.AxmHomeSetVel(1, moveSpeed, moveSpeed / 4, moveSpeed / 8, moveSpeed / 16, moveSpeed * 10, moveSpeed * 10);
        }
        private void CmdHomeReturn () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                ControlRock = true;
                DIO_OnOff(false);
                SetHomeReturnSpeed(100000);

                CAXM.AxmHomeSetStart(2); // Z
                Delay(1000);
                CAXM.AxmHomeSetStart(0); // Y
                CAXM.AxmHomeSetStart(1); // X

                if (GetHomeReturnStateValue((int)ServoIndex.ZPOSITION) != 1 || GetHomeReturnStateValue((int)ServoIndex.YPOSITION) != 1 || GetHomeReturnStateValue((int)ServoIndex.XPOSITION) != 1) {
                    _HomeReturnDipatcher = new DispatcherTimer {
                        Interval = TimeSpan.FromMilliseconds(100)
                    };
                    _HomeReturnDipatcher.Tick += HomeReturn_Timer_Tick;
                    _HomeReturnDipatcher.Start();
                } 
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }
        public void HomeReturn_Timer_Tick (object sender, EventArgs e) {
            if (GetHomeReturnStateValue((int)ServoIndex.ZPOSITION) == 1 && GetHomeReturnStateValue((int)ServoIndex.YPOSITION) == 1 && GetHomeReturnStateValue((int)ServoIndex.XPOSITION) == 1) {
                ControlRock = false;
                _HomeReturnDipatcher.Stop();
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
