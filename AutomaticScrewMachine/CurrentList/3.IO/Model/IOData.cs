using AutomaticScrewMachine.CurrentList._0.ParentModel;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AutomaticScrewMachine.CurrentList._3.IO.Model {
    public class IOData : ParentsData {

        public ObservableCollection<ControlSetOutput> ControlSetsOutput { get; set; }
        // 0
        private Brush _selfStartButton = Brushes.Gray;
        public Brush SelfStartButton {
            get { return _selfStartButton; }
            set {
                _selfStartButton = value;
                RaisePropertyChanged(nameof(SelfStartButton));
            }
        }
        // 3
        private Brush _selfStart2Button = Brushes.Gray;
        public Brush SelfStart2Button {
            get { return _selfStart2Button; }
            set {
                _selfStart2Button = value;
                RaisePropertyChanged(nameof(SelfStart2Button));
            }
        }

        // 1
        private Brush _selfResetButton = Brushes.Gray;
        public Brush SelfResetButton {
            get { return _selfResetButton; }
            set {
                _selfResetButton = value;
                RaisePropertyChanged(nameof(SelfResetButton));
            }
        }

        // 2
        private Brush _selfEmgButton = Brushes.Gray;
        public Brush SelfEmgButton {
            get { return _selfEmgButton; }
            set {
                _selfEmgButton = value;
                RaisePropertyChanged(nameof(SelfEmgButton));
            }
        }

        // 4
        private Brush _torquButton = Brushes.Gray;
        public Brush TorquButton {
            get { return _torquButton; }
            set {
                _torquButton = value;
                RaisePropertyChanged(nameof(TorquButton));
            }
        }
        // 5,6 Preset
        // 7
        private Brush _ngBox = Brushes.Gray;
        public Brush NGBOX {
            get { return _ngBox; }
            set {
                _ngBox = value;
                RaisePropertyChanged(nameof(NGBOX));
            }
        }
    }
}
