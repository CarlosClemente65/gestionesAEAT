using gestionesAEAT.Utilidades;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionesAEAT.Metodos
{
    public class DatosPagoAutoliquidacion
    {
        public ElementosOperacion operacion { get; set; }
        public ElementosDeclaracion declaracion { get; set; }
        public ElementosObligado obligado { get; set; }
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
        public List<ElementosErrorDatosPago> error { get; set; }
    }

    public class ElementosErrorDatosPago
    {
        public string codigo { get; set; }
        public string descripcion { get; set; }

    }
    public class pagoNRC
    {
        Utiles utilidad = new Utiles();

        public string atributo = string.Empty;
        public string valor = string.Empty;


        public void envioPeticion()
        {
            string ficheroEntrada = Parametros.ficheroEntrada;
            string ficheroSalida = Parametros.ficheroSalida;
            string serieCertificado = Parametros.serieCertificado;
            string ficheroResultado = Parametros.ficheroResultado;
            ////string url = utilidad.url;

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

                envio.envioPost(utilidad.url, jsonEnvio, serieCertificado, "json");
                string respuestaAEAT = envio.respuestaEnvioAEAT;

                //Procesa la respuesta
                if (envio.estadoRespuestaAEAT == "OK")
                {
                    //Si se ha podido enviar, se serializa la respuesta de Hacienda
                    RespuestaDatosPago respuestaEnvioModelos = JsonConvert.DeserializeObject<RespuestaDatosPago>(respuestaAEAT);
                    StringBuilder textoSalida = new StringBuilder();
                    if (respuestaEnvioModelos.nrc != null)
                    {
                        textoSalida.AppendLine($"nrc = { respuestaEnvioModelos.nrc}");
                    }
                    else
                    {
                        textoSalida.AppendLine($"nrc = ");
                    }
                    if (respuestaEnvioModelos.error != null)
                    {
                        foreach (var elemento in respuestaEnvioModelos.error)
                        {
                            textoSalida.AppendLine($"errores = {elemento}");
                        }
                    }
                    else
                    {
                        textoSalida.AppendLine("errores = ");
                    }

                    utilidad.GrabarSalida(textoSalida.ToString(), ficheroSalida);
                    utilidad.GrabarSalida("OK", ficheroResultado);

                }


            }
            catch (Exception ex)
            {
                //Si se ha producido algun error en el envio
                string mensaje = $"MENSAJE = Proceso cancelado o error en el envio. {ex.Message}";
                utilidad.GrabarSalida(mensaje, Parametros.ficheroResultado);
                utilidad.grabadaSalida = true;
            }




        }
        public void cargaDatosPago(DatosPagoAutoliquidacion contenidoEnvio, string ficheroEntrada)
        {
            //Prepara el guion
            utilidad.cargaDatosGuion(ficheroEntrada);

            //Instanciacion de la clase para almacenar los valores de la cabecera
            contenidoEnvio.operacion = new ElementosOperacion();
            contenidoEnvio.declaracion = new ElementosDeclaracion();
            contenidoEnvio.obligado = new ElementosObligado();
            contenidoEnvio.ingreso = new ElementosIngreso();

            //Formatear datos de la cabecera
            foreach (var elemento in utilidad.cabecera)
            {
                (atributo, valor) = utilidad.divideCadena(elemento, '=');

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

                    case "NIF":
                        contenidoEnvio.obligado.nif = valor;
                        break;

                    case "NOMBRE":
                        contenidoEnvio.obligado.nombre = valor;
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
