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

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                if (!MotionRock) {
                    // 이벤트 관장 핸들러 메신저 Jog 컨트롤
                    Messenger.Default.Register<SignalMessage>(this, HandleSignalMessage);

                    //SEQ BTN
                    AddPosition = new RelayCommand(AddPos); // Seq 추가
                    RemoveSequenceCommand = new RelayCommand(RemoveSelectedSequenceListItem); // 삭제
                    SequenceStart = new RelayCommand(GetSequenceStart);

                    
                    // Servo
                    ServoCheckX = new RelayCommand(() => SetServoState(1, valueX));
                    ServoCheckY = new RelayCommand(() => SetServoState(0, valueY));
                    ServoCheckZ = new RelayCommand(() => SetServoState(2, valueZ));

                    // Sylinder AND Air
                    TorqIO = new RelayCommand(() => SetWriteOutport(8, torqS));
                    DepthIO = new RelayCommand(() => SetWriteOutport(9, depthS));
                    AirIO = new RelayCommand(() => SetWriteOutport(10, airS));

                    ReadRecipe = new RelayCommand(ReadRecipeCommand);
                    AddRecipe = new RelayCommand(AddRecipeCommand);
                    SavePosDataRecipe = new RelayCommand(SaveRecipeCommand);
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
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
                    PositionDataList[i].Torq_IO.ToString(),
                    PositionDataList[i].Depth_IO.ToString(),
                };

                totlaList.Add(itemList);
            }

            string newWorksheetName = "Recipe#" + SequenceDataList.Count;
            ExcelAdapter.Add(isFolderName, isFileName, newWorksheetName, totlaList);

            SequenceDataList.Add(new SequenceData {
                Name = "Recipe#" + SequenceDataList.Count
            });
        }

        public void DefaultSet () {
            MotionRock = false;
            JogMoveSpeed = 1;
        }

        private void SaveRecipeCommand () {
            PositionDataList.Clear();
            ExcelAdapter.Connect(isFolderName, isFileName, 0);
        }

        private void ReadRecipeCommand () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
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
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }
        }


        private void ServoMotorThread () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                Motion_IO_Dispatcher = new DispatcherTimer {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                Motion_IO_Dispatcher.Tick += Motion_Timer_Tick;
                Motion_IO_Dispatcher.Start();
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
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
            CAXD.AxdoReadOutport(8, ref torqS);
            TorqSig = RecevieSignalColor(torqS);
            CAXD.AxdoReadOutport(9, ref depthS);
            DepthSig = RecevieSignalColor(depthS);
            CAXD.AxdoReadOutport(10, ref airS);
            AirSig = RecevieSignalColor(airS);
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

            ScrewPosList = new Thickness(PositionValueX * 0.00098, PositionValueY * 0.0009, 0, 0);
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
                if (!MotionRock) {
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
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }



        #region ListBoxConf
        private void AddPos () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                TitleName = (PositionDataList.Count + 1).ToString();

                PosData posData = new PosData() {
                    Name = TitleName,
                    X = PositionValueX,
                    Y = PositionValueY,
                    Z = PositionValueZ,
                    Torq_IO = torqS,
                    Depth_IO = depthS
                };

                PositionDataList.Add(posData);
                TitleName = "";

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void RemoveSelectedSequenceListItem () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                if (SelectedSequenceItem != null) {
                    ExcelAdapter.Remove(isFolderName, isFileName, RemoveAndGetIndex(SelectedSequenceItem));
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }
        }
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
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {

                if (SeqStartHoldPosition() == true) { // 대기 포지션으로 잘 이동 하였는가
                    Console.WriteLine("준비 Position 도착");
                    if (GetterScrewMotion() == true) { // 스크류캐치 시퀀스를 진행하였는가
                        Console.WriteLine("Screw Catch Succes");
                        for (int i = 0; i < PositionDataList.Count; i++) {
                            CAXM.AxmMovePos(1, PositionDataList[i].X, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                            CAXM.AxmMovePos(0, PositionDataList[i].Y, MC_JogSpeed, MC_JogAcl, MC_JogDcl);

                            CAXM.AxmMovePos(2, PositionDataList[i].Z, MC_JogSpeed, MC_JogAcl, MC_JogDcl);

                            CAXM.AxmMovePos(2, 0, MC_JogSpeed, MC_JogAcl, MC_JogDcl);


                        }
                    }
                    
                } else {
                    Console.WriteLine("준비 position 실패");
                    
                    /**
                    for (int i = 0; i < PositionDataList.Count; i++) {
                        CAXM.AxmMovePos(2, PositionDataList[i].Z - 5000, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);

                        CAXM.AxmMovePos(1, PositionDataList[i].X, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                        CAXM.AxmMovePos(0, PositionDataList[i].Y, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                        CAXM.AxmMovePos(2, PositionDataList[i].Z, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                    }
                    CAXM.AxmMovePos(2, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);

                    /**/

                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }
        }

        private bool GetterScrewMotion () {
            CAXM.AxmMovePos(2, 54000, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);

            CAXD.AxdoReadOutport(8,ref torqS); // IO 토크
            SetWriteOutport(8, torqS);

            CAXD.AxdoReadOutport(10, ref airS); // IO Air
            SetWriteOutport(10, airS);

            Thread.Sleep(500);
            CAXM.AxmMovePos(2, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);

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
        /// <summary>
        /// 시퀀스 시작전 대기 장소 (스크류공급기 위치)
        /// </summary>
        private bool SeqStartHoldPosition () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                double[] XYHoldPos = new double[2] { 175000, 113122 }; // X/Y 대기 장소
                CAXM.AxmMovePos(2, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);
                MultiMovePos(XYHoldPos, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                ReturnPosValue(1);
                ReturnPosValue(0);
                ReturnPosValue(2);
                if (XYHoldPos[0] == ReturnPosValue(1) && XYHoldPos[1] == ReturnPosValue(0) && ReturnPosValue(2) < 1000) {
                    return true;

                } else {
                    return false;
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }
        }
        #endregion

        private void EmergencyStop () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {

                CAXM.AxmMoveEStop(0);
                CAXM.AxmMoveEStop(1);
                CAXM.AxmMoveEStop(2);
                
                CAXM.AxmSignalServoOn(2, (uint)(valueZ == 0 ? 1 : 0));
                CAXM.AxmSignalServoOn(1, (uint)(valueX == 0 ? 1 : 0));
                CAXM.AxmSignalServoOn(0, (uint)(valueY == 0 ? 1 : 0));

                

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }


        //View.Test test ;
        private void CmdHomeReturn () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {

                MotionRock = true;
                // DI/O
                SetWriteOutport(8, 1);
                SetWriteOutport(9, 1);
                SetWriteOutport(10, 1);

                //CAXM.AxmMovePos(2, 0, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                double homeSetSpeed = 80000;
                double homeSetSpeed2 = 20000;
                double homeSetSpeed3 = 10000;
                double homeSetSpeedDev = 5000;

                CAXM.AxmHomeSetVel(2, homeSetSpeed, homeSetSpeed2, homeSetSpeed3, homeSetSpeedDev, homeSetSpeed * 10, homeSetSpeed * 10);
                CAXM.AxmHomeSetVel(0, homeSetSpeed, homeSetSpeed2, homeSetSpeed3, homeSetSpeedDev, homeSetSpeed * 10, homeSetSpeed * 10);
                CAXM.AxmHomeSetVel(1, homeSetSpeed, homeSetSpeed2, homeSetSpeed3, homeSetSpeedDev, homeSetSpeed * 10, homeSetSpeed * 10);

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
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void MultiMovePos (double[] positionList, double jogSpeed, double jogAcl, double jogDcl) {
            int[] jogList = { 1, 0 }; // X,Y
            double[] speedList = { jogSpeed, jogSpeed };
            double[] aclList = { jogAcl, jogAcl };
            double[] dclList = { jogDcl, jogDcl };

            CAXM.AxmMoveMultiPos(2, jogList, positionList, speedList, aclList, dclList);
        }

        public void Pos_Timer_Tick (object sender, EventArgs e) {

            zPosStateValue = AxinStateControll(2);// PositionValueZ
            yPosStateValue = AxinStateControll(0);// PositionValueY
            xPosStateValue = AxinStateControll(1);// PositionValueX

            if (zPosStateValue == 1 && yPosStateValue == 1 && xPosStateValue == 1) {
                MotionRock = false;
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
