using AutomaticScrewMachine.Utiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace AutomaticScrewMachine.CurrentList._1.Jog.Model {
    public class JogData : ViewModelBase {
        public enum DIOIndex {
            STARTBTN = 0,
            RESETBTN = 1,
            EMGBTN = 2,
            START2BTN = 3,

            IDK4 = 4,
            IDK5 = 5,
            IDK6 = 6,

            ERRORMODEL = 7,

            DRIVER = 8,
            DEPTH = 9,
            VACUUM = 10,

            OK_LED_PORT1 = 11,
            OK_LED_PORT2 = 12,
            OK_LED_PORT3 = 13,
            OK_LED_PORT4 = 14,
            OK_LED_PORT5 = 15,

            NG_LED_PORT1 = 16,
            NG_LED_PORT2 = 17,
            NG_LED_PORT3 = 18,
            NG_LED_PORT4 = 19,
            NG_LED_PORT5 = 20,

            LED_BUZZER_RED = 21,
            LED_BUZZER_YELLOW = 22,
            LED_BUZZER_GREEN = 23,
            SOUND_BUZZER = 24,


            IDK25 = 25,
            IDK26 = 26,
            IDK27 = 27,
            IDK28 = 28,
            IDK29 = 29,
            IDK30 = 30,
            IDK31 = 31
        }
        public enum ServoIndex {
            YPOSITION = 0,
            XPOSITION = 1,
            ZPOSITION = 2
        }
        public static readonly string isFolderName = "Data";
        public static readonly string isFileName = "JogData.xlsx";

        private SequenceData _selectedSequenceItem;
        public SequenceData SelectedSequenceItem {
            get { return _selectedSequenceItem; }
            set {
                if (_selectedSequenceItem != value) {
                    _selectedSequenceItem = value;
                    RaisePropertyChanged(nameof(_selectedSequenceItem));

                    // 선택된 항목에 대한 처리 수행
                    if (_selectedSequenceItem != null) {
                        SelectedSequenceIndex = SequenceDataList.IndexOf(_selectedSequenceItem);
                        GetReadingData(SelectedSequenceIndex);
                    } else {
                        PositionDataList.Clear();
                    }
                }
            }
        }

        public void GetReadingData (int workSheetIndex) {
            PositionDataList?.Clear();
            ObservableCollection<List<string>> GetJogDataList;

            GetJogDataList = ExcelAdapter.GetReadData(isFolderName, isFileName, workSheetIndex);
            if (GetJogDataList != null) {
                for (int j = 1; j < GetJogDataList.Count; j++) // j = 0 CategoryList 그래서 1부터 시작
                {
                    PosData posData = new PosData {
                        Name = GetJogDataList[j][0].ToString(),
                        X = double.Parse(GetJogDataList[j][1]),
                        Y = double.Parse(GetJogDataList[j][2]),
                        Z = double.Parse(GetJogDataList[j][3]),
                        Driver_IO = uint.Parse(GetJogDataList[j][4]),
                        Depth_IO = uint.Parse(GetJogDataList[j][5]),
                        ChangePositionDataBtn = new RelayCommand(ChangePosTrigger)
                    };

                    PositionDataList.Add(posData);
                }
            }
        }

        private void ChangePosTrigger()
        {
            UpdateSelectedPosData(SelectedPositionIndex);
        }

        private PosData _selectedPositionItem;
        public PosData SelectedPositionItem {
            get { return _selectedPositionItem; }
            set {
                if (_selectedPositionItem != value) {
                    _selectedPositionItem = value;
                    RaisePropertyChanged(nameof(SelectedPositionItem));

                    // 선택된 항목에 대한 처리 수행
                    if (_selectedPositionItem != null) {
                        SelectedPositionIndex = PositionDataList.IndexOf(_selectedPositionItem);
                        //UpdateSelectedPosData(SelectedPositionIndex);
                    } 
                }
            }
        }
        
        private void UpdateSelectedPosData (int index) {

            PositionDataList[index].X = PositionValueX;
            PositionDataList[index].Y = PositionValueY;
            PositionDataList[index].Z = PositionValueZ;
            PositionDataList[index].Driver_IO = DriverBuzzerSignal;
            PositionDataList[index].Depth_IO = DepthBuzzerSignal;
        }

        private Thickness _DriverPosList;
        public Thickness DriverPosList {
            get => _DriverPosList;
            set {
                if (_DriverPosList != value) {
                    _DriverPosList = value;
                    RaisePropertyChanged(nameof(DriverPosList));
                }
            }
        }
        public bool ControlRock { get; set; }
        public ICommand AddPosition { get; set; }
        public ICommand RemoveSequenceCommand { get; set; }
        public ICommand RemovePositionCommand { get; set; }
        public ICommand SequenceStart { get; set; }
        public ICommand HomeCommand { get; set; }
        public ICommand EmergencyStopCommand { get; set; }

        public ICommand DriverIO { get; set; }
        public ICommand DepthIO { get; set; }
        public ICommand VacuumIO { get; set; }


        public ICommand ReadRecipe { get; set; }
        public ICommand SavePosDataRecipe { get; set; }
        public ICommand AddRecipe { get; set; }


        public ICommand JogSpeedUp => new RelayCommand(() => JogMoveSpeed += 0.5);
        public ICommand JogSpeedDown => new RelayCommand(SpeedDown);
        private void SpeedDown () {
            JogMoveSpeed = Math.Max(0.101, JogMoveSpeed - (JogMoveSpeed < 1.1 ? 0.1 : 0.5));
        }

        public ICommand ServoCheckX { get; set; }
        public ICommand ServoCheckY { get; set; }
        public ICommand ServoCheckZ { get; set; }

        private ObservableCollection<PosData> _posDataList = new ObservableCollection<PosData>();
        public ObservableCollection<PosData> PositionDataList {
            get { return _posDataList; }
            set {
                _posDataList = value;
                RaisePropertyChanged(nameof(PositionDataList));
            }
        }
        private ObservableCollection<SequenceData> _sequenceDataList = new ObservableCollection<SequenceData>();
        public ObservableCollection<SequenceData> SequenceDataList {
            get { return _sequenceDataList; }
            set {
                _sequenceDataList = value;
                RaisePropertyChanged(nameof(SequenceDataList));
            }
        }
        private double _jogMoveSpeed = 1;
        public double JogMoveSpeed {
            get { 
                return _jogMoveSpeed; 
            }
            set {
                _jogMoveSpeed = value;
                RaisePropertyChanged(nameof(JogMoveSpeed));
                MC_JogSpeed = _jogMoveSpeed * 100000;
                MC_JogAcl = MC_JogSpeed * 10;
                MC_JogDcl = MC_JogSpeed * 10;
            }
        }
        private int _selectedPositionIndex;
        public int SelectedPositionIndex
        {
            get { return _selectedPositionIndex; }
            set
            {
                if (_selectedPositionIndex != value)
                {
                    _selectedPositionIndex = value;
                    RaisePropertyChanged(nameof(SelectedPositionIndex));
                }
            }
        }
        private int _selectedSequenceIndex;
        public int SelectedSequenceIndex {
            get { return _selectedSequenceIndex; }
            set
            {
                if (_selectedSequenceIndex != value)
                {
                    _selectedSequenceIndex = value;
                    RaisePropertyChanged(nameof(SelectedSequenceIndex));
                }
            }
        }
        public double MC_JogSpeed { get; set; }
        public double MC_JogAcl { get; set; }
        public double MC_JogDcl { get; set; }

        private int _btnSize = 35;
        public int BtnSize {
            get { return _btnSize; }
            set {
                _btnSize = value;
                RaisePropertyChanged(nameof(BtnSize));
            }
        }

        private double _screwMCForcus = 0.0;
        public double ScrewMCForcus {
            get { return _screwMCForcus; }
            set {
                _screwMCForcus = value * 0.00001 * 2;
                RaisePropertyChanged(nameof(ScrewMCForcus));
            }
        }
        private string _titleName = "";
        public string TitleName {
            get { return _titleName; }
            set {
                _titleName = "#" + value;
                RaisePropertyChanged(nameof(TitleName));
            }
        }
        private double _positionValueX;
        public double PositionValueX {
            get { return _positionValueX; }
            set {
                _positionValueX = value;
                RaisePropertyChanged(nameof(PositionValueX));
            }
        }

        private Brush _buzzerX = Brushes.Gray;
        public Brush BuzzerX {
            get { return _buzzerX; }
            set {
                _buzzerX = value;
                RaisePropertyChanged(nameof(BuzzerX));
            }
        }


        private double _positionValueY = 0;
        public double PositionValueY {
            get { return _positionValueY; }
            set {
                _positionValueY = value;
                RaisePropertyChanged(nameof(PositionValueY));
            }
        }
        public double[] SequenceReadyPosition = new double[2] { 167119, 117239 };
        private Brush _buzzerY = Brushes.Gray;
        public Brush BuzzerY {
            get { return _buzzerY; }
            set {
                _buzzerY = value;
                RaisePropertyChanged(nameof(BuzzerY));
            }
        }

        private double _positionValueZ = 0;
        public double PositionValueZ {
            get { return _positionValueZ; }
            set {
                _positionValueZ = value;
                RaisePropertyChanged(nameof(PositionValueZ));
            }
        }

        private Brush _buzzerZ = Brushes.Gray;
        public Brush BuzzerZ {
            get { return _buzzerZ; }
            set {
                _buzzerZ = value;
                RaisePropertyChanged(nameof(BuzzerZ));
            }
        }

        private Brush _servoX = Brushes.Gray;
        public Brush ServoX {
            get { return _servoX; }
            set {
                _servoX = value;
                RaisePropertyChanged(nameof(ServoX));
            }
        }
        private Brush _servoY = Brushes.Gray;
        public Brush ServoY {
            get { return _servoY; }
            set {
                _servoY = value;
                RaisePropertyChanged(nameof(ServoY));
            }
        }
        private Brush _servoZ = Brushes.Gray;
        public Brush ServoZ {
            get { return _servoZ; }
            set {
                _servoZ = value;
                RaisePropertyChanged(nameof(ServoZ));
            }
        }

        private Brush _driverBuzzer = Brushes.Gray;
        public Brush DriverBuzzer {
            get { return _driverBuzzer; }
            set {
                _driverBuzzer = value;
                RaisePropertyChanged(nameof(DriverBuzzer));
            }
        }

        private Brush _depthBuzzer = Brushes.Gray;
        public Brush DepthBuzzer {
            get { return _depthBuzzer; }
            set {
                _depthBuzzer = value;
                RaisePropertyChanged(nameof(DepthBuzzer));
            }
        }

        private Brush _vacuumBuzzer = Brushes.Gray;
        public Brush VacuumBuzzer {
            get { return _vacuumBuzzer; }
            set {
                _vacuumBuzzer = value;
                RaisePropertyChanged(nameof(VacuumBuzzer));
            }
        }
        public DateTime Delay (int MS) {

            DateTime thisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime afterMoment = thisMoment.Add(duration);

            while (afterMoment >= thisMoment) {
                Application.Current?.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                thisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }




        public void FrameDelay (int waitMS) {
            Stopwatch timer = new Stopwatch();
            timer.Start ();
            do {
                DoEvents();
            } while (timer.ElapsedMilliseconds < waitMS);
        }

        private void DoEvents () {
            DispatcherFrame f = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action<object>((arg) => {
                    DispatcherFrame fr = arg as DispatcherFrame;
                    fr.Continue = false;
                }), f);
            Dispatcher.PushFrame(f);
        }

        public Brush RecevieSignalColor (uint signalCode) {
            Brush ReturnBrush;
            if (signalCode == 1) {
                ReturnBrush = Brushes.Green;
            } else {
                ReturnBrush = Brushes.Gray;
            }

            return ReturnBrush;
        }
        public double MoveAbleZPosition = 10000;
        public uint valueX = 9;
        public uint valueY = 9;
        public uint valueZ = 9;
        public bool ServoSignal = false;

        // ControllCheckList
        public DispatcherTimer _HomeReturnDipatcher;

        public uint DriverBuzzerSignal = 9;
        public uint DepthBuzzerSignal = 9;
        public uint VacuumBuzzerSignal = 9;



    }
}
