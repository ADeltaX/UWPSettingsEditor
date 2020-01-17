using System.Windows.Controls;
using UWPSettingsEditor.Interfaces;

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
            throw new System.NotImplementedException();
        }
    }
}
