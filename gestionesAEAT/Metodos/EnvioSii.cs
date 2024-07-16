using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gestionesAEAT.Metodos
{
    public class EnvioSii
    {
        List<string> listaUrls = new List<string>();
        string ficheroUrls = string.Empty;

        public EnvioSii(string listaUrls)
        {
            this.ficheroUrls = listaUrls;
            string tipo = string.Empty;
            string valor = string.Empty;

            cargarUrls();

        }

        public void cargarUrls()
        {
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
            return listaUrls[indice];

        }
    }
}
