using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gestionesAEAT
{
    public class certificadoInfo
    {
        public string nifCertificado { get; set; }
        public string titularCertificado { get; set; }
        public DateTime fechaCertificado { get; set; }
        public string nifRepresentante { get; set; }
        public string nombreRepresentante { get; set; }
        public string serieCertificado { get; set; }
        //public string nombrePF { get; set; }
        //public string apellidoPF { get; set; }
        //public string nifPF { get; set; }
        //public string nombrePJ { get; set; }
        //public string cifPJ { get; set; }
        //public string nombreRepresentante { get; set; }
        //public string apellidoRepresentante { get; set; }
    }

    public class gestionCertificados
    {
        private List<X509Certificate2> certificados;
        private static gestionCertificados instancia;

        private gestionCertificados()
        {
            certificados = new List<X509Certificate2>();
            cargarCertificados();
        }

        // Método estático para obtener la instancia única de la clase
        public static gestionCertificados ObtenerInstancia()
        {
            if (instancia == null)
            {
                instancia = new gestionCertificados();
            }
            return instancia;
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

        public List<certificadoInfo> listaCertificados()
        {
            List<certificadoInfo> certificados = new List<certificadoInfo>();

            foreach (X509Certificate2 cert in this.certificados)
            {
                if (cert.Subject.Contains("SERIALNUMBER")) //Deben tener esto para que sean de persona fisica o juridica
                {
                    string datosSubject = cert.Subject;
                    certificadoInfo info = new certificadoInfo
                    {
                        serieCertificado = cert.SerialNumber,
                        fechaCertificado = cert.NotAfter
                    };
                    obtenerDatosSubject(datosSubject, info);
                    certificados.Add(info);
                }
            }

            certificados = ordenarCertificados(certificados, "titularCertificado", true);
            return certificados;
        }

        public List<certificadoInfo> ordenarCertificados(List<certificadoInfo> certificados, string campoOrdenacion, bool ascendente)
        {
            if (certificados == null || certificados.Count == 0)
            {
                return certificados;
            }

            switch (campoOrdenacion)
            {
                case "nifCertificado":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.nifCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.nifCertificado).ToList());
                    }
                    break;

                case "titularCertificado":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.titularCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.titularCertificado).ToList());
                    }
                    break;

                case "fechaCertificado":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.fechaCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.fechaCertificado).ToList());
                    }
                    break;


                case "nifRepresentante":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.nifRepresentante).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.nifRepresentante).ToList());
                    }
                    break;

                case "nombreRepresentante":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.nombreRepresentante).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.nombreRepresentante).ToList());
                    }
                    break;

                case "serieCertificado":
                    if (ascendente)
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderBy(certificado => certificado.serieCertificado).ToList());
                    }
                    else
                    {
                        certificados = new List<certificadoInfo>(certificados.OrderByDescending(certificado => certificado.serieCertificado).ToList());
                    }
                    break;

            }
            return certificados;

        }

        public void obtenerDatosSubject(string subject, certificadoInfo info)
        {
            bool juridica = false;
            if (subject.Contains("2.5.4.97")) juridica = true;
            string nombrePF = string.Empty; ;
            string apellidoPF = string.Empty;
            string nombrePJ = string.Empty;
            string nombreRepresentante = string.Empty;
            string apellidoRepresentante = string.Empty;
            string nifCertificado = string.Empty;


            string[] partes = subject.Split(',');
            foreach (string parte in partes)
            {
                string[] elementos = parte.Trim().Split('=');
                string elemento = string.Empty;
                string valor = string.Empty;
                if (elementos.Length == 2)
                {
                    elemento = elementos[0];
                    valor = elementos[1];
                }


                switch (elemento)
                {
                    case "G": //Nombre del titular del certificado o del representante si es juridica
                        if (juridica)
                        {
                            nombreRepresentante = valor;
                        }
                        else
                        {
                            nombrePF = valor;
                        }
                        break;

                    case "SN": //Apellido del titular del certificado o del representante si es juridica
                        if (juridica)
                        {
                            apellidoRepresentante = valor;
                        }
                        else
                        {
                            apellidoPF = valor;
                        }
                        break;

                    case "SERIALNUMBER": //NIF del titular del certificado o del representante si es juridica
                        if (juridica)
                        {
                            info.nifRepresentante = valor.Substring(6);
                        }
                        else
                        {
                            nifCertificado = valor.Substring(6);
                        }
                        break;

                    case "O": //Nombre de la sociedad
                        nombrePJ = valor;
                        break;

                    case "OID.2.5.4.97": //NIF de la sociedad
                        nifCertificado = valor.Substring(6);
                        break;
                }

                info.nifCertificado = nifCertificado;
                if (juridica)
                {
                    info.titularCertificado = nombrePJ;
                    info.nombreRepresentante = nombreRepresentante + " " + apellidoRepresentante;
                }
                else
                {
                    info.titularCertificado = nombrePF + " " + apellidoPF;
                }
            }
        }
    }
}