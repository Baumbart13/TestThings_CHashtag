namespace Network
{
    public enum MessageType : Int32
    {
        #region Probably removing these, as Messages consist of them
        Byte,
        Integer,
        Float,
        Double,
        Boolean,
        String,
        #endregion
        Image = 0,
        ImageRequest = 1,
        Exception
    }
}