using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class Responce
    {
       public Guid SockId { get; set; }
       public String Code { get; set; }
       public List<Object> Data { get; set; }
       
    }
}
