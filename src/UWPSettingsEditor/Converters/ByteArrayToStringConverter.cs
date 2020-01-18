using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using static UWPSettingsEditor.UWPDeserializer;

namespace UWPSettingsEditor.Converters
{
    public class ByteArrayToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] data = (byte[])values[0];
            DataTypeEnum dataType = (DataTypeEnum)values[1];

            return DisplayData(data, dataType);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string DisplayData(byte[] dataRaw, DataTypeEnum dataType)
        {
            if ((int)dataType > 256 && (int)dataType < 294)
            {
                var splittedDataRaw = MethodHelpers.SplitDataRaw(dataRaw);
                var data = splittedDataRaw.Key;
                //var timestamp = DateTimeOffset.FromFileTime(BitConverter.ToInt64(timestamp, 0));
                //var timeStamp = ;

                switch (dataType)
                {
                    case DataTypeEnum.RegUwpByte:
                        return GetByte(data).ToString();
                    case DataTypeEnum.RegUwpInt16:
                        return GetInt16(data).ToString();
                    case DataTypeEnum.RegUwpUint16:
                        return GetUInt16(data).ToString();
                    case DataTypeEnum.RegUwpInt32:
                        return GetInt32(data).ToString();
                    case DataTypeEnum.RegUwpUint32:
                        return GetUInt32(data).ToString();
                    case DataTypeEnum.RegUwpInt64:
                        return GetInt64(data).ToString();
                    case DataTypeEnum.RegUwpUint64:
                        return GetUInt64(data).ToString();
                    case DataTypeEnum.RegUwpSingle:
                        return GetSingle(data).ToString();
                    case DataTypeEnum.RegUwpDouble:
                        return GetDouble(data).ToString();
                    case DataTypeEnum.RegUwpChar16:
                        return "'" + GetChar(data).ToString() + "'";
                    case DataTypeEnum.RegUwpBoolean:
                        return GetBoolean(data).ToString();
                    case DataTypeEnum.RegUwpString:
                        return "\"" + MethodHelpers.ReplaceMultilineWithSymbols(GetString(data)) + "\"";
                    case DataTypeEnum.RegUwpCompositeValue:
                        return PrettyPrintDictionary(GetCompositeValue(data));
                    case DataTypeEnum.RegUwpDateTimeOffset:
                        return GetDateTimeOffset(data).ToString();
                    case DataTypeEnum.RegUwpTimeSpan:
                        return GetTimeSpan(data).ToString();
                    case DataTypeEnum.RegUwpGuid:
                        return GetGuid(data).ToString();
                    case DataTypeEnum.RegUwpPoint:
                        return GetPoint(data).ToString();
                    case DataTypeEnum.RegUwpSize:
                        return GetSize(data).ToString();
                    case DataTypeEnum.RegUwpRect:
                        return GetRect(data).ToString();
                    case DataTypeEnum.RegUwpArrayByte:
                        return PrettyPrintArray(GetArray(data.Length, 1, i => GetByte(data, i)));
                    // PrettyPrintArray(data, 1, i => GetByte(data, i).ToString("X2"));
                    case DataTypeEnum.RegUwpArrayInt16:
                        return PrettyPrintArray(GetArray(data.Length, 2, i => GetInt16(data, i)));
                    case DataTypeEnum.RegUwpArrayUint16:
                        return PrettyPrintArray(GetArray(data.Length, 2, i => GetUInt16(data, i)));
                    case DataTypeEnum.RegUwpArrayInt32:
                        return PrettyPrintArray(GetArray(data.Length, 4, i => GetInt32(data, i)));
                    case DataTypeEnum.RegUwpArrayUint32:
                        return PrettyPrintArray(GetArray(data.Length, 4, i => GetUInt32(data, i)));
                    case DataTypeEnum.RegUwpArrayInt64:
                        return PrettyPrintArray(GetArray(data.Length, 8, i => GetInt64(data, i)));
                    case DataTypeEnum.RegUwpArrayUint64:
                        return PrettyPrintArray(GetArray(data.Length, 8, i => GetUInt64(data, i)));
                    case DataTypeEnum.RegUwpArraySingle:
                        return PrettyPrintArray(GetArray(data.Length, 4, i => GetSingle(data, i)));
                    case DataTypeEnum.RegUwpArrayDouble:
                        return PrettyPrintArray(GetArray(data.Length, 8, i => GetDouble(data, i)));
                    case DataTypeEnum.RegUwpArrayChar16:
                        return PrettyPrintArray(GetArray(data.Length, 2, i => GetChar(data, i)), "'");
                    case DataTypeEnum.RegUwpArrayBoolean:
                        return PrettyPrintArray(GetArray(data.Length, 1, i => GetBoolean(data, i)));
                    case DataTypeEnum.RegUwpArrayString:
                        return PrettyPrintStringArrayFromRaw(data);
                    case DataTypeEnum.RegUwpArrayDateTimeOffset:
                        return PrettyPrintArray(GetArray(data.Length, 8, i => GetDateTimeOffset(data, i)));
                    case DataTypeEnum.RegUwpArrayTimeSpan:
                        return PrettyPrintArray(GetArray(data.Length, 8, i => GetTimeSpan(data, i)));
                    case DataTypeEnum.RegUwpArrayGuid:
                        return PrettyPrintArray(GetArray(data.Length, 16, i => GetGuid(data, i)));
                    case DataTypeEnum.RegUwpArrayPoint:
                        return PrettyPrintArray(GetArray(data.Length, 4 * 2, i => GetPoint(data, i)));
                    case DataTypeEnum.RegUwpArraySize:
                        return PrettyPrintArray(GetArray(data.Length, 4 * 2, i => GetSize(data, i)));
                    case DataTypeEnum.RegUwpArrayRect:
                        return PrettyPrintArray(GetArray(data.Length, 4 * 4, i => GetRect(data, i)));
                    default:
                        return "***-UNPARSED-***";
                }
            }
            else
            {
                switch (dataType)
                {
                    case DataTypeEnum.RegSz:
                    case DataTypeEnum.RegExpandSz:
                        break;
                    case DataTypeEnum.RegDword:
                        break;
                    case DataTypeEnum.RegDwordBigEndian:
                        break;
                    case DataTypeEnum.RegLink:
                        break;
                    case DataTypeEnum.RegMultiSz:
                        break;
                    case DataTypeEnum.RegResourceList:
                        break;
                    case DataTypeEnum.RegFullResourceDescription:
                        break;
                    case DataTypeEnum.RegResourceRequirementsList:
                        break;
                    case DataTypeEnum.RegQword:
                        break;
                    case DataTypeEnum.RegFileTime:
                        break;
                    case DataTypeEnum.RegNone:
                    case DataTypeEnum.RegBinary:
                        break;
                    case DataTypeEnum.RegUnknown:
                        break;
                    default:
                        break;
                }

                return "-TODO-";
            }
        }
    }
}
