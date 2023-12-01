using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                if (clickedBorder != null) {
                    StaticControllerSignal.StopControllerSignalView();
                }
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }
    }
}
