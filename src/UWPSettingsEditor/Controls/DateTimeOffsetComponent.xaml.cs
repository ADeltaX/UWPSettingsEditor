using System;
using System.ComponentModel;
using UWPSettingsEditor.Interfaces;
using static UWPSettingsEditor.UWPDeserializer;
using static UWPSettingsEditor.UWPSerializer;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPSettingsEditor.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DateTimeOffsetComponent : IValueDataSet
    {
        private DateTimeOffset _timestamp;
        private DateTimeOffset _dateTimeOffset;

        public DateTimeOffsetComponent()
        {
            this.InitializeComponent();
            dateBox.SelectedDate = DateTime.Now;
            _timestamp = DateTimeOffset.Now;
        }

        public bool IsDataValid => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;

        public byte[] GetValueData() 
            => FromDateTimeOffset(new DateTimeOffset(dateBox.SelectedDate, _dateTimeOffset.Offset), _timestamp);

        public void SetValueData(byte[] dataRaw)
        {
            var tmp = MethodHelpers.SplitDataRaw(dataRaw);

            _timestamp = GetDateTimeOffset(tmp.Key);
            _dateTimeOffset = GetDateTimeOffset(tmp.Value);

            dateBox.SelectedDate = _dateTimeOffset.DateTime;
        }
    }
}
