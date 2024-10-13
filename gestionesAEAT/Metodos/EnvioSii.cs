using gestionesAEAT.Utilidades;
using System.IO;
using System.Text;
using System.Xml;

namespace gestionesAEAT.Metodos
{
    public class EnvioSii
    {
        //Instanciacion de las clases de envio y utilidades
        envioAeat envio = new envioAeat();
        Utiles utilidad = Program.utilidad;


        public void envioFacturas()
        {
            string ficheroFacturas = Parametros.ficheroEntrada;
            string ficheroSalida = Parametros.ficheroSalida;
            string UrlSii = Parametros.UrlSii;
            string serieCertificado = Parametros.serieCertificado;
            //Metodo para hacer el envio a la AEAT de las facturas del lote

            //Carga los datos a enviar desde el ficheroFacturas
            string datosEnvio = File.ReadAllText(ficheroFacturas);
            envio.envioPost(UrlSii, datosEnvio, serieCertificado, "xml");

            if (envio.estadoRespuestaAEAT == "OK") //Si no ha habido error en la comunicacion
            {
                string respuestaAEAT = utilidad.formateaXML(envio.respuestaEnvioAEAT);
                string pathRespuestaAEAT = Path.ChangeExtension(Parametros.ficheroSalida, "aeat");
                string respuestaDiagram = formateaXML(respuestaAEAT);
                utilidad.GrabarSalida(respuestaDiagram, ficheroSalida);
                utilidad.GrabarSalida(respuestaAEAT, pathRespuestaAEAT);
                utilidad.GrabarSalida("OK",Parametros.ficheroResultado);
            }
            else
            {
                if (!string.IsNullOrEmpty(envio.respuestaEnvioAEAT)) utilidad.GrabarSalida(envio.respuestaEnvioAEAT, ficheroSalida);
                utilidad.GrabarSalida("Problemas al conectar con el servidor de la AEAT", Parametros.ficheroResultado);
                utilidad.grabadaSalida = true;
            }
        }

        public string formateaXML(string xmlRespuesta)
        {
            //Metodo para obtener las respuestas del XML de respuesta de Hacienda segun la relacion de respuestas que se pasan en el guion.
            string[] etiquetas = Parametros.respuesta;
            StringBuilder respuestaFormateada = new StringBuilder();

            // Crea un objeto XmlDocument y carga el XML de la respuesta
            XmlDocument documento = new XmlDocument();
            documento.LoadXml(xmlRespuesta);

            //Manejador de espacios de nombres
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(documento.NameTable);
            namespaceManager.AddNamespace("env", "http://schemas.xmlsoap.org/soap/envelope/");
            namespaceManager.AddNamespace("sii", "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/SuministroInformacion.xsd");
            namespaceManager.AddNamespace("siiR", "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/RespuestaSuministro.xsd");

            //Se carga la 'listaRespuestas' dentro del nodo 'siiR:RespuestaLinea'
            XmlNodeList listaRespuestas = documento.SelectNodes("//siiR:RespuestaLinea", namespaceManager);
            //Solo se procesan las respuestas dentro del nodo 'env:Body'
            XmlNode bodyNode = documento.SelectSingleNode("//env:Body", namespaceManager);

            //Iterar sobre cada nodo de respuesta

            //Primero se procesa la cabecera
            if (bodyNode != null)
            {
                ProcesarNodos(bodyNode, etiquetas, respuestaFormateada);
            }
            return respuestaFormateada.ToString();
        }

        private StringBuilder ProcesarNodos(XmlNode nodo, string[] etiquetas, StringBuilder respuesta)
        {
            //Recorre todos los nodos hijos del nodo actual
            foreach (XmlNode child in nodo.ChildNodes)
            {
                //Excluir el nodo 'sii:Cabecera'
                if (child.Name == "siiR:Cabecera")
                {
                    continue;
                }
                //Si el nodo es un elemento y coincide con alguna etiqueta, se añade a la respuesta
                if (child.NodeType == XmlNodeType.Element)
                {
                    foreach (string etiqueta in etiquetas)
                    {
                        if (child.Name == etiqueta && child.InnerText != null) respuesta.AppendLine($"{etiqueta}={child.InnerText}");
                    }
                    if (child.HasChildNodes) ProcesarNodos(child, etiquetas, respuesta);
                }
            }
            return respuesta;
        }

    }
}
