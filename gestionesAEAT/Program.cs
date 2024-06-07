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

            string respuestaAeat = string.Empty;
            X509Certificate2 certificado = null; //Certificado que se utilizara para el envio

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
                if (argumentos.Length == 4)
                {
                    if (tipo != "2")
                    {
                        log += "Parametros incorrectos. Con 4 parametros el tipo debe ser 2.";
                        salirAplicacion();
                    }
                    ficheroSalida = argumentos[3];
                }
                else
                {
                    if (argumentos.Length >= 6) //Tiene que haber por lo menos 5 argumentos (clave, tipo, entrada, salida y certificado)
                    {
                        ficheroEntrada = Path.Combine(pathFicheros, argumentos[3]);
                        if (!File.Exists(ficheroEntrada))
                        {
                            log += $"El fichero de entrada {ficheroEntrada} no existe";
                            salirAplicacion();
                        }
                        ficheroSalida = Path.Combine(pathFicheros, argumentos[4]);
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
                            else //Se pasa el numero de serie del certificado
                            {
                                serieCertificado = argumentos[6].ToUpper();
                            }
                        }
                    }
                }
            }
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
                    //Envio de modelos; necesita certificado (parametro 5 = SI) por lo que debe venir el nº de serie para leerlo del almacen de certificados o con fichero y pass
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
                        else
                        {
                            //Desarrollar la parte del envio del modelo, en el que habra que pasar el guion y el certificado (ver como se ha hecho en ratificarDomicilio)
                        }
                    }
                    else
                    {
                        log += "Certificado no encontrado en el almacen";
                        salirAplicacion();
                    }
                    break;

                case "2":
                    //Obtener datos certificados instalados
                    instanciaCertificado.exportarDatosCertificados(ficheroSalida);
                    break;

                case "3":
                    //Validacion de modelos. No necesita certificado
                    validarModelos valida = new validarModelos();
                    valida.envioPeticion(ficheroEntrada, ficheroSalida);
                    break;

                case "4":
                    //Ratificacion de domicilio. Necesita certificado
                    ratificarDomicilio ratifica = new ratificarDomicilio();
                    //serieCertificado = "726e0db7a17efa04603b7f010ba43fa6".ToUpper();//Certificado de prueba mio
                    ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 1, instanciaCertificado);
                    ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 2, instanciaCertificado);
                    break;

                case "5":
                    //Consulta de modelos presentados. Hay que formar el html y mandarlo como POST y la respuesta sera un XML con la relacion de modelos presentados con sus metadatos. Crear un metodo para montar la peticion, con la respuesta pasarla al metodo 'descargaModelos.obtenerModelos' que devolvera un string que es el que hay que grabar en un fichero.
                    descargaModelos proceso = new descargaModelos();

                    proceso.obtenerModelos(ficheroEntrada, ficheroSalida, serieCertificado, instanciaCertificado);

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
                string salida = Path.Combine(pathFicheros, "errores.log");
                File.WriteAllText(salida, log);
            }
            Environment.Exit(0);
        }

        public static void mostrarAyuda()
        {
            StringBuilder mensaje = new StringBuilder();
            mensaje.AppendLine("");
            mensaje.AppendLine("Uso de la aplicacion");
            mensaje.AppendLine("\tgestionesAEAT dsclave tipo entrada salida (SI | NO) (numeroserie | certificado) password");
            mensaje.AppendLine("Parametros:");
            mensaje.AppendLine("\tdsclave\t\tclave de ejecucion del programa");
            mensaje.AppendLine("\ttipo\t\t1 = Envio de modelos");
            mensaje.AppendLine("\t    \t\t2 = Obtener datos de certificados en el equipo");
            mensaje.AppendLine("\t    \t\t3 = Validacion de modelos (no necesita certificado)");
            mensaje.AppendLine("\t    \t\t4 = Ratificacion domicilio renta");
            mensaje.AppendLine("\t    \t\t5 = Consulta y descarga PDF de modelos presentados");
            mensaje.AppendLine("\tentrada\t\tNombre del fichero con los datos a enviar (guion)");
            mensaje.AppendLine("\tsalida\t\tNombre del fichero donde se grabara la salida");
            mensaje.AppendLine("\t(SI | NO)\tIndica si el proceso necesita certificado o no");
            mensaje.AppendLine("\tnumeroserie\tNumero de serie del certificado a utilizar de los que hay en el almacen de certificados");
            mensaje.AppendLine("\tcertificado\tNombre del fichero.pfx que contiene el certificado digital");
            mensaje.AppendLine("\t           \tSi no se pasa numero de serie se debe pasar el fichero con el certificado");
            mensaje.AppendLine("\tpassword\tContraseña del certificado si se ha pasado en un fichero");
            mensaje.AppendLine("");
            mensaje.AppendLine("Notas:");
            mensaje.AppendLine("  - Si el quinto parametro es SI y no se pasa numero de serie o certificado, se mostrara el formulario de seleccion");
            mensaje.AppendLine("    de certificados");
            mensaje.AppendLine("  - Con el tipo 3 (validacion de modelos) el parametro 5 debe ser un NO");
            mensaje.AppendLine("    ejemplo:\tgestionesAEAT dsclave 3 entrada.txt salida.txt NO");
            mensaje.AppendLine("  - Con el tipo 5, el tercer parametro siempre sera el fichero de salida (solo se pasan 3 parametros");
            mensaje.AppendLine("    ejemplo:\tgestionesAEAT dsclave 5 fichero.txt");
            mensaje.AppendLine("");
            mensaje.AppendLine("Pulse una tecla para continuar");

            Console.WriteLine(mensaje.ToString());
            Console.ReadLine();
        }
    }
}
