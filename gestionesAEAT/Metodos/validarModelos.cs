using gestionesAEAT.Utilidades;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace gestionesAEAT.Metodos
{
    public class servaliDos
    {
        [JsonProperty("MODELO")]
        public string Modelo { get; set; }

        [JsonProperty("EJERCICIO")]
        public string Ejercicio { get; set; }

        [JsonProperty("PERIODO")]
        public string Periodo { get; set; }

        [JsonProperty("F01")]
        public string F01 { get; set; }

        [JsonProperty("IDI")]
        public string Idioma { get; set; }

        [JsonProperty("SINVIL")]
        public string SinValidar { get; set; }
    }

    public class RespuestaValidarModelos
    {
        [JsonProperty("respuesta")]
        public elementosRespuesta respuesta { get; set; }
    }
    public class elementosRespuesta
    {
        [JsonProperty("errores")]
        public List<string> errores { get; set; }

        [JsonProperty("pdf")]
        public List<string> pdf { get; set; }

        [JsonProperty("avisos")]
        public List<string> avisos { get; set; }

        [JsonProperty("advertencias")]
        public List<string> advertencias { get; set; }
    }

    public class validarModelos
    {
        string atributo = string.Empty; //Cada una de las variables que se pasan a la AEAT
        string valor = string.Empty; //Valor del atributo que se pasa a la AEAT

        string respuestaAEAT; //Contenido de la respuesta de la AEAT a la solicitud enviada

        List<string> textoEnvio = new List<string>(); //Prepara una lista con los datos del guion

        string textoSalida = string.Empty; //Texto que se grabara en el fichero de salida

        Utiles utilidad = Program.utilidad; //Instanciacion de las utilidades para poder usarlas
        envioAeat envio = new envioAeat();

        public void envioPeticion(string ficheroEntrada, string ficheroSalida)
        {
            textoEnvio = utilidad.prepararGuion(ficheroEntrada); //Se procesa el guion para formar una lista que se pueda pasar al resto de metodos

            try
            {
                utilidad.cargaDatosGuion(textoEnvio); //Monta en la clase Utiles las listas "cabecera", "body" y "respuesta" para luego acceder a esos datos a montar el envio

                //Instanciacion de la clase para almacenar los valores de la cabecera
                servaliDos dato = new servaliDos();

                //Formatear datos de la cabecera
                foreach (var elemento in utilidad.cabecera)
                {
                    string[] partes = elemento.Split('=');
                    if (partes.Length == 2)
                    {
                        atributo = partes[0].Trim();
                        valor = partes[1].Trim();
                    }

                    // Verificar si el nombre coincide con alguna propiedad de la clase servaliDos y asignar el valor correspondiente
                    switch (atributo)
                    {
                        case "MODELO":
                            dato.Modelo = valor;
                            break;

                        case "EJERCICIO":
                            dato.Ejercicio = valor;
                            break;

                        case "PERIODO":
                            dato.Periodo = valor;
                            break;

                        case "F01":
                            dato.F01 = valor;
                            break;

                        case "IDI":
                            dato.Idioma = valor;
                            break;

                        case "SINVL":
                            dato.SinValidar = valor;
                            break;

                        default:
                            break;
                    }
                }

                //Serializa el Json para hacer el envio
                string jsonEnvio = JsonConvert.SerializeObject(dato, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented

                });

                envio.envioPost(utilidad.url, jsonEnvio, "json");//Metodo sin certificado
                respuestaAEAT = envio.respuestaEnvioAEAT;

                if (envio.estadoRespuestaAEAT == "OK")
                {
                    //Deserializa la respuesta de Hacienda con la clase RespuestaValidarModelos
                    utilidad.respuestaValidarModelos = JsonConvert.DeserializeObject<RespuestaValidarModelos>(respuestaAEAT);
                    textoSalida = utilidad.generarRespuesta(ficheroSalida, "validar");

                    //Grabar un fichero con los errores, avisos o advertencias que se han podido producir
                    if (!string.IsNullOrEmpty(textoSalida))
                    {
                        string rutaHtml = Path.ChangeExtension(ficheroSalida, "html");
                        File.WriteAllText(rutaHtml, textoSalida);
                    }

                    //Si hay una respuesta en pdf se graba en la ruta de salida
                    if (utilidad.respuestaValidarModelos.respuesta.pdf != null)
                    {
                        string ficheroPdf = Path.ChangeExtension(ficheroSalida, "pdf");
                        string respuestaPDF = utilidad.respuestaValidarModelos.respuesta.pdf[0];
                        byte[] contenidoPDF = Convert.FromBase64String(respuestaPDF);
                        File.WriteAllBytes(ficheroPdf, contenidoPDF);
                    }
                }
            }

            catch (Exception ex)
            {
                //Si se ha producido algun error en el envio
                string mensaje = $"MENSAJE = Proceso cancelado o error en el envio. {ex.Message}";
                utilidad.GrabarSalida(mensaje, Program.ficheroResultado);
                utilidad.grabadaSalida = true;
            }
        }
    }
}