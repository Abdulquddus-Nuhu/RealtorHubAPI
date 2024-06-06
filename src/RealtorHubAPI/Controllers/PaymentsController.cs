using Coinbase.Commerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RealtorHubAPI.Data;
using RealtorHubAPI.Entities.Identity;
using RealtorHubAPI.Models.Requests;
using RealtorHubAPI.Services;

namespace RealtorHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly CoinbaseService _coinbaseService;
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public PaymentsController(CoinbaseService coinbaseService, AppDbContext context, UserManager<User> userManager)
        {
            _coinbaseService = coinbaseService;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("initiate")]
        public async Task<IActionResult> InitiatePayment(PaymentInitiateRequest dto)
        {
            var user = await _userManager.GetUserAsync(User);
            var charge = await _coinbaseService.CreateChargeAsync(dto.Name, dto.Description, dto.Amount, dto.Currency);

            var payment = new Entities.Payment
            {
                ChargeId = charge.Id,
                Status = "PENDING",
                Amount = dto.Amount,
                Currency = dto.Currency,
                LandId = dto.LandId,
                UserId = user.Id
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(charge);
        }

        //[HttpPost("verify")]
        //public async Task<IActionResult> VerifyPayment(PaymentVerifyRequest dto)
        //{
        //    var charge = await _coinbaseService.RetrieveChargeAsync(dto.ChargeId);
        //    if (charge.Timeline.Exists(t => t.Status == "COMPLETED"))
        //    {
        //        var payment = await _context.Payments.SingleOrDefaultAsync(p => p.ChargeId == charge.Id);
        //        if (payment != null)
        //        {
        //            payment.Status = "COMPLETED";
        //            _context.Entry(payment).State = EntityState.Modified;
        //            await _context.SaveChangesAsync();
        //            return Ok(payment);
        //        }
        //    }

        //    return BadRequest("Payment not completed");
        //}

        //[HttpPost("webhook")]
        //public async Task<IActionResult> Webhook()
        //{
        //    //var request = await HttpContext.Request.ReadAsStringAsync();
        //    var request = await HttpContext.Request.ReadFromJsonAsync();
        //    var signature = Request.Headers["X-Cc-Webhook-Signature"];

        //    if (_coinbaseService.VerifySignature(request, signature))
        //    {
        //        var webhookEvent = JsonConvert.DeserializeObject<WebhookEvent>(request);

        //        if (webhookEvent.Type == "charge:confirmed")
        //        {
        //            var charge = webhookEvent.Data.As<Charge>();

        //            var payment = await _context.Payments.SingleOrDefaultAsync(p => p.ChargeId == charge.Id);
        //            if (payment != null)
        //            {
        //                payment.Status = "COMPLETED";
        //                _context.Entry(payment).State = EntityState.Modified;
        //                await _context.SaveChangesAsync();
        //            }
        //        }

        //        return Ok();
        //    }

        //    return Unauthorized();
        //}
    }
}
