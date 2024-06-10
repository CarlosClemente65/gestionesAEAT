using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using gestionesAEAT.Metodos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gestionesAEAT
{
    public class Utiles
    {
        public string url { get; set; } //Variable que almacena la url a la que enviar los datos a la AEAT

        public List<string> cabecera = new List<string>(); //Lista con las lineas que vienen en el guion como cabecera
        public List<string> body = new List<string>(); //Bloque de datos identificados como body en la entrada
        public List<string> respuesta = new List<string>(); //Bloque de datos identificados como respuesta en la entrada

        //Variables para almacenar las respuestas del envio
        private List<string> erroresArray = new List<string>();
        private List<string> avisosArray = new List<string>();
        private List<string> advertenciasArray = new List<string>();

        private RespuestaJson respuestaJson; //Varible que almacena la respuesta completa de la AEAT

        public string quitaRaros(string cadena)
        {
            //Metodo para eliminar caracteres raros
            List<(string, string)> caracteresReemplazo = new List<(string, string)>
            {
                ("á", "a"),
                ("é", "e"),
                ("í", "i"),
                ("ó", "o"),
                ("ú", "u"),
                ("º", "."),
                ("ª", "."),
                ("ñ", "¤"),
                ("Á", "A"),
                ("É", "E"),
                ("Í", "I"),
                ("Ó", "O"),
                ("Ú", "U"),
                ("Ñ", "¤")
            };

            foreach (var tupla in caracteresReemplazo)
            {
                cadena = cadena.Replace(tupla.Item1, tupla.Item2);
            }
            return cadena;

        }

        public string codificacionFicheroEntrada(string guion)
        {
            //Permite obtener la codificacion UTF-8 o ISO8859-1 (ascii extendido 256 bits o ansi)
            List<string> textoGuion = new List<string>();
            using (StreamReader sr = new StreamReader(guion))
            {
                string linea;
                while ((linea = sr.ReadLine()) != null)
                {
                    textoGuion.Add(linea);
                }
            }

            string cadena, valor;
            int bloque = 0;

            valor = "";

            for (int x = 0; x < textoGuion.Count; x++)
            {
                cadena = textoGuion[x].ToString().Trim();
                if (cadena != "")
                {
                    switch (cadena)
                    {
                        case "[cabecera]":
                            bloque = 2;
                            continue;
                    }

                    if (bloque == 2)
                    {
                        string[] parte = cadena.ToString().Split('=');
                        if (parte[0] == "CODIFICACION")
                        {
                            if (parte[1].Length > 1) valor = parte[1];
                            break;
                        }
                    }
                }
            }

            if (valor == "") valor = "UTF-8";
            return valor.ToUpper();
        }

        public void borrarFicheros(string fichero)
        {
            if (File.Exists(fichero)) File.Delete(fichero);
        }

        public string procesarGuionHtml(string guion)
        {
            //Procesa el guion para poder hacer el envio a la AEAT
            List<string> textoEntrada = prepararGuion(guion);
            string textoAEAT = string.Empty;

            cargaDatosGuion(textoEntrada);

            for (int linea = 0; linea < cabecera.Count; linea++)
            {
                if (string.IsNullOrEmpty(textoAEAT))
                {
                    textoAEAT = cabecera[linea].ToString();
                }
                else
                {
                    textoAEAT += "&" + cabecera[linea].ToString();
                }
            }
            return textoAEAT;
        }

        public void cargaDatosGuion(List<string> textoEntrada)
        {
            //Lee el fichero de entrada y monta una lista con todas las lineas segun si son de la cabecera, body o respuesta
            string cadena;
            int bloque = 0; //Controla el tipo de dato a grabar en el fichero

            for (int x = 0; x < textoEntrada.Count; x++)
            {
                cadena = textoEntrada[x].ToString().Trim();
                if (cadena != "")
                {
                    //Control para saber que parte del fichero se va a procesar
                    switch (cadena)
                    {
                        case "[url]":
                            bloque = 1;
                            continue;

                        case "[cabecera]":
                            bloque = 2;
                            continue;

                        case "[body]":
                            bloque = 3;
                            continue;

                        case "[respuesta]":
                            bloque = 4;
                            continue;
                    }
                    switch (bloque)
                    {
                        //Las lineas que van despues de cada parte se asignan a cada una de ellas
                        case 1:
                            url = cadena;
                            break;

                        case 2:
                            cabecera.Add(cadena);
                            break;

                        case 3:
                            body.Add(cadena);
                            break;

                        case 4:
                            respuesta.Add(cadena);
                            break;
                    }
                }
            }
        }

        public List<string> prepararGuion(string ficheroEntrada)
        {
            //Recibe un string y devuelve una lista
            
            //Obtiene la codificacion del texto para procesarlo
            Encoding codificacion = Encoding.GetEncoding(codificacionFicheroEntrada(ficheroEntrada));

            //Monta una lista con el fichero de entrada para procesarlo
            List<string> textoEntrada = new List<string>();
            using (StreamReader sr = new StreamReader(ficheroEntrada, codificacion))
            {
                string linea;
                while ((linea = sr.ReadLine()) != null)
                {
                    textoEntrada.Add(linea);
                }
            }
            return textoEntrada;
        }

        public string generarRespuesta(RespuestaJson respuesta, string ficheroRespuesta, int control = 0)
        {
            this.respuestaJson = respuesta;
            string[] elementos = { "errores", "avisos", "advertencias" };
            string modelo = string.Empty;
            string ejercicio = string.Empty;
            string periodo = string.Empty;
            string cliente = string.Empty;
            string respuestaHtml = string.Empty;

            //Detecta si en la respuesta hay alguno de los elementos para ver si hay errores, avisos o advertencias
            if (respuestaJson != null && respuestaJson.respuesta != null)
            {
                foreach (string elemento in elementos)
                {
                    switch (elemento)
                    {
                        case "errores":
                            if (respuestaJson.respuesta.errores != null && respuestaJson.respuesta.errores.Count > 0)
                            {
                                control++;
                            }
                            break;

                        case "avisos":
                            if (respuestaJson.respuesta.avisos != null && respuestaJson.respuesta.avisos.Count > 0)
                            {
                                control++;
                            }
                            break;

                        case "advertencias":
                            if (respuestaJson.respuesta.advertencias != null && respuestaJson.respuesta.advertencias.Count > 0)
                            {
                                control++;
                            }
                            break;

                        default:
                            break;

                    }
                }
            }

            //Si se ha encontrado algun error, aviso o advertencia, hace el html
            if (control > 0)
            {
                int indice;
                int posicion;

                //Asigna las variables modelo, ejercicio y periodo segun los valores de la cabecera
                foreach (string linea in cabecera)
                {
                    indice = linea.IndexOf("=");
                    if (indice != -1)
                    {
                        if (linea.StartsWith("MODELO")) modelo = linea.Substring(indice + 1);
                        if (linea.StartsWith("EJERCICIO")) ejercicio = linea.Substring(indice + 1);
                        if (linea.StartsWith("PERIODO")) periodo = linea.Substring(indice + 1);
                    }
                }

                //Intenta asignar el numero de cliente tomandolo del nombre del fichero de respuesta
                try
                {
                    string codCliente = Path.GetFileNameWithoutExtension(ficheroRespuesta);
                    posicion = codCliente.IndexOf("_salida");
                    if (posicion != -1)
                    {
                        cliente = codCliente.Substring(0, posicion);

                    }
                }
                catch (Exception ex)
                {
                    //Si no se encuentra el cliente se devuelve vacio
                }

                respuestaHtml = generarHtml(modelo, ejercicio, periodo, cliente);
            }

            return respuestaHtml;
        }

        private string generarHtml(string modelo, string ejercicio, string periodo, string cliente)
        {
            StringBuilder respuestaHtml = new StringBuilder();

            //Almacena las respuestas en las listas para procesarlas despues
            if (respuestaJson != null && respuestaJson.respuesta != null)
            {
                erroresArray = respuestaJson.respuesta.errores;
                avisosArray = respuestaJson.respuesta.avisos;
                advertenciasArray = respuestaJson.respuesta.advertencias;
            }

            //Construye el html
            StringBuilder contenidoHtml = new StringBuilder();

            //Cabecera del html y datos informativos del cliente, modelo, ejercicio y periodo
            contenidoHtml.AppendLine("<!DOCTYPE html>");
            contenidoHtml.AppendLine("<html");
            contenidoHtml.AppendLine("<head>");
            contenidoHtml.AppendLine("<style>");
            contenidoHtml.AppendLine("th, td{border: 1px solid red;padding: 5px 5px 5px 15px;text-align: justify; font-size:1em}");
            contenidoHtml.AppendLine("td{font-size:0.9em;padding: 5px 20px 5px 40px}");
            contenidoHtml.AppendLine("</style>");
            contenidoHtml.AppendLine("</head>");
            contenidoHtml.AppendLine("<body  style='margin: 40px; font-family: Calibri; font-size: 1.2em;'>");
            contenidoHtml.AppendLine("<title>Resultado de la validaci&oacute;n</title>");
            contenidoHtml.AppendLine("<p style='font-family:Calibri; font-size: 1.5em; text-align:center'>Resultado de la validaci&oacute;n</p>");
            contenidoHtml.AppendLine($"<p style='font-family:Calibri; font-size: 0.9em; text-align: center'>Cliente: {cliente}&nbsp;&nbsp;&nbsp;--&nbsp;&nbsp;&nbsp;Modelo: {modelo}&nbsp;&nbsp;&nbsp;--&nbsp;&nbsp;&nbsp;Ejercicio: {ejercicio}&nbsp;&nbsp;&nbsp;--&nbsp;&nbsp;&nbsp;Periodo: {periodo}&nbsp;&nbsp;&nbsp;--&nbsp;&nbsp;&nbsp;Fecha generacion: {DateTime.Now}</p>");

            //Contenido del html si hay errores
            if (erroresArray != null && erroresArray.Count > 0)
            {
                //Generar tabla de errores
                contenidoHtml.AppendLine("<table style='margin: 10px; width: 100%; border-collapse: collapse; font-size: 1em;'>");
                contenidoHtml.AppendLine("<tr style='background-color: #FFBFBF'><th><span style='color: red;font-size: 1em;margin-right: 5px;'>&#128711;</span>Errores. No es posible presentar la declaracion</th></tr>");
                contenidoHtml.AppendLine(generarFilasHtml("errores"));
                contenidoHtml.AppendLine("</table>");
            }

            //Contenido del html si hay avisos
            if (avisosArray != null && avisosArray.Count > 0)
            {
                //Generar tabla de avisos
                contenidoHtml.AppendLine("<table style='margin: 10px; width: 100%; border-collapse: collapse; font-size: 1em;'>");
                contenidoHtml.AppendLine("<tr style='background-color: #F9E79F'><th><span style='color: red;font-size: 1em;margin-right: 5px;'>&#9888;</span>Avisos que deben revisarse. Permiten presentar la declaracion</th></tr>");
                contenidoHtml.AppendLine(generarFilasHtml("avisos"));
                contenidoHtml.AppendLine("</table>");
            }

            //Contenido del html si hay advertencias
            if (advertenciasArray != null && advertenciasArray.Count > 0)
            {
                //Generar tabla de avisos
                contenidoHtml.AppendLine("<table style='margin: 10px; width: 100%; border-collapse: collapse; font-size: 1em;'>");
                contenidoHtml.AppendLine("<tr style='background-color: #AED6F1'><th><span style='color: red;font-size: 1em;margin-right: 5px;'>&#128712;</span>Advertencias. Pueden provocar un requerimiento de la AEAT</th></tr>");
                contenidoHtml.AppendLine(generarFilasHtml("avisos"));
                contenidoHtml.AppendLine("</table>");
            }

            //Cierre del html
            contenidoHtml.AppendLine("</body>");
            contenidoHtml.AppendLine("</html>");

            return contenidoHtml.ToString();
        }

        private string generarFilasHtml(string clave)
        {
            string elementos = string.Empty;
            string color_error = "#FFEBEE";
            string color_aviso = "#FCF3CF";
            string color_advertencia = "#EBF5FB";

            switch (clave)
            {
                case "errores":
                    foreach (var elemento in erroresArray)
                    {
                        elementos += $"<tr style='background-color: {color_error}'><tr><td>{elemento}</td></tr>";
                    }
                    break;

                case "avisos":
                    foreach (var elemento in avisosArray)
                    {
                        elementos += $"<tr style='background-color: {color_aviso}'><tr><td>{elemento}</td></tr>";
                    }
                    break;

                case "advertencias":
                    foreach (var elemento in advertenciasArray)
                    {
                        elementos += $"<tr style='background-color: {color_advertencia}'><tr><td>{elemento}</td></tr>";
                    }
                    break;

                default:
                    break;

            }

            return elementos;
        }
    }
}
