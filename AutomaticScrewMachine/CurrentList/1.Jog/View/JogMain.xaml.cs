﻿using AutomaticScrewMachine.Bases;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutomaticScrewMachine.CurrentList._1.Jog.View {
    /// <summary>
    /// JogMain.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class JogMain : UserControl {
        private Border clickedBorder;
        public JogMain () {
            InitializeComponent();
        }
        private void JogStop_MouseUp (object sender, MouseButtonEventArgs e) {
            if (clickedBorder != null) {
                StaticControllerSignal.StopControllerSignalView();
            }
        }

    }
}
