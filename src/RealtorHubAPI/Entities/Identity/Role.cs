using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;

namespace RealtorHubAPI.Entities.Identity
{
    public class Role : IdentityRole<Guid>
    {
        public Role()
        {
            Id = Guid.NewGuid();
        }

        public Role(string roleName)
        {
            Id = Guid.NewGuid();
            Name = roleName;
        }

    }

}
