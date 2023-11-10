using GalaSoft.MvvmLight;
using System.ComponentModel;
using System.Windows.Media;

namespace AutomaticScrewMachine.Model
{
    public class JogController : ViewModelBase
    {
        private double _positionValueX = 0;
        public double PositionValueX
        {
            get { return _positionValueX; }
            set
            {
                _positionValueX = value;
                RaisePropertyChanged(nameof(PositionValueX));
            }
        }

        private double _positionValueY = 0;
        public double PositionValueY
        {
            get { return _positionValueY; }
            set
            {
                _positionValueY = value;
                RaisePropertyChanged(nameof(PositionValueY));
            }
        }

        private double _positionValueZ = 0;
        public double PositionValueZ
        {
            get { return _positionValueZ; }
            set
            {
                _positionValueZ = value;
                RaisePropertyChanged(nameof(PositionValueZ));
            }
        }
    }
}
