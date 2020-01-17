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
        RegResourceRequirementsList = 10,
        RegQword = 11,
        RegFileTime = 16,

        RegUwpByte = 257,
        RegUwpInt16 = 258,
        RegUwpUint16 = 259,
        RegUwpInt32 = 260,
        RegUwpUint32 = 261,
        RegUwpInt64 = 262,
        RegUwpUint64 = 263,
        RegUwpSingle = 264,
        RegUwpDouble = 265,
        RegUwpChar16 = 266,
        RegUwpBoolean = 267,
        RegUwpString = 268,
        RegUwpCompositeValue = 269,
        RegUwpDateTimeOffset = 270,
        RegUwpTimeSpan = 271,
        RegUwpGuid = 272,
        RegUwpPoint = 273,
        RegUwpSize = 274,
        RegUwpRect = 275,
        RegUwpArrayByte = 276,
        RegUwpArrayInt16 = 277,
        RegUwpArrayUint16 = 278,
        RegUwpArrayInt32 = 279,
        RegUwpArrayUint32 = 280,
        RegUwpArrayInt64 = 281,
        RegUwpArrayUint64 = 282,
        RegUwpArraySingle = 283,
        RegUwpArrayDouble = 284,
        RegUwpArrayChar16 = 285,
        RegUwpArrayBoolean = 286,
        RegUwpArrayString = 287,
        RegUwpArrayDateTimeOffset = 288,
        RegUwpArrayTimeSpan = 289,
        RegUwpArrayGuid = 290,
        RegUwpArrayPoint = 291,
        RegUwpArraySize = 292,
        RegUwpArrayRect = 293,

        RegUnknown = 999
    }
}
