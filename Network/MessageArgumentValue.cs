using System.Runtime.InteropServices;

namespace Network
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MessageArgumentValue
    {
        // if this would not be with the same offset, we would be f*cked up
        // this would be a huge empty struct, so let's create a somewhat of a c++ union
        [FieldOffset(0)]
        public byte Byte;

        [FieldOffset(0)]
        public char Character;

        [FieldOffset(0)]
        public short Short;
        
        [FieldOffset(0)]
        public int Integer;

        [FieldOffset(0)]
        public long Long;
        
        [FieldOffset(0)]
        public float Float;
        
        [FieldOffset(0)]
        public double Double;

        [FieldOffset(0)]
        public decimal Decimal;
        
        [FieldOffset(0)]
        public bool Boolean;
    }
}