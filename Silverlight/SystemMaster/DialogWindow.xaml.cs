using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SystemMaster
{
    public partial class DialogWindow : ChildWindow
    {
        IDialogWindowControl dialogWindowControl = null;
        public UserControl ContentUserControl = null;

        public DialogWindow(UserControl contentUserControl, double width, double height)
        {
            InitializeComponent();

            this.Width = width;
            this.Height = height;

            //this.OverlayBrush = new SolidColorBrush(Colors.Transparent);
            //this.OverlayOpacity = 0.5;
            //this.Opacity = 0.75;

            LayoutRoot.RowDefinitions[0].Height = new GridLength(this.Height - 63);

            this.ContentUserControl = contentUserControl;

            if (contentUserControl is IDialogWindowControl)
                this.dialogWindowControl = (IDialogWindowControl)contentUserControl;

            this.ContentPanel.Children.Add(contentUserControl);

            ThemeManager.ApplyStyle(contentUserControl);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.OKButton.IsEnabled = false;
            this.CancelButton.IsEnabled = false;

            if (this.dialogWindowControl != null)
                this.dialogWindowControl.OKClose();
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.OKButton.IsEnabled = false;
            this.CancelButton.IsEnabled = false;

            if (this.dialogWindowControl != null)
                this.dialogWindowControl.CancelClose();

            this.DialogResult = false;
        }
    }
}

