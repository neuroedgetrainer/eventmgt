using EventMgt.Models;
using EventMgt.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventMgt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RegistrationsController : BaseApiController
    {
        private readonly IEventRegistrationService _registrationService;

        public RegistrationsController(IEventRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        /// <summary>
        /// Registers the authenticated participant for an event (Participant / Admin).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Participant,Admin")]
        [ProducesResponseType(typeof(EventRegistrationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] EventRegistrationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _registrationService.RegisterAsync(CurrentUserId, dto);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves all event registrations made by the currently authenticated user (Participant / Admin).
        /// </summary>
        [HttpGet("my-registrations")]
        [Authorize(Roles = "Participant,Admin")]
        [ProducesResponseType(typeof(IReadOnlyList<EventRegistrationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyRegistrations()
        {
            var result = await _registrationService.GetMyRegistrationsAsync(CurrentUserId);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves a specific registration by ID (Participant, Organizer, or Admin).
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EventRegistrationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _registrationService.GetByIdAsync(id, CurrentUserId, IsAdmin);
            return HandleResult(result);
        }

        /// <summary>
        /// Cancels a participant's registration (The registered user or Admin).
        /// </summary>
        [HttpPatch("{id:int}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelRegistration(int id)
        {
            var result = await _registrationService.CancelRegistrationAsync(id, CurrentUserId, IsAdmin);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves the attendee roster for a specific event (Event's Organizer or Admin).
        /// </summary>
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organizer,Admin")]
        [ProducesResponseType(typeof(IReadOnlyList<EventRegistrationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventAttendees(int eventId)
        {
            var result = await _registrationService.GetEventRegistrationsAsync(eventId, CurrentUserId, IsAdmin);
            return HandleResult(result);
        }
    }
}
