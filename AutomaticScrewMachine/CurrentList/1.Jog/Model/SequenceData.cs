using System.ComponentModel;
using System.Windows.Input;

namespace AutomaticScrewMachine.CurrentList._1.Jog.Model {
    public class SequenceData : INotifyPropertyChanged {
        private string _name;
        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public ICommand SequenceListStart { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged (string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
