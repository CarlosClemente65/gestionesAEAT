using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace gestionesAEAT.Utilidades
{
    public class Parametros
    {
        public string dsclave { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string pathFicheros { get; set; } = string.Empty;
        public string ficheroOpciones { get; set; } = string.Empty;
        public string ficheroEntrada { get; set; } = string.Empty;
        public string ficheroSalida { get; set; } = string.Empty;
        public string ficheroErrores { get; set; } = string.Empty;
        public string textoBusqueda { get; set; } = string.Empty;
        public string serieCertificado { get; set; } = string.Empty;
        public string ficheroCertificado { get; set; } = string.Empty;
        public string passwordCertificado { get; set; } = string.Empty;
        public bool conCertificado { get; set; } = false;
        public string nifDf { get; set; } = string.Empty;
        public string refRenta { get; set; } = string.Empty;
        public string datosPersonales { get; set; } = "S";
        public string urlDescargaDf { get; set; } = string.Empty;
        public int indiceUrl { get; set; } = -1;

        public string cliente { get; set; } = string.Empty;


        public Parametros(string rutaFichero)
        {
            this.ficheroOpciones = rutaFichero;
            CargarOpciones();
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
                    }
                }
            }

            else if (tipoPropiedad.PropertyType == typeof(int))
            {
                if (propiedad == "indiceUrl")
                {
                    if ((int)valorPropiedad < 0)
                    {
                        mensaje = "No se ha pasado el indice de la url a la que enviar las facturas";
                    }
                }
            }

            return mensaje;

        }


        private void CargarOpciones()
        {
            //Metodo para procesar el fichero de opciones
            string[] lineas = File.ReadAllLines(ficheroOpciones);
            foreach (string linea in lineas)
            {
                //Evita procesar lineas vacias
                if (string.IsNullOrWhiteSpace(linea)) continue;

                //Divide la linea en clave=valor
                string[] partes = linea.Split('=');
                string clave = string.Empty;
                string valor = string.Empty;
                if (partes.Length == 2)
                {
                    clave = partes[0].Trim();
                    valor = partes[1].Trim();
                }

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
                        ficheroErrores = Path.Combine(pathFicheros, "errores.txt");
                        ficheroSalida = valor;
                        break;

                    case "INDICESII":
                        if (int.TryParse(valor, out int valorUrl)) indiceUrl = valorUrl;
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

        public static class Configuracion
        {
            public static Parametros Parametros { get; private set; }

            public static void Inicializar(string RutaFichero)
            {
                Parametros = new Parametros(RutaFichero);
            }
        }
    }
}
