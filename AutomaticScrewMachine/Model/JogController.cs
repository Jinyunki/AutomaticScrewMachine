using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace AutomaticScrewMachine.Model
{
    public class JogController : ViewModelBase
    {

        #region JogControlBtn
        public ICommand BtnJogRight { get; private set; }
        public ICommand BtnJogLeft { get; private set; }
        public ICommand BtnJogDown { get; private set; }
        public ICommand BtnJogUp { get; private set; }
        public ICommand BtnJogStop { get; private set; }
        #endregion

        #region ScrewUpDownBtn
        public ICommand BtnScrewUp { get; private set; }
        public ICommand BtnScrewDown { get; private set; }
        public ICommand BtnScrewReturn { get; private set; }
        #endregion
        private Brush _XsignalColor = Brushes.Gray;
        public Brush X_SignalColor
        {
            get => _XsignalColor;
            set
            {
                if (_XsignalColor != value)
                {
                    _XsignalColor = value;
                    RaisePropertyChanged("X_SignalColor");
                }
            }
        }


        public event PropertyChangedEventHandler TPropertyChanged;
        protected virtual void TRaisePropertyChanged(string propertyName)
        {
            TPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void ScrewController()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                BtnScrewUp = new RelayCommand(ScrewUp);
                BtnScrewDown = new RelayCommand(ScrewDown);
                BtnScrewReturn = new RelayCommand(ScrewReturn);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void ScrewReturn()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {

            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void ScrewDown()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {

            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void ScrewUp()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {

            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        public void JogControl()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                TPropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "SignalColor")
                    {
                        RaisePropertyChanged("SignalColor");

                    }
                };

                BtnJogRight = new RelayCommand(JogRight);
                BtnJogLeft = new RelayCommand(JogLeft);
                BtnJogDown = new RelayCommand(JogDown);
                BtnJogUp = new RelayCommand(JogUp);
                BtnJogStop = new RelayCommand(JogStop);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void JogStop()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {

            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void JogUp()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {

            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void JogDown()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {

            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void JogLeft()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                X_SignalColor = Brushes.Green;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }

        private void JogRight()
        {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\n");
            try
            {
                X_SignalColor = Brushes.Green;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + (MethodBase.GetCurrentMethod().Name) + "\nException : " + ex);
                throw;
            }

        }
    }
}
