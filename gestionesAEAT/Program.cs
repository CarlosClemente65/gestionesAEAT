using gestionesAEAT.Formularios;
using gestionesAEAT.Metodos;
using System;
using System.Collections.Generic;
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
        static GestionCertificados instanciaCertificado = new GestionCertificados();
        static string log = string.Empty; //Sirve para grabar un log de los posibles errores que se hayan producido en el proceso.

        //Declaracion de variables a nivel de clase para hacerlas accesibles al resto.
        static string dsclave = string.Empty; //Unicamente servira como medida de seguridad de ejecucion y debe pasarse 'ds123456'
        static string tipo = string.Empty;
        static string ficheroOpciones = string.Empty;
        static string ficheroEntrada = string.Empty;
        static string ficheroSalida = "salida.txt";
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

            if (argumentos.Length < 3)
            {
                if (argumentos.Length > 1 && (argumentos[1] == "-h" || argumentos[1] == "?"))
                {
                    utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                }
                else
                {
                    log += "Son necesarios al menos 2 parametros: dsclave y fichero de opciones";
                    utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                }
            }
            else
            {
                dsclave = argumentos[1];
                ficheroOpciones = Path.Combine(pathFicheros, argumentos[2]);
                if (dsclave != "ds123456")
                {
                    log += "Clave de ejecucion incorrecta";
                    utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                }

                if (!File.Exists(ficheroOpciones))
                {
                    log += "No existe el fichero de opciones";
                    utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                }
                else
                {
                    GestionOpciones();

                    log += ControlParametros(tipo);
                    if (!string.IsNullOrEmpty(log)) utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);

                    try
                    {
                        ControlCertificado();
                        EjecutaProceso();
                    }

                    catch (ArgumentException ex)
                    {
                        log += $"Se ha producido un error al procesar la peticion. {ex.Message}";
                        utilidad.SalirAplicacion(log, pathFicheros, ficheroErrores);
                    }
                }
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
            utilidad.borrarFicheros(ficheroSalida);
        }

        private static void EjecutaProceso()
        {
            switch (tipo)
            {
                case "1":
                    //Envio de modelos. Necesita certificado 

                    //Ejemplo de fichero de opciones (se solicita el certificado en pantalla)
                    //TIPO=1
                    //ENTRADA=empresa_guion.txt
                    //SALIDA=empresa_salida.txt
                    //OBLIGADO=SI

                    //Ejemplo de fichero de opciones (se pasa el numero de serie del certificado del almacen)
                    //TIPO=1
                    //ENTRADA=empresa_guion.txt
                    //SALIDA=empresa_salida.txt
                    //OBLIGADO=SI
                    //BUSQUEDA=numeroSerie

                    //Ejemplo de fichero de opciones (se pasa fichero y password del certificado)
                    //TIPO=1
                    //ENTRADA=empresa_guion.txt
                    //SALIDA=empresa_salida.txt
                    //OBLIGADO=SI
                    //CERTIFICADO=certificado.pfx
                    //PASSWORD=contraseña

                    presentacionDirecta envioDirecto = new presentacionDirecta();
                    envioDirecto.envioPeticion(ficheroEntrada, ficheroSalida, serieCertificado, instanciaCertificado);

                    break;

                case "2":
                    //Validacion de modelos. No necesita certificado

                    //Ejemplo de fichero de opciones
                    //TIPO=2
                    //ENTRADA=empresa_guion.txt
                    //SALIDA=empresa_salida.txt
                    //OBLIGADO=NO

                    validarModelos valida = new validarModelos();
                    valida.envioPeticion(ficheroEntrada, ficheroSalida);

                    break;

                case "3":
                    //Consulta de modelos presentados. Necesita certificado

                    //Ejemplo de fichero de opciones (se solicita el certificado en pantalla)
                    //TIPO=3
                    //ENTRADA=empresa_guion.txt
                    //SALIDA=empresa_salida.txt
                    //OBLIGADO=SI

                    //Ejemplo de fichero de opciones (se pasa el numero de serie del certificado del almacen)
                    //TIPO=3
                    //ENTRADA=empresa_guion.txt
                    //SALIDA=empresa_salida.txt
                    //OBLIGADO=SI
                    //BUSQUEDA=numeroSerie

                    //Ejemplo de fichero de opciones (se pasa fichero y password del certificado)
                    //TIPO=3
                    //ENTRADA=empresa_guion.txt
                    //SALIDA=empresa_salida.txt
                    //OBLIGADO=SI
                    //CERTIFICADO=certificado.pfx
                    //PASSWORD=contraseña

                    descargaModelos descarga = new descargaModelos();
                    descarga.obtenerModelos(ficheroEntrada, ficheroSalida, serieCertificado, instanciaCertificado);
                    break;

                case "4":
                    //Ratificacion de domicilio. Necesita certificado

                    //Ejemplo de fichero de opciones (se solicita el certificado en pantalla)
                    //TIPO=4
                    //ENTRADA=empresa_guion.txt
                    //SALIDA=empresa_salida.txt
                    //OBLIGADO=SI

                    //Ejemplo de fichero de opciones (se pasa el numero de serie del certificado del almacen)
                    //TIPO=4
                    //ENTRADA=empresa_guion.txt
                    //SALIDA=empresa_salida.txt
                    //OBLIGADO=SI
                    //BUSQUEDA=numeroSerie

                    //Ejemplo de fichero de opciones (se pasa fichero y password del certificado)
                    //TIPO=4
                    //ENTRADA=empresa_guion.txt
                    //SALIDA=empresa_salida.txt
                    //OBLIGADO=SI
                    //CERTIFICADO=certificado.pfx
                    //PASSWORD=contraseña

                    ratificarDomicilio ratifica = new ratificarDomicilio();

                    //Se procesa dos veces para el titular y conyuge
                    ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 1, instanciaCertificado);
                    if (ratifica.nifConyuge)
                    {
                        ratifica.envioPeticion(serieCertificado, ficheroEntrada, ficheroSalida, 2, instanciaCertificado);
                    }
                    break;

                case "5":
                    //Descarga datos fiscales renta. No necesita certificado
                    //Se puede hacer con certificado, pero esta preparado para hacerlo con la referencia de renta.

                    //Ejemplo de fichero de opciones (renta 2023)
                    //TIPO=5
                    //SALIDA=empresa_salida.txt
                    //NIFRENTA=05197043D
                    //REFRENTA=KEKTXP
                    //DPRENTA=S
                    //URLRENTA=https://www9.agenciatributaria.gob.es/wlpl/DFPA-D182/SvDesDF23Pei

                    descargaDatosFiscales descargaDF = new descargaDatosFiscales();
                    descargaDF.descargaDF(urlDescargaDf, nifDf, refRenta, datosPersonales, ficheroSalida);
                    break;

                case "6":
                    //Obtener datos certificados instalados. No necesita certificado

                    //Ejemplo de fichero de opciones (renta 2023)
                    //TIPO=6
                    //SALIDA=certificados_salida.txt

                    instanciaCertificado.exportarDatosCertificados(ficheroSalida);
                    break;

                case "7":
                    //Presentacion facturas SII. Necesita certificado

                    //Ejemplo de fichero de opciones (se solicita el certificado en pantalla)
                    //TIPO=7
                    //ENTRADA=facturaEmitida.xml
                    //SALIDA=respuesta-xml
                    //INDICESII=0
                    //OBLIGADO=SI

                    //Ejemplo de fichero de opciones (se pasa el numero de serie del certificado del almacen)
                    //TIPO=7
                    //ENTRADA=facturaEmitida.xml
                    //SALIDA=respuesta-xml
                    //INDICESII=0
                    //OBLIGADO=SI
                    //BUSQUEDA=numeroSerie

                    //Ejemplo de fichero de opciones (se pasa fichero y password del certificado)
                    //TIPO=7
                    //ENTRADA=facturaEmitida.xml
                    //SALIDA=respuesta-xml
                    //INDICESII=0
                    //OBLIGADO=SI
                    //CERTIFICADO=certificado.pfx
                    //PASSWORD=contraseña

                    string ficheroUrls = Path.Combine(Path.GetDirectoryName(ficheroEntrada), "sii_urls.txt");
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

                    EnvioSii nuevoEnvio = new EnvioSii(ficheroUrls);//Instanciacion de la clase que carga las urls
                    utilidad.url = nuevoEnvio.urlEnvio(indiceUrl);

                    break;
            }
        }

        private static string ControlParametros(string tipo)
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
            ficheroErrores = Path.Combine(Path.GetDirectoryName(ficheroSalida), "errores.txt");
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
                if (!string.IsNullOrEmpty(textoBusqueda))
                {
                    serieCertificado = instanciaCertificado.buscarCertificado(textoBusqueda);
                }
                else
                {
                    if (!string.IsNullOrEmpty(ficheroCertificado))
                    {
                        if (!File.Exists(ficheroCertificado))
                        {
                            mensaje = $"El fichero del certificado {ficheroCertificado} no existe";
                            utilidad.SalirAplicacion(mensaje, pathFicheros, ficheroErrores);
                        }
                        if (string.IsNullOrEmpty(passwordCertificado))
                        {
                            mensaje = "No se ha pasado la contraseña del certificado";
                            utilidad.SalirAplicacion(mensaje, pathFicheros, ficheroErrores);

                        }

                        //Se pasa el fichero del certificado y el pass
                        string resultadoLectura = instanciaCertificado.leerCertificado(ficheroCertificado, passwordCertificado);
                        if (!string.IsNullOrEmpty(resultadoLectura))
                        {
                            mensaje = $"Error al leer el certificado. {resultadoLectura}";
                            utilidad.SalirAplicacion(mensaje, pathFicheros, ficheroErrores);
                        }
                        var certificadosInfo = instanciaCertificado.relacionCertificados();
                        serieCertificado = certificadosInfo.LastOrDefault()?.serieCertificado;
                    }
                }

                //Si no se ha podido leer el certificado se solicita por pantalla
                if (string.IsNullOrEmpty(serieCertificado)) serieCertificado = seleccionCertificados();
            }
        }

        private static void GestionOpciones()
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
                    case "TIPO":
                        tipo = valor;

                        break;

                    case "ENTRADA":
                        if (!string.IsNullOrEmpty(valor))
                        {
                            ficheroEntrada = Path.Combine(pathFicheros, valor);
                        }

                        break;

                    case "SALIDA":
                        if (!string.IsNullOrEmpty(valor))
                        {
                            ficheroSalida = Path.Combine(pathFicheros, valor);
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
                        if (!string.IsNullOrEmpty(valor))
                        {
                            ficheroCertificado = Path.Combine(pathFicheros, valor);
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
