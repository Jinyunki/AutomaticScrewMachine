using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using System;
using System.ComponentModel;

namespace AutomaticScrewMachine.Model
{
    public class WindowStyle : ViewModelBase
    {
        private WindowState _windowState;
        public WindowState WindowState
        {
            get { return _windowState; }
            set
            {
                if (_windowState != value)
                {
                    _windowState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _minTransparent = 1;
        public int MinTransparent
        {
            get => _minTransparent;
            set
            {
                if (_minTransparent != value)
                {
                    _minTransparent = value;
                    RaisePropertyChanged("MinTransparent");
                }
            }
        }
        private int _maxTransparent = 100;
        public int MaxTransparent
        {
            get => _maxTransparent;
            set
            {
                if (_maxTransparent != value)
                {
                    _maxTransparent = value;
                    RaisePropertyChanged("MaxTransparent");
                }
            }
        }
        private int _transparentValue = 80;
        public int TransparentValue
        {
            get => _transparentValue;
            set
            {
                if (_transparentValue != value)
                {
                    _transparentValue = value;
                    TRaisePropertyChanged("TransparentValue");
                    RealTransparentValue = value * 0.01;
                }
            }
        }

        private double _winBtnOpacity = 0.8;
        public double WindowBtnOpacity
        {
            get => _winBtnOpacity;
            set
            {
                if (value != _winBtnOpacity)
                {
                    _winBtnOpacity = value;
                    RaisePropertyChanged("WindowBtnOpacity");
                }
            }
        }
        
        private double _realTransparentValue = 0.8;
        public double RealTransparentValue
        {
            get => _realTransparentValue;
            set
            {
                if (_realTransparentValue != value)
                {
                    _realTransparentValue = value;
                    TRaisePropertyChanged("RealTransparentValue");
                }
            }
        }

        public void RealTime()
        {
            TPropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "RealTransparentValue")
                {
                    RaisePropertyChanged("RealTransparentValue");
                    Console.WriteLine(RealTransparentValue);
                }
            };
        }
        public event PropertyChangedEventHandler TPropertyChanged;
        protected virtual void TRaisePropertyChanged(string propertyName)
        {
            TPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand BtnMinmize { get; private set; }
        public ICommand BtnMaxsize { get; private set; }
        public ICommand BtnClose { get; private set; }

        public void WinBtnEvent()
        {
            BtnMinmize = new RelayCommand(WinMinmize);
            BtnMaxsize = new RelayCommand(WinMaxSize);
            BtnClose = new RelayCommand(WindowClose);
            WindowBtnOpacity = 0.5;
        }
        // Window Minimize
        private void WinMinmize()
        {
            WindowState = WindowState.Minimized;
        }

        // Window Size
        private void WinMaxSize()
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }
        private void WindowClose()
        {
            Application.Current.Shutdown();
        }
    }
}
