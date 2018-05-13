using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace metaquotes_web.Controllers
{
    public class Helper
    {
        public static object GenerateJsonResult(Location location, bool ok) {
            if (ok)
            {
                var data = new
                {
                    city = System.Text.Encoding.UTF8.GetString(location.city).Replace("\0", string.Empty),
                    country = System.Text.Encoding.UTF8.GetString(location.country).Replace("\0", string.Empty).Substring(4),
                    region = System.Text.Encoding.UTF8.GetString(location.region).Replace("\0", string.Empty).Substring(4),
                    postal = System.Text.Encoding.UTF8.GetString(location.postal).Replace("\0", string.Empty).Substring(4),
                    organization = System.Text.Encoding.UTF8.GetString(location.organization).Replace("\0", string.Empty).Substring(4),
                    latitude = location.latitude,
                    longitude = location.longitude
                };

                return data;
            }
            else
                return new { error = "Данные отсутствуют" };
        }
    }
}