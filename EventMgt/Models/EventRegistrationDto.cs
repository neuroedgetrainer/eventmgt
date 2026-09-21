using System.ComponentModel.DataAnnotations;

namespace EventMgt.Models
{
    public class EventRegistrationDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Event ID is required.")]
        public int EventId { get; set; }

        public int UserId { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Registered"; // "Registered", "Cancelled"

        public DateTime? CancelledDate { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Read-only populated participant details
        public string? ParticipantName { get; set; }
        public string? ParticipantEmail { get; set; }

        // Read-only populated event details
        public string? EventTitle { get; set; }
        public DateTime? EventStartDate { get; set; }
        public DateTime? EventEndDate { get; set; }
        public string? VenueName { get; set; }
        public string? CategoryName { get; set; }
    }
}
