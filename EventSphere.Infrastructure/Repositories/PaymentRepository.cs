using System.Threading.Tasks;
using EventSphere.Application.Dtos.Payments;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using backend.Data; // Add this for AppDbContext
using Microsoft.EntityFrameworkCore;
using EventSphere.Domain.Enums;

namespace EventSphere.Infrastructure.Repositories
{
    /// <summary>
    /// Creates a Stripe Express account and returns the account ID and onboarding link.
    /// </summary>
   
    public class PaymentRepository : EventSphere.Application.Repositories.IPaymentRepository
    {
        /// <summary>
        /// Checks the onboarding status of a Stripe Express account.
        /// Returns true if payouts are enabled, false otherwise.
        /// </summary>
        public async Task<bool> IsStripeAccountOnboardingCompleteAsync(string stripeAccountId)
        {
            var secretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                _logger.LogError("Stripe secret key not found in environment variables. Set STRIPE_SECRET_KEY in your .env or configuration.");
                throw new InvalidOperationException("Stripe secret key not found in environment variables. Set STRIPE_SECRET_KEY in your .env or configuration.");
            }
            StripeConfiguration.ApiKey = secretKey;
            var accountService = new Stripe.AccountService();
            var account = await accountService.GetAsync(stripeAccountId);
            // Stripe recommends checking payouts_enabled for Express accounts
            return account.PayoutsEnabled;
        }
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PaymentRepository> _logger;

        public PaymentRepository(IConfiguration configuration, AppDbContext dbContext, ILogger<PaymentRepository> logger)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _logger = logger;
            // Set Stripe API key from environment variable
            var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            if (!string.IsNullOrWhiteSpace(stripeSecretKey))
            {
                StripeConfiguration.ApiKey = stripeSecretKey;
            }
            else
            {
                Console.WriteLine("[ERROR] Stripe secret key not found in environment variables. Set STRIPE_SECRET_KEY in your .env or configuration.");
            }
        }
    public async Task<(string AccountId, string OnboardingUrl)> CreateStripeExpressAccountAsync(string email, string returnUrl, string refreshUrl)
    {
        StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
        var accountOptions = new Stripe.AccountCreateOptions
            {
                Type = "express",
                Email = email,
                Country = "US", // Change as needed
                Capabilities = new Stripe.AccountCapabilitiesOptions
                {
                    Transfers = new Stripe.AccountCapabilitiesTransfersOptions { Requested = true },
                    CardPayments = new Stripe.AccountCapabilitiesCardPaymentsOptions { Requested = true }
                }
            };
            var accountService = new Stripe.AccountService();
            var account = await accountService.CreateAsync(accountOptions);

            var accountLinkOptions = new Stripe.AccountLinkCreateOptions
            {
                Account = account.Id,
                RefreshUrl = refreshUrl,
                ReturnUrl = returnUrl,
                Type = "account_onboarding"
            };
            var accountLinkService = new Stripe.AccountLinkService();
            var accountLink = await accountLinkService.CreateAsync(accountLinkOptions);

            return (account.Id, accountLink.Url);
        }

        /// <summary>
        /// Checks if a payment session is still valid (not completed).
        /// Returns true if session is valid, false if already completed.
        /// </summary>
        public async Task<bool> IsPaymentSessionValidAsync(string paymentIntentId)
        {
            var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.TransactionId == paymentIntentId);
            if (payment == null)
                return true; // No payment yet, session is valid
            // If payment is marked as Success, session is not valid anymore
            return payment.Status != PaymentResultStatus.Success;
        }
     

        public async Task<PaymentResponseDto> CreatePaymentIntentAsync(PaymentRequestDto request)
        {
            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            try
            {
                // Validate required fields
                var eventDetails = await _dbContext.Events.FindAsync((int)request.EventId);
                if (eventDetails == null)
                {
                    try
                    {
                        throw new KeyNotFoundException($"Event with ID {request.EventId} not found.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Event with ID {EventId} not found.", request.EventId);
                        throw;
                    }
                }
                if (eventDetails.Price == null)
                {
                    try
                    {
                        throw new InvalidOperationException($"Event with ID {request.EventId} does not have a valid price.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Event with ID {EventId} does not have a valid price.", request.EventId);
                        throw;
                    }
                }

                if (request.UserId <= 0 || request.EventId <= 0 || request.TicketCount <= 0)
                {
                    try
                    {
                        throw new InvalidOperationException("Invalid registration/payment details. UserId, EventId, and TicketCount must be positive integers.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Invalid registration/payment details. UserId, EventId, and TicketCount must be positive integers.");
                        throw;
                    }
                }

                // Allow multiple registrations for the same user and event (no duplicate check)

                // Validate event status (not cancelled or ended)
                var eventType = eventDetails.GetType();
                var statusProp = eventType.GetProperty("Status");
                if (statusProp != null)
                {
                    var statusValue = statusProp.GetValue(eventDetails)?.ToString()?.ToLower();
                    if (statusValue == "cancelled")
                    {
                        try
                        {
                            throw new InvalidOperationException("Event is cancelled.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Event is cancelled.");
                            throw;
                        }
                    }
                }
                var endTimeProp = eventType.GetProperty("EndTime");
                if (endTimeProp != null)
                {
                    var endTimeValue = endTimeProp.GetValue(eventDetails) as DateTime?;
                    if (endTimeValue != null && endTimeValue < DateTime.UtcNow)
                    {
                        try
                        {
                            throw new InvalidOperationException("Event has already ended.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Event has already ended.");
                            throw;
                        }
                    }
                }

                // Attendee limit check
                if (eventDetails.MaxAttendees.HasValue)
                {
                    int currentAttendees = await _dbContext.Registrations
                        .CountAsync(r => r.EventId == (int)request.EventId);
                    if (currentAttendees + request.TicketCount > eventDetails.MaxAttendees.Value)
                    {
                        try
                        {
                            throw new InvalidOperationException($"Cannot register: Max attendees limit ({eventDetails.MaxAttendees.Value}) reached for event {request.EventId}.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Cannot register: Max attendees limit ({MaxAttendees}) reached for event {EventId}.", eventDetails.MaxAttendees.Value, request.EventId);
                            throw;
                        }
                    }
                }

                var correctAmount = eventDetails.Price.Value * request.TicketCount;

                // Fetch the event organizer's StripeAccountId
                var organizer = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == eventDetails.OrganizerId);
                string? organizerStripeAccountId = organizer?.StripeAccountId;
                if (string.IsNullOrWhiteSpace(organizerStripeAccountId))
                {
                    try
                    {
                        throw new InvalidOperationException("Event organizer does not have a Stripe account set up.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Event organizer does not have a Stripe account set up.");
                        throw;
                    }
                }
                Console.WriteLine($"[PAYMENT] Routing payment to organizer StripeAccountId: {organizerStripeAccountId}");

                // Set application fee (platform fee) as 10% of the transaction amount
                long applicationFeeAmount = (long)((double)correctAmount * 0.10 * 100); // 10% commission, in paise/cents

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(correctAmount * 100), // Convert to paise/cents
                    Currency = "inr",
                    TransferData = new PaymentIntentTransferDataOptions
                    {
                        Destination = organizerStripeAccountId
                    },
                    ApplicationFeeAmount = applicationFeeAmount,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", request.UserId.ToString() },
                        { "eventId", request.EventId.ToString() },
                        { "ticketCount", request.TicketCount.ToString() },
                        { "email", string.IsNullOrWhiteSpace(request.Email) ? "" : request.Email }
                    },
                    ReceiptEmail = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                return new PaymentResponseDto { ClientSecret = paymentIntent.ClientSecret };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent for EventId: {EventId}", request.EventId);
                throw;
            }
        }

        public async Task<CheckoutSessionResponseDto> CreateCheckoutSessionAsync(CheckoutSessionRequestDto request)
        {
            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            try
            {
                Console.WriteLine($"[REPO] CreateCheckoutSessionAsync called with: EventId={request?.EventId}, UserId={request?.UserId}, TicketCount={request?.TicketCount}, Currency={request?.Currency}, SuccessUrl={request?.SuccessUrl}, CancelUrl={request?.CancelUrl}");
                var eventDetails = request?.EventId != null ? await _dbContext.Events.FindAsync((int)request.EventId) : null;
                if (eventDetails == null)
                {
                    var eventIdMsg = request != null ? request.EventId.ToString() : "null";
                    Console.WriteLine($"[REPO][ERROR] Event with ID {eventIdMsg} not found.");
                    throw new KeyNotFoundException($"Event with ID {eventIdMsg} not found.");
                }

                if (eventDetails.Price == null)
                {
                    var eventIdMsg = request != null ? request.EventId.ToString() : "null";
                    Console.WriteLine($"[REPO][ERROR] Event with ID {eventIdMsg} does not have a valid price.");
                    throw new InvalidOperationException($"Event with ID {eventIdMsg} does not have a valid price.");
                }
                var correctAmount = (eventDetails.Price != null ? eventDetails.Price.Value : 0) * (request != null ? request.TicketCount : 0);

                    // Fetch the event organizer's StripeAccountId
                    var organizer = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == eventDetails.OrganizerId);
                    string? organizerStripeAccountId = organizer?.StripeAccountId;
                    if (string.IsNullOrWhiteSpace(organizerStripeAccountId))
                    {
                        Console.WriteLine($"[REPO][ERROR] Event organizer does not have a Stripe account set up for event {(request != null ? request.EventId.ToString() : "null") }.");
                        throw new InvalidOperationException("Event organizer does not have a Stripe account set up.");
                    }
                    Console.WriteLine($"[PAYMENT] Routing payment to organizer StripeAccountId: {organizerStripeAccountId}");

                // Ensure SuccessUrl and CancelUrl are valid URLs
                var frontendBaseUrl = _configuration["Frontend:Url"] ?? "https://dep-2-frontend.vercel.app";
                string successUrl = request != null && !string.IsNullOrWhiteSpace(request.SuccessUrl) ? request.SuccessUrl : frontendBaseUrl.TrimEnd('/') + "/payment-success";
                string cancelUrl = request != null && !string.IsNullOrWhiteSpace(request.CancelUrl) ? request.CancelUrl : frontendBaseUrl.TrimEnd('/') + "/payment-cancel";

                // Diagnostic: throw if URLs are not valid
                if (!successUrl.StartsWith("http://") && !successUrl.StartsWith("https://"))
                {
                    Console.WriteLine($"[REPO][ERROR] SuccessUrl is invalid: '{successUrl}'");
                    throw new InvalidOperationException($"SuccessUrl is invalid: '{successUrl}'");
                }
                if (!cancelUrl.StartsWith("http://") && !cancelUrl.StartsWith("https://"))
                {
                    Console.WriteLine($"[REPO][ERROR] CancelUrl is invalid: '{cancelUrl}'");
                    throw new InvalidOperationException($"CancelUrl is invalid: '{cancelUrl}'");
                }

                if (request == null || request.UserId <= 0)
                {
                    Console.WriteLine($"[REPO][ERROR] UserId must be a positive integer. Value: {(request != null ? request.UserId.ToString() : "null")}");
                    throw new InvalidOperationException("UserId must be a positive integer for Stripe metadata.");
                }
                // Set application fee (platform fee) in paise/cents
                long applicationFeeAmount = 5000; // e.g., ₹50 or $50, set as needed

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = request.Currency ?? "inr",
                                UnitAmount = (long)(correctAmount * 100),
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Event Registration"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", request.UserId.ToString() },
                        { "eventId", request.EventId.ToString() },
                        { "ticketCount", request.TicketCount.ToString() },
                           { "email", string.IsNullOrWhiteSpace(request.Email) ? "" : request.Email }
                    },
                    PaymentIntentData = new SessionPaymentIntentDataOptions
                    {
                        ApplicationFeeAmount = applicationFeeAmount,
                        TransferData = new SessionPaymentIntentDataTransferDataOptions
                        {
                            Destination = organizerStripeAccountId
                        },
                        Metadata = new Dictionary<string, string>
                        {
                            { "userId", request.UserId.ToString() },
                            { "eventId", request.EventId.ToString() },
                            { "ticketCount", request.TicketCount.ToString() },
                               { "email", string.IsNullOrWhiteSpace(request.Email) ? "" : request.Email }
                        }
                    },
                    CustomerEmail = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                Console.WriteLine($"[REPO] Stripe session created. Url: {session.Url}");
                return new CheckoutSessionResponseDto { Url = session.Url };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REPO][EXCEPTION] {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> ConfirmAndUpdateAsync(string paymentIntentId, string transactionId, string status, decimal amount, int userId, long eventId)
        {
            // Declare variables for email details
            string eventTitle = "";
            int numberOfTickets = 0;
            // Find payment by PaymentIntentId (store PaymentIntentId in TransactionId or add a new field if needed)
            var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.TransactionId == paymentIntentId);
            // Get user email and event title for storing in payment
            string? userEmailForPayment = null;
            string? eventTitleForPayment = null;
            var registrationForEmail = await _dbContext.Registrations
                .Where(r => r.UserId == userId && r.EventId == (int)eventId)
                .OrderByDescending(r => r.RegisteredAt)
                .FirstOrDefaultAsync();
            if (registrationForEmail != null)
                userEmailForPayment = registrationForEmail.UserEmail;
            var eventEntityForTitle = await _dbContext.Events.FirstOrDefaultAsync(e => e.EventId == (int)eventId);
            if (eventEntityForTitle != null)
                eventTitleForPayment = eventEntityForTitle.Title;
            if (payment == null)
            {
                // Double-check for duplicate before creating
                payment = await _dbContext.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.TransactionId == paymentIntentId);
                if (payment == null)
                {
                    payment = new EventSphere.Domain.Entities.Payment
                    {
                        TransactionId = paymentIntentId,
                        Status = status == "succeeded" ? PaymentResultStatus.Success : PaymentResultStatus.Failed,
                        Amount = amount,
                        PaymentTime = DateTime.UtcNow,
                        UserId = userId,
                        EventId = (int)eventId,
                        PaymentMethod = "Stripe",
                        UserEmail = userEmailForPayment,
                        EventTitle = eventTitleForPayment
                    };
                    _dbContext.Payments.Add(payment);
                    Console.WriteLine($"[PAYMENT][DB] New payment entry ADDED: TransactionId={paymentIntentId}, UserId={userId}, EventId={eventId}, Amount={amount}, Status={payment.Status}");
                }
                else
                {
                    Console.WriteLine($"[PAYMENT][DB] Duplicate payment detected for TransactionId={paymentIntentId}, SKIPPING creation.");
                }
            }
            else
            {
                var oldStatus = payment.Status;
                var oldAmount = payment.Amount;
                payment.Status = status == "succeeded" ? PaymentResultStatus.Success : PaymentResultStatus.Failed;
                payment.TransactionId = transactionId;
                payment.Amount = amount;
                payment.PaymentTime = DateTime.UtcNow;
                payment.UserEmail = userEmailForPayment;
                payment.EventTitle = eventTitleForPayment;
                Console.WriteLine($"[PAYMENT][DB] Payment entry UPDATED: PaymentId={payment.PaymentId}, TransactionId={transactionId}, UserId={userId}, EventId={eventId}, OldAmount={oldAmount}, NewAmount={amount}, OldStatus={oldStatus}, NewStatus={payment.Status}");
            }
            var changes = await _dbContext.SaveChangesAsync();
            Console.WriteLine($"[PAYMENT][DB] SaveChangesAsync returned: {changes}. PaymentId={payment.PaymentId}, TransactionId={payment.TransactionId}, Status={payment.Status}");

            // Update or create Registration after payment
            // Always get the latest registration for this user/event (by RegisteredAt descending)
            var registration = await _dbContext.Registrations
                .Where(r => r.UserId == userId && r.EventId == (int)eventId)
                .OrderByDescending(r => r.RegisteredAt)
                .FirstOrDefaultAsync();
            if (registration != null)
            {
                registration.PayStatus = PaymentStatus.Paid;
                registration.PaymentId = payment.PaymentId;
                await _dbContext.SaveChangesAsync();
            }
            else if (status == "succeeded")
            {
                // Try to find a valid OccurrenceId and TicketId for the event
                var occurrence = await _dbContext.EventOccurrences.FirstOrDefaultAsync(o => o.EventId == (int)eventId);
                var ticket = await _dbContext.Tickets.FirstOrDefaultAsync(t => t.EventId == (int)eventId);
                if (occurrence == null || ticket == null)
                {
                    Console.WriteLine($"[ERROR] Cannot create registration: Occurrence or Ticket not found for EventId={eventId}");
                }
                else
                {
                    var newRegistration = new EventSphere.Domain.Entities.Registration
                    {
                        UserId = userId,
                        EventId = (int)eventId,
                        Status = RegistrationStatus.Registered,
                        PayStatus = PaymentStatus.Paid,
                        RegisteredAt = DateTime.UtcNow,
                        // OccurrenceId = occurrence.OccurrenceId,
                        // TicketId = ticket.TicketId
                    };
                    _dbContext.Registrations.Add(newRegistration);
                    try
                    {
                        await _dbContext.SaveChangesAsync();
                        registration = newRegistration; // Use this for email below
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Failed to save registration: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
            // Send confirmation email after successful payment and registration
            if (status == "succeeded")
            {
                // Only use the email entered in the registration form
                string? userEmail = registration?.UserEmail;
                if (!string.IsNullOrWhiteSpace(userEmail))
                {
                    // Calculate number of tickets and event title before sending email
                    var eventEntity = await _dbContext.Events.FirstOrDefaultAsync(e => e.EventId == (int)eventId);
                    if (eventEntity != null && !string.IsNullOrWhiteSpace(eventEntity.Title))
                        eventTitle = eventEntity.Title;
                    if (eventEntity != null && eventEntity.Price > 0 && payment != null && payment.Amount > 0)
                        numberOfTickets = (int)(payment.Amount / eventEntity.Price);

                    try
                    {
                        using (var smtpClient = new System.Net.Mail.SmtpClient())
                        {
                            var smtpHost = _configuration["Smtp:Host"];
                            if (string.IsNullOrEmpty(smtpHost))
                            {
                                _logger.LogError("SMTP host is not configured. Please set Smtp:Host in configuration.");
                                throw new InvalidOperationException("SMTP host is not configured.");
                            }
                            smtpClient.Host = smtpHost;
                            smtpClient.Port = int.TryParse(_configuration["Smtp:Port"], out var port) ? port : 587;
                            smtpClient.EnableSsl = true;
                            smtpClient.Credentials = new System.Net.NetworkCredential(
                                _configuration["Smtp:User"] ?? "username",
                                _configuration["Smtp:Pass"] ?? "password"
                            );
                            var mail = new System.Net.Mail.MailMessage();
                            mail.From = new System.Net.Mail.MailAddress(_configuration["Smtp:From"] ?? "noreply@eventsphere.com", "EventSphere");
                            mail.To.Add(userEmail);
                            mail.Subject = "🎉 Event Registration & Payment Confirmation";
                            mail.IsBodyHtml = true;

                                                        string qrCodeHtml = "";
                                                        if (registration != null && !string.IsNullOrWhiteSpace(registration.QrCode))
                                                        {
                                                                qrCodeHtml = $"<div style='text-align:center;margin:32px 0;'><img src='data:image/png;base64,{registration.QrCode}' alt='QR Code' style='max-width:220px;border-radius:8px;box-shadow:0 1px 4px rgba(0,0,0,0.10);'/><div style='color:#555;font-size:0.95em;margin-top:8px;'>Scan this QR code at the event for check-in.</div></div>";

                                                                // Attach QR code as PNG file
                                                                try
                                                                {
                                                                    byte[] qrBytes = Convert.FromBase64String(registration.QrCode);
                                                                    var qrStream = new System.IO.MemoryStream(qrBytes);
                                                                    var attachment = new System.Net.Mail.Attachment(qrStream, "EventQRCode.png", "image/png");
                                                                    mail.Attachments.Add(attachment);
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    Console.WriteLine($"[EMAIL][QR][ERROR] Could not attach QR code: {ex.Message}");
                                                                }
                                                        }

                                                        mail.Body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;background:#f6f8fa;padding:32px;'>
    <div style='max-width:600px;margin:auto;background:#fff;border-radius:12px;box-shadow:0 2px 8px rgba(0,0,0,0.07);padding:32px;'>
        <h2 style='color:#4f46e5;text-align:center;margin-bottom:24px;'>Thank you for registering and completing payment!</h2>
        <table style='width:100%;border-collapse:collapse;margin-bottom:24px;'>
            <tr><td style='font-weight:600;padding:8px 0;'>Event Title:</td><td style='padding:8px 0;'>{eventTitle}</td></tr>
            <tr><td style='font-weight:600;padding:8px 0;'>Event ID:</td><td style='padding:8px 0;'>{eventId}</td></tr>
            <tr><td style='font-weight:600;padding:8px 0;'>User ID:</td><td style='padding:8px 0;'>{userId}</td></tr>
            <tr><td style='font-weight:600;padding:8px 0;'>Number of Tickets:</td><td style='padding:8px 0;'>{numberOfTickets}</td></tr>
            <tr><td style='font-weight:600;padding:8px 0;'>Amount Paid:</td><td style='padding:8px 0;'>₹{amount}</td></tr>
            <tr><td style='font-weight:600;padding:8px 0;'>Payment ID:</td><td style='padding:8px 0;'>{payment?.PaymentId}</td></tr>
            <tr><td style='font-weight:600;padding:8px 0;'>Transaction ID:</td><td style='padding:8px 0;'>{payment?.TransactionId}</td></tr>
            <tr><td style='font-weight:600;padding:8px 0;'>Registration ID:</td><td style='padding:8px 0;'>{registration?.RegistrationId}</td></tr>
        </table>
        {qrCodeHtml}
        <div style='text-align:center;margin-bottom:24px;'>
            <span style='display:inline-block;background:#e0e7ff;color:#3730a3;padding:12px 24px;border-radius:8px;font-size:1.1em;font-weight:600;'>You are now confirmed for the event!</span>
        </div>
        <div style='font-size:0.95em;color:#555;text-align:center;'>
            <p>We look forward to seeing you there.<br/>If you have any questions, reply to this email.</p>
            <p style='margin-top:32px;color:#888;'>Best regards,<br/><b>EventSphere Team</b></p>
        </div>
    </div>
</div>
";
                                                        await smtpClient.SendMailAsync(mail);
                                                }
                                                Console.WriteLine($"[EMAIL] Confirmation email sent to {userEmail}");
                                        }
                                        catch (Exception ex)
                                        {
                                                Console.WriteLine($"[EMAIL][ERROR] Failed to send confirmation email: {ex.Message}");
                                        }
                                }
                else
                {
                    Console.WriteLine($"[EMAIL][ERROR] No valid user email found for payment confirmation. userId={userId}, eventId={eventId}");
                }
            }
            return status == "succeeded";
        }

        /// <summary>
        /// Test method to verify SMTP email sending independently.
        /// </summary>
        public async Task<bool> SendTestEmailAsync(string toEmail)
        {
            try
            {
                Console.WriteLine($"[EMAIL][TEST] Attempting to send test email to {toEmail}");
                using (var smtpClient = new System.Net.Mail.SmtpClient())
                {
                    var smtpHost = _configuration["Smtp:Host"];
                    if (string.IsNullOrEmpty(smtpHost))
                    {
                        _logger.LogError("SMTP host is not configured. Please set Smtp:Host in configuration.");
                        throw new InvalidOperationException("SMTP host is not configured.");
                    }
                    smtpClient.Host = smtpHost;
                    smtpClient.Port = int.TryParse(_configuration["Smtp:Port"], out var port) ? port : 587;
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new System.Net.NetworkCredential(
                        _configuration["Smtp:User"] ?? "username",
                        _configuration["Smtp:Pass"] ?? "password"
                    );
                    var mail = new System.Net.Mail.MailMessage();
                    mail.From = new System.Net.Mail.MailAddress(_configuration["Smtp:From"] ?? "noreply@eventsphere.com", "EventSphere");
                    mail.To.Add(toEmail);
                    mail.Subject = "SMTP Test Email from EventSphere";
                    mail.IsBodyHtml = true;
                    mail.Body = "<h2>This is a test email from EventSphere backend.</h2><p>If you received this, SMTP is working!</p>";
                    await smtpClient.SendMailAsync(mail);
                }
                Console.WriteLine($"[EMAIL][TEST] Test email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL][TEST][ERROR] Failed to send test email: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
        public async Task AddAsync(EventSphere.Domain.Entities.Payment payment)
        {
            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<EventSphere.Domain.Entities.Payment>> GetPaymentsForEventAsync(int eventId)
        {
            return await _dbContext.Payments
                .Where(p => p.EventId == eventId)
                .OrderByDescending(p => p.PaymentTime)
                .ToListAsync();
        }

        public async Task<List<EventSphere.Domain.Entities.Payment>> GetAllEventPaymentsAsync()
        {
            return await _dbContext.Payments.ToListAsync();
        }

        public async Task<List<EventIncomeDto>> GetEventIncomeSummaryAsync()
        {
            var payments = await _dbContext.Payments.ToListAsync();
            var eventGroups = payments
                .Where(p => p.Status.ToString() == "Success" || (int)p.Status == 0)
                .GroupBy(p => new { p.EventId, p.EventTitle })
                .Select(g => new EventIncomeDto
                {
                    EventId = g.Key.EventId,
                    EventTitle = g.Key.EventTitle ?? "",
                    TotalAmount = g.Sum(p => p.Amount)
                })
                .ToList();
            return eventGroups;
        }
    }
}
