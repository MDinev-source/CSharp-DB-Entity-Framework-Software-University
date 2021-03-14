namespace Cinema.DataProcessor.ExportDto
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class ExportMovieCustomersDto
    {
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("Balance")]
        public string Balance { get; set; }

        

    }
}
