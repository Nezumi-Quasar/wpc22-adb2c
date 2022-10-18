using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Linq;
using WPC22Demo05Api.Model;
using System.Net;
using System.Xml.Linq;

namespace WPC22Demo05Api
{
    public static class CheckEmailDomain
    {
        [FunctionName("CheckEmailDomain")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "checkemail")] HttpRequest req,
            ILogger log)
        {
            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string userEmailAddress = data?.signinName;

            if (userEmailAddress == null)
                return new BadRequestResult();

            var blackList = System.Text.Json.JsonSerializer.Deserialize<string[]>(File.ReadAllText("Assets/domains.json"));
            if (MailAddress.TryCreate(userEmailAddress, out MailAddress parsedEmail)
                && !blackList.Contains(userEmailAddress.Split('@')[1].ToLower()))
            {
                return new OkObjectResult(new B2CResponseContent
                {
                    code = "API200",
                    userMessage = "L'indirizzo email utilizzato è valido",
                    developerMessage = String.Empty,
                    moreInfo = String.Empty,
                    requestId = GetRequestId(req),
                    status = (int)HttpStatusCode.OK,
                    version = typeof(CheckEmailDomain).Assembly.ImageRuntimeVersion

                });   
            }
            return new ConflictObjectResult(new B2CResponseContent
            {
                code = "API 409",
                userMessage = "L'indirizzo email utilizzato non è valido",
                developerMessage = String.Empty,
                moreInfo = String.Empty,
                requestId = GetRequestId(req),
                status = (int)HttpStatusCode.Conflict,
                version = typeof(CheckEmailDomain).Assembly.ImageRuntimeVersion

            });
        }


        private static string GetRequestId(HttpRequest req)
        {
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }
            var item = req.HttpContext.Items["MS_AzureFunctionsRequestID"];
            if (item is string && item != null)
            {
                return item as string;
            }
            return null;

        }

    }
}
