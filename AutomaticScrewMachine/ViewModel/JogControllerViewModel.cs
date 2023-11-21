﻿using AdapterCollection;
using AutomaticScrewMachine.Model;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace AutomaticScrewMachine.ViewModel
{

    public class JogControllerViewModel : JogController
    {
        public JogControllerViewModel()
        {
            JogMoveSpeed = 1;
            // 이벤트 관장 핸들러 메신저 Jog 컨트롤
            Messenger.Default.Register<SignalMessage>(this, HandleSignalMessage);

            // ServoMotor 제어 Thread Start
            ServoMotorThread();

            //SEQ BTN
            AddPosition = new RelayCommand(AddPos);
            RemoveSelectedCommand = new RelayCommand(RemoveSelected);
            CheckSelectedCommand = new RelayCommand(CheckSelected);

            // HOME, EMERGENCY
            HomeCommand = new RelayCommand(CmdHomeReturn);
            EmergencyStopCommand = new RelayCommand(EmergencyStop);

            // Servo
            ServoCheckX = new RelayCommand(() => SetServoState(1, valueX));
            ServoCheckY = new RelayCommand(() => SetServoState(0, valueY));
            ServoCheckZ = new RelayCommand(() => SetServoState(2, valueZ));

            // Sylinder AND Air
            TorqIO = new RelayCommand(() => SetWriteOutport(8, torqS));
            DepthIO = new RelayCommand(() => SetWriteOutport(9, depthS));
            AirIO = new RelayCommand(() => SetWriteOutport(10, airS));

            ReadRecipe = new RelayCommand(ReadRecipeCommand);
            UpdateRecipe = new RelayCommand(UpdateRecipeCommand);
        }

        private void UpdateRecipeCommand()
        {
            ExcelAdapter.Save();
        }

        private void ReadRecipeCommand()
        {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                ExcelAdapter.IsFolderName = "Data";
                ExcelAdapter.IsFileName = "JogData.xlsx";
                ExcelAdapter.Connect();
                for (int j = 1; j < ExcelAdapter.IsRowCount; j++) // j = 0 CategoryList 그래서 1부터 시작
                {
                    JogData jogData = new JogData
                    {
                        Name = ExcelAdapter.TotalDataList[j][0].ToString(),
                        X = double.Parse(ExcelAdapter.TotalDataList[j][1]),
                        Y = double.Parse(ExcelAdapter.TotalDataList[j][2]),
                        Z = double.Parse(ExcelAdapter.TotalDataList[j][3]),
                        Torq_IO = uint.Parse(ExcelAdapter.TotalDataList[j][4]),
                        Depth_IO = uint.Parse(ExcelAdapter.TotalDataList[j][5])
                    };

                    JogDataList.Add(jogData);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }
        }


        private void ServoMotorThread()
        {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                Motion_IO_Dispatcher = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                Motion_IO_Dispatcher.Tick += Motion_Timer_Tick;
                Motion_IO_Dispatcher.Start();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        public void Motion_Timer_Tick(object sender, EventArgs e)
        {
            GetServoData();
            GetPositionData();
            GetBuzzerData();
            GetSylinderData();
        }

        private void GetSylinderData()
        {
            CAXD.AxdoReadOutport(8, ref torqS);
            TorqSig = RecevieSignalColor(torqS);
            CAXD.AxdoReadOutport(9, ref depthS);
            DepthSig = RecevieSignalColor(depthS);

            CAXD.AxdoReadOutport(10, ref airS);
            AirSig = RecevieSignalColor(airS);
        }
        public void GetServoData()
        {
            CAXM.AxmSignalIsServoOn(1, ref valueX);
            CAXM.AxmSignalIsServoOn(0, ref valueY);
            CAXM.AxmSignalIsServoOn(2, ref valueZ);
            ServoX = RecevieSignalColor(valueX);
            ServoY = RecevieSignalColor(valueY);
            ServoZ = RecevieSignalColor(valueZ);
        }
        public void GetPositionData()
        {
            double dpX = 9;
            double dpY = 9;
            double dpZ = 9;
            CAXM.AxmStatusGetCmdPos(1, ref dpX);
            CAXM.AxmStatusGetCmdPos(0, ref dpY);
            CAXM.AxmStatusGetCmdPos(2, ref dpZ);
            PositionValueX = dpX;
            PositionValueY = dpY;
            PositionValueZ = dpZ;
        }
        public void GetBuzzerData()
        {
            uint moveSigX = 9;
            uint moveSigY = 9;
            uint moveSigZ = 9;
            CAXM.AxmStatusReadInMotion(1, ref moveSigX);
            CAXM.AxmStatusReadInMotion(0, ref moveSigY);
            CAXM.AxmStatusReadInMotion(2, ref moveSigZ);
            BuzzerX = RecevieSignalColor(moveSigX);
            BuzzerY = RecevieSignalColor(moveSigY);
            BuzzerZ = RecevieSignalColor(moveSigZ);

        }

        private void SetServoState(int axis, uint value)
        {
            CAXM.AxmSignalServoOn(axis, (uint)(value == 0 ? 1 : 0));
        }

        private void SetWriteOutport(int axis, uint value)
        {
            CAXD.AxdoWriteOutport(axis, (uint)(value == 0 ? 1 : 0));
        }


        private void HandleSignalMessage(SignalMessage message)
        {
            try
            {
                var isPress = message.IsPress;
                var isViewName = message.IsViewName;
                //if (isPress)
                //{
                switch (isViewName)
                {
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

                if (!isPress)
                {
                    CAXM.AxmMoveSStop(0);
                    CAXM.AxmMoveSStop(1);
                    CAXM.AxmMoveSStop(2);
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }



        #region ListBoxConf
        private void AddPos()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                if (TitleName != "")
                {
                    JogData jogData = new JogData()
                    {
                        Name = TitleName,
                        X = PositionValueX,
                        Y = PositionValueY,
                        Z = PositionValueZ,
                        Torq_IO = torqS,
                        Depth_IO = depthS
                    };

                    JogDataList.Add(jogData);
                    TitleName = "";
                }
                else
                {
                    TitleName = "#" + JogDataList.Count.ToString();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void RemoveSelected()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                if (SelectedItem != null)
                {
                    JogDataList.Remove(SelectedItem);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }
        }
        private void CheckSelected()
        {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                // Z축을 올려주고 진행이됨.
                CAXM.AxmMovePos(2, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);

                //double[] posList = new double[] { SelectedItem.Y, SelectedItem.X }; // Y(0번 인덱스) X(1번 인덱스)가 각각 이동할 위치 의 정보
                //MultiMovePos(2, new double[] { SelectedItem.Y, SelectedItem.X }, MC_JogSpeed, MC_JogAcl,MC_JogDcl);

                for (int i = 0; i < JogDataList.Count; i++)
                {
                    CAXM.AxmMovePos(2, 25000, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);
                    MultiMovePos(2, new double[] { JogDataList[i].Y, JogDataList[i].X }, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                    CAXM.AxmMovePos(2, JogDataList[i].Z, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
                    //SetWriteOutport(8, torqS);
                }

                //CAXM.AxmMovePos(0, SelectedItem.Y, MC_JogSpeed, MC_JogAcl, MC_JogDcl); //Y
                //CAXM.AxmMovePos(1, SelectedItem.X, MC_JogSpeed, MC_JogAcl, MC_JogDcl); //X
                //
                //CAXM.AxmMovePos(2, SelectedItem.Z, MC_JogSpeed * 0.1, MC_JogAcl, MC_JogDcl); //Z
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }
        #endregion

        private void EmergencyStop()
        {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                CAXM.AxmMoveSStop(0);
                CAXM.AxmMoveSStop(1);
                CAXM.AxmMoveSStop(2);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }


        //View.Test test ;
        private void CmdHomeReturn()
        {
            //test = new View.Test();
            //test.Show();
            SetWriteOutport(8, 1);
            SetWriteOutport(9, 1);
            SetWriteOutport(10, 1);
            CAXM.AxmMovePos(2, 0, MC_JogSpeed, MC_JogAcl, MC_JogDcl);
            MultiMovePos(2, new double[] { 0, 0 }, MC_JogSpeed, MC_JogAcl, MC_JogDcl);

            CAXM.AxmHomeSetStart(2); // Z
            CAXM.AxmHomeSetStart(0); // Y
            CAXM.AxmHomeSetStart(1); // X
            if (zPosStateValue != 1 || yPosStateValue != 1 || xPosStateValue != 1)
            {
                _positionDipatcher = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                _positionDipatcher.Tick += Pos_Timer_Tick;
                _positionDipatcher.Start();
            }
        }

        private void MultiMovePos(int AxisCount, double[] positionList, double jogSpeed, double jogAcl, double jogDcl)
        {
            int[] jogList = { 0, 1 };
            double[] speedList = { jogSpeed, jogSpeed };
            double[] aclList = { jogAcl, jogAcl };
            double[] dclList = { jogDcl, jogDcl };

            CAXM.AxmMoveMultiPos(AxisCount, jogList, positionList, speedList, aclList, dclList);
        }

        public void Pos_Timer_Tick(object sender, EventArgs e)
        {
            zPosStateValue = AxinStateControll(2);// PositionValueZ
            yPosStateValue = AxinStateControll(0);// PositionValueY
            xPosStateValue = AxinStateControll(1);// PositionValueX

            BuzzerZ = RecevieSignalColor(zPosStateValue);
            BuzzerY = RecevieSignalColor(yPosStateValue);
            BuzzerX = RecevieSignalColor(xPosStateValue);


            if (zPosStateValue == 1 && yPosStateValue == 1 && xPosStateValue == 1)
            {
                _positionDipatcher.Stop();
                zPosStateValue = 9;
                yPosStateValue = 9;
                xPosStateValue = 9;


                //test.Close();
            }
        }

        public uint AxinStateControll(int MotionIndexNum)
        {
            uint posStateValue = 9;
            CAXM.AxmHomeGetResult(MotionIndexNum, ref posStateValue);
            return posStateValue;
        }
    }
}