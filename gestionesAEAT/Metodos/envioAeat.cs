﻿using System.IO;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System;

namespace gestionesAEAT
{
    public class envioAeat
    {
        public string respuestaEnvioAEAT { get; set; } //Almacena la respuesta del servidor en formato string
        public byte[] respuestaEnvioAEATBytes { get; set; } //Almacena la respuesta del servidor en formato bytes (si es un fichero PDF).
        public string estadoRespuestaAEAT { get; set; } //Almacena si ha habido algun problema de conexion
        string contenidoRespuesta; //Almacena la respuesta del servidor para poder quitar simbolos extraños
        X509Certificate2 certificado = null;
        Utiles utilidad = new Utiles();


        public void envioPost(string url, string datosEnvio, string serieCertificado, gestionCertificados instanciaCertificados, string tipoEnvio = "form") //Se pone el tipo de envio opcional como formulario y si es de tipo json se debe pasar en la llamada al metodo
        {
            try
            {
                certificado = instanciaCertificados.buscarSerieCertificado(serieCertificado);

                if (certificado != null)
                {
                    //Protocolo de seguridad
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    //Crear datos para la solicitud HTTP
                    HttpWebRequest solicitudHttp = (HttpWebRequest)WebRequest.Create(url);

                    //Configurar la solicitud
                    solicitudHttp.Method = "POST";
                    
                    //configura el contenido en funcion de si el envio se hace por un formulario o un json
                    if (tipoEnvio == "form")
                    {
                        solicitudHttp.ContentType = "application/x-www-form-urlencoded";
                    }
                    else if (tipoEnvio == "json")
                    {
                        solicitudHttp.ContentType = "application/json;charset=UTF-8";
                    }

                    //solicitudHttp.ContentType = "application/x-www-form-urlencoded";
                    solicitudHttp.ContentLength = datosEnvio.Length;
                    solicitudHttp.ClientCertificates.Add(certificado);
                    //solicitudHttp.Proxy = null;
                    //Las lineas siguientes estan comentadas porque no es necesaria la identificacion del navegador que realiza la solicitud (UserAGent). Habria que poner una de ellas pero pueden cambiar con el tiempo, asi que no se usan
                    //Firefox
                    //solicitudHttp.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:27.0) Gecko/20100101 Firefox/27.0"; 
                    //Chrome
                    //solicitudHttp.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.9999.999 Safari/537.36";
                    //Edge
                    //solicitudHttp.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.182 Safari/537.36 Edg/88.0.705.81";

                    //Grabacion de los datos a enviar al servidor
                    byte[] datosEnvioBytes = Encoding.UTF8.GetBytes(datosEnvio);
                    solicitudHttp.ContentLength = datosEnvioBytes.Length;
                    Stream requestStream = solicitudHttp.GetRequestStream();
                    requestStream.Write(datosEnvioBytes, 0, datosEnvioBytes.Length);
                    requestStream.Close();

                    HttpWebResponse respuesta = (HttpWebResponse)solicitudHttp.GetResponse();
                    //Devuelve el estado 'OK' si todo ha ido bien
                    estadoRespuestaAEAT = respuesta.StatusDescription;

                    //Grabar el contenido de la respuesta para devolverlo al metodo de llamada
                    using (MemoryStream ms = new MemoryStream())
                    {
                        respuesta.GetResponseStream().CopyTo(ms);
                        contenidoRespuesta = Encoding.UTF8.GetString(ms.ToArray()); //Se pasa a una variable temporal para poder pasar el quita raros.

                        //Grabacion de la respuesta en formato bytes (si es una consulta si se usa directamente, en el resto de metodos no esta claro si tiene uso, pero lo dejo por compatibilidad)
                        ms.Seek(0, SeekOrigin.Begin);
                        respuestaEnvioAEATBytes = ms.ToArray();
                    }

                    respuestaEnvioAEAT = utilidad.quitaRaros(contenidoRespuesta); //Solo se quitan los caracteres raros en el string, ya que en byte no procede
                }
                else
                {
                    //Si no se ha cargado el certificado, se devuelve una respuesta vacia.
                    respuestaEnvioAEAT = string.Empty;
                    respuestaEnvioAEATBytes = null;
                    estadoRespuestaAEAT = "KO";
                }
            }

            catch (Exception ex)
            {
                File.WriteAllText("errores.txt", $"Error en la conexion con el servidor. {ex.Message}");
            }
        }

        public void envioPostSinCertificado(string url, string datosEnvio, string tipoEnvio)
        {
            //Mismo metodo que el anterior pero cuando no se necesita certificado 
            try
            {
                //Protocolo de seguridad
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //Crear datos para la solicitud HTTP
                HttpWebRequest solicitudHttp = (HttpWebRequest)WebRequest.Create(url);

                //Configurar la solicitud
                solicitudHttp.Method = "POST";

                //configura el contenido en funcion de si el envio se hace por un formulario o un json
                if (tipoEnvio == "form")
                {
                    solicitudHttp.ContentType = "application/x-www-form-urlencoded";
                }
                else if (tipoEnvio == "json")
                {
                    solicitudHttp.ContentType= "application/json;charset=UTF-8";
                }

                solicitudHttp.ContentLength = datosEnvio.Length;

                //Grabacion de los datos a enviar al servidor
                byte[] datosEnvioBytes = Encoding.UTF8.GetBytes(datosEnvio);
                solicitudHttp.ContentLength = datosEnvioBytes.Length;
                Stream requestStream = solicitudHttp.GetRequestStream();
                requestStream.Write(datosEnvioBytes, 0, datosEnvioBytes.Length);
                requestStream.Close();

                HttpWebResponse respuesta = (HttpWebResponse)solicitudHttp.GetResponse();
                //Devuelve el estado 'OK' si todo ha ido bien
                estadoRespuestaAEAT = respuesta.StatusDescription;

                //Grabar el contenido de la respuesta para devolverlo al metodo de llamada
                using (MemoryStream ms = new MemoryStream())
                {
                    respuesta.GetResponseStream().CopyTo(ms);
                    respuestaEnvioAEAT = Encoding.UTF8.GetString(ms.ToArray());

                    //Grabacion de la respuesta en formato bytes para el metodo de consulta de modelos que la respuesta siempre sera un PDF en base64 (se lee directamente desde el metodo esta variable)
                    ms.Seek(0, SeekOrigin.Begin);
                    respuestaEnvioAEATBytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("errores.txt", $"Error en la conexion con el servidor. {ex.Message}");
            }
        }
    }
}
