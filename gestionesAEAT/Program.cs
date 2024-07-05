using gestionesAEAT.Formularios;
using gestionesAEAT.Metodos;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace gestionesAEAT
{
    internal class Program
    {
        //Instanciacion de las clases a nivel de clase para hacerlas accesibles a toda la clase
        static Utiles utilidad = new Utiles();
        static gestionCertificados instanciaCertificado = new gestionCertificados();
        static string log = string.Empty; //Sirve para grabar un log de los posibles errores que se hayan producido en el proceso.

        //Permite controlar si la aplicacion se ejecuta por consola
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        private const int ATTACH_PARENT_PROCESS = -1;

        static string pathFicheros = string.Empty;
        static string ficheroErrores = string.Empty;

        static void Main(string[] args)
        {
            //Variables para almacenar los argumentos pasados
            string dsclave = string.Empty; //Unicamente servira como medida de seguridad de ejecucion y debe pasarse 'ds123456'
            string tipo = string.Empty;
            string ficheroEntrada = string.Empty;
            string ficheroSalida = "salida.txt";
            string serieCertificado = string.Empty;
            string ficheroCertificado = string.Empty;
            string passwordCertificado = string.Empty;
            bool conCertificado = false;
            string nifDf = string.Empty;
            string refRenta = string.Empty;
            string datosPersonales = "S";
            string urlDescargaDf = string.Empty;
            string respuestaAeat = string.Empty;

            //Se usa el path en varias partes del programa, y si se esta en modo de pruebas se cambia

#if DEBUG
            {
                pathFicheros = @"C:\Programacion\c#\gestionesAEAT\pruebas"; //Path por defecto para almacenar los ficheros (dejar en blanco para la version de produccion)
            }
#endif

            string[] argumentos = Environment.GetCommandLineArgs(); //Almacena en un array los argumentos introducidos.

            if (argumentos.Length >= 4)
            {
                //Control para que si la clave no es correcta no se ejecute el programa
                dsclave = argumentos[1];
                if (dsclave != "ds123456")
                {
                    log += "Clave de ejecucion incorrecta";
                    salirAplicacion();
                }
                tipo = argumentos[2];
                if (argumentos.Length == 4) //La obtencion de los certificados instalados solo tiene 4 parametros
                {
                    if (tipo != "6")
                    {
                        log += "Parametros incorrectos. Con 4 parametros el tipo debe ser 6.";
                        salirAplicacion();
                    }
                    ficheroSalida = Path.Combine(pathFicheros, argumentos[3]);
                    utilidad.borrarFicheros(ficheroSalida);
                }
                else
                {
                    if (argumentos.Length >= 6) //Tiene que haber por lo menos 5 argumentos
                    {
                        if (tipo != "5") //La descarga de datos fiscales tiene otros parametros
                        {
                            ficheroEntrada = Path.Combine(pathFicheros, argumentos[3]);
                            if (!File.Exists(ficheroEntrada))
                            {
                                log += $"El fichero de entrada {ficheroEntrada} no existe";
                                salirAplicacion();
                            }

                            ficheroErrores = Path.Combine(Path.GetDirectoryName(ficheroEntrada), "errores.txt");
                            utilidad.borrarFicheros(ficheroErrores);

                            ficheroSalida = Path.Combine(pathFicheros, argumentos[4]);
                            utilidad.borrarFicheros(ficheroSalida);

                            if (argumentos[5].ToUpper() == "SI") conCertificado = true;
                            if (conCertificado)
                            {
                                if (argumentos.Length == 6)
                                {
                                    //No se ha pasado ni el numero de serie ni el fichero, por lo que hay que cargar el formulario de seleccion de certificados.
                                    serieCertificado = seleccionCertificados();
                                }
                                else if (argumentos.Length > 7)
                                {
                                    //Se pasa el fichero del certificado y el pass
                                    ficheroCertificado = Path.Combine(pathFicheros, argumentos[6]);
                                    if (!File.Exists(ficheroCertificado))
                                    {
                                        log += $"El fichero del certificado {ficheroCertificado} no existe";
                                        salirAplicacion();
                                    }
                                    passwordCertificado = argumentos[7];

                                    string resultadoLectura = instanciaCertificado.leerCertificado(ficheroCertificado, passwordCertificado);
                                    if (!string.IsNullOrEmpty(resultadoLectura))
                                    {
                                        log += $"Error al leer el certificado. {resultadoLectura}";
                                        salirAplicacion();
                                    }
                                    var certificadosInfo = instanciaCertificado.listaCertificados();
                                    serieCertificado = certificadosInfo.LastOrDefault()?.serieCertificado;
                                }
                                else if (argumentos.Length == 7) //Se pasa el numero de serie del certificado
                                {
                                    serieCertificado = argumentos[6].ToUpper();
                                }
                            }
                        }
                        else if (tipo == "5")
                        {
                            ficheroSalida = Path.Combine(pathFicheros, argumentos[3]);
                            utilidad.borrarFicheros(ficheroSalida);
                            ficheroErrores = Path.Combine(Path.GetDirectoryName(ficheroSalida), "errores.txt");
                            utilidad.borrarFicheros(ficheroErrores);

                            nifDf = argumentos[4].ToUpper();
                            refRenta = argumentos[5].ToUpper();
                            datosPersonales = argumentos[6].ToUpper();
                            if (datosPersonales != "S" && datosPersonales != "N") datosPersonales = "S"; //Se fuerza una 'S' si viene otra cosa como parametro
                            urlDescargaDf = argumentos[7];
                        }
                    }
                }
            }
            //Muestra la ayuda del programa si se pasa como parametro '-h' o '?'
            else if (argumentos.Length > 1 && (argumentos[1] == "-h" || argumentos[1] == "?"))
            {
                salirAplicacion();
            }
            else
            {
                log += "Numero de parametros incorrectos";
                salirAplicacion();
            }

            //Procesado de los datos segun el tipo pasado como argumento
            switch (tipo)
            {
                case "1":
                    //Envio de modelos. Necesita certificado 

                    //Ejemplo de ejecucion (se solicita el certificado en pantalla)
                    //gestionesAEAT.exe ds123456 1 empresa_guion.txt empresa_salida.txt SI

                    //Ejemplo de ejecucion (se pasa el numero de serie del certificado del almacen)
                    //gestionesAEAT.exe ds123456 1 empresa_guion.txt empresa_salida.txt SI numeroserie

                    //Ejemplo de ejecucion (se pasa fichero y password del certificado)
                    //gestionesAEAT.exe ds123456 1 empresa_guion.txt empresa_salida.txt SI certificado.pdf password

                    //Como es necesario el certificado, se controla si se ha pasado como parametro
                    controlCertificado(ref serieCertificado);

                    enviarModelos envio = new enviarModelos();
                    envio.envioPeticion(ficheroEntrada, ficheroSalida, serieCertificado, instanciaCertificado);

                    break;


                case "2":
                    //Validacion de modelos. No necesita certificado

                    //Ejemplo de ejecucion
                    //gestionesAEAT.exe ds123456 2 empresa_guion.txt empresa_salida.txt NO

                    validarModelos valida = new validarModelos();
                    valida.envioPeticion(ficheroEntrada, ficheroSalida);

                    break;

                case "3":
                    //Consulta de modelos presentados. Necesita certificado

                    //Ejemplo de ejecucion (solicita certificado en pantalla)
                    //gestionesAEAT.exe ds123456 3 empresa_guion.txt empresa_salida.txt SI

                    //Ejemplo de ejecucion (se pasa el numero de serie del certificado del almacen)
                    //gestionesAEAT.exe ds123456 3 empresa_guion.txt empresa_salida.txt SI numeroserie

                    //Ejemplo de ejecucion (se pasa fichero y password del certificado)
                    //gestionesAEAT.exe ds123456 3 empresa_guion.txt empresa_salida.txt SI certificado.pdf password

                    //Como es necesario el certificado, se controla si se ha pasado como parametro
                    controlCertificado(ref serieCertificado);

                    descargaModelos descarga = new descargaModelos();
                    descarga.obtenerModelos(ficheroEntrada, ficheroSalida, serieCertificado, instanciaCertificado);

                    break;

                case "4":
                    //Ratificacion de domicilio. Necesita certificado

                    //Ejemplo de ejecucion (se solicita el certificado en pantalla)
                    //gestionesAEAT.exe ds123456 4 empresa_guion.txt empresa_salida.txt SI

                    //Ejemplo de ejecucion (se pasa el numero de serie del certificado del almacen)
                    //gestionesAEAT.exe ds123456 4 empresa_guion.txt empresa_salida.txt SI numeroserie

                    //Ejemplo de ejecucion (se pasa fichero y password del certificado)
                    //gestionesAEAT.exe ds123456 4 empresa_guion.txt empresa_salida.txt SI certificado.pdf password

                    //Como es necesario el certificado, se controla si se ha pasado como parametro
                    controlCertificado(ref serieCertificado);

                    ratificarDomicilio ratifica = new ratificarDomicilio();

                    //Se procesa dos veces para el titular y conyuge
                    ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 1, instanciaCertificado);
                    ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 2, instanciaCertificado);
                    break;

                case "5":
                    //Descarga datos fiscales renta. No necesita certificado
                    //Se puede hacer con certificado, pero esta preparado para hacerlo con la referencia de renta.

                    //Ejemplo de ejecucion (renta 2023) 
                    // gestionesAEAT.exe ds123456 5 05197043D KEKTXP S fichero.txt https://www9.agenciatributaria.gob.es/wlpl/DFPA-D182/SvDesDF23Pei

                    descargaDatosFiscales descargaDF = new descargaDatosFiscales();
                    descargaDF.descargaDF(urlDescargaDf, nifDf, refRenta, datosPersonales, ficheroSalida);
                    break;

                case "6":
                    //Obtener datos certificados instalados. No necesita certificado

                    //Ejemplo de ejecucion
                    //gestionesAEAT.exe ds123456 6 certificados_salida.txt

                    instanciaCertificado.exportarDatosCertificados(ficheroSalida);
                    break;
            }
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

        private static void controlCertificado(ref string serieCertificado)
        {
            //Metodo para controlar si no se ha seleccionado un certificado y solicitarlo por pantalla (la variable se pasa por referencia para que si hay que modificarla se grabe en la clase
            X509Certificate2 certificado;
            if (string.IsNullOrEmpty(serieCertificado))
            {
                //Si no se ha grabado la serie del certificado, se vuelve a mostrar la pantalla de seleccion de certificados
                serieCertificado = seleccionCertificados();
            }
            certificado = instanciaCertificado.buscarSerieCertificado(serieCertificado);
            if (certificado != null)
            {
                DateTime caducidad = Convert.ToDateTime(certificado.GetExpirationDateString());
                if (caducidad < DateTime.Now)
                {
                    log += $"El certificado de {certificado.SubjectName.Name} esta caducado. Fecha de caducidad: {certificado.GetExpirationDateString()}";
                    salirAplicacion();
                }
            }
            else
            {
                log += "Certificado no encontrado en el almacen";
                salirAplicacion();
            }
        }

        private static void salirAplicacion()
        {
            //Controla si se esta ejecutando la aplicacion desde la consola para poder mostrar un mensaje de uso
            if (Environment.UserInteractive)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                mostrarAyuda();
            }

            //Si hay algun texto de error en el log, lo graba en un fichero
            if (!string.IsNullOrEmpty(log))
            {
                string salida = Path.Combine(pathFicheros, ficheroErrores);
                File.WriteAllText(salida, log);
            }
            Environment.Exit(0);
        }

        public static void mostrarAyuda()
        {
            StringBuilder mensaje = new StringBuilder();
            mensaje.AppendLine("");
            mensaje.AppendLine("Uso de la aplicacion: gestionesAEAT parametros");
            mensaje.AppendLine("Parametros:");
            mensaje.AppendLine("\tdsclave\t\t\tclave de ejecucion del programa (obligatorio)");
            mensaje.AppendLine("\ttipo\t\t\t1 = Envio de modelos");
            mensaje.AppendLine("\t    \t\t\t2 = Validar modelos (no necesita certificado)");
            mensaje.AppendLine("\t    \t\t\t3 = Consulta y descarga PDF de modelos presentados");
            mensaje.AppendLine("\t    \t\t\t4 = Ratificacion domicilio renta");
            mensaje.AppendLine("\t    \t\t\t5 = Descarga datos fiscales renta");
            mensaje.AppendLine("\t    \t\t\t6 = Obtener datos de certificados instalados en el equipo");
            mensaje.AppendLine("\tentrada.txt\t\tNombre del fichero con los datos a enviar");
            mensaje.AppendLine("\tsalida.txt\t\tNombre del fichero donde se grabara la salida");
            mensaje.AppendLine("\t(SI | NO)\t\tIndica si el proceso necesita certificado(una de las dos opciones)");
            mensaje.AppendLine("\tnumeroserie\t\tNumero de serie del certificado de los instalados en la maquina a utilizar en el proceso");
            mensaje.AppendLine("\tcertificado\t\tNombre del fichero.pfx que contiene el certificado digital");
            mensaje.AppendLine("\tpassword\t\tContraseña del certificado que se pasa por fichero");
            mensaje.AppendLine("\tNIF\t\t\tPara la descarga de datos fiscales es necesario el NIF del contribuyente");
            mensaje.AppendLine("\trefRenta\t\tCodigo de 5 caracteres de la referencia de renta para la descarga de datos fiscales");
            mensaje.AppendLine("\tdatosPersonales\t\tEn la descarga de datos fiscales indica si se quieren tambien los datos personales");
            mensaje.AppendLine("\turlDescarga\t\tDireccion a la que hacer la peticion de descarga de datos fiscales (cambia cada año");
            mensaje.AppendLine("\nEjemplos de uso:");
            mensaje.AppendLine("\tEnvio modelos:\t\t\tgestionesAEAT dsclave 1 entrada.txt salida.txt SI (numeroserie | (certificado password)");
            mensaje.AppendLine("\tValidar modelos:\t\tgestionesAEAT dsclave 2 entrada.txt salida.txt NO");
            mensaje.AppendLine("\tConsulta modelos:\t\tgestionesAEAT dsclave 3 entrada.txt salida.txt SI (numeroserie | (certificado password)");
            mensaje.AppendLine("\tRatificar domicilio renta:\tgestionesAEAT dsclave 4 entrada.txt salida.txt SI (numeroserie | (certificado password)");
            mensaje.AppendLine("\tDescarga datos fiscales:\tgestionesAEAT dsclave 5 salida.txt NIF refRenta datosPersonales urlDescarga");
            mensaje.AppendLine("\tRelacion certificados:\t\tgestionesAEAT dsclave tipo salida");
            mensaje.AppendLine("\nNotas:");
            mensaje.AppendLine("\t- Si no se pasan los datos del certificado y el proceso lo requerire, se mostrara el formulario de seleccion");
            mensaje.AppendLine("\t- Con la validacion de modelos (tipo 2) el parametro 5 debe ser un NO");
            mensaje.AppendLine("\nPulse una tecla para continuar");

            Console.WriteLine(mensaje.ToString());
            Console.ReadLine();
        }
    }
}
