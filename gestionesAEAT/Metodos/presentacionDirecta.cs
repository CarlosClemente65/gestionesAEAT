using gestionesAEAT.Utilidades;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace gestionesAEAT.Metodos
{
    public class PresBasicaDos
    {
        public string MODELO { get; set; }
        public string EJERCICIO { get; set; }
        public string PERIODO { get; set; }
        public string NRC { get; set; }
        public string IDI { get; set; }
        public string F01 { get; set; }
        public string FIR { get; set; }
        public string FIRNIF { get; set; }
        public string FIRNOMBRE { get; set; }
    }

    public class RespuestaPresBasicaDos
    {
        public ElementosRespuestaPresBasicaDos respuesta { get; set; }
    }

    public class ElementosRespuestaPresBasicaDos
    {
        public RespuestaCorrectaPresBasicaDos correcta { get; set; } = new RespuestaCorrectaPresBasicaDos();
        public List<string> errores { get; set; } = new List<string>();
    }

    public class RespuestaCorrectaPresBasicaDos
    {
        public string FormaPago { get; set; }
        public string CodigoSeguroVerificacion { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string Expediente { get; set; }
        public string NIFPresentador { get; set; }
        public string ApellidosNombrePresentador { get; set; }
        public string TipoRepresentacion { get; set; }
        public string NIFDeclarante { get; set; }
        public string ApellidosNombreDeclarante { get; set; }
        public string Modelo { get; set; }
        public string Ejercicio { get; set; }
        public string Periodo { get; set; }
        public string Justificante { get; set; }
        public string NRCPago { get; set; }
        public string ImporteAIngresar { get; set; }
        public string Idioma { get; set; }
        public string urlPdf { get; set; }
        public List<string> avisos { get; set; } = new List<string>();
        public List<string> advertencias { get; set; } = new List<string>();
    }

    public class presentacionDirecta
    {
        string atributo = string.Empty; //Cada una de las variables que se pasan a la AEAT
        string valor = string.Empty; //Valor del atributo que se pasa a la AEAT

        string respuestaAEAT; //Contenido de la respuesta de la AEAT a la solicitud enviada

        List<string> textoEnvio = new List<string>(); //Prepara una lista con los datos del guion

        string textoSalida = string.Empty; //Texto que se grabara en el fichero de salida

        Utiles utilidad = Program.utilidad; //Instanciacion de las utilidades para poder usarlas


        public void envioPeticion()
        {
            string ficheroEntrada = Parametros.ficheroEntrada;
            string ficheroSalida = Parametros.ficheroSalida;
            string serieCertificado = Parametros.serieCertificado;
            string ficheroResultado = Parametros.ficheroResultado;

            envioAeat envio = new envioAeat();

            try
            {
                //Prepara el guion
                utilidad.cargaDatosGuion(ficheroEntrada); //Monta en la clase Utiles las listas "cabecera", "body" y "respuesta" para luego acceder a esos datos a montar el envio

                //Instanciacion de la clase para almacenar los valores de la cabecera
                PresBasicaDos contenidoEnvio = new PresBasicaDos();

                //Formatear datos de la cabecera
                foreach (var elemento in utilidad.cabecera)
                {
                    (atributo, valor) = utilidad.divideCadena(elemento, '=');

                    // Verificar si el nombre coincide con alguna propiedad de la clase servaliDos y asignar el valor correspondiente
                    switch (atributo)
                    {
                        case "MODELO":
                            contenidoEnvio.MODELO = valor;
                            break;

                        case "EJERCICIO":
                            contenidoEnvio.EJERCICIO = valor;
                            break;

                        case "PERIODO":
                            contenidoEnvio.PERIODO = valor;
                            break;

                        case "NRC":
                            contenidoEnvio.NRC = valor;
                            break;

                        case "IDI":
                            contenidoEnvio.IDI = valor;
                            break;

                        case "F01":
                            contenidoEnvio.F01 = valor;
                            break;

                        case "FIR":
                            contenidoEnvio.FIR = valor;
                            break;

                        case "FIRNIF":
                            contenidoEnvio.FIRNIF = valor;
                            break;

                        case "FIRNOMBRE":
                            contenidoEnvio.FIRNOMBRE = valor;
                            break;

                        default:
                            break;
                    }
                }


                //Prepara y envia los datos a la AEAT
                string jsonEnvio = JsonConvert.SerializeObject(contenidoEnvio, new JsonSerializerSettings
                {
                    //Serializa el json ignorando valores nulos y formateando la respuesta de forma indentada
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                envio.envioPost(utilidad.url, jsonEnvio, serieCertificado, "json");
                respuestaAEAT = envio.respuestaEnvioAEAT;


                //Procesa la respuesta
                if (envio.estadoRespuestaAEAT == "OK")
                {
                    //Si se ha podido enviar, se serializa la respuesta de Hacienda
                    utilidad.respuestaEnvioModelos = JsonConvert.DeserializeObject<RespuestaPresBasicaDos>(respuestaAEAT);
                    textoSalida = utilidad.generarRespuesta(ficheroSalida, "enviar");

                    //Procesado de los tipos de respuesta posibles
                    var respuestaEnvio = utilidad.respuestaEnvioModelos.respuesta;
                    if (respuestaEnvio.correcta != null && !string.IsNullOrEmpty(respuestaEnvio.correcta.CodigoSeguroVerificacion))
                    {
                        //Si hay datos en las propiedades de la respuesta correcta se graba el PDF y el fichero con los datos del modelo
                        var elementosRespuesta = respuestaEnvio.correcta;
                        if (elementosRespuesta.urlPdf != null)
                        {
                            //Grabar el PDF en la ruta
                            string ficheroPdf = Path.ChangeExtension(ficheroSalida, "pdf");

                            using (WebClient clienteWeb = new WebClient())
                            {
                                byte[] contenidoPdf = clienteWeb.DownloadData(elementosRespuesta.urlPdf);
                                File.WriteAllBytes(ficheroPdf, contenidoPdf);
                            }
                        }

                        //Grabacion de los datos de la respuesta en el fichero de salida
                        using (StreamWriter writer = new StreamWriter(ficheroSalida))
                        {
                            var propiedadesSeleccionadas = new List<string> //Permite buscar solo las propiedades que necesitamos grabar en el fichero
                        {
                            "Modelo", "Ejercicio", "Periodo","CodigoSeguroVerificacion", "Justificante", "Expediente"
                        };

                            Type tipo = elementosRespuesta.GetType();
                            var propiedades = tipo.GetProperties();

                            foreach (var propiedad in propiedades)
                            {
                                if (propiedadesSeleccionadas.Contains(propiedad.Name))
                                {
                                    var valor = propiedad.GetValue(elementosRespuesta);
                                    writer.WriteLine($"{propiedad.Name} = {valor}");

                                }
                            }
                            if (elementosRespuesta.avisos != null) //Si hay avisos tambien se graban en el fichero de salida
                            {
                                for (int i = 0; i < elementosRespuesta.avisos.Count; i++)
                                {
                                    writer.WriteLine($"A{i + 1.ToString("D2")} = {elementosRespuesta.avisos[i]}");
                                }
                            }

                            if (elementosRespuesta.advertencias != null) //Si hay advertencias tambien se graban en el fichero de salida
                            {
                                for (int i = 0; i < elementosRespuesta.advertencias.Count; i++)
                                {
                                    writer.WriteLine($"D{i + 1.ToString("D2")} = {elementosRespuesta.advertencias[i]}");
                                }
                            }
                        }
                    }

                    else
                    {
                        //Procesa la respuesta de la validacion para generar el fichero de salida
                        var respuestaEnvioModelos = utilidad.respuestaEnvioModelos;
                        string resultadoSalida = utilidad.generaFicheroSalida(respuestaEnvioModelos);

                        //Graba el fichero de salida
                        Encoding codificacionCP437 = Encoding.GetEncoding("IBM437");
                        File.WriteAllText(ficheroSalida, resultadoSalida, codificacionCP437);
                    }


                    //Grabar un html con los errores, avisos o advertencias generados
                    if (!string.IsNullOrEmpty(textoSalida))
                    {
                        string rutaHtml = Path.ChangeExtension(ficheroSalida, "html");
                        File.WriteAllText(rutaHtml, textoSalida);
                    }

                    //Grabar el fichero de respuesta
                    File.WriteAllText(ficheroResultado, "OK");

                }
            }

            catch (Exception ex)
            {
                //Si se ha producido algun error en el envio
                string mensaje = $"MENSAJE = Proceso cancelado o error en el envio. {ex.Message}";
                utilidad.GrabarSalida(mensaje, Parametros.ficheroResultado);
                utilidad.grabadaSalida = true;
            }
        }
    }
}
