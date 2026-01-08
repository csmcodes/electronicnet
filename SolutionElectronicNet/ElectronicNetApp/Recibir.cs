using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using ElectronicNetCore;

namespace ElectronicNetApp
{
    public static class Recibir
    {
        [FunctionName("recibirxml")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            string requestBody = await req.Content.ReadAsStringAsync();
            Models.RecepcionRequest request = JsonConvert.DeserializeObject<Models.RecepcionRequest>(requestBody);
            string xmlsigned = Offline.RecibirComprobante(request.xml);
            //log.Info("C# HTTP trigger function processed a request.");
            return req.CreateResponse(HttpStatusCode.OK, "Recibir  " + xmlsigned);

            /*// parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            if (name == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                name = data?.name;
            }

            return name == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "Hello Friend " + name);*/
        }
    }
}
