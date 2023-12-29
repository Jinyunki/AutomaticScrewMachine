using AutomaticScrewMachine.Model;
using System;
using System.Windows;

namespace AutomaticScrewMachine.ViewModel {
    public class MainViewModel : MainViewBindingData {
        public MainViewModel()
        {
            WinBtnEvent();
            RealTime();
            CurrentViewModel = _locator.JogViewModel;
            Docking();
        }
        private void Docking () {
            //++ AXL(AjineXtek Library)�� ��밡���ϰ� �ϰ� ������ ������� �ʱ�ȭ�մϴ�.
            //if (CAXL.AxlOpenNoReset(7) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            if (CAXL.AxlOpen(7) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                MessageBox.Show("Intialize Fail..!!");
            if (CAXM.AxmMotLoadParaAll(szFilePath) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                MessageBox.Show("Mot File Not Found.");

        }
    }
}