using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Citas_.Filters;
using Citas_.Models;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Citas_.Controllers
{
    [AdminSession]
    public class UsersController : Controller
    {
        // GET: Users

        private string conexion = ConfigurationManager.ConnectionStrings["ConnDB"].ToString();

        private static List<Usuarios> OLista = new List<Usuarios>();
        public ActionResult Index()
        {
            OLista.Clear();

            int userId = Convert.ToInt32(Session["usuario_id"]);

            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Usuarios", OConnection);
                cmd.CommandType = CommandType.Text;
                OConnection.Open();
                using (SqlDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        // Crear una nueva instancia
                        Usuarios NewCita = new Usuarios();
                        NewCita.Id = (int)dataReader["id"];
                        NewCita.Nombre = dataReader["nombre"].ToString();
                        NewCita.Email = dataReader["email"].ToString();
                        NewCita.Tipo = dataReader["tipo"].ToString();

                        // 
                        OLista.Add(NewCita);
                    }
                }
            }
            return View(OLista);

        }

        // GET: Users/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Users/Create
        public ActionResult CreateUser()
        {
            return View();
        }

        // POST: Users/Create

        [HttpPost]
        public ActionResult CreateUser(Usuarios OUsuario)
        {

            if (OUsuario.Pass == OUsuario.ConfPass)
            {

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

                cmd.Parameters.AddWithValue("nombre", OUsuario.Nombre);
                cmd.Parameters.AddWithValue("email", OUsuario.Email);
                cmd.Parameters.AddWithValue("pass", OUsuario.Pass);
                cmd.Parameters.AddWithValue("tipo", OUsuario.Tipo);
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
            return RedirectToAction("Index", "Users");
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


        // GET: Users/Edit/5
        [HttpGet]
        public ActionResult EditUser(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "User");
            }
            OLista.Clear();
            //int userId = Convert.ToInt32(Session["usuario_id"]);

            // Obtener los vehículos del usuario y almacenarlos en ViewBag
            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Usuarios WHERE id = @usuario_id", OConnection);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@usuario_id", id);

                OConnection.Open();
                using (SqlDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Usuarios usr = new Usuarios();

                        usr.Id = Convert.ToInt32(dataReader["id"]);
                        usr.Nombre = dataReader["nombre"].ToString();
                        usr.Email = dataReader["Email"].ToString();
                        usr.Tipo = dataReader["Tipo"].ToString();
                        OLista.Add(usr);
                    }
                }
            }


            Usuarios OUser = OLista.Where(c => c.Id == id).FirstOrDefault();

            return View(OUser);
        }

        // POST: Users/Edit/5

        [HttpPost]
        public ActionResult EditUser(Usuarios OUser)
        {
            // Validar que ingrese la contraseñaa para hacer el cambio

            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                // Verificarloa

                OUser.ConfPass = EncriptarMD5(OUser.ConfPass);


                SqlCommand checkVehicleCommand = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE id = @Id AND pass = @Pass", OConnection);
                checkVehicleCommand.Parameters.AddWithValue("@Id", OUser.Id);
                checkVehicleCommand.Parameters.AddWithValue("@Pass", OUser.ConfPass);

                OConnection.Open();
                int usrcheckar = (int)checkVehicleCommand.ExecuteScalar();

                if (usrcheckar == 0)
                {
                    OConnection.Close();

                    // El ID del usuario no concide con la contra
                    return Json(new { success = false, message = "Error contraseñá incorrecta" });
                }

                SqlCommand cmd = new SqlCommand("sp_EditUsuario", OConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("id", OUser.Id);
                cmd.Parameters.AddWithValue("nombre", OUser.Nombre);
                cmd.Parameters.AddWithValue("email", OUser.Email);
                cmd.Parameters.AddWithValue("pass", OUser.Pass);
                cmd.Parameters.AddWithValue("tipo", OUser.Tipo);

                cmd.ExecuteNonQuery();

                OConnection.Close();
                return Json(new { success = true, message = "Cambio realizado correctamente." });
            }
            //return RedirectToAction("Index", "Users");
        }

        // GET: Users/Delete/5
        [HttpGet]
        public ActionResult DeleteUser(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Users");
            }

            Usuarios OUser = OLista.Where(c => c.Id == Id).FirstOrDefault();
            return View(OUser);
        }




        // POST: Users/Delete/5
        [HttpPost]
        public ActionResult DeleteUser(string Id)
        {
            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("spEliminarUsuarioAdmin", OConnection);
                cmd.Parameters.AddWithValue("@usuario_id", Id);
                cmd.CommandType = CommandType.StoredProcedure;

                OConnection.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index", "Users");

        }

    }
}
