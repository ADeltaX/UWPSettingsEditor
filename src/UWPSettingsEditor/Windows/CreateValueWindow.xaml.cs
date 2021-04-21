using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UWPSettingsEditor.Controls;
using UWPSettingsEditor.Interfaces;
using System.Windows.Interop;
using static UWPSettingsEditor.NativeMethods;
using System;
using System.Runtime.InteropServices;

namespace UWPSettingsEditor.Windows
{
    /// <summary>
    /// Interaction logic for CreateValueWindow.xaml
    /// </summary>
    public partial class CreateValueWindow : Window
    {
        IValueDataSet valueDataSet;
        readonly string _path;

        List<DataTypeEnum> registryValueTypes = new List<DataTypeEnum>()
        {
            DataTypeEnum.RegUwpBoolean,
            DataTypeEnum.RegUwpByte,
            DataTypeEnum.RegUwpChar,
            DataTypeEnum.RegUwpDateTimeOffset,
            DataTypeEnum.RegUwpDouble,
            DataTypeEnum.RegUwpGuid,
            DataTypeEnum.RegUwpInt16,
            DataTypeEnum.RegUwpInt32,
            DataTypeEnum.RegUwpInt64,
            DataTypeEnum.RegUwpPoint,
            DataTypeEnum.RegUwpRect,
            DataTypeEnum.RegUwpSingle,
            DataTypeEnum.RegUwpSize,
            DataTypeEnum.RegUwpString,
            DataTypeEnum.RegUwpUint16,
            DataTypeEnum.RegUwpUint32,
            DataTypeEnum.RegUwpUint64
        };

        public CreateValueWindow(string path)
        {
            _path = path;
            InitializeComponent();
            ValueTypeComboBox.ItemsSource = registryValueTypes;
            ValueTypeComboBox.SelectedIndex = 0;
        }

        private void ValueTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var hndl = new WindowInteropHelper(this).EnsureHandle();
            IntPtr brush = CreateSolidBrush(uint.MinValue);
            SetClassLong(hndl, -10, brush);

            SetWindowTheme(hndl, "DarkMode_Explorer", IntPtr.Zero);

            int attrValue = 1; //TRUE
            DwmSetWindowAttribute(hndl, 20, ref attrValue, Marshal.SizeOf(typeof(int)));

            //DARK THEME ^

            var val = (DataTypeEnum)ValueTypeComboBox.SelectedItem;

            if (valueDataSet != null)
            {
                valueDataSet.PropertyChanged -= ValueDataSet_PropertyChanged;
                valueDataSet = null;
                ContainerGrid.Children.Clear();

                //TODO: additional check when "if key !exists" is implemented
                OkButton.IsEnabled = true;
            }

            if (val == DataTypeEnum.RegUwpString)
            {
                var componentControl = new StringComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(250);
                ResizeMode = ResizeMode.CanResizeWithGrip;
            }
            else if (val == DataTypeEnum.RegUwpBoolean)
            {
                var componentControl = new BooleanComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(210);
                ResizeMode = ResizeMode.NoResize;
            }
            else if ((int)val >= (int)DataTypeEnum.RegUwpByte &&
                        (int)val <= (int)DataTypeEnum.RegUwpDouble) //byte, int16, uint16, int32, uint32, int64, uint64, double, single
            {
                var componentControl = new IntComponent(val);
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(210);
                ResizeMode = ResizeMode.NoResize;
            }
            else if (val == DataTypeEnum.RegUwpDateTimeOffset)
            {
                var componentControl = new DateTimeOffsetComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(210);
                ResizeMode = ResizeMode.NoResize;
            }
            else if (val == DataTypeEnum.RegUwpChar)
            {
                var componentControl = new CharComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(210);
                ResizeMode = ResizeMode.NoResize;
            }
            else if (val == DataTypeEnum.RegUwpGuid)
            {
                var componentControl = new GuidComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(210);
                ResizeMode = ResizeMode.NoResize;
            }
            else if (val == DataTypeEnum.RegUwpRect)
            {
                var componentControl = new RectComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(210);
                ResizeMode = ResizeMode.NoResize;
            }
            else if (val == DataTypeEnum.RegUwpPoint || val == DataTypeEnum.RegUwpSize)
            {
                var componentControl = new PointSizeComponent(val);
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(210);
                ResizeMode = ResizeMode.NoResize;
            }

            valueDataSet.PropertyChanged += ValueDataSet_PropertyChanged;
        }

        public DataTypeEnum GetValueType() => (DataTypeEnum)ValueTypeComboBox.SelectedItem;
        public byte[] GetValueData() => valueDataSet?.GetValueData();
        public string GetValueName() => ValueNameTextBox.Text;

        private void ValueDataSet_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.PropertyName == "IsDataValid")
            {
                OkButton.IsEnabled = valueDataSet.IsDataValid;
            }
        }

        private void SetMinHeightAndHeight(double targetHeight)
        {
            MinHeight = targetHeight;
            Height = targetHeight;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: check if the value name isn't actually used
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
