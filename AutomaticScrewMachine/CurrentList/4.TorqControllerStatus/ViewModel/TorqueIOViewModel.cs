using AutomaticScrewMachine.Bases;
using AutomaticScrewMachine.CurrentList._0.ParentModel;
using AutomaticScrewMachine.CurrentList._4.TorqControllerStatus.Model;
using AutomaticScrewMachine.Utiles;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace AutomaticScrewMachine.CurrentList._4.TorqControllerStatus.ViewModel {
    public class TorqueIOViewModel : TorqueModel {
        

        public TorqueIOViewModel() {
            SerialPortAdapter.ConnectedSerial();
            BtnReadParameter = new RelayCommand(() => SnedTest("P",ReadParameterCommand));
            BtnUpdateParameter = new RelayCommand(() => SnedTest("S", UpdateParameterCommand));
        }

        private void SnedTest (string cmd, string sendString) {
            //SerialPortAdapter.SendData("S",sendString+"0030");
            SerialPortAdapter.SendData(cmd, sendString);
            Delay(100);
            if (cmd.Equals("P")) {
                ReadParamResult = SerialPortAdapter.ReadData;
                ReadUpdateResult = "";
            } else {
                ReadParamResult = "";
                ReadUpdateResult = SerialPortAdapter.ReadData;
            }
            
        }


    }
}
