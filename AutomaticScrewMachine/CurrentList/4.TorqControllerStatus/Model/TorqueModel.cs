using AutomaticScrewMachine.CurrentList._0.ParentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutomaticScrewMachine.CurrentList._4.TorqControllerStatus.Model {
    public class TorqueModel : ParentsData {
        public ICommand BtnReadParameter { get; set; }
        public ICommand BtnUpdateParameter { get; set; }
        private string _parameterCommand;
        public string ReadParameterCommand {
            get { return _parameterCommand; }
            set {
                _parameterCommand = value;
                RaisePropertyChanged(nameof(ReadParameterCommand));
            }
        }
        private string _updateParameterCommand;
        public string UpdateParameterCommand {
            get { return _updateParameterCommand; }
            set {
                _updateParameterCommand = value;
                RaisePropertyChanged(nameof(UpdateParameterCommand));
            }
        }

        private string _readParamResult;
        public string ReadParamResult {
            get { return _readParamResult; }
            set {
                _readParamResult = value;
                RaisePropertyChanged(nameof(ReadParamResult));
            }
        }

        private string _readUpdateResult;
        public string ReadUpdateResult {
            get { return _readUpdateResult; }
            set {
                _readUpdateResult = value;
                RaisePropertyChanged(nameof(ReadUpdateResult));
            }
        }
    }
}
