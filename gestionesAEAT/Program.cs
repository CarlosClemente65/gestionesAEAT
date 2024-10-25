using GestionCertificadosDigitales;
using gestionesAEAT.Formularios;
using gestionesAEAT.Metodos;
using gestionesAEAT.Utilidades;
using System;
using System.IO;
using System.Text;


namespace gestionesAEAT
{
    internal class Program
    {
        //Instanciacion de las clases a nivel de clase para hacerlas accesibles a toda la clase
        public static Utiles utilidad = new Utiles();
        public static GestionarCertificados gestionCertificados = new GestionarCertificados(); //Instanciacion de la clase que gestiona los certificados

        //Declaracion de variables a nivel de clase para hacerlas accesibles al resto.
        //Nota. Revisar la asingacion de variables porque pueden estar en parametros todas.
        static string log = string.Empty; //Sirve para grabar un log de los posibles errores que se hayan producido en el proceso.
        static string dsclave = string.Empty; //Unicamente servira como medida de seguridad de ejecucion y debe pasarse 'ds123456'


        [STAThread] //Atributo necesario para que la aplicacion pueda abrir el formulario de carga de certificado
        static void Main(string[] argumentos)
        {
            try
            {
                if (argumentos.Length < 2)
                {
                    if (argumentos.Length > 0 && (argumentos[0].ToUpper() == "-H" || argumentos[0] == "?"))
                    {
                        utilidad.MostrarAyuda();
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

                    Parametros.Configuracion.Inicializar();
                    Parametros.CargarOpciones(argumentos[1]);

                    if (!File.Exists(Parametros.ficheroOpciones))
                    {
                        log += "No existe el fichero de opciones";
                        utilidad.SalirAplicacion(log);
                    }
                    else
                    {
                        //Controla si las opciones pasadas son correctas.
                        string controlTipo = Parametros.tipo;
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
                utilidad.GrabarSalida(mensaje, Parametros.ficheroResultado);
                utilidad.grabadaSalida = true;
            }
        }

        private static void EjecutaProceso()
        {
            switch (Parametros.tipo)
            {
                case "1":
                    //Envio de modelos. Necesita certificado 
                    presentacionDirecta envioDirecto = new presentacionDirecta();
                    envioDirecto.envioPeticion();
                    break;

                case "2":
                    //Validacion de modelos. No necesita certificado
                    validarModelos valida = new validarModelos();
                    valida.envioPeticion();
                    break;

                case "3":
                    //Consulta de modelos presentados. Necesita certificado
                    descargaModelos descarga = new descargaModelos();
                    descarga.obtenerModelos();
                    break;

                case "4":
                    //Ratificacion de domicilio. Necesita certificado
                    ratificarDomicilio ratifica = new ratificarDomicilio();

                    int paso = 1;
                    //Se procesa para el titular primero
                    ratifica.envioPeticion(paso);

                    //Si se ha pasado el nif del conyuge se procesa de nuevo
                    if (ratifica.nifConyuge)
                    {
                        paso = 2;
                        ratifica.envioPeticion(paso);
                    }
                    break;

                case "5":
                    //Descarga datos fiscales renta. No necesita certificado
                    //Se puede hacer con certificado, pero esta preparado para hacerlo con la referencia de renta.
                    descargaDatosFiscales descargaDF = new descargaDatosFiscales();
                    descargaDF.descargaDF();
                    break;

                case "6":
                    //Presentacion facturas SII. Necesita certificado
                    EnvioSii nuevoEnvio = new EnvioSii();//Instanciacion de la clase 
                    nuevoEnvio.envioFacturas();
                    break;

                case "7":
                    presentacionInformativas nuevaPresentacion = new presentacionInformativas();
                    nuevaPresentacion.envioPeticion();
                    break;

                case "8":
                    PagoNRC nuevoPago = new PagoNRC();
                    nuevoPago.envioPeticion();
                    break;

                case "9":
                    descargaCSV descargaCSV = new descargaCSV();
                    descargaCSV.descargaDocumentoCSV().GetAwaiter().GetResult();
                    break;
            }
        }

        private static string ControlOpciones(string tipo)
        {
            string mensajeControl = string.Empty;
            StringBuilder mensaje = new StringBuilder();
            switch (tipo)
            {
                case "1":
                    frmSeleccion.tituloVentana = "Envio modelos AEAT";
                    if (Parametros.conCertificado == false) Parametros.conCertificado = true;
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }

                    break;

                case "2":
                    frmSeleccion.tituloVentana = "Validacion de modelos";
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }

                    break;

                case "3":
                    frmSeleccion.tituloVentana = "Consulta modelos presentados AEAT";
                    if (Parametros.conCertificado == false) Parametros.conCertificado = true;
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }

                    break;

                case "4":
                    frmSeleccion.tituloVentana = "Ratificar domicilio renta";
                    if (Parametros.conCertificado == false) Parametros.conCertificado = true;
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }

                    break;

                case "5":
                    frmSeleccion.tituloVentana = "Descarga datos fiscales renta";
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("nifDf");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("refRenta");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("urlDescargaDf");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }

                    break;

                case "6":
                    frmSeleccion.tituloVentana = "Envio facturas al SII";
                    if (Parametros.conCertificado == false) Parametros.conCertificado = true;
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("UrlSii");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }

                    break;

                case "7":
                    frmSeleccion.tituloVentana = "Presentacion declaraciones informativas";
                    if (Parametros.conCertificado == false) Parametros.conCertificado = true;
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("procesoInformativas");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    break;

                case "8":
                    frmSeleccion.tituloVentana = "Pago modelos mediante NRC con cargo en cuenta";
                    mensajeControl = Parametros.ControlDatosParametros("ficheroEntrada");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    break;

                case "9":
                    mensajeControl = Parametros.ControlDatosParametros("ficheroSalida");
                    if (!string.IsNullOrEmpty(mensajeControl))
                    {
                        mensaje.AppendLine(mensajeControl);
                        mensajeControl = string.Empty;
                    }
                    break;

                default:
                    mensaje.AppendLine("Opcion de ejecucion invalida");
                    break;
            }

            //Borrado de ficheros de salida y errores si existen de una ejecucion anterior.
            utilidad.borrarFicheros(Parametros.ficheroSalida);
            utilidad.borrarFicheros(Parametros.ficheroResultado);

            return mensaje.ToString();
        }

        private static void seleccionCertificados()
        {
            //Muestra el formulario de seleccion de certificados
            gestionCertificados.cargarCertificadosAlmacen();

            frmSeleccion frmSeleccion = new frmSeleccion();
            frmSeleccion.ShowDialog();
        }

        private static void ControlCertificado()
        {
            //Carga los certificados del almacen
            Program.gestionCertificados.cargarCertificadosAlmacen();
            string mensaje = string.Empty;
            //Metodo para controlar si no se ha seleccionado un certificado y solicitarlo por pantalla
            if (Parametros.conCertificado)
            {
                //Si se ha pasado un texto de busqueda, localiza el numero de serie del certificado por ese texto.
                if (!string.IsNullOrEmpty(Parametros.textoBusqueda))
                {
                    Parametros.serieCertificado = gestionCertificados.buscarCertificado(Parametros.textoBusqueda);
                }
                else
                {
                    //Si se ha pasado un fichero con el certificado
                    if (!string.IsNullOrEmpty(Parametros.ficheroCertificado))
                    {
                        //Controla si existe el fichero y se ha pasado la contraseña
                        if (!File.Exists(Parametros.ficheroCertificado))
                        {
                            mensaje = $"El fichero del certificado {Parametros.ficheroCertificado} no existe";
                            utilidad.SalirAplicacion(mensaje);
                        }
                        if (string.IsNullOrEmpty(Parametros.passwordCertificado))
                        {
                            mensaje = "No se ha pasado la contraseña del certificado";
                            utilidad.SalirAplicacion(mensaje);
                        }

                        (string resultadoLectura, bool resultado) = gestionCertificados.leerCertificado(Parametros.ficheroCertificado, Parametros.passwordCertificado);
                        if (resultadoLectura != "OK")
                        {
                            mensaje = $"Error al leer el certificado. {resultadoLectura}";
                            utilidad.SalirAplicacion(mensaje);
                        }

                        Parametros.serieCertificado = gestionCertificados.consultaPropiedades(GestionarCertificados.nombresPropiedades.serieCertificado);
                    }
                }

                //Si no se ha podido leer el certificado se solicita por pantalla
                if (string.IsNullOrEmpty(Parametros.serieCertificado))
                {
                    seleccionCertificados();
                }
            }
        }
    }
}
