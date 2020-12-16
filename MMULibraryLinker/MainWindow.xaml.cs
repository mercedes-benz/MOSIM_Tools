// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Kłodowski

using System;
using System.Windows;
using System.Windows.Controls;
using MMICSharp.Common.Communication;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;

namespace MMULibraryLinker
{
    [Serializable]
    public class LibraryLink
    {
        public string url;
        public string token;
        public string name;
    }

    public class LibraryManagers
    {
        public LibraryManagers(string Name, string Path, int Index)
        {
            this.name = Name;
            this.path = Path;
            this.index = Index;
        }

        public string name { get; set; }
        public string path { get; set; }
        public int index { get; set; }
    }

    public enum ErrorType {NoLibraryManagers,LinkError,NoLink};

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private List<string> LauncherInstances;
        private int DefaultLibrary = 0;
        public LibraryLink link;
        public static ObservableCollection<LibraryManagers> LibraryManagerCollection
        { get; set; } = new ObservableCollection<LibraryManagers>();

        public void GetPathsFromRegistry()
        {
            LibraryManagerCollection.Clear();
            RegistryKey key;
            key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\MOSIM\\Launcher");
            if (key != null)
            {
                var values = key.GetSubKeyNames();
                var managerName = "";
                for (int i = 0; i < key.SubKeyCount; i++)
                    if (values[i].IndexOf("Instance") == 0)
                    {
                        int curinstance = 0;
                        if (Int32.TryParse(values[i].Substring(8), out curinstance))
                        {
                            var launcherKey = key.OpenSubKey(values[i]);
                            if (launcherKey != null)
                            {
                                managerName="Launcher "+ launcherKey.GetValue("Version").ToString()+" - "+ launcherKey.GetValue("Name").ToString();
                                var path = System.IO.Path.GetDirectoryName(launcherKey.GetValue("Path").ToString());
                                if (path[path.Length - 1] != '\\')
                                    path += "\\";
                                if (Directory.Exists(path))
                                LibraryManagerCollection.Add(new LibraryManagers(managerName, path, curinstance));
                                launcherKey.Close();
                            }
                        }
                    }
                key.Close();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadSettingsRegistry();
            GetPathsFromRegistry();
            DecodeLibraryLink();

            if ((App.Args != null) && (App.Args.Length > 0) && (App.Args[0] == "/install"))
            {
                if (!System.Reflection.Assembly.GetExecutingAssembly().CodeBase.StartsWith("file:///"))
                    RegisterURLHandler("mmulib", System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace('/', '\\'),true);
                else
                    RegisterURLHandler("mmulib", System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring(8).Replace('/', '\\'),true);
            }
            else
            {
                if (!System.Reflection.Assembly.GetExecutingAssembly().CodeBase.StartsWith("file:///")) //automatic install on first run, if other version is installed it won't change it.
                    RegisterURLHandler("mmulib", System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace('/', '\\'));
                else
                    RegisterURLHandler("mmulib", System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring(8).Replace('/', '\\'));
                AppPath.DataContext = LibraryManagerCollection;
                LibraryManagerCombo.ItemsSource = LibraryManagerCollection;
                for (int i = 0; i < LibraryManagerCollection.Count; i++)
                    if (LibraryManagerCollection[i].index == DefaultLibrary)
                        LibraryManagerCombo.SelectedIndex = i;

                if (LibraryManagerCollection.Count == 0)
                    Error(ErrorType.NoLibraryManagers);
            }
        }

        public void RegisterURLHandler(string protocolName, string applicationPath, bool force=false)
        {
            var alreadyinstalled = false;
            var success = false;
            var mainkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
            if (mainkey != null)
            {
                var keytest = mainkey.OpenSubKey(protocolName);
                if (keytest!=null)
                {
                    alreadyinstalled = true;
                    keytest.Close();
                }
                
                if ((!alreadyinstalled) || force)
                {
                    var key = mainkey.CreateSubKey(protocolName);
                    if (key != null)
                    {
                        key.SetValue("", "MMU library importer");
                        key.SetValue("URL Protocol", "");
                        key.CreateSubKey(@"shell\open\command").SetValue("", "\"" + applicationPath + "\" %1");
                        key.Close();
                        success = true;
                    }
                }
                mainkey.Close();
            }

            if (!alreadyinstalled)
            {
                MMULibraryNameLabel.Content = "MMU Library Linker installation";
                LibraryManagerCombo.Visibility = Visibility.Hidden;
                AppPath.Content = "";
                if (success)
                    MsgLabel.Content = "MMU Library linker is now installed.";
                else
                    MsgLabel.Content = "Error: MMU Library linker could not be installed.";
                OKButton.Visibility = Visibility.Hidden;
                CancelButton.Content = "Close";
            }
        }

        private void DecodeLibraryLink()
        {
            if (App.Args != null)
            {
                if (App.Args.Length > 0)
                {
                    try
                    {
                        var uri = new Uri(App.Args[0]);
                        var query = uri.Query.Split('&');
                        var itemscount = 0;
                        link = new LibraryLink();
                        for (int j = 0; j < query.Length; j++)
                        {
                            var keyval = (j == 0 ? query[j].Substring(1) : query[j]);
                            var a = keyval.IndexOf("=");
                            var key = keyval.Substring(0, a);
                            var value = keyval.Substring(a + 1);
                            if ((key == "name") || (key == "url"))
                            {
                                var decodedBytes = System.Convert.FromBase64String(value);
                                value = System.Text.UTF8Encoding.UTF8.GetString(decodedBytes);
                            }

                            if (key == "name")
                            {
                                link.name = value;
                                itemscount++;
                                MMULibraryNameLabel.Content = "Add library: " + link.name;
                            }
                            if (key == "url")
                            {
                                link.url = value;
                                itemscount++;
                            }
                            if (key == "token")
                            {
                                link.token = value;
                                itemscount++;
                            }
                        }

                        if (itemscount != 3)
                        {
                            link = null;
                            Error(ErrorType.LinkError);
                        }
                    }
                    catch
                    {
                        Error(ErrorType.LinkError);
                    }
                }
                else
                    Error(ErrorType.NoLink);
            }
            else
                Error(ErrorType.NoLink);
        }

        private void Error(ErrorType error)
        {
            if (error == ErrorType.NoLibraryManagers)
            {
                LibraryManagerCombo.Visibility = Visibility.Hidden;
                MsgLabel.Content = "No local MMU library managers are currently available";
                OKButton.Visibility = Visibility.Hidden;
                AppPath.Visibility = Visibility.Hidden;
                CancelButton.Content = "Close";
            }

            if (error == ErrorType.LinkError)
            {
                MMULibraryNameLabel.Content = "";
                MsgLabel.Content = "Error: Invalid MMU library data!";
                LibraryManagerCombo.Visibility = Visibility.Hidden;
                AppPath.Content = "";
                OKButton.Visibility = Visibility.Hidden;
                CancelButton.Content = "Close";
            }

            if (error == ErrorType.NoLink)
            {
                MMULibraryNameLabel.Content = "";
                MsgLabel.Content = "Warning: No MMU library link was used to start this app";
                LibraryManagerCombo.Visibility = Visibility.Hidden;
                AppPath.Content = "";
                OKButton.Visibility = Visibility.Hidden;
                CancelButton.Content = "Close";
            }
        }

        private bool isInLibrary(LibraryLink testLink)
        {
            if (testLink == null)
                return false;
            if (LibraryManagerCombo.SelectedIndex > -1)
            {
                var path = (LibraryManagerCombo.SelectedItem as LibraryManagers).path + "settings\\libraries\\mmu\\";
                if (!Directory.Exists(path))
                    return true;
                var libs = Directory.GetFiles(path);
                for (int i = 0; i < libs.Length; i++)
                    try
                    {
                        if (libs[i].EndsWith(".json"))
                        {
                            var item = Serialization.FromJsonString<LibraryLink>(File.ReadAllText(libs[i]));
                            if ((item.url == testLink.url) && (item.token == testLink.token))
                                return true;
                        }
                    }
                    catch
                    { 
                    }

            }
            return false;
        }

        private void LoadSettingsRegistry()
        {
            DefaultTaskEditorLink.IsChecked = true;
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\MOSIM\\Library Linker");
            if (key != null)
            {
                var value = key.GetValue("Default library");
                if (value != null)
                    if (key.GetValueKind("Default library") == RegistryValueKind.DWord)
                        DefaultLibrary = (int)value;
                value = key.GetValue("Change default Task Editor");
                if (value != null)
                    if (key.GetValueKind("Change default Task Editor") == RegistryValueKind.DWord)
                        DefaultTaskEditorLink.IsChecked = (int)value==1;
                key.Close();
            }
        }

        private void SaveSettingsRegistry()
        {
            if (LibraryManagerCombo.SelectedIndex>-1)
            DefaultLibrary=(LibraryManagerCombo.SelectedItem as LibraryManagers).index;

            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\MOSIM\\Library Linker");
            if (key != null)
            {
                key.SetValue("Change default Task Editor", DefaultTaskEditorLink.IsChecked==true?1:0);
                key.SetValue("Default library", DefaultLibrary);
                key.SetValue("Path", System.Reflection.Assembly.GetExecutingAssembly().Location);
                key.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if ((link!=null) && (LibraryManagerCollection.Count > 0) && (LibraryManagerCombo.SelectedIndex > -1))
            {
                string mmupath = (LibraryManagerCombo.SelectedItem as LibraryManagers).path + "settings\\libraries\\mmu\\";
                if (!Directory.Exists(mmupath))
                    Directory.CreateDirectory(mmupath);
                int n = 1;
                while (System.IO.File.Exists(mmupath + "lib" + n.ToString() + ".json"))
                    n++;
                if (DefaultTaskEditorLink.IsChecked == true)
                {
                    if (System.IO.File.Exists(mmupath + "defaultlib.txt"))
                        System.IO.File.Delete(mmupath + "defaultlib.txt");
                    System.IO.File.WriteAllText(mmupath + "defaultlib.txt", n.ToString());
                }
                System.IO.File.WriteAllText(mmupath + "lib" + n.ToString() + ".json", Serialization.ToJsonString(link));
                
            }
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettingsRegistry();
        }

        private void LibraryManagerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool inLib = false;
            if (LibraryManagerCombo.SelectedIndex > -1)
            {
                AppPath.Content = (LibraryManagerCombo.SelectedItem as LibraryManagers).path;
                if (isInLibrary(link))
                {
                    AppPath.Content += "\r\n - Library is already included in selected library manager";
                    inLib = true;
                }
            }
            else
                AppPath.Content = "";
            OKButton.IsEnabled = (LibraryManagerCombo.SelectedIndex > -1) && !inLib;
        }
    }
}
