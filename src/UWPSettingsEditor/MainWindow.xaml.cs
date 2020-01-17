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
using static UWPSettingsEditor.UWPDeserializer;
using System.IO;

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

            //var path = @"C:\Users\ADelt\Documents\DRIVERS";
            //var path = @"C:\Users\ADelt\AppData\Local\Packages\ab6f14c6-abb5-4c2d-adb6-acbf601f6c7a_stwyy6x120xkg\Settings\settings - Copy.dat"; //@"C:\Users\ADelt\AppData\Local\Packages\Microsoft.WindowsStore_8wekyb3d8bbwe\Settings\settings.dat"
            //LoadHive(path);
            
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
                        listView.Items.Add(new KeyVal { Name = keyval.ValueName, Type = GetStringNameFromDataType(keyval.VkRecord.DataTypeRaw), Data = HandleData(keyval.ValueDataRaw, keyval.VkRecord.DataTypeRaw) });
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

        public static IEnumerable<T> SkipLast<T>(IEnumerable<T> source, int n)
        {
            var it = source.GetEnumerator();
            bool hasRemainingItems = false;
            var cache = new Queue<T>(n + 1);

            do
            {
                if (hasRemainingItems = it.MoveNext())
                {
                    cache.Enqueue(it.Current);
                    if (cache.Count > n)
                        yield return cache.Dequeue();
                }
            } while (hasRemainingItems);
        }

        private string HandleData(byte[] dataRaw, uint dataType)
        {
            //TODO: FIX OFFSET & ADD COMPOSITE

            byte[] data = SkipLast(dataRaw, 8).ToArray();
            byte[] timestamp = dataRaw.Skip(data.Length).ToArray();
            var timeStampOffsetless = DateTimeOffset.FromFileTime(BitConverter.ToInt64(timestamp, 0));

            switch ((DataTypeEnum)dataType)
            {
                case DataTypeEnum.RegUwpByte:
                    return GetByte(data).ToString("X2");
                case DataTypeEnum.RegUwpInt16:
                    return GetInt16(data).ToString();
                case DataTypeEnum.RegUwpUint16:
                    return GetUInt16(data).ToString();
                case DataTypeEnum.RegUwpInt32:
                    return GetInt32(data).ToString();
                case DataTypeEnum.RegUwpUint32:
                    return GetUInt32(data).ToString();
                case DataTypeEnum.RegUwpInt64:
                    return GetInt64(data).ToString();
                case DataTypeEnum.RegUwpUint64:
                    return GetUInt64(data).ToString();
                case DataTypeEnum.RegUwpSingle:
                    return GetSingle(data).ToString();
                case DataTypeEnum.RegUwpDouble:
                    return GetDouble(data).ToString();
                case DataTypeEnum.RegUwpChar16:
                    return "'" + GetChar(data).ToString() + "'";
                case DataTypeEnum.RegUwpBoolean:
                    return GetBoolean(data).ToString();
                case DataTypeEnum.RegUwpString:
                    return "\"" + GetString(data) + "\"";
                case DataTypeEnum.RegUwpCompositeValue:
                    return PrettyPrintDictionary(GetCompositeValue(data));
                case DataTypeEnum.RegUwpDateTimeOffset:
                    return GetDateTimeOffset(data).ToString();
                case DataTypeEnum.RegUwpTimeSpan:
                    return GetTimeSpan(data).ToString();
                case DataTypeEnum.RegUwpGuid:
                    return GetGuid(data).ToString();
                case DataTypeEnum.RegUwpPoint:                   
                    return GetPoint(data).ToString();
                case DataTypeEnum.RegUwpSize:
                    return GetSize(data).ToString();
                case DataTypeEnum.RegUwpRect:
                    return GetRect(data).ToString();
                case DataTypeEnum.RegUwpArrayByte:
                    return PrettyPrintArray(GetArray(data.Length, 1, i => GetByte(data, i)));
                    // PrettyPrintArray(data, 1, i => GetByte(data, i).ToString("X2"));
                case DataTypeEnum.RegUwpArrayInt16:
                    return PrettyPrintArray(GetArray(data.Length, 2, i => GetInt16(data, i)));
                case DataTypeEnum.RegUwpArrayUint16:
                    return PrettyPrintArray(GetArray(data.Length, 2, i => GetUInt16(data, i)));
                case DataTypeEnum.RegUwpArrayInt32:
                    return PrettyPrintArray(GetArray(data.Length, 4, i => GetInt32(data, i)));
                case DataTypeEnum.RegUwpArrayUint32:
                    return PrettyPrintArray(GetArray(data.Length, 4, i => GetUInt32(data, i)));
                case DataTypeEnum.RegUwpArrayInt64:
                    return PrettyPrintArray(GetArray(data.Length, 8, i => GetInt64(data, i)));
                case DataTypeEnum.RegUwpArrayUint64:
                    return PrettyPrintArray(GetArray(data.Length, 8, i => GetUInt64(data, i)));
                case DataTypeEnum.RegUwpArraySingle:
                    return PrettyPrintArray(GetArray(data.Length, 4, i => GetSingle(data, i)));
                case DataTypeEnum.RegUwpArrayDouble:
                    return PrettyPrintArray(GetArray(data.Length, 8, i => GetDouble(data, i)));
                case DataTypeEnum.RegUwpArrayChar16:
                    return PrettyPrintArray(GetArray(data.Length, 2, i => GetChar(data, i)));
                case DataTypeEnum.RegUwpArrayBoolean:
                    return PrettyPrintArray(GetArray(data.Length, 1, i => GetBoolean(data, i)));
                case DataTypeEnum.RegUwpArrayString:
                    return PrettyPrintStringArray(data);
                case DataTypeEnum.RegUwpArrayDateTimeOffset:
                    return PrettyPrintArray(GetArray(data.Length, 8, i => GetDateTimeOffset(data, i)));
                case DataTypeEnum.RegUwpArrayTimeSpan:
                    return PrettyPrintArray(GetArray(data.Length, 8, i => GetTimeSpan(data, i)));
                case DataTypeEnum.RegUwpArrayGuid:
                    return PrettyPrintArray(GetArray(data.Length, 16, i => GetGuid(data, i)));
                case DataTypeEnum.RegUwpArrayPoint:
                    return PrettyPrintArray(GetArray(data.Length, 4 * 2, i => GetPoint(data, i)));
                case DataTypeEnum.RegUwpArraySize:
                    return PrettyPrintArray(GetArray(data.Length, 4 * 2, i => GetSize(data, i)));
                case DataTypeEnum.RegUwpArrayRect:
                    return PrettyPrintArray(GetArray(data.Length, 4 * 4, i => GetRect(data, i)));
                case DataTypeEnum.RegUnknown:
                default:
                    return "***-TODO-***";
            }
        }

        private string GetStringNameFromDataType(uint dataType)
        {
            switch ((DataTypeEnum)dataType)
            {
                case DataTypeEnum.RegNone:
                    return "REG_NONE";
                case DataTypeEnum.RegSz:
                    return "REG_SZ";
                case DataTypeEnum.RegExpandSz:
                    return "REG_EXPAND_SZ";
                case DataTypeEnum.RegBinary:
                    return "REG_BINARY";
                case DataTypeEnum.RegDword:
                    return "REG_DWORD";
                case DataTypeEnum.RegDwordBigEndian:
                    return "REG_DWORD_BIG_ENDIAN";
                case DataTypeEnum.RegLink:
                    return "REG_LINK";
                case DataTypeEnum.RegMultiSz:
                    return "REG_MULTI_SZ";
                case DataTypeEnum.RegResourceList:
                    return "REG_RESOURCE_LIST";
                case DataTypeEnum.RegFullResourceDescription:
                    return "REG_FULL_RES_DESC";
                case DataTypeEnum.RegResourceRequirementsList:
                    return "REG_RES_REQ_LIST";
                case DataTypeEnum.RegQword:
                    return "REG_QWORD";
                case DataTypeEnum.RegFileTime:
                    return "REG_FILETIME";
                case DataTypeEnum.RegUwpByte:
                    return "REG_UWP_BYTE";
                case DataTypeEnum.RegUwpInt16:
                    return "REG_UWP_INT16";
                case DataTypeEnum.RegUwpUint16:
                    return "REG_UWP_UINT16";
                case DataTypeEnum.RegUwpInt32:
                    return "REG_UWP_INT32";
                case DataTypeEnum.RegUwpUint32:
                    return "REG_UWP_UINT32";
                case DataTypeEnum.RegUwpInt64:
                    return "REG_UWP_INT64";
                case DataTypeEnum.RegUwpUint64:
                    return "REG_UWP_UINT64";
                case DataTypeEnum.RegUwpSingle:
                    return "REG_UWP_SINGLE";
                case DataTypeEnum.RegUwpDouble:
                    return "REG_UWP_DOUBLE";
                case DataTypeEnum.RegUwpChar16:
                    return "REG_UWP_CHAR16";
                case DataTypeEnum.RegUwpBoolean:
                    return "REG_UWP_BOOLEAN";
                case DataTypeEnum.RegUwpString:
                    return "REG_UWP_STRING";
                case DataTypeEnum.RegUwpCompositeValue:
                    return "REG_UWP_COMPOSITE";
                case DataTypeEnum.RegUwpDateTimeOffset:
                    return "REG_UWP_DATETIMEOFFSET";
                case DataTypeEnum.RegUwpTimeSpan:
                    return "REG_UWP_TIMESPAN";
                case DataTypeEnum.RegUwpGuid:
                    return "REG_UWP_GUID";
                case DataTypeEnum.RegUwpPoint:
                    return "REG_UWP_POINT";
                case DataTypeEnum.RegUwpSize:
                    return "REG_UWP_SIZE";
                case DataTypeEnum.RegUwpRect:
                    return "REG_UWP_RECT";
                case DataTypeEnum.RegUwpArrayByte:
                    return "REG_UWP_ARRAY_BYTE";
                case DataTypeEnum.RegUwpArrayInt16:
                    return "REG_UWP_ARRAY_INT16";
                case DataTypeEnum.RegUwpArrayUint16:
                    return "REG_UWP_ARRAY_UINT16";
                case DataTypeEnum.RegUwpArrayInt32:
                    return "REG_UWP_ARRAY_INT32";
                case DataTypeEnum.RegUwpArrayUint32:
                    return "REG_UWP_ARRAY_UINT32";
                case DataTypeEnum.RegUwpArrayInt64:
                    return "REG_UWP_ARRAY_INT64";
                case DataTypeEnum.RegUwpArrayUint64:
                    return "REG_UWP_ARRAY_UINT64";
                case DataTypeEnum.RegUwpArraySingle:
                    return "REG_UWP_ARRAY_SINGLE";
                case DataTypeEnum.RegUwpArrayDouble:
                    return "REG_UWP_ARRAY_DOUBLE";
                case DataTypeEnum.RegUwpArrayChar16:
                    return "REG_UWP_ARRAY_CHAR16";
                case DataTypeEnum.RegUwpArrayBoolean:
                    return "REG_UWP_ARRAY_BOOLEAN";
                case DataTypeEnum.RegUwpArrayString:
                    return "REG_UWP_ARRAY_STRING";
                case DataTypeEnum.RegUwpArrayDateTimeOffset:
                    return "REG_UWP_ARRAY_DATETIMEOFFSET";
                case DataTypeEnum.RegUwpArrayTimeSpan:
                    return "REG_UWP_ARRAY_TIMESPAN";
                case DataTypeEnum.RegUwpArrayGuid:
                    return "REG_UWP_ARRAY_GUID";
                case DataTypeEnum.RegUwpArrayPoint:
                    return "REG_UWP_ARRAY_POINT";
                case DataTypeEnum.RegUwpArraySize:
                    return "REG_UWP_ARRAY_SIZE";
                case DataTypeEnum.RegUwpArrayRect:
                    return "REG_UWP_ARRAY_RECT";
                case DataTypeEnum.RegUnknown:
                default:
                    return "UNKNOWN (" + dataType + ")";
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
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
                        MessageBox.Show("A hive is already loaded, skipping", "Hive already loaded", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        var res = LoadHive(file);
                    }
                }
            }
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
    }

    public enum DataTypeEnum
    {
        RegNone = 0,
        RegSz = 1,
        RegExpandSz = 2,
        RegBinary = 3,
        RegDword = 4,
        RegDwordBigEndian = 5,
        RegLink = 6,
        RegMultiSz = 7,
        RegResourceList = 8,
        RegFullResourceDescription = 9,
        RegResourceRequirementsList = 10,
        RegQword = 11,
        RegFileTime = 16,

        RegUwpByte = 257,
        RegUwpInt16 = 258,
        RegUwpUint16 = 259,
        RegUwpInt32 = 260,
        RegUwpUint32 = 261,
        RegUwpInt64 = 262,
        RegUwpUint64 = 263,
        RegUwpSingle = 264,
        RegUwpDouble = 265,
        RegUwpChar16 = 266,
        RegUwpBoolean = 267,
        RegUwpString = 268,
        RegUwpCompositeValue = 269,
        RegUwpDateTimeOffset = 270,
        RegUwpTimeSpan = 271,
        RegUwpGuid = 272,
        RegUwpPoint = 273,
        RegUwpSize = 274,
        RegUwpRect = 275,
        RegUwpArrayByte = 276,
        RegUwpArrayInt16 = 277,
        RegUwpArrayUint16 = 278,
        RegUwpArrayInt32 = 279,
        RegUwpArrayUint32 = 280,
        RegUwpArrayInt64 = 281,
        RegUwpArrayUint64 = 282,
        RegUwpArraySingle = 283,
        RegUwpArrayDouble = 284,
        RegUwpArrayChar16 = 285,
        RegUwpArrayBoolean = 286,
        RegUwpArrayString = 287,
        RegUwpArrayDateTimeOffset = 288,
        RegUwpArrayTimeSpan = 289,
        RegUwpArrayGuid = 290,
        RegUwpArrayPoint = 291,
        RegUwpArraySize = 292,
        RegUwpArrayRect = 293,

        RegUnknown = 999
    }

    public class RootTreeView
    {
        public RootTreeView()
        {
            RegistryHiveTreeViews = new ObservableCollection<RegistryHiveTreeView>();
        }

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
        public RegistryHive AttachedHive { get; set; }
        public ObservableCollection<RegistryKeyTreeView> Children { get; set; }
    }

    public class KeyVal
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Data { get; set; }
    }
}
