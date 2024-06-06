﻿namespace RealtorHubAPI.Models.Responses
{
    public record UserResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string? MiddleName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }

        public string FullName { get => $"{FirstName} {LastName}"; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

}
