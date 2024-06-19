﻿using Microsoft.AspNetCore.Identity;
using RealtorHubAPI.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace RealtorHubAPI.Entities.Identity
{
    public class User : IdentityUser<int>
    {
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(50)]
        public string? MiddleName { get; set; }

        public string FullName { get => $"{FirstName} {LastName}"; }

        public RoleType Role { get; set; }


        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }


        [StringLength(50)]
        public string? Pin { get; set; }

        public string? PinHash { get; set; }
        public bool IsActive { get; set; } = false;
 


        public bool IsDeleted { get; set; }
        public string? DeletedBy { get; protected set; } = string.Empty;
        public virtual DateTime? Deleted { get; protected set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime? Modified { get; protected set; }
        public virtual string? LastModifiedBy { get; protected set; }

        public User(DateTime created, bool isDeleted)
        {
            Created = created;
            IsDeleted = isDeleted;
        }
        public User() : this(DateTime.UtcNow, false) { }
    }

}
