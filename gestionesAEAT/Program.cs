using gestionesAEAT.Formularios;
using gestionesAEAT.Metodos;
using gestionesAEAT.Utilidades;
using System;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Text;

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

        public static string ficheroErrores = "errores.txt";


        [STAThread] //Atributo necesario para que la aplicacion pueda abrir el formulario de carga de certificado
        static void Main(string[] argumentos)
        {
            try
            {   
                if (argumentos.Length < 2)
                {
                    if (argumentos.Length > 1 && (argumentos[1] == "-h" || argumentos[1] == "?"))
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


                        //CargarOpciones();

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
                    //Obtener datos certificados instalados. No necesita certificado
                    instanciaCertificado.exportarDatosCertificados(parametros.ficheroSalida);
                    break;

                case "7":
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

                case "8":
                    //Grabar datos de certificado desde fichero (para certbase)

                    // Inicializa la aplicación de Windows Forms
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    using (frmCarga frmCarga = new frmCarga())
                    {
                        frmCarga.ShowDialog();
                    }

                    break;
            }
        }

        private static string ControlOpciones(string tipo)
        {
            var parametros = Parametros.Configuracion.Parametros;
            StringBuilder mensaje = new StringBuilder();
            switch (tipo)
            {
                case "1":
                case "2":
                case "3":
                case "4":
                    if (string.IsNullOrEmpty(parametros.ficheroEntrada)) mensaje.AppendLine("No se ha pasado el fichero de entrada");
                    if (string.IsNullOrEmpty(parametros.ficheroSalida)) mensaje.AppendLine("No se ha pasado el fichero de salida");
                    break;

                case "5":
                    if (string.IsNullOrEmpty(parametros.ficheroSalida)) mensaje.AppendLine("No se ha pasado el fichero de salida");
                    if (string.IsNullOrEmpty(parametros.nifDf)) mensaje.AppendLine("No se ha pasado el NIF del contribuyente");
                    if (string.IsNullOrEmpty(parametros.refRenta)) mensaje.AppendLine("No se ha pasado la referencia de la renta");
                    if (string.IsNullOrEmpty(parametros.urlDescargaDf)) mensaje.AppendLine("No se ha pasado la url de descarga de datos fiscales");
                    break;

                case "6":
                case "8":
                    if (string.IsNullOrEmpty(parametros.ficheroSalida)) mensaje.AppendLine("No se ha pasado el fichero de salida");
                    break;

                case "7":
                    if (string.IsNullOrEmpty(parametros.ficheroEntrada)) mensaje.AppendLine("No se ha pasado el fichero de entrada");
                    if (string.IsNullOrEmpty(parametros.ficheroSalida)) mensaje.AppendLine("No se ha pasado el fichero de salida");
                    if (parametros.indiceUrl < 0) mensaje.AppendLine("No se ha pasado el indice de la url a la que enviar las facturas");
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
                    parametros.serieCertificado = instanciaCertificado.buscarSerieCertificado(parametros.textoBusqueda);
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
    }
}
