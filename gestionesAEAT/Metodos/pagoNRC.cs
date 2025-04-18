﻿using gestionesAEAT.Utilidades;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace gestionesAEAT.Metodos
{
    public class DatosPagoAutoliquidacion
    {
        public ElementosOperacion operacion { get; set; }
        public ElementosDeclaracion declaracion { get; set; }
        public ElementosObligado obligadoTributario { get; set; }
        public ElementosIngreso ingreso { get; set; }
    }

    public class ElementosOperacion
    {
        public string tipo { get; set; }

    }

    public class ElementosDeclaracion
    {
        public string modelo { get; set; }
        public int ejercicio { get; set; }
        public string periodo { get; set; }
        public string resultado { get; set; }
        public string fracciona { get; set; }
    }

    public class ElementosObligado
    {
        public string nif { get; set; }
        public string nombre { get; set; }
    }

    public class ElementosIngreso
    {
        public string importe { get; set; }
        public string iban { get; set; }
    }

    public class RespuestaDatosPago
    {
        public string nrc { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
    }

    public class PagoNRC
    {
        public string atributo = string.Empty;
        public string valor = string.Empty;


        public void envioPeticion()
        {
            string ficheroEntrada = Parametros.ficheroEntrada;
            string ficheroSalida = Parametros.ficheroSalida;
            string serieCertificado = Parametros.serieCertificado;
            string ficheroResultado = Parametros.ficheroResultado;

            envioAeat envio = new envioAeat();

            try
            {
                // Instanciacion de la clase para almacenar las propiedades
                DatosPagoAutoliquidacion contenidoEnvio = new DatosPagoAutoliquidacion();

                //Hace la carga del guion en las propiedades de las clases
                cargaDatosPago(contenidoEnvio, ficheroEntrada);

                //Prepara y envia los datos a la AEAT
                string jsonEnvio = JsonConvert.SerializeObject(contenidoEnvio, new JsonSerializerSettings
                {
                    //Serializa el json ignorando valores nulos y formateando la respuesta de forma indentada
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                //Serializa la respuesta
                StringBuilder textoSalida = new StringBuilder();
                try
                {
                    envio.envioPost(Utiles.url, jsonEnvio, serieCertificado, "json");
                    string respuestaAEAT = envio.respuestaEnvioAEAT;

                    //Procesa la respuesta
                    if (envio.estadoRespuestaAEAT == "OK")
                    {
                        //Si se ha podido enviar, se serializa la respuesta de Hacienda
                        RespuestaDatosPago respuestaEnvioModelos = JsonConvert.DeserializeObject<RespuestaDatosPago>(respuestaAEAT);
                        if (respuestaEnvioModelos.nrc != null)
                        {
                            textoSalida.AppendLine($"nrc = {respuestaEnvioModelos.nrc}");
                        }
                    }

                }

                //Si se produce una excepcion hay que capturar la respuesta ya que ahi vienen los errores
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        WebResponse respuestaAEAT = (HttpWebResponse)ex.Response;
                        string contenidoError = string.Empty;

                        Stream stream = respuestaAEAT.GetResponseStream();
                        StreamReader reader = new StreamReader(stream);
                        contenidoError = reader.ReadToEnd();

                        RespuestaDatosPago respuestaEnvioModelos = JsonConvert.DeserializeObject<RespuestaDatosPago>(contenidoError);
                        int indice = 0;
                        if (respuestaEnvioModelos.codigo != null)
                        {
                            textoSalida.AppendLine($"E{indice.ToString("D2")} = {respuestaEnvioModelos.codigo} : {respuestaEnvioModelos.descripcion}");
                        }
                    }
                }

                //Si se produce otra excepcion
                catch (Exception ex)
                {
                    Utiles.GrabarSalida("Problemas al conectar con el servidor de la AEAT", ficheroResultado);
                    Utiles.grabadaSalida = true;
                }

                //Grabacion de los ficheros de salida y resultado
                Utiles.GrabarSalida(textoSalida.ToString(), ficheroSalida);
                Utiles.GrabarSalida("OK", ficheroResultado);
            }

            catch (Exception ex)
            {
                //Si se ha producido algun error en el envio
                string mensaje = $"MENSAJE = Proceso cancelado o error en el envio. {ex.Message}";
                Utiles.GrabarSalida(mensaje, Parametros.ficheroResultado);
                Utiles.grabadaSalida = true;
            }
        }
        public void cargaDatosPago(DatosPagoAutoliquidacion contenidoEnvio, string ficheroEntrada)
        {
            //Prepara el guion
            Utiles.cargaDatosGuion(ficheroEntrada);

            //Instanciacion de la clase para almacenar los valores de la cabecera
            contenidoEnvio.operacion = new ElementosOperacion();
            contenidoEnvio.declaracion = new ElementosDeclaracion();
            contenidoEnvio.obligadoTributario = new ElementosObligado();
            contenidoEnvio.ingreso = new ElementosIngreso();

            //Formatear datos de la cabecera
            foreach (var elemento in Utiles.cabecera)
            {
                (atributo, valor) = Utiles.divideCadena(elemento, '=');

                // Verificar si el nombre coincide con alguna propiedad de la clase servaliDos y asignar el valor correspondiente
                switch (atributo)
                {
                    case "TIPO":
                        contenidoEnvio.operacion.tipo = valor;
                        break;

                    case "MODELO":
                        contenidoEnvio.declaracion.modelo = valor;
                        break;

                    case "EJERCICIO":
                        contenidoEnvio.declaracion.ejercicio = Convert.ToInt32(valor);
                        break;

                    case "PERIODO":
                        contenidoEnvio.declaracion.periodo = valor;
                        break;

                    case "RESULTADO":
                        contenidoEnvio.declaracion.resultado = valor;
                        break;

                    case "FRACCIONA":
                        contenidoEnvio.declaracion.fracciona = valor;
                        break;

                    case "NIF":
                        contenidoEnvio.obligadoTributario.nif = valor;
                        break;

                    case "NOMBRE":
                        contenidoEnvio.obligadoTributario.nombre = valor;
                        break;

                    case "IMPORTE":
                        contenidoEnvio.ingreso.importe = valor;
                        break;

                    case "IBAN":
                        contenidoEnvio.ingreso.iban = valor;
                        break;

                }
            }
        }
    }
}
