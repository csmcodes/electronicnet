using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using java.security;
using java.io;
using java.util;
using java.security.cert;
using javax.xml.parsers;
using es.mityc.javasign.pkstore;
using es.mityc.javasign.pkstore.keystore;
using es.mityc.javasign.trust;
using es.mityc.javasign.xml.xades.policy;
using es.mityc.firmaJava.libreria.xades;
using es.mityc.javasign.xml.refs;
using es.mityc.firmaJava.libreria.utilidades;
using org.w3c.dom;
using sviudes.blogspot.com;
using es.mityc.firmaJava.role;
using System.IO;

namespace ElectronicNetCore
{
    public class Firma
    {
        public static Document LoadXMLFromString(String xmlString)
        {
            com.sun.org.apache.xerces.@internal.jaxp.SAXParserFactoryImpl s = new com.sun.org.apache.xerces.@internal.jaxp.SAXParserFactoryImpl();
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder builder = factory.newDocumentBuilder();
            InputStream inputStream = new ByteArrayInputStream(Encoding.UTF8.GetBytes(xmlString));
            org.w3c.dom.Document document = builder.parse(inputStream);
            return document;
        }
        private static Document LoadXML(string path)
        {
            DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
            dbf.setNamespaceAware(true);
            return dbf.newDocumentBuilder().parse(new BufferedInputStream(new FileInputStream(path)));
        }

        private static byte[] GetStreamFromUrl(string url)
        {
            byte[] imageData = null;

            using (var wc = new System.Net.WebClient())
                imageData = wc.DownloadData(url);

            return imageData;
            //return new MemoryStream(imageData);
        }

        private static X509Certificate LoadCertificate(string path, string password, out PrivateKey privateKey, out Provider provider, int? indice)
        {
            X509Certificate certificate = null;
            provider = null;
            privateKey = null;

            //Cargar certificado de fichero PFX  
            KeyStore ks = KeyStore.getInstance("PKCS12");

            FileInputStream fs = new FileInputStream(path);

            try
            {
                ks.load(new BufferedInputStream(fs), password.ToCharArray());
                IPKStoreManager storeManager = new KSStore(ks, new PassStoreKS(password));
                List certificates = storeManager.getSignCertificates();

                //Si encontramos el certificado...  
                if (!indice.HasValue)
                    indice = 0;
                //if (certificates.size() == 1)
                // {
                certificate = (X509Certificate)certificates.get(indice.Value);

                // Obtención de la clave privada asociada al certificado  
                privateKey = storeManager.getPrivateKey(certificate);

                // Obtención del provider encargado de las labores criptográficas  
                provider = storeManager.getProvider(certificate);
            }
            catch (Exception ex)
            {
                fs.close();
                throw ex;
            }

            //}

            return certificate;
        }
        private static X509Certificate LoadCertificateFromUrl(string url, string password, out PrivateKey privateKey, out Provider provider, int? indice)
        {
            X509Certificate certificate = null;
            provider = null;
            privateKey = null;

            //Cargar certificado de fichero PFX  
            KeyStore ks = KeyStore.getInstance("PKCS12");

            //java.io.File file = new java.io.File(new java.net.URI(url));
            //FileInputStream fs = new FileInputStream(file);

            byte[] byteArray = GetStreamFromUrl(url);

            java.io.InputStream fs = new java.io.ByteArrayInputStream(byteArray);

            try
            {
                ks.load(new BufferedInputStream(fs), password.ToCharArray());
                IPKStoreManager storeManager = new KSStore(ks, new PassStoreKS(password));
                List certificates = storeManager.getSignCertificates();

                //Si encontramos el certificado...  
                if (!indice.HasValue)
                    indice = 0;
                //if (certificates.size() == 1)
                // {
                certificate = (X509Certificate)certificates.get(indice.Value);

                // Obtención de la clave privada asociada al certificado  
                privateKey = storeManager.getPrivateKey(certificate);

                // Obtención del provider encargado de las labores criptográficas  
                provider = storeManager.getProvider(certificate);
            }
            catch (Exception ex)
            {
                fs.close();
                throw ex;
            }

            //}

            return certificate;
        }

        public static string signxml(string pathcertificado, string xml, string certificado, string password, int? indice)
        {
            string retorno = "";
            PrivateKey privateKey;
            Provider provider;
            //X509Certificate certificate = LoadCertificate("c:\\temp\\certificado.pfx", "contraseña", out privateKey, out provider);

            //string pathfirma = pathcertificado + "\\" + certificado;
            //string pathxml = path + "\\temp\\" + numero + "_" + empresa + ".xml";
            string urlfirma = pathcertificado + "/" + certificado;
            X509Certificate certificate = LoadCertificateFromUrl(urlfirma, password, out privateKey, out provider, indice);
            //Si encontramos el certificado...  
            if (certificate != null)
            {
                //Política de firma (Con las librerías JAVA, esto se define en tiempo de ejecución)  
                TrustFactory.instance = es.mityc.javasign.trust.TrustExtendFactory.newInstance();
                TrustFactory.truster = es.mityc.javasign.trust.MyPropsTruster.getInstance();
                PoliciesManager.POLICY_SIGN = new es.mityc.javasign.xml.xades.policy.facturae.Facturae31Manager();
                PoliciesManager.POLICY_VALIDATION = new es.mityc.javasign.xml.xades.policy.facturae.Facturae31Manager();

                DataToSign dataToSign = new DataToSign();
                dataToSign.setXadesFormat(EnumFormatoFirma.XAdES_BES);
                dataToSign.setEsquema(XAdESSchemas.XAdES_132);
                dataToSign.setPolicyKey("factura");
                dataToSign.setAddPolicy(true);
                dataToSign.setXMLEncoding("UTF-8");
                dataToSign.addClaimedRol(new SimpleClaimedRole("Rol de firma"));
                dataToSign.setEnveloped(true);
                //dataToSign.addObject(new ObjectToSign(new AllXMLToSign(), "Documento de ejemplo", null, "text/xml", null));
                dataToSign.addObject(new ObjectToSign(new InternObjectToSign("comprobante"), "Documento de ejemplo", null, "text/xml", null));
                dataToSign.setParentSignNode("comprobante");
                //dataToSign.setDocument(LoadXMLFromString(xml));
                dataToSign.setDocument(LoadXML(xml));
                //Crear datos a firmar  
                /*DataToSign dataToSign = new DataToSign();
                dataToSign.setXadesFormat(EnumFormatoFirma.XAdES_BES); //XAdES-EPES                                  
                dataToSign.setEsquema(XAdESSchemas.XAdES_132);
                dataToSign.setPolicyKey("facturae31"); //Da igual lo que pongamos aquí, la política de firma se define arriba  
                dataToSign.setAddPolicy(true);
                dataToSign.setXMLEncoding("UTF-8");
                dataToSign.setEnveloped(true);
                dataToSign.addObject(new ObjectToSign(new AllXMLToSign(), "Descripcion del documento", null, "text/xml", null));                
                dataToSign.setDocument(LoadXML(path));*/

                //Firmar  
                Object[] res = new FirmaXML().signFile(certificate, dataToSign, privateKey, provider);


                OutputStream a = new ByteArrayOutputStream();
                //FileOutputStream a = new FileOutputStream(pathxml);
                // Guardamos la firma a un fichero en el home del usuario  
                UtilidadTratarNodo.saveDocumentToOutputStream((Document)res[0], a, true);

                retorno = a.toString();
                a.close();



            }
            return retorno;

        }

        public static string signxml(string urlcertificado, string xml, string password, int? indice)
        {
            string retorno = "";
            PrivateKey privateKey;
            Provider provider;
            //X509Certificate certificate = LoadCertificate("c:\\temp\\certificado.pfx", "contraseña", out privateKey, out provider);

            //string pathfirma = pathcertificado + "\\" + certificado;
            //string pathxml = path + "\\temp\\" + numero + "_" + empresa + ".xml";           
            X509Certificate certificate = LoadCertificateFromUrl(urlcertificado, password, out privateKey, out provider, indice);
            //Si encontramos el certificado...  
            if (certificate != null)
            {
                //Política de firma (Con las librerías JAVA, esto se define en tiempo de ejecución)  
                TrustFactory.instance = es.mityc.javasign.trust.TrustExtendFactory.newInstance();
                TrustFactory.truster = es.mityc.javasign.trust.MyPropsTruster.getInstance();
                PoliciesManager.POLICY_SIGN = new es.mityc.javasign.xml.xades.policy.facturae.Facturae31Manager();
                PoliciesManager.POLICY_VALIDATION = new es.mityc.javasign.xml.xades.policy.facturae.Facturae31Manager();

                DataToSign dataToSign = new DataToSign();
                dataToSign.setXadesFormat(EnumFormatoFirma.XAdES_BES);
                dataToSign.setEsquema(XAdESSchemas.XAdES_132);
                dataToSign.setPolicyKey("factura");
                dataToSign.setAddPolicy(true);
                dataToSign.setXMLEncoding("UTF-8");
                dataToSign.addClaimedRol(new SimpleClaimedRole("Rol de firma"));
                dataToSign.setEnveloped(true);
                //dataToSign.addObject(new ObjectToSign(new AllXMLToSign(), "Documento de ejemplo", null, "text/xml", null));
                dataToSign.addObject(new ObjectToSign(new InternObjectToSign("comprobante"), "Documento de ejemplo", null, "text/xml", null));
                dataToSign.setParentSignNode("comprobante");
                //dataToSign.setDocument(LoadXMLFromString(xml));
                dataToSign.setDocument(LoadXMLFromString(xml));
                //Crear datos a firmar  
                /*DataToSign dataToSign = new DataToSign();
                dataToSign.setXadesFormat(EnumFormatoFirma.XAdES_BES); //XAdES-EPES                                  
                dataToSign.setEsquema(XAdESSchemas.XAdES_132);
                dataToSign.setPolicyKey("facturae31"); //Da igual lo que pongamos aquí, la política de firma se define arriba  
                dataToSign.setAddPolicy(true);
                dataToSign.setXMLEncoding("UTF-8");
                dataToSign.setEnveloped(true);
                dataToSign.addObject(new ObjectToSign(new AllXMLToSign(), "Descripcion del documento", null, "text/xml", null));                
                dataToSign.setDocument(LoadXML(path));*/

                //Firmar  
                Object[] res = new FirmaXML().signFile(certificate, dataToSign, privateKey, provider);


                OutputStream a = new ByteArrayOutputStream();
                //FileOutputStream a = new FileOutputStream(pathxml);
                // Guardamos la firma a un fichero en el home del usuario  
                UtilidadTratarNodo.saveDocumentToOutputStream((Document)res[0], a, true);

                retorno = a.toString();
                a.close();



            }
            return retorno;

        }


        public static DateTime FromUnixTime(long unixTimeMillis)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTimeMillis);
        }


        public static string GetCertificateData(string path, string certificado, string password, string pathcertificado, ref DateTime desde, ref DateTime hasta)
        {
            PrivateKey privateKey;
            Provider provider;
            string pathfirma = path + "\\" + pathcertificado;

            try
            {

                X509Certificate certificate = LoadCertificate(pathfirma + "\\" + certificado, password, out privateKey, out provider, null);
                //Si encontramos el certificado...  
                if (certificate != null)
                {
                    desde = FromUnixTime(certificate.getNotBefore().getTime());
                    hasta = FromUnixTime(certificate.getNotAfter().getTime());

                    return "OK";
                }
                return "NO hay certificado";
            }
            catch (Exception ex)
            {
                if (ex.Message == "")
                    return "Archivo de certificado incorrecto";
                if (ex.Message.Contains("failed to decrypt"))
                    return "Contraseña incorrecta";
                return ex.Message;
            }


        }

    }
}
