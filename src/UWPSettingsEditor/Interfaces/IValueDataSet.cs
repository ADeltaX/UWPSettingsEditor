using System.ComponentModel;

namespace UWPSettingsEditor.Interfaces
{
    public interface IValueDataSet : INotifyPropertyChanged
    {
        void SetValueData(byte[] dataRaw);
        byte[] GetValueData();
        bool IsDataValid { get; }
    }
}
