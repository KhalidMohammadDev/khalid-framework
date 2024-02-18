using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Khalid.Core.Framework
{

    public class FileModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("generatedName")]
        public string GeneratedName { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
