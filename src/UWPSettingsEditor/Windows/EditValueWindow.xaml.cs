using UWPSettingsEditor.Interfaces;
using UWPSettingsEditor.Controls;
using static UWPSettingsEditor.UWPDeserializer;

namespace UWPSettingsEditor
{
    /// <summary>
    /// Interaction logic for EditValueWindow.xaml
    /// </summary>
    public partial class EditValueWindow
    {
        readonly IValueDataSet valueDataSet;
        KeyVal currentKeyVal;

        public EditValueWindow(KeyVal val)
        {
            currentKeyVal = val;

            InitializeComponent();
            ValueNameTextBox.Text = val.Name;

            if (val.DataTypeEnum == DataTypeEnum.RegUwpString)
            {
                var componentControl = new StringComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl as IValueDataSet;
                SetMinHeightAndHeight(250);
                ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;
            }
            else if (val.DataTypeEnum == DataTypeEnum.RegUwpBoolean)
            {
                var componentControl = new BooleanComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl as IValueDataSet;
                SetMinHeightAndHeight(154);
            }
            else if ((int)val.DataTypeEnum >= 257 && (int)val.DataTypeEnum <= 263) //byte, int16, uint16, int32, uint32, int64, uint64
            {
                var componentControl = new IntComponent(val.DataTypeEnum);
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl as IValueDataSet;
                SetMinHeightAndHeight(154);
            }

            valueDataSet?.SetValueData(val.Data);
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

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //if edited, return true otherwise false

            var val = valueDataSet.GetValueData();
            var isSame = MethodHelpers.EqualBytesLongUnrolled(val, currentKeyVal.Data);
            DialogResult = !isSame;
        }
    }
}
