using AutomaticScrewMachine.Bases;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutomaticScrewMachine.CurrentList._1.Jog.View {
    /// <summary>
    /// JogMain.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class JogMain : UserControl {
        private Border clickedBorder;
        public JogMain () {
            InitializeComponent();
        }
        private void JogStop_MouseUp (object sender, MouseButtonEventArgs e) {
            if (clickedBorder != null) {
                StaticControllerSignal.StopControllerSignalView();
            }
        }

        private void Grid_MouseDown (object sender, MouseButtonEventArgs e) {

            Point ClickPos = e.GetPosition((IInputElement)sender);

            double ClickX = (double)ClickPos.X;
            double ClickY = (double)ClickPos.Y;

            Console.WriteLine("MouseDown 위치 : " + ClickX + " " + ClickY);

            // 하기 차후 정밀도 수정 필요
/*
            double ReciveClickPosX = (ClickX - 100) * 530.5365;
            double ReciveClickPosY = ClickY * 488.4958;
            double[] XYPOS = new double[2] {ReciveClickPosX, ReciveClickPosY};
            int[] JogCtrList = { 1, 0 };

            double jogSpeed = 100000;
            double[] speedList = { jogSpeed, jogSpeed };
            double[] aclList = { jogSpeed * 10, jogSpeed * 10 };
            double[] dclList = { jogSpeed * 10, jogSpeed * 10 };

            CAXM.AxmMoveStartMultiPos(2, JogCtrList, XYPOS, speedList, aclList,dclList);*/
        }
    }
}
