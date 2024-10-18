using gestionesAEAT.Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace gestionesAEAT.Metodos
{
    public class ratificarDomicilio
    {
        string atributo = string.Empty; //Cada una de las variables que se pasan a la AEAT
        string valor = string.Empty; //Valor del atributo que se pasa a la AEAT
        string datosEnvio; //Datos formateados para hacer el envio a Hacienda
        string respuestaAEAT; //Contenido de la respuesta de la AEAT a la solicitud enviada
        bool valido = false; //Control si el domicilio esta ratificado
        public bool nifConyuge = false; //Si hay que pasar tambien el NIF del conyuge
        List<string> textoEntrada = new List<string>();

        string mensaje; //Variable auxiliar para la grabacion de la respuesta

        Utiles utilidad = Program.utilidad; //Instanciacion de las utilidades para poder usarlas
        envioAeat envio = new envioAeat();


        public void envioPeticion(int paso)
        {
            string serieCertificado = Parametros.serieCertificado;
            string ficheroEntrada = Parametros.ficheroEntrada;
            string ficheroSalida = Parametros.ficheroSalida;

            string rutaSalida = Path.GetDirectoryName(ficheroEntrada);
            string ficheroSalidaConyuge = Path.Combine(rutaSalida, Path.GetFileNameWithoutExtension(ficheroSalida) + "2" + Path.GetExtension(ficheroSalida));

            //Borrar los ficheros si existen en la ruta pasada
            if (paso == 1)
            {
                textoEntrada = utilidad.prepararGuion(ficheroEntrada); //Solo se procesa en el paso 1 ya que se almacena el contenido en una variable de clase
                utilidad.borrarFicheros(ficheroSalida);
            }

            if (paso == 2) utilidad.borrarFicheros(ficheroSalidaConyuge);

            try
            {
                if (paso == 1) utilidad.cargaDatosGuion(textoEntrada);

                datosEnvio = formateoCabecera(paso); //Genera los datos a enviar (paso 1 para el titular y paso 2 para el conyuge)

                envio.envioPost(utilidad.url, datosEnvio, serieCertificado, "form");//Metodo con certificado
                respuestaAEAT = envio.respuestaEnvioAEAT;

                if (envio.estadoRespuestaAEAT == "OK")
                {
                    mensaje = formateaRespuesta();
                }

                if (!valido)
                {
                    //Si no se ha ratificado el domicilio
                    string ruta = Path.ChangeExtension(ficheroSalida, "html");
                    File.WriteAllText(ruta, respuestaAEAT, Encoding.Default);
                }
            }

            catch (Exception ex)
            {
                //Si se ha producido algun error en el envio
                mensaje = $"MENSAJE = Proceso cancelado o error en el envio. {ex.Message}";
                utilidad.GrabarSalida(mensaje, Parametros.ficheroResultado);
                utilidad.grabadaSalida = true;
            }

            try
            {
                //Graba el fichero con la respuesta del titular
                if (paso == 1) File.WriteAllText(ficheroSalida, mensaje, Encoding.Default);
                if (paso == 2 && nifConyuge) File.WriteAllText(ficheroSalidaConyuge, mensaje, Encoding.Default);
                File.WriteAllText(Parametros.ficheroResultado, "OK");
            }

            catch (Exception ex)
            {
                mensaje = $"MENSAJE = Se ha producido un error al grabar los ficheros de respuesta. {ex.Message}";
                utilidad.GrabarSalida(mensaje, Parametros.ficheroResultado);
                utilidad.grabadaSalida = true;
            }
        }

        private string formateaRespuesta()
        {
            //Ejemplo de respuesta valida
            //{"status":"OK","visible":"N","crashlytics":"N","respuesta":{"domicilioRatificado":true}}
            int primerCaracter;
            int inicio;
            string car;
            string valor;
            string cadena = string.Empty;
            var respuesta = utilidad.respuesta;
            for (int i = 0; i < respuesta.Count; i++)
            {
                //Busca la posicion de la palabra que se pasa en el bloque 'respuesta' del fichero de entrada
                primerCaracter = respuestaAEAT.IndexOf(respuesta[i].ToString());
                valor = ""; //Almacena el resultado de respuesta recibida

                if (primerCaracter != -1) //Si ha encontrado el texto
                {
                    inicio = primerCaracter + respuesta[i].ToString().Length + 1 + 1; //Se posiciona el inicio de la cadena despues de los dos puntos y la comilla
                    for (int x = inicio; x < respuestaAEAT.Length; x++) //Recorre todos los caracteres para localizar el final del texto con el valor
                    {
                        car = respuestaAEAT.Substring(x, 1); //Procesa uno a uno los caracteres para encontrar el primer caracter que sera una llave de cierre '}'
                        if (car == "}")
                        {
                            if (x == inicio) continue; //Si se encuentra el caracter en el inicio se salta el bucle
                            valor = respuestaAEAT.Substring(inicio, x - inicio); //Se carga el valor con el texto que va desde el inicio hasta final
                            valor = valor.Replace(":", "").Trim(); //Se eliminan los dos puntos
                            valido = true; //Se ha encontrado un valor valido
                            break; //Sale del bucle al encontrar el valor
                        }
                    }
                }
                cadena = cadena + respuesta[i] + " = " + valor;

            }
            return cadena;
        }

        private string formateoCabecera(int paso)
        {
            nifConyuge = false;
            for (int i = 0; i < utilidad.cabecera.Count; i++)
            {
                cargaCabecera(utilidad.cabecera[i]);

                if (paso == 1 && atributo == "NIF2")
                {
                    continue;
                }
                if (paso == 2)
                {
                    if (atributo == "NIF") continue;
                    if (atributo == "NIF2" && !string.IsNullOrEmpty(valor))
                    {
                        nifConyuge = true;
                        atributo = "NIF"; //Hay que modificar el atributo para pasarlo en el POST
                    }
                }
                if (i == 0)
                {
                    datosEnvio = atributo + "=" + valor;
                }
                else
                {
                    datosEnvio += "&" + atributo + "=" + valor;
                }

            }
            return datosEnvio;
        }

        private void cargaCabecera(object cadena)
        {
            try
            {
                (atributo, valor) = utilidad.divideCadena(cadena.ToString(), '=');
                atributo = atributo.Trim();
                valor = valor.Trim();
            }

            catch (Exception ex)
            {
                mensaje = $"Se ha producido un error al procesar el guion. {ex.Message}";
                utilidad.GrabarSalida(mensaje, Parametros.ficheroResultado);
                utilidad.grabadaSalida = true;
            }
        }
    }
}
