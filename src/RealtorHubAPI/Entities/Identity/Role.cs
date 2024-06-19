using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;

namespace RealtorHubAPI.Entities.Identity
{
    public class Role : IdentityRole<int>
    {
        public Role()
        {
        }

        public Role(string roleName)
        {
            Name = roleName;
        }

    }

}
