using gestionesAEAT.Utilidades;
using System.IO;

namespace gestionesAEAT.Metodos
{
    public class descargaCSV
    {
        Utiles utilidad = Program.utilidad;
        envioAeat envio = new envioAeat();

        public void descargaPDF()
        {
            //Metodo para descargar el PDF de los modelos presentados a traves del CSV

            string ficheroSalida = Parametros.ficheroSalida;
            string ficheroResultado = Parametros.ficheroResultado;

            string csv = Parametros.csvDescarga;

            string url = @"https://www2.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.eeca.catalogo.vis.VisualizaSc"; //Url para envios reales
            string urlPre = @"https://prewww2.aeat.es/wlpl/inwinvoc/es.aeat.dit.adu.eeca.catalogo.vis.VisualizaSc"; //Url para el servicio de pruebas

            string datosEnvio = string.Empty;
            datosEnvio = $"COMPLETA=SI&ORIGEN=E&NIF=B02314169&CSV={csv}";
            envio.envioPost(url, datosEnvio, "form");//Metodo sin certificado

            //Procesa la respuesta
            if (envio.estadoRespuestaAEAT == "OK")
            {
                if (envio.respuestaEnvioAEAT.Contains("<!DOCTYPE html>"))
                {
                    //Puede llegar un html con algun tipo de error
                    string path = Path.ChangeExtension(ficheroSalida, "html");
                    File.WriteAllText(path, envio.respuestaEnvioAEAT);
                    File.WriteAllText(ficheroResultado, "E00 = El CSV no es valido");
                }
                else
                {
                    //string ficheroPDF = Path.Combine(pathSalida, respuesta.nombreFicheroPDF);
                    File.WriteAllBytes(ficheroSalida, envio.respuestaEnvioAEATBytes);
                    File.WriteAllText(ficheroResultado, "OK");
                }
            }
        }
    }
}
