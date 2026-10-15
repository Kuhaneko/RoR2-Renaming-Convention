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
        List<string> languagesFiles;
        string customFolderLocation = null;

        //official
        List<string> officialFiles;
        string officialFolderLocation = null;


        //currently editing
        JObject currentlyEditing_JSON;
        List<string> currentlyEditing_Hash;
        string currentlyEditingFileName;

        //mirror for view
        JObject officialMirror_JSON;

        string paranoia_key = null;
        string paranoia_value = null;

        public MainWindow()
        {
            languagesFiles = new List<string>();
            officialFiles = new List<string>();
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
            checkbox_hideloredesc.Visibility = Visibility.Hidden;
            checkbox_hideloredesc.IsEnabled = false;
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

            SaveCurrentEdit();
            paranoia_key = null;
            paranoia_value = null;
            CustomLanguage_Textbox.Text = "";
            OfficialLnguage_Textbox.Text = "";

            currentlyEditing_Hash = new List<string>();
            currentlyEditingFileName = DLCList.SelectedItem.ToString();
            string jsonBytes = File.ReadAllText($"{customFolderLocation}\\{currentlyEditingFileName}.json");
            currentlyEditing_JSON = JObject.Parse(jsonBytes);
            JToken token = currentlyEditing_JSON.GetValue(currentlyEditingFileName != "language" ? "strings" : "language");
            foreach (JProperty curr in token.Children())
            {
                currentlyEditing_Hash.Add(curr.Name);
            }
            currentlyEditing_Hash.Sort();
            LangItemList.ItemsSource = currentlyEditing_Hash;

            if(IsOfficialLoaded())
            {
                jsonBytes = File.ReadAllText($"{officialFolderLocation}\\{currentlyEditingFileName}.json");
                officialMirror_JSON = JObject.Parse(jsonBytes);
            }
            FilterLangItems();
        }


        private void SelectLangItem(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (currentlyEditing_JSON == null) return;

            SaveCurrentEdit();

            JToken token = currentlyEditing_JSON.GetValue(DLCList.SelectedItem.ToString() != "language" ? "strings" : "language");
            paranoia_key = LangItemList.SelectedItem.ToString();
            paranoia_value = token[paranoia_key].ToString();
            CustomLanguage_Textbox.Text = paranoia_value;

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

        private void Click_FilterLangItems(object sender, RoutedEventArgs e)
        {
            SelectDLC(null, null);
        }

        private void FilterLangItems()
        {
            //ignore filters if both are not checked
            if((bool)!checkbox_untranslatedFilter.IsChecked && (bool)!checkbox_hideloredesc.IsChecked)
            {
                return;
            }

            //for now i dont really care about hiding the files themself because it's a hurdle to
            //needlessly check every single item for every file... just inefficient
            //it would be at the start if it was implemented though...


            //if we not editing any file then get me out
            if (currentlyEditing_Hash == null)
            {
                return;
            }

            List<string> copy_currentlyEditing = new List<string>(currentlyEditing_Hash);

            //for every one
            foreach(string lang in copy_currentlyEditing)
            {
                //first i'd hide lore because i dont want to run this twice
                if((bool)checkbox_hideloredesc.IsChecked)
                {
                    if(lang.ToUpper().Contains("_LORE") || lang.ToUpper().Contains("_DESC"))
                    {
                        currentlyEditing_Hash.Remove(lang);
                    }
                }

                if((bool)checkbox_untranslatedFilter.IsChecked)
                {
                    string stringorlang = StringOrLang(currentlyEditing_JSON);
                    if (currentlyEditing_JSON[stringorlang][lang].ToString() != officialMirror_JSON[stringorlang][lang].ToString())
                    {
                        currentlyEditing_Hash.Remove(lang);
                    }
                }

            }



            //checkbox_untranslatedFilter
            //checkbox_hideloredesc
        }

        string StringOrLang(JObject jobject)
        {
            string stringOrLang = "strings";
            JToken originalToken = jobject.GetValue(stringOrLang);
            if (originalToken == null)
            {
                stringOrLang = "language";
                originalToken = jobject.GetValue(stringOrLang);
            }
            return stringOrLang;
        }

        void SaveCurrentEdit()
        {
            if (paranoia_value == null) return;

            //if i changed
            if(paranoia_value != CustomLanguage_Textbox.Text)
            {
                string stringOrLang = StringOrLang(currentlyEditing_JSON);

                //now i know if it's strings or lang

                currentlyEditing_JSON[stringOrLang][paranoia_key] = CustomLanguage_Textbox.Text;


                string output = JsonConvert.SerializeObject(currentlyEditing_JSON, Formatting.Indented);
                File.WriteAllText($"{customFolderLocation}\\{currentlyEditingFileName}.json", output);
            }

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

            CustomLanguage_Textbox.Text = "";
            OfficialLnguage_Textbox.Text = "";
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
            checkbox_hideloredesc.Visibility = Visibility.Visible;
            checkbox_hideloredesc.IsEnabled = true;
            OfficialLnguage_Label.Visibility = Visibility.Visible;
            OfficialLnguage_Label.IsEnabled = true;
            OfficialLnguage_Textbox.Visibility = Visibility.Visible;

            ///SelectDLC
            if (DLCList.SelectedItem == null) return;

            string jsonBytes = File.ReadAllText($"{officialFolderLocation}\\{DLCList.SelectedItem.ToString()}.json");
            officialMirror_JSON = JObject.Parse(jsonBytes);
            JToken token = officialMirror_JSON.GetValue(DLCList.SelectedItem.ToString() != "language" ? "strings" : "language");



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
                List<string> customHash = LoadStringSetFromJObject(customToSaveLater);
                List<string> originalHash = LoadStringSetFromJObject(originalToReadData);


                //for all entries in a file that's officially included
                foreach (string entry in originalHash)
                {
                    //check if it exists and skip
                    if (customHash.Contains(entry)) continue;

                    //this entry needs to be added to the custom
                    string stringOrLang = StringOrLang(originalToReadData);

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

        List<string> LoadStringSetFromJObject(JObject jobject)
        {
            //file exists, check it's contents against the customs
            List<string> hashset = new List<string>();
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
