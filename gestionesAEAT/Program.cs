using gestionesAEAT.Formularios;
using gestionesAEAT.Metodos;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace gestionesAEAT
{
    internal class Program
    {
        //Instanciacion de las clases a nivel de clase para hacerlas accesibles a toda la clase
        static Utiles utilidad = new Utiles();
        static gestionCertificados instanciaCertificado = new gestionCertificados();
        static string log = string.Empty; //Sirve para grabar un log de los posibles errores que se hayan producido en el proceso.


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


            string[] argumentos = Environment.GetCommandLineArgs(); //Almacena en un array los argumentos introducidos.

            //Control para que si la clave no es correcta no se ejecute el programa
            dsclave = argumentos[1];
            if (dsclave != "ds123456") Environment.Exit(0);

            if (argumentos.Length >= 6) //Tiene que haber por lo menos 5 argumentos (clave, tipo, entrada, salida y certificado)
            {
                tipo = argumentos[2];
                ficheroEntrada = argumentos[3];
                ficheroSalida = argumentos[4];
                if (argumentos[5] == "SI") conCertificado = true;
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
                        ficheroCertificado = argumentos[6];
                        if (!File.Exists(ficheroCertificado))
                        {
                            log += $"El fichero del certificado {ficheroCertificado} no existe";
                            salirAplicacion();
                        }
                        passwordCertificado = argumentos[7];
                        instanciaCertificado.leerCertificado(ficheroCertificado, passwordCertificado);
                        var certificadosInfo = instanciaCertificado.listaCertificados();
                        serieCertificado = certificadosInfo.LastOrDefault()?.serieCertificado;
                    }
                    else //Se pasa el numero de serie del certificado
                    {
                        serieCertificado = argumentos[6].ToUpper();
                    }
                }
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
                    //No desarrollado
                    break;

                case "3":
                    //Validacion de modelos. No necesita certificado
                    break;

                case "4":
                    //Ratificacion de domicilio. Necesita certificado
                    ratificarDomicilio ratifica = new ratificarDomicilio();
                    //serieCertificado = "726e0db7a17efa04603b7f010ba43fa6".ToUpper();//Certificado de prueba mio
                    ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 1, instanciaCertificado);
                    ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 2, instanciaCertificado);
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
            File.WriteAllText("errores.log", log);
            Environment.Exit(0);
        }
    }
}
