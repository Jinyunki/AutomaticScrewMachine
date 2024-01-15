using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace AutomaticScrewMachine.CurrentList._3.IO.Model {
    public class ControlSetOutput : ViewModelBase{
        private Brush _buttonBackground ; 
        public Brush ButtonBackground {
            get { return _buttonBackground; }
            set {
                _buttonBackground = value;
                RaisePropertyChanged(nameof(ButtonBackground));
            }
        }
        private ICommand _buttonCommand;
        public ICommand ButtonCommand {
            get { return _buttonCommand; }
            set {
                _buttonCommand = value;
                RaisePropertyChanged(nameof(ButtonCommand));
            }
        }
        public string ButtonText { get; set; }
    }
}
