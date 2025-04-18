﻿using gestionesAEAT.Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace gestionesAEAT.Metodos
{
    //Clase que contiene el envio (header y body)
    [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope
    {
        [XmlElement(ElementName = "Header", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public string Header { get; set; } = string.Empty;

        [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public Body Body { get; set; }
    }

    public class Body
    {
        //Clase que contiene el desglose del body
        [XmlElement(ElementName = "VNifV2Ent", Namespace = "http://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/VNifV2Ent.xsd")]
        public VNifV2Ent VNifV2Ent { get; set; }
    }

    public class VNifV2Ent
    {
        //Clase que contiene el desglose de VNifV2Ent
        [XmlElement("Contribuyente")]
        public List<Contribuyente> Contribuyentes { get; set; } = new List<Contribuyente>();
    }

    public class Contribuyente
    {
        //Clase que contiene el desglose del contribuyente
        [XmlElement("Nif")]
        public string Nif { get; set; }

        [XmlElement("Nombre")]
        public string Nombre { get; set; }
    }


    [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class EnvelopeResponse
    {
        //Clase que contiene la respuesta (solo viene el body)
        [XmlElement(ElementName = "Body")]
        public BodyResponse Body { get; set; }
    }

    public class BodyResponse
    {
        //Clase que contiene el desglose del body de la respuesta
        //Campo para la respuesta correcta
        [XmlElement(ElementName = "VNifV2Sal", Namespace = "http://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/VNifV2Sal.xsd")]
        public VNifV2Sal VNifV2Sal { get; set; }

        //Campo para si viene un error
        [XmlElement(ElementName = "Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public RespuestaError respuestaError { get; set; }

    }

    [XmlRoot("VNifV2Sal", Namespace = "http://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/VNifV2Sal.xsd")]
    public class VNifV2Sal
    {
        //Clase que contiene el desglose de VNifV2Sal
        [XmlElement("Contribuyente")]
        public List<RespuestaContribuyente> Respuestas { get; set; } = new List<RespuestaContribuyente>();
    }

    public class RespuestaContribuyente
    {
        //Clase que contiene la respuesta valida del contribuyente
        [XmlElement("Nif")]
        public string Nif { get; set; }

        [XmlElement("Nombre")]
        public string Nombre { get; set; }

        [XmlElement("Resultado")]
        public string Resultado { get; set; }

    }

    [XmlRoot("Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class RespuestaError
    {
        //Clase que contiene el desglose de la respuesta erronea
        [XmlElement("faultcode", Namespace ="")]
        public string FaultCode { get; set; }

        [XmlElement("faultstring", Namespace ="")]
        public string FaultString { get; set; }
    }

    public class validarNif
    {
        envioAeat envio = new envioAeat();
        string estadoRespuestaAEAT = string.Empty;
        string contenidoRespuesta = string.Empty;
        string respuestaEnvioAEAT = string.Empty;
        string urlValida = @"https://www1.agenciatributaria.gob.es/wlpl/BURT-JDIT/ws/VNifV2SOAP";

        public async Task envioPeticion()
        {
            var datosEnvio = asignarValores();
            var envelope = new Envelope
            {
                Body = new Body
                {
                    VNifV2Ent = datosEnvio
                }
            };

            // Serializamos el objeto a XML
            string xmlDatos = SerializarAxml(envelope);

            // Enviamos la solicitud al servidor
            await EnviarSolicitud(xmlDatos, urlValida, Parametros.serieCertificado);

            if (estadoRespuestaAEAT == "OK")
            {
                var envelopeResponse = DeserializarRespuesta(respuestaEnvioAEAT);

                var datosSalida = new StringBuilder();
                // Acceder a la lista de respuestas si no hay errores en el envio
                if (envelopeResponse.Body.VNifV2Sal != null)
                {
                    foreach (var respuesta in envelopeResponse.Body.VNifV2Sal.Respuestas)
                    {
                        datosSalida.AppendLine($"NIF={respuesta.Nif}");
                        datosSalida.AppendLine($"NOMBRE={respuesta.Nombre}");
                        datosSalida.AppendLine($"RESULTADO={respuesta.Resultado}");
                    }
                }
                else if (envelopeResponse.Body.respuestaError != null)
                {
                    //Cuando hay algun error en el envio, puede venir una respuesta con el error
                    datosSalida.AppendLine($"ERROR={envelopeResponse.Body.respuestaError.FaultString}");
                }


                Utiles.GrabarSalida(datosSalida.ToString(), Parametros.ficheroSalida);


                //////Esta parte genera un xml con las respuestas y lo graba al ficheroSalida
                ////var xmlSalida = new XmlSerializer(typeof(EnvelopeResponse));
                ////using (var streamWriter = new StreamWriter(Parametros.ficheroSalida))
                ////{
                ////    xmlSalida.Serialize(streamWriter, envelopeResponse);
                ////}

            }
            Utiles.GrabarSalida("OK", Parametros.ficheroResultado);
        }


        private string FormatearXml(string xmlOriginal)
        {
            //Este metodo es para intentar formatear la respuesta del xml quitando los espacios al inicio, y poniendo un salto de linea antes de cada cierre de etiqueta que es como se hacia con el programa anterior. Lo dejo por referencia pero no se utiliza (se graba un fichero de texto con los valores)
            // Cargar el XML original en un StringBuilder
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(xmlOriginal);

            // Procesar las líneas del StringBuilder
            var lines = stringBuilder.ToString().Split(new[] { '\n' }, StringSplitOptions.None);
            var nuevoXml = new StringBuilder();

            foreach (var line in lines)
            {
                // Eliminar espacios al principio de la línea
                string nuevaLinea = line.TrimStart();

                // Comprobar si la línea contiene una etiqueta de cierre
                if (nuevaLinea.StartsWith("</"))
                {
                    // Añadir un salto de línea antes de la etiqueta de cierre
                    nuevoXml.AppendLine();
                }

                // Añadir la línea procesada al nuevo XML
                nuevoXml.AppendLine(nuevaLinea);
            }

            return nuevoXml.ToString();
        }

        public VNifV2Ent asignarValores()
        {
            //Procesa el guion para asignar los valores a las propiedades de la clase de envio
            var contribuyentes = new List<Contribuyente>();
            string rutaArchivo = Parametros.ficheroEntrada;

            Contribuyente contribuyenteActual = null;

            List<string> lineasGuion = new List<string>();
            using (StreamReader sr = new StreamReader(rutaArchivo)) 
            {
                string linea;
                while ((linea = sr.ReadLine()) != null)
                {
                    lineasGuion.Add(linea);
                }
            }

            foreach (var linea in lineasGuion)
            {
                (string clave, string valor) = Utiles.divideCadena(linea, '=');

                if (clave.Equals("NIF", StringComparison.OrdinalIgnoreCase))
                {
                    // Si ya tenemos un contribuyente en progreso (con Nif pero sin Nombre), lo añadimos antes de empezar uno nuevo
                    if (contribuyenteActual != null)
                    {
                        contribuyentes.Add(contribuyenteActual);
                    }
                    // Creamos un nuevo contribuyente y asignamos el Nif
                    contribuyenteActual = new Contribuyente { Nif = valor };
                }
                else if (clave.Equals("NOMBRE", StringComparison.OrdinalIgnoreCase) && contribuyenteActual != null)
                {
                    // Si encontramos el Nombre, lo asignamos al contribuyente actual
                    contribuyenteActual.Nombre = valor ;
                    contribuyentes.Add(contribuyenteActual); // Añadimos el contribuyente completo a la lista
                    contribuyenteActual = null; // Reiniciamos para el próximo contribuyente
                }

            }

            return new VNifV2Ent { Contribuyentes = contribuyentes };
        }

        private string SerializarAxml(Envelope envelope)
        {
            //Serializa en un XML los valores de las propiedades de la clase de envio
            var xmlSerializer = new XmlSerializer(typeof(Envelope));

            // Definir los espacios de nombres con prefijos requeridos
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
            namespaces.Add("vnif", "http://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/burt/jdit/ws/VNifV2Ent.xsd");
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(false)))
                {
                    using (var xmlWriter = XmlWriter.Create(streamWriter, new XmlWriterSettings { Indent = true }))
                    {
                        xmlSerializer.Serialize(xmlWriter, envelope, namespaces);
                    }
                }

                // Convertir el MemoryStream a string
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        private async Task EnviarSolicitud(string datosEnvio, string url, string serieCertificado)
        {
            //Metodo para hacer el envio de forma asincrona
            (X509Certificate2 certificado, bool resultado) = Program.gestionCertificados.exportaCertificadoDigital(serieCertificado);

            if (certificado != null)
            {
                //Protocolo de seguridad
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //Crear datos para la solicitud HTTP
                HttpWebRequest solicitudHttp = (HttpWebRequest)WebRequest.Create(url);

                //Configurar la solicitud
                solicitudHttp.Method = "POST";

                //Configura el contenido a xml
                solicitudHttp.ContentType = $"text/xml;charset=UTF-8";

                //Añade el certificado
                solicitudHttp.ClientCertificates.Add(certificado);

                //Grabacion de los datos a enviar al servidor
                byte[] datosEnvioBytes = Encoding.UTF8.GetBytes(datosEnvio);
                solicitudHttp.ContentLength = datosEnvioBytes.Length;

                using (Stream requestStream = await solicitudHttp.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(datosEnvioBytes, 0, datosEnvioBytes.Length);

                }

                using (HttpWebResponse respuesta = (HttpWebResponse)await solicitudHttp.GetResponseAsync())
                {
                    //Devuelve el estado 'OK' si todo ha ido bien
                    estadoRespuestaAEAT = respuesta.StatusDescription;

                    //Grabar el contenido de la respuesta para devolverlo al metodo de llamada
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await respuesta.GetResponseStream().CopyToAsync(ms);
                        contenidoRespuesta = Encoding.UTF8.GetString(ms.ToArray()); //Se pasa a una variable temporal para poder pasar el quita raros.
                    }

                    respuestaEnvioAEAT = Utiles.quitaRaros(contenidoRespuesta); //Solo se quitan los caracteres raros en el string, ya que en byte no procede
                }

            }
            else
            {
                //Si no se ha cargado el certificado, se devuelve una respuesta vacia.
                respuestaEnvioAEAT = string.Empty;
                estadoRespuestaAEAT = "KO";
            }
        }

        private EnvelopeResponse DeserializarRespuesta(string xmlRespuesta)
        {
            //Metodo para deserializar la respuesta recibida con la clase de la respuesta
            var serializer = new XmlSerializer(typeof(EnvelopeResponse));
            using (var stringReader = new StringReader(xmlRespuesta))
            {
                return (EnvelopeResponse)serializer.Deserialize(stringReader);
            }
        }
    }
}
