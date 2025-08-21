using Microsoft.AspNetCore.Mvc;
using EventSphere.Application.Dtos.Payments;
using EventSphere.Application.Interfaces;
using EventSphere.Domain.Enums;
using Microsoft.Extensions.Configuration;

using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;

    public PaymentsController(IPaymentService paymentService, IConfiguration configuration)
    {
        _paymentService = paymentService;
        _configuration = configuration;
    }

    [HttpPost("create-payment-intent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequestDto request)
    {
        if (request == null)
            return BadRequest(new { success = false, message = "Invalid payment request." });

        var response = await _paymentService.CreatePaymentIntentAsync(request);
        return Ok(new { clientSecret = response.ClientSecret });
    }

    [HttpPost("create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutSessionRequestDto request)
    {
        Console.WriteLine("[DEBUG] Entered CreateCheckoutSession endpoint");
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            Console.WriteLine($"[ERROR] ModelState invalid: {errors}");
            return BadRequest(new { success = false, message = "Model binding failed", errors });
        }
        try
        {
            // Log the incoming request for debugging
            Console.WriteLine($"[DEBUG] Incoming checkout session request: EventId={request?.EventId}, UserId={request?.UserId}, TicketCount={request?.TicketCount}, Currency={request?.Currency}, SuccessUrl={request?.SuccessUrl}, CancelUrl={request?.CancelUrl}");

            if (request == null || request.EventId <= 0 || request.TicketCount <= 0)
            {
                Console.WriteLine($"[ERROR] Invalid checkout session request: {System.Text.Json.JsonSerializer.Serialize(request)}");
                return BadRequest(new { success = false, message = "Invalid checkout session request." });
            }

            // Ensure SuccessUrl and CancelUrl are valid URLs
            if (string.IsNullOrWhiteSpace(request.SuccessUrl))
                request.SuccessUrl = "https://dep-2-frontend.vercel.app/payment-success";
            if (string.IsNullOrWhiteSpace(request.CancelUrl))
                request.CancelUrl = "https://dep-2-frontend.vercel.app/payment-cancel";

            var response = await _paymentService.CreateCheckoutSessionAsync(request);
            if (response == null || string.IsNullOrEmpty(response.Url))
            {
                Console.WriteLine($"[ERROR] Payment service returned null or empty URL for request: {System.Text.Json.JsonSerializer.Serialize(request)}");
                return BadRequest(new { success = false, message = "Failed to create checkout session. No URL returned." });
            }
            return Ok(new { url = response.Url });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception in CreateCheckoutSession: {ex.Message}\n{ex.StackTrace}");
            return BadRequest(new { success = false, message = ex.Message, details = ex.StackTrace });
        }
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"];
        var secret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
        Console.WriteLine($"[Webhook] Stripe WebhookSecret from config: '{secret}'");
        Stripe.Event stripeEvent;
        try
        {
            stripeEvent = Stripe.EventUtility.ConstructEvent(json, stripeSignature, secret);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Webhook] Stripe signature verification failed: {ex.Message}");
            return BadRequest();
        }

        Console.WriteLine($"[Webhook] Received event: {stripeEvent.Type}");
        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var paymentIntent = (Stripe.PaymentIntent)stripeEvent.Data.Object;
            Console.WriteLine($"[Webhook] payment_intent.succeeded for PaymentIntentId: {paymentIntent.Id}");
            foreach (var kv in paymentIntent.Metadata)
            {
                Console.WriteLine($"[Webhook] Metadata: {kv.Key} = {kv.Value}");
            }
            decimal amount = paymentIntent.AmountReceived > 0 ? paymentIntent.AmountReceived / 100m : paymentIntent.Amount / 100m;
            await _paymentService.UpdatePaymentAndRegistrationAsync(paymentIntent.Id, paymentIntent.Metadata, amount);
        }
        else if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = (Stripe.Checkout.Session)stripeEvent.Data.Object;
            Console.WriteLine($"[Webhook] checkout.session.completed for SessionId: {session.Id}");
            foreach (var kv in session.Metadata)
            {
                Console.WriteLine($"[Webhook] Session Metadata: {kv.Key} = {kv.Value}");
            }
            // Fetch PaymentIntent and use its metadata if needed
            if (!string.IsNullOrEmpty(session.PaymentIntentId))
            {
                var paymentIntentService = new Stripe.PaymentIntentService();
                var paymentIntent = await paymentIntentService.GetAsync(session.PaymentIntentId);
                Console.WriteLine($"[Webhook] Fetched PaymentIntent for Session: {paymentIntent.Id}");
                foreach (var kv in paymentIntent.Metadata)
                {
                    Console.WriteLine($"[Webhook] PaymentIntent Metadata: {kv.Key} = {kv.Value}");
                }
                decimal amount = paymentIntent.AmountReceived > 0 ? paymentIntent.AmountReceived / 100m : paymentIntent.Amount / 100m;
                await _paymentService.UpdatePaymentAndRegistrationAsync(paymentIntent.Id, paymentIntent.Metadata, amount);
            }
        }
        // Handle other event types as needed
        return Ok();
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmationDto dto)
    {
        // TODO: Validate payment with Stripe API, then update Payment and Registration records
        // Example: var payment = await _paymentService.ConfirmAndUpdateAsync(dto);
        await Task.CompletedTask;
        return Ok(new { success = true });
    }
}
