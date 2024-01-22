using AutomaticScrewMachine.Utiles;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutomaticScrewMachine.CurrentList._4.TorqControllerStatus.ViewModel {
    public class TorqueIOViewModel :ViewModelBase{
        public ICommand SendTorq { get; set; }
        private string _parameterCommand;
        public string ParameterCommand {
            get { return _parameterCommand; }
            set {
                _parameterCommand = value;
                RaisePropertyChanged(nameof(ParameterCommand));
            }
        }
        
        private string _commandResult;
        public string CommandResult {
            get { return _commandResult; }
            set {
                _commandResult = value;
                RaisePropertyChanged(nameof(CommandResult));
            }
        }

        public TorqueIOViewModel() {
            SerialPortAdapter.ConnectedSerial();
            SendTorq = new RelayCommand(() => SerialPortAdapter.SendData(ParameterCommand));
            Console.WriteLine("TEST" + SerialPortAdapter.ReadData);
        }

    }
}
