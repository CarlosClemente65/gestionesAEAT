using gestionesAEAT.Formularios;
using gestionesAEAT.Metodos;
using gestionesAEAT.Utilidades;
using System;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

namespace gestionesAEAT
{
    internal class Program
    {
        //Instanciacion de las clases a nivel de clase para hacerlas accesibles a toda la clase
        public static Utiles utilidad = new Utiles();
        static GestionCertificados instanciaCertificado = new GestionCertificados();
        static string log = string.Empty; //Sirve para grabar un log de los posibles errores que se hayan producido en el proceso.

        //Declaracion de variables a nivel de clase para hacerlas accesibles al resto.
        static string dsclave = string.Empty; //Unicamente servira como medida de seguridad de ejecucion y debe pasarse 'ds123456'
        static string ficheroOpciones = string.Empty;
        static string respuestaAeat = string.Empty;
        static Parametros parametros = Parametros.Configuracion.Parametros;
        public static string tituloVentana = string.Empty;

        public static string ficheroErrores = "errores.txt";


        [STAThread] //Atributo necesario para que la aplicacion pueda abrir el formulario de carga de certificado
        static void Main(string[] argumentos)
        {
            //Configurar la carga de las bibliotecas necesarias
            AppDomain.CurrentDomain.AssemblyResolve += CargarDlls;

            try
            {
                if (argumentos.Length < 2)
                {
                    if (argumentos.Length > 0 && (argumentos[0] == "-h" || argumentos[0] == "?"))
                    {
                        utilidad.SalirAplicacion(log);
                    }
                    else
                    {
                        log += $"Son necesarios al menos 2 parametros: dsclave y fichero de opciones";
                        utilidad.SalirAplicacion(log);
                    }
                }

                else
                {
                    dsclave = argumentos[0];
                    if (dsclave != "ds123456")
                    {
                        log += "Clave de ejecucion incorrecta";
                        utilidad.SalirAplicacion(log);
                    }

                    ficheroOpciones = argumentos[1];

                    if (!File.Exists(ficheroOpciones))
                    {
                        log += "No existe el fichero de opciones";
                        utilidad.SalirAplicacion(log);
                    }
                    else
                    {
                        //Procesa el fichero de opciones
                        Parametros.Configuracion.Inicializar(ficheroOpciones);

                        //Controla si las opciones pasadas son correctas.
                        string controlTipo = Parametros.Configuracion.Parametros.tipo;
                        log += ControlOpciones(controlTipo);
                        if (!string.IsNullOrEmpty(log)) utilidad.SalirAplicacion(log);

                        try
                        {
                            //Controla si hace falta solicitar el certificado por pantalla
                            ControlCertificado();

                            EjecutaProceso();
                        }

                        catch (ArgumentException ex)
                        {
                            log += $"Se ha producido un error al procesar la peticion. {ex.Message}";
                            utilidad.SalirAplicacion(log);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string mensaje = $"Error en el proceso {ex.Message}";
                File.WriteAllText(ficheroErrores, mensaje);
            }
        }

        private static void EjecutaProceso()
        {
            var parametros = Parametros.Configuracion.Parametros;
            switch (parametros.tipo)
            {
                case "1":
                    //Envio de modelos. Necesita certificado 
                    presentacionDirecta envioDirecto = new presentacionDirecta();
                    envioDirecto.envioPeticion(parametros.ficheroEntrada, parametros.ficheroSalida, parametros.serieCertificado, instanciaCertificado);
                    break;

                case "2":
                    //Validacion de modelos. No necesita certificado
                    validarModelos valida = new validarModelos();
                    valida.envioPeticion(parametros.ficheroEntrada, parametros.ficheroSalida);
                    break;

                case "3":
                    //Consulta de modelos presentados. Necesita certificado
                    descargaModelos descarga = new descargaModelos();
                    descarga.obtenerModelos(parametros.ficheroEntrada, parametros.ficheroSalida, parametros.serieCertificado, instanciaCertificado);
                    break;

                case "4":
                    //Ratificacion de domicilio. Necesita certificado
                    ratificarDomicilio ratifica = new ratificarDomicilio();

                    //Se procesa para el titular primero
                    ratifica.envioPeticion(parametros.serieCertificado, parametros.ficheroEntrada, parametros.ficheroSalida, 1, instanciaCertificado);

                    //Si se ha pasado el nif del conyuge se procesa de nuevo
                    if (ratifica.nifConyuge)
                    {
                        ratifica.envioPeticion(parametros.serieCertificado, parametros.ficheroEntrada, parametros.ficheroSalida, 2, instanciaCertificado);
                    }
                    break;

                case "5":
                    //Descarga datos fiscales renta. No necesita certificado
                    //Se puede hacer con certificado, pero esta preparado para hacerlo con la referencia de renta.
                    descargaDatosFiscales descargaDF = new descargaDatosFiscales();
                    descargaDF.descargaDF(parametros.urlDescargaDf, parametros.nifDf, parametros.refRenta, parametros.datosPersonales, parametros.ficheroSalida);
                    break;

                case "6":
                    //Presentacion facturas SII. Necesita certificado
                    //El fichero 'sii_urls.txt' debe estar en la misma ruta que el fichero de entrada
                    string ficheroUrls = Path.Combine(Path.GetDirectoryName(parametros.ficheroEntrada), "sii_urls.txt");

                    //Controla que exista el fichero de urls
                    if (!File.Exists(ficheroUrls))
                    {
                        log += "El fichero de urls no exite en la ruta de ejecucion";
                        utilidad.SalirAplicacion(log);
                    }

                    EnvioSii nuevoEnvio = new EnvioSii(ficheroUrls);//Instanciacion de la clase que carga las urls
                    nuevoEnvio.envioFacturas(parametros.ficheroEntrada, parametros.ficheroSalida, parametros.serieCertificado, instanciaCertificado, parametros.indiceUrl);
                    break;
            }
        }

        private static string ControlOpciones(string tipo)
        {
            var parametros = Parametros.Configuracion.Parametros;
            string mensajeControl = string.Empty;
            StringBuilder mensaje = new StringBuilder();
            switch (tipo)
            {
                case "1":
                    tituloVentana = "Envio modelos AEAT";
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);

                    break;

                case "2":
                    tituloVentana = "Validacion de modelos";
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);

                    break;

                case "3":
                    tituloVentana = "Consulta modelos presentados AEAT";
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);

                    break;

                case "4":
                    tituloVentana = "Ratificar domicilio renta";
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);

                    break;

                case "5":
                    tituloVentana = "Descarga datos fiscales renta";
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);
                    mensajeControl = Parametros.ControlDatosParametros("nifDf");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);
                    mensajeControl = Parametros.ControlDatosParametros("refRenta");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);
                    mensajeControl = Parametros.ControlDatosParametros("urlDescargaDf");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);

                    break;

                case "6":
                    tituloVentana = "Envio facturas al SII";
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);
                    mensajeControl = Parametros.ControlDatosParametros("indiceUrl");
                    if (!string.IsNullOrEmpty(mensajeControl)) mensaje.AppendLine(mensajeControl);

                    break;

                default:
                    mensaje.AppendLine("Opcion de ejecucion invalida");
                    break;
            }

            //Borrado de ficheros de salida y errores si existen de una ejecucion anterior.
            utilidad.borrarFicheros(parametros.ficheroSalida);
            utilidad.borrarFicheros(parametros.ficheroErrores);

            return mensaje.ToString();
        }

        private static string seleccionCertificados()
        {
            //Muestra el formulario de seleccion de certificados
            string serieCertificado;
            instanciaCertificado.cargarCertificados();

            frmSeleccion frmSeleccion = new frmSeleccion(instanciaCertificado);
            frmSeleccion.ShowDialog();

            serieCertificado = frmSeleccion.certificadoSeleccionado.serieCertificado;
            return serieCertificado;
        }

        private static void ControlCertificado()
        {
            var parametros = Parametros.Configuracion.Parametros;
            string mensaje = string.Empty;
            //Metodo para controlar si no se ha seleccionado un certificado y solicitarlo por pantalla
            if (parametros.conCertificado)
            {
                //Si se ha pasado un texto de busqueda, localiza el numero de serie del certificado por ese texto.
                if (!string.IsNullOrEmpty(parametros.textoBusqueda))
                {
                    parametros.serieCertificado = instanciaCertificado.buscarCertificado(parametros.textoBusqueda);
                }
                else
                {
                    //Si se ha pasado un fichero con el certificado
                    if (!string.IsNullOrEmpty(parametros.ficheroCertificado))
                    {
                        //Controla si existe el fichero y se ha pasado la contraseña
                        if (!File.Exists(parametros.ficheroCertificado))
                        {
                            mensaje = $"El fichero del certificado {parametros.ficheroCertificado} no existe";
                            utilidad.SalirAplicacion(mensaje);
                        }
                        if (string.IsNullOrEmpty(parametros.passwordCertificado))
                        {
                            mensaje = "No se ha pasado la contraseña del certificado";
                            utilidad.SalirAplicacion(mensaje);
                        }

                        string resultadoLectura = instanciaCertificado.leerCertificado(parametros.ficheroCertificado, parametros.passwordCertificado);
                        if (!string.IsNullOrEmpty(resultadoLectura))
                        {
                            mensaje = $"Error al leer el certificado. {resultadoLectura}";
                            utilidad.SalirAplicacion(mensaje);
                        }
                        var certificadosInfo = instanciaCertificado.relacionCertificados();
                        parametros.serieCertificado = certificadosInfo.LastOrDefault()?.serieCertificado;
                    }
                }

                //Si no se ha podido leer el certificado se solicita por pantalla
                if (string.IsNullOrEmpty(parametros.serieCertificado)) parametros.serieCertificado = seleccionCertificados();
            }
        }

        private static Assembly CargarDlls(object sender, ResolveEventArgs argumentos)
        {
            // Obtiene la carpeta donde están las DLLs
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dse_dlls");

            // Construye el nombre del archivo de la librería que se está buscando
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(argumentos.Name).Name + ".dll");

            if (File.Exists(assemblyPath))
            {
                // Carga el ensamblado desde la ruta especificada
                return Assembly.LoadFrom(assemblyPath);
            }

            // Si no encuentra el ensamblado, retorna null
            return null;
        }
    }
}
