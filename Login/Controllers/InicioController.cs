using System.Web.Mvc;
using Login.Data;
using Login.Models;
using Login.Servicios;

namespace Login.Controllers
{
    public class InicioController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string correo, string clave)
        {
            var usuario = DBUsuario.Validar(correo, UtilidadServicio.ConvertirSHA256(clave));
            if (usuario != null)
            {
                if (!usuario.Confirmado)
                    ViewBag.Mensaje = $"Falta confirmar su cuenta. Se le envio un correo a {correo}";
                else if (usuario.Restablecer)
                    ViewBag.Mensaje =
                        $"Se ha solicitado restablecer su cuenta, favor revise su bandeja del correo {correo}";
                else
                    return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Mensaje = "No se encontraron coincidencias";
            }

            return View();
        }

        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registrar(UsuarioDTO user_dto)
        {
            if (user_dto.Clave != user_dto.ConfirmarClave)
            {
                ViewBag.Nombre = user_dto.Nombre;
                ViewBag.Correo = user_dto.Correo;
                ViewBag.Mensaje = "Las contraseñas no coinciden";
                return View();
            }

            if (DBUsuario.Obtener(user_dto.Correo) == null)
            {
                user_dto.Clave = UtilidadServicio.ConvertirSHA256(user_dto.Clave);
                user_dto.Token = UtilidadServicio.GenerarToken();
                user_dto.Restablecer = false;
                user_dto.Confirmado = false;
                var respuesta = DBUsuario.Registrar(user_dto);

                if (respuesta)
                {
                    var path = HttpContext.Server.MapPath("~/Plantillas/Confirmar.html");
                    var content = System.IO.File.ReadAllText(path);
                    var url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Headers["host"],
                        "/Inicio/Confirmar?token=" + user_dto.Token);

                    var htmlBody = string.Format(content, user_dto.Nombre, url);

                    var correoDto = new CorreoDTO
                    {
                        Para = user_dto.Correo,
                        Asunto = "Confirmacion de cuenta",
                        Contenido = htmlBody
                    };

                    var enviado = CorreoServicio.Enviar(correoDto);
                    ViewBag.Creado = true;
                    ViewBag.Mensaje =
                        $"Su cuenta ha sido creada. Hemos enviado un mensaje al correo {user_dto.Correo} para confirmar su cuenta";
                }
                else
                {
                    ViewBag.Mensaje = "No se pudo crear su cuenta";
                }
            }
            else
            {
                ViewBag.Mensaje = "El correo ya se encuentra registrado";
            }

            return View();
        }
        
        
        public ActionResult Confirmar(string token)
        {
            ViewBag.Respuesta = DBUsuario.Confirmar(token);
            return View();
        }
        
        public ActionResult Restablecer()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Restablecer(string correo)
        {
            UsuarioDTO usuario = DBUsuario.Obtener(correo);
            ViewBag.Correo = correo;
            if(usuario != null)
            {
                bool respuesta = DBUsuario.RestablecerActualizar(1, usuario.Clave, usuario.Token);

                if (respuesta)
                {
                    string path = HttpContext.Server.MapPath("~/Plantilla/Restablecer.html");
                    string content = System.IO.File.ReadAllText(path);
                    string url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Headers["host"], "/Inicio/Actualizar?token=" + usuario.Token);

                    string htmlBody = string.Format(content, usuario.Nombre, url);

                    CorreoDTO correoDTO = new CorreoDTO()
                    {
                        Para = correo,
                        Asunto = "Restablecer cuenta",
                        Contenido = htmlBody
                    };

                    bool enviado = CorreoServicio.Enviar(correoDTO);
                    ViewBag.Restablecido = true;
                }
                else
                {
                    ViewBag.Mensaje = "No se pudo restablecer la cuenta";
                }

            }
            else
            {
                ViewBag.Mensaje = "No se encontraron coincidencias con el correo";
            }

            return View();
        }

        public ActionResult Actualizar(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public ActionResult Actualizar(string token,string clave, string confirmarClave)
        {
            ViewBag.Token = token;
            if (clave != confirmarClave)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden";
                return View();
            }

            bool respuesta = DBUsuario.RestablecerActualizar(0, UtilidadServicio.ConvertirSHA256(clave), token);

            if (respuesta)
                ViewBag.Restablecido = true;
            else
                ViewBag.Mensaje = "No se pudo actualizar";

            return View();
        }
    }
}

