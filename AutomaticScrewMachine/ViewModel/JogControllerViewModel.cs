using AutomaticScrewMachine.Model;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Diagnostics;
using System.Reflection;

namespace AutomaticScrewMachine.ViewModel
{
    public class JogControllerViewModel : JogController
    {
        public JogControllerViewModel()
        {
            Messenger.Default.Register<SignalMessage>(this, HandleSignalMessage);
        }
        private void HandleSignalMessage(SignalMessage message)
        {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                var isPress = message.IsPress;
                var isViewName = message.IsViewName;
                if (isPress)
                {
                    switch (isViewName)
                    {
                        // x
                        case string n when n == StaticControllerSignal.JOG_RIGHT:
                            PositionValueX += 1;
                            break;
                        case string n when n == StaticControllerSignal.JOG_LEFT:
                            PositionValueX -= 1;
                            break;

                        // y    
                        case string n when n == StaticControllerSignal.JOG_STRAIGHT:
                            PositionValueY += 1;
                            break;
                        case string n when n == StaticControllerSignal.JOG_BACK:
                            PositionValueY -= 1;
                            break;

                        // z    
                        case string n when n == StaticControllerSignal.JOG_UP:
                            PositionValueZ += 1;
                            break;
                        case string n when n == StaticControllerSignal.JOG_DOWN:
                            PositionValueZ -= 1;
                            break;



                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }
    }
}
