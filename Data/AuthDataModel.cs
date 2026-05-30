using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data
{
    public class AuthDataModel
    {
        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
