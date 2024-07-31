using System.Collections.Generic;
using System.IO;

namespace gestionesAEAT.Metodos
{
    public class EnvioSii
    {
        //Lista con las urls a las que hacer el envio
        List<string> listaUrls = new List<string>();
        int indiceUrl = -1; //Se inicializa asi para controlar luego que se ha pasado un indice correcto
        
        //Instanciacion de las clases de envio y utilidades
        envioAeat envio = new envioAeat();
        Utiles utilidad = new Utiles();

        public EnvioSii(string ficheroUrls)
        {
            cargarUrls(ficheroUrls);
        }

        public void envioFacturas(string ficheroFacturas, string ficheroSalida, string serieCertificado, GestionCertificados instanciaCertificado, int indice)
        {
            //Metodo para hacer el envio a la AEAT de las facturas del lote
            
            //Obtiene la url a la que hacer el envio pasando el indice de la lista
            utilidad.url = urlEnvio(indice);

            //Carga los datos a enviar desde el ficheroFacturas
            string datosEnvio = File.ReadAllText(ficheroFacturas);
            envio.envioPost(utilidad.url, datosEnvio, serieCertificado, instanciaCertificado,"xml");

            if (envio.estadoRespuestaAEAT == "OK") //Si no ha habido error en la comunicacion
            {
                string respuestaXML = envio.respuestaEnvioAEAT; 
                File.WriteAllText(ficheroSalida, respuestaXML);
            }
            else
            {
                File.WriteAllText(ficheroSalida, envio.respuestaEnvioAEAT);
            }
        }

        public void cargarUrls(string ficheroUrls)
        {
            //Metodo para cargar el fichero de urls de envio a la AEAT
            var lineas = File.ReadAllLines(ficheroUrls);
            foreach (string linea in lineas)
            {
                if (!string.IsNullOrWhiteSpace(linea))
                {
                    string[] parte = linea.Split('#');

                    listaUrls.Add(parte[1]);
                }
            }
        }

        public string urlEnvio(int indice)
        {
            //Devuelve la url a la que hacer el envio segun el indice pasado
            return listaUrls[indice];
        }
    }
}
