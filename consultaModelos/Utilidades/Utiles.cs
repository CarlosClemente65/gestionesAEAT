using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace consultaModelos
{
    public class Utiles
    {
        public string quitaRaros(string cadena)
        {
            List<(string, string)> caracteresReemplazo = new List<(string, string)>
            {
                ("á", "a"),
                ("é", "e"),
                ("í", "i"),
                ("ó", "o"),
                ("ú", "u"),
                ("º", "."),
                ("ª", "."),
                ("ñ", "¤"),
                ("Á", "A"),
                ("É", "E"),
                ("Í", "I"),
                ("Ó", "O"),
                ("Ú", "U"),
                ("Ñ", "¤")
            };

            foreach (var tupla in caracteresReemplazo)
            {
                cadena = cadena.Replace(tupla.Item1, tupla.Item2);
            }
            return cadena;

        }

        public string codificacionModelo(string fichero)
        {
            //Permite obtener la codificacion UTF-8 o ISO8859-1 (ascii extendido 256 bits o ansi)
            string cadena, valor;
            ArrayList lista = new ArrayList();
            int sw = 0;
            string[] arra;

            valor = "";
            using (StreamReader sr = new StreamReader(fichero))
            {
                string line = string.Empty;
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
                            sw = 1;
                            break;

                        case "[cabecera]":
                            if (cadena.Substring(1, 12) == "CODIFICACION")
                            {
                                arra = cadena.Split('=');
                                if (arra.Length > 1)
                                {
                                    valor = arra[1].ToString();
                                }
                                break;
                            }
                            break;

                        case "[body]":
                            sw = 3;
                            break;

                        case "[respuesta]":
                            sw = 4;
                            break;
                    }
                }
            }

            if (valor == "") valor = "utf-8";
            return valor.ToUpper();
        }
    }
}
