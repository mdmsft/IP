using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace IP.Models
{
    public sealed class DnsParameters
    {
        public readonly IDictionary<string, string> Query;

        public readonly IDictionary<string, string> Headers;

        public DnsParameters()
        {
            Headers = new Dictionary<string, string>();
            Query = new Dictionary<string, string>();
        }

        public DnsParameters(IHeaderDictionary headers, IQueryCollection query)
        {
            Headers = new Dictionary<string, string>(headers.Select(header => new KeyValuePair<string, string>(header.Key, header.Value)));
            Query = new Dictionary<string, string>(query.Select(q => new KeyValuePair<string, string>(q.Key, q.Value)));
        }
    }
}
