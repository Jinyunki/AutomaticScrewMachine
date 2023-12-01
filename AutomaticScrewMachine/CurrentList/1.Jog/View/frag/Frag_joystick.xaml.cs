using AutomaticScrewMachine.Bases;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutomaticScrewMachine.CurrentList._1.Jog.View.frag {
    /// <summary>
    /// frag_joystick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Frag_joystick : UserControl {
        private Border clickedBorder;
        public Frag_joystick () {
            InitializeComponent();
        }
        private void JogStop_MouseDown (object sender, MouseButtonEventArgs e) {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try {
                clickedBorder = sender as Border;
                if (clickedBorder != null && e.LeftButton == MouseButtonState.Pressed) {
                    StaticControllerSignal.ControllerSignalView(clickedBorder.Name);
                }

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }
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

        private void JogControl_MouseLeave (object sender, MouseEventArgs e) {
            if (clickedBorder != null && e.LeftButton == MouseButtonState.Pressed) {
                StaticControllerSignal.StopControllerSignalView();
            }
                
        }
    }
}
