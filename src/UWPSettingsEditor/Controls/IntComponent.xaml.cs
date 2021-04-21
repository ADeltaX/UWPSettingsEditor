using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using UWPSettingsEditor.Interfaces;
using static UWPSettingsEditor.UWPDeserializer;
using static UWPSettingsEditor.UWPSerializer;

namespace UWPSettingsEditor.Controls
{
    /// <summary>
    /// Interaction logic for IntComponent.xaml
    /// </summary>
    public partial class IntComponent : UserControl, IValueDataSet
    {
        readonly SolidColorBrush valid = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        readonly SolidColorBrush invalid = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00));

        private DateTimeOffset _timestamp;
        private DataTypeEnum _dataType;

        private bool _isDataValid;
        public bool IsDataValid => _isDataValid;

        public IntComponent(DataTypeEnum dataType) 
        {
            _dataType = dataType;
            InitializeComponent();
            _timestamp = DateTimeOffset.Now;

            txBox.Text = "0";
            txBox.TextChanged += TxBox_TextChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public byte[] GetValueData()
        {
            switch (_dataType)
            {
                case DataTypeEnum.RegUwpByte:
                    return FromByte(byte.Parse(txBox.Text), _timestamp);
                case DataTypeEnum.RegUwpInt16:
                    return FromInt16(short.Parse(txBox.Text), _timestamp);
                case DataTypeEnum.RegUwpUint16:
                    return FromUInt16(ushort.Parse(txBox.Text), _timestamp);
                case DataTypeEnum.RegUwpInt32:
                    return FromInt32(int.Parse(txBox.Text), _timestamp);
                case DataTypeEnum.RegUwpUint32:
                    return FromUInt32(uint.Parse(txBox.Text), _timestamp);
                case DataTypeEnum.RegUwpInt64:
                    return FromInt64(long.Parse(txBox.Text), _timestamp);
                case DataTypeEnum.RegUwpUint64:
                    return FromUInt64(ulong.Parse(txBox.Text), _timestamp);
                case DataTypeEnum.RegUwpSingle:
                    return FromSingle(float.Parse(txBox.Text), _timestamp);
                case DataTypeEnum.RegUwpDouble:
                    return FromDouble(double.Parse(txBox.Text), _timestamp);
                default:
                    return null;
            }
        }

        public void SetValueData(byte[] dataRaw)
        {
            var tmp = MethodHelpers.SplitDataRaw(dataRaw);

            _timestamp = GetDateTimeOffset(tmp.Key);

            switch (_dataType)
            {
                case DataTypeEnum.RegUwpByte:
                    txBox.Text = GetByte(tmp.Value).ToString();
                    break;
                case DataTypeEnum.RegUwpInt16:
                    txBox.Text = GetInt16(tmp.Value).ToString();
                    break;
                case DataTypeEnum.RegUwpUint16:
                    txBox.Text = GetUInt16(tmp.Value).ToString();
                    break;
                case DataTypeEnum.RegUwpInt32:
                    txBox.Text = GetInt32(tmp.Value).ToString();
                    break;
                case DataTypeEnum.RegUwpUint32:
                    txBox.Text = GetUInt32(tmp.Value).ToString();
                    break;
                case DataTypeEnum.RegUwpInt64:
                    txBox.Text = GetInt64(tmp.Value).ToString();
                    break;
                case DataTypeEnum.RegUwpUint64:
                    txBox.Text = GetUInt64(tmp.Value).ToString();
                    break;
                case DataTypeEnum.RegUwpSingle:
                    txBox.Text = GetSingle(tmp.Value).ToString("G9");
                    break;
                case DataTypeEnum.RegUwpDouble:
                    txBox.Text = GetDouble(tmp.Value).ToString("G17");
                    break;
                default:
                    break;
            }
        }

        private void FlagDataAsValid()
        {
            _isDataValid = true;
            txBox.Foreground = valid;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDataValid"));
        }

        private void FlagDataAsInvalid()
        {
            _isDataValid = false;
            txBox.Foreground = invalid;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDataValid"));
        }

        private void TxBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            switch (_dataType)
            {
                case DataTypeEnum.RegUwpByte:
                    if (byte.TryParse(txBox.Text, out _))
                        FlagDataAsValid();
                    else
                        FlagDataAsInvalid();
                    break;
                case DataTypeEnum.RegUwpInt16:
                    if (Int16.TryParse(txBox.Text, out _))
                        FlagDataAsValid();
                    else
                        FlagDataAsInvalid();
                    break;
                case DataTypeEnum.RegUwpUint16:
                    if (UInt16.TryParse(txBox.Text, out _))
                        FlagDataAsValid();
                    else
                        FlagDataAsInvalid();
                    break;
                case DataTypeEnum.RegUwpInt32:
                    if (Int32.TryParse(txBox.Text, out _))
                        FlagDataAsValid();
                    else
                        FlagDataAsInvalid();
                    break;
                case DataTypeEnum.RegUwpUint32:
                    if (UInt32.TryParse(txBox.Text, out _))
                        FlagDataAsValid();
                    else
                        FlagDataAsInvalid();
                    break;
                case DataTypeEnum.RegUwpInt64:
                    if (Int64.TryParse(txBox.Text, out _))
                        FlagDataAsValid();
                    else
                        FlagDataAsInvalid();
                    break;
                case DataTypeEnum.RegUwpUint64:
                    if (UInt64.TryParse(txBox.Text, out _))
                        FlagDataAsValid();
                    else
                        FlagDataAsInvalid();
                    break;
                case DataTypeEnum.RegUwpSingle:
                    if (Single.TryParse(txBox.Text, out _))
                        FlagDataAsValid();
                    else
                        FlagDataAsInvalid();
                    break;
                case DataTypeEnum.RegUwpDouble:
                    if (Double.TryParse(txBox.Text, out _))
                        FlagDataAsValid();
                    else
                        FlagDataAsInvalid();
                    break;
                default:
                    break;
            }
        }
    }
}
