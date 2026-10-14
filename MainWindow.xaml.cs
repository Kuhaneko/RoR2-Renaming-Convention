using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;



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

        //official
        HashSet<string> officialFiles;
        string officialFolderLocation = null;


        //currently editing
        JObject currentlyEditing_JSON;
        HashSet<string> currentlyEditing_Hash;

        //mirror for view
        JObject officialMirror_JSON;
        HashSet<string> officialMirror_Hash;

        public MainWindow()
        {
            languagesFiles = new HashSet<string>();
            officialFiles = new HashSet<string>();
            InitializeComponent();
            SwitchWindow(true);
        }

        /// /// /// /// /// ///
        /// LANG  FUNCTIONS ///
        /// /// /// /// /// ///

        void SetUpCustomLanguageWindow()
        {
            SwitchWindow(false);
            UpdateCustomWithOriginal_Button.Visibility = Visibility.Hidden;
            UpdateCustomWithOriginal_Button.IsEnabled = false;
            checkbox_untranslatedFilter.Visibility = Visibility.Hidden;
            checkbox_untranslatedFilter.IsEnabled = false;
            OfficialLnguage_Label.Visibility = Visibility.Hidden;
            OfficialLnguage_Label.IsEnabled = false;
            OfficialLnguage_Textbox.Visibility = Visibility.Hidden;
            OfficialLnguage_Textbox.IsEnabled = false;

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

            if(IsOfficialLoaded())
            {
                officialMirror_Hash = new HashSet<string>();
                jsonBytes = File.ReadAllText($"{officialFolderLocation}\\{DLCList.SelectedItem.ToString()}.json");
                officialMirror_JSON = JObject.Parse(jsonBytes);
                token = officialMirror_JSON.GetValue(DLCList.SelectedItem.ToString() != "language" ? "strings" : "language");
                foreach (JProperty curr in token.Children())
                {
                    officialMirror_Hash.Add(curr.Name);
                }
            }
        }


        private void SelectLangItem(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (currentlyEditing_JSON == null) return;

            JToken token = currentlyEditing_JSON.GetValue(DLCList.SelectedItem.ToString() != "language" ? "strings" : "language");
            CustomLanguage_Textbox.Text = token[LangItemList.SelectedItem.ToString()].ToString();

            if(IsOfficialLoaded())
            {
                token = officialMirror_JSON.GetValue(DLCList.SelectedItem.ToString() != "language" ? "strings" : "language");
                OfficialLnguage_Textbox.Text = token[LangItemList.SelectedItem.ToString()].ToString();
            }
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //checkbox_untranslatedFilter
        }

        bool IsOfficialLoaded()
        {
            return officialFiles.Count > 0;
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

        private void Click_LoadOriginalLanguage(object sender, RoutedEventArgs e)
        {
            officialFolderLocation = LoadLanguagePathFromDialog();
            if (officialFolderLocation == null) { return; }

            string[] folderFiles = Directory.GetFiles(officialFolderLocation, "*.json", SearchOption.TopDirectoryOnly);
            foreach (string folder in folderFiles)
            {
                officialFiles.Add(Path.GetFileNameWithoutExtension(folder));
            }

            UpdateCustomWithOriginal_Button.Visibility = Visibility.Visible;
            UpdateCustomWithOriginal_Button.IsEnabled = true;
            checkbox_untranslatedFilter.Visibility = Visibility.Visible;
            checkbox_untranslatedFilter.IsEnabled = true;
            OfficialLnguage_Label.Visibility = Visibility.Visible;
            OfficialLnguage_Label.IsEnabled = true;
            OfficialLnguage_Textbox.Visibility = Visibility.Visible;

            ///SelectDLC
            if (DLCList.SelectedItem == null) return;

            officialMirror_Hash = new HashSet<string>();
            string jsonBytes = File.ReadAllText($"{officialFolderLocation}\\{DLCList.SelectedItem.ToString()}.json");
            officialMirror_JSON = JObject.Parse(jsonBytes);
            JToken token = officialMirror_JSON.GetValue(DLCList.SelectedItem.ToString() != "language" ? "strings" : "language");
            foreach (JProperty curr in token.Children())
            {
                officialMirror_Hash.Add(curr.Name);
            }


            ///SelectLangItem
            if (DLCList.SelectedItem == null || currentlyEditing_JSON == null) return;

            token = officialMirror_JSON.GetValue(DLCList.SelectedItem.ToString() != "language" ? "strings" : "language");
            OfficialLnguage_Textbox.Text = token[LangItemList.SelectedItem.ToString()].ToString();

        }


        private void Click_UpdateCustomWithOriginal(object sender, RoutedEventArgs e)
        {
            //for all the files that are officially included
            foreach (string original in officialFiles)
            {
                //check if it exists in custom before continuing
                if (!File.Exists($"{customFolderLocation}\\{original}.json"))
                {
                    File.Copy($"{officialFolderLocation}\\{original}.json", $"{customFolderLocation}\\{original}.json");
                    continue;
                }

                //file exists, check it's contents against the customs
                JObject customToSaveLater = GetJObjectFromJson($"{customFolderLocation}\\{original}.json");
                JObject originalToReadData = GetJObjectFromJson($"{officialFolderLocation}\\{original}.json");
                HashSet<string> customHash = LoadStringSetFromJObject(customToSaveLater);
                HashSet<string> originalHash = LoadStringSetFromJObject(originalToReadData);


                //for all entries in a file that's officially included
                foreach (string entry in originalHash)
                {
                    //check if it exists and skip
                    if (customHash.Contains(entry)) continue;

                    //this entry needs to be added to the custom
                    string stringOrLang = "strings";
                    JToken originalToken = originalToReadData.GetValue(stringOrLang);
                    if (originalToken == null)
                    {
                        stringOrLang = "language";
                        originalToken = originalToReadData.GetValue(stringOrLang);
                    }

                    customToSaveLater[stringOrLang][entry] = originalToReadData[stringOrLang][entry];
                }

                string output = JsonConvert.SerializeObject(customToSaveLater, Formatting.Indented);
                File.WriteAllText($"{customFolderLocation}\\{original}.json", output);
            }
        }

        JObject GetJObjectFromJson(string path)
        {
            string jsonBytes = File.ReadAllText(path);
            JObject currentlyEditing_JSON = JObject.Parse(jsonBytes);
            return currentlyEditing_JSON;
        }

        HashSet<string> LoadStringSetFromJObject(JObject jobject)
        {
            //file exists, check it's contents against the customs
            HashSet<string> hashset = new HashSet<string>();
            JToken token = jobject.GetValue("strings");
            if(token == null)
            {
                token = jobject.GetValue("language");
            }
            foreach (JProperty curr in token.Children())
            {
                hashset.Add(curr.Name);
            }

            return hashset;
        }
    }
}
