using CorrectMe.Enums;
using CorrectMe.Extensions;
using CorrectMe.Localization;
using CorrectMe.Services;
using GPTSdk;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CorrectMe
{
    public class ModelItem
    {
        public string Caption { get; set; }
        public string Name { get; set; }
        public bool isDefault { get; set; }

        public override string ToString()
        {
            return Caption;
        }
    }

    /// <summary>
    /// Interaction logic for OpenAIKeyDialog.xaml
    /// </summary>
    public partial class OpenAIKeyDialog : MetroWindow
    {
        bool isInitializing;
        IChatGPTApiClient _selectedGptClient;
        string _gptType = AppSettings.GPT_Type;
        public OpenAIKeyDialog()
        {
            try
            {
                isInitializing = true;
                InitializeComponent();
                initForm(false);
            }
            finally
            {
                isInitializing = false;
            }
        }

        private void initForm(bool isChangingSelection)
        {
            cmbGPTSelection.ItemsSource = Enum.GetValues(typeof(GPTServiceType)).Cast<GPTServiceType>()
                                              .Select( gpt => AppResources.ResourceManager.GetString("GPTSelection_" + gpt))
                                              .ToList();
            cmbGPTSelection.SelectedIndex = (int)Enum.Parse(typeof(GPTServiceType), AppSettings.GPT_Type);
            
            if( !isChangingSelection)
                cmbGPTSelection_SelectionChanged(cmbGPTSelection, null);

            if (cmbGPTSelection.SelectedIndex == (int)GPTServiceType.Custom)
            {
                txtGPTURL.Text = AppSettings.GPT_URL;
            }
            txtSecretKey.Text = AppSettings.GPT_Key;
            fillModelsList();
        }

        private async void fillModelsList()
        {
            if (cmbGPTSelection.SelectedIndex != -1 && !string.IsNullOrWhiteSpace(txtSecretKey.Text))
            {
                if( cmbGPTSelection.SelectedIndex == (int)GPTServiceType.Custom && string.IsNullOrWhiteSpace(txtGPTURL.Text))
                {
                    return;
                }

                var models = await getModelsList();
                if(models == null)
                    return;

                cmbGPTModelSelection.ItemsSource = models;
                if (!AppSettings.GPT_Model.IsNullOrEmpty())
                {
                    cmbGPTModelSelection.SelectedValue = models.FirstOrDefault( model => model.Name == AppSettings.GPT_Model);
                }
                else
                if (!String.IsNullOrEmpty(_selectedGptClient?.DefaultModel) &&
                    models.Any(model => model.Name == _selectedGptClient?.DefaultModel))
                {
                    cmbGPTModelSelection.SelectedValue = _selectedGptClient.DefaultModel;
                }
            }
        }

        private void btnSaveKey_Click(object sender, RoutedEventArgs e)
        {
            var GPTAIKey = txtSecretKey.Text;
            if (String.IsNullOrWhiteSpace(GPTAIKey))
            {
                MessageBox.Show(AppResources.Error_TheKeyCannotBeEmpty, AppResources.MsgBoxTitle_Error, 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _gptType = AppSettings.GPT_Type = ((GPTServiceType)cmbGPTSelection.SelectedIndex).ToString();
            AppSettings.GPT_Key = GPTAIKey.Trim();

            var selectedModel = (cmbGPTModelSelection.SelectedValue as ModelItem)?.Name;
            AppSettings.GPT_Model = selectedModel != null && selectedModel != AppResources.msgOpenToLoad ? selectedModel : "";

            switch ((GPTServiceType)cmbGPTSelection.SelectedIndex)
            {
                case GPTServiceType.Custom:
                    AppSettings.GPT_URL = txtGPTURL.Text.Trim();
                    break;
                default:
                    AppSettings.GPT_URL = string.Empty;
                    break;
            }

            this.DialogResult = true;
        }

        private async Task<List<ModelItem>> getModelsList()
        {
            try
            {
                var result = new List<ModelItem>();

                var serviceType = (GPTServiceType)cmbGPTSelection.SelectedIndex;

                _selectedGptClient = GPTClientFactory.CreateClient(serviceType, txtSecretKey.Text, txtGPTURL.Text);
                var models = await _selectedGptClient.ListModelsAsync();
                switch (serviceType)
                {
                    case GPTServiceType.OpenAI:
                        models = models.Where(model => model.Contains("gpt") && !model.Contains("audio")).OrderBy( m => m);
                        break;
                }

                models = !_selectedGptClient.DefaultModel.IsNullOrEmpty() ? models.Where(m => m != _selectedGptClient.DefaultModel).ToList() 
                                                                 : models;

                result = models.Select(m => new ModelItem { Caption = m, Name = m, isDefault = false }).ToList();
                
                if ( !_selectedGptClient.DefaultModel.IsNullOrEmpty())
                {
                    result.Insert(0, new ModelItem
                    {
                        Caption = String.Format(AppResources.DefaultModel, _selectedGptClient.DefaultModel),
                        Name = _selectedGptClient.DefaultModel,
                        isDefault = true
                    } );
                }
                return result;
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format( AppResources.ErrorRetrievingModels, ex.Message), 
                                AppResources.MsgBoxTitle_Error,
                                MessageBoxButton.OK, MessageBoxImage.Error);

            }
            return null;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            AppSettings.GPT_Type = _gptType; // get back to the last model saved or the model selected before opening the dialog
        }
        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            AppSettings.GPT_Type = _gptType; // get back to the last model saved or the model selected before opening the dialog
        }

        private void cmbGPTSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmb = sender as ComboBox;
            if ( cmb != null && stkGPTURL!=null)
            {
                if ( cmb.SelectedIndex == (int)GPTServiceType.Custom)
                {
                    stkGPTURL.Visibility = Visibility.Visible;
                    this.Height = 460;
                }
                else
                {
                    stkGPTURL.Visibility = Visibility.Collapsed;
                    this.Height = 410;
                }
                if (!isInitializing)
                {
                    AppSettings.GPT_Type = ((GPTServiceType)cmbGPTSelection.SelectedIndex).ToString();
                    initForm(true);
                }
            }
        }

        bool mustReloadModels = false;
        private void cmbGPTModelSelection_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void textComponent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInitializing)
                return;

            mustReloadModels = true;
            cmbGPTModelSelection.ItemsSource = new List<string> { AppResources.msgOpenToLoad };
            cmbGPTModelSelection.SelectedIndex = 0;
        }

        private void cmbGPTModelSelection_DropDownOpened(object sender, EventArgs e)
        {
            if (mustReloadModels)
            {
                this.Dispatcher.Invoke( () =>
                {
                    cmbGPTModelSelection.ItemsSource = new List<string> { AppResources.msgLoading };
                    cmbGPTModelSelection.SelectedIndex = 0;
                    fillModelsList();
                });
                mustReloadModels = false;
            }
        }

    }
}
