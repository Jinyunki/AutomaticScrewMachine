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
using System.Windows.Data;
using AutomaticScrewMachine.Bases;
using System.Net.NetworkInformation;

namespace AutomaticScrewMachine.CurrentList._1.Jog.Model {
    public class JogData : ViewModelBase {
        public Dictionary<int, Brush> _callBackBrush = new Dictionary<int, Brush>();

        public enum DIOIndex {
            STARTBTN = 0,
            RESETBTN = 1,
            EMGBTN = 2,
            START2BTN = 3,

            TORQUE_DRIVER = 4, // Torqu 시작 P25 (4)
            IDK5 = 5, // Preset선택 P25 (1)
            IDK6 = 6, // Preset선택 P25 (2)

            NGBOX = 7,

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


            IDK25 = 25, // NULL 비어있는 Index
            IDK26 = 26, // NULL 비어있는 Index
            IDK27 = 27, // NULL 비어있는 Index
            IDK28 = 28, // NULL 비어있는 Index
            IDK29 = 29, // NULL 비어있는 Index
            IDK30 = 30, // NULL 비어있는 Index
            IDK31 = 31  // NULL 비어있는 Index
        }
        public enum ServoIndex {
            YPOSITION = 0,
            XPOSITION = 1,
            ZPOSITION = 2
        }
        public static readonly string isFolderName = "Data";
        public static readonly string isFileName = "JogData.xlsx";

        public const uint SignalON = 1u;
        public const uint SignalOFF = 0;

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

        //TODO : TABCOUNT 구조 변경해야함
        private int _tabCnt = 1;
        public int TabCnt {
            get { return _tabCnt; }
            set {
                _tabCnt = value;
                RaisePropertyChanged(nameof(TabCnt));
            }
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
        public ICommand JogSpeedDown => new RelayCommand(() => JogMoveSpeed = Math.Max(0.101, JogMoveSpeed - (JogMoveSpeed < 1.1 ? 0.1 : 0.5)));
        public ICommand SetMovePosition {get; set;}

        public ICommand ServoCheckX { get; set; }
        public ICommand ServoCheckY { get; set; }
        public ICommand ServoCheckZ { get; set; } 
        public ICommand NGBoxCommand { get; set; } 
        
        public ICommand MovePosition1 { get; set; }
        public ICommand MovePosition2 { get; set; }
        public ICommand MovePosition3 { get; set; }
        public ICommand MovePosition4 { get; set; }
        public ICommand MovePosition5 { get; set; }
        public ICommand MovePositionSupply { get; set; }

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
        private double _inputPositionValueX ;
        public double InputPositionValueX {
            get { return _inputPositionValueX; }
            set {
                _inputPositionValueX = value;
                RaisePropertyChanged(nameof(InputPositionValueX));
            }
        }
        private double _inputpositionValueX;
        public double InputPositionValueY {
            get { return _inputpositionValueX; }
            set {
                _inputpositionValueX = value;
                RaisePropertyChanged(nameof(InputPositionValueY));
            }
        }
        #region PORT OK/NG Signal
        private Brush _p1_ok = Brushes.Gray;
        public Brush P1_OK {
            get { return _p1_ok; }
            set {
                _p1_ok = value;
                RaisePropertyChanged(nameof(P1_OK));
            }
        }

        
        private Brush _p2_ok = Brushes.Gray;
        public Brush P2_OK {
            get { return _p2_ok; }
            set {
                _p2_ok = value;
                RaisePropertyChanged(nameof(P2_OK));
            }
        }

        
        private Brush _p3_ok = Brushes.Gray;
        public Brush P3_OK {
            get { return _p3_ok; }
            set {
                _p3_ok = value;
                RaisePropertyChanged(nameof(P3_OK));
            }
        }

        
        private Brush _p4_ok = Brushes.Gray;
        public Brush P4_OK {
            get { return _p4_ok; }
            set {
                _p4_ok = value;
                RaisePropertyChanged(nameof(P4_OK));
            }
        }

        
        private Brush _p5_ok = Brushes.Gray;
        public Brush P5_OK {
            get { return _p5_ok; }
            set {
                _p5_ok = value;
                RaisePropertyChanged(nameof(P5_OK));
            }
        }

        private Brush _p1_ng = Brushes.Gray;
        public Brush P1_NG {
            get { return _p1_ng; }
            set {
                _p1_ng = value;
                RaisePropertyChanged(nameof(P1_NG));
            }
        }

        
        private Brush _p2_ng = Brushes.Gray;
        public Brush P2_NG {
            get { return _p2_ng; }
            set {
                _p2_ng = value;
                RaisePropertyChanged(nameof(P2_NG));
            }
        }

        
        private Brush _p3_ng = Brushes.Gray;
        public Brush P3_NG {
            get { return _p3_ng; }
            set {
                _p3_ng = value;
                RaisePropertyChanged(nameof(P3_NG));
            }
        }

        
        private Brush _p4_ng = Brushes.Gray;
        public Brush P4_NG {
            get { return _p4_ng; }
            set {
                _p4_ng = value;
                RaisePropertyChanged(nameof(P4_NG));
            }
        }

        
        private Brush _p5_ng = Brushes.Gray;
        public Brush P5_NG {
            get { return _p5_ng; }
            set {
                _p5_ng = value;
                RaisePropertyChanged(nameof(P5_NG));
            }
        }


        #endregion

        private Brush _buzzerX = Brushes.Gray;
        public Brush ServoMoveCheckX {
            get { return _buzzerX; }
            set {
                _buzzerX = value;
                RaisePropertyChanged(nameof(ServoMoveCheckX));
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
        public double StartPortXPos = 73800;
        public double StartPortYPos = 249100;
        public double GetPortInterval = 53000;
        public double GetTabInterval = 14000;
        public double Interval { get; set; }
        public double TorqReadyZposition = 45000;
        public double[] SupplyPosition = new double[2] { 167119, 117239 };
        public double[] Port1Position /*= new double[2] { 73800, 249100 }*/;
        private Brush _buzzerY = Brushes.Gray;
        public Brush ServoMoveCheckY {
            get { return _buzzerY; }
            set {
                _buzzerY = value;
                RaisePropertyChanged(nameof(ServoMoveCheckY));
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
        public Brush ServoMoveCheckZ {
            get { return _buzzerZ; }
            set {
                _buzzerZ = value;
                RaisePropertyChanged(nameof(ServoMoveCheckZ));
            }
        }

        private Brush _servoX = Brushes.Gray;
        public Brush ServoStatusX {
            get { return _servoX; }
            set {
                _servoX = value;
                RaisePropertyChanged(nameof(ServoStatusX));
            }
        }
        private Brush _servoY = Brushes.Gray;
        public Brush ServoStatusY {
            get { return _servoY; }
            set {
                _servoY = value;
                RaisePropertyChanged(nameof(ServoStatusY));
            }
        }
        private Brush _servoZ = Brushes.Gray;
        public Brush ServoStatusZ {
            get { return _servoZ; }
            set {
                _servoZ = value;
                RaisePropertyChanged(nameof(ServoStatusZ));
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
        
        private Brush _ngBOx = Brushes.Gray;
        public Brush NGBOX {
            get { return _ngBOx; }
            set {
                _ngBOx = value;
                RaisePropertyChanged(nameof(NGBOX));
            }
        }
         
        private Brush _screwSupplyOnoff = Brushes.Gray;
        public Brush ScrewSupplyOnoff {
            get { return _screwSupplyOnoff; }
            set {
                _screwSupplyOnoff = value;
                RaisePropertyChanged(nameof(ScrewSupplyOnoff));
            }
        }
         
        private Brush _screwSupplyINOUT = Brushes.Gray;
        public Brush ScrewSupplyINOUT {
            get { return _screwSupplyINOUT; }
            set {
                _screwSupplyINOUT = value;
                RaisePropertyChanged(nameof(ScrewSupplyINOUT));
            }
        }
        
        private Brush _selfStartButton = Brushes.Gray;
        public Brush SelfStartButton {
            get { return _selfStartButton; }
            set {
                _selfStartButton = value;
                RaisePropertyChanged(nameof(SelfStartButton));
            }
        }
         private Brush _selfStart2Button = Brushes.Gray;
        public Brush SelfStart2Button {
            get { return _selfStart2Button; }
            set {
                _selfStart2Button = value;
                RaisePropertyChanged(nameof(SelfStart2Button));
            }
        }
        
        private Brush _selfResetButton = Brushes.Gray;
        public Brush SelfResetButton {
            get { return _selfResetButton; }
            set {
                _selfResetButton = value;
                RaisePropertyChanged(nameof(SelfResetButton));
            }
        }
        
        
        private Brush _selfEmgButton = Brushes.Gray;
        public Brush SelfEmgButton {
            get { return _selfEmgButton; }
            set {
                _selfEmgButton = value;
                RaisePropertyChanged(nameof(SelfEmgButton));
            }
        }
        
        
        private Brush _selfPowerButton = Brushes.Gray;
        public Brush SelfPowerButton {
            get { return _selfPowerButton; }
            set {
                _selfPowerButton = value;
                RaisePropertyChanged(nameof(SelfPowerButton));
            }
        }
        
        private Brush _jigP1 = Brushes.Gray;
        public Brush JigP1 {
            get { return _jigP1; }
            set {
                _jigP1 = value;
                RaisePropertyChanged(nameof(JigP1));
            }
        }
        
        private Brush _jigP2 = Brushes.Gray;
        public Brush JigP2 {
            get { return _jigP2; }
            set {
                _jigP2 = value;
                RaisePropertyChanged(nameof(JigP2));
            }
        }
        
        private Brush _jigP3 = Brushes.Gray;
        public Brush JigP3 {
            get { return _jigP3; }
            set {
                _jigP3 = value;
                RaisePropertyChanged(nameof(JigP3));
            }
        }
        
        private Brush _jigP4 = Brushes.Gray;
        public Brush JigP4 {
            get { return _jigP4; }
            set {
                _jigP4 = value;
                RaisePropertyChanged(nameof(JigP4));
            }
        }
        
        private Brush _jigP5 = Brushes.Gray;
        public Brush JigP5 {
            get { return _jigP5; }
            set {
                _jigP5 = value;
                RaisePropertyChanged(nameof(JigP5));
            }
        }
        
        private Brush _emgLine = Brushes.Gray;
        public Brush EmgLine {
            get { return _emgLine; }
            set {
                _emgLine = value;
                RaisePropertyChanged(nameof(EmgLine));
            }
        }
         
        private Brush _buzzerAlarmOK = Brushes.Gray;
        public Brush BuzzerAlarmOK {
            get { return _buzzerAlarmOK; }
            set {
                _buzzerAlarmOK = value;
                RaisePropertyChanged(nameof(BuzzerAlarmOK));
            }
        }
        
         
        private Brush _buzzerAlarmERR = Brushes.Gray;
        public Brush BuzzerAlarmERR {
            get { return _buzzerAlarmERR; }
            set {
                _buzzerAlarmERR = value;
                RaisePropertyChanged(nameof(BuzzerAlarmERR));
            }
        }
        
         
        private Brush _buzzerAlarmNG = Brushes.Gray;
        public Brush BuzzerAlarmNG {
            get { return _buzzerAlarmNG; }
            set {
                _buzzerAlarmNG = value;
                RaisePropertyChanged(nameof(BuzzerAlarmNG));
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


        public Brush RecevieSignalColor (uint signalCode) {
            Brush ReturnBrush;
            ReturnBrush = signalCode == 1 ? Brushes.Green : Brushes.Gray;
            return ReturnBrush;
        }

    }
}
