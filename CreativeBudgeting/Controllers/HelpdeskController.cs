using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using CreativeBudgeting.Models;
using CreativeBudgeting.Services;
using CreativeBudgeting.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreativeBudgeting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelpdeskController : ControllerBase
    {
        private readonly BudgetDbContext _context;
        private readonly IEmailService _emailService;


        public HelpdeskController(BudgetDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("helpdesk-ticket")]
        public async Task<IActionResult> CreateHelpdeskTicket(HelpdeskTicketDto ticket)
        {
            try
            {
                var helpdeskTicket = new HelpdeskTicket
                {
                    Id = ticket.Id ?? 0,
                    Name = ticket.Name,
                    Email = ticket.Email,
                    Subject = ticket.Subject,
                    TicketSeverityId = ticket.TicketSeverityId,
                    Message = ticket.Message,
                };
                
                _context.HelpdeskTickets.Add(helpdeskTicket);
                await _context.SaveChangesAsync();

                // Send auto-reply confirmation email
                if (!string.IsNullOrWhiteSpace(helpdeskTicket.Email))
                {
                    await SendTicketConfirmationEmail(helpdeskTicket);
                }

                return Ok(helpdeskTicket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating ticket: {ex}");
                return StatusCode(500, new { message = "Failed to create ticket", error = ex.Message });
            }
        }

        private async Task SendTicketConfirmationEmail(HelpdeskTicket ticket)
        {
            try
            {
                var emailSubject = $"Ticket Confirmation - We've Received Your Request #{ticket.Id}";
                var emailContent = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                            <h2 style='margin: 0;'>Support Ticket Confirmation</h2>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; border: 1px solid #dee2e6;'>
                            <p>Dear {ticket.Name},</p>
                            
                            <p>Thank you for contacting our support team! We have successfully received your support ticket and wanted to confirm that we're here to help.</p>
                            
                            <div style='background-color: white; padding: 20px; border-left: 4px solid #28a745; margin: 20px 0; border-radius: 4px;'>
                                <h4 style='color: #28a745; margin-top: 0;'>📋 Ticket Details:</h4>
                                <p><strong>Ticket ID:</strong> #{ticket.Id}</p>
                                <p><strong>Subject:</strong> {ticket.Subject}</p>
                                <p><strong>Submitted:</strong> {DateTime.Now:MMMM dd, yyyy 'at' h:mm tt}</p>
                            </div>
                            
                            <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0; border-radius: 4px;'>
                                <h4 style='color: #856404; margin-top: 0;'>⏰ What to Expect:</h4>
                                <p style='margin-bottom: 0; color: #856404;'>Our support team will review your request and respond within <strong>24-72 hours</strong>. We appreciate your patience as we work to provide you with the best possible assistance.</p>
                            </div>
                            
                            <div style='background-color: white; padding: 20px; border: 1px solid #dee2e6; margin: 20px 0; border-radius: 4px;'>
                                <h4 style='margin-top: 0;'>📞 Need Immediate Assistance?</h4>
                                <p>If your issue is urgent, please don't hesitate to contact us directly:</p>
                                <ul>
                                    <li>Email: dchambers@creativecanvasdesigns.net</li>
                                    <li>Phone: (817) 526-9864</li>
                                </ul>
                            </div>
                            
                            <p>We value your business and are committed to resolving your request as quickly as possible.</p>
                            
                            <p>Best regards,<br>
                            <strong>Creative Canvas Designs Support Team</strong></p>
                        </div>
                        
                        <div style='text-align: center; padding: 20px; color: #6c757d; font-size: 12px;'>
                            <p>This is an automated confirmation email. Please do not reply to this message.</p>
                            <p>Ticket Reference: #{ticket.Id} | Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                        </div>
                    </div>
                </body>
                </html>";

                await _emailService.SendEmailAsync(ticket.Email, emailSubject, emailContent);
                Console.WriteLine($"✅ Auto-reply confirmation sent to {ticket.Email} for ticket #{ticket.Id}");
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the ticket creation
                Console.WriteLine($"❌ Failed to send auto-reply email for ticket #{ticket.Id}: {ex.Message}");
            }
        }

        [HttpGet("helpdesk-tickets")]
        public async Task<IActionResult> GetHelpdeskTickets()
        {
            var tickets = await _context.HelpdeskTickets.ToListAsync();

            // Map HelpdeskTicket to HelpdeskTicketDto if needed
            var ticketDtos = tickets.Select(t => new HelpdeskTicketDto
            {
                Id = t.Id,
                Name = t.Name,
                Email = t.Email,
                Subject = t.Subject,
                TicketSeverityId = t.TicketSeverityId,
                TicketSeverityName = t.TicketSeverity?.Value,
                Message = t.Message,
                IsResolved = t.IsResolved
            }).OrderByDescending(t => t.IsResolved != true).ToList();

            return Ok(ticketDtos);
        }

        [HttpGet("helpdesk-ticket/{id}")]
        public async Task<IActionResult> GetHelpdeskTicket([FromRoute] int id)
        {
            var ticket = await _context.HelpdeskTickets.Include(ht => ht.TicketSeverity).Where(ht => ht.Id == id).FirstOrDefaultAsync();
            var result = new HelpdeskTicketDto
            {
                Id = ticket?.Id,
                Name = ticket?.Name,
                Email = ticket?.Email,
                Subject = ticket?.Subject,
                TicketSeverityId = ticket?.TicketSeverityId,
                TicketSeverityName = ticket?.TicketSeverity?.Value,
                Message = ticket?.Message,
                IsResolved = ticket.IsResolved
            };
            return Ok(result);
        }
        [HttpPatch("resolve-ticket/{id}")]
        public async Task<IActionResult> ResolveTicket(int id)
        {
            try
            {
                var ticket = await _context.HelpdeskTickets.FirstOrDefaultAsync(ht => ht.Id == id);
                if (ticket == null)
                {
                    return NotFound();
                }

                // Check if ticket is already resolved to avoid duplicate emails
                bool wasAlreadyResolved = ticket.IsResolved;

                ticket.IsResolved = true;
                _context.HelpdeskTickets.Update(ticket);
                await _context.SaveChangesAsync();

                // Send resolution email only if ticket wasn't already resolved
                if (!wasAlreadyResolved && !string.IsNullOrWhiteSpace(ticket.Email))
                {
                    await SendTicketResolutionEmail(ticket);
                }

                return Ok(new { 
                    success = true, 
                    message = "Ticket resolved successfully",
                    ticket = ticket,
                    emailSent = !wasAlreadyResolved && !string.IsNullOrWhiteSpace(ticket.Email)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resolving ticket: {ex}");
                return StatusCode(500, new { message = "Failed to resolve ticket", error = ex.Message });
            }
        }

        [HttpPatch("unresolve-ticket/{id}")]
        public async Task<IActionResult> UnresolveTicket(int id)
        {
            var ticket = await _context.HelpdeskTickets.FirstOrDefaultAsync(ht => ht.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }
            ticket.IsResolved = false;
            _context.HelpdeskTickets.Update(ticket);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                success = true,
                message = "Ticket unresolved successfully",
                
            });
        }

        private async Task SendTicketResolutionEmail(HelpdeskTicket ticket)
        {
            try
            {
                var emailSubject = $"Ticket Resolved - Your Support Request #{ticket.Id} is Complete";
                var emailContent = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                            <h2 style='margin: 0;'>✅ Ticket Resolved</h2>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; border: 1px solid #dee2e6;'>
                            <p>Dear {ticket.Name},</p>
                            
                            <p>Great news! We're pleased to inform you that your support ticket has been <strong>successfully resolved</strong>.</p>
                            
                            <div style='background-color: white; padding: 20px; border-left: 4px solid #28a745; margin: 20px 0; border-radius: 4px;'>
                                <h4 style='color: #28a745; margin-top: 0;'>📋 Resolved Ticket Details:</h4>
                                <p><strong>Ticket ID:</strong> #{ticket.Id}</p>
                                <p><strong>Subject:</strong> {ticket.Subject}</p>
                                <p><strong>Status:</strong> <span style='color: #28a745; font-weight: bold;'>RESOLVED</span></p>
                                <p><strong>Resolved on:</strong> {DateTime.Now:MMMM dd, yyyy 'at' h:mm tt}</p>
                            </div>
                            
                            <div style='background-color: #d1ecf1; padding: 15px; border-left: 4px solid #17a2b8; margin: 20px 0; border-radius: 4px;'>
                                <h4 style='color: #0c5460; margin-top: 0;'>💡 Was this helpful?</h4>
                                <p style='margin-bottom: 0; color: #0c5460;'>We hope our support team was able to resolve your issue satisfactorily. Your feedback helps us improve our service quality.</p>
                            </div>
                            
                            <div style='background-color: white; padding: 20px; border: 1px solid #dee2e6; margin: 20px 0; border-radius: 4px;'>
                                <h4 style='margin-top: 0;'>🔄 Need Further Assistance?</h4>
                                <p>If you have any additional questions or if this issue resurfaces, please don't hesitate to contact us:</p>
                                <ul>
                                    <li>Email: dchambers@creativecanvasdesigns.net</li>
                                    <li>Phone: (817) 526-9864</li>
                                </ul>
                                <p><em>You can also submit a new support ticket through our website if needed.</em></p>
                            </div>
                            
                            <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0; border-radius: 4px;'>
                                <h4 style='color: #856404; margin-top: 0;'>⭐ Leave a Review</h4>
                                <p style='margin-bottom: 0; color: #856404;'>If you're satisfied with our service, we'd appreciate if you could leave a review or testimonial. Your feedback means the world to us!</p>
                            </div>
                            
                            <p>Thank you for choosing Creative Canvas Designs. We appreciate your business and look forward to serving you again in the future.</p>
                            
                            <p>Best regards,<br>
                            <strong>Creative Canvas Designs Support Team</strong></p>
                        </div>
                        
                        <div style='text-align: center; padding: 20px; color: #6c757d; font-size: 12px;'>
                            <p>This is an automated resolution notification. Please contact us if you need further assistance.</p>
                            <p>Ticket Reference: #{ticket.Id} | Resolved on {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                        </div>
                    </div>
                </body>
                </html>";

                await _emailService.SendEmailAsync(ticket.Email, emailSubject, emailContent);
                Console.WriteLine($"✅ Resolution notification sent to {ticket.Email} for ticket #{ticket.Id}");
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the ticket resolution
                Console.WriteLine($"❌ Failed to send resolution email for ticket #{ticket.Id}: {ex.Message}");
            }
        }

        [HttpPost("{id}/reply")]
        public async Task<IActionResult> SendReply(int id, [FromBody] TicketReplyDto replyDto)
        {
            try
            {
                var ticket = await _context.HelpdeskTickets
                    .Include(t => t.TicketSeverity)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                {
                    return NotFound("Ticket not found");
                }

                // Validate email addresses
                if (string.IsNullOrWhiteSpace(ticket.Email))
                {
                    return BadRequest("Ticket does not have a valid email address");
                }

                if (string.IsNullOrWhiteSpace(replyDto.Message))
                {
                    return BadRequest("Reply message cannot be empty");
                }

                var emailSubject = $"Re: {ticket.Subject} - Ticket #{ticket.Id}";
                var emailContent = $@"
            <html>
            <body>
                <h3>Support Team Reply</h3>
                <p>Hello {ticket.Name},</p>
                <p>We have replied to your support ticket:</p>
                <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 20px 0;'>
                    {replyDto.Message}
                </div>
                <p>If you have any further questions, please don't hesitate to contact us.</p>
                <p>Best regards,<br>Support Team</p>
                <hr>
                <small>Ticket ID: {ticket.Id}</small>
            </body>
            </html>";

                await _emailService.SendEmailAsync(ticket.Email, emailSubject, emailContent);

                return Ok(new { 
                    success = true, 
                    message = "Reply sent and email delivered successfully",
                    emailDetails = new {
                        sentTo = ticket.Email,
                        subject = emailSubject,
                        isDevelopment = true,
                        note = "In development mode, emails are logged to console instead of being sent"
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Full exception: {ex}");
                return StatusCode(500, new { success = false, message = "Failed to send reply", error = ex.Message });
            }
        }


    }
}
