using gestionesAEAT.Utilidades;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionesAEAT.Metodos
{
    public class descargaCSV
    {
        Utiles utilidad = Program.utilidad;
        envioAeat envio = new envioAeat();

        public void descargaPDF()
        {

            string ficheroEntrada = Parametros.ficheroEntrada;
            string ficheroSalida = Parametros.ficheroSalida;
            string ficheroResultado = Parametros.ficheroResultado;
            
            string respuestaXML = string.Empty;
            string csv = string.Empty;

            //Prepara los datos del guion
            utilidad.cargaDatosGuion(ficheroEntrada); //Monta en la clase Utiles las listas "cabecera", "body" y "respuesta" para luego acceder a esos datos a montar el envio

            //Carga el CSV que viene en el guion
            foreach (var elemento in utilidad.cabecera)
            {
                (string atributo, string valor) = utilidad.divideCadena(elemento, '=');
                switch (atributo)
                {
                    case "CSV":
                        csv = valor;
                        break;

                    default:
                        break;
                }
            }

            //Metodo para descargar el PDF de los modelos presentados a traves del CSV
            string url = @"https://www2.agenciatributaria.gob.es/wlpl/inwinvoc/es.aeat.dit.adu.eeca.catalogo.vis.VisualizaSc";
            string urlPre = @"https://prewww2.aeat.es/wlpl/inwinvoc/es.aeat.dit.adu.eeca.catalogo.vis.VisualizaSc";
            string datosEnvio = string.Empty;

            datosEnvio = $"COMPLETA=SI&ORIGEN=E&NIF=B02314169&CSV={csv}";
            envio.envioPost(urlPre, datosEnvio, "form");//Metodo sin certificado

            //Procesa la respuesta
            if (envio.estadoRespuestaAEAT == "OK")
            {
                //string ficheroPDF = Path.Combine(pathSalida, respuesta.nombreFicheroPDF);
                File.WriteAllBytes(ficheroSalida, envio.respuestaEnvioAEATBytes);
            }
        }
    }
}
