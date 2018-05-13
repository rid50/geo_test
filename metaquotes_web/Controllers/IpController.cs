using metaquotes_web.Controllers;
using System.Web.Http;
using System.Web.Mvc;

namespace metaquotes_web
{
    public class IpController : ApiController
    {
        // GET ip/location
        public JsonResult GetIP(string ip)
        {
            uint uip = Helper.IpStringToIpUint(ip);

            int index = -1;
            for (int i = 0; i < Cache.ips.Length; i++)
            {
                if (uip < Cache.ips[i].ip_from)
                    continue;
                if (uip > Cache.ips[i].ip_to)
                    continue;
                index = (int)Cache.ips[i].location_index;
            }

            object data;
            if (index != -1)
                data = Helper.GenerateJsonResult(Cache.locations[index]);
            else
                data = new { error = "Данные отсутствуют" };

            return new JsonResult { Data = data, ContentType = "application/json; charset=UTF-8" };
        }
    }
}
