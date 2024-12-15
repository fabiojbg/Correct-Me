using CorrectMe.Localization;
using CorrectMe.Services;
using CorrectMe.Services.Models;
using OpenAI.Chat;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CorrectMe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        const string CHATGPT_MODEL = "gpt-4o";
        bool isRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            // set the size of the window to 75% of the screen size
            this.Width = SystemParameters.PrimaryScreenWidth * 0.60;
            this.Height = SystemParameters.PrimaryScreenHeight * 0.70;

            switchLanguage(AppSettings.UILanguage);
        }

        void UpdateUI(System.Action action)
        {
            this.Dispatcher.Invoke(action);
        }

        private void btnCorrect_Click(object sender, RoutedEventArgs e)
        {
            if (!tryGetKey(out string openAIKey))
                return;

            if (isRunning)
                return;

            isRunning = true;
            DetectedLanguage detectedLanguage = new DetectedLanguage(LanguageDetector.UNKOWN_LANGUAGE, LanguageDetector.UNKOWN_LANGUAGE);
            string userText = txtUserInput.Text;
            _ = Task.Run(() =>
            {
                try
                {
                    UpdateUI(() =>
                    {
                        txtWaitMsg.Text = AppResources.Wait_DetectingLanguage;
                        grdWait.Visibility = Visibility.Visible;
                    });

                    var detector = new LanguageDetector(openAIKey);
                    detectedLanguage = detector.DetectLanguage("gpt-4o-mini",
                                                               userText);

                    if (detectedLanguage.LanguageInEnglish == LanguageDetector.UNKOWN_LANGUAGE)
                    {
                        UpdateUI(() =>
                        {
                            txtAIResponse.Text = AppResources.Error_CouldNotDetermineTheLanguage;
                        });
                        return;
                    }
                    UpdateUI(() =>
                    {
                        txtAIResponse.Text = String.Format(AppResources.Msg_DetectedLanguage, detectedLanguage.LanguageInNative.ToUpper());
                    });
                }
                catch (Exception ex)
                {
                    UpdateUI(() =>
                    {
                        txtAIResponse.Text = String.Format(AppResources.Error_DetectingInputLanguageError, ex.Message);
                    });
                    return;
                }
                finally
                {
                    isRunning = false;
                    UpdateUI(() =>
                    {
                        grdWait.Visibility = Visibility.Hidden;
                    });
                }
            })
            .ContinueWith((t) =>
            {
                try
                {
                    if (detectedLanguage.LanguageInEnglish == LanguageDetector.UNKOWN_LANGUAGE)
                        return;

                    correctTextMistakes(openAIKey, detectedLanguage, userText);
                }
                catch (Exception ex)
                {
                    UpdateUI(() =>
                    {
                        txtAIResponse.Text = String.Format(AppResources.Error_CorrectingTheTextError, ex.Message);
                    });
                }
                finally
                {
                    isRunning = false;
                }
            });
        }

        private string getCurrentUILanguage()
        {
            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "pt")
                return "Brazilian Portuguese";

            return "American English";
        }

        private void correctTextMistakes(string openAIKey,
                                         DetectedLanguage language,
                                         string userText)
        {
            var stylingOutput = @"
<html>
<head>
    <meta charset=""UTF-8"">
    <style>
    ins {
	    background-color: 'green';
        color: 'white';
	    text-decoration: none;
    }

    del {
	    color: #333;
	    background-color:#FEC8C8;
    }
    .bodyFont 
    { 
        font-family: 'sans-serif',
        font-size:'18pt'
    }
    </style>
</head>
<body class='bodyFont'>
{{0}}
</body>
</html>
";
            Task.Run(() =>
            {
                var languageCorrector = new LanguageCorrector(openAIKey);
                var completionUpdates = languageCorrector.CorrectTextMistakes(language.LanguageInEnglish,
                                                                              getCurrentUILanguage(),
                                                                              userText);

                var aiResponse = "";
                foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
                {
                    if (completionUpdate.ContentUpdate.Count > 0)
                    {
                        var partResponse = completionUpdate.ContentUpdate[0].Text;
                        aiResponse += partResponse;
                        UpdateUI(() =>
                        {
                            txtAIResponse.Text += partResponse;
                        });
                    }
                }

                var diffHelper = new HtmlDiff.HtmlDiff(userText.Replace("\n", "<br>"),
                                                       aiResponse.Replace("  \r\n", "\r\n")
                                                                 .Replace("  \n", "\n")
                                                                 .Replace("\n", "<br>"));
                string diffOutput = diffHelper.Build();
                diffOutput = stylingOutput.Replace("{{0}}", diffOutput);
                UpdateUI(() =>
                {
                    webDiff.NavigateToString(diffOutput);
                });
            });
        }

        bool tryGetKey(out string openAIKey)
        {
            openAIKey = SecretKeyKeeper.GetKey();
            if (openAIKey == null)
            {
                MessageBox.Show(AppResources.Msg_SetOpenAIAPIKeyInTheSettingsMenu, AppResources.MsgBoxTitle_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }


        ResourceDictionary dictLanguage = new ResourceDictionary();
        private void switchLanguage(string newLanguage)
        {
            if (newLanguage == "PT")
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-BR");
                mnuLang_English.IsChecked = false;
                mnuLang_Portuguese.IsChecked = true;
            }
            else
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                mnuLang_English.IsChecked = true;
                mnuLang_Portuguese.IsChecked = false;
            }
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;

            switch (newLanguage)
            {
                case "PT":
                    dictLanguage.Source = new Uri("..\\Localization\\UIResources.pt-BR.xaml", UriKind.Relative);
                    break;
                case "EN":
                default:
                    dictLanguage.Source = new Uri("..\\Localization\\UIResources.xaml", UriKind.Relative);
                    break;
            }

            // fill the combo box with the languages available to translation
            var currentSelection = cmbLanguagesToTranslate.SelectedItem as LanguageToTranslate;
            var savedTargetSelection = AppSettings.TranslateTarget;
            var indexedLanguages = AppResources.LanguagesToTranslate.Split(';')
                                                                    .Select(indexedLanguage => LanguageToTranslate.Decode(indexedLanguage))
                                                                    .OrderBy(name => name.LocalizedName);
            cmbLanguagesToTranslate.Items.Clear();
            var defaultLanguageComboIndex = 0;
            foreach (var indexedLanguage in indexedLanguages)
            {
                cmbLanguagesToTranslate.Items.Add(indexedLanguage);

                if (currentSelection == null) // first time
                {
                    if( savedTargetSelection != -1 && savedTargetSelection == indexedLanguage.Id)
                        defaultLanguageComboIndex = cmbLanguagesToTranslate.Items.Count - 1;
                    else
                    if (indexedLanguage.IsDefault) 
                        defaultLanguageComboIndex = cmbLanguagesToTranslate.Items.Count - 1;
                }
                else
                {
                    if (indexedLanguage.Id == currentSelection.Id)
                        defaultLanguageComboIndex = cmbLanguagesToTranslate.Items.Count - 1;
                }
            }
            cmbLanguagesToTranslate.SelectedIndex = defaultLanguageComboIndex;
            AppSettings.TranslateTarget = (cmbLanguagesToTranslate.SelectedItem as LanguageToTranslate).Id;

            this.Resources.MergedDictionaries.Add(dictLanguage);
        }

        private void mnuSetKey_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenAIKeyDialog();
            dialog.Resources.MergedDictionaries.Add(dictLanguage);
            dialog.ShowDialog();
        }

        private void mnuLang_Change_Click(object sender, RoutedEventArgs e)
        {
            if (sender == mnuLang_English)
                switchLanguage("EN");
            if (sender == mnuLang_Portuguese)
                switchLanguage("PT");
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.Resources.MergedDictionaries.Add(dictLanguage);
            dialog.ShowDialog();

        }

        private void btnTranslate_Click(object sender, RoutedEventArgs e)
        {
            if (!tryGetKey(out string openAIKey))
                return;

            if (isRunning)
                return;

            isRunning = true;
            DetectedLanguage detectedLanguage = new DetectedLanguage(LanguageDetector.UNKOWN_LANGUAGE, LanguageDetector.UNKOWN_LANGUAGE);
            var selectedLanguageInCombo = cmbLanguagesToTranslate.SelectedItem as LanguageToTranslate;
            string userText = txtUserInputToTranslate.Text;
            txtAITranslatedResponse.Text = "";

            _ = Task.Run(() => {

                try
                {
                    var selectedLanguageInEnglish = getTargetLanguagesInEnglish().FirstOrDefault(x => x.Id == selectedLanguageInCombo?.Id);

                    proceedTranslation(openAIKey,
                                        detectedLanguage.LanguageInEnglish,
                                        selectedLanguageInEnglish.ToString(),
                                        userText);
                }
                catch (Exception ex)
                {
                    UpdateUI(() =>
                    {
                        txtAITranslatedResponse.Text = String.Format(AppResources.Error_CorrectingTheTextError, ex.Message);
                    });
                }
                finally
                {
                    isRunning = false;
                }

            });


        }

        private void proceedTranslation(string openAIKey,
                                         string fromLanguage,
                                         string toLanguage,
                                         string userText)
        {
            Task.Run(() =>
            {
                var languageCorrector = new LanguageTranslator(openAIKey);
                var completionUpdates = languageCorrector.Translate(fromLanguage,
                                                                     toLanguage,
                                                                     userText);

                var aiResponse = "";
                foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
                {
                    if (completionUpdate.ContentUpdate.Count > 0)
                    {
                        var partResponse = completionUpdate.ContentUpdate[0].Text;
                        aiResponse += partResponse;
                        UpdateUI(() =>
                        {
                            txtAITranslatedResponse.Text += partResponse;
                        });
                    }
                }

            });
        }

        LanguageToTranslate[] getTargetLanguagesInEnglish()
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            var result =  AppResources.LanguagesToTranslate.Split(';')
                                                    .Select(indexedLanguage => LanguageToTranslate.Decode(indexedLanguage))
                                                    .ToArray();

            Thread.CurrentThread.CurrentUICulture = currentCulture;
            return result;
        }

        private void cmbLanguagesToTranslate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( cmbLanguagesToTranslate.SelectedItem != null)
                AppSettings.TranslateTarget = (cmbLanguagesToTranslate.SelectedItem as LanguageToTranslate).Id;
        }
    }
}
