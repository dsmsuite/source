﻿using System.Windows;

namespace DsmSuite.DsmViewer.View.Dialogs
{
    /// <summary>
    /// Interaction logic for SnapshotCreateDialog.xaml
    /// </summary>
    public partial class SnapshotCreateDialog
    {
        public SnapshotCreateDialog()
        {
            InitializeComponent();
        }

        private void OnOkButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
