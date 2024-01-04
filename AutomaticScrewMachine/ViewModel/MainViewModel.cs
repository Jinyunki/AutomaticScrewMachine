using AutomaticScrewMachine.Model;
using AutomaticScrewMachine.Utiles;
using GalaSoft.MvvmLight.Command;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace AutomaticScrewMachine.ViewModel {
    public class MainViewModel : MainViewBindingData {
        public MainViewModel()
        {

            SerialPortAdapter.ConnectedSerial();
            
            WinBtnEvent();
            RealTime();
            Docking();
        }
        private void Docking () {
            //++ AXL(AjineXtek Library)을 사용가능하게 하고 장착된 보드들을 초기화합니다.
            //if (CAXL.AxlOpenNoReset(7) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            if (CAXL.AxlOpen(7) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                MessageBox.Show("Intialize Fail..!!");
            if (CAXM.AxmMotLoadParaAll(szFilePath) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                MessageBox.Show("Mot File Not Found.");

        }

        public void WinBtnEvent () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                // Window Default Btn
                BtnMinmize = new RelayCommand(WinMinmize);
                BtnMaxsize = new RelayCommand(WinMaxSize);
                BtnClose = new RelayCommand(WindowClose);
                // CurrentViewControl
                CurrentJogView = new RelayCommand(() => CurrentViewModel = _locator.JogViewModel);
                //CurrentMainView = new RelayCommand(() => SerialPortAdapter.WriteTorqSerial());

                WindowBtnOpacity = 0.5;
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }


        // Window Minimize
        private void WinMinmize () {
            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                WindowState = WindowState.Minimized;
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }
        }

        // Window Size
        private void WinMaxSize () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }
        private void WindowClose () {

            Trace.WriteLine("==========   Start   ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\n");
            try {
                Application.Current.Shutdown();

            } catch (Exception ex) {
                Trace.WriteLine("========== Exception ==========\nMethodName : " + MethodBase.GetCurrentMethod().Name + "\nException : " + ex);
                throw;
            }

        }
    }
}