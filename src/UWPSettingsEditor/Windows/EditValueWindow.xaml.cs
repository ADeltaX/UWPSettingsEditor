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
            var splitted = MethodHelpers.SplitDataRaw(val.Data);

            InitializeComponent();
            ValueNameTextBox.Text = val.Name;

            if (val.DataTypeEnum == DataTypeEnum.RegUwpString)
            {
                var componentControl = new StringComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl as IValueDataSet;

                valueDataSet.SetValueData(splitted.Key);
            }
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //Check for modifications, if modified, return true otherwise false
            DialogResult = true;
        }
    }
}
