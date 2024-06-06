using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RealtorHubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected string Id => GetClaim(ClaimTypes.NameIdentifier) ?? string.Empty;
        protected string Username => GetClaim(ClaimTypes.Name) ?? string.Empty;
        protected string Email => GetClaim(ClaimTypes.Email) ?? string.Empty;
        private string? GetClaim(string claimType) => User.Claims
            .FirstOrDefault(x => x.Type.Equals(claimType))?.Value;


        protected Guid UserId => GetClaimAsGuid(ClaimTypes.NameIdentifier);

        private Guid GetClaimAsGuid(string claimType)
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == claimType);
            if (claim != null && Guid.TryParse(claim.Value, out Guid guid))
            {
                return guid;
            }
            return Guid.Empty;
        }


        //protected string Role => GetClaim(TrickingLibraryConstants.Claims.Role);

        //var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), // Ensure this is stored as a GUID
        //    new Claim(ClaimTypes.Email, persona.Email),
        //    new Claim(ClaimTypes.Name, persona.Email),
        //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //    new Claim("fullName", persona.FirstName + " " + persona.LastName)
        //};


    }
}
