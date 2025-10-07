using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingZone.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(120)]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        [Column("phone")]
        public string? Phone { get; set; }

        [Column("role")]
        public UserRole Role { get; set; } = UserRole.client;

        [ScaffoldColumn(false)] // no se muestra en formularios
        [DataType(DataType.Password)]
        public string HashPassword { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("registration_date")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
