namespace SharedLibrary.Packet.Http
{
    public class Login
    {
        public string GuidKey { get; set; } = string.Empty;
    }    
    
    public class LoginRes
    {
        public ResultCode ResultCode { get; set; }
        
        public long AccountId { get; set; }

        public string AccessToken { get; set; } = string.Empty;
    }   
}

