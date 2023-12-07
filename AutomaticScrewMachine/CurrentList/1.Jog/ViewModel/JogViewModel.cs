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
using static OfficeOpenXml.ExcelErrorValue;

namespace AutomaticScrewMachine.CurrentList._1.Jog.ViewModel {
    public class JogViewModel : JogData {
        public JogViewModel () {
            DefaultSet();
            // EMERGENCY
            EmergencyStopCommand = new RelayCommand(EmergencyStop);
            // HOME
            HomeCommand = new RelayCommand(CmdHomeReturn);

            // ServoMotor 제어 Thread Start
            ServoMotorThread();

            // Button TriggerEvent
            SetButtonEvent();
        }

        private void SetButtonEvent () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                if (!ControlRock) {
                    // 이벤트 관장 핸들러 메신저 Jog 컨트롤
                    Messenger.Default.Register<SignalMessage>(this, HandleSignalMessage);

                    //SEQ BTN
                    AddPosition = new RelayCommand(AddPos); // Seq 추가
                    RemoveSequenceCommand = new RelayCommand(RemoveSelectedSequenceListItem); // SEQ삭제
                    RemovePositionCommand = new RelayCommand(RemoveSelectedPositionListItem); // POS삭제
                    SequenceStart = new RelayCommand(GetSequenceStart);

                    
                    // Servo
                    ServoCheckX = new RelayCommand(() => SetServoState((int)ServoIndex.XPOSITION, valueX));
                    ServoCheckY = new RelayCommand(() => SetServoState((int)ServoIndex.YPOSITION, valueY));
                    ServoCheckZ = new RelayCommand(() => SetServoState((int)ServoIndex.ZPOSITION, valueZ));

                    // Sylinder AND Air
                    DriverIO = new RelayCommand(() => SetWriteOutport((int)DIOIndex.DRIVER, DriverSignal));
                    DepthIO = new RelayCommand(() => SetWriteOutport((int)DIOIndex.DEPTH, DepthSignal));
                    VacuumIO = new RelayCommand(() => SetWriteOutport((int)DIOIndex.VACUUM, VacuumSignal));

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

        public void DefaultSet () {
            ControlRock = false;
            JogMoveSpeed = 1;
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


        private void ServoMotorThread () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                Motion_IO_Dispatcher = new DispatcherTimer {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                Motion_IO_Dispatcher.Tick += Motion_Timer_Tick;
                Motion_IO_Dispatcher.Start();
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }

        public void Motion_Timer_Tick (object sender, EventArgs e) {
            GetServoData();
            GetPositionData();
            GetBuzzerData();
            GetSylinderData();
        }

        private void GetSylinderData () {
            CAXD.AxdoReadOutport(8, ref DriverSignal);
            TorqSig = RecevieSignalColor(DriverSignal);
            CAXD.AxdoReadOutport(9, ref DepthSignal);
            DepthSig = RecevieSignalColor(DepthSignal);
            CAXD.AxdoReadOutport(10, ref VacuumSignal);
            VacuumSig = RecevieSignalColor(VacuumSignal);
        }
        public void GetServoData () {
            CAXM.AxmSignalIsServoOn(1, ref valueX);
            ServoX = RecevieSignalColor(ReturnServoState(1, valueX));
            CAXM.AxmSignalIsServoOn(0, ref valueY);
            ServoY = RecevieSignalColor(ReturnServoState(0, valueY));
            CAXM.AxmSignalIsServoOn(2, ref valueZ);
            ServoZ = RecevieSignalColor(ReturnServoState(2, valueZ));
        }
        
        public void GetPositionData () {
            ReturnPosValue(1);
            ReturnPosValue(0);
            ReturnPosValue(2);
            PositionValueX = ReturnPosValue(1);
            PositionValueY = ReturnPosValue(0);
            PositionValueZ = ReturnPosValue(2);

            DriverPosList = new Thickness(PositionValueX * 0.00098, PositionValueY * 0.0009, 0, 0);
            ScrewMCForcus = PositionValueZ;
        }
        public void GetBuzzerData () {
            BuzzerX = RecevieSignalColor(ReturnMotionState(1));
            BuzzerY = RecevieSignalColor(ReturnMotionState(0));
            BuzzerZ = RecevieSignalColor(ReturnMotionState(2));
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
        private void SetWriteOutport (int axis, uint value) {
            CAXD.AxdoWriteOutport(axis, (uint)(value == 0 ? 1 : 0));
        }


        private void HandleSignalMessage (SignalMessage message) {
            try {
                var isPress = message.IsPress;
                var isViewName = message.IsViewName;
                if (!ControlRock) {
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
                    Driver_IO = DriverSignal,
                    Depth_IO = DepthSignal
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
                /**/
                _motionChecker = new DispatcherTimer {
                    Interval = TimeSpan.FromMilliseconds(100),
                };
                _motionChecker.Tick += MotionChecker_Tick;
                _motionChecker.Start();
                /**/


                /**
                if (SelectedSequenceItem != null) {
                    if (CheckServoOn()) {
                        for (int i = 0; i < PositionDataList.Count; i++) {
                            if (SeqStartHoldPosition()) {
                                if (GetterScrewMotion()) { // Z POS스크류 습득
                                    GetMoveXYPos(i); // XY POS
                                }
                                CAXM.AxmMovePos(2, 0, MC_JogSpeed, MC_JogAcl, MC_JogDcl); // Z UP
                            } 
                        }
                        SeqStartHoldPosition();
                    }
                } else {
                    MessageBox.Show("시퀀스가 선택 되어 있지 않습니다. 원점 이동을 시작합니다");
                    CmdHomeReturn();
                }
                /**/

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }
        }

        private void MotionChecker_Tick (object sender, EventArgs e) {
            Console.WriteLine("ReturnMotionState((int)ServoIndex.ZPOSITION) DATA ::::" + ReturnMotionState((int)ServoIndex.ZPOSITION));

            if (ReturnMotionState((int)ServoIndex.YPOSITION) == 0 && ReturnMotionState((int)ServoIndex.XPOSITION) == 0) {

                double[] XYHoldPos = new double[2] { 175000, 113122 };
                MultiMovePos(XYHoldPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                
                // X,Y 축 지정 위치와, 지령 위치의 오차가 10%미만 일때
                if ((int)XYHoldPos[0]*0.1 == (int)PositionValueX*0.1 && (int)XYHoldPos[1]*0.1 == (int)PositionValueY*0.1) {

                    // Z축을 하강시킴
                    CAXM.AxmMoveStartPos(2, 54000, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);

                    // Z축이 모두 하강하여 움직이고 있지않으면
                    if (ReturnMotionState((int)ServoIndex.ZPOSITION) == 0) {

                        CAXD.AxdoWriteOutport((int)DIOIndex.DRIVER, 1); // 1번 = 스위치 온
                        CAXD.AxdoWriteOutport((int)DIOIndex.VACUUM, 1); // 1번 = 스위치 온

                        CAXD.AxdoReadOutport((int)DIOIndex.DRIVER, ref DriverSignal); // IO 토크
                        CAXD.AxdoReadOutport((int)DIOIndex.VACUUM, ref VacuumSignal); // IO Air

                        if (DriverSignal == 1 && VacuumSignal == 1) {
                            Thread.Sleep(500);
                            CAXM.AxmMoveStartPos(2, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl); // 스크류를 습득하여 올라가는것 까지 구현
                            _motionChecker.Stop();
                        }
                        
                    } 

                    
                    /*
                   
                    if (ReturnMotionState((int)ServoIndex.ZPOSITION) == 0) {
                        Console.WriteLine("여기까진옴 zPOS<500");
                        double[] XYHoldPos2 = new double[2] { PositionDataList[0].X, PositionDataList[0].Y }; // X/Y 리스트의 데이터를 참조
                        MultiMovePos(XYHoldPos2, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                        
                        //if ((int)XYHoldPos2[0] * 0.1 == (int)PositionValueX * 0.1 && (int)XYHoldPos2[1] * 0.1 == (int)PositionValueY * 0.1) {
                        //    Console.WriteLine("여기까진옴");
                        //    _motionChecker.Stop();
                        //}
                        //CAXM.AxmMoveStartPos(2, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);
                    }

                    /**/

                }

            } else {
                Console.WriteLine("XXXXX : " + PositionValueX);
                Console.WriteLine("YYYYY : " + PositionValueY);
            }

        }
        private void MultiMovePos (double[] positionList, double jogSpeed, double jogAcl, double jogDcl) {

            int[] jogList = { 1, 0 }; // X,Y
            double[] speedList = { jogSpeed, jogSpeed };
            double[] aclList = { jogAcl, jogAcl };
            double[] dclList = { jogDcl, jogDcl };
            //CAXM.AxmMoveMultiPos(2, jogList, positionList, speedList, aclList, dclList);
            CAXM.AxmMoveStartMultiPos(2, jogList, positionList, speedList, aclList, dclList); 

        }

        /// <summary>
        /// 시퀀스 시작전 대기 장소 (스크류공급기 위치)
        /// </summary>
        private bool SeqStartHoldPosition () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                // DI/O OffSet
                CAXD.AxdoWriteOutport((int)DIOIndex.DRIVER, 0);
                CAXD.AxdoWriteOutport((int)DIOIndex.DEPTH, 0);
                CAXD.AxdoWriteOutport((int)DIOIndex.VACUUM, 0);

                double[] XYHoldPos = new double[2] { 175000, 113122 }; // X/Y 대기 장소
                CAXM.AxmMovePos((int)ServoIndex.ZPOSITION, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl); // 후순위 탈출
                //CAXM.AxmMoveStartPos((int)ServoIndex.ZPOSITION, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl); // 명령 즉시 탈출
                MultiMovePos(XYHoldPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);

                if (XYHoldPos[0] == ReturnPosValue((int)ServoIndex.XPOSITION) && XYHoldPos[1] == ReturnPosValue((int)ServoIndex.YPOSITION) && ReturnPosValue((int)ServoIndex.ZPOSITION) < 1000) {
                    return true;
                } else {
                    return false;
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }
        }

        private void GetMoveXYPos (int index) {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                double[] XYHoldPos = new double[2] { PositionDataList[index].X, PositionDataList[index].Y }; // X/Y 리스트의 데이터를 참조
                MultiMovePos(XYHoldPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }

        private bool CheckServoOn () {
            if (valueX != 0 && valueY != 0 && valueZ != 0) {
                return true;
            } else {
                return false;
            }
        }
        private bool GetterScrewMotion () {
            CAXM.AxmMovePos(2, 54000, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);
            //CAXM.AxmMoveStartPos(2, 54000, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);
            CAXD.AxdoReadOutport((int)DIOIndex.DRIVER,ref DriverSignal); // IO 토크
            SetWriteOutport((int)DIOIndex.DRIVER, DriverSignal);
            CAXD.AxdoReadOutport((int)DIOIndex.VACUUM, ref VacuumSignal); // IO Air
            SetWriteOutport((int)DIOIndex.VACUUM, VacuumSignal);

            Thread.Sleep(500);
            CAXM.AxmMovePos(2, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);
            //CAXM.AxmMoveStartPos(2, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);

            if (PositionValueZ < 500) {
                return true;
            } else {
                return false;
            }
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

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }
        private void CmdHomeReturn () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {

                ControlRock = true;
                // DI/O ZeroSet
                SetWriteOutport((int)DIOIndex.DRIVER, 1);
                SetWriteOutport((int)DIOIndex.DEPTH, 1);
                SetWriteOutport((int)DIOIndex.VACUUM, 1);

                double homeSetSpeed = 80000;

                CAXM.AxmHomeSetVel(2, homeSetSpeed, homeSetSpeed / 4, homeSetSpeed / 8, homeSetSpeed / 16, homeSetSpeed * 10, homeSetSpeed * 10);
                CAXM.AxmHomeSetVel(0, homeSetSpeed, homeSetSpeed / 4, homeSetSpeed / 8, homeSetSpeed / 16, homeSetSpeed * 10, homeSetSpeed * 10);
                CAXM.AxmHomeSetVel(1, homeSetSpeed, homeSetSpeed / 4, homeSetSpeed / 8, homeSetSpeed / 16, homeSetSpeed * 10, homeSetSpeed * 10);

                CAXM.AxmHomeSetStart(2); // Z
                Thread.Sleep(1000);

                CAXM.AxmHomeSetStart(0); // Y
                CAXM.AxmHomeSetStart(1); // X
                if (zPosStateValue != 1 || yPosStateValue != 1 || xPosStateValue != 1) {
                    _positionDipatcher = new DispatcherTimer {
                        Interval = TimeSpan.FromMilliseconds(100)
                    };
                    _positionDipatcher.Tick += Pos_Timer_Tick;
                    _positionDipatcher.Start();
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }
        
        public void Pos_Timer_Tick (object sender, EventArgs e) {
            zPosStateValue = AxinStateControll((int)ServoIndex.ZPOSITION);// PositionValueZ
            yPosStateValue = AxinStateControll((int)ServoIndex.YPOSITION);// PositionValueY
            xPosStateValue = AxinStateControll((int)ServoIndex.XPOSITION);// PositionValueX

            if (zPosStateValue == 1 && yPosStateValue == 1 && xPosStateValue == 1) {
                ControlRock = false;
                _positionDipatcher.Stop();
                zPosStateValue = 9;
                yPosStateValue = 9;
                xPosStateValue = 9;
            }
        }

        public uint AxinStateControll (int MotionIndexNum) {
            uint posStateValue = 9;
            CAXM.AxmHomeGetResult(MotionIndexNum, ref posStateValue);
            return posStateValue;
        }
    }
}
