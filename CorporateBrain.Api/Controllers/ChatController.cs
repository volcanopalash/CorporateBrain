using CorporateBrain.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CorporateBrain.Api.Controllers
{

    public record AskRequestDto(string Question);
    public record TeachRequestDto(string Text);

    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IAiChatServices _aiServices;

        public ChatController(IAiChatServices aiServices)
        {
            _aiServices = aiServices;
        }

        [Authorize]
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskRequestDto request)
        {
            // 1. Call the AI Service
            string response = await _aiServices.ChatAsync(request.Question);
            // 2. Return the response
            return Ok(new { Response = response });
        }

        // New Endpoint: Teach the AI
        [Authorize]
        [HttpPost("teach")]
        public async Task<IActionResult> Teach([FromBody] TeachRequestDto request)
        {

            // Prevent empty strings from being saved
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { Error = "Text cannot be empty!" });

            // 1. Ingest the document into the AI's knowledge base
            await _aiServices.IngestDocumentAsync(request.Text);
            // 2. Return a success message
            return Ok(new { Message = "I have memorized this document." });
        }
    }
}
