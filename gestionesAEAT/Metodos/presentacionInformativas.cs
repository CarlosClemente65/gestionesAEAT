using gestionesAEAT.Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace gestionesAEAT.Metodos
{
    public class inicializaInformativas
    {
        public string modelo { get; set; }
        public string ejercicio { get; set; }
        public string periodo { get; set; }
        public string ndc { get; set; }
        public string idioma { get; set; }
        public string numbloques { get; set; }
        public string codificacion { get; set; }

    }

    public class respuestaInicializa
    {
        public string idenvio { get; set; }
        public string estado { get; set; }
        public string sigbloque { get; set; }
        public string codigo { get; set; }
        public string mensaje { get; set; }

    }

    public class envioInformativas
    {
        public string idenvio { get; set; }
        public string numbloque { get; set; }
        public string codificacion { get; set; }
    }

    public class respuestaEnvio
    {
        public string idenvio { get; set; }
        public string estado { get; set; }
        public string sigbloque { get; set; }
        public string codigo { get; set; }
        public string mensaje { get; set; }
        public string totalt2ok { get; set; }
        public string totalt2ko { get; set; }
        public string bloquet2ok { get; set; }
        public string bloquet2ko { get; set; }
        public string avisos { get; set; }
    }

    public class presentaInformativas
    {
        public string idenvio { get; set; }
        public string firnif { get; set; }
        public string firnombre { get; set; }
        public string fir { get; set; }

    }

    public class respuestaPresenta
    {
        public string idenvio { get; set; }
        public string estado { get; set; }
        public string numeroregistros { get; set; }
        public string csv { get; set; }
        public string expediente { get; set; }
        public string codigo { get; set; }
        public string mensaje { get; set; }

    }

    public class recuperarErrores
    {
        public string idenvio { get; set; }
        public string codificacion { get; set; }
    }

    public class respuestaRecuperarErrores
    {
        public string idenvio { get; set; }
        public string estado { get; set; }
        public string codigo { get; set; }
        public string mensaje { get; set; }
    }

    public class bajaInformativas
    {
        public string firnif { get; set; }
        public string firnombre { get; set; }
        public string fir { get; set; }
        public string modelo { get; set; }
        public string ejercicio { get; set; }
        public string periodo { get; set; }
        public string ndc { get; set; }
        public string expediente { get; set; }
    }

    public class respuestaBaja
    {
        public string expediente { get; set; }
        public string codigo { get; set; }
        public string mensaje { get; set; }
    }

    public class recuperarEnvio
    {
        public string modelo { get; set; }
        public string ejercicio { get; set; }
        public string periodo { get; set; }
        public string ndc { get; set; }
    }

    public class respuestaRecuperarEnvio
    {
        public string idenvio { get; set; }
        public string estado { get; set; }
        public string numbloques { get; set; }
        public string codigo { get; set; }
        public string mensaje { get; set; }
        public string enviot2ok { get; set; }
        public string enviot2ko { get; set; }
    }


    public class presentacionInformativas
    {
        Utiles utilidad = Program.utilidad; //Instanciacion de las utilidades para poder usarlas

        string atributo = string.Empty; //Cada una de las variables que se pasan a la AEAT
        string valor = string.Empty; //Valor del atributo que se pasa a la AEAT

        List<string> datosCabecera = new List<string>(); //Lineas de la cabecera preparadas para el header


        public void envioPeticion()
        {
            //El proceso puede ser: inicializa, envio, presenta, recupera y baja 

            envioAeat envio = new envioAeat();
            Type claseRespuesta = null;

            try
            {
                utilidad.cargaDatosGuion(Parametros.ficheroEntrada); //Monta en la clase Utiles las listas "cabecera", "body" y "respuesta" para luego acceder a esos datos a montar el envio

                //Obtenemos el tipo de proceso segun la ultima parte de la url que venga en el guion
                int indice = utilidad.url.LastIndexOf('/');
                string proceso = utilidad.url.Substring(indice + 1);

                // Objeto que contendrá la instancia de la clase a rellenar
                object instanciaClase = null;

                switch (proceso)
                {
                    case "InicializarEnvio":
                        instanciaClase = new inicializaInformativas();
                        claseRespuesta = typeof(respuestaInicializa);
                        break;

                    case "EnviarDatos":
                        instanciaClase = new envioInformativas();
                        claseRespuesta = typeof(respuestaEnvio);
                        break;

                    case "PresentarEnvio":
                        instanciaClase = new presentaInformativas();
                        claseRespuesta = typeof(respuestaPresenta);
                        break;

                    case "RecuperarErrores":
                        instanciaClase = new recuperarErrores();
                        claseRespuesta = typeof(respuestaRecuperarErrores);
                        break;

                    case "BajaDeclaracion":
                        instanciaClase = new bajaInformativas();
                        claseRespuesta = typeof(respuestaBaja);
                        break;

                    case "RecuperarEnvio":
                        instanciaClase = new recuperarEnvio();
                        claseRespuesta = typeof(respuestaRecuperarEnvio);
                        break;
                }

                // Asignación de los valores a las propiedades de la clase usando reflexión
                AsignarValoresClase(instanciaClase);

                //Prepara el cuerpo del envio
                string datosBody = string.Empty;
                for (int i = 0; i < utilidad.body.Count; i++)
                {
                    if (i == utilidad.body.Count - 1)
                    {
                        datosBody += utilidad.body[i];
                    }
                    else
                    {
                        datosBody += utilidad.body[i] + @"\n";
                    }
                }

                //Envia los datos
                envioInformativas(datosBody, claseRespuesta);
            }

            catch (Exception ex)
            {
                //Si se ha producido algun error en el envio
                string mensaje = $"MENSAJE = Proceso cancelado o error en el envio. {ex.Message}";
                utilidad.GrabarSalida(mensaje, Parametros.ficheroResultado);
                utilidad.grabadaSalida = true;
            }
        }

        public void AsignarValoresClase(object instanciaClase)
        {
            //Asigna los valores de la cabecera a las propiedades de la clase
            List<string> listaValores = utilidad.cabecera; //Carga la lista con los valores de la cabecera
            foreach (var linea in listaValores)
            {
                string nombre = string.Empty;
                string valor = string.Empty;
                (nombre, valor) = utilidad.divideCadena(linea, '=');
                if (!string.IsNullOrEmpty(valor))
                {
                    // Obtener el tipo de la clase instanciada
                    Type tipoClase = instanciaClase.GetType();

                    // Buscar si la clase tiene una propiedad que coincida con el nombre (se pone en minusculas porque asi esta definida la clase y es como se debe enviar a la AEAT y es como se recibe la respuesta.
                    var propiedad = tipoClase.GetProperty(nombre.ToLower());

                    // Si la propiedad existe y es escribible
                    if (propiedad != null && propiedad.CanWrite)
                    {
                        // Convertir el valor al tipo adecuado
                        object valorConvertido = Convert.ChangeType(valor, propiedad.PropertyType);

                        // Asignar el valor a la propiedad
                        propiedad.SetValue(instanciaClase, valorConvertido);

                        //Se rellena la lista 'datosCabecera' que luego se usa para el envio a la AEAT
                        datosCabecera.Add($"{nombre}={valorConvertido}"); // Formato parametro=valor
                    }
                }
            }
        }

        public void envioInformativas(string datosBody, Type claseRespuesta)
        {
            string url = utilidad.url;
            try
            {
                //Carga el certificado digital segun el numero de serie
                (X509Certificate2 certificado, bool resultado) = Program.gestionCertificados.exportaCertificadoDigital(Parametros.serieCertificado);

                if (certificado != null)
                {
                    //Protocolo de seguridad
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    //Crear datos para la solicitud HTTP
                    HttpWebRequest solicitudHttp = (HttpWebRequest)WebRequest.Create(url);

                    //Configurar la solicitud
                    solicitudHttp.Method = "POST";

                    //Configurar el tipo de contenido
                    solicitudHttp.ContentType = "text/plain;charset=ISO-8859-15";

                    if (datosCabecera != null)
                    {
                        foreach (var linea in datosCabecera)
                        {
                            (string nombre, string valor) = utilidad.divideCadena(linea, '=');
                            if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(valor))
                            {
                                solicitudHttp.Headers[nombre] = valor;
                            }
                        }
                    }

                    solicitudHttp.ClientCertificates.Add(certificado);

                    //Grabacion de los datos a enviar al servidor
                    byte[] datosEnvioBytes = Parametros.codificacion.GetBytes(datosBody);
                    using (var requestStream = solicitudHttp.GetRequestStream())
                    {
                        requestStream.Write(datosEnvioBytes, 0, datosEnvioBytes.Length);
                    }

                    HttpWebResponse respuesta = (HttpWebResponse)solicitudHttp.GetResponse();
                    //Devuelve el estado 'OK' si todo ha ido bien
                    string estadoRespuestaAEAT = respuesta.StatusDescription;

                    if (estadoRespuestaAEAT == "OK")
                    {
                        StringBuilder contenidoRespuesta = new StringBuilder();
                        for (int i = 0; i < respuesta.Headers.Count; i++)
                        {
                            string nombreHeader = respuesta.Headers.GetKey(i); // Obtener el nombre de la cabecera
                            string valorHeader = respuesta.Headers[nombreHeader]; // Obtener el valor de la cabecera

                            var propiedad = claseRespuesta.GetProperty(nombreHeader.ToLower());
                            if (propiedad != null)
                            {
                                nombreHeader = nombreHeader.ToUpper();
                                contenidoRespuesta.AppendLine($"{nombreHeader} = {valorHeader}"); // Añadir nombre y valor al StringBuilder
                            }
                        }


                        //Almacena el cuerpo si tiene contenido (siempre seran los registros con errores de la presentacion
                        var erroresAEAT = respuesta.GetResponseStream();
                        if (erroresAEAT != null)
                        {
                            StringBuilder contenidoErrores = new StringBuilder();
                            using (StreamReader reader = new StreamReader(erroresAEAT))
                            {
                                string body = reader.ReadToEnd();

                                //Solo añade al reader las lineas que no estan vacias.
                                if (!string.IsNullOrWhiteSpace(body))
                                {
                                    contenidoErrores.AppendLine(body); // Añadir el body al contenido
                                }
                            }

                            if (contenidoErrores.Length > 0)
                            {
                                //string pathErrores = Path.Combine(Path.GetDirectoryName(Parametros.ficheroSalida), "errores.txt");

                                //Se modifica el fichero de salida para poner primero el error y luego los datos del registro
                                string[] lineasErrores = contenidoErrores.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                                StringBuilder erroresSalida = new StringBuilder();

                                int reg = 1;
                                foreach (string linea in lineasErrores)
                                {
                                    if (!string.IsNullOrWhiteSpace(linea))
                                    {
                                        if (linea.Contains(";"))
                                        {
                                            (string registro, string error) = utilidad.divideCadena(linea, ';');
                                            erroresSalida.AppendLine($"E{reg.ToString("D2")} = {error} - Registro {reg.ToString("D2")} = {registro}");
                                            reg++;
                                        }
                                        else
                                        {
                                            erroresSalida.AppendLine($"E{reg.ToString("D2")} = {linea}");
                                            reg++;
                                        }
                                    }
                                }

                                //Se unen los dos resultados para grabar el fichero de salida
                                contenidoRespuesta.AppendLine(erroresSalida.ToString());

                                utilidad.GrabarSalida(contenidoRespuesta.ToString(), Parametros.ficheroSalida);
                                //utilidad.GrabarSalida(erroresSalida.ToString(), pathErrores);
                            }

                        }
                    }

                    utilidad.GrabarSalida("OK", Parametros.ficheroResultado);
                }

                else
                {
                    utilidad.GrabarSalida("No se ha cargado el certificado", Parametros.ficheroResultado);
                }
            }

            catch (Exception ex)
            {
                utilidad.GrabarSalida($"Error en la conexion con el servidor. {ex.Message}", Parametros.ficheroResultado);
                utilidad.grabadaSalida = true;
            }
        }
    }
}
