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

        public RespuestaValidarModelos respuestaValidarModelos; //Varible que almacena la respuesta completa de la AEAT en la validacion de modelos
        public RespuestaPresBasicaDos respuestaEnvioModelos; //Variable que almacena la respuesta completa de la AEAT en la presentacion directa

        public string quitaRaros(string cadena)
        {
            //Metodo para eliminar caracteres raros
            Dictionary<char, char> caracteresReemplazo = new Dictionary<char, char>
            {
                {'á', 'a'}, {'é', 'e'}, {'í', 'i'}, {'ó', 'o'}, {'ú', 'u'},
                {'Á', 'A'}, {'É', 'E'}, {'Í', 'I'}, {'Ó', 'O'}, {'Ú', 'U'}
                //{'\u00AA', '.'}, {'ª', '.'}, {'\u00BA', '.'}, {'°', '.' }
            };
            //Nota: los caracteres ª y º estan con la forma unicode y en caracter para contemplar ambas opcioens, pero los comento porque no esta mal que salgan (si dan pegas ya se arreglara)

            StringBuilder resultado = new StringBuilder(cadena.Length);
            foreach (char c in cadena)
            {
                if (caracteresReemplazo.TryGetValue(c, out char reemplazo))
                {
                    resultado.Append(reemplazo);
                }
                else
                {
                    resultado.Append(c);
                }
            }

            return resultado.ToString();
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

        public string generarRespuesta(string ficheroRespuesta, string tipo)
        {
            string modelo = string.Empty;
            string ejercicio = string.Empty;
            string periodo = string.Empty;
            string cliente = string.Empty;
            string respuestaHtml = string.Empty;
            int control = 0;

            switch (tipo)
            {
                case "validar":
                    var respuestaValidar = respuestaValidarModelos.respuesta;
                    if (respuestaValidar.errores != null && respuestaValidar.errores.Count > 0)
                    {
                        erroresArray = respuestaValidar.errores;
                        control++;
                    }

                    if (respuestaValidar.avisos != null && respuestaValidar.avisos.Count > 0)
                    {
                        avisosArray = respuestaValidar.avisos;
                        control++;
                    }

                    if (respuestaValidar.advertencias != null && respuestaValidar.advertencias.Count > 0)
                    {
                        advertenciasArray = respuestaValidar.advertencias;
                        control++;
                    }

                    break;

                case "enviar":
                    var respuestaEnvio = respuestaEnvioModelos.respuesta;
                    if (respuestaEnvio.correcta != null)
                    {
                        //Si viene la respuesta correcta mirar si hay avisos o advertencias
                        if (respuestaEnvio.correcta.avisos != null)
                        {
                            avisosArray = respuestaEnvio.correcta.avisos;
                            control++;
                        }

                        if (respuestaEnvio.correcta.advertencias != null)
                        {
                            advertenciasArray = respuestaEnvio.correcta.advertencias;
                            control++;
                        }
                    }

                    if (respuestaEnvio.errores != null)
                    {
                        erroresArray = respuestaEnvio.errores;
                        control++;
                    }

                    break;
            }

            //Si se ha encontrado algun error, aviso o advertencia, hace el html
            if (control > 0)
            {
                int indice;

                //Asigna las variables modelo, ejercicio y periodo segun los valores de la cabecera
                foreach (string linea in cabecera)
                {
                    indice = linea.IndexOf("=");
                    if (indice != -1)
                    {
                        if (linea.StartsWith("MODELO")) modelo = linea.Substring(indice + 1);
                        if (linea.StartsWith("EJERCICIO")) ejercicio = linea.Substring(indice + 1);
                        if (linea.StartsWith("PERIODO")) periodo = linea.Substring(indice + 1);

                        //Cuando se ponga en la cabecera el cliente habilitar esta parte
                        if (linea.StartsWith("CLIENTE")) cliente = linea.Substring(indice + 1);
                    }
                }

                //int posicion;
                ////Intenta asignar el numero de cliente tomandolo del nombre del fichero de respuesta
                //try
                //{
                //    string codCliente = Path.GetFileNameWithoutExtension(ficheroRespuesta);
                //    posicion = codCliente.IndexOf("_salida");
                //    if (posicion != -1)
                //    {
                //        cliente = codCliente.Substring(0, posicion);

                //    }
                //}
                //catch (Exception ex)
                //{
                //    //Si no se encuentra el cliente se devuelve vacio
                //}

                respuestaHtml = generarHtml(modelo, ejercicio, periodo, cliente);
            }

            return respuestaHtml;
        }

        private string generarHtml(string modelo, string ejercicio, string periodo, string cliente)
        {
            StringBuilder respuestaHtml = new StringBuilder();

            //Construye el html
            StringBuilder contenidoHtml = new StringBuilder();

            //Cabecera del html y datos informativos del cliente, modelo, ejercicio y periodo
            contenidoHtml.AppendLine("<!DOCTYPE html>");
            contenidoHtml.AppendLine(@"<html");
            contenidoHtml.AppendLine(@"<head>");
            contenidoHtml.AppendLine(@"  <meta charset='utf-8'>");
            contenidoHtml.AppendLine(@"  <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css'>");
            contenidoHtml.AppendLine(@"  <style>");
            contenidoHtml.AppendLine(@"    th, td{padding: 5px 5px 5px 15px;text-align: justify; font-size:1em}");
            contenidoHtml.AppendLine(@"    td{font-size:0.9em;padding: 5px 20px 5px 40px}");
            contenidoHtml.AppendLine(@"  </style>");
            contenidoHtml.AppendLine(@"</head>");
            contenidoHtml.AppendLine(@"<body  style='margin: 40px; font-family: Calibri; font-size: 1.2em;'>");
            contenidoHtml.AppendLine(@"  <title>Resultado de la validaci&oacute;n</title>");
            contenidoHtml.AppendLine(@"  <p style='font-family:Calibri; font-size: 1.5em; text-align:center'>Resultado de la validaci&oacute;n</p>");
            contenidoHtml.AppendLine($@"  <p style='font-family:Calibri; font-size: 0.9em; text-align: center'>Cliente: {cliente}&nbsp;&nbsp;&nbsp;--&nbsp;&nbsp;&nbsp;Modelo: {modelo}&nbsp;&nbsp;&nbsp;--&nbsp;&nbsp;&nbsp;Ejercicio: {ejercicio}&nbsp;&nbsp;&nbsp;--&nbsp;&nbsp;&nbsp;Periodo: {periodo}&nbsp;&nbsp;&nbsp;--&nbsp;&nbsp;&nbsp;Fecha generacion: {DateTime.Now}</p>");

            //Colores para la tabla html de avisos y errores
            string fondo1 = string.Empty; //Cabecera de la tabla
            string fondo2 = string.Empty; //Lineas de la tabla
            string borde = string.Empty; // Borde e icono

            //Contenido del html si hay errores
            if (erroresArray != null && erroresArray.Count > 0)
            {
                fondo1 = "#FFBFBF"; //Cabecera tabla
                fondo2 = "#FFEBEE"; // Lineas tabla
                borde = "#ED1C24"; // Borde e icono

                //Generar tabla de errores
                contenidoHtml.AppendLine($@"  <table style='margin: 10px; width: 100%; border-collapse: collapse; font-size: 1em; border: 1px solid {borde}'>");
                contenidoHtml.AppendLine($@"    <tr style='background-color: {fondo1}'>");
                contenidoHtml.AppendLine(@"      <th>");
                contenidoHtml.AppendLine($@"        <i class='fa-solid fa-rectangle-xmark' style='color: {borde};font-size: 1.2em;margin-right: 5px;'></i>&nbsp;&nbsp;&nbsp;Errores. No es posible presentar la declaracion");
                contenidoHtml.AppendLine(@"      </th>");
                contenidoHtml.AppendLine(@"    </tr>");
                contenidoHtml.AppendLine(generarFilasHtml("errores", fondo2, borde));
                contenidoHtml.AppendLine(@"  </table>");
            }

            //Contenido del html si hay advertencias
            if (advertenciasArray != null && advertenciasArray.Count > 0)
            {
                fondo1 = "#F9E79F"; // Cabecera tabla
                fondo2 = "#FCF3CF"; // Lineas tabla
                borde = "#FFA500"; // Borde e icono

                //Generar tabla de advertencias
                contenidoHtml.AppendLine($@"  <table style='margin: 10px; width: 100%; border-collapse: collapse; font-size: 1em; border: 1px solid {borde}'>");
                contenidoHtml.AppendLine($@"    <tr style='background-color: {fondo1}'>");
                contenidoHtml.AppendLine(@"      <th>");
                contenidoHtml.AppendLine($@"        <i class='fa-solid fa-triangle-exclamation' style='color: {borde};font-size: 1.2em;margin-right: 5px;'></i>&nbsp;&nbsp;&nbsp;Advertencias. Pueden provocar un requerimiento de la AEAT");
                contenidoHtml.AppendLine(@"      </th>");
                contenidoHtml.AppendLine(@"    </tr>");

                contenidoHtml.AppendLine(generarFilasHtml("advertencias", fondo2, borde));
                contenidoHtml.AppendLine(@"  </table>");
            }

            //Contenido del html si hay avisos
            if (avisosArray != null && avisosArray.Count > 0)
            {
                //Generar tabla de avisos
                fondo1 = "#AED6F1"; // Cabecera tabla
                fondo2 = "#EBF5FB"; // Lineas tabla
                borde = "#6A5ACD"; // Borde e icono

                contenidoHtml.AppendLine($@"  <table style='margin: 10px; width: 100%; border-collapse: collapse; font-size: 1em; border: 1px solid {borde}'>");
                contenidoHtml.AppendLine($@"    <tr style='background-color: {fondo1}'>");
                contenidoHtml.AppendLine(@"      <th>");
                contenidoHtml.AppendLine($@"        <i class='fa-solid fa-circle-info' style='color: {borde};font-size: 1.2em;margin-right: 5px;'></i>&nbsp;&nbsp;&nbsp;Avisos que deben revisarse. Permiten presentar la declaracion");
                contenidoHtml.AppendLine(@"      </th>");
                contenidoHtml.AppendLine(@"    </tr>");
                contenidoHtml.AppendLine(generarFilasHtml("avisos", fondo2, borde));
                contenidoHtml.AppendLine(@"  </table>");
            }

            //Cierre del html
            contenidoHtml.AppendLine(@" </body>");
            contenidoHtml.AppendLine("</html>");

            return contenidoHtml.ToString();
        }

        private string generarFilasHtml(string clave, string fondo, string borde)
        {
            StringBuilder elementos = new StringBuilder();
            List<string> listaElementos = null;

            switch (clave)
            {
                case "errores":
                    listaElementos = erroresArray;
                    break;

                case "avisos":
                    listaElementos = avisosArray;
                    break;

                case "advertencias":
                    listaElementos = advertenciasArray;
                    break;

                default:
                    return string.Empty;
                    break;
            }

            foreach (var elemento in listaElementos)
            {
                elementos.AppendLine($@"          <tr style='background-color: {fondo} ; border: 1px solid  {borde}'><td>{elemento}</td></tr>");
            }

            return elementos.ToString();
        }
    }
}
