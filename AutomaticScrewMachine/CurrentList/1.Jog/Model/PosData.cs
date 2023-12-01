using System.ComponentModel;

namespace AutomaticScrewMachine.CurrentList._1.Jog.Model {
    public class PosData : INotifyPropertyChanged {

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

        private double _x;
        public double X {
            get { return _x; }
            set {
                if (_x != value) {
                    _x = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        private double _y;
        public double Y {
            get { return _y; }
            set {
                if (_y != value) {
                    _y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        private double _z;
        public double Z {
            get { return _z; }
            set {
                if (_z != value) {
                    _z = value;
                    OnPropertyChanged(nameof(Z));
                }
            }
        }

        private uint _torq_IO;
        public uint Torq_IO {
            get { return _torq_IO; }
            set {
                if (_torq_IO != value) {
                    _torq_IO = value;
                    OnPropertyChanged(nameof(Torq_IO));
                }
            }
        }


        private uint _depth_IO;
        public uint Depth_IO {
            get { return _depth_IO; }
            set {
                if (_depth_IO != value) {
                    _depth_IO = value;
                    OnPropertyChanged(nameof(Depth_IO));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged (string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
