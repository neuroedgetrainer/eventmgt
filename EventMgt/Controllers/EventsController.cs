using EventMgt.Helpers.Common;
using EventMgt.Models;
using EventMgt.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventMgt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : BaseApiController
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Searches, filters, and paginates through active events (Public).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<EventDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] EventQueryParameters queryParams)
        {
            var result = await _eventService.GetEventsAsync(queryParams);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves full event details, venue specs, organizer info, and seat availability (Public).
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _eventService.GetByIdAsync(id);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves all events managed by the currently logged-in organizer (Organizer / Admin).
        /// </summary>
        [HttpGet("my-events")]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(typeof(IReadOnlyList<EventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyOrganizedEvents()
        {
            var result = await _eventService.GetEventsByOrganizerAsync(CurrentUserId);
            return HandleResult(result);
        }

        /// <summary>
        /// Creates a new event owned by the authenticated organizer/admin (Organizer / Admin).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] EventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _eventService.CreateAsync(CurrentUserId, dto);
            return HandleResult(result);
        }

        /// <summary>
        /// Updates event details (Only the organizer who created the event or an Admin).
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] EventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _eventService.UpdateAsync(id, CurrentUserId, IsAdmin, dto);
            return HandleResult(result);
        }

        /// <summary>
        /// Cancels an event and records the cancellation reason without hard-deleting (Organizer / Admin).
        /// </summary>
        [HttpPatch("{id:int}/cancel")]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelEvent(int id, [FromBody] string cancellationReason)
        {
            var result = await _eventService.CancelEventAsync(id, CurrentUserId, IsAdmin, cancellationReason);
            return HandleResult(result);
        }

        /// <summary>
        /// Soft-deactivates an event (Organizer / Admin).
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _eventService.SoftDeleteAsync(id, CurrentUserId, IsAdmin);
            return HandleResult(result);
        }
    }
}
