namespace Network
{
    public enum MessageType : Int32
    {
        #region Probably removing these, as Messages consist of them
        [Obsolete("Why should someone send just a byte as a message?")]
        Byte = -5,
        
        [Obsolete("Why should someone send just an integer as a message?")]
        Integer = -4,
        
        [Obsolete("Why should someone send just a float as a message?")]
        Float = -3,
        
        [Obsolete("Why should someone send just a double as a message?")]
        Double = -2,
        
        [Obsolete("Why should someone send just a boolean as a message?")]
        Boolean = -1,

        #endregion

        Image = 0,
        ImageRequest = 1,
        Exception = 2,
        String = 3,
    }
}