using DigitalArena.DBContext;

public partial class EngagementModel
{
    public int EngagementId { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public bool Liked { get; set; }
    public bool Disliked { get; set; }

    public virtual Product Product { get; set; }
    public virtual User User { get; set; }
}