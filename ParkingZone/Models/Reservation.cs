using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingZone.Models
{
    [Table("reservations")]
    public class Reservation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("space_id")]
        public int SpaceId { get; set; }

        [Required]
        [Column("entry_time")]
        public DateTime EntryTime { get; set; }

        [Column("status")]
        public ReservationStatus Status { get; set; } = ReservationStatus.active;

        // Navigation
        public User? User { get; set; }
        public Space? Space { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
