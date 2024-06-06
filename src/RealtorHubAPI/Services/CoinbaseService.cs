using System;
using System.Threading.Tasks;
using Coinbase.Commerce;
using Coinbase.Commerce.Models;
using Microsoft.Extensions.Options;
using RealtorHubAPI.Configurations;

namespace RealtorHubAPI.Services
{
    public class CoinbaseService
    {
        private readonly CommerceApi _coinbaseClient;
        private readonly string _webhookSecret;

        public CoinbaseService(IOptions<CoinbaseOptions> options)
        {
            _coinbaseClient = new CommerceApi(options.Value.ApiKey);
            _webhookSecret = options.Value.WebhookSecret;
        }

        /// <summary>
        /// Creates a charge for the specified product or service.
        /// </summary>
        public async Task<Charge> CreateChargeAsync(string name, string description, decimal amount, string currency)
        {
            var charge = new CreateCharge
            {
                Name = name,
                Description = description,
                PricingType = PricingType.FixedPrice,
                LocalPrice = new Money { Amount = amount, Currency = currency },
            };

            try
            {
                var response = await _coinbaseClient.CreateChargeAsync(charge);
                return response.Data;
            }
            catch (Exception ex)
            {
                // Handle the exception (log, notify user, etc.)
                Console.WriteLine($"Error creating charge: {ex.Message}");
                throw; // Rethrow the exception or handle it as needed
            }
        }

        /// <summary>
        /// Retrieves details of a previously created charge.
        /// </summary>
        public async Task<Charge> RetrieveChargeAsync(string chargeId)
        {
            var response = await _coinbaseClient.GetChargeAsync(chargeId);
            return response.Data;
        }

        /// <summary>
        /// Verifies the signature of a webhook payload.
        /// </summary>
        //public bool VerifySignature(string payload, string signature)
        //{
        //    return CoinbaseSignature.IsValid(payload, signature, _webhookSecret);
        //}
    }
}
