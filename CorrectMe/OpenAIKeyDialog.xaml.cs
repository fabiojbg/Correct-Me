using CorrectMe.Localization;
using CorrectMe.Services;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CorrectMe
{
    /// <summary>
    /// Interaction logic for OpenAIKeyDialog.xaml
    /// </summary>
    public partial class OpenAIKeyDialog : MetroWindow
    {
        public OpenAIKeyDialog()
        {
            InitializeComponent();
        }

        private void btnSaveKey_Click(object sender, RoutedEventArgs e)
        {
            var openAIKey = txtSecretKey.Text;
            if (String.IsNullOrEmpty(openAIKey))
            {
                MessageBox.Show(AppResources.Error_TheKeyCannotBeEmpty, AppResources.MsgBoxTitle_Error, 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SecretKeyKeeper.SaveKey(openAIKey);
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
