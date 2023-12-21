using AutomaticScrewMachine.CurrentList._1.Jog.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticScrewMachine.CurrentList._1.Jog.Base {
    public abstract class JogBase : JogData {
        public abstract void JogStickControl_IO ();
    }
}
