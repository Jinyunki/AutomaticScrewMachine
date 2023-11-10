using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutomaticScrewMachine.View
{
    /// <summary>
    /// JogController.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class JogController : UserControl
    {
        private Border clickedBorder;
        public JogController()
        {
            InitializeComponent();
        }

        private void JogStop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                clickedBorder = sender as Border;
                if (clickedBorder != null)
                {
                    StaticControllerSignal.ControllerSignalView(clickedBorder.Name,buzzerPositionX,buzzerPositionY,buzzerPositionZ);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }
        }

        private void JogStop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                StaticControllerSignal.StopControllerSignalView();
                StaticControllerSignal.BuzzerOff(clickedBorder.Name, buzzerPositionX, buzzerPositionY, buzzerPositionZ);
                
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

    }
}
