using gestionesAEAT.Utilidades;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;

namespace gestionesAEAT.Metodos
{
    public class descargaCSV
    {
        Utiles utilidad = Program.utilidad;
        envioAeat envio = new envioAeat();

        HttpClient cliente = new HttpClient();
        public async Task descargaDocumentoCSV()
        {
            string ficheroSalida = Parametros.ficheroSalida;
            string ficheroResultado = Parametros.ficheroResultado;
            string ficheroSalidaTxt = Path.ChangeExtension(ficheroSalida, "txt");
            string url = Parametros.urlCSV;

            try
            {
                HttpResponseMessage respuesta = await cliente.GetAsync(url);
                // Verificar que la respuesta fue exitosa
                if (respuesta.IsSuccessStatusCode)
                {
                    var tipoContenido = respuesta.Content.Headers.ContentType.MediaType;
                    // Verificar si el contenido es PDF
                    if (tipoContenido == "application/pdf")
                    {
                        // Leer el contenido en bytes y guardar como PDF
                        byte[] contenidoPdf = await respuesta.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(ficheroSalida, contenidoPdf);
                    }
                    else if (tipoContenido == "text/html")
                    {
                        // Leer el contenido como cadena de texto (HTML u otro) y guardar como HTML
                        string contenidoHtml = await respuesta.Content.ReadAsStringAsync();
                        string ficheroHtml = Path.ChangeExtension(Parametros.ficheroSalida, "html");
                        File.WriteAllText(ficheroHtml, contenidoHtml);
                        File.WriteAllText(ficheroSalidaTxt, "E00 = No se ha podido descargar el documento");
                    }

                    else
                    {
                        File.WriteAllText(ficheroSalida, "E00 = No se ha podido descargar el documento");
                    }
                }
                else
                {
                    File.WriteAllText(ficheroSalida, "E00 = No se ha podido descargar el documento");
                }

                File.WriteAllText(ficheroResultado, "OK");

            }
            catch (Exception ex)
            {
                File.WriteAllText(ficheroSalida, $"No se ha podido descargar el documento. {ex.Message}");
                File.WriteAllText(ficheroResultado, "OK");
            }
        }
    }
}
