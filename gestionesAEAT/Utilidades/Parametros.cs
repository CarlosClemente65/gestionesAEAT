using System.IO;
using System.Reflection;

namespace gestionesAEAT.Utilidades
{
    public class Parametros
    {
        public static string dsclave { get; set; } = string.Empty;
        public static string tipo { get; set; } = string.Empty;
        public static string pathFicheros { get; set; } = string.Empty;
        public static string ficheroOpciones { get; set; } = string.Empty;
        public static string ficheroEntrada { get; set; } = string.Empty;
        public static string ficheroSalida { get; set; } = string.Empty;
        public static string ficheroResultado { get; set; } = "errores.sal";
        public static string textoBusqueda { get; set; } = string.Empty;
        public static string serieCertificado { get; set; } = string.Empty;
        public static string ficheroCertificado { get; set; } = string.Empty;
        public static string passwordCertificado { get; set; } = string.Empty;
        public static bool conCertificado { get; set; } = false;
        public static string nifDf { get; set; } = string.Empty;
        public static string refRenta { get; set; } = string.Empty;
        public static string datosPersonales { get; set; } = "S";
        public static string urlDescargaDf { get; set; } = string.Empty;
        public static string UrlSii { get; set; } = string.Empty;
        public static string[] respuesta { get; set; } = new string[0];
        public static string cliente { get; set; } = string.Empty;

        static Utiles utilidad = new Utiles();

        public static void CargarOpciones(string _ficheroOpciones)
        {
            ficheroOpciones = _ficheroOpciones;
            //Metodo para procesar el fichero de opciones
            string[] lineas = File.ReadAllLines(ficheroOpciones);
            foreach (string linea in lineas)
            {
                //Evita procesar lineas vacias
                if (string.IsNullOrWhiteSpace(linea)) continue;

                //Divide la linea en clave=valor
                string clave = string.Empty;
                string valor = string.Empty;
                (clave, valor) = utilidad.divideCadena(linea, '=');

                switch (clave)
                {
                    case "CLIENTE":
                        cliente = valor;
                        break;

                    case "TIPO":
                        tipo = valor;
                        break;

                    case "ENTRADA":
                        ficheroEntrada = valor;
                        break;

                    case "SALIDA":
                        //Se controla si se pasa el fichero de salida para evitar una excepcion al asignarlo a la variable
                        if (!string.IsNullOrEmpty(valor))
                        {
                            //Como el fichero de salida siempre tiene que estar presente, se carga la ruta de los ficheros
                            pathFicheros = Path.GetDirectoryName(valor);
                        }
                        ficheroSalida = valor;
                        ficheroResultado = Path.ChangeExtension(ficheroSalida, "sal");
                        break;

                    case "URLSII":
                        UrlSii = valor;
                        break;

                    case "RESPUESTA":
                        //Almacena las etiquetas de las respuestas que queremos procesar
                        respuesta = valor.Trim('[', ']').Split(',');
                        break;

                    case "OBLIGADO":
                        if (valor.ToUpper() == "SI") conCertificado = true;
                        break;

                    case "BUSQUEDA":
                        textoBusqueda = valor;
                        break;

                    case "CERTIFICADO":
                        ficheroCertificado = valor;
                        break;

                    case "PASSWORD":
                        passwordCertificado = valor;
                        break;

                    case "NIFRENTA":
                        nifDf = valor;
                        break;

                    case "REFRENTA":
                        refRenta = valor;
                        break;

                    case "DPRENTA":
                        datosPersonales = valor.ToUpper();
                        if (datosPersonales != "S" && datosPersonales != "N") datosPersonales = "S"; //Se fuerza una 'S' si viene otra cosa como parametro
                        break;

                    case "URLRENTA":
                        urlDescargaDf = valor;
                        break;
                }
            }
        }


        static public string ControlDatosParametros(string propiedad)
        {
            string mensaje = string.Empty;
            Parametros parametros = Configuracion.Parametros;
            PropertyInfo tipoPropiedad = typeof(Parametros).GetProperty(propiedad);
            object valorPropiedad = tipoPropiedad.GetValue(parametros);
            if (tipoPropiedad.PropertyType == typeof(string))
            {
                if (string.IsNullOrEmpty((string)valorPropiedad))
                {
                    switch (propiedad)
                    {
                        case "ficheroEntrada":
                            mensaje = "No se ha pasado el fichero de entrada";
                            break;

                        case "ficheroSalida":
                            mensaje = "No se ha pasado el fichero de salida";
                            break;

                        case "nifDf":
                            mensaje = "No se ha pasado el NIF del contribuyente";
                            break;

                        case "refRenta":
                            mensaje = "No se ha pasado la referencia de la renta";
                            break;

                        case "urlDescargaDf":
                            mensaje = "No se ha pasado la url de descarga de datos fiscales";
                            break;

                        case "UrlSii":
                            mensaje = "No se ha pasado la url a la que enviar las facturas";
                            break;
                    }
                }
            }

            return mensaje;
        }


        public static class Configuracion
        {
            public static Parametros Parametros { get; private set; }

            public static void Inicializar()
            {
                Parametros = new Parametros();
            }
        }
    }
}
