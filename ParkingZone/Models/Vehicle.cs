using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingZone.Models
{
    [Table("vehicles")]
    public class Vehicle
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required, StringLength(15)]
        [Column("plate")]
        public string Plate { get; set; } = string.Empty;

        [Column("type")]
        public VehicleType Type { get; set; }

        // Navigation
        public User? User { get; set; }
        public ICollection<Parking> Parkings { get; set; } = new List<Parking>();
    }
}
