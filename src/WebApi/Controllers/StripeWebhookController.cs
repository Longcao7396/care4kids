using GiveAID.Application.Features.Donations.Commands.ConfirmWebhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// L-04: Dedicated Stripe webhook controller.
/// 
/// IMPORTANT: This controller reads the raw request body BEFORE any model binding
/// because Stripe's signature validation requires the exact raw payload bytes.
/// ASP.NET's [FromBody] binding consumes the stream, making signature validation impossible.
///
/// Stripe sends the signature in the `Stripe-Signature` HTTP header.
/// The format is: `t=timestamp,v1=signature`
/// </summary>
[ApiController]
[Route("api/v1/webhooks/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(ISender mediator, ILogger<StripeWebhookController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Receives Stripe webhook events.
    /// 
    /// Stripe calls this endpoint with a POST request containing:
    /// - Body: Raw JSON webhook payload
    /// - Header: Stripe-Signature (contains timestamp and HMAC signature)
    /// 
    /// The raw body must be read before any model binding because the signature
    /// is computed over the exact raw bytes.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        // L-04: Read the raw request body for signature validation
        // This MUST be done before any model binding
        string rawBody;
        using (var reader = new StreamReader(Request.Body))
        {
            rawBody = await reader.ReadToEndAsync();
        }

        if (string.IsNullOrEmpty(rawBody))
        {
            _logger.LogWarning("Stripe webhook received with empty body");
            return BadRequest(new { error = "Empty request body" });
        }

        // L-04: SECURITY - Always require Stripe-Signature header
        var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(stripeSignature))
        {
            // L-04: Log as CRITICAL security event - missing signature is a potential attack
            _logger.LogCritical(
                "SECURITY ALERT: Stripe webhook received without Stripe-Signature header from {RemoteIp}. " +
                "Request rejected. If this is unexpected, investigate for potential webhook spoofing attempt.",
                HttpContext.Connection.RemoteIpAddress);
            return BadRequest(new { error = "Missing Stripe-Signature header" });
        }

        _logger.LogInformation(
            "Stripe webhook received. Signature present: {HasSignature}, Body length: {BodyLength}",
            !string.IsNullOrEmpty(stripeSignature), rawBody.Length);

        try
        {
            // L-04: Dispatch to the command handler which performs signature verification
            // and updates the donation status
            var result = await _mediator.Send(new ConfirmWebhookCommand
            {
                Gateway = "stripe",
                Payload = rawBody,
                Signature = stripeSignature ?? ""
            });

            if (result)
            {
                // Return 200 to acknowledge receipt to Stripe
                // Stripe retries webhooks on non-2xx responses
                return Ok(new { received = true });
            }
            else
            {
                // Signature validation failed
                _logger.LogWarning("Stripe webhook processing failed - signature validation error");
                return BadRequest(new { error = "Webhook verification failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            // Return 500 so Stripe will retry
            return StatusCode(500, new { error = "Internal error processing webhook" });
        }
    }
}
