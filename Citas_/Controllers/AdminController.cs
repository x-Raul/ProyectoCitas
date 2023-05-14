using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Citas_.Models;
using Citas_.Filters;

namespace Citas_.Controllers
{
    [AdminSession]
    public class AdminController : Controller
    {
        private string conexion = ConfigurationManager.ConnectionStrings["ConnDB"].ToString();

        private static List<UsuarioCitaModel> OLista = new List<UsuarioCitaModel>();
        private static List<HomeCitas> OCitasLista = new List<HomeCitas>();

        // GET: Admin

        public ActionResult Index()
        {
            OLista.Clear();
            List<UsuarioCitaModel> ListUM = new List<UsuarioCitaModel>();

            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT u.nombre AS NombreUsuario, u.email AS EmailUsuario, v.marca AS MarcaVehiculo,
       v.modelo AS ModeloVehiculo, v.anio AS AnioVehiculo, c.initdate AS InitDateCita,
       c.estado AS EstadoCita, c.comentario AS ComentarioCita, c.enddate AS EndDateCita,
       c.id AS IdCita
FROM Usuarios u
JOIN Vehiculos v ON u.id = v.cliente_id
JOIN Citas c ON v.id = c.vehiculo_id", OConnection);
                cmd.CommandType = CommandType.Text;
                OConnection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    UsuarioCitaModel usuarioCita = new UsuarioCitaModel();
                    usuarioCita.NombreUsuario = reader["NombreUsuario"].ToString();
                    usuarioCita.EmailUsuario = reader["EmailUsuario"].ToString();
                    usuarioCita.MarcaVehiculo = reader["MarcaVehiculo"].ToString();
                    usuarioCita.ModeloVehiculo = reader["ModeloVehiculo"].ToString();
                    usuarioCita.AnioVehiculo = Convert.ToInt32(reader["AnioVehiculo"]);
                    usuarioCita.InitDateCita = Convert.ToDateTime(reader["InitDateCita"]);
                    usuarioCita.EstadoCita = reader["EstadoCita"].ToString();
                    usuarioCita.ComentarioCita = reader["ComentarioCita"].ToString();
                    usuarioCita.EndDateCita = reader["EndDateCita"] as DateTime?;
                    usuarioCita.IdCita = (int)reader["IdCita"];




                    ListUM.Add(usuarioCita);
                }
            }

            return View(ListUM);
        }


        [HttpGet]
        public ActionResult Editar(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Admin");
            }
            UsuarioCitaModel oCita = null;
            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT u.nombre AS NombreUsuario, u.email AS EmailUsuario, v.marca AS MarcaVehiculo,
           v.modelo AS ModeloVehiculo, v.anio AS AnioVehiculo, c.initdate AS InitDateCita,
           c.estado AS EstadoCita, c.comentario AS ComentarioCita, c.enddate AS EndDateCita,
           c.id AS IdCita
    FROM Usuarios u
    JOIN Vehiculos v ON u.id = v.cliente_id
    JOIN Citas c ON v.id = c.vehiculo_id
    WHERE c.id = @id", OConnection);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", id);
                OConnection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
            oCita = new UsuarioCitaModel();
            oCita.NombreUsuario = reader["NombreUsuario"].ToString();
            oCita.EmailUsuario = reader["EmailUsuario"].ToString();
            oCita.MarcaVehiculo = reader["MarcaVehiculo"].ToString();
            oCita.ModeloVehiculo = reader["ModeloVehiculo"].ToString();
            oCita.AnioVehiculo = Convert.ToInt32(reader["AnioVehiculo"]);
            oCita.InitDateCita = Convert.ToDateTime(reader["InitDateCita"]);
            oCita.EstadoCita = reader["EstadoCita"].ToString();
            oCita.ComentarioCita = reader["ComentarioCita"].ToString();
            oCita.EndDateCita = reader["EndDateCita"] as DateTime?;
            oCita.IdCita = (int)reader["IdCita"];
                }
            }
            
            if (oCita == null)
            {
                return RedirectToAction("Index", "Admin");
            }

    return View(oCita);
        }
        [HttpPost]
        public ActionResult Editar(UsuarioCitaModel OCita)
        {
            // Validar que el vehículo exista

            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                try
                {
                    OConnection.Open();

                    SqlCommand cmd = new SqlCommand("sp_EditCitaAdmin", OConnection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("id", OCita.Id);
                    cmd.Parameters.AddWithValue("estado", OCita.Estado);
                    cmd.Parameters.AddWithValue("comentario", OCita.Comentario ?? "");
                    cmd.Parameters.AddWithValue("enddate", OCita.EndDate);

                    cmd.ExecuteNonQuery();
                    OConnection.Close();
                    return Json(new { success = true, message = "Cambio realizado correctamente." });

                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error" });

                }

            }

        }
        //return RedirectToAction("Index", "Home");

        public ActionResult Mostrar(int? id)
        {

            try
            {

                using (SqlConnection OConnection = new SqlConnection(conexion))
                {
                    OConnection.Open();

                    SqlCommand cmd = new SqlCommand("SELECT id, vehiculo_id, initdate, estado, comentario, enddate FROM Citas WHERE id = @id", OConnection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("id", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        HomeCitas cita = new HomeCitas();
                        cita.Id = Convert.ToInt32(reader["id"]);
                        cita.VehiculoId = Convert.ToInt32(reader["vehiculo_id"]);
                        cita.InitDate = Convert.ToDateTime(reader["initdate"]);
                        cita.Estado = reader["estado"].ToString();
                        cita.Comentario = reader["comentario"].ToString();
                        cita.EndDate = reader["enddate"] as DateTime?;

                        return View(cita);
                    }
                    else
                    {
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                return HttpNotFound();
            }
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Admin");
            }

            HomeCitas oCita = null;

            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Citas WHERE id = @id", OConnection);
                cmd.Parameters.AddWithValue("@id", id);
                OConnection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    oCita = new HomeCitas();
                    oCita.Id = Convert.ToInt32(reader["id"]);
                    oCita.InitDate = Convert.ToDateTime(reader["initdate"]);
                }
            }

            if (oCita == null)
            {
                return RedirectToAction("Index", "Admin");
            }

            return View(oCita);
        }

        // POST: Vehi/Delete/5
        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                using (SqlConnection OConnection = new SqlConnection(conexion))
                {
                    SqlCommand cmd = new SqlCommand("sp_DeleteCita", OConnection);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.CommandType = CommandType.StoredProcedure;

                    OConnection.Open();
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index", "Admin");
            }
            catch
            {
                return View();
            }
        }


        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult User()
        {
            return View();
        }




    }

}





