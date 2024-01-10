using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using AutomaticScrewMachine.ViewModel;

namespace AutomaticScrewMachine.Model {
    public class MainViewBindingData  : ViewModelBase {


        public static string szFilePath = @"D:\WindowsFormsApp1\WindowsFormsApp1\Teaching\motor_para.mot";
        public ViewModelLocator _locator = new ViewModelLocator();
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel {
            get {
                return _currentViewModel;
            }
            set {
                _currentViewModel = value;
                RaisePropertyChanged("CurrentViewModel");
            }
        }


        private WindowState _windowState;
        public WindowState WindowState {
            get { return _windowState; }
            set {
                if (_windowState != value) {
                    _windowState = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _minTransparent = 1;
        public int MinTransparent {
            get => _minTransparent;
            set {
                if (_minTransparent != value) {
                    _minTransparent = value;
                    RaisePropertyChanged("MinTransparent");
                }
            }
        }
        private int _maxTransparent = 100;
        public int MaxTransparent {
            get => _maxTransparent;
            set {
                if (_maxTransparent != value) {
                    _maxTransparent = value;
                    RaisePropertyChanged("MaxTransparent");
                }
            }
        }
        private int _transparentValue = 100;
        public int TransparentValue {
            get => _transparentValue;
            set {
                if (_transparentValue != value) {
                    _transparentValue = value;
                    TRaisePropertyChanged("TransparentValue");
                    RealTransparentValue = value * 0.01;
                }
            }
        }
        private double _winBtnOpacity = 0.8;
        public double WindowBtnOpacity {
            get => _winBtnOpacity;
            set {
                if (value != _winBtnOpacity) {
                    _winBtnOpacity = value;
                    RaisePropertyChanged("WindowBtnOpacity");
                }
            }
        }

        private double _realTransparentValue = 1.0;
        public double RealTransparentValue {
            get => _realTransparentValue;
            set {
                if (_realTransparentValue != value) {
                    _realTransparentValue = value;
                    TRaisePropertyChanged("RealTransparentValue");
                }
            }
        }

        public void RealTime () {
            TPropertyChanged += (sender, args) => {
                if (args.PropertyName == "RealTransparentValue") {
                    RaisePropertyChanged("RealTransparentValue");

                }
            };
        }
        public event PropertyChangedEventHandler TPropertyChanged;
        protected virtual void TRaisePropertyChanged (string propertyName) {
            TPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Window Viewing Btn
        public ICommand BtnMinmize { get; set; }
        public ICommand BtnMaxsize { get;  set; }
        public ICommand BtnClose { get; set; }
        public ICommand CurrentJogView { get; set; }
        public ICommand CurrentMainView { get; set; }
        public ICommand CurrentIO { get; set; }
        public ICommand CurrentTORQUE { get; set; }
        #endregion



        
    }
}
