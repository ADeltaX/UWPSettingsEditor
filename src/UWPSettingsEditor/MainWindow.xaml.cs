using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System;
using System.Windows.Media.Imaging;
using static UWPSettingsEditor.NativeMethods;
using System.IO;
using System.Windows.Input;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Media;
using System.Linq;
using DiscUtils.Registry;
using RegistryKey = DiscUtils.Registry.RegistryKey;
using System.Windows.Data;
using System.Collections.ObjectModel;
using UWPSettingsEditor.Windows;

namespace UWPSettingsEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //TODO: localize messages

        private readonly List<RootTreeView> treeRoot = new List<RootTreeView>();
        private readonly ObservableCollection<KeyVal> keyVals = new ObservableCollection<KeyVal>();
        private readonly BitmapSource computerBitmap;
        private readonly BitmapSource folderBitmap;

        public MainWindow()
        {
            InitializeComponent();

            //var uwu = new DiscUtils.Registry.RegistryHive(new FileStream(@"C:\Users\ADelt\Documents\settings.dat", FileMode.Open));
            //var e = uwu.Root.OpenSubKey("LocalState").GetValueType("hasUnreadValuesBOOL");
            WindowState = WindowState.Minimized;

            computerBitmap = ExtractIcon("Shell32.dll", 15, false);
            folderBitmap = ExtractIcon("Shell32.dll", 3, false);

            selectedIconImage.Source = computerBitmap;
            treeRoot.Add(new RootTreeView() { Name = "Computer", ImageSource = computerBitmap });
            treeView.ItemsSource = treeRoot;
            listView.ItemsSource = keyVals;

            //Darkmode

            var hndl = new WindowInteropHelper(this).EnsureHandle();
            IntPtr brush = CreateSolidBrush(uint.MinValue);
            SetClassLong(hndl, -10, brush);

            SetWindowTheme(hndl, "DarkMode_Explorer", IntPtr.Zero);

            int attrValue = 1; //TRUE
            DwmSetWindowAttribute(hndl, 20, ref attrValue, Marshal.SizeOf(typeof(int)));

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Activate();
            WindowState = WindowState.Normal;
        }

        private void LoadAndVerifyHives(string[] filenames)
        {
            //TODO: background thread processing.

            bool isMoreThanOneFile = filenames.Length > 1;

            StringBuilder aggregatedMessages = null;

            if (isMoreThanOneFile)
                aggregatedMessages = new StringBuilder();

            foreach (var file in filenames)
            {
                var isloaded = IsHiveAlreadyLoaded(file);

                if (isloaded)
                {
                    //ToDo: more meaningful warning message
                    if (isMoreThanOneFile)
                        aggregatedMessages.AppendLine("This registry hive is already loaded\nFile: " + file + "\n");
                    else
                        MessageBox.Show("This registry hive is already loaded\nFile: " + file, "Hive already loaded", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    try
                    {
                        var res = LoadHive(file, true); //TEST
                        if (res)
                        {
                            //Get the root item (as treeviewitem given the datatemplate) and open it
                            var uwu = treeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                            uwu.IsExpanded = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (isMoreThanOneFile)
                            aggregatedMessages.AppendLine("This registry hive couldn't be loaded\nDetails: " + ex.Message + "\n");
                        else
                            MessageBox.Show("This registry hive couldn't be loaded\nDetails: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            if (isMoreThanOneFile && aggregatedMessages.Length > 0)
                MessageBox.Show(aggregatedMessages.ToString(), "Multiple messages", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private bool LoadHive(string filename, bool onDemand)
        {
            var root = treeRoot[0];

            var hive = new RegistryHive(new FileStream(filename, FileMode.Open));

            var rootKey = hive.Root;

            if (rootKey != null)
            {
                var t_hive = new RegistryHiveTreeView(hive)
                {
                    Name = Path.GetFileName(filename),
                    FilePath = filename,
                    ImageSource = folderBitmap
                };

                root.RegistryHiveTreeViews.Add(t_hive);

                if (rootKey.SubKeys != null && rootKey.GetSubKeyNames().Length > 0)
                    t_hive.LoadDummyChild();

                return true;
            }
            else
            {
                return false;
            }
        }

        private void LoadSubkeyRoot(RegistryHiveTreeView registryHiveTreeView)
        {
            if (registryHiveTreeView.IsDummy)
                registryHiveTreeView.RemoveDummyChild();

            var regKeyTreeViews = GetRegistryKeyTreeViews(registryHiveTreeView.AttachedHive, "\\");

            for (int i = 0; i < regKeyTreeViews.Length; i++)
            {
                regKeyTreeViews[i].Root = registryHiveTreeView;
                registryHiveTreeView.Children.Add(regKeyTreeViews[i]);
            }
        }

        private void LoadSubkey(RegistryKeyTreeView regKey)
        {
            if (regKey.IsDummy)
                regKey.RemoveDummyChild();

            var regKeyTreeViews = GetRegistryKeyTreeViews(regKey.AttachedHive, regKey.Path);

            for (int i = 0; i < regKeyTreeViews.Length; i++)
            {
                regKeyTreeViews[i].Root = regKey.Root;
                regKey.Children.Add(regKeyTreeViews[i]);
            }
        }

        private RegistryKeyTreeView[] GetRegistryKeyTreeViews(RegistryHive registryHive, string registryKeyPath)
        {
            var subKey = (registryKeyPath == "\\") ? registryHive.Root : registryHive.Root.OpenSubKey(registryKeyPath);

            RegistryKeyTreeView[] registryKeyTreeViews = new RegistryKeyTreeView[subKey.SubKeyCount];

            var i = 0;
            foreach (var subKeyDet in subKey.SubKeys)
            {
                var regChild = new RegistryKeyTreeView
                {
                    AttachedHive = registryHive,
                    ImageSource = folderBitmap,
                    Name = subKeyDet.Name.Split('\\').Last(), //TODO
                    Path = subKeyDet.Name
                };

                if (subKeyDet.SubKeyCount > 0)
                    regChild.LoadDummyChild();

                registryKeyTreeViews[i] = regChild;

                i++;
            }

            return registryKeyTreeViews;
        }

        private bool IsHiveAlreadyLoaded(string filename)
        {
            foreach (var regHive in treeRoot[0].RegistryHiveTreeViews)
            {
                var p1 = Path.GetFullPath(filename);
                var p2 = Path.GetFullPath(regHive.FilePath);

                if (string.Equals(p1, p2, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void treeView_Expanded(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem tvi)
            {
                if (tvi.DataContext is RegistryHiveTreeView registryHiveTreeView)
                {
                    if (registryHiveTreeView.IsDummy)
                    {
                        LoadSubkeyRoot(registryHiveTreeView);
                    }
                }
                else if (tvi.DataContext is RegistryKeyTreeView registryKeyTreeView)
                {
                    if (registryKeyTreeView.IsDummy)
                    {
                        LoadSubkey(registryKeyTreeView);
                    }
                }
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == e.OldValue)
                return;

            keyVals.Clear();

            void PopulateListView(RegistryKey registryKey, RegistryHive registryBase)
            {
                if (registryKey.ValueCount > 0)
                {
                    foreach (var keyval in registryKey.GetValueNames())
                    {
                        keyVals.Add(new KeyVal 
                        { 
                            Name = keyval, 
                            DataTypeEnum = (DataTypeEnum)registryKey.GetValueType(keyval), 
                            Data = registryKey.GetValueRaw(keyval), 
                            Path = registryKey.Name,
                            Hive = registryBase
                        });
                    }
                }
            }

            if (e.NewValue is RegistryKeyTreeView registryKeyTreeView)
            {
                RegistryKey currKey = registryKeyTreeView.AttachedHive.Root.OpenSubKey(registryKeyTreeView.Path);

                if (currKey == null)
                    Debugger.Break();

                currentPathTxt.Text = "Computer" + "\\" + registryKeyTreeView.Root.Name + "\\" + registryKeyTreeView.Path;
                selectedIconImage.Source = registryKeyTreeView.ImageSource;
                PopulateListView(currKey, registryKeyTreeView.AttachedHive);

            }
            else if (e.NewValue is RegistryHiveTreeView registryHiveTreeView)
            {

                RegistryKey currKey = registryHiveTreeView.AttachedHive.Root;

                currentPathTxt.Text = "Computer" + "\\" + registryHiveTreeView.Name;
                selectedIconImage.Source = new BitmapImage(new Uri("Assets/RegistryIcon.png", UriKind.Relative));
                PopulateListView(currKey, registryHiveTreeView.AttachedHive);
            }
            else
            {
                currentPathTxt.Text = "Computer";
                selectedIconImage.Source = computerBitmap;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var sel = treeView.SelectedItem;

            if (sel == null)
                return;

            //TODO: IMPLEMENT FOR HIVE TOO

            if (sel is RegistryHiveTreeView)
                return;

            if (sel is RegistryKeyTreeView key)
            {
                CreateValueWindow cvw = new CreateValueWindow(key.Path);
                cvw.Owner = this;
                var result = cvw.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var subKey = key.AttachedHive.Root.OpenSubKey(key.Path);

                    if (subKey.GetValue(cvw.GetValueName()) == null)
                    {
                        subKey.SetValueRaw(cvw.GetValueName(), cvw.GetValueData(), (RegistryValueType)cvw.GetValueType());
                    }
                    else
                    {
                        MessageBox.Show($"A key named {cvw.GetValueName()} already exists. Value not created", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    
                }
            }

            //
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open settings.dat",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
                LoadAndVerifyHives(openFileDialog.FileNames);
        }

        private void treeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = (e.OriginalSource as DependencyObject).VisualUpwardSearch<TreeViewItem>();

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ListViewItem listViewItem = (e.OriginalSource as DependencyObject).VisualUpwardSearch<ListViewItem>();
                if (listViewItem != null)
                {
                    EditValueWindow window = new EditValueWindow((KeyVal)listViewItem.DataContext);
                    window.Owner = this;
                    var isEdited = window.ShowDialog();

                    if (isEdited.HasValue && isEdited.Value)
                    {
                        if (window.IsKeyValueEdited)
                        {
                            var dataContext = ((KeyVal)listViewItem.DataContext);
                            var p = dataContext.Path;
                            var h = dataContext.Hive.Root.OpenSubKey(p);

                            var dataByteArray = window.GetValueData();

                            h.SetValue(dataContext.Name, dataByteArray, (RegistryValueType)dataContext.DataTypeEnum);
                            dataContext.Data = dataByteArray;

                            //TODO: ADD A HISTORY (?)
                        }
                    }
                }
            }
        }

        #region Drag & drop

        private void treeView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadAndVerifyHives(files);
            }
        }

        private void treeView_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

        private void listView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //ListView listView = sender as ListView;
            //GridView gView = listView.View as GridView;
        }

        private void currentPathTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var src = e.OriginalSource;
        }

        private void currentPathTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            UpdateSuggestedList((sender as TextBox).Text + e.Text);
        }

        private void UpdateSuggestedList(string text)
        {
            //TODO: BROKEN

            var filter = text;

            if (filter.StartsWith(@"Computer\", StringComparison.InvariantCultureIgnoreCase))
                filter = filter.Remove(0, @"Computer\".Length);

            listViewSuggestion.ItemsSource = null;

            if (string.IsNullOrEmpty(filter))
            {
                IList<string> str = new List<string>();

                foreach (var x in treeRoot[0].RegistryHiveTreeViews)
                    str.Add(@"Computer\" + x.Name);

                listViewSuggestion.ItemsSource = str;
            }
            else
            {
                var splitted = filter.Split(new char[] { '\\' }, 2);
                if (splitted.Length >= 2)
                {
                    splitted[1] = splitted[1].Trim();

                    List<RegistryHive> registries = new List<RegistryHive>();

                    foreach (var x in treeRoot[0].RegistryHiveTreeViews)
                    {
                        if (x.Name.Equals(splitted[0], StringComparison.InvariantCultureIgnoreCase))
                            registries.Add(x.AttachedHive);
                    }

                    IList<string> str = new List<string>();
                    var subSplitted = splitted[1].Split(new char[] { '\\' });

                    var subIncorporated = string.Join("\\", subSplitted, 0, subSplitted.Length - 1);

                    foreach (var reg in registries)
                    {

                        var key = reg.Root.OpenSubKey(splitted[1] == "" ? "\\" : subIncorporated);
                        if (key != null)
                            foreach (var subKey in key.SubKeys)
                                str.Add(@"Computer\" + splitted[0] + subKey.Name); //TODO

                    }

                    if (subSplitted.Length >= 1 && !string.IsNullOrEmpty(subSplitted[0]))
                        str = str.Where(x => x.StartsWith(@"Computer\" + splitted[0] + "\\" + splitted[1], StringComparison.InvariantCultureIgnoreCase)).ToList();

                    listViewSuggestion.ItemsSource = str;
                }
                else
                {
                    IList<string> str = new List<string>();

                    foreach (var x in treeRoot[0].RegistryHiveTreeViews)
                    {
                        if (x.Name.StartsWith(splitted[0], StringComparison.InvariantCultureIgnoreCase))
                            str.Add(@"Computer\" + x.Name);
                    }

                    listViewSuggestion.ItemsSource = str;
                }
            }
        }

        private void currentPathTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!listViewSuggestion.IsFocused)
                listViewSuggestion.Visibility = Visibility.Hidden;
        }

        private void currentPathTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateSuggestedList((sender as TextBox).Text);
            listViewSuggestion.Visibility = Visibility.Visible;
        }

        private void currentPathTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (!listView.Focus())
                    treeView.Focus();

                listViewSuggestion.Visibility = Visibility.Hidden;
                return;
            }
            else if (e.Key == Key.Down)
            {
                if (listViewSuggestion.Items.Count > 0 && listViewSuggestion.SelectedIndex + 1 < listViewSuggestion.Items.Count)
                    listViewSuggestion.SelectedIndex++;

                //currentPathTxt.Text

                return;
            }

            UpdateSuggestedList((sender as TextBox).Text);
        }

        private void currentPathTxt_KeyUp(object sender, KeyEventArgs e)
        {
            //UpdateSuggestedList((sender as TextBox).Text);
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentPathTxt.IsFocused)
            {
                if (!listView.Focus())
                    treeView.Focus();

                listViewSuggestion.Visibility = Visibility.Hidden;
            }
        }
    }
}
