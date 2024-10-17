using gestionesAEAT.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace gestionesAEAT.Metodos
{
    public class inicializaInformativas
    {
        public string MODELO { get; set; }
        public string EJERCICIO { get; set; }
        public string PERIODO { get; set; }
        public string NDC { get; set; }
        public string IDIOMA { get; set; }
        public string NUMBLOQUES { get; set; }
        public string CODIFICACION { get; set; }

    }

    public class respuestaInicializa
    {
        public string IDENVIO { get; set; }
        public string ESTADO { get; set; }
        public string SIGBLOQUE { get; set; }
        public string CODIGO { get; set; }
        public string MENSAJE { get; set; }

    }

    public class envioInformativas
    {
        public string IDENVIO { get; set; }
        public string NUMBLOQUE { get; set; }
        public string CODIFICACION { get; set; }
    }

    public class respuestaEnvio
    {
        public string IDENVIO { get; set; }
        public string ESTADO { get; set; }
        public string SIGBLOQUE { get; set; }
        public string CODIGO { get; set; }
        public string MENSAJE { get; set; }
        public string TOTALT2OK { get; set; }
        public string TOTALT2KO { get; set; }
        public string BLOQUET2OK { get; set; }
        public string BLOQUET2KO { get; set; }
        public string AVISOS { get; set; }
    }

    public class presentaInformativas
    {
        public string IDENVIO { get; set; }
        public string FIRNIF { get; set; }
        public string FIRNOMBRE { get; set; }
        public string FIR { get; set; }

    }

    public class respuestaPresenta
    {
        public string IDENVIO { get; set; }
        public string ESTADO { get; set; }
        public string NUMEROREGISTROS { get; set; }
        public string CSV { get; set; }
        public string EXPEDIENTE { get; set; }
        public string CODIGO { get; set; }
        public string MENSAJE { get; set; }

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
                        instanciaClase = new inicializaInformativas();

                        break;

                    case "presentacion":
                        instanciaClase = new inicializaInformativas();

                        break;
                }

                // Asignación de los valores a las propiedades de la clase usando reflexión
                AsignarValoresClase(instanciaClase, utilidad.cabecera);

            }

            catch (Exception ex)
            {

            }
        }

        public void AsignarValoresClase(object instanciaClase, List<string> listaValores)
        {
            /*Esta clase no tengo claro si es necesaria
            En el programa de david recorre los elementos de la 'cabecera' y crea un request del siguiente modo
                For x = 0 To Cabecera.Count - 1 ' preparamos los headers
                    titulo = ""
                    valor = ""
                    saca_header(Cabecera(x), titulo, valor)
                    request.Headers(titulo) = valor  ' ejemplo request.Headers("MODELO") = "180"
                Next

            Pero esto esta despues de todo el proceso de asignacion de la configuracion del envio
            */
            foreach (var linea in listaValores)
            {
                string[] partes = linea.Split('=');
                if (partes.Length == 2)
                {
                    string nombre = partes[0].Trim(); // Nombre de la propiedad
                    string valor = partes[1].Trim();  // Valor de la propiedad

                    // Obtener el tipo de la clase instanciada
                    Type tipoClase = instanciaClase.GetType();

                    // Buscar si la clase tiene una propiedad que coincida con el nombre
                    var propiedad = tipoClase.GetProperty(nombre);

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
    }
}
