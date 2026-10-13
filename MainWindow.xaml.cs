using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;



namespace RoR2RenamingConvention
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //custom
        HashSet<string> languagesFiles;
        string customFolderLocation = null;
        object customLangaugeData = null;

        //official
        string officialFolderLocation = null;


        //currently editing
        JObject currentlyEditing_JSON;
        HashSet<string> currentlyEditing_Hash;

        public MainWindow()
        {
            languagesFiles = new HashSet<string>();
            InitializeComponent();
            SwitchWindow(true);
        }

        /// /// /// /// /// ///
        /// LANG  FUNCTIONS ///
        /// /// /// /// /// ///

        void SetUpCustomLanguageWindow()
        {
            SwitchWindow(false);
            //set up icon
            if (File.Exists(customFolderLocation + "/icon.png"))
            {
                IconImage.Source = new BitmapImage(new Uri(customFolderLocation + "/icon.png"));
            }

            DLCList.ItemsSource = languagesFiles;


            ///                             ///
            /// Tutorial how to import JSON ///
            ///                             ///
            string langaugeJson = File.ReadAllText(customFolderLocation + "/language.json");
            JObject languageTest = JObject.Parse(langaugeJson);
            LanguageLabel.Content = languageTest["language"]?["selfname"]?.ToString();
        }

        private void SelectDLC(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DLCList.SelectedItem == null) return;

            currentlyEditing_Hash = new HashSet<string>();
            string jsonBytes = File.ReadAllText($"{customFolderLocation}\\{DLCList.SelectedItem.ToString()}.json");
            currentlyEditing_JSON = JObject.Parse(jsonBytes);
            JToken token = currentlyEditing_JSON.GetValue(DLCList.SelectedItem.ToString() != "language" ? "strings" : "language");
            foreach (JProperty curr in token.Children())
            {
                currentlyEditing_Hash.Add(curr.Name);
            }
            LangItemList.ItemsSource = currentlyEditing_Hash;
        }

        private void SelectLangItem(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (currentlyEditing_JSON == null) return;

            JToken token = currentlyEditing_JSON.GetValue(DLCList.SelectedItem.ToString() != "language" ? "strings" : "language");
                
            debug_DLCitemlabel.Content = token[LangItemList.SelectedItem.ToString()].ToString();
        }

        string LoadLanguagePathFromDialog()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ValidateNames = false;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Filter = "Language setter|language.json";
            ofd.Title = "Load Custom Language (if you don't have one copy from \"en\" and name it another 2 letter language)";
            ofd.ShowDialog();
            if (ofd.FileName == "")
            {
                return null;
            }
            return Path.GetDirectoryName(ofd.FileName);
        }

        /// /// /// /// /// ///
        /// INIT  FUNCTIONS ///
        /// /// /// /// /// ///

        private void Click_LoadCustomLanguage(object sender, RoutedEventArgs e)
        {
            customFolderLocation = LoadLanguagePathFromDialog();
            if(customFolderLocation == null) { return; }

            string[] folderFiles = Directory.GetFiles(customFolderLocation, "*.json", SearchOption.TopDirectoryOnly);
            foreach(string folder in folderFiles)
            {
                languagesFiles.Add(Path.GetFileNameWithoutExtension(folder));
            }
            
            SetUpCustomLanguageWindow();
        }

        void SwitchWindow(bool initialBoot)
        {
            ConventionGrid.IsEnabled = !initialBoot;
            ConventionGrid.Visibility = initialBoot? Visibility.Hidden : Visibility.Visible;

            LoadGrid.IsEnabled = initialBoot;
            LoadGrid.Visibility = initialBoot ? Visibility.Visible : Visibility.Hidden;
        }


    }
}
