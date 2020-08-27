﻿using Nop.Core.Configuration;

namespace Nop.Plugin.Api.Domain
{
    public class ApiSettings : ISettings
    {
        public bool EnableApi { get; set; } = true;

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public int TokenExpiryInDays { get; set; } = 0;
    }
}
