namespace Database.Db.Row;

public record PlayerPurchaseLimitRow
{
    public long id { get; set; }
    
    public long player_id { get; set; }
    
    public int product_id { get; set; }
    
    public int purchase_count { get; set; }
    
    public DateTime updated_at { get; set; }
    
    public DateTime created_at { get; set; }
}