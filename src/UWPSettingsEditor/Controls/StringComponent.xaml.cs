using System.Windows.Controls;
using UWPSettingsEditor.Interfaces;
using static UWPSettingsEditor.UWPDeserializer;

namespace UWPSettingsEditor.Controls
{
    /// <summary>
    /// Interaction logic for StringComponent.xaml
    /// </summary>
    public partial class StringComponent : UserControl, IValueDataSet
    {
        public StringComponent()
        {
            InitializeComponent();
        }

        public byte GetValueData()
        {
            throw new System.NotImplementedException();
        }

        public void SetValueData(byte[] data)
        {
            TextBox.Text = GetString(data);
        }
    }
}
