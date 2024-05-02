using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace consultaModelos.Metodos
{
    public class ratificarDomicilio
    {
        ArrayList cabecera = new ArrayList(); //Datos identificados como cabecera en la entrada
        ArrayList body = new ArrayList(); //Datos identificados como body en la entrada
        ArrayList respuesta = new ArrayList(); //Datos identificado como respuesta en la entrada
        string atributo = string.Empty; //Cada una de las variables que se pasan a la AEAT
        string valor = string.Empty; //Valor del atributo que se pasa a la AEAT
        string aux;
        string codificacion; //Codificacion del texto que se pasa a la url
        string datosEnvio; //Datos formateados para hacer el envio a Hacienda
        string url; //Url a la que se envian los datos
        string contenidoPost; //Contenido del texto a pasar
        bool valido = false; //Control si la respuesta es valida
        bool nifConyuge = false; //Si hay que pasar tambien el NIF del conyuge
        string textBoxText = string.Empty; //Variable provisional hasta que vea que hace; es el texto a pasar

        Utiles utilidad = new Utiles(); //Instanciacion de las utilidades para poder utilizarlas

        public void envioPeticion(string serieCertificado)
        {
            try
            {
                codificacion = utilidad.codificacionModelo(contenido);
                cargaDatos(contenido, codificacion, cabecera, body, respuesta, url);
                datosEnvio = generarFormulario();
                contenidoPost = envioAeat.envioPost(url, datosEnvio, serieCertificado);

            }

            catch (Exception ex)
            {

            }
        }

        private string generarFormulario()
        {
            for (int i = 0; i < cabecera.Count; i++)
            {
                cargaCabecera(cabecera[i]);
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

                //Chequeo si se pide el NIF del conyuge
                if (atributo == "NIF2" && string.IsNullOrEmpty(valor))
                {
                    atributo = "NIF"; //Se modifica para el envio a la AEAT
                    nifConyuge = true;
                }
            }

            catch (Exception ex)
            {
                //Falta el control de la posible excepcion
            }

        }

        private void cargaDatos(string datosEntrada, string codificacion, ArrayList cabecera, ArrayList body, ArrayList respuesta, string url)
        {
            string cadena;
            ArrayList lista = new ArrayList();

            using (StreamReader sr = new StreamReader(datosEntrada, Encoding.GetEncoding(codificacion)))
            {
                string line;
                do
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        lista.Add(line);
                    }
                } while (line != null);
            }

            for (int x = 0; x < lista.Count; x++)
            {
                cadena = lista[x].ToString().Trim();
                if (cadena != "")
                {
                    switch (cadena)
                    {
                        case "[url]":
                            url = cadena;
                            break;

                        case "[cabecera]":
                            cabecera.Add(cadena);
                            break;

                        case "[body]":
                            body.Add(cadena);
                            break;

                        case "[respuesta]":
                            respuesta.Add(cadena);
                            break;
                    }
                }
            }
        }
    }
}
