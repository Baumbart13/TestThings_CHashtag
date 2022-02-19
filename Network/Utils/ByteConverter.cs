using System.Text;

namespace Network.Utils;

public static class ByteConverter
{
#region ToByte

    public static byte[] ToBytes(this string str)
    {
        var b = new byte[str.Length*2];
        for (int strI = 0, bI = 0; strI < str.Length; strI += 1, bI += 2)
        {
            var c = str[strI].ToBytes();
            b[bI] = c[0];
            b[bI + 1] = c[1];
        }

        return b;
    }

    public static byte[] ToBytes(this int i)
    {
        return new byte[]
        {
            (byte)(i >> 24),
            (byte)(i >> 16),
            (byte)(i >> 8),
            (byte)i
        };
    }

    public static byte[] ToBytes(this char c)
    {
        return new byte[]
        {
            (byte)(c >> 8),
            (byte)c
        };
    }

    public static byte ToByte(this bool b)
    {
        return b ? byte.MaxValue : (byte)0;
    }

    public static byte[] ToBytes(this short s)
    {
        return new byte[]
        {
            (byte)(s >> 8),
            (byte)s
        };
    }

    public static byte[] ToBytes(this long l)
    {
        return new byte[]
        {
            (byte)(l >> 56),
            (byte)(l >> 48),
            (byte)(l >> 40),
            (byte)(l >> 32),
            (byte)(l >> 24),
            (byte)(l >> 16),
            (byte)(l >> 8),
            (byte)l
        };
    }

#endregion
#region From Bytes

    public static int ToInt32(this byte[] b)
    {
        return ((int)(b[0]) << 24) |
               ((int)(b[1]) << 16) |
               ((int)(b[2]) << 8) |
               (int)(b[3]);
    }

    public static short ToInt16(this byte[] b)
    {
        return (short)((short)(b[0] << 8) |
               (short)(b[1]));
    }

    public static bool ToBoolean(this byte b)
    {
        return b != 0;
    }

    public static char ToCharacter(this byte[] b)
    {
        return (char)b.ToInt16();
    }

    public static string ArrayToString(this byte[] b)
    {
        var sb = new StringBuilder((int)Math.Ceiling(b.Length * 0.5));

        for (var i = 0; i < b.Length; i += 2)
        {
            var c = (char)(((short)b[i] << 8) | b[i+1]);
            sb.Append(c);
        }
        
        return sb.ToString();
    }

    public static long ToInt64(this byte[] b)
    {
        return (long)((long)((long)(b[0]) << 56) |
            (long)((long)(b[1]) << 48) |
            (long)((long)(b[2]) << 40) |
            (long)((long)(b[3]) << 32) |
            (long)((long)(b[4]) << 24) |
            (long)((long)(b[5]) << 16) |
            (long)((long)(b[6]) << 8) |
            (long)(b[7]));
    }

#endregion
}