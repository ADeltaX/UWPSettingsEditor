using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using Registry;
using System;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Text;
using static UWPSettingsEditor.NativeMethods;
using System.IO;
using System.Windows.Media;
using System.Windows.Input;

namespace UWPSettingsEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<RootTreeView> t_root = new List<RootTreeView>();
        BitmapSource ComputerBitmap;
        BitmapSource FolderBitmap;

        public MainWindow()
        {
            InitializeComponent();

            ComputerBitmap = ExtractIcon("Shell32.dll", 15, false);
            FolderBitmap = ExtractIcon("Shell32.dll", 3, false);

            t_root.Add(new RootTreeView() { Name = "Computer", ImageSource = ComputerBitmap });            
            treeView.ItemsSource = t_root;
        }

        private bool LoadHive(string filename)
        {
            var root = t_root[0];

            var hive = new RegistryHive(filename);
            if (hive.ParseHive())
            {
                var t_hive = new RegistryHiveTreeView(hive)
                { 
                    Name = Path.GetFileName(filename),
                    ImageSource = FolderBitmap
                };

                root.RegistryHiveTreeViews.Add(t_hive);

                if (hive.Root.SubKeys != null && hive.Root.SubKeys.Count > 0)
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

            var regKeyTreeViews = GetRegistryKeyTreeViews(registryHiveTreeView.AttachedHive, registryHiveTreeView.AttachedHive.Root);

            for (int i = 0; i < regKeyTreeViews.Length; i++)
                registryHiveTreeView.Children.Add(regKeyTreeViews[i]);
        }

        private void LoadSubkey(RegistryKeyTreeView regKey)
        {
            if (regKey.IsDummy)
                regKey.RemoveDummyChild();

            var regKeyTreeViews = GetRegistryKeyTreeViews(regKey.AttachedHive, regKey.AttachedHive.GetKey(regKey.Path));

            for (int i = 0; i < regKeyTreeViews.Length; i++)
                regKey.Children.Add(regKeyTreeViews[i]);
        }

        private RegistryKeyTreeView[] GetRegistryKeyTreeViews(RegistryHive registryHive, Registry.Abstractions.RegistryKey registryKey)
        {
            RegistryKeyTreeView[] registryKeyTreeViews = new RegistryKeyTreeView[registryKey.SubKeys.Count];

            for (int i = 0; i < registryKey.SubKeys.Count; i++)
            {
                var regChild = new RegistryKeyTreeView
                {
                    AttachedHive = registryHive,
                    ImageSource = FolderBitmap,
                    Name = registryKey.SubKeys[i].KeyName,
                    Path = registryKey.SubKeys[i].KeyPath
                };

                if (registryKey.SubKeys[i].SubKeys != null && registryKey.SubKeys[i].SubKeys.Count > 0)
                    regChild.LoadDummyChild();

                registryKeyTreeViews[i] = regChild;
            }

            return registryKeyTreeViews;
        }

        

        private bool IsHiveAlreadyLoaded(string filename)
        {
            foreach (var regHive in t_root[0].RegistryHiveTreeViews)
            {

                var p1 = Path.GetFullPath(filename);
                var p2 = Path.GetFullPath(regHive.AttachedHive.HivePath);

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

            listView.Items.Clear();

            void PopulateListView(Registry.Abstractions.RegistryKey registryKey)
            {
                if (registryKey.Values != null && registryKey.Values.Count > 0)
                {
                    foreach (var keyval in registryKey.Values)
                    {
                        listView.Items.Add(new KeyVal 
                        { 
                            Name = keyval.ValueName, 
                            DataTypeEnum = (DataTypeEnum)keyval.VkRecord.DataTypeRaw, 
                            Data = keyval.ValueDataRaw, 
                            Path = registryKey.KeyPath 
                        });
                    }
                }
            }

            if (e.NewValue is RegistryKeyTreeView registryKeyTreeView)
            {
                var currKey = registryKeyTreeView.AttachedHive.GetKey(registryKeyTreeView.Path);
                PopulateListView(currKey);

            }
            else if (e.NewValue is RegistryHiveTreeView registryHiveTreeView)
            {
                var currKey = registryHiveTreeView.AttachedHive.Root;
                PopulateListView(currKey);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
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
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    var isloaded = IsHiveAlreadyLoaded(file);

                    if (isloaded)
                    {
                        //ToDo: more meaningful warning message
                        MessageBox.Show("This registry hive is already loaded\nFile: " + file, "Hive already loaded", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        try
                        {
                            var res = LoadHive(file);
                            if (res)
                            {
                                //Get the root item (as treeviewitem given the datatemplate) and open it
                                var uwu = treeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                                uwu.IsExpanded = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("This registry hive couldn't be loaded\nDetails: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
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
                    window.ShowDialog();
                }
            }
        }
    }
}
