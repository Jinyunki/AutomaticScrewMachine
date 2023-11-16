using AutomaticScrewMachine.Model;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AutomaticScrewMachine.ViewModel
{

    public class JogControllerViewModel : JogController
    {
        public JogControllerViewModel()
        {
            // 이벤트 관장 핸들러 메신저
            Messenger.Default.Register<SignalMessage>(this, HandleSignalMessage);

            // MOVE SPEED CTR
            SetMoveSpeed();

            //SEQ BTN
            AddPosition = new RelayCommand(AddPos);
            RemoveSelectedCommand = new RelayCommand(RemoveSelected);
            CheckSelectedCommand = new RelayCommand(CheckSelected);

            // HOME, EMERGENCY
            HomeCommand = new RelayCommand(CmdHomeReturn);
            EmergencyStopCommand = new RelayCommand(EmergencyStop);

            ServoCheckX = new RelayCommand(SetServoStateX);
            ServoCheckY = new RelayCommand(SetServoStateY);
            ServoCheckZ = new RelayCommand(SetServoStateZ);
        }

        private void SetServoStateX()
        {
            uint ServoOn = 1;
            uint ServoOff = 0;
            uint value = 0;
            CAXM.AxmSignalIsServoOn(1, ref value); // 서보 온오프 상태
            Console.WriteLine(value);
            if (value == 0)
            {
                CAXM.AxmSignalServoOn(1, ServoOn); // On
                ServoX = Brushes.Green;
            }
            else
            {
                CAXM.AxmSignalServoOn(1, ServoOff); // Off
                ServoX = Brushes.Gray;
            }
        }
        
        private void SetServoStateY()
        {
            uint ServoOn = 1;
            uint ServoOff = 0;
            uint value = 0;
            CAXM.AxmSignalIsServoOn(0, ref value); // 서보 온오프 상태
            Console.WriteLine(value);
            if (value == 0)
            {
                CAXM.AxmSignalServoOn(0, ServoOn); // On
                ServoY = Brushes.Green;
            }
            else
            {
                CAXM.AxmSignalServoOn(0, ServoOff); // Off
                ServoY = Brushes.Gray;
            }
        }
        
        private void SetServoStateZ()
        {
            uint ServoOn = 1;
            uint ServoOff = 0;
            uint value = 0;
            CAXM.AxmSignalIsServoOn(2, ref value); // 서보 온오프 상태
            Console.WriteLine(value);
            if (value == 0)
            {
                CAXM.AxmSignalServoOn(2, ServoOn); // On
                ServoZ = Brushes.Green;
            }
            else
            {
                CAXM.AxmSignalServoOn(2, ServoOff); // Off
                ServoZ = Brushes.Gray;
            }
        }

        private void HandleSignalMessage(SignalMessage message)
        {
            try
            {
                var isPress = message.IsPress;
                var isViewName = message.IsViewName;
                if (isPress)
                {
                    switch (isViewName)
                    {
                        // y 전후방
                        case string n when n == StaticControllerSignal.JOG_STRAIGHT:
                            PositionValueY = AxinMoveControll(0, -MC_JogSpeed);

                            break;
                        case string n when n == StaticControllerSignal.JOG_BACK:
                            PositionValueY = AxinMoveControll(0, MC_JogSpeed);
                            break;


                        // x 좌우
                        case string n when n == StaticControllerSignal.JOG_LEFT:
                            PositionValueX = AxinMoveControll(1, -MC_JogSpeed);
                            break;
                        case string n when n == StaticControllerSignal.JOG_RIGHT:
                            PositionValueX = AxinMoveControll(1, MC_JogSpeed);
                            break;


                        // z 위아래
                        case string n when n == StaticControllerSignal.JOG_UP:
                            PositionValueZ = AxinMoveControll(2, -MC_JogSpeed * 0.1);
                            break;

                        case string n when n == StaticControllerSignal.JOG_DOWN:
                            PositionValueZ = AxinMoveControll(2, MC_JogSpeed * 0.1);
                            break;


                        default:
                            break;
                    }
                }

                else
                {
                    BuzzerX = Brushes.Gray;
                    BuzzerY = Brushes.Gray;
                    BuzzerZ = Brushes.Gray;
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

        #region JogSpeedCtr
        private void SetMoveSpeed()
        {
            JogMoveSpeed = 1;
            JogSpeedUp = new RelayCommand(SpeedUp);
            JogSpeedDown = new RelayCommand(SpeedDown);
        }

        private void SpeedUp()
        {
            JogMoveSpeed += 0.5;
        }

        private void SpeedDown()
        {
            if (JogMoveSpeed <= 0.101)
            {
                return;
            }
            if (JogMoveSpeed < 1.1)
            {
                JogMoveSpeed -= 0.1;
            }
            else
            {
                JogMoveSpeed -= 0.5;
            }
        }

        #endregion

        #region ListBoxConf
        private void AddPos()
        {
            if (TitleName != "")
            {
                JogData jogData = new JogData()
                {
                    Name = TitleName,
                    X = PositionValueX,
                    Y = PositionValueY,
                    Z = PositionValueZ,
                };

                JogDataList.Add(jogData);
                TitleName = "";
            }
            else
            {
                TitleName = "#" + JogDataList.Count.ToString();
            }
        }

        private void RemoveSelected()
        {
            if (SelectedItem != null)
            {
                JogDataList.Remove(SelectedItem);
            }

            Console.WriteLine(JogDataList.Count.ToString());
        }
        private void CheckSelected()
        {
            // Z축을 올려주고 진행이됨.
            CAXM.AxmMovePos(2, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);

            CAXM.AxmMovePos(0, SelectedItem.Y, MC_JogSpeed, MC_JogAcl, MC_JogDcl); //Y
            CAXM.AxmMovePos(1, SelectedItem.X, MC_JogSpeed, MC_JogAcl, MC_JogDcl); //X

            CAXM.AxmMovePos(2, SelectedItem.Z, MC_JogSpeed * 0.1, MC_JogAcl, MC_JogDcl); //Z 축 부터
        }
        #endregion

        private void EmergencyStop()
        {
            CAXM.AxmMoveSStop(0);
            CAXM.AxmMoveSStop(1);
            CAXM.AxmMoveSStop(2);
        }

        private void CmdHomeReturn()
        {
            CAXM.AxmMovePos(2, 0, MC_JogSpeed * 0.5, MC_JogAcl, MC_JogDcl);


            CAXM.AxmHomeSetStart(2); // Z
            CAXM.AxmHomeSetStart(0); // Y
            CAXM.AxmHomeSetStart(1); // X
            if (zPosStateValue != 1 || yPosStateValue != 1 || xPosStateValue != 1)
            {
                VMdispatcherTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                VMdispatcherTimer.Tick += Pos_Timer_Tick;
                VMdispatcherTimer.Start();
            }
        }

        public void Pos_Timer_Tick(object sender, EventArgs e)
        {
            zPosStateValue = AxinStateControll(2);// PositionValueZ
            yPosStateValue = AxinStateControll(0);// PositionValueY
            xPosStateValue = AxinStateControll(1);// PositionValueX

            Console.WriteLine("zPosStateValue : " + zPosStateValue);
            if (zPosStateValue == 1 && PositionValueY < 100 && PositionValueX < 100)
            {
                VMdispatcherTimer.Stop();
                BuzzerZ = Brushes.Gray;
                BuzzerY = Brushes.Gray;
                BuzzerX = Brushes.Gray;
                zPosStateValue = 9;
                yPosStateValue = 9;
                xPosStateValue = 9;
            }
        }

        public uint AxinStateControll(int MotionIndexNum)
        {

            uint posStateValue = 9;
            double value = 0;
            CAXM.AxmStatusGetCmdPos(MotionIndexNum, ref value);
            CAXM.AxmHomeGetResult(MotionIndexNum, ref posStateValue);

            switch (MotionIndexNum)
            {
                case 0:
                    BuzzerY = Brushes.Green;
                    PositionValueY = value;
                    break;
                case 1:
                    BuzzerX = Brushes.Green;
                    PositionValueX = value;
                    break;
                case 2:
                    BuzzerZ = Brushes.Green;
                    PositionValueZ = value;
                    break;
            }


            return posStateValue;
        }

        private DispatcherTimer VMdispatcherTimer;
        public uint xPosStateValue = 9;
        public uint yPosStateValue = 9;
        public uint zPosStateValue = 9;


        public double AxinMoveControll(int MotionIndexNum, double jogSpeed)
        {
            switch (MotionIndexNum)
            {
                case 0:
                    BuzzerY = Brushes.Green;
                    break;
                case 1:
                    BuzzerX = Brushes.Green;
                    break;
                case 2:
                    BuzzerZ = Brushes.Green;
                    break;

                default:
                    Console.WriteLine("미존재 예외처리");
                    break;

            }
            double Dp = 0;
            CAXM.AxmMoveVel(MotionIndexNum, jogSpeed, MC_JogAcl, MC_JogDcl);
            CAXM.AxmStatusGetCmdPos(MotionIndexNum, ref Dp);
            return Dp;
        }
    }
}
