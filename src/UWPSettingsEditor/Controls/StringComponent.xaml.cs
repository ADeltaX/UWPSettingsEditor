﻿using System;
using System.Windows.Controls;
using UWPSettingsEditor.Interfaces;
using static UWPSettingsEditor.UWPDeserializer;
using static UWPSettingsEditor.UWPSerializer;

namespace UWPSettingsEditor.Controls
{
    /// <summary>
    /// Interaction logic for StringComponent.xaml
    /// </summary>
    public partial class StringComponent : UserControl, IValueDataSet
    {
        private DateTimeOffset _timestamp;

        public StringComponent() => InitializeComponent();

        public byte[] GetValueData() => FromString(TextBox.Text, _timestamp);

        public void SetValueData(byte[] dataRaw)
        {
            var tmp = MethodHelpers.SplitDataRaw(dataRaw);

            _timestamp = GetDateTimeOffset(tmp.Key);
            TextBox.Text = GetString(tmp.Value);
        }
    }
}
