using System.Windows.Controls;
using System.Windows.Input;

namespace AutomaticScrewMachine.CurrentList._1.Jog.View.frag {
    /// <summary>
    /// Frag_SequnceList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Frag_SequnceList : UserControl {
        public Frag_SequnceList () {
            InitializeComponent();
        }

        private void SeqListBox_MouseDown (object sender, MouseButtonEventArgs e) {
            SeqListBox.SelectedItem = null;
        }
    }
}
