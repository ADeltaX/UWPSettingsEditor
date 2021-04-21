using UWPSettingsEditor.Interfaces;
using UWPSettingsEditor.Controls;
using static UWPSettingsEditor.UWPDeserializer;
using System.Windows.Interop;
using static UWPSettingsEditor.NativeMethods;
using System;
using System.Runtime.InteropServices;

namespace UWPSettingsEditor
{
    /// <summary>
    /// Interaction logic for EditValueWindow.xaml
    /// </summary>
    public partial class EditValueWindow
    {
        public bool IsKeyValueEdited { get; private set; }
        public bool IsKeyNameEdited { get; private set; }

        readonly IValueDataSet valueDataSet;
        readonly KeyVal currentKeyVal;

        public EditValueWindow(KeyVal val)
        {
            var hndl = new WindowInteropHelper(this).EnsureHandle();
            IntPtr brush = CreateSolidBrush(uint.MinValue);
            SetClassLong(hndl, -10, brush);

            SetWindowTheme(hndl, "DarkMode_Explorer", IntPtr.Zero);

            int attrValue = 1; //TRUE
            DwmSetWindowAttribute(hndl, 20, ref attrValue, Marshal.SizeOf(typeof(int)));

            //DARK THEME ^

            currentKeyVal = val;

            InitializeComponent();
            ValueNameTextBox.Text = val.Name;

            if (val.DataTypeEnum == DataTypeEnum.RegUwpString)
            {
                var componentControl = new StringComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(250);
                ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;
            }
            else if (val.DataTypeEnum == DataTypeEnum.RegUwpBoolean)
            {
                var componentControl = new BooleanComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(154);
            }
            else if ((int)val.DataTypeEnum >= (int)DataTypeEnum.RegUwpByte && 
                        (int)val.DataTypeEnum <= (int)DataTypeEnum.RegUwpDouble) //byte, int16, uint16, int32, uint32, int64, uint64, double, single
            {
                var componentControl = new IntComponent(val.DataTypeEnum);
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(154);
            }
            else if (val.DataTypeEnum == DataTypeEnum.RegUwpDateTimeOffset)
            {
                var componentControl = new DateTimeOffsetComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(154);
            }
            else if (val.DataTypeEnum == DataTypeEnum.RegUwpChar)
            {
                var componentControl = new CharComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(154);
            }
            else if (val.DataTypeEnum == DataTypeEnum.RegUwpGuid)
            {
                var componentControl = new GuidComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(154);
            }
            else if (val.DataTypeEnum == DataTypeEnum.RegUwpRect)
            {
                var componentControl = new RectComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(154);
            }
            else if (val.DataTypeEnum == DataTypeEnum.RegUwpPoint || val.DataTypeEnum == DataTypeEnum.RegUwpSize)
            {
                var componentControl = new PointSizeComponent(val.DataTypeEnum);
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl;
                SetMinHeightAndHeight(154);
            }
            else
            {
                unimplementedTxt.Visibility = System.Windows.Visibility.Visible;
            }

            Title = $"Edit value [type: {val.DataTypeEnum}]";

            valueDataSet?.SetValueData(val.Data);
            if (valueDataSet != null)
                valueDataSet.PropertyChanged += ValueDataSet_PropertyChanged;
        }

        private void ValueDataSet_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDataValid")
            {
                if (valueDataSet.IsDataValid)
                    OkButton.IsEnabled = true;
                else
                    OkButton.IsEnabled = false;
            }
        }

        private void SetMinHeightAndHeight(double targetHeight)
        {
            MinHeight = targetHeight;
            Height = targetHeight;
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public byte[] GetValueData() => valueDataSet?.GetValueData();
        public string GetValueName() => ValueNameTextBox.Text;

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //if edited, return true otherwise false

            if (valueDataSet == null)
            {
                DialogResult = false;
                return;
            }

            var val = valueDataSet.GetValueData();
            IsKeyValueEdited = !MethodHelpers.EqualBytesLongUnrolled(val, currentKeyVal.Data);
            IsKeyNameEdited = ValueNameTextBox.Text != currentKeyVal.Name;

            DialogResult = (IsKeyNameEdited || IsKeyValueEdited);
        }
    }
}
