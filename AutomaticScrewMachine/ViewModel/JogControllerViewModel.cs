using AutomaticScrewMachine.Model;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

namespace AutomaticScrewMachine.ViewModel
{
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


        private int _index;
        public int Index
        {
            get { return _index; }
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged(nameof(Index));
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

    public class JogControllerViewModel : JogController
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
                        int index = _selectedItem.Index;
                        Console.WriteLine("선택된 index : " + index);
                        Console.WriteLine("선택된 index : " + SelectedItem.Name);
                        // index를 사용하여 필요한 처리 수행
                    }
                }
            }
        }
        public ICommand RemoveSelectedCommand { get; }


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

        public JogControllerViewModel()
        {
            Messenger.Default.Register<SignalMessage>(this, HandleSignalMessage);
            AddPosition = new RelayCommand(AddPos);
            RemoveSelectedCommand = new RelayCommand(RemoveSelected);

        }

        private void RemoveSelected()
        {
            Console.WriteLine("Name : " + SelectedItem.Name);
            Console.WriteLine("Index : " + SelectedItem.Index);
            Console.WriteLine("X : " + SelectedItem.X);
            Console.WriteLine("Y : " + SelectedItem.Y);
            Console.WriteLine("Z : " + SelectedItem.Z);
            //if (SelectedItem != null)
            //{
            //    JogDataList.Remove(SelectedItem);
            //}
        }

        private void AddPos()
        {
            JogData jogData = new JogData()
            {
                Name = TitleName,
                X = PositionValueX,
                Y = PositionValueY,
                Z = PositionValueZ,
                Index = JogDataList.Count
            };
            JogDataList.Add(jogData);
            Console.WriteLine("totalCount : " + JogDataList.Count);
        }

        private void HandleSignalMessage(SignalMessage message)
        {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                var isPress = message.IsPress;
                var isViewName = message.IsViewName;
                if (isPress)
                {
                    switch (isViewName)
                    {
                        // x
                        case string n when n == StaticControllerSignal.JOG_RIGHT:
                            PositionValueX += 1;
                            break;
                        case string n when n == StaticControllerSignal.JOG_LEFT:
                            PositionValueX -= 1;
                            break;

                        // y    
                        case string n when n == StaticControllerSignal.JOG_STRAIGHT:
                            PositionValueY += 1;
                            break;
                        case string n when n == StaticControllerSignal.JOG_BACK:
                            PositionValueY -= 1;
                            break;

                        // z    
                        case string n when n == StaticControllerSignal.JOG_UP:
                            PositionValueZ += 1;
                            break;
                        case string n when n == StaticControllerSignal.JOG_DOWN:
                            PositionValueZ -= 1;
                            break;



                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }
    }
}
