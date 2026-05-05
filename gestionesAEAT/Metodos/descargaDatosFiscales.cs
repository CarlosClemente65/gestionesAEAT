using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using gestionesAEAT.Utilidades;

namespace gestionesAEAT.Metodos
{
    public class refrenta
    {
        [XmlAttribute("servicio")]
        public string Servicio { get; set; }

        [XmlElement("status")]
        public int Status { get; set; }

        [XmlElement("error", IsNullable = true)]
        public elementosError Error { get; set; }
    }

    public class elementosError
    {
        [XmlElement("codigo")]
        public int Codigo { get; set; }

        [XmlElement("descripcion")]
        public string Descipcion { get; set; }
    }

    public class descargaDatosFiscales
    {
        string urlDescarga = string.Empty;
        string urlAutenticacion = "https://www2.agenciatributaria.gob.es/wlpl/ADHT-AUTH/AjaxRef";
        string ficheroSalida = Parametros.ficheroSalida;
        string ficheroResultado = Parametros.ficheroResultado;

        public void descargaDF()
        {
            //Metodo para descargar los datos fiscales
            //urlDescarga = Parametros.DescargarConCertificado ? Parametros.urlDescargaDfConCertificado : Parametros.urlDescargaDfConReferencia;

            // Llama al metodo que corresponde segun si se ha pasado o no la referencia de renta
            if(Parametros.DescargarConCertificado)
            {
                urlDescarga = Parametros.urlDescargaDfConCertificado;
                DescargaConCertificado();
            }
            else
            {
                urlDescarga = Parametros.urlDescargaDfConReferencia;
                DescargaConReferencia();
            }
        }


        private void DescargaConReferencia()
        {
            string respuestaAEAT = string.Empty;
            byte[] datosEnvio = null;
            try
            {
                //Se debe encriptar la referencia de la renta para pasarla a la AEAT
                string refEncriptada = encriptaRefRenta(Parametros.refRenta);
                StringBuilder datos = new StringBuilder();

                datos.Append($"nif={Parametros.nifDf}");
                datos.Append($"&refRenta={refEncriptada}");
                datos.Append($"&idioma=I");

                datosEnvio = Encoding.Default.GetBytes(datos.ToString()); //Codificacion en ansi

                respuestaAEAT = envioSolicitud(datosEnvio);

                string mensajeError = string.Empty;

                // Procesar la respuesta de la AEAT
                if(!string.IsNullOrEmpty(respuestaAEAT))
                {
                    tipoContenido tipoRespuesta = detectarTipoRespuestaAEAT(respuestaAEAT);
                    switch(tipoRespuesta)
                    {
                        case tipoContenido.XML:
                            try
                            {
                                refrenta respuestaXML = DeserializarXml<refrenta>(respuestaAEAT);
                                if(respuestaXML.Status != 0)
                                {
                                    mensajeError = $"Error en la descarga. Descripcion del error: {respuestaXML.Error.Descipcion}";
                                }
                            }

                            catch(Exception ex)
                            {
                                mensajeError = ($"Error en la descarga. Descripcion del error: {ex.Message}");
                            }

                            break;

                        case tipoContenido.HTML:
                            //Si es un html se graba el fichero para ver los errores
                            ficheroSalida = Path.ChangeExtension(ficheroSalida, "html");

                            break;

                        case tipoContenido.TXT:
                            //Si es un fichero de texto no hay que modificar nada

                            break;

                        case tipoContenido.desconocido:
                            mensajeError = $"Error en la descarga. Tipo de respuesta de la AEAT desconocida.";

                            break;

                        default:

                            break;
                    }
                    File.WriteAllText(ficheroResultado, "OK");
                }

                //Si se ha producido un error se graba la salida y no se procesan mas veces la salida (grabarSalida = true)
                if(!string.IsNullOrEmpty(mensajeError))
                {
                    Utiles.GrabarSalida(mensajeError, ficheroResultado);
                    Utiles.grabadaSalida = true;
                }

                if(string.IsNullOrEmpty(mensajeError) && !string.IsNullOrEmpty(respuestaAEAT)) Utiles.GrabarSalida(respuestaAEAT, ficheroSalida);
            }

            catch(Exception ex)
            {
                Utiles.GrabarSalida($"Error al descargar datos fiscales. {ex.Message}", ficheroResultado);
                Utiles.grabadaSalida = true;
            }
        }

        private void DescargaConCertificado()
        {
            string respuestaAEAT = string.Empty;
            string mensajeError = string.Empty;
            X509Certificate2 certificado = null; // Necesario para la autenticacion en la web de descarga de la AEAT

            bool resultado = false;
            (certificado, resultado) = Program.gestionCertificados.exportaCertificadoDigital(Parametros.serieCertificado);

            // Configuración de la URL para la descarga
            string urlPeticion = $"{Parametros.urlDescargaDfConCertificado}?nif={Parametros.nifDf}&pdp={Parametros.datosPersonales}";

            try
            {
                if(certificado != null)
                {
                    // Protocolo de seguridad requerido
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    HttpWebRequest solicitud = (HttpWebRequest)WebRequest.Create(urlPeticion);
                    solicitud.Method = "GET";
                    solicitud.UserAgent = "Mozilla/5.0"; // Evita bloqueos de seguridad básicos
                    solicitud.KeepAlive = true;

                    // Se añade el certificado a la peticion
                    solicitud.ClientCertificates.Add(certificado);

                    using(HttpWebResponse respuesta = (HttpWebResponse)solicitud.GetResponse())
                    {
                        // La AEAT suele usar Encoding.Default (ANSI) para los ficheros de datos
                        using(StreamReader reader = new StreamReader(respuesta.GetResponseStream(), Encoding.Default))
                        {
                            respuestaAEAT = reader.ReadToEnd();
                        }
                    }

                    // 2. Procesar la respuesta recibida
                    if(!string.IsNullOrEmpty(respuestaAEAT))
                    {
                        // Aquí reutilizas tu lógica de detección de tipo (XML, HTML, TXT)
                        tipoContenido tipoRespuesta = detectarTipoRespuestaAEAT(respuestaAEAT);

                        switch(tipoRespuesta)
                        {
                            case tipoContenido.XML:
                                try
                                {
                                    refrenta respuestaXML = DeserializarXml<refrenta>(respuestaAEAT);
                                    if(respuestaXML.Status != 0)
                                    {
                                        mensajeError = $"Error en la descarga. Descripcion del error: {respuestaXML.Error.Descipcion}";
                                    }
                                }

                                catch(Exception ex)
                                {
                                    mensajeError = ($"Error en la descarga. Descripcion del error: {ex.Message}");
                                }

                                break;

                            case tipoContenido.HTML:
                                //Si es un html se graba el fichero para ver los errores
                                ficheroSalida = Path.ChangeExtension(ficheroSalida, "html");

                                break;

                            case tipoContenido.TXT:
                                //Si es un fichero de texto no hay que modificar nada

                                break;

                            case tipoContenido.desconocido:
                                mensajeError = $"Error en la descarga. Tipo de respuesta de la AEAT desconocida.";

                                break;

                            default:

                                break;
                        }

                        //Si se ha producido un error se graba la salida y no se procesan mas veces la salida (grabarSalida = true)
                        if(!string.IsNullOrEmpty(mensajeError))
                        {
                            Utiles.GrabarSalida(mensajeError, ficheroResultado);
                            Utiles.grabadaSalida = true;
                        }

                        if(string.IsNullOrEmpty(mensajeError) && !string.IsNullOrEmpty(respuestaAEAT)) Utiles.GrabarSalida(respuestaAEAT, ficheroSalida);
                    }
                }
            }
            catch(WebException ex)
            {
                // Manejo de errores específicos de red o HTTP (ej. 403 No autorizado)[cite: 1]
                mensajeError = $"Error en la conexión con certificado: {ex.Message}";
                if(ex.Response != null)
                {
                    using(var reader = new StreamReader(ex.Response.GetResponseStream()))
                        respuestaAEAT = reader.ReadToEnd(); // Aquí puede venir el detalle del error en XML
                }
            }

            catch(Exception ex)
            {
                Utiles.GrabarSalida($"Error al descargar datos fiscales. {ex.Message}", ficheroResultado);
                Utiles.grabadaSalida = true;
            }
        }

        public string encriptaRefRenta(string refRenta)
        {
            //Metodo para encriptar la referencia renta ya que debe pasarse asi
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] bytes = Encoding.UTF8.GetBytes(refRenta);

            using(SHA512 sha = SHA512.Create())
            {
                byte[] hashBytes = sha.ComputeHash(bytes);
                StringBuilder refEncriptada = new StringBuilder(128);

                foreach(byte b in hashBytes)
                {
                    refEncriptada.Append(b.ToString("x2"));
                }
                return refEncriptada.ToString().ToUpper(); ;
            }
        }

        private string envioSolicitud(byte[] datosEnvio)
        {
            //Metodo para hacer el envio a Hacienda. No se incluye en el metodo envioAeat porque aqui se necesitan las cookies de sesion para validarse y descargar luego.
            string contenidoRespuesta = string.Empty;
            try
            {
                //Protocolo de seguridad
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                CookieContainer cookies = new CookieContainer(); //Para que todas las llamadas se hagan sobre la misma sesion

                //Crear datos para la solicitud HTTP
                HttpWebRequest solicitudHttp = (HttpWebRequest)WebRequest.Create(urlAutenticacion);

                //Configurar la solicitud
                solicitudHttp.Method = "POST";
                solicitudHttp.ContentType = "application/x-www-form-urlencoded";
                solicitudHttp.ContentLength = datosEnvio.Length;
                solicitudHttp.CookieContainer = cookies;
                solicitudHttp.KeepAlive = true;

                //Grabacion de los datos a enviar al servidor
                Stream requestStream = solicitudHttp.GetRequestStream();
                requestStream.Write(datosEnvio, 0, datosEnvio.Length);
                requestStream.Close();

                HttpWebResponse respuesta = (HttpWebResponse)solicitudHttp.GetResponse();
                Stream datosRespuesta = respuesta.GetResponseStream();
                StreamReader reader = new StreamReader(datosRespuesta, Encoding.UTF8, false, 512);
                contenidoRespuesta = reader.ReadToEnd();

                //Devuelve el estado 'OK' si todo ha ido bien
                string estadoRespuestaAEAT = respuesta.StatusDescription;

                if(estadoRespuestaAEAT == "OK")
                {
                    if(contenidoRespuesta.IndexOf("<status>0</status>") != -1)
                    {
                        urlDescarga = $"{Parametros.urlDescargaDfConCertificado}?nif={Parametros.nifDf}&pdp={Parametros.datosPersonales}";
                        HttpWebRequest solicitudHttp1 = (HttpWebRequest)WebRequest.Create(urlDescarga);
                        solicitudHttp1.CookieContainer = cookies;
                        solicitudHttp1.KeepAlive = true;
                        solicitudHttp1.Method = "GET";

                        HttpWebResponse respuesta1 = (HttpWebResponse)solicitudHttp1.GetResponse();
                        Stream datosRespuesta1 = respuesta1.GetResponseStream();
                        StreamReader reader1 = new StreamReader(datosRespuesta1, Encoding.Default, false, 512);

                        string estadoRespuestaAEAT1 = respuesta1.StatusDescription;
                        if(estadoRespuestaAEAT1 == "OK") contenidoRespuesta = reader1.ReadToEnd();
                    }
                    File.WriteAllText(ficheroResultado, "OK");
                }
            }
            catch(Exception ex)
            {
                Utiles.GrabarSalida($"Error en la conexion con el servidor. {ex.Message}", ficheroResultado);
                Utiles.grabadaSalida = true;
            }

            return contenidoRespuesta;
        }

        private T DeserializarXml<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using(StringReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        private enum tipoContenido
        {
            //Tipos de contenido que pueden tener las respuestas
            XML,
            HTML,
            TXT,
            desconocido
        }

        private tipoContenido detectarTipoRespuestaAEAT(string input)
        {
            //Metodo que devuelve el tipo de contenido que tiene una respuesta.
            //Comrpueba si es una cadena vacia o solo tiene espacios en blanco
            if(string.IsNullOrWhiteSpace(input)) return tipoContenido.desconocido;

            //Comprobar si es un XML
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(input);
                return tipoContenido.XML;
            }
            catch(XmlException)
            {
                //No es XML seguir con otras comprobaciones
            }

            //Comprobar si es un HTML (debe tener etiquetas de html)
            if(Regex.IsMatch(input, @"<\s*(html|head|body|title|meta|div|span|a|p)[^>]*>", RegexOptions.IgnoreCase)) return tipoContenido.HTML;

            //Comprobar si es texto plano (no debe contener etiquetas comunes de XML o HTML
            if(!Regex.IsMatch(input, @"<[^>]+>")) return tipoContenido.TXT;

            return tipoContenido.desconocido;
        }
    }
}
