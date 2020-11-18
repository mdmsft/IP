namespace IP.Models
{
    public sealed class DnsPayload
    {
        public string V4 { get; set; }

        public string V6 { get; set; }

        public string Domain { get; set; }

        public string Client { get; set; }

        public string Secret { get; set; }

        public bool IsDualStack { get; set; }

        public string ZoneName => string.Join('.', Domain.Split('.')[1..]);

        public string RecordSetName => Domain.Split('.')[0];
    }
}
