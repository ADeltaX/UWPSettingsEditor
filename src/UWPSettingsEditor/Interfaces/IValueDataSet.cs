namespace UWPSettingsEditor.Interfaces
{
    public interface IValueDataSet
    {
        void SetValueData(byte[] dataRaw);
        byte[] GetValueData();
    }
}
