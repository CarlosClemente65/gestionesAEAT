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
        public string Ejercicio { get; set; }
        public string Modelo { get; set; }
        public string Periodo { get; set; }
        public string NIF { get; set; }
        public string CSV { get; set; }
        public string Expediente { get; set; }
        public string Justificante { get; set; }
        public DateTime FechaYHoraPresentacion { get; set; }
    }

    public class descargaModelos
    {
        public void obtenerModelos(string guion, string ficheroSalida, string serieCertificado, gestionCertificados instanciaCertificado)
        {
            //El parametro 'guion' sera el texto que viene en el fichero, por lo que hay que montar un metodo que lo transforme a los datos que espera Hacienda, similar a lo que se hace con el metodo ratificarDomicilio.cargaDatos que procesa el [url], [cabecera] y [respuesta], y una vez montado, utilizar el metodo envioAEAT.envioPOST. Ver si se puede generar un metodo dentro de 'Utilidades' para formatear los datos del guion ya que se usara en mas sitios.
            //El parametro 'serieCertificado' es necesario para luego pasarlo al metodo de envio

            
            
            string respuestaAEAT = string.Empty;
            string datosEnvio = string.Empty;
            string url = string.Empty; //Esta variable hay que sacarla de la cabecera del guion pero para que no de error el metodo envioAEAT.envioPOST la inicializo vacia
            // Ejemplo de XML recibido de la API. Habra que sustituirlo por la respuesta real.
            string xmlString = @"<servicioConsultasDirectas>
                                <respuestaCorrecta>
                                    <ejercicio>2023</ejercicio>
		                            <modelo>303</modelo>
                            		<periodo>1T</periodo>
                            		<nif>21523660F</nif>
                            		<csv>HQHH4VAUADX5USW5</csv>
		                            <expediente>202330323660010D </expediente>
		                            <justificante>3034297995951</justificante>
		                            <fechaYHoraPresentacion>2023-04-02T21:34:33</fechaYHoraPresentacion>
                                </respuestaCorrecta>
                                <respuestaCorrecta>
                                    <ejercicio>2023</ejercicio>
		                            <modelo>303</modelo>
		                            <periodo>2T</periodo>
		                            <nif>21523660F</nif>
		                            <csv>BEJG38NFWTDCJ9YD</csv>
		                            <expediente>202330323660380P </expediente>
		                            <justificante>3034288442455</justificante>
		                            <fechaYHoraPresentacion>2023-07-02T23:27:18</fechaYHoraPresentacion>
                                </respuestaCorrecta>
                            </servicioConsultasDirectas>";
            
            //Falta implementar un metodo que formatee el guion.txt que tendra los datos a enviar, para que se adapte al formato que pide Hacienda, y luego hacer el envio por POST como formulario, almacenando la respuesta en una variable que luego se pasa al metodo que la formatea para poder grabarla despues en el main.

            Utiles utilidad = new Utiles();
            datosEnvio = utilidad.procesarGuionHtml(guion);

            envioAeat proceso = new envioAeat();
            proceso.envioPost(url, datosEnvio, serieCertificado, instanciaCertificado);
            
            respuestaAEAT =  formateaRespuesta(xmlString);
            File.WriteAllText(ficheroSalida, respuestaAEAT);
        }

        public string formateaRespuesta(string datos)
        {

            // Deserializar el XML en objetos
            ServicioConsultasDirectas servicio = DeserializeFromXml<ServicioConsultasDirectas>(datos);

            // Acceder a los valores de las respuestas correctas
            string textoSalida = string.Empty;
            int elemento = 1;
            foreach (var respuesta in servicio.RespuestasCorrectas)
            {
                textoSalida += $"Resultado {elemento}\n";
                textoSalida += $"NIF: {respuesta.nif}\n";
                textoSalida += $"Modelo: {respuesta.modelo}\n";
                textoSalida += $"Ejercicio: {respuesta.ejercicio}\n";
                textoSalida += $"Periodo: {respuesta.periodo}\n";
                textoSalida += $"CSV: {respuesta.Csv}\n";
                textoSalida += $"Justificante: {respuesta.Justificante}\n";
                textoSalida += $"Expediente: {respuesta.Expediente}\n";
                textoSalida += $"Fecha presentacion: {respuesta.fechaYHoraPresentacion}\n\n";
                elemento++;
            }

            return textoSalida;
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
