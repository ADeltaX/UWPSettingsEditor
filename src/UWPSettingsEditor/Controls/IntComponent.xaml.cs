using System;
using System.Windows.Controls;
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
        private DateTimeOffset _timestamp;
        private DataTypeEnum _dataType;

        public IntComponent(DataTypeEnum dataType) 
        {
            _dataType = dataType;
            InitializeComponent(); 
        }

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
                default:
                    break;
            }

        }
    }
}
