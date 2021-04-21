using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPSettingsEditor
{
    public enum DataTypeEnum
    {
        RegNone = 0,
        RegSz = 1,
        RegExpandSz = 2,
        RegBinary = 3,
        RegDword = 4,
        RegDwordBigEndian = 5,
        RegLink = 6,
        RegMultiSz = 7,
        RegResourceList = 8,
        RegFullResourceDescription = 9,
        RegResourceRequirementsList = 0xA,
        RegQword = 0xB,
        RegFileTime = 0x10,

        RegUwpByte = 0x5f5e_101,
        RegUwpInt16 = 0x5f5e_102,
        RegUwpUint16 = 0x5f5e_103,
        RegUwpInt32 = 0x5f5e_104,
        RegUwpUint32 = 0x5f5e_105,
        RegUwpInt64 = 0x5f5e_106,
        RegUwpUint64 = 0x5f5e_107,
        RegUwpSingle = 0x5f5e_108,
        RegUwpDouble = 0x5f5e_109,
        RegUwpChar = 0x5f5e_10A,
        RegUwpBoolean = 0x5f5e_10B,
        RegUwpString = 0x5f5e_10C,
        RegUwpCompositeValue = 0x5f5e_10D,
        RegUwpDateTimeOffset = 0x5f5e_10E,
        RegUwpTimeSpan = 0x5f5e_10F,
        RegUwpGuid = 0x5f5e_110,
        RegUwpPoint = 0x5f5e_111,
        RegUwpSize = 0x5f5e_112,
        RegUwpRect = 0x5f5e_113,
        RegUwpArrayByte = 0x5f5e_114,
        RegUwpArrayInt16 = 0x5f5e_115,
        RegUwpArrayUint16 = 0x5f5e_116,
        RegUwpArrayInt32 = 0x5f5e_117,
        RegUwpArrayUint32 = 0x5f5e_118,
        RegUwpArrayInt64 = 0x5f5e_119,
        RegUwpArrayUint64 = 0x5f5e_11A,
        RegUwpArraySingle = 0x5f5e_11B,
        RegUwpArrayDouble = 0x5f5e_11C,
        RegUwpArrayChar16 = 0x5f5e_11D,
        RegUwpArrayBoolean = 0x5f5e_11E,
        RegUwpArrayString = 0x5f5e_11F,
        RegUwpArrayDateTimeOffset = 0x5f5e_120,
        RegUwpArrayTimeSpan = 0x5f5e_121,
        RegUwpArrayGuid = 0x5f5e_122,
        RegUwpArrayPoint = 0x5f5e_123,
        RegUwpArraySize = 0x5f5e_124,
        RegUwpArrayRect = 0x5f5e_125,

        RegUnknown = 999
    }
}
