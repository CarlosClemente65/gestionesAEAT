using gestionesAEAT.Formularios;
using gestionesAEAT.Metodos;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace gestionesAEAT
{
    internal class Program
    {
        //Instanciacion de las clases a nivel de clase para hacerlas accesibles a toda la clase
        static Utiles utilidad = new Utiles();
        static GestionCertificados instanciaCertificado = new GestionCertificados();
        static string log = string.Empty; //Sirve para grabar un log de los posibles errores que se hayan producido en el proceso.

        //Declaracion de variables a nivel de clase para hacerlas accesibles al resto.
        static string dsclave = string.Empty; //Unicamente servira como medida de seguridad de ejecucion y debe pasarse 'ds123456'
        static string tipo = string.Empty;
        static string pathFicheros = string.Empty;
        static string ficheroOpciones = string.Empty;
        static string ficheroEntrada = string.Empty;
        static string ficheroSalida = string.Empty;
        static string textoBusqueda = string.Empty;
        static string serieCertificado = string.Empty;
        static string ficheroCertificado = string.Empty;
        static string passwordCertificado = string.Empty;
        static bool conCertificado = false;
        static string nifDf = string.Empty;
        static string refRenta = string.Empty;
        static string datosPersonales = "S";
        static string urlDescargaDf = string.Empty;
        static string respuestaAeat = string.Empty;
        static int indiceUrl = -1;

        public static string ficheroErrores = "errores.txt";

        static void Main(string[] argumentos)
        {
            try
            {
                //Console.WriteLine("Directorio actual: " + Directory.GetCurrentDirectory());
                //Console.WriteLine("Argumentos recibidos: ");
                //foreach (var arg in args)
                //{
                //    Console.WriteLine(arg);
                //}

                if (argumentos.Length < 2)
                {
                    if (argumentos.Length > 1 && (argumentos[1] == "-h" || argumentos[1] == "?"))
                    {
                        utilidad.SalirAplicacion(log);
                    }
                    else
                    {
                        log += $"Son necesarios al menos 2 parametros: dsclave y fichero de opciones\n{argumentos[0]}\n{argumentos[1]}";
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
                        CargarOpciones();

                        //Controla si las opciones pasadas son correctas.
                        log += ControlOpciones(tipo);
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
                string mensaje = $"Error en el proceso {ex.Message}\nRuta de acceso: {Directory.GetCurrentDirectory()}";
                File.WriteAllText("errorProceso.txt", mensaje);
            }
        }

        private static void EjecutaProceso()
        {
            switch (tipo)
            {
                case "1":
                    //Envio de modelos. Necesita certificado 
                    presentacionDirecta envioDirecto = new presentacionDirecta();
                    envioDirecto.envioPeticion(ficheroEntrada, ficheroSalida, serieCertificado, instanciaCertificado);
                    break;

                case "2":
                    //Validacion de modelos. No necesita certificado
                    validarModelos valida = new validarModelos();
                    valida.envioPeticion(ficheroEntrada, ficheroSalida);
                    break;

                case "3":
                    //Consulta de modelos presentados. Necesita certificado
                    descargaModelos descarga = new descargaModelos();
                    descarga.obtenerModelos(ficheroEntrada, ficheroSalida, serieCertificado, instanciaCertificado);
                    break;

                case "4":
                    //Ratificacion de domicilio. Necesita certificado
                    ratificarDomicilio ratifica = new ratificarDomicilio();

                    //Se procesa para el titular primero
                    ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 1, instanciaCertificado);

                    //Si se ha pasado el nif del conyuge se procesa de nuevo
                    if (ratifica.nifConyuge)
                    {
                        ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 2, instanciaCertificado);
                    }
                    break;

                case "5":
                    //Descarga datos fiscales renta. No necesita certificado
                    //Se puede hacer con certificado, pero esta preparado para hacerlo con la referencia de renta.
                    descargaDatosFiscales descargaDF = new descargaDatosFiscales();
                    descargaDF.descargaDF(urlDescargaDf, nifDf, refRenta, datosPersonales, ficheroSalida);
                    break;

                case "6":
                    //Obtener datos certificados instalados. No necesita certificado
                    instanciaCertificado.exportarDatosCertificados(ficheroSalida);
                    break;

                case "7":
                    //Presentacion facturas SII. Necesita certificado
                    //El fichero 'sii_urls.txt' debe estar en la misma ruta que el fichero de entrada
                    string ficheroUrls = Path.Combine(Path.GetDirectoryName(ficheroEntrada), "sii_urls.txt");

                    //Controla que exista el fichero de urls
                    if (!File.Exists(ficheroUrls))
                    {
                        log += "El fichero de urls no exite en la ruta de ejecucion";
                        utilidad.SalirAplicacion(log);
                    }

                    EnvioSii nuevoEnvio = new EnvioSii(ficheroUrls);//Instanciacion de la clase que carga las urls
                    nuevoEnvio.envioFacturas(ficheroEntrada, ficheroSalida, serieCertificado, instanciaCertificado, indiceUrl);
                    break;
            }
        }

        private static string ControlOpciones(string tipo)
        {
            StringBuilder mensaje = new StringBuilder();
            switch (tipo)
            {
                case "1":
                case "2":
                case "3":
                case "4":
                    if (string.IsNullOrEmpty(ficheroEntrada)) mensaje.AppendLine("No se ha pasado el fichero de entrada");
                    if (string.IsNullOrEmpty(ficheroSalida)) mensaje.AppendLine("No se ha pasado el fichero de salida");
                    break;

                case "5":
                    if (string.IsNullOrEmpty(ficheroSalida)) mensaje.AppendLine("No se ha pasado el fichero de salida");
                    if (string.IsNullOrEmpty(nifDf)) mensaje.AppendLine("No se ha pasado el NIF del contribuyente");
                    if (string.IsNullOrEmpty(refRenta)) mensaje.AppendLine("No se ha pasado la referencia de la renta");
                    if (string.IsNullOrEmpty(urlDescargaDf)) mensaje.AppendLine("No se ha pasado la url de descarga de datos fiscales");
                    break;

                case "6":
                    if (string.IsNullOrEmpty(ficheroSalida)) mensaje.AppendLine("No se ha pasado el fichero de salida");
                    break;

                case "7":
                    if (string.IsNullOrEmpty(ficheroEntrada)) mensaje.AppendLine("No se ha pasado el fichero de entrada");
                    if (string.IsNullOrEmpty(ficheroSalida)) mensaje.AppendLine("No se ha pasado el fichero de salida");
                    if (indiceUrl < 0) mensaje.AppendLine("No se ha pasado el indice de la url a la que enviar las facturas");
                    break;
            }

            //Borrado de ficheros de salida y errores si existen de una ejecucion anterior.
            utilidad.borrarFicheros(ficheroSalida);
            utilidad.borrarFicheros(ficheroErrores);

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
            string mensaje = string.Empty;
            //Metodo para controlar si no se ha seleccionado un certificado y solicitarlo por pantalla
            if (conCertificado)
            {
                //Si se ha pasado un texto de busqueda, localiza el numero de serie del certificado por ese texto.
                if (!string.IsNullOrEmpty(textoBusqueda))
                {
                    serieCertificado = instanciaCertificado.buscarSerieCertificado(textoBusqueda);
                }
                else
                {
                    //Si se ha pasado un fichero con el certificado
                    if (!string.IsNullOrEmpty(ficheroCertificado))
                    {
                        //Controla si existe el fichero y se ha pasado la contraseña
                        if (!File.Exists(ficheroCertificado))
                        {
                            mensaje = $"El fichero del certificado {ficheroCertificado} no existe";
                            utilidad.SalirAplicacion(mensaje);
                        }
                        if (string.IsNullOrEmpty(passwordCertificado))
                        {
                            mensaje = "No se ha pasado la contraseña del certificado";
                            utilidad.SalirAplicacion(mensaje);
                        }

                        string resultadoLectura = instanciaCertificado.leerCertificado(ficheroCertificado, passwordCertificado);
                        if (!string.IsNullOrEmpty(resultadoLectura))
                        {
                            mensaje = $"Error al leer el certificado. {resultadoLectura}";
                            utilidad.SalirAplicacion(mensaje);
                        }
                        var certificadosInfo = instanciaCertificado.relacionCertificados();
                        serieCertificado = certificadosInfo.LastOrDefault()?.serieCertificado;
                    }
                }

                //Si no se ha podido leer el certificado se solicita por pantalla
                if (string.IsNullOrEmpty(serieCertificado)) serieCertificado = seleccionCertificados();
            }
        }

        private static void CargarOpciones()
        {
            //Metodo para procesar el fichero de opciones
            string[] lineas = File.ReadAllLines(ficheroOpciones);
            foreach (string linea in lineas)
            {
                //Evita procesar lineas vacias
                if (string.IsNullOrWhiteSpace(linea)) continue;

                //Divide la linea en clave=valor
                string[] partes = linea.Split('=');
                string clave = partes[0].Trim();
                string valor = partes[1].Trim();

                switch (clave)
                {
                    case "CLIENTE":
                        utilidad.cliente = valor;
                        break;

                    case "TIPO":
                        tipo = valor;
                        break;

                    case "ENTRADA":
                        //Se controla si se pasa el fichero de entrada para evitar una excepcion al asignarlo a la variable
                        if (!string.IsNullOrEmpty(valor))
                        {
                            ficheroEntrada = valor;
                        }
                        break;

                    case "SALIDA":
                        //Se controla si se pasa el fichero de salida para evitar una excepcion al asignarlo a la variable
                        if (!string.IsNullOrEmpty(valor))
                        {
                            //Como el fichero de salida siempre tiene que estar presente, se carga la ruta de los ficheros
                            pathFicheros = Path.GetDirectoryName(valor);
                            ficheroErrores = Path.Combine(pathFicheros, "errores.txt");
                            ficheroSalida = valor;
                        }
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
                        //Se controla si se pasa el fichero del certificado para evitar una excepcion al asignarlo a la variable
                        if (!string.IsNullOrEmpty(valor))
                        {
                            ficheroCertificado = valor;
                        }
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
    }
}
