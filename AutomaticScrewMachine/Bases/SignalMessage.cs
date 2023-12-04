namespace AutomaticScrewMachine.Bases {
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
