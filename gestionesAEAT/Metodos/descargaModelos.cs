using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace gestionesAEAT.Metodos
{
    //Esta clase sirve para serializar los datos del XML recidibo con la respuesta a una consulta de modelos presentados y poder tratarlos para generar el fichero de salida.

    // Define la clase para representar la respuesta XML
    [XmlRoot("servicioConsultasDirectas")]
    public class RespuestaWebService
    {
        [XmlElement("respuestaCorrecta")]
        public List<RespuestaCorrecta> Respuestas { get; set; }
    }

    public class RespuestaCorrecta
    {
        public string ejercicio { get; set; }
        public string modelo { get; set; }
        public string periodo { get; set; }
        public string nif { get; set; }
        public string csv { get; set; }
        public string expediente { get; set; }
        public string justificante { get; set; }
        public DateTime fechaYHoraPresentacion { get; set; }
    }

    public class descargaModelos
    {
        public string estadoRespuestaAEAT { get; set; }
        public string respuestaEnvioAEAT { get; set; }
        public List<RespuestaCorrecta> respuestasCorrectas = new List<RespuestaCorrecta>();
        envioAeat envio = new envioAeat();
        Utiles utilidad = new Utiles();


        public void obtenerModelos(string guion, string ficheroSalida, string serieCertificado, gestionCertificados instanciaCertificado)
        {
            //El parametro 'guion' sera el texto que viene en el fichero, por lo que hay que montar un metodo que lo transforme a los datos que espera Hacienda, similar a lo que se hace con el metodo ratificarDomicilio.cargaDatos que procesa el [url], [cabecera] y [respuesta], y una vez montado, utilizar el metodo envioAEAT.envioPOST. Ver si se puede generar un metodo dentro de 'Utilidades' para formatear los datos del guion ya que se usara en mas sitios.
            //El parametro 'serieCertificado' es necesario para luego pasarlo al metodo de envio


            string respuestaXML = string.Empty;
            string datosEnvio = string.Empty;

            //Falta implementar un metodo que formatee el guion.txt que tendra los datos a enviar, para que se adapte al formato que pide Hacienda, y luego hacer el envio por POST como formulario, almacenando la respuesta en una variable que luego se pasa al metodo que la formatea para poder grabarla despues en el main.

            
            datosEnvio = utilidad.procesarGuionHtml(guion);
            envio.envioPost(utilidad.url, datosEnvio, serieCertificado, instanciaCertificado);

            if (envio.estadoRespuestaAEAT == "OK")
            {
                respuestaXML = formateaRespuesta(envio.respuestaEnvioAEAT);
                File.WriteAllText(ficheroSalida, respuestaXML);
            }
            else
            {
                File.WriteAllText(ficheroSalida, envio.respuestaEnvioAEAT);
            }

            descargaPDF();
        }

        private void descargaPDF()
        {
            //envioAeat envio = new envioAeat();
            string url = @"https://www2.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.eeca.catalogo.vis.VisualizaSc";
            string datosEnvio = string.Empty;
            foreach(var elemento in respuestasCorrectas)
            {
                datosEnvio = $"COMPLETA=SI&ORIGEN=E&NIF={elemento.nif}&CSV={elemento.csv}";
                envio.envioPostSinCertificado(url, datosEnvio);
                if (envio.estadoRespuestaAEAT == "OK")
                {
                    byte[] respuestaPDF = Encoding.UTF8.GetBytes(envio.respuestaEnvioAEAT);
                    File.WriteAllBytes("fichero.pdf", respuestaPDF);
                }

            }
        }

        public string formateaRespuesta(string datos)
        {

            // Deserializar el XML en objetos
            RespuestaWebService servicio = DeserializeFromXml<RespuestaWebService>(datos);
            //RespuestaWebService servicio2 = DeserializeFromXml<RespuestaWebService>(datos);
            respuestasCorrectas.AddRange(servicio.Respuestas);

            // Acceder a los valores de las respuestas correctas
            StringBuilder textoSalida = new StringBuilder();
            int elemento = 1;
            //foreach (var respuesta in servicio.RespuestasCorrectas)
            foreach (var respuesta in servicio.Respuestas)

            {
                textoSalida.AppendLine($"Resultado {elemento}");
                textoSalida.AppendLine($"NIF: {respuesta.nif}");
                textoSalida.AppendLine($"Modelo: {respuesta.modelo}");
                textoSalida.AppendLine($"Ejercicio: {respuesta.ejercicio}");
                textoSalida.AppendLine($"Periodo: {respuesta.periodo}");
                textoSalida.AppendLine($"CSV: {respuesta.csv}");
                textoSalida.AppendLine($"Justificante: {respuesta.justificante}");
                textoSalida.AppendLine($"Expediente: {respuesta.expediente}");
                textoSalida.AppendLine($"Fecha presentacion: {respuesta.fechaYHoraPresentacion}");
                elemento++;
            }

            return textoSalida.ToString();
        }

        public T DeserializeFromXml<T>(string datos)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(datos))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}
