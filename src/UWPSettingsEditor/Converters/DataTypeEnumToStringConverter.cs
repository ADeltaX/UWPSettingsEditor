using System;
using System.Globalization;
using System.Windows.Data;

namespace UWPSettingsEditor.Converters
{
    public class DataTypeEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dataType = (DataTypeEnum)value;
            return GetStringNameFromDataType(dataType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetStringNameFromDataType(DataTypeEnum dataType)
        {
            switch (dataType)
            {
                case DataTypeEnum.RegNone:
                    return "REG_NONE";
                case DataTypeEnum.RegSz:
                    return "REG_SZ";
                case DataTypeEnum.RegExpandSz:
                    return "REG_EXPAND_SZ";
                case DataTypeEnum.RegBinary:
                    return "REG_BINARY";
                case DataTypeEnum.RegDword:
                    return "REG_DWORD";
                case DataTypeEnum.RegDwordBigEndian:
                    return "REG_DWORD_BIG_ENDIAN";
                case DataTypeEnum.RegLink:
                    return "REG_LINK";
                case DataTypeEnum.RegMultiSz:
                    return "REG_MULTI_SZ";
                case DataTypeEnum.RegResourceList:
                    return "REG_RESOURCE_LIST";
                case DataTypeEnum.RegFullResourceDescription:
                    return "REG_FULL_RES_DESC";
                case DataTypeEnum.RegResourceRequirementsList:
                    return "REG_RES_REQ_LIST";
                case DataTypeEnum.RegQword:
                    return "REG_QWORD";
                case DataTypeEnum.RegFileTime:
                    return "REG_FILETIME";
                case DataTypeEnum.RegUwpByte:
                    return "REG_UWP_BYTE";
                case DataTypeEnum.RegUwpInt16:
                    return "REG_UWP_INT16";
                case DataTypeEnum.RegUwpUint16:
                    return "REG_UWP_UINT16";
                case DataTypeEnum.RegUwpInt32:
                    return "REG_UWP_INT32";
                case DataTypeEnum.RegUwpUint32:
                    return "REG_UWP_UINT32";
                case DataTypeEnum.RegUwpInt64:
                    return "REG_UWP_INT64";
                case DataTypeEnum.RegUwpUint64:
                    return "REG_UWP_UINT64";
                case DataTypeEnum.RegUwpSingle:
                    return "REG_UWP_SINGLE";
                case DataTypeEnum.RegUwpDouble:
                    return "REG_UWP_DOUBLE";
                case DataTypeEnum.RegUwpChar16:
                    return "REG_UWP_CHAR16";
                case DataTypeEnum.RegUwpBoolean:
                    return "REG_UWP_BOOLEAN";
                case DataTypeEnum.RegUwpString:
                    return "REG_UWP_STRING";
                case DataTypeEnum.RegUwpCompositeValue:
                    return "REG_UWP_COMPOSITE";
                case DataTypeEnum.RegUwpDateTimeOffset:
                    return "REG_UWP_DATETIMEOFFSET";
                case DataTypeEnum.RegUwpTimeSpan:
                    return "REG_UWP_TIMESPAN";
                case DataTypeEnum.RegUwpGuid:
                    return "REG_UWP_GUID";
                case DataTypeEnum.RegUwpPoint:
                    return "REG_UWP_POINT";
                case DataTypeEnum.RegUwpSize:
                    return "REG_UWP_SIZE";
                case DataTypeEnum.RegUwpRect:
                    return "REG_UWP_RECT";
                case DataTypeEnum.RegUwpArrayByte:
                    return "REG_UWP_ARRAY_BYTE";
                case DataTypeEnum.RegUwpArrayInt16:
                    return "REG_UWP_ARRAY_INT16";
                case DataTypeEnum.RegUwpArrayUint16:
                    return "REG_UWP_ARRAY_UINT16";
                case DataTypeEnum.RegUwpArrayInt32:
                    return "REG_UWP_ARRAY_INT32";
                case DataTypeEnum.RegUwpArrayUint32:
                    return "REG_UWP_ARRAY_UINT32";
                case DataTypeEnum.RegUwpArrayInt64:
                    return "REG_UWP_ARRAY_INT64";
                case DataTypeEnum.RegUwpArrayUint64:
                    return "REG_UWP_ARRAY_UINT64";
                case DataTypeEnum.RegUwpArraySingle:
                    return "REG_UWP_ARRAY_SINGLE";
                case DataTypeEnum.RegUwpArrayDouble:
                    return "REG_UWP_ARRAY_DOUBLE";
                case DataTypeEnum.RegUwpArrayChar16:
                    return "REG_UWP_ARRAY_CHAR16";
                case DataTypeEnum.RegUwpArrayBoolean:
                    return "REG_UWP_ARRAY_BOOLEAN";
                case DataTypeEnum.RegUwpArrayString:
                    return "REG_UWP_ARRAY_STRING";
                case DataTypeEnum.RegUwpArrayDateTimeOffset:
                    return "REG_UWP_ARRAY_DATETIMEOFFSET";
                case DataTypeEnum.RegUwpArrayTimeSpan:
                    return "REG_UWP_ARRAY_TIMESPAN";
                case DataTypeEnum.RegUwpArrayGuid:
                    return "REG_UWP_ARRAY_GUID";
                case DataTypeEnum.RegUwpArrayPoint:
                    return "REG_UWP_ARRAY_POINT";
                case DataTypeEnum.RegUwpArraySize:
                    return "REG_UWP_ARRAY_SIZE";
                case DataTypeEnum.RegUwpArrayRect:
                    return "REG_UWP_ARRAY_RECT";
                case DataTypeEnum.RegUnknown:
                default:
                    return "UNKNOWN (" + dataType + ")";
            }
        }
    }
}
