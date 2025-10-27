namespace Domain.Entities
{
    public class UserRole : Entity
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}
