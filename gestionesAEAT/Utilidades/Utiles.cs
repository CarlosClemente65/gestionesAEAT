using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using gestionesAEAT.Metodos;
using gestionesAEAT.Utilidades;
using Microsoft.Win32;

namespace gestionesAEAT
{
    public static class Utiles
    {
        //Clase con diversas utilidades que se usan en los metodos
        public static string url { get; set; } //Variable que almacena la url a la que enviar los datos a la AEAT

        public static string cliente { get; set; }

        //Variables para almacenar en listas los datos de la cabecera, body y respuesta que vienen en el guion
        public static List<string> cabecera = new List<string>(); //Lineas de la cabecera
        public static List<string> body = new List<string>(); //Lineas del body 
        public static List<string> respuesta = new List<string>(); //Lineas de la respuesta 
        public static List<string> textoGuion = new List<string>(); //Lista que almacena las lineas del guion para procesarlo

        //Variables para almacenar las respuestas del envio
        public static List<string> erroresArray = new List<string>();
        public static List<string> avisosArray = new List<string>();
        public static List<string> advertenciasArray = new List<string>();

        public static RespuestaValidarModelos respuestaValidarModelos; //Variable que almacena la respuesta completa de la AEAT en la validacion de modelos
        public static RespuestaPresBasicaDos respuestaEnvioModelos; //Variable que almacena la respuesta completa de la AEAT en la presentacion directa

        public static bool grabadaSalida = false;

        public static string quitaRaros(string cadena)
        {
            //Metodo para eliminar caracteres raros
            Dictionary<char, char> caracteresReemplazo = new Dictionary<char, char>
            {
                {'á', 'a'}, {'é', 'e'}, {'í', 'i'}, {'ó', 'o'}, {'ú', 'u'},
                {'Á', 'A'}, {'É', 'E'}, {'Í', 'I'}, {'Ó', 'O'}, {'Ú', 'U'},
                {'¥', 'Ñ'}, {'¤', 'ñ'}
                //{'\u00AA', '.'}, {'ª', '.'}, {'\u00BA', '.'}, {'°', '.' }
            };
            //Nota: los caracteres ª y º estan con la forma unicode y en caracter para contemplar ambas opciones, pero los comento porque no esta mal que salgan (si dan pegas ya se arreglara)

            StringBuilder resultado = new StringBuilder(cadena.Length);
            foreach(char c in cadena)
            {
                if(caracteresReemplazo.TryGetValue(c, out char reemplazo))
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

        public static void cargaFicheroEntrada(string ficheroEntrada)
        {
            //Carga el fichero de entrada en una lista y obtiene la codificacion UTF-8 o ISO8859-1 (ascii extendido 256 bits o ansi), ya que algun guion se le pasa como parametro la codificacion

            //Carga del fichero
            using(StreamReader sr = new StreamReader(ficheroEntrada, Parametros.codificacion))
            {
                string linea;
                while((linea = sr.ReadLine()) != null)
                {
                    textoGuion.Add(linea);
                }
            }

            //Obtiene la codificacion
            string cadena;
            int bloque = 0;
            Encoding nuevaCodificacion = null;

            for(int x = 0; x < textoGuion.Count; x++)
            {
                cadena = textoGuion[x].ToString().Trim();
                if(cadena != "")
                {
                    switch(cadena)
                    {
                        case "[cabecera]":
                            bloque = 2;
                            continue;
                    }

                    if(bloque == 2)
                    {
                        (string atributo, string valor) = divideCadena(cadena.ToString(), '=');
                        if(atributo == "CODIFICACION")
                        {
                            if(valor.Length > 1)
                            {
                                try
                                {
                                    //Se intenta asignar el valor de la codificacion para evitar una excepcion
                                    nuevaCodificacion = Encoding.GetEncoding(valor);
                                    Parametros.codificacion = nuevaCodificacion;
                                }
                                catch
                                {
                                    //Si no se puede asignar se deja el valor por defecto que es UTF-8
                                }
                            }
                            break;
                        }
                    }
                }
            }

            //Si se detecta que se ha cargado una nueva codificacion, se vuelve a cargar el guion
            if(nuevaCodificacion != null && nuevaCodificacion != Encoding.UTF8)
            {
                textoGuion.Clear();
                using(StreamReader sr = new StreamReader(ficheroEntrada, nuevaCodificacion))
                {
                    string linea;
                    while((linea = sr.ReadLine()) != null)
                    {
                        textoGuion.Add(linea);
                    }
                }
            }
        }

        public static void borrarFicheros(string fichero)
        {
            //Borra ficheros anteriores antes de algunas ejecuciones
            string rutaSalida = string.Empty;
            if(!string.IsNullOrEmpty(fichero))
            {
                rutaSalida = Path.GetDirectoryName(fichero);
            }
            if(string.IsNullOrEmpty(rutaSalida))
            {
                rutaSalida = Directory.GetCurrentDirectory();
            }
            string patronFicheros = Path.GetFileNameWithoutExtension(fichero) + ".*";
            string[] elementos = Directory.GetFiles(rutaSalida, patronFicheros);//Se borran todos los ficheros de salida posibles ya que puede haber .txt, .html o .pdf

            foreach(string elemento in elementos)
            {
                //Evitamos borrar el fichero de entrada por si tienen el mismo nombre
                bool controlEntrada = Path.GetFileName(Parametros.ficheroEntrada) == Path.GetFileName(elemento);

                if(!controlEntrada)
                {
                    bool eliminado = false;
                    do
                    {

                        try
                        {
                            File.Delete(elemento);
                            eliminado = true;
                        }
                        catch(IOException ex)
                        {
                            DialogResult resultado = MessageBox.Show($"No se pudo eliminar el archivo {elemento} porque está en uso. \n\nCierrelo para continuar", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            //if(resultado == DialogResult.Cancel)
                            //{
                            //    break; // Si el usuario cancela, salimos del bucle
                            //}
                        }
                    }
                    while(!eliminado);
                }
            }
        }

        public static string procesarGuionHtml()
        {
            //Procesa el guion para poder hacer el envio a la AEAT
            string textoAEAT = string.Empty;

            for(int linea = 0; linea < cabecera.Count; linea++)
            {
                if(string.IsNullOrEmpty(textoAEAT))
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

        public static void cargaDatosGuion(string ficheroEntrada)
        {
            //Procesa el fichero de entrada para montar una lista con las lineas del guion
            cargaFicheroEntrada(ficheroEntrada);

            //Lee el fichero de entrada y monta una lista con todas las lineas segun si son de la cabecera, body o respuesta
            string cadena;
            int bloque = 0; //Controla el tipo de dato a grabar en el fichero

            for(int x = 0; x < textoGuion.Count; x++)
            {
                cadena = textoGuion[x].ToString();
                if(cadena != "")
                {
                    //Control para saber que parte del fichero se va a procesar
                    switch(cadena)
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
                    switch(bloque)
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

        public static string generarRespuesta(string ficheroRespuesta, string tipo)
        {
            //Metodo para generar un html si hay errores, avisos o advertencias. Se recibe como parametro el tipo ya que el tratamiento de la respuesta cambia si es en el envio o en la validacion
            string modelo = string.Empty;
            string ejercicio = string.Empty;
            string periodo = string.Empty;
            string respuestaHtml = string.Empty;
            int control = 0;

            switch(tipo)
            {
                case "validar":
                    var respuestaValidar = respuestaValidarModelos.respuesta;
                    if(respuestaValidar.errores != null && respuestaValidar.errores.Count > 0)
                    {
                        erroresArray = respuestaValidar.errores;
                        control++;
                    }

                    if(respuestaValidar.avisos != null && respuestaValidar.avisos.Count > 0)
                    {
                        avisosArray = respuestaValidar.avisos;
                        control++;
                    }

                    if(respuestaValidar.advertencias != null && respuestaValidar.advertencias.Count > 0)
                    {
                        advertenciasArray = respuestaValidar.advertencias;
                        control++;
                    }

                    break;

                case "enviar":
                    var respuestaEnvio = respuestaEnvioModelos.respuesta;
                    if(respuestaEnvio.correcta != null)
                    {
                        //Si viene la respuesta correcta mirar si hay avisos o advertencias
                        if(respuestaEnvio.correcta.avisos != null && respuestaEnvio.correcta.avisos.Count > 0)
                        {
                            avisosArray = respuestaEnvio.correcta.avisos;
                            control++;
                        }

                        if(respuestaEnvio.correcta.advertencias != null && respuestaEnvio.correcta.advertencias.Count > 0)
                        {
                            advertenciasArray = respuestaEnvio.correcta.advertencias;
                            control++;
                        }
                    }

                    if(respuestaEnvio.errores != null && respuestaEnvio.errores.Count > 0)
                    {
                        erroresArray = respuestaEnvio.errores;
                        control++;
                    }

                    break;
            }

            //Si se ha encontrado algun error, aviso o advertencia, hace el html
            if(control > 0)
            {
                //Asigna las variables modelo, ejercicio y periodo segun los valores de la cabecera
                foreach(string linea in cabecera)
                {
                    (string atributo, string valor) = divideCadena(linea, '=');

                    switch(atributo)
                    {
                        case "MODELO":
                            modelo = valor;
                            break;

                        case "EJERCICIO":
                            ejercicio = valor;
                            break;

                        case "PERIODO":
                            periodo = valor;
                            break;
                    }

                }
                cliente = Parametros.cliente;

                respuestaHtml = generarHtml(modelo, ejercicio, periodo, cliente);
            }

            return respuestaHtml;
        }

        private static string generarHtml(string modelo, string ejercicio, string periodo, string cliente)
        {
            //Metodo que crea el html

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
            if(erroresArray != null && erroresArray.Count > 0)
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
            if(advertenciasArray != null && advertenciasArray.Count > 0)
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
            if(avisosArray != null && avisosArray.Count > 0)
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

        private static string generarFilasHtml(string clave, string fondo, string borde)
        {
            //Metodo que devuelve todas las lineas de los errores, avisos o advertencias con sus colores correspondientes
            StringBuilder elementos = new StringBuilder();
            List<string> listaElementos = null;

            switch(clave)
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
            }

            foreach(var elemento in listaElementos)
            {
                elementos.AppendLine($@"          <tr style='background-color: {fondo} ; border: 1px solid  {borde}'><td>{elemento}</td></tr>");
            }

            return elementos.ToString();
        }

        public static void SalirAplicacion(string mensaje)
        {
            //Si hay algun texto de error en el log, lo graba en un fichero
            if(!string.IsNullOrEmpty(mensaje))
            {
                GrabarSalida(mensaje, Parametros.ficheroResultado);
                grabadaSalida = true;
            }
            Environment.Exit(0);
        }

        public static void MostrarAyuda()
        {
            StringBuilder mensaje = new StringBuilder();
            mensaje.AppendLine("");
            mensaje.AppendLine(@"Uso de la aplicacion: gestionesAEAT.exe clave c:\carpeta\guion.txt");
            mensaje.AppendLine(@"    clave\t\tclave de ejecucion del programa");
            mensaje.AppendLine(@"    opciones.txt\tFichero que contiene las siguientes opciones que admite la aplicacion:");
            mensaje.AppendLine("\n     Parametros que pueden incluirse en el guion:");
            mensaje.AppendLine(@"        CLIENTE= Codigo de cliente para incluirlo en el html de respuestas");
            mensaje.AppendLine(@"        TIPO= Tipo de proceso a ejecutar segun la siguiente lista:");
            mensaje.AppendLine(@"            1 = Envio de modelos");
            mensaje.AppendLine(@"            2 = Validar modelos (no necesita certificado)");
            mensaje.AppendLine(@"            3 = Consulta y descarga PDF de modelos presentados");
            mensaje.AppendLine(@"            4 = Ratificacion domicilio renta");
            mensaje.AppendLine(@"            5 = Descarga datos fiscales renta");
            mensaje.AppendLine(@"            6 = Envio de facturas al SII");
            mensaje.AppendLine(@"            7 = Prestar declaraciones informativas");
            mensaje.AppendLine(@"            8 = Alta y consulta de pagos de modelos mediante cargo en cuenta con NRC");
            mensaje.AppendLine(@"            9 = Descarga de documentos PDF a traves del codigo CSV");
            mensaje.AppendLine(@"        ENTRADA= Nombre del fichero con los datos a enviar");
            mensaje.AppendLine(@"        SALIDA= Nombre del fichero donde se grabara la salida");
            mensaje.AppendLine(@"        URLSII= Url a la que hacer el envio de facturas al SII");
            mensaje.AppendLine(@"        RESPUESTA= Etiquetas del xml de respuesta en el envio al SII de las que se quiere obtener los resultados");
            mensaje.AppendLine(@"        OBLIGADO= Indica si el proceso necesita usar certificado (valores SI/NO)");
            mensaje.AppendLine(@"        BUSQUEDA= Cadena a buscar en los certificados (numero serie, NIF o nombre del titular o del representante del certificado");
            mensaje.AppendLine(@"        CERTIFICADO= Nombre del fichero.pfx que contiene el certificado digital");
            mensaje.AppendLine(@"        PASSWORD= Contraseña del certificado que se pasa por fichero");
            mensaje.AppendLine(@"        NIFRENTA= Para la descarga de datos fiscales es necesario el NIF del contribuyente");
            mensaje.AppendLine(@"        REFRENTA= Codigo de 5 caracteres de la referencia de renta para la descarga de datos fiscales");
            mensaje.AppendLine(@"        DPRENTA= En la descarga de datos fiscales indica si se quieren tambien los datos personales (valor 'S' o 'N')");
            mensaje.AppendLine(@"        URLRENTA= Direccion a la que hacer la peticion de descarga de datos fiscales (cambia cada año)");
            mensaje.AppendLine(@"        URLCSV= Url de descarga del documento mediante codigo CSV");
            mensaje.AppendLine("\nEjemplos de fichero guion:");
            mensaje.AppendLine(@"    Envio modelos con numero de serie:");
            mensaje.AppendLine(@"        CLIENTE=00001");
            mensaje.AppendLine(@"        TIPO=1");
            mensaje.AppendLine(@"        ENTRADA=guion.txt");
            mensaje.AppendLine(@"        SALIDA=salida.txt");
            mensaje.AppendLine(@"        OBLIGADO=SI");
            mensaje.AppendLine(@"        BUSQUEDA=numeroSerieCertificado");
            mensaje.AppendLine(@"    Validar modelos:");
            mensaje.AppendLine(@"        CLIENTE=00001");
            mensaje.AppendLine(@"        TIPO=2");
            mensaje.AppendLine(@"        ENTRADA=guion.txt");
            mensaje.AppendLine(@"        SALIDA=salida.txt");
            mensaje.AppendLine(@"        OBLIGADO=NO");
            mensaje.AppendLine(@"    Consulta modelos con fichero y password:");
            mensaje.AppendLine(@"        CLIENTE=00001");
            mensaje.AppendLine(@"        TIPO=3");
            mensaje.AppendLine(@"        ENTRADA=guion.txt");
            mensaje.AppendLine(@"        SALIDA=salida.txt");
            mensaje.AppendLine(@"        OBLIGADO=SI");
            mensaje.AppendLine(@"        CERTIFICADO=certificado.pfx");
            mensaje.AppendLine(@"        PASSWORD=contraseña");
            mensaje.AppendLine(@"    Ratificar domicilio renta con NIF del titular del certificado");
            mensaje.AppendLine(@"        CLIENTE=00001");
            mensaje.AppendLine(@"        TIPO=4");
            mensaje.AppendLine(@"        ENTRADA=guion.txt");
            mensaje.AppendLine(@"        SALIDA=salida.txt");
            mensaje.AppendLine(@"        OBLIGADO=SI");
            mensaje.AppendLine(@"        BUSQUEDA=nifTitularCertificado");
            mensaje.AppendLine(@"    Descarga datos fiscales:");
            mensaje.AppendLine(@"        TIPO=5");
            mensaje.AppendLine(@"        SALIDA=salida.txt");
            mensaje.AppendLine(@"        NIFRENTA=nifTitular");
            mensaje.AppendLine(@"        REFRENTA=referenciaRenta");
            mensaje.AppendLine(@"        DPRENTA=S");
            mensaje.AppendLine(@"        URLRENTA=https://www9.agenciatributaria.gob.es/wlpl/DFPA-D182/SvDesDF23Pei");
            mensaje.AppendLine(@"    Envio facturas al SII con nombre del titular del certificado:");
            mensaje.AppendLine(@"        CLIENTE=00001");
            mensaje.AppendLine(@"        TIPO=6");
            mensaje.AppendLine(@"        ENTRADA=facturaEmitida.xml");
            mensaje.AppendLine(@"        SALIDA=respuesta.xml");
            mensaje.AppendLine(@"        URLSII=https://prewww1.aeat.es/wlpl/SSII-FACT/ws/fe/SiiFactFEV1SOAP");
            mensaje.AppendLine(@"        OBLIGADO=SI");
            mensaje.AppendLine(@"        BUSQUEDA=nombreCertificado");
            mensaje.AppendLine(@"    Presentacion de informativas con nombre del titular del certificado:");
            mensaje.AppendLine(@"        CLIENTE=00001");
            mensaje.AppendLine(@"        TIPO=7");
            mensaje.AppendLine(@"        ENTRADA=guion.txt");
            mensaje.AppendLine(@"        SALIDA=salida.txt");
            mensaje.AppendLine(@"        OBLIGADO=SI");
            mensaje.AppendLine(@"        BUSQUEDA=nombreCertificado");
            mensaje.AppendLine(@"    Pago de modelos mediante solicitud de NRC con nombre del titular del certificado:");
            mensaje.AppendLine(@"        CLIENTE=00001");
            mensaje.AppendLine(@"        TIPO=8");
            mensaje.AppendLine(@"        ENTRADA=guion.txt");
            mensaje.AppendLine(@"        SALIDA=salida.txt");
            mensaje.AppendLine(@"        OBLIGADO=SI");
            mensaje.AppendLine(@"        BUSQUEDA=nombreCertificado");
            mensaje.AppendLine(@"    Descarga de documentos PDF mediante CSV (no necesita certificado)");
            mensaje.AppendLine(@"        TIPO=9");
            mensaje.AppendLine(@"        CSV=https://prewww2.aeat.es/wlpl/inwinvoc/es.aeat.dit.adu.eeca.catalogo.vis.VisualizaSc?COMPLETA=SI&ORIGEN=E&NIF=&CSV=SDMCFEZXM8QKF9ZU");
            mensaje.AppendLine(@"        SALIDA=salida.PDF");
            mensaje.AppendLine("\nNotas:");
            mensaje.AppendLine(@"    - Si no se pasan los datos del certificado y el proceso lo requerire, se mostrara el formulario de seleccion");
            mensaje.AppendLine(@"    	- En la validacion de modelos, si el guion tiene la variable VALIDAR=NO se genera el pdf sin validar (el defecto es SI)");
            mensaje.AppendLine(@"    	- En todos los procesos, el fichero de entrada tiene la estructura siguiente:");
            mensaje.AppendLine(@"    	    [url] - En la linea siguiente ira la direccion a la que invocar el servicio");
            mensaje.AppendLine(@"    	    [cabecera] - En las lineas siguientes iran todos los parametros que sean necesarios enviar el servicio con el formato PARAMETRO = valor");
            mensaje.AppendLine(@"    	    [body] - En las lineas siguientes iran todas las lineas que sea necesario enviar al servicio");
            mensaje.AppendLine(@"    	[respuesta] - En las lineas siguientes iran todas las variables que se espera recibir como respuesta");
            mensaje.AppendLine(@"    	              Nota: algunos procesos no necesitan esta parte ya que estan previstas en clases especificas de respuesta");
            mensaje.AppendLine(@"    	- En el proceso de descarga de documentos con CSV el fichero de entrada no es necesario, ya que se pasan los parametros en el fichero de opciones");
            mensaje.AppendLine("\nPulse una tecla para continuar");

            Console.WriteLine(mensaje.ToString());
            Console.ReadLine();
        }

        public static string formateaXML(string xmlRespuesta)
        {
            // Crea un objeto XmlDocument y carga el XML de la respuesta
            XmlDocument documento = new XmlDocument();
            documento.LoadXml(xmlRespuesta);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                NewLineHandling = NewLineHandling.Replace,
                NewLineChars = "\n",
                OmitXmlDeclaration = false
            };

            using(var stringWriter = new StringWriter())
            using(var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                documento.Save(xmlWriter);
                return stringWriter.ToString();
            }
        }

        public static void GrabarSalida(string mensajeSalida, string ficheroSalida)
        {
            if(!grabadaSalida) File.WriteAllText(ficheroSalida, mensajeSalida, Encoding.Default);
        }

        public static string generaFicheroSalida<T>(T proceso, string ficheroPdf = "")
        {
            //Se recibe un tipo generico (T) que luego se usa para montar las respuestas segun de donde vienen
            StringBuilder resultadoSalida = new StringBuilder();

            //
            switch(proceso)
            {
                case RespuestaValidarModelos respuestaValidar:
                    //Si se ha generado el PDF lo graba en el fichero de salida
                    if(respuestaValidar.respuesta.pdf != null && respuestaValidarModelos.respuesta.pdf.Count > 0)
                    {
                        resultadoSalida.AppendLine($"pdf = {ficheroPdf}");
                    }
                    else
                    {
                        resultadoSalida.AppendLine("pdf =");
                    }

                    //Si se han generado errores los graba en el fichero de salida
                    if(respuestaValidar.respuesta.errores != null && respuestaValidar.respuesta.errores.Count > 0)
                    {
                        List<string> listaErrores = new List<string>();
                        listaErrores = erroresArray;
                        int linea = 0;
                        for(int i = 0; i < listaErrores.Count; i++)
                        {
                            listaErrores[i] = Regex.Replace(listaErrores[i], @"E\d{2} \- ", "");
                            resultadoSalida.AppendLine($"E{linea.ToString("D2")} = {listaErrores[i]}");
                            linea++;
                        }
                    }

                    //Si se han generado avisos los graba en el fichero de salida
                    if(respuestaValidar.respuesta.avisos != null && respuestaValidar.respuesta.avisos.Count > 0)
                    {
                        List<string> listaAvisos = new List<string>();
                        listaAvisos = avisosArray;
                        int linea = 0;
                        foreach(var elemento in listaAvisos)
                        {
                            resultadoSalida.AppendLine($"A{linea.ToString("D2")} = {elemento}");
                            linea++;
                        }
                    }

                    //Si se han generado advertencias las graba en el fichero de salida
                    if(respuestaValidar.respuesta.advertencias != null && respuestaValidar.respuesta.advertencias.Count > 0)
                    {
                        List<string> listaAdvertencias = new List<string>();
                        listaAdvertencias = advertenciasArray;
                        int linea = 0;
                        foreach(var elemento in listaAdvertencias)
                        {
                            resultadoSalida.AppendLine($"D{linea.ToString("D2")} = {elemento}");
                            linea++;
                        }
                    }
                    break;

                case RespuestaPresBasicaDos respuestaPresentacionDirecta:
                    //Si se han generado errores los graba en el fichero de salida
                    if(respuestaPresentacionDirecta.respuesta.errores != null && respuestaPresentacionDirecta.respuesta.errores.Count > 0)
                    {
                        List<string> listaErrores = new List<string>();
                        listaErrores = erroresArray;
                        int linea = 0;
                        for(int i = 0; i < listaErrores.Count; i++)
                        {
                            listaErrores[i] = Regex.Replace(listaErrores[i], @"E\d{2} \- ", "");
                            resultadoSalida.AppendLine($"E{linea.ToString("D2")} = {listaErrores[i]}");
                            linea++;
                        }
                    }

                    //Si se han generado avisos los graba en el fichero de salida
                    if(respuestaPresentacionDirecta.respuesta.correcta.avisos != null && respuestaPresentacionDirecta.respuesta.correcta.avisos.Count > 0)
                    {
                        List<string> listaAvisos = new List<string>();
                        listaAvisos = avisosArray;
                        int linea = 0;
                        foreach(var elemento in listaAvisos)
                        {
                            resultadoSalida.AppendLine($"A{linea.ToString("D2")} = {elemento}");
                            linea++;
                        }
                    }

                    //Si se han generado advertencias las graba en el fichero de salida
                    if(respuestaPresentacionDirecta.respuesta.correcta.advertencias != null && respuestaPresentacionDirecta.respuesta.correcta.advertencias.Count > 0)
                    {
                        List<string> listaAdvertencias = new List<string>();
                        listaAdvertencias = advertenciasArray;
                        int linea = 0;
                        foreach(var elemento in listaAdvertencias)
                        {
                            resultadoSalida.AppendLine($"D{linea.ToString("D2")} = {elemento}");
                            linea++;
                        }
                    }

                    break;

            }

            return resultadoSalida.ToString();
        }

        public static (string, string) divideCadena(string cadena, char divisor)
        {
            //Permite dividir una cadena por el divisor pasado y solo la divide en un maximo de 2 partes (divide desde el primer divisor que encuentra)
            string atributo = string.Empty;
            string valor = string.Empty;
            string[] partes = cadena.Split(new[] { divisor }, 2);
            if(partes.Length == 2)
            {
                atributo = partes[0].Trim();
                valor = partes[1].Trim();
            }

            return (atributo, valor);
        }

        public static bool ChequeoFramework(int versionNecesaria)
        {
            int versionInstalada = 0;
            using(RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
            .OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
            {
                if(ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    versionInstalada = (int)ndpKey.GetValue("Release");
                }
            }
            return versionInstalada >= versionNecesaria;
        }

    }
}
