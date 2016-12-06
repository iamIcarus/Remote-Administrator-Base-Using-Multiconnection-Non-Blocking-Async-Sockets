using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class ConnectionEventArgs
    {
        public int ConnectionType { get; set; }
        public Guid Id { get; set; }
        public string RemoteIp { get; set; }

    }
}
