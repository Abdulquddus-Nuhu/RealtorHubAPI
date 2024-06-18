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


        protected int UserId => GetClaimAsInteger("id");

        private int GetClaimAsInteger(string claimType)
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == claimType);
            if (claim != null && int.TryParse(claim.Value, out int guid))
            {
                return guid;
            }
            return 0;
        }


        //protected string Role => GetClaim(TrickingLibraryConstants.Claims.Role);

    }
}
