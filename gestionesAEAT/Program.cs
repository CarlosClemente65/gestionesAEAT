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

        //Declaracion de variables a nivel de clase para hacerlas accesibles al resto.
        static string dsclave = string.Empty; //Unicamente servira como medida de seguridad de ejecucion y debe pasarse 'ds123456'
        static string ficheroEntrada = string.Empty;
        static string tipo = string.Empty;
        static string ficheroSalida = "salida.txt";
        static string serieCertificado = string.Empty;
        static string ficheroCertificado = string.Empty;
        static string passwordCertificado = string.Empty;
        static bool conCertificado = false;
        static string nifDf = string.Empty;
        static string refRenta = string.Empty;
        static string datosPersonales = "S";
        static string urlDescargaDf = string.Empty;
        static string respuestaAeat = string.Empty;

        static string pathFicheros = string.Empty;
        static string ficheroErrores = "errores.txt";

        static string[] argumentos = null;

        static void Main(string[] args)
        {
            //Se usa el path en varias partes del programa, y si se esta en modo de pruebas se cambia

#if DEBUG
            {
                pathFicheros = @"C:\Programacion\c#\gestionesAEAT\pruebas"; //Path por defecto para almacenar los ficheros (dejar en blanco para la version de produccion)
            }
#endif

            argumentos = Environment.GetCommandLineArgs(); //Almacena en un array los argumentos introducidos.

            if (argumentos.Length < 2)
            {
                if (argumentos.Length > 1 && (argumentos[1] == "-h" || argumentos[1] == "?"))
                {
                    utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                }
                else
                {
                    log += "Son necesarios al menos 2 parametros: dsclave y tipo";
                    utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                }
            }

            dsclave = argumentos[1];
            if (dsclave != "ds123456")
            {
                log += "Clave de ejecucion incorrecta";
                utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
            }

            try
            {
                EjecutaProceso();
            }

            catch (ArgumentException ex)
            {
                log += $"Parametros incorrectos. {ex.Message}";
                utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
            }
        }

        private static void ValidarFicheros()
        {
            ficheroEntrada = Path.Combine(pathFicheros, argumentos[3]);
            if (!File.Exists(ficheroEntrada))
            {
                log += $"El fichero de entrada {ficheroEntrada} no existe";
                utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
            }

            ficheroErrores = Path.Combine(Path.GetDirectoryName(ficheroEntrada), "errores.txt");
            utilidad.borrarFicheros(ficheroErrores);

            ficheroSalida = Path.Combine(pathFicheros, argumentos[4]);

            string rutaSalida = Path.GetDirectoryName(ficheroSalida);
            string patronFicheros = Path.GetFileNameWithoutExtension(ficheroSalida) + ".*";

            string[] ficherosSalida = Directory.GetFiles(rutaSalida, patronFicheros);//Se borran todos los ficheros de salida posibles ya que puede haber .txt, .html o .pdf
            foreach (string fichero in ficherosSalida)
            {
                utilidad.borrarFicheros(fichero);
            }
        }

        private static void EjecutaProceso()
        {
            tipo = argumentos[2];
            string control = string.Empty;
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

                    //Se necesitan pasar 6 parametros como minimo
                    ControlParametros(6);

                    presentacionDirecta envio = new presentacionDirecta();
                    envio.envioPeticion(ficheroEntrada, ficheroSalida, serieCertificado, instanciaCertificado);

                    break;

                case "2":
                    //Validacion de modelos. No necesita certificado

                    //Ejemplo de ejecucion
                    //gestionesAEAT.exe ds123456 2 empresa_guion.txt empresa_salida.txt NO

                    //Se necesitan pasar 6 parametros 
                    ControlParametros(6);

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

                    //Se necesitan pasar 6 parametros como minimo
                    ControlParametros(6);

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

                    //Se necesitan pasar 6 parametros como minimo
                    ControlParametros(6);

                    ratificarDomicilio ratifica = new ratificarDomicilio();

                    //Se procesa dos veces para el titular y conyuge
                    ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 1, instanciaCertificado);
                    ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 2, instanciaCertificado);
                    break;

                case "5":
                    //Descarga datos fiscales renta. No necesita certificado
                    //Se puede hacer con certificado, pero esta preparado para hacerlo con la referencia de renta.

                    //Ejemplo de ejecucion (renta 2023) 
                    // gestionesAEAT.exe ds123456 5 salida.txt 05197043D KEKTXP S https://www9.agenciatributaria.gob.es/wlpl/DFPA-D182/SvDesDF23Pei

                    //Como este tipo es especial se controlan los parametros directamente aqui
                    if (argumentos.Length != 8)
                    {
                        log += "Parámetros insuficientes para la operación solicitada.";
                        utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                    }
                    else
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

                    descargaDatosFiscales descargaDF = new descargaDatosFiscales();
                    descargaDF.descargaDF(urlDescargaDf, nifDf, refRenta, datosPersonales, ficheroSalida);
                    break;

                case "6":
                    //Obtener datos certificados instalados. No necesita certificado

                    //Ejemplo de ejecucion
                    //gestionesAEAT.exe ds123456 6 certificados_salida.txt

                    //La obtencion de los certificados instalados solo tiene 4 parametros
                    if (argumentos.Length == 4)
                    {
                        ficheroSalida = Path.Combine(pathFicheros, argumentos[3]);
                        utilidad.borrarFicheros(ficheroSalida);
                    }
                    else
                    {
                        log += "Parámetros insuficientes para la operación solicitada.";
                        utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                    }

                    instanciaCertificado.exportarDatosCertificados(ficheroSalida);
                    break;

                case "7":
                    //Presentacion facturas SII. Necesita certificado

                    //Ejemplo de ejecucion (se solicita el certificado en pantalla)
                    //gestionesAEAT.exe ds123456 7 facturaEmitida.xml respuesta.xml 0 SI

                    //Ejemplo de ejecucion (se pasa el numero de serie del certificado del almacen)
                    //gestionesAEAT.exe ds123456 7 empresa_guion.txt empresa_salida.txt 0 SI numeroserie

                    //Ejemplo de ejecucion (se pasa fichero y password del certificado)
                    //gestionesAEAT.exe ds123456 7 empresa_guion.txt empresa_salida.txt 0 SI certificado.pdf password

                    //Ejemplo de ejecucion (se pasa el nombre a buscar en los certificados)
                    //gestionesAEAT.exe ds123456 7 facturaEmitida.xml respuesta.xml 0 SI textoBusqueda


                    /*Nota: desarrollar esta gestion teniendo en cuenta que se genera el fichero sii_urls.txt que tiene la lista
                     * de las urls a las que hacer el envio segun si se trata de emitidas, recibidas, etc. En la ejecucion anterior
                     * se pasaba el numero de linea a la que hacer el envio en el tercer parametro (despues de la dsclave). Ademas se
                     * añade en el sexto parametro un NO seguido del numero de serie del certificado, y si tiene 6 parametros, el
                     * ultimo parametro es el nombre del certificado a utilizar que estan puesto en los parametros del sii_base en el
                     * campo "Texto de busqueda del certificado automatico"
                     * 
                     * El proceso utiliza los mismos procesos que el envio de modelos, pero se envia un XML con el lote de facturas, 
                     * y se recibe un XML con la respuesta de Hacienda, que luego se graba con el nombre de salida; 
                     * el metodo "utilidad.envioPost" es el metodo que hace el envio y recibe la url, los datos de envio
                     * (que sera el xml), la instancia del certificado, y el tipo de envio), por lo que solo habria que hacer la
                     * llamada mandando la url y el xml leido del fichero de entrada.

                    */

                    string ficheroUrls = string.Empty;

                    if (argumentos.Length < 7)
                    {
                        log += "Parámetros insuficientes para la operación solicitada.";
                        utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                    }

                    else
                    {
                        ficheroEntrada = argumentos[3];
                        ficheroSalida = argumentos[4];
                        ficheroUrls = Path.Combine(Path.GetDirectoryName(ficheroEntrada), "sii_urls.txt");

#if DEBUG
                        {
                            ficheroUrls = Path.Combine(pathFicheros, "sii_urls.txt");
                        }
#endif
                        if (!File.Exists(ficheroUrls))
                        {
                            log += "El fichero de urls no exite en la ruta de ejecucion";
                            utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                        }

                        int indiceUrl = int.Parse(argumentos[5]);

                        EnvioSii nuevoEnvio = new EnvioSii(ficheroUrls);//Instanciacion de la clase que carga las urls
                        utilidad.url = nuevoEnvio.urlEnvio(indiceUrl);


                        if (argumentos.Length == 8)
                        {
                            //Viene con el nombre del certificado que hay que buscar
                            serieCertificado = instanciaCertificado.buscarNombreCertificado(argumentos[7]);
                        }

                        //Esto es copia de la presentacion directa, modificarlo para este metodo
                        //presentacionDirecta envio = new presentacionDirecta();
                        //envio.envioPeticion(ficheroEntrada, ficheroSalida, serieCertificado, instanciaCertificado);


                    }

                    break;
            }
        }

        private static void ControlParametros(int cantidadParametros)
        {
            string control = string.Empty;

            int totalParametros = argumentos.Length;
            if (totalParametros < cantidadParametros)
            {
                control = "Parámetros insuficientes para la operación solicitada.";
            }
            else
            {
                if (totalParametros >= 6)
                {
                    if (argumentos[5].ToUpper() == "SI") conCertificado = true;

                    if (conCertificado)
                    {
                        if (totalParametros == 6)
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
                                utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                            }
                            passwordCertificado = argumentos[7];

                            string resultadoLectura = instanciaCertificado.leerCertificado(ficheroCertificado, passwordCertificado);
                            if (!string.IsNullOrEmpty(resultadoLectura))
                            {
                                log += $"Error al leer el certificado. {resultadoLectura}";
                                utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
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
            }

            if (!string.IsNullOrEmpty(control))
            {
                log += control;
                utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
            }
            ValidarFicheros();
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

        private static void ControlCertificado(ref string serieCertificado)
        {
            //Metodo para controlar si no se ha seleccionado un certificado y solicitarlo por pantalla (la variable se pasa por referencia para que si hay que modificarla se grabe en la clase
            //Ya no tiene uso porque con el metodo 'ValidarParametros' siempre se generará le nº de serie del certificado, bien pidiendolo por pantalla o leyendolo de los parametros (nº serie o fichero)
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
                    utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                }
            }
            else
            {
                log += "Certificado no encontrado en el almacen";
                utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
            }
        }
    }
}
