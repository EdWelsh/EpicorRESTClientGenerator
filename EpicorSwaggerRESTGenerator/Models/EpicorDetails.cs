using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicorSwaggerRESTGenerator.Models
{
    public class EpicorDetails
    {
        public string Namespace { get; set; }
        public bool useBaseClass { get; set; }
        public string BaseClass { get; set; }
        public string APIURL { get; set; }
        public string Project { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
