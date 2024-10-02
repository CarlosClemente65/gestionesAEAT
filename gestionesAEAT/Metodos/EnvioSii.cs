using gestionesAEAT.Utilidades;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace gestionesAEAT.Metodos
{
    public class EnvioSii
    {
        //Instanciacion de las clases de envio y utilidades
        envioAeat envio = new envioAeat();
        Utiles utilidad = Program.utilidad;
        



        public void envioFacturas(string ficheroFacturas, string ficheroSalida, string serieCertificado, GestionCertificados instanciaCertificado, string UrlSii)
        {
            //Metodo para hacer el envio a la AEAT de las facturas del lote

            //Carga los datos a enviar desde el ficheroFacturas
            string datosEnvio = File.ReadAllText(ficheroFacturas);
            envio.envioPost(UrlSii, datosEnvio, serieCertificado, instanciaCertificado,"xml");

            if (envio.estadoRespuestaAEAT == "OK") //Si no ha habido error en la comunicacion
            {
                string respuestaXML = utilidad.formateaXML(envio.respuestaEnvioAEAT); 
                File.WriteAllText(ficheroSalida, respuestaXML);
                utilidad.GrabarSalida("OK", Parametros.Configuracion.Parametros.ficheroResultado);
                

            }
            else
            {
                File.WriteAllText(ficheroSalida, envio.respuestaEnvioAEAT);
                utilidad.GrabarSalida("Problemas al conectar con el servidor de la AEAT", Parametros.Configuracion.Parametros.ficheroResultado);
            }
        }

    }
}
