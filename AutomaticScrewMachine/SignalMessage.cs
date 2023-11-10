using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticScrewMachine
{
    public class SignalMessage
    {
        public bool IsPress { get; }
        public string IsViewName { get; }

        public SignalMessage(string viewName, bool isPress)
        {
            IsPress = isPress;
            IsViewName = viewName;
        }
    }
}
