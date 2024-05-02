using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Net;

namespace consultaModelos
{
    public class envioAeat
    {
        string respuestaAeat = string.Empty;
        X509Certificate2 certificado = null;
        string estado = string.Empty; //Almacena el resultado del envio a la AEAT


        public string envioPost(string url, string datosEnvio, string serieCertificado)
        {
            gestionCertificados gestionCertificado = new gestionCertificados();
            certificado = gestionCertificado.buscarSerieCertificado(serieCertificado);

            if (certificado != null)
            {
                //Protocolo de seguridad
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //Crear datos para la solicitud HTTP
                HttpWebRequest solicitudHttp = HttpWebRequest.Create(url) as HttpWebRequest;
                string contenidoRespuesta;

                //Configurar la solicitud
                solicitudHttp.Method = "POST";
                solicitudHttp.ContentType = "applicacion/x-www-form-urlencoded";
                solicitudHttp.ContentLength = datosEnvio.Length;

                if (certificado != null)
                {
                    solicitudHttp.ClientCertificates.Add(certificado);
                }

                using (Stream requestStream = solicitudHttp.GetRequestStream())
                {
                    requestStream.Write(System.Text.Encoding.UTF8.GetBytes(datosEnvio), 0, datosEnvio.Length);
                }

                HttpWebResponse respuesta = (HttpWebResponse)solicitudHttp.GetResponse();
                estado = respuesta.StatusDescription;

                using (StreamReader sr = new StreamReader(respuesta.GetResponseStream()))
                {
                    StringBuilder sb = new StringBuilder();
                    while (!sr.EndOfStream)
                    {
                        sb.Append(sr.ReadLine());
                    }
                    contenidoRespuesta = sb.ToString();
                }

                return contenidoRespuesta; 
            }
            return string.Empty;
        }
    }
}
