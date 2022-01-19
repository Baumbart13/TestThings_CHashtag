namespace Network
{
    public enum MessageType : Int32
    {
        Image = 0,
        ImageRequest = 1,
        String,
        // Probably removing these, as Messages consist of these
        Byte,
        Integer,
        Float,
        Double,
        Boolean
    }
}