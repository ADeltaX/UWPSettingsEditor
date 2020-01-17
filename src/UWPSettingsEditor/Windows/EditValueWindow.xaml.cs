using UWPSettingsEditor.Interfaces;
using UWPSettingsEditor.Controls;

namespace UWPSettingsEditor
{
    /// <summary>
    /// Interaction logic for EditValueWindow.xaml
    /// </summary>
    public partial class EditValueWindow
    {
        readonly IValueDataSet valueDataSet;

        public EditValueWindow(KeyVal val)
        {
            InitializeComponent();
            if (val.DataTypeEnum == DataTypeEnum.RegUwpString)
            {
                var componentControl = new StringComponent();
                ContainerGrid.Children.Add(componentControl);
                valueDataSet = componentControl as IValueDataSet;

                valueDataSet.SetValueData(val.Data);
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
