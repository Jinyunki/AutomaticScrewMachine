using AutomaticScrewMachine.CurrentList._1.Jog.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AutomaticScrewMachine.CurrentList._1.Jog.ViewModel {
    public class JogSystem : JogBase {
        public override bool CheckServoIO (int servoIndex) {
            uint servoState = 9; // 0 = on, 1 = off
            CAXM.AxmSignalIsServoOn(servoIndex, ref servoState);

            if (servoState == 0) {
                return true;
            } else {
                return false;
            }
        }

        public override uint ServoData (int servoIndex) {
            throw new NotImplementedException();
        }

    }
}
