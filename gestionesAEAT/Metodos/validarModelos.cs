﻿using gestionesAEAT.Utilidades;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace gestionesAEAT.Metodos
{
    public class servaliDos
    {
        [JsonProperty("MODELO")]
        public string Modelo { get; set; }

        [JsonProperty("EJERCICIO")]
        public string Ejercicio { get; set; }

        [JsonProperty("PERIODO")]
        public string Periodo { get; set; }

        [JsonProperty("F01")]
        public string F01 { get; set; }

        [JsonProperty("IDI")]
        public string Idioma { get; set; }

        [JsonProperty("SINVL")]
        public string SinValidar { get; set; }
    }

    public class RespuestaValidarModelos
    {
        [JsonProperty("respuesta")]
        public elementosRespuesta respuesta { get; set; }
    }
    public class elementosRespuesta
    {
        [JsonProperty("errores")]
        public List<string> errores { get; set; } = new List<string>();

        [JsonProperty("pdf")]
        public List<string> pdf { get; set; } = new List<string>();

        [JsonProperty("avisos")]
        public List<string> avisos { get; set; } = new List<string>();

        [JsonProperty("advertencias")]
        public List<string> advertencias { get; set; } = new List<string>();
    }

    public class validarModelos
    {
        string atributo = string.Empty; //Cada una de las variables que se pasan a la AEAT
        string valor = string.Empty; //Valor del atributo que se pasa a la AEAT

        string respuestaAEAT; //Contenido de la respuesta de la AEAT a la solicitud enviada

        List<string> textoEnvio = new List<string>(); //Prepara una lista con los datos del guion

        string textoSalida = string.Empty; //Texto que se grabara en el fichero de salida

        envioAeat envio = new envioAeat();

        public void envioPeticion()
        {
            string ficheroEntrada = Parametros.ficheroEntrada;
            string ficheroSalida = Parametros.ficheroSalida;
            string ficheroResultado = Parametros.ficheroResultado;

            try
            {
                //Prepara los datos del guion
                Utiles.cargaDatosGuion(ficheroEntrada); //Monta en la clase Utiles las listas "cabecera", "body" y "respuesta" para luego acceder a esos datos a montar el envio

                //Instanciacion de la clase para almacenar los valores de la cabecera
                servaliDos dato = new servaliDos();

                //Formatear datos de la cabecera
                foreach (var elemento in Utiles.cabecera)
                {
                    (atributo, valor) = Utiles.divideCadena(elemento, '=');

                    // Verificar si el nombre coincide con alguna propiedad de la clase servaliDos y asignar el valor correspondiente
                    switch (atributo)
                    {
                        case "MODELO":
                            dato.Modelo = valor;
                            break;

                        case "EJERCICIO":
                            dato.Ejercicio = valor;
                            break;

                        case "PERIODO":
                            dato.Periodo = valor;
                            break;

                        case "F01":
                            dato.F01 = valor;
                            break;

                        case "IDI":
                            dato.Idioma = valor;
                            break;

                        case "VALIDAR":
                            if (valor == "NO")
                            {
                                dato.SinValidar = string.Empty;
                            }
                            break;

                        default:
                            break;
                    }
                }

                //Serializa el Json para hacer el envio
                string jsonEnvio = JsonConvert.SerializeObject(dato, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented

                });

                envio.envioPost(Utiles.url, jsonEnvio, "json");//Metodo sin certificado
                respuestaAEAT = envio.respuestaEnvioAEAT;


                //Procesa la respuesta
                if (envio.estadoRespuestaAEAT == "OK")
                {
                    //Deserializa la respuesta de Hacienda con la clase RespuestaValidarModelos
                    Utiles.respuestaValidarModelos = JsonConvert.DeserializeObject<RespuestaValidarModelos>(respuestaAEAT);
                    textoSalida = Utiles.generarRespuesta(ficheroSalida, "validar");

                    //Grabar un fichero con los errores, avisos o advertencias que se han podido producir
                    if (!string.IsNullOrEmpty(textoSalida))
                    {
                        string rutaHtml = Path.ChangeExtension(ficheroSalida, "html");
                        File.WriteAllText(rutaHtml, textoSalida);
                    }

                    //Si hay una respuesta en pdf se graba en la ruta de salida
                    string ficheroPdf = string.Empty;
                    if (Utiles.respuestaValidarModelos.respuesta.pdf != null && Utiles.respuestaValidarModelos.respuesta.pdf.Count > 0)
                    {
                        ficheroPdf = Path.ChangeExtension(ficheroSalida, "pdf");
                        string respuestaPDF = Utiles.respuestaValidarModelos.respuesta.pdf[0];
                        byte[] contenidoPDF = Convert.FromBase64String(respuestaPDF);
                        File.WriteAllBytes(ficheroPdf, contenidoPDF);
                    }

                    //Procesa la respuesta de la validacion para generar el fichero de salida
                    var respuestaValidar = Utiles.respuestaValidarModelos;
                    string resultadoSalida = Utiles.generaFicheroSalida(respuestaValidar, ficheroPdf);

                    //Graba el fichero de salida
                    File.WriteAllText(ficheroSalida, resultadoSalida.ToString());

                    //Graba el ficheroResultado
                    File.WriteAllText(ficheroResultado, "OK");
                }
            }

            catch (Exception ex)
            {
                //Si se ha producido algun error en el envio
                string mensaje = $"MENSAJE = Proceso cancelado o error en el envio. {ex.Message}";
                Utiles.GrabarSalida(mensaje, ficheroResultado);
                Utiles.grabadaSalida = true;
            }
        }
    }
}