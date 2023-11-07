using System;
using Login.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;


namespace Login.Servicios
{
    public static class CorreoServicio
    {
        private static string _Host = "smtp.gmail.com";
        private static int _Port = 587;
        private static string _NombreEnvia = "matticry";
        private static string _Correo = "matticry1@gmail.com";
        private static string _Clave = "oibkcqoamtuqdftg";


        public static bool Enviar(CorreoDTO correoDto)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_NombreEnvia , _Correo));
                email.To.Add(MailboxAddress.Parse(correoDto.Para));
                email.Subject = correoDto.Asunto;
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = correoDto.Contenido
                };
                var smtp = new SmtpClient();
                smtp.Connect(_Host, _Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_Correo, _Clave);
                smtp.Send(email);
                smtp.Disconnect(true);
                return true;



            }
            catch
            {
                return false;
            }
            
            
            
            
            
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
    }
}