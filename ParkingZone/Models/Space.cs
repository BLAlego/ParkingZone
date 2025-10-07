using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingZone.Models
{
    [Table("spaces")]
    public class Space
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required, StringLength(20)]
        [Column("code")]
        public string Code { get; set; } = string.Empty; // e.g., A01

        [Column("type")]
        public VehicleType Type { get; set; }

        [Column("available")]
        public bool Available { get; set; } = true;

        [Column("level")]
        public int Level { get; set; } = 1;

        [Column("hourly_rate", TypeName = "decimal(10,2)")]
        public decimal HourlyRate { get; set; }

        // Navigation
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Parking> Parkings { get; set; } = new List<Parking>();
    }
}
