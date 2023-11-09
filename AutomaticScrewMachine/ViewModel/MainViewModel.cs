using AutomaticScrewMachine.Model;

namespace AutomaticScrewMachine.ViewModel
{
    public class MainViewModel : WindowStyle
    {
        public MainViewModel()
        {
            WinBtnEvent();
            RealTime();
            CurrentViewModel = _locator.JogControllerViewModel;
        }

    }
}