using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.CloudGames.Games.Infrastructure.Configuration.Search
{
    public sealed class ElasticsearchSettings
    {
        public string Url { get; set; } = "http://localhost:9200";
        public string Index { get; set; } = "games";
    }
}
