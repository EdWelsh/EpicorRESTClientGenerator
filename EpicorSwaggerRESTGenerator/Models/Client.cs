using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EpicorSwaggerRESTGenerator.Models
{
    public static class Client
    {
        public static WebClient getWebClient(string username = "", string password = "")
        {
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback += (senderC, cert, chain, sslPolicyErrors) => true;
            if (!string.IsNullOrEmpty(username))
            {
                client.Credentials = new NetworkCredential() { UserName = username, Password = password };
            }else
            {
                client.UseDefaultCredentials = true;
            }         
            return client;
        }
    }
}
