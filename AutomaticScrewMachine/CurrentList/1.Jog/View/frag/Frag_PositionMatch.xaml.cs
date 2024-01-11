using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutomaticScrewMachine.CurrentList._1.Jog.View.frag {
    /// <summary>
    /// Frag_PositionMatch.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Frag_PositionMatch : UserControl {
        public Frag_PositionMatch () {
            InitializeComponent();
        }
        private void UserControl_MouseDown (object sender, MouseButtonEventArgs e) {


            Point ClickPos = e.GetPosition((IInputElement)sender);

            double ClickX = (double)ClickPos.X;
            double ClickY = (double)ClickPos.Y;

            Console.WriteLine("MouseDown 위치X : " + ClickX);
            Console.WriteLine("MouseDown 위치Y : " + ClickY);
            Console.WriteLine();

            double Xtt = 322901 / 500;
            double Ytt = 335300 / 550;
            double ServoXInputX = ClickX * Xtt;
            double ServoXInputY = ClickY * Ytt;
            Console.WriteLine("ServoXInputX : " + ServoXInputX);
            Console.WriteLine("ServoXInputY : " + ServoXInputY);

            // 하기 차후 정밀도 수정 필요

            double[] XYPOS = new double[2] { ServoXInputX, ServoXInputY };
            int[] JogCtrList = { 1, 0 };

            double jogSpeed = 100000;
            double[] speedList = { jogSpeed, jogSpeed };
            double[] aclList = { jogSpeed * 10, jogSpeed * 10 };
            double[] dclList = { jogSpeed * 10, jogSpeed * 10 };

            CAXM.AxmMoveStartMultiPos(2, JogCtrList, XYPOS, speedList, aclList, dclList);
        }
    }
}
