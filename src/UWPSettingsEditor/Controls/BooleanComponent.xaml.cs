using System;
using System.ComponentModel;
using System.Windows.Controls;
using UWPSettingsEditor.Interfaces;
using static UWPSettingsEditor.UWPDeserializer;
using static UWPSettingsEditor.UWPSerializer;

namespace UWPSettingsEditor.Controls
{
    /// <summary>
    /// Interaction logic for BooleanComponent.xaml
    /// </summary>
    public partial class BooleanComponent : UserControl, IValueDataSet
    {
        private DateTimeOffset _timestamp;

        public BooleanComponent()
        {
            InitializeComponent();
            cmBox.SelectedIndex = 0;
            _timestamp = DateTimeOffset.Now;
        }

        public bool IsDataValid => true;

        public event PropertyChangedEventHandler PropertyChanged;

        public byte[] GetValueData() => FromBoolean(bool.Parse((cmBox.SelectedItem as ComboBoxItem).Tag.ToString()), _timestamp);

        public void SetValueData(byte[] dataRaw)
        {
            var tmp = MethodHelpers.SplitDataRaw(dataRaw);

            _timestamp = GetDateTimeOffset(tmp.Key);
            var @bool = GetBoolean(tmp.Value);

            if (@bool)
                cmBox.SelectedIndex = 0;
            else
                cmBox.SelectedIndex = 1;
        }
    }
}
