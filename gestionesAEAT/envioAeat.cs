﻿using System;
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

namespace gestionesAEAT
{
    public class envioAeat
    {
        public string respuestaEnvioAEAT { get; set; }
        public string estadoRespuestaAEAT { get; set; }
        X509Certificate2 certificado = null;
        Utiles utilidad = new Utiles();


        public void envioPost(string url, string datosEnvio, string serieCertificado, gestionCertificados instanciaCertificados)
        {
            certificado = instanciaCertificados.buscarSerieCertificado(serieCertificado);

            if (certificado != null)
            {
                //Protocolo de seguridad
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //Crear datos para la solicitud HTTP
                HttpWebRequest solicitudHttp = (HttpWebRequest)WebRequest.Create(url);
                string contenidoRespuesta;

                //Configurar la solicitud
                solicitudHttp.Method = "POST";
                solicitudHttp.ContentType = "application/x-www-form-urlencoded";
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

                //Grabacion de la respuesta a la solicitud
                byte[] datosEnvioBytes = Encoding.UTF8.GetBytes(datosEnvio);
                solicitudHttp.ContentLength = datosEnvioBytes.Length;
                Stream requestStream = solicitudHttp.GetRequestStream();
                requestStream.Write(datosEnvioBytes, 0, datosEnvioBytes.Length);
                requestStream.Close();

                HttpWebResponse respuesta = (HttpWebResponse)solicitudHttp.GetResponse();
                //Devuelve el estado 'OK' si todo ha ido bien
                estadoRespuestaAEAT = respuesta.StatusDescription;

                //Grabar el contenido de la respuesta para devolverlo al metodo de llamada
                using (StreamReader sr = new StreamReader(respuesta.GetResponseStream()))
                {
                    StringBuilder sb = new StringBuilder();
                    while (!sr.EndOfStream)
                    {
                        sb.Append(sr.ReadLine());
                    }
                    contenidoRespuesta = sb.ToString();
                }

                respuestaEnvioAEAT = utilidad.quitaRaros(contenidoRespuesta);
            }
            else
            {
                //Si no se ha cargado el certificado, se devuelve una respuesta vacia.
                respuestaEnvioAEAT = string.Empty;
                estadoRespuestaAEAT = "KO";
            }
        }

        public void envioPostSinCertificado(string url, string datosEnvio)
        {
            //Protocolo de seguridad
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //Crear datos para la solicitud HTTP
            HttpWebRequest solicitudHttp = (HttpWebRequest)WebRequest.Create(url);
            string contenidoRespuesta;

            //Configurar la solicitud
            solicitudHttp.Method = "POST";
            solicitudHttp.ContentType = "application/x-www-form-urlencoded";
            solicitudHttp.ContentLength = datosEnvio.Length;

            //Grabacion de la respuesta a la solicitud
            byte[] datosEnvioBytes = Encoding.UTF8.GetBytes(datosEnvio);
            solicitudHttp.ContentLength = datosEnvioBytes.Length;
            Stream requestStream = solicitudHttp.GetRequestStream();
            requestStream.Write(datosEnvioBytes, 0, datosEnvioBytes.Length);
            requestStream.Close();

            HttpWebResponse respuesta = (HttpWebResponse)solicitudHttp.GetResponse();
            //Devuelve el estado 'OK' si todo ha ido bien
            estadoRespuestaAEAT = respuesta.StatusDescription;

            //Grabar el contenido de la respuesta para devolverlo al metodo de llamada
            using (StreamReader sr = new StreamReader(respuesta.GetResponseStream()))
            {
                StringBuilder sb = new StringBuilder();
                while (!sr.EndOfStream)
                {
                    sb.Append(sr.ReadLine());
                }
                contenidoRespuesta = sb.ToString();
            }
            //respuestaEnvioAEAT = utilidad.quitaRaros(contenidoRespuesta);
            respuestaEnvioAEAT = contenidoRespuesta;
        }
    }
}
