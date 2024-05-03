using gestionesAEAT.Metodos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace gestionesAEAT
{
    internal class Program
    {
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
            string respuestaAeat = string.Empty;
            X509Certificate2 certificado = null; //Certificado que se utilizara para el envio
            string log = string.Empty; //Sirve para grabar un log de los posibles errores que se hayan producido en el proceso.


            string[] argumentos = Environment.GetCommandLineArgs(); //Almacena en un array los argumentos introducidos.
            Utiles utilidad = new Utiles();

            if (argumentos.Length >= 4) //Se pasa el numero de serie o el certificado con su pass
            {
                dsclave = argumentos[1];
                if (dsclave != "ds123456") Environment.Exit(0);
                tipo = argumentos[2];
                ficheroEntrada = argumentos[3];
                ficheroSalida = argumentos[4];

                //Nota: revisar esta parte porque no me cuadra el nº de argumentos
                if (argumentos.Length == 6) //Se pasa el numero de serie del certificado
                {
                    serieCertificado = argumentos[5].ToUpper();
                }

                if (argumentos.Length == 8)
                {
                    ficheroCertificado = argumentos[6];
                    passwordCertificado = argumentos[7];
                }

            }

            //Lanzar ejemplo de serializacion (el primer parametro lo paso vacio porque en el ejemplo ya hay un xml de prueba; quitar esa prueba en la version de produccion.
            //gestionXml gestion = new gestionXml(respuestaAeat, ficheroSalida);

            //Procesado de los datos segun el tipo pasado como argumento
            switch (tipo)
            {
                case "1":
                    //Envio de modelos con el almacen de certificados; es necesario obtener el indice
                    gestionCertificados proceso = new gestionCertificados();
                    proceso.cargarCertificados();

                    certificado = proceso.buscarSerieCertificado(serieCertificado);
                    if (certificado != null)
                    {
                        DateTime caducidad = Convert.ToDateTime(certificado.GetExpirationDateString());
                        if (caducidad < DateTime.Now)
                        {
                            log += $"El certificado de {certificado.SubjectName.Name} esta caducado. Fecha de caducidad: {certificado.GetExpirationDateString()}";
                        }
                        else
                        {

                        }

                    }
                    else
                    {
                        log += "Certificado no encontrado en el almacen";
                    }




                    break;

                case "2":
                    //Envio de modelos con fichero de certificado
                    break;

                case "3":
                    //Validacion de modelos
                    break;

                case "4":
                    //Ratificacion de domicilio
                    ratificarDomicilio ratifica = new ratificarDomicilio();
                    serieCertificado = "726e0db7a17efa04603b7f010ba43fa6".ToUpper();//Certificado de prueba mio
                    ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida);
                    break;

            }

        }

    }
}
