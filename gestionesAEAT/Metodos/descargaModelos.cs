using gestionesAEAT.Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace gestionesAEAT.Metodos
{
    //Clase para serializar los datos del XML recibido con la respuesta a una consulta de modelos presentados y poder tratarlos para generar el fichero de salida.

    // Define la clase para representar la respuesta XML
    [XmlRoot("servicioConsultasDirectas")]
    public class RespuestaWebService
    {
        [XmlElement("respuestaCorrecta")]
        public List<RespuestaCorrectaDescarga> Respuestas { get; set; }

        [XmlElement("error")]
        public List<RespuestaErrorDescarga> Errores { get; set; }
    }

    public class RespuestaCorrectaDescarga
    {
        //Clase con las propiedades que devuelve Hacienda si la respuesta es correcta
        public string ejercicio { get; set; }
        public string modelo { get; set; }
        public string periodo { get; set; }
        public string nif { get; set; }
        public string csv { get; set; }
        public string expediente { get; set; }
        public string justificante { get; set; }
        public DateTime fechaYHoraPresentacion { get; set; }
        public string nombreFicheroPDF { get; set; } //Al descargar los modelos, el mombre de cada PDF se forma con el NIF + modelo + ejercicio + periodo + justificante
    }

    public class RespuestaErrorDescarga
    {
        //Clase con las propiedades que devuelve Hacienda si hay errores en la respuesta
        public string descripcionError { get; set; }
    }

    public class descargaModelos
    {
        public string estadoRespuestaAEAT { get; set; }
        public string respuestaEnvioAEAT { get; set; }
        public List<RespuestaCorrectaDescarga> respuestasCorrectas = new List<RespuestaCorrectaDescarga>();
        public List<RespuestaErrorDescarga> respuestasError = new List<RespuestaErrorDescarga>();
        envioAeat envio = new envioAeat();
        Utiles utilidad = Program.utilidad;


        public void obtenerModelos()
        {
            string ficheroEntrada = Parametros.ficheroEntrada;
            string ficheroSalida = Parametros.ficheroSalida;
            string serieCertificado = Parametros.serieCertificado;
            string ficheroResultado = Parametros.ficheroResultado;
            //El parametro 'guion' es el texto que viene en el fichero que luego se formatea para poder hacer el envio al metodo 'envioAEAT.envioPOST'
            //El parametro 'serieCertificado' es necesario para luego pasarlo al metodo de envio

            string respuestaXML = string.Empty;
            string datosEnvio = string.Empty;


            //Prepara los datos del guion
            utilidad.cargaDatosGuion(ficheroEntrada); //Monta en la clase Utiles las listas "cabecera", "body" y "respuesta" para luego acceder a esos datos a montar el envio

            datosEnvio = utilidad.procesarGuionHtml(); //Formatea el guion para poder pasarlo al servidor

            //Realiza el envio a la AEAT
            envio.envioPost(utilidad.url, datosEnvio, serieCertificado, "form");

            //Procesa la respuesta
            if (envio.estadoRespuestaAEAT == "OK") //Si no ha habido error en la comunicacion
            {
                File.WriteAllText(ficheroResultado, "OK");
                if (envio.respuestaEnvioAEAT.Contains("<!DOCTYPE html>"))
                {
                    //Puede llegar un html con algun tipo de error
                    string path = Path.ChangeExtension(ficheroResultado, "html");
                    File.WriteAllText(path, envio.respuestaEnvioAEAT);
                }
                else
                {
                    respuestaXML = formateaRespuesta(envio.respuestaEnvioAEAT); //Se recibe un XML con la relacion de modelos presentados
                    File.WriteAllText(ficheroSalida, respuestaXML);
                }
            }
            else
            {
                File.WriteAllText(ficheroSalida, envio.respuestaEnvioAEAT);
            }
            if (respuestasCorrectas.Count > 0)
            {
                descargaPDF(Parametros.pathFicheros);
            }
        }

        private void descargaPDF(string pathSalida)
        {
            //Metodo para descargar el PDF de los modelos presentados a traves del CSV
            string url = @"https://www2.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.eeca.catalogo.vis.VisualizaSc";

            //Si el envio se hace al servidor de pruebas, la descarga hay que hacerla a otra url
            if (utilidad.url.Contains("prewww1"))
            {
                url = @"https://prewww2.aeat.es/wlpl/inwinvoc/es.aeat.dit.adu.eeca.catalogo.vis.VisualizaSc";
            }

            string datosEnvio = string.Empty;
            StringBuilder erroresCSV = new StringBuilder();

            foreach (var respuesta in respuestasCorrectas)
            {
                datosEnvio = $"COMPLETA=SI&ORIGEN=E&NIF={respuesta.nif}&CSV={respuesta.csv}";
                envio.envioPost(url, datosEnvio, "form");//Metodo sin certificado
                if (envio.estadoRespuestaAEAT == "OK")
                {
                    string ficheroSalida = Path.Combine(pathSalida, respuesta.nombreFicheroPDF);
                    if (envio.respuestaEnvioAEAT.Contains("<!DOCTYPE html>"))
                    {
                        //Si se devuelve un html es porque hay errores en la descarga y se almacena en el fichero de salida
                        ficheroSalida = Path.ChangeExtension(respuesta.nombreFicheroPDF, "html");
                        erroresCSV.AppendLine($"Error al descargar el modelo con CSV {respuesta.csv}");
                    }
                    File.WriteAllBytes(ficheroSalida, envio.respuestaEnvioAEATBytes);
                }
            }

            //Si hay errores se graba en el fichero de resultado
            if (erroresCSV.Length > 0) File.WriteAllText(Parametros.ficheroResultado, erroresCSV.ToString());
        }

        public string formateaRespuesta(string datos)
        {
            // Deserializar el XML en objetos
            RespuestaWebService servicio = DeserializeFromXml<RespuestaWebService>(datos);
            StringBuilder textoSalida = new StringBuilder();
            int elemento = 1;

            if (servicio.Errores.Count > 0)
            {
                respuestasError.AddRange(servicio.Errores); //Graba en la lista los errores que se hayan descargado
                foreach (var respuesta in servicio.Errores)
                {
                    textoSalida.AppendLine($"E{elemento.ToString("D2")} = {respuesta.descripcionError}");
                    elemento++;
                }
            }
            else
            {
                respuestasCorrectas.AddRange(servicio.Respuestas);

                // Procesa cada respuesta para grabar el fichero PDF obtenido
                foreach (var respuesta in servicio.Respuestas)
                {
                    //Forma el nombre del fichero PDF que se grabara para ponerlo en el fichero de respuestas.
                    respuesta.nombreFicheroPDF = $"{respuesta.nif}_{respuesta.modelo}_{respuesta.ejercicio}_{respuesta.periodo}_{respuesta.justificante}.pdf";
                    textoSalida.AppendLine($"NIF = {respuesta.nif}");
                    textoSalida.AppendLine($"Modelo = {respuesta.modelo}");
                    textoSalida.AppendLine($"Ejercicio = {respuesta.ejercicio}");
                    textoSalida.AppendLine($"Periodo = {respuesta.periodo}");
                    textoSalida.AppendLine($"CSV = {respuesta.csv}");
                    textoSalida.AppendLine($"Justificante = {respuesta.justificante}");
                    textoSalida.AppendLine($"Expediente = {respuesta.expediente}");
                    textoSalida.AppendLine($"Fecha presentacion = {respuesta.fechaYHoraPresentacion}");
                    textoSalida.AppendLine($"Fichero PDF = {respuesta.nombreFicheroPDF}");
                    //textoSalida.AppendLine();
                    elemento++;
                }
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
