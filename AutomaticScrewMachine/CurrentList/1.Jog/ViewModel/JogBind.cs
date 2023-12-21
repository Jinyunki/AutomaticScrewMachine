using AutomaticScrewMachine.Bases;
using AutomaticScrewMachine.CurrentList._1.Jog.Base;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutomaticScrewMachine.CurrentList._1.Jog.ViewModel {
    public class JogBind : JogBase {
        BackgroundWorker worker = new BackgroundWorker();
        public JogBind () {
            JogStickControl_IO();
        }

        public override void JogStickControl_IO () {
           Messenger.Default.Register<SignalMessage>(this, HandleSignalMessage);
            
        }

        private void HandleSignalMessage (SignalMessage message) {
            var clickViewName = message.IsViewName;
            var isPress = message.IsPress;
            switch (clickViewName) {
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
            }

            if (!isPress) {
                CAXM.AxmMoveSStop(0);
                CAXM.AxmMoveSStop(1);
                CAXM.AxmMoveSStop(2);
            }
        }
    }
}

