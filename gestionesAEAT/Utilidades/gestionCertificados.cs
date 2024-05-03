using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace gestionesAEAT
{
    public class gestionCertificados
    {
        private List<X509Certificate2> certificados;

        public gestionCertificados()
        {
            certificados = new List<X509Certificate2>();
            cargarCertificados();
        }
        public void cargarCertificados() 
        {
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 cert in store.Certificates)
            {
                certificados.Add(cert);
            }
            store.Close();
        }

        public X509Certificate2 buscarSerieCertificado(string serieCertificado)
        {
            return certificados.Find(cert => cert.SerialNumber == serieCertificado);
        }


    }
}
