using System.Windows.Controls;
using System.Windows.Input;

namespace AutomaticScrewMachine.CurrentList._1.Jog.View.frag {
    /// <summary>
    /// Frag_PositionList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Frag_PositionList : UserControl {
        public Frag_PositionList () {
            InitializeComponent();
        }

        private void PosListBox_MouseDown (object sender, MouseButtonEventArgs e) {
            PosListBox.SelectedItem = null;
        }
    }
}
