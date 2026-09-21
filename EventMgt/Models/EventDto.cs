using EventMgt.Helpers.Common;
using System.ComponentModel.DataAnnotations;

namespace EventMgt.Models
{
    public class EventDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Event title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Venue is required.")]
        public int? VenueId { get; set; }

        public int OrganizerId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max participants must be greater than zero.")]
        public int MaxParticipants { get; set; }

        public string Status { get; set; } = "Scheduled"; // Scheduled, Ongoing, Completed, Cancelled

        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }

        [Url(ErrorMessage = "Event image must be a valid URL.")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
        public string? EventImageUrl { get; set; }

        public bool IsCancelled { get; set; }

        [StringLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters.")]
        public string? CancellationReason { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Read-only relational & computed properties for UI/API consumers
        public string? CategoryName { get; set; }
        public string? VenueName { get; set; }
        public string? OrganizerName { get; set; }
        public string? OrganizerEmail { get; set; }
        public int RegisteredParticipantsCount { get; set; }
        public int AvailableSeats => Math.Max(0, MaxParticipants - RegisteredParticipantsCount);
    }
    public class EventQueryParameters : PaginationParams
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? VenueId { get; set; }
        public int? OrganizerId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? OnlyActive { get; set; } = true;
    }
}
