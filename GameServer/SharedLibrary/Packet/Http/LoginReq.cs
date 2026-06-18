namespace SharedLibrary.Packet.Http
{
    public class LoginReq
    {
        public string GuidKey { get; set; } = string.Empty;
    }    
    
    public class LoginRes
    {
        public ResultCode ResultCode { get; set; }
        
        public long SessionId { get; set; }
    }   
}

