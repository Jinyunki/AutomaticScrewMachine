using AutomaticScrewMachine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticScrewMachine.ViewModel
{
    public class JogControllerViewModel : JogController
    {
        public JogControllerViewModel()
        {
            JogControl();
            ScrewController();
        }
    }
}
