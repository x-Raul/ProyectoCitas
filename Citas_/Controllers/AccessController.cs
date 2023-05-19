using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

using Citas_.Models;
using System.Web.Helpers;
using System.Collections.ObjectModel;
using Citas_.Filters;

namespace Citas_.Controllers
{
    public class AccessController : Controller
    {
        private string conexion = ConfigurationManager.ConnectionStrings["ConnDB"].ToString();

        // GET: Access
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Signup()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(Usuarios OUsuario)
        {
            //OUsuario.Pass = EncriptarMD5(OUsuario.Pass);
            using (SqlConnection Conn = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("sp_ValidarUsuario", Conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@email", OUsuario.Email);
                cmd.Parameters.AddWithValue("@pass", OUsuario.Pass);

                try
                {
                    Conn.Open();
                    cmd.ExecuteNonQuery();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        // Asignar los valores de los campos devueltos a las propiedades de OUsuario
                        OUsuario.Id = reader.GetInt32(0);
                        OUsuario.Nombre = reader.GetString(1);
                        OUsuario.Email = reader.GetString(2);
                        OUsuario.Tipo = reader.GetString(3);
                    }
                    reader.Close();
                    OUsuario.Id = Convert.ToInt32((cmd.ExecuteScalar()).ToString());
                }
                catch (SqlException ex)
                {
                    ViewBag.ErrorMessage = ex.Message;
                    return View();
                }
                if(OUsuario.Tipo == "admin")
                {
                    Session["admin"] = OUsuario;
                    return RedirectToAction("Index", "Admin");
                }
                else if( OUsuario.Id > 0)
                {
                    Session["usuario_id"] = OUsuario.Id;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewData["Mensaje"] = "Credenciales invalidas";
                    return View();
                }

            }
        }
        [HttpPost]
        public ActionResult Signup(Usuarios OUsuario)
        {

            if(OUsuario.Pass == OUsuario.ConfPass) {

                OUsuario.Pass = EncriptarMD5(OUsuario.Pass);
                
            }
            else
            {
                ViewData["MensajeSignUp"] = "Las contraseñas no conciden";  
                return View();
            }
            using (SqlConnection Conn = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("sp_RegistrarUsuario", Conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@nombre", OUsuario.Nombre);
                cmd.Parameters.AddWithValue("@email", OUsuario.Email);
                cmd.Parameters.AddWithValue("@pass", OUsuario.Pass);
                cmd.Parameters.AddWithValue("@tipo", "user");
                try
                {
                    Conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    ViewBag.ErrorMessage = ex.Message;
                    ViewData["MensajeSignUp"] = ex.Message;

                    //ViewData["MensajeSignUp"] = "Las contraseñas no coinciden";

                    return View();
                }

            }
            return RedirectToAction("Login", "Access");
        }
        private string EncriptarMD5(string texto)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(texto);
                var hashBytes = md5.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        public ActionResult CerrarSesion()
        {
            Session["usuario"] = null;
            Session.Clear();

            return RedirectToAction("Login", "Access");
        }
    }
}