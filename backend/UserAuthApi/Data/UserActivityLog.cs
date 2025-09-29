namespace UserAuthApi.Data;

public class UserActivityLog
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Activity { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
}