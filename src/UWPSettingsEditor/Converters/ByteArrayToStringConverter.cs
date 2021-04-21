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
            if ((int)dataType > 0x5f5e_100 && (int)dataType < 0x5f5e_126)
            {
                if (dataRaw.Length < 8)
                    return "(invalid data)";

                var splittedDataRaw = MethodHelpers.SplitDataRaw(dataRaw);
                var data = splittedDataRaw.Value;
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
                        return GetSingle(data).ToString("G9");
                    case DataTypeEnum.RegUwpDouble:
                        return GetDouble(data).ToString("G17");
                    case DataTypeEnum.RegUwpChar:
                        return "'" + GetChar(data).ToString() + "'";
                    case DataTypeEnum.RegUwpBoolean:
                        return GetBoolean(data).ToString();
                    case DataTypeEnum.RegUwpString:
                        return "\"" + GetString(data).ReplaceMultilineWithSymbols() + "\"";
                    case DataTypeEnum.RegUwpCompositeValue:
                        return PrettyPrintDictionary(GetCompositeValue(data)).ReplaceMultilineWithSymbols();
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
                        return PrettyPrintArray(GetArray(data.Length, 1, i => GetByte(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayInt16:
                        return PrettyPrintArray(GetArray(data.Length, 2, i => GetInt16(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayUint16:
                        return PrettyPrintArray(GetArray(data.Length, 2, i => GetUInt16(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayInt32:
                        return PrettyPrintArray(GetArray(data.Length, 4, i => GetInt32(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayUint32:
                        return PrettyPrintArray(GetArray(data.Length, 4, i => GetUInt32(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayInt64:
                        return PrettyPrintArray(GetArray(data.Length, 8, i => GetInt64(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayUint64:
                        return PrettyPrintArray(GetArray(data.Length, 8, i => GetUInt64(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArraySingle:
                        return PrettyPrintArray(GetArray(data.Length, 4, i => GetSingle(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayDouble:
                        return PrettyPrintArray(GetArray(data.Length, 8, i => GetDouble(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayChar16:
                        return PrettyPrintArray(GetArray(data.Length, 2, i => GetChar(data, i)), "'").ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayBoolean:
                        return PrettyPrintArray(GetArray(data.Length, 1, i => GetBoolean(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayString:
                        return PrettyPrintStringArrayFromRaw(data).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayDateTimeOffset:
                        return PrettyPrintArray(GetArray(data.Length, 8, i => GetDateTimeOffset(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayTimeSpan:
                        return PrettyPrintArray(GetArray(data.Length, 8, i => GetTimeSpan(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayGuid:
                        return PrettyPrintArray(GetArray(data.Length, 16, i => GetGuid(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayPoint:
                        return PrettyPrintArray(GetArray(data.Length, 4 * 2, i => GetPoint(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArraySize:
                        return PrettyPrintArray(GetArray(data.Length, 4 * 2, i => GetSize(data, i))).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegUwpArrayRect:
                        return PrettyPrintArray(GetArray(data.Length, 4 * 4, i => GetRect(data, i))).ReplaceMultilineWithSymbols();
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
                        return GetString(dataRaw).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegDword:
                        {
                            var dword = Deserializer.GetUInt32(dataRaw);
                            return $"0x{dword:X8} ({dword})";
                        }
                    case DataTypeEnum.RegDwordBigEndian:
                        {
                            var dword = Deserializer.GetUInt32BigEndian(dataRaw);
                            return $"0x{dword:X8} ({dword})";
                        }
                    case DataTypeEnum.RegMultiSz:
                        return GetString(dataRaw).ReplaceMultilineWithSymbols();
                    case DataTypeEnum.RegQword:
                        {
                            var qword = Deserializer.GetUInt64(dataRaw);
                            return $"0x{qword:X16} ({qword})";
                        }
                    case DataTypeEnum.RegNone:
                        return "";
                    case DataTypeEnum.RegLink:
                    case DataTypeEnum.RegResourceList:
                    case DataTypeEnum.RegFullResourceDescription:
                    case DataTypeEnum.RegResourceRequirementsList:
                    case DataTypeEnum.RegFileTime:
                    case DataTypeEnum.RegBinary:
                    case DataTypeEnum.RegUnknown:
                        return PrettyPrintArray(dataRaw).ReplaceMultilineWithSymbols();
                    default:
                        return PrettyPrintArray(dataRaw).ReplaceMultilineWithSymbols();
                }
            }
        }
    }
}
