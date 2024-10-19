using GestionCertificadosDigitales;
using gestionesAEAT.Utilidades;
using Newtonsoft.Json;
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


    public class presentacionInformativas
    {
        Utiles utilidad = Program.utilidad; //Instanciacion de las utilidades para poder usarlas

        List<string> textoEnvio = new List<string>();//Prepara una lista con los datos del guion

        string atributo = string.Empty; //Cada una de las variables que se pasan a la AEAT
        string valor = string.Empty; //Valor del atributo que se pasa a la AEAT

        string respuestaAEAT; //Contenido de la respuesta de la AEAT a la solicitud enviada

        string textoSalida = string.Empty; //Texto que se grabara en el fichero de salida

        public void envioPeticion(string proceso)
        {
            //El proceso puede ser: inicializa, envio, presentacion
            string ficheroEntrada = Parametros.ficheroEntrada;
            string ficheroSalida = Parametros.ficheroSalida;
            string serieCertificado = Parametros.serieCertificado;

            envioAeat envio = new envioAeat();

            textoEnvio = utilidad.prepararGuion(ficheroEntrada); //Se procesa el guion para formar una lista que se pueda pasar al resto de metodos

            try
            {
                utilidad.cargaDatosGuion(textoEnvio); //Monta en la clase Utiles las listas "cabecera", "body" y "respuesta" para luego acceder a esos datos a montar el envio

                // Objeto que contendrá la instancia de la clase a rellenar
                object instanciaClase = null;

                switch (proceso)
                {
                    case "inicializa":
                        instanciaClase = new inicializaInformativas();

                        break;

                    case "envio":
                        instanciaClase = new envioInformativas();

                        break;

                    case "presentacion":
                        instanciaClase = new presentaInformativas();

                        break;
                }

                // Asignación de los valores a las propiedades de la clase usando reflexión

                AsignarValoresClase(instanciaClase, utilidad.cabecera);

                //Prepara la cabecera
                foreach (var propiedad in instanciaClase.GetType().GetProperties())
                {
                    string nombre = propiedad.Name; // Nombre de la propiedad
                    string valor = propiedad.GetValue(instanciaClase)?.ToString(); // Valor de la propiedad

                    if (!string.IsNullOrEmpty(valor))
                    {
                        utilidad.datosCabecera.Add($"{nombre}={valor}"); // Formato parametro=valor
                    }
                }

                //Prepara el cuerpo del envio
                string datosBody = string.Join("\n", utilidad.body);

                //Envia los datos
                envioInformativas(utilidad.url, datosBody, Parametros.serieCertificado);


                //Revisar esta parte
                respuestaAEAT = envio.respuestaEnvioAEAT;

                if (envio.estadoRespuestaAEAT == "OK")
                {
                    //Si se ha podido enviar, se serializa la respuesta de Hacienda
                    utilidad.respuestaEnvioModelos = JsonConvert.DeserializeObject<RespuestaPresBasicaDos>(respuestaAEAT);
                    textoSalida = utilidad.generarRespuesta(ficheroSalida, "enviar");

                    //Procesado de los tipos de respuesta posibles
                    var respuestaEnvio = utilidad.respuestaEnvioModelos.respuesta;
                    if (respuestaEnvio.correcta != null && !string.IsNullOrEmpty(respuestaEnvio.correcta.CodigoSeguroVerificacion))
                    {
                        //Si hay datos en las propiedades de la respuesta correcta se graba el PDF y el fichero con los datos del modelo
                        var elementosRespuesta = respuestaEnvio.correcta;
                        if (elementosRespuesta.urlPdf != null)
                        {
                            //Grabar el PDF en la ruta
                            string ficheroPdf = Path.ChangeExtension(ficheroSalida, "pdf");

                            using (WebClient clienteWeb = new WebClient())
                            {
                                byte[] contenidoPdf = clienteWeb.DownloadData(elementosRespuesta.urlPdf);
                                File.WriteAllBytes(ficheroPdf, contenidoPdf);
                            }
                        }

                        //Grabacion de los datos de la respuesta en el fichero de salida
                        using (StreamWriter writer = new StreamWriter(ficheroSalida))
                        {
                            var propiedadesSeleccionadas = new List<string> //Permite buscar solo las propiedades que necesitamos grabar en el fichero
                        {
                            "Modelo", "Ejercicio", "Periodo","CodigoSeguroVerificacion", "Justificante", "Expediente"
                        };

                            Type tipo = elementosRespuesta.GetType();
                            var propiedades = tipo.GetProperties();

                            foreach (var propiedad in propiedades)
                            {
                                if (propiedadesSeleccionadas.Contains(propiedad.Name))
                                {
                                    var valor = propiedad.GetValue(elementosRespuesta);
                                    writer.WriteLine($"{propiedad.Name}: {valor}");

                                }
                            }
                            if (elementosRespuesta.avisos != null) //Si hay avisos tambien se graban en el fichero de salida
                            {
                                for (int i = 0; i < elementosRespuesta.avisos.Count; i++)
                                {
                                    writer.WriteLine($"A{i + 1.ToString("D2")}: {elementosRespuesta.avisos[i]}");
                                }
                            }

                            if (elementosRespuesta.advertencias != null) //Si hay advertencias tambien se graban en el fichero de salida
                            {
                                for (int i = 0; i < elementosRespuesta.advertencias.Count; i++)
                                {
                                    writer.WriteLine($"D{i + 1.ToString("D2")}: {elementosRespuesta.advertencias[i]}");
                                }
                            }
                        }
                    }

                    else
                    {
                        //Procesa la respuesta de la validacion para generar el fichero de salida
                        var respuestaEnvioModelos = utilidad.respuestaEnvioModelos;
                        string resultadoSalida = utilidad.generaFicheroSalida(respuestaEnvioModelos);

                        //Graba el fichero de salida
                        File.WriteAllText(Parametros.ficheroSalida, resultadoSalida);
                    }


                    //Grabar un html con los errores, avisos o advertencias generados
                    if (!string.IsNullOrEmpty(textoSalida))
                    {
                        string rutaHtml = Path.ChangeExtension(ficheroSalida, "html");
                        File.WriteAllText(rutaHtml, textoSalida);
                    }

                    //Grabar el fichero de respuesta
                    File.WriteAllText(Parametros.ficheroResultado, "OK");

                }

            }

            catch (Exception ex)
            {

            }
        }

        public void AsignarValoresClase(object instanciaClase, List<string> listaValores)
        {
            foreach (var linea in listaValores)
            {
                string nombre = string.Empty;
                string valor = string.Empty;
                (nombre, valor) = utilidad.divideCadena(linea, '=');
                if (!string.IsNullOrEmpty(valor))
                {
                    // Obtener el tipo de la clase instanciada
                    Type tipoClase = instanciaClase.GetType();

                    // Buscar si la clase tiene una propiedad que coincida con el nombre
                    var propiedad = tipoClase.GetProperty(nombre.ToLower());

                    // Si la propiedad existe y es escribible
                    if (propiedad != null && propiedad.CanWrite)
                    {
                        // Convertir el valor al tipo adecuado
                        object valorConvertido = Convert.ChangeType(valor, propiedad.PropertyType);

                        // Asignar el valor a la propiedad
                        propiedad.SetValue(instanciaClase, valorConvertido);
                    }
                }
            }
        }


        public void envioInformativas(string url, string datosEnvio, string serieCertificado) //Se pone el tipo de envio opcional como formulario y si es de tipo json se debe pasar en la llamada al metodo
        {
            try
            {
                (X509Certificate2 certificado, bool resultado) = Program.gestionCertificados.exportaCertificadoDigital(serieCertificado);

                if (certificado != null)
                {
                    //Protocolo de seguridad
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    //Crear datos para la solicitud HTTP
                    HttpWebRequest solicitudHttp = (HttpWebRequest)WebRequest.Create(url);

                    //Configurar la solicitud
                    solicitudHttp.Method = "POST";

                    //Configurar el tipo de contenido
                    solicitudHttp.ContentType = "text/plain;charset=ISO-8859-15";

                    if (utilidad.datosCabecera != null)
                    {
                        foreach (var linea in utilidad.datosCabecera)
                        {
                            //solicitudHttp.Headers.Add(linea);
                            (string nombre, string valor) = utilidad.divideCadena(linea, '=');
                            if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(valor))
                            {
                                solicitudHttp.Headers[nombre] = valor;
                            }
                        }
                    }

                    solicitudHttp.ClientCertificates.Add(certificado);

                    //Asigna la codificacion del envio
                    Encoding encoding;

                    string codificacion = "UTF-8"; //Tipo de codificacion a utilizar en el envio

                    try
                    {
                        encoding = Encoding.GetEncoding(codificacion);
                    }
                    catch (ArgumentException)
                    {
                        encoding = Encoding.UTF8;
                    }


                    //Grabacion de los datos a enviar al servidor
                    byte[] datosEnvioBytes = encoding.GetBytes(datosEnvio);
                    using (var requestStream = solicitudHttp.GetRequestStream())
                    {
                        requestStream.Write(datosEnvioBytes, 0, datosEnvioBytes.Length);
                    }

                    HttpWebResponse respuesta = (HttpWebResponse)solicitudHttp.GetResponse();
                    //Devuelve el estado 'OK' si todo ha ido bien
                    string estadoRespuestaAEAT = respuesta.StatusDescription;

                    StringBuilder contenidoRespuesta = new StringBuilder();
                    if (estadoRespuestaAEAT == "OK")
                    {
                        Type tipoRespuesta = typeof(respuestaInicializa);

                        for (int i = 0; i < respuesta.Headers.Count; i++)
                        {
                            string nombreHeader = respuesta.Headers.GetKey(i); // Obtener el nombre de la cabecera
                            string valorHeader = respuesta.Headers[nombreHeader]; // Obtener el valor de la cabecera

                            var propiedad = tipoRespuesta.GetProperty(nombreHeader.ToLower());
                            if (propiedad != null)
                            {
                                contenidoRespuesta.AppendLine($"{nombreHeader}= {valorHeader}"); // Añadir nombre y valor al StringBuilder
                            }
                        }
                        utilidad.GrabarSalida(contenidoRespuesta.ToString(), Parametros.ficheroSalida);
                        utilidad.GrabarSalida("OK", Parametros.ficheroResultado);
                    }

                }
                else
                {
                    utilidad.GrabarSalida("No se ha cargado el certificado",Parametros.ficheroResultado);
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
