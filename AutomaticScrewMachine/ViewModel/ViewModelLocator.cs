/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:AutomaticScrewMachine"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using AutomaticScrewMachine.CurrentList._1.Jog.ViewModel;
using AutomaticScrewMachine.CurrentList._3.IO.ViewModel;
using AutomaticScrewMachine.CurrentList._4.TorqControllerStatus.ViewModel;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace AutomaticScrewMachine.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<JogViewModel>();
            SimpleIoc.Default.Register<IOMapViewModel>();
            SimpleIoc.Default.Register<TorqueIOViewModel>();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }
        public JogViewModel JogViewModel {
            get
            {
                return ServiceLocator.Current.GetInstance<JogViewModel>();
            }
        }
         public IOMapViewModel IOMapViewModel {
            get
            {
                return ServiceLocator.Current.GetInstance<IOMapViewModel>();
            }
        }
        
        
         public TorqueIOViewModel TorqueIOViewModel {
            get
            {
                return ServiceLocator.Current.GetInstance<TorqueIOViewModel>();
            }
        }
        
        
        public static void Cleanup()
        {
            // ºä¸ðµ¨ Á¤¸® ÄÚµå Ãß°¡
            SimpleIoc.Default.GetInstance<MainViewModel>()?.Cleanup();
            SimpleIoc.Default.GetInstance<JogViewModel>()?.Cleanup();
            SimpleIoc.Default.GetInstance<IOMapViewModel>()?.Cleanup();
        }
    }
}