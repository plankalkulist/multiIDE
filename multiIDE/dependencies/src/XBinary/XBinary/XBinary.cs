using System;
using System.Text;

namespace XSpace
{
    public enum ByteOrders
    {
        LittleEndian = 0,
        BigEndian = 1
    }

    public static class XBinary
    {
        public static readonly ByteOrders HostMachineByteOrder = BitConverter.IsLittleEndian ? ByteOrders.LittleEndian : ByteOrders.BigEndian;

        public static byte[] FormatSignedNumberBytes(byte[] srcBytes, int length
            , ByteOrders srcByteOrder, ByteOrders dstByteOrder)
        {
            if (length == -1)
                length = srcBytes.Length;

            byte[] dstBytes = new byte[length];
            int minLength = srcBytes.Length < length ?
                srcBytes.Length : length;
            byte appendByte = (byte)(srcByteOrder == ByteOrders.LittleEndian ?
                (srcBytes[srcBytes.Length - 1] >> 7) * 255 : (srcBytes[0] >> 7) * 255);

            if (srcByteOrder == ByteOrders.LittleEndian)
            {
                if (dstByteOrder == ByteOrders.LittleEndian)
                {
                    for (int i = 0; i < minLength; i++)
                        dstBytes[i] = srcBytes[i];
                    for (int i = srcBytes.Length; i < length; i++)
                        dstBytes[i] = appendByte;
                }
                else
                {
                    for (int i = 0; i < minLength; i++)
                        dstBytes[length - 1 - i] = srcBytes[i];
                    for (int i = length - 1 - srcBytes.Length; i > -1; i--)
                        dstBytes[i] = appendByte;
                }
            }
            else
            {
                if (dstByteOrder == ByteOrders.LittleEndian)
                {
                    for (int i = 0; i < minLength; i++)
                        dstBytes[i] = srcBytes[srcBytes.Length - 1 - i];
                    for (int i = minLength; i < length; i++)
                        dstBytes[i] = appendByte;
                }
                else
                {
                    for (int i = 0; i < minLength; i++)
                        dstBytes[length - 1 - i] = srcBytes[srcBytes.Length - 1 - i];
                    for (int i = length - 1 - minLength; i > -1; i--)
                        dstBytes[i] = appendByte;
                }
            }

            return dstBytes;
        }

        //public static byte[] FormatUnsignedNumberBytes(byte[] srcBytes, int length = -1

        public static byte[] ConvertBinaryNumberBytesToTextNumberBytes(byte[] srcBytes, ByteOrders srcByteOrder
                                                                        , Encoding dstCharSet)
        {
            object number;
            if (srcBytes.Length == 1)
                number = (object)(sbyte)(srcBytes[0]);
            else if (srcBytes.Length == 2)
                number = (object)(BitConverter.ToInt16(srcBytes, 0));
            else if (srcBytes.Length <= 4)
                number = (object)(BitConverter.ToInt32
                    (FormatSignedNumberBytes(srcBytes, 4, srcByteOrder, HostMachineByteOrder), 0));
            else
                number = (object)(BitConverter.ToInt64
                    (FormatSignedNumberBytes(srcBytes, 8, srcByteOrder, HostMachineByteOrder), 0));

            string unicodeNumber = number.ToString();
            byte[] unicodeNumberBytes = Encoding.Unicode.GetBytes(unicodeNumber);
            byte[] dstencNumberBytes = Encoding.Convert(Encoding.Unicode, dstCharSet, unicodeNumberBytes);

            return dstencNumberBytes;
        }

        public static bool TryParseTextNumberBytesToBinaryNumberBytes(byte[] srcBytes, Encoding srcCharSet
                                                                , out byte[] dstBytes, int length, ByteOrders dstByteOrder)
        {
            byte[] unicodeNumberBytes = Encoding.Convert(srcCharSet, Encoding.Unicode, srcBytes);
            char[] unicodeNumberChars = new char[Encoding.Unicode.GetCharCount(unicodeNumberBytes, 0, unicodeNumberBytes.Length)];
            Encoding.Unicode.GetChars(unicodeNumberBytes, 0, unicodeNumberBytes.Length, unicodeNumberChars, 0);
            string unicodeNumber = new string(unicodeNumberChars);
            Int64 number;

            if (Int64.TryParse(unicodeNumber, out number))
            {
                dstBytes = FormatSignedNumberBytes(BitConverter.GetBytes(number), length, HostMachineByteOrder, dstByteOrder);
                return true;
            }
            else
            {
                dstBytes = new byte[0];
                return false;
            }
        }

        public static int MaxDigitCount(int srcBase, int dstBase, int srcDigitCount, bool complement = true)
        {
            double n = complement ? 0.5 : 1.0;
            int i = 0;
            int dstDigitCount = 0;

            while (i < srcDigitCount)
            {
                n *= srcBase;
                i++;
                while (n >= 1)
                {
                    n /= dstBase;
                    dstDigitCount++;
                }
            }

            return dstDigitCount;
        }
    }
}
