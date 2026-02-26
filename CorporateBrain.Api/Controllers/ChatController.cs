using CorporateBrain.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CorporateBrain.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IAiChatServices _aiServices;

        public ChatController(IAiChatServices aiServices)
        {
            _aiServices = aiServices;
        }

        //[Authorize]
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] string message)
        {
            // 1. Call the AI Service
            string response = await _aiServices.ChatAsync(message);
            // 2. Return the response
            return Ok(new { Response = response });
        }

        // New Endpoint: Teach the AI
        [HttpPost("teach")]
        public async Task<IActionResult> Teach([FromBody] string documentText)
        {
            // 1. Ingest the document into the AI's knowledge base
            await _aiServices.IngestDocumentAsync(documentText);
            // 2. Return a success message
            return Ok(new { Message = "I have memorized this document." });
        }
    }
}
