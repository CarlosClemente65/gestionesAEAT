using System;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Text.RegularExpressions;
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
        string nifDf = string.Empty;
        string refRenta = string.Empty;
        string datosPersonales = string.Empty;
        string ficheroSalida = Parametros.ficheroSalida;
        string ficheroResultado = Parametros.ficheroResultado;

        Utiles utilidad = Program.utilidad;


        public void descargaDF()
        {
            //Metodo para descargar los datos fiscales
            urlDescarga = Parametros.urlDescargaDf;
            nifDf = Parametros.nifDf;
            refRenta = Parametros.refRenta;
            datosPersonales = Parametros.datosPersonales;
            string respuestaAEAT = string.Empty;
            byte[] datosEnvio = null;

            try
            {
                //Se debe encriptar la referencia de la renta para pasarla a la AEAT
                string refEncriptada = encriptaRefRenta(refRenta);
                StringBuilder datos = new StringBuilder();

                datos.Append($"nif={nifDf}");
                datos.Append($"&refRenta={refEncriptada}");
                datos.Append($"&idioma=I");

                datosEnvio = Encoding.Default.GetBytes(datos.ToString()); //Codificacion en ansi

                respuestaAEAT = envioSolicitud(datosEnvio);

                string mensajeError = string.Empty;
                if (!string.IsNullOrEmpty(respuestaAEAT))
                {
                    tipoContenido tipoRespuesta = detectarTipoRespuestaAEAT(respuestaAEAT);
                    switch (tipoRespuesta)
                    {
                        case tipoContenido.XML:
                            try
                            {
                                refrenta respuestaXML = DeserializarXml<refrenta>(respuestaAEAT);
                                if (respuestaXML.Status != 0)
                                {
                                    mensajeError = $"Error en la descarga. Descripcion del error: {respuestaXML.Error.Descipcion}";
                                }
                            }

                            catch (Exception ex)
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
                if (!string.IsNullOrEmpty(mensajeError))
                {
                    utilidad.GrabarSalida(mensajeError, ficheroResultado);
                    utilidad.grabadaSalida = true;
                }

                if (string.IsNullOrEmpty(mensajeError) && !string.IsNullOrEmpty(respuestaAEAT)) utilidad.GrabarSalida(respuestaAEAT, ficheroSalida);
            }

            catch (Exception ex)
            {
                utilidad.GrabarSalida($"Error al descargar datos fiscales. {ex.Message}", ficheroResultado);
                utilidad.grabadaSalida = true;
            }
        }

        public string encriptaRefRenta(string refRenta)
        {
            //Metodo para encriptar la referencia renta ya que debe pasarse asi
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] bytes = Encoding.UTF8.GetBytes(refRenta);

            using (SHA512 sha = SHA512.Create())
            {
                byte[] hashBytes = sha.ComputeHash(bytes);
                StringBuilder refEncriptada = new StringBuilder(128);

                foreach (byte b in hashBytes)
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

                if (estadoRespuestaAEAT == "OK")
                {
                    if (contenidoRespuesta.IndexOf("<status>0</status>") != -1)
                    {
                        urlDescarga = $"{urlDescarga}?nif={nifDf}&pdp={datosPersonales}";
                        HttpWebRequest solicitudHttp1 = (HttpWebRequest)WebRequest.Create(urlDescarga);
                        solicitudHttp1.CookieContainer = cookies;
                        solicitudHttp1.KeepAlive = true;
                        solicitudHttp1.Method = "GET";

                        HttpWebResponse respuesta1 = (HttpWebResponse)solicitudHttp1.GetResponse();
                        Stream datosRespuesta1 = respuesta1.GetResponseStream();
                        StreamReader reader1 = new StreamReader(datosRespuesta1, Encoding.Default, false, 512);

                        string estadoRespuestaAEAT1 = respuesta1.StatusDescription;
                        if (estadoRespuestaAEAT1 == "OK") contenidoRespuesta = reader1.ReadToEnd();
                    }
                    File.WriteAllText(ficheroResultado, "OK");
                }
            }
            catch (Exception ex)
            {
                utilidad.GrabarSalida($"Error en la conexion con el servidor. {ex.Message}", ficheroResultado);
                utilidad.grabadaSalida = true;
            }

            return contenidoRespuesta;
        }

        private T DeserializarXml<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
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
            if (string.IsNullOrWhiteSpace(input)) return tipoContenido.desconocido;

            //Comprobar si es un XML
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(input);
                return tipoContenido.XML;
            }
            catch (XmlException)
            {
                //No es XML seguir con otras comprobaciones
            }

            //Comprobar si es un HTML (debe tener etiquetas de html)
            if (Regex.IsMatch(input, @"<\s*(html|head|body|title|meta|div|span|a|p)[^>]*>", RegexOptions.IgnoreCase)) return tipoContenido.HTML;

            //Comprobar si es texto plano (no debe contener etiquetas comunes de XML o HTML
            if (!Regex.IsMatch(input, @"<[^>]+>")) return tipoContenido.TXT;

            return tipoContenido.desconocido;
        }
    }
}
