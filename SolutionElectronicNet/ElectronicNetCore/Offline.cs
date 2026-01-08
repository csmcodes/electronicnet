using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicNetCore
{
    public class Offline
    {

        public static string RecibirComprobante(string xml)
        {
            string urlfirma = "https://sicecloudstorage.blob.core.windows.net/firmas/0103567665001/cristhian_marcelo_sanmartin_munoz2021.p12";
            string xmlsigned = Firma.signxml(urlfirma, xml, "pjh2021", null);
            return xml;
        }
    }
}
