using Interviewer.Contracts.Models;
using Interviewer.Contracts.Requests;
using Interviewer.Services;
using Interviewer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Interviewer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InterviewsController : ControllerBase
{
    private readonly IInterviewService _interviewService;
    private readonly IInterviewGeneratorService _interviewGeneratorService;

    public InterviewsController(IInterviewService interviewService, IInterviewGeneratorService interviewGeneratorService)
    {
        _interviewService = interviewService;
        _interviewGeneratorService = interviewGeneratorService;
    }

    // GET api/interviews
    [HttpGet]
    [ProducesResponseType(typeof(List<Interview>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var interviews = await _interviewService.GetAllAsync();
        return Ok(interviews);
    }

    // GET api/interviews/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Interview), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var interview = await _interviewService.GetByIdAsync(id);

        if (interview is null)
            return NotFound(new { message = $"Interview with id '{id}' not found." });

        return Ok(interview);
    }

    // POST api/interviews
    [HttpPost]
    [ProducesResponseType(typeof(Interview), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Interview interview)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Interview.Id is a plain string with no [BsonId]/[BsonRepresentation(ObjectId)], so the driver has
        // no id generator for it: assigning null would store _id: null and the *second* insert would fail
        // with a duplicate key error. Assign the id server-side (same Guid strategy as the Blazor host) so a
        // caller also cannot overwrite an existing document via POST.
        interview.Id = Guid.NewGuid().ToString();
        await _interviewService.CreateAsync(interview);

        return CreatedAtAction(nameof(GetById), new { id = interview.Id }, interview);
    }

    // PUT api/interviews/{id}
    // No length(24) constraint: ids are 36-char Guid strings, not Mongo ObjectIds.
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] Interview updatedInterview)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _interviewService.GetByIdAsync(id);

        if (existing is null)
            return NotFound(new { message = $"Interview with id '{id}' not found." });

        updatedInterview.Id = id;
        await _interviewService.UpdateAsync(id, updatedInterview);

        return NoContent();
    }

    // DELETE api/interviews/{id}
    // No length(24) constraint: ids are 36-char Guid strings, not Mongo ObjectIds.
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _interviewService.GetByIdAsync(id);

        if (existing is null)
            return NotFound(new { message = $"Interview with id '{id}' not found." });

        await _interviewService.DeleteAsync(id);

        return NoContent();
    }


    //Generation
    [HttpPost("plan")]
    public async Task<IActionResult> GeneratePlan([FromBody] InterviewRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Position))
            return BadRequest("Position is required.");

        var plan = await _interviewGeneratorService.GenerateInterviewPlanAsync(request.Position, request.CvText);
        return Ok(plan);
    }

    [HttpPost("feedback/stream")]
    public async Task StreamFeedback([FromBody] Interview interviewData)
    {
        // Set the response type to stream plain text directly to the client
        Response.ContentType = "text/plain; charset=utf-8";

        await foreach (var chunk in _interviewGeneratorService.GenerateFeedbackAsync(interviewData))
        {
            // Write the chunk to the response body
            await Response.WriteAsync(chunk);

            // Flush ensures the chunk is sent to the client immediately 
            // instead of buffering until the end
            await Response.Body.FlushAsync();
        }
    }
}
