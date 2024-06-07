﻿namespace RealtorHubAPI.Entities
{
    public class LandVideo
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public Guid LandId { get; set; }
        public Land Land { get; set; }
    }

}