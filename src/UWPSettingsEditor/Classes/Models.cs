using DiscUtils.Registry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace UWPSettingsEditor
{

    public class RootTreeView
    {
        public RootTreeView()
        {
            RegistryHiveTreeViews = new ObservableCollection<RegistryHiveTreeView>();
        }
        public bool IsExpanded { get; set; }
        public BitmapSource ImageSource { get; set; }
        public string Name { get; set; }
        public ObservableCollection<RegistryHiveTreeView> RegistryHiveTreeViews { get; set; }
    }

    public class RegistryHiveTreeView
    {
        public RegistryHiveTreeView(RegistryHive registryHive)
        {
            AttachedHive = registryHive;
            Children = new ObservableCollection<RegistryKeyTreeView>();
        }

        public void LoadDummyChild()
        {
            Children.Add(new RegistryKeyTreeView());
            IsDummy = true;
        }
        public void RemoveDummyChild()
        {
            Children.Clear();
            IsDummy = false;
        }

        public BitmapSource ImageSource { get; set; }
        public bool IsDummy { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public RegistryHive AttachedHive { get; set; }
        public ObservableCollection<RegistryKeyTreeView> Children { get; set; }
    }

    public class RegistryKeyTreeView
    {
        public RegistryKeyTreeView()
        {
            Children = new ObservableCollection<RegistryKeyTreeView>();
        }

        public void LoadDummyChild()
        {
            Children.Add(new RegistryKeyTreeView());
            IsDummy = true;
        }
        public void RemoveDummyChild()
        {
            Children.Clear();
            IsDummy = false;
        }

        public BitmapSource ImageSource { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public bool IsDummy { get; set; }
        public RegistryHiveTreeView Root { get; set; }
        public RegistryHive AttachedHive { get; set; }
        public ObservableCollection<RegistryKeyTreeView> Children { get; set; }
    }

    public class KeyVal : INotifyPropertyChanged
    {
        private byte[] data;

        public string Path { get; set; }
        public string Name { get; set; }
        public DataTypeEnum DataTypeEnum { get; set; }
        public RegistryHive Hive { get; set; }
        public byte[] Data { get => data; set { data = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Data")); } }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
