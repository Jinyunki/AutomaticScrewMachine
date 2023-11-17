using AutomaticScrewMachine.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AutomaticScrewMachine.Model
{
    public class JogController : ViewModelBase
    {
        private JogData _selectedItem;
        public JogData SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    RaisePropertyChanged(nameof(SelectedItem));

                    // 선택된 항목에 대한 처리 수행
                    if (_selectedItem != null)
                    {
                        int index = JogDataList.IndexOf(_selectedItem);

                        Console.WriteLine("선택된 index : " + index);
                        Console.WriteLine("Name : " + _selectedItem.Name);
                        Console.WriteLine("X : " + _selectedItem.X);
                        Console.WriteLine("Y : " + _selectedItem.Y);
                        Console.WriteLine("Z : " + _selectedItem.Z);

                    }
                }
            }
        }


        public ICommand AddPosition { get; set; }
        public ICommand RemoveSelectedCommand { get; set; }
        public ICommand CheckSelectedCommand { get; set; }
        public ICommand HomeCommand { get; set; }
        public ICommand EmergencyStopCommand { get; set; }

        public ICommand TorqIO { get; set; }
        public ICommand DepthIO { get; set; }
        public ICommand AirIO { get; set; }


        public ICommand JogSpeedUp => new RelayCommand(() => JogMoveSpeed += 0.5);
        public ICommand JogSpeedDown => new RelayCommand(SpeedDown);
        private void SpeedDown()
        {
            JogMoveSpeed = Math.Max(0.101, JogMoveSpeed - (JogMoveSpeed < 1.1 ? 0.1 : 0.5));
        }

        public ICommand ServoCheckX { get; set; }
        public ICommand ServoCheckY { get; set; }
        public ICommand ServoCheckZ { get; set; }

        private ObservableCollection<JogData> _jogDataList = new ObservableCollection<JogData>();
        public ObservableCollection<JogData> JogDataList
        {
            get { return _jogDataList; }
            set
            {
                _jogDataList = value;
                RaisePropertyChanged(nameof(JogDataList));
            }
        }
        private double _jogMoveSpeed = 1;
        public double JogMoveSpeed
        {
            get { return _jogMoveSpeed;}
            set
            {
                _jogMoveSpeed = value;
                RaisePropertyChanged(nameof(JogMoveSpeed));
                MC_JogSpeed = _jogMoveSpeed * 100000;
                MC_JogAcl = MC_JogSpeed * 10;
                MC_JogDcl = MC_JogSpeed * 10;
            }
        }

        public double MC_JogSpeed {  get; set; }
        public double MC_JogAcl {  get; set; }
        public double MC_JogDcl {  get; set; }

        private int _btnSize = 55;
        public int BtnSize
        {
            get { return _btnSize; }
            set
            {
                _btnSize = value;
                RaisePropertyChanged(nameof(BtnSize));
            }
        }
        private string _titleName = "Seq Name";
        public string TitleName
        {
            get { return _titleName; }
            set
            {
                _titleName = value;
                RaisePropertyChanged(nameof(TitleName));
            }
        }
        private double _positionValueX ;
        public double PositionValueX
        {
            get { return _positionValueX; }
            set
            {
                _positionValueX = value;
                RaisePropertyChanged(nameof(PositionValueX));
            }
        }

        private Brush _buzzerX = Brushes.Gray;
        public Brush BuzzerX
        {
            get { return _buzzerX;}
            set
            {
                _buzzerX = value;
                RaisePropertyChanged(nameof(BuzzerX));
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

        private Brush _buzzerY = Brushes.Gray;
        public Brush BuzzerY
        {
            get { return _buzzerY; }
            set
            {
                _buzzerY = value;
                RaisePropertyChanged(nameof(BuzzerY));
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

        private Brush _buzzerZ = Brushes.Gray;
        public Brush BuzzerZ
        {
            get { return _buzzerZ; }
            set
            {
                _buzzerZ = value;
                RaisePropertyChanged(nameof(BuzzerZ));
            }
        }

        private Brush _servoX = Brushes.Gray;
        public Brush ServoX
        {
            get { return _servoX; }
            set
            {
                _servoX = value;
                RaisePropertyChanged(nameof(ServoX));
            }
        }
        private Brush _servoY = Brushes.Gray;
        public Brush ServoY
        {
            get { return _servoY; }
            set
            {
                _servoY = value;
                RaisePropertyChanged(nameof(ServoY));
            }
        }
        private Brush _servoZ = Brushes.Gray;
        public Brush ServoZ
        {
            get { return _servoZ; }
            set
            {
                _servoZ = value;
                RaisePropertyChanged(nameof(ServoZ));
            }
        }

        private Brush _torqSig = Brushes.Gray;
        public Brush TorqSig
        {
            get { return _torqSig; }
            set
            {
                _torqSig = value;
                RaisePropertyChanged(nameof(TorqSig));
            }
        }

        private Brush _depthSig = Brushes.Gray;
        public Brush DepthSig
        {
            get { return _depthSig; }
            set
            {
                _depthSig = value;
                RaisePropertyChanged(nameof(DepthSig));
            }
        }

        private Brush _airSig = Brushes.Gray;
        public Brush AirSig
        {
            get { return _airSig; }
            set
            {
                _airSig = value;
                RaisePropertyChanged(nameof(AirSig));
            }
        }

        public Brush RecevieSignalColor(uint signalCode)
        {
            Brush ReturnBrush;
            if (signalCode == 1)
            {
                ReturnBrush = Brushes.Green;
            }
            else
            {
                ReturnBrush = Brushes.Gray;
            }

            return ReturnBrush;
        }

        // ServoCheckList
        public DispatcherTimer Motion_IO_Dispatcher;
        public uint valueX = 9;
        public uint valueY = 9;
        public uint valueZ = 9;

        // ControllCheckList
        public DispatcherTimer _positionDipatcher;
        public uint xPosStateValue = 9;
        public uint yPosStateValue = 9;
        public uint zPosStateValue = 9;


        public uint torqS = 9;
        public uint depthS = 9;
        public uint airS = 9;

    }








    #region ListBox용 Jog클래스
    public class JogData : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private double _x;
        public double X
        {
            get { return _x; }
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        private double _y;
        public double Y
        {
            get { return _y; }
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        private double _z;
        public double Z
        {
            get { return _z; }
            set
            {
                if (_z != value)
                {
                    _z = value;
                    OnPropertyChanged(nameof(Z));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion
}
