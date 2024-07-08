using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gestionesAEAT.Metodos
{
    public class ratificarDomicilio
    {
        string ficheroEntrada = string.Empty;
        string ficheroSalida = string.Empty;
        string ficheroSalidaConyuge = string.Empty;
        string url; //Url a la que se envian los datos
        List<string> cabecera = new List<string>(); //Bloque de datos identificados como cabecera en la entrada
        List<string> body = new List<string>(); //Bloque de datos identificados como body en la entrada
        List<string> respuesta = new List<string>(); //Bloque de datos identificados como respuesta en la entrada
        string atributo = string.Empty; //Cada una de las variables que se pasan a la AEAT
        string valor = string.Empty; //Valor del atributo que se pasa a la AEAT
        string codificacion; //Codificacion del texto que se pasa a la url
        string datosEnvio; //Datos formateados para hacer el envio a Hacienda
        string respuestaAEAT; //Contenido de la respuesta de la AEAT a la solicitud enviada
        string estadoRespuesta; //Devuelve OK si la respuesta es correcta
        bool valido = false; //Control si el domicilio esta ratificado
        bool nifConyuge = false; //Si hay que pasar tambien el NIF del conyuge
        List<string> textoEntrada = new List<string>();
        string textoSalida = string.Empty; //Texto que se grabara en el fichero de salida

        string aux; //Variable auxiliar para la grabacion de la respuesta

        Utiles utilidad = new Utiles(); //Instanciacion de las utilidades para poder usarlas
        envioAeat envio = new envioAeat();

        public void envioPeticion(string serieCertificado, string entrada, string salida, int paso, gestionCertificados instanciaCertificado)
        {
            this.ficheroEntrada = entrada; //Se pasa como parametro el fichero de entrada
            this.ficheroSalida = salida; //Se pasa como parametro el fichero de salida
            ficheroSalidaConyuge = Path.GetFileNameWithoutExtension(ficheroSalida) + "2" + Path.GetExtension(ficheroSalida);

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

                //Se cargan las variables del metodo con la lectura del metodo de utilidades
                this.url = utilidad.url;
                this.cabecera = utilidad.cabecera;
                this.body = utilidad.body;

                datosEnvio = formateoCabecera(paso); //Genera los datos a enviar (paso 1 para el titular y paso 2 para el conyuge)

                envio.envioPost(url, datosEnvio, serieCertificado, instanciaCertificado);//Metodo con certificado
                respuestaAEAT = envio.respuestaEnvioAEAT;

                if (envio.estadoRespuestaAEAT == "OK")
                {
                    aux = formateaRespuesta();
                }

                if (!valido)
                {
                    //Si no se ha ratificado el domicilio
                    string ruta = Path.ChangeExtension(ficheroSalida,"html");
                    File.WriteAllText(ruta, respuestaAEAT, Encoding.Default);
                }
            }

            catch (Exception ex)
            {
                //Si se ha producido algun error en el envio
                aux = $"MENSAJE = Proceso cancelado o error en el envio. {ex.Message}";
            }

            try
            {
                if (ficheroSalida != null)
                {
                    //Graba el fichero con la respuesta del titular
                    if (paso == 1) File.WriteAllText(ficheroSalida, aux, Encoding.Default);
                    if (paso == 2 && nifConyuge) File.WriteAllText(ficheroSalidaConyuge, aux, Encoding.Default);
                }
            }

            catch (Exception ex)
            {

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
            this.respuesta = utilidad.respuesta;
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
            for (int i = 0; i < cabecera.Count; i++)
            {
                cargaCabecera(cabecera[i]);

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
            string[] parte;
            try
            {
                parte = cadena.ToString().Split('=');
                atributo = parte[0].ToString().Trim();
                valor = parte[1].ToString().Trim();
            }

            catch (Exception ex)
            {
                //Falta el control de la posible excepcion
            }
        }
    }
}
