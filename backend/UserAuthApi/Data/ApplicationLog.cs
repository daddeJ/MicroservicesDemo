namespace UserAuthApi.Data;

public class Applicationlog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
    public string CorrelationId { get; set; }
    public string Exception { get; set; }
}