namespace UserAuthApi.Data;

public class SecurityAuditLog
{
    public int Id { get; set; }
    public string Event { get; set; }
    public string Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
}