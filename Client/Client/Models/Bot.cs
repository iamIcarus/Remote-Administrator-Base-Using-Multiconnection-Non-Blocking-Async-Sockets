using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class Bot
    {
        public int No { get; set; }

        public Guid Id { get; set; }

        public string IP { get; set; }

        public string ComputerId { get; set; }

        public int Lag { get; set; }

    }
}
