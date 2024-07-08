using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace gestionesAEAT.Metodos
{
    public class servaliDos
    {
        public string MODELO { get; set; }
        public string EJERCICIO { get; set; }
        public string PERIODO { get; set; }
        public string F01 { get; set; }
        public string IDI { get; set; }
        public string SINVL { get; set; }
    }

    public class RespuestaValidarModelos
    {
        public elementosRespuesta respuesta { get; set; }
    }
    public class elementosRespuesta
    {
        public List<string> errores { get; set; }

        public List<string> pdf { get; set; }

        public List<string> avisos { get; set; }

        public List<string> advertencias { get; set; }
    }

    public class validarModelos
    {
        string url; //Url a la que se envian los datos

        string atributo = string.Empty; //Cada una de las variables que se pasan a la AEAT
        string valor = string.Empty; //Valor del atributo que se pasa a la AEAT

        string respuestaAEAT; //Contenido de la respuesta de la AEAT a la solicitud enviada
        string estadoRespuesta; //Devuelve OK si la respuesta es correcta

        string datosEnvio; //Datos a enviar a la AEAT ya formateados

        List<string> textoEnvio = new List<string>(); //Prepara una lista con los datos del guion

        string textoSalida = string.Empty; //Texto que se grabara en el fichero de salida
        string aux; //Variable auxiliar para la grabacion de la respuesta

        Utiles utilidad = new Utiles(); //Instanciacion de las utilidades para poder usarlas
        envioAeat envio = new envioAeat();

        public void envioPeticion(string ficheroEntrada, string ficheroSalida)
        {
            textoEnvio = utilidad.prepararGuion(ficheroEntrada); //Se procesa el guion para formar una lista que se pueda pasar al resto de metodos
            utilidad.borrarFicheros(ficheroSalida); //Se borra el fichero de salida si existe, para que no haya problemas

            try
            {
                utilidad.cargaDatosGuion(textoEnvio); //Monta en la clase Utiles las listas "cabecera", "body" y "respuesta" para luego acceder a esos datos a montar el envio

                //Instanciacion de la clase para almacenar los valores de la cabecera
                servaliDos dato = new servaliDos();

                //Formatear datos de la cabecera
                foreach (var elemento in utilidad.cabecera)
                {
                    int indice = elemento.IndexOf("=");
                    if (indice != -1)
                    {
                        atributo = elemento.Substring(0, indice).Trim();
                        valor = elemento.Substring(indice + 1).Trim();
                    }

                    // Verificar si el nombre coincide con alguna propiedad de la clase servaliDos y asignar el valor correspondiente
                    switch (atributo)
                    {
                        case "MODELO":
                            dato.MODELO = valor;
                            break;
                        case "EJERCICIO":
                            dato.EJERCICIO = valor;
                            break;
                        case "PERIODO":
                            dato.PERIODO = valor;
                            break;
                        case "F01":
                            dato.F01 = valor;
                            break;
                        case "IDI":
                            dato.IDI = valor;
                            break;
                        case "SINVL":
                            dato.SINVL = valor;
                            break;

                        default:
                            break;
                    }
                }

                string jsonEnvio = JsonConvert.SerializeObject(dato, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented

                });

                envio.envioPost(utilidad.url, jsonEnvio, "json");//Metodo sin certificado
                respuestaAEAT = envio.respuestaEnvioAEAT;

                if (envio.estadoRespuestaAEAT == "OK")
                {
                    utilidad.respuestaValidarModelos = JsonConvert.DeserializeObject<RespuestaValidarModelos>(respuestaAEAT);
                    textoSalida = utilidad.generarRespuesta(ficheroSalida, "validar");

                    //Grabar un fichero con los errores
                    if (!string.IsNullOrEmpty(textoSalida))
                    {
                        string rutaHtml = Path.ChangeExtension(ficheroSalida, "html");
                        File.WriteAllText(rutaHtml, textoSalida);
                    }

                    if (utilidad.respuestaValidarModelos.respuesta.pdf != null)
                    {
                        //Grabar el PDF en la ruta
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
                aux = $"MENSAJE = Proceso cancelado o error en el envio. {ex.Message}";
            }
        }


        private void cargaCabecera(object cadena)
        {
            string[] parte;
            try
            {
                parte = cadena.ToString().Split('=');
                atributo = parte[0].ToString().Trim();
                valor = parte[1].ToString().Trim();
            }

            catch (Exception ex)
            {
                //Falta el control de la posible excepcion
            }
        }
    }
}