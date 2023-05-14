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
using Citas_.Models;
using Citas_.Filters;
using System.Data.Common;

namespace Citas_.Controllers
{
    [Sessions]
    public class HomeController : Controller
    {
        private string conexion = ConfigurationManager.ConnectionStrings["ConnDB"].ToString();

        private static List<HomeCitas> OLista = new List<HomeCitas>();
        private static List<Vehiculos> OVehiLista = new List<Vehiculos>();

        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["usuario_id"]);

            OLista.Clear();

            using (SqlConnection OConnection = new SqlConnection(conexion)) {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Citas C  INNER JOIN Vehiculos V ON V.id = C.vehiculo_id WHERE V.cliente_id = @usuario_id ", OConnection);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@usuario_id", userId);

                OConnection.Open();
                using (SqlDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        HomeCitas NewCita = new HomeCitas();

                        NewCita.Id = Convert.ToInt32(dataReader["id"]);
                        NewCita.VehiculoId = Convert.ToInt32(dataReader["vehiculo_id"]);
                        NewCita.InitDate = Convert.ToDateTime(dataReader["initdate"]);
                        NewCita.Estado = dataReader["estado"].ToString();
                        NewCita.Comentario = dataReader["comentario"].ToString();
                        if (dataReader["enddate"] != DBNull.Value)
                        {
                            NewCita.EndDate = Convert.ToDateTime(dataReader["enddate"]);
                        }

                        // Agregar la nueva cita a la lista de citas
                        OLista.Add(NewCita);
                    }
                }
            }
            return View(OLista);
        }
        //Vista
        [HttpGet]
        public ActionResult CrearCita()
        {
            ViewBag.Message = "Crear Cita";
            OVehiLista.Clear();
            int userId = Convert.ToInt32(Session["usuario_id"]);

            // Obtener los vehículos del usuario y almacenarlos en ViewBag
            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Vehiculos WHERE cliente_id = @usuario_id", OConnection);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@usuario_id", userId);

                OConnection.Open();
                using (SqlDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Vehiculos vehiculo = new Vehiculos();

                        vehiculo.Id = Convert.ToInt32(dataReader["id"]);
                        vehiculo.IdUsuario = Convert.ToInt32(dataReader["cliente_id"]);
                        vehiculo.Marca = dataReader["marca"].ToString();
                        vehiculo.Modelo = dataReader["modelo"].ToString();
                        vehiculo.Anio = (int)dataReader["anio"];

                        OVehiLista.Add(vehiculo);
                    }
                }
            }

            var vehiculos = OVehiLista.Where(v => v.IdUsuario == userId).ToList();

            ViewBag.Vehiculos = vehiculos;


            return View();
        }
        //Registrar
        [HttpPost]
        public ActionResult Creacion(HomeCitas OCita)
        {
            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                // Verificar si el ID del vehículo existe
                SqlCommand checkVehicleCommand = new SqlCommand("SELECT COUNT(*) FROM Vehiculos WHERE Id = @vehiculo_id", OConnection);
                checkVehicleCommand.Parameters.AddWithValue("@vehiculo_id", OCita.VehiculoId);

                OConnection.Open();
                int vehicleCount = (int)checkVehicleCommand.ExecuteScalar();

                if (vehicleCount == 0)
                {
                    OConnection.Close();

                    // El ID del vehículo no existe, devuelve un mensaje de error..
                    return Json(new { success = false, message = "Error Vehiculo no existe cree uno si aun no lo agrega" });
                }


                SqlCommand checkCommand = new SqlCommand("SELECT COUNT(*) FROM Citas WHERE vehiculo_id = @vehiculo_id AND ((@initdate BETWEEN initdate AND DATEADD(hour, 2, initdate)) OR (DATEADD(hour, 2, @initdate) BETWEEN initdate AND DATEADD(hour, 2, initdate)))", OConnection);
                checkCommand.Parameters.AddWithValue("vehiculo_id", OCita.VehiculoId);
                checkCommand.Parameters.AddWithValue("initdate", OCita.InitDate);


                int count = (int)checkCommand.ExecuteScalar();
                if (count > 0)
                {
                    OConnection.Close();
                    return Json(new { success = false, message = "Horario ocupado seleciona otra fecha y hora" });
                }
                else { 

                SqlCommand cmd = new SqlCommand("sp_CreateCita", OConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("vehiculo_id", OCita.VehiculoId);
                cmd.Parameters.AddWithValue("initdate", OCita.InitDate);
                cmd.Parameters.AddWithValue("estado", "En espera");
                cmd.Parameters.AddWithValue("comentario", OCita.Comentario ?? "");
                cmd.Parameters.AddWithValue("enddate", OCita.InitDate.AddHours(2));

                cmd.ExecuteNonQuery();
                }
                OConnection.Close();
                return Json(new { success = true, message = "Cita Realizada correctamente." });
            }
            //return RedirectToAction("Index", "Home");

        }
        [HttpGet]
        public ActionResult Editar(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            OVehiLista.Clear();
            int userId = Convert.ToInt32(Session["usuario_id"]);

            // Obtener los vehículos del usuario y almacenarlos en ViewBag
            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Vehiculos WHERE cliente_id = @usuario_id", OConnection);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@usuario_id", userId);

                OConnection.Open();
                using (SqlDataReader dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Vehiculos vehiculo = new Vehiculos();

                        vehiculo.Id = Convert.ToInt32(dataReader["id"]);
                        vehiculo.IdUsuario = Convert.ToInt32(dataReader["cliente_id"]);
                        vehiculo.Marca = dataReader["marca"].ToString();
                        vehiculo.Modelo = dataReader["modelo"].ToString();
                        vehiculo.Anio = (int)dataReader["anio"];

                        OVehiLista.Add(vehiculo);
                    }
                }
            }

            var vehiculos = OVehiLista.Where(v => v.IdUsuario == userId).ToList();

            ViewBag.Vehiculos = vehiculos;

            HomeCitas oCita = OLista.Where(c => c.Id == id).FirstOrDefault();

            return View(oCita);
        }
        [HttpPost]
        public ActionResult Editar(HomeCitas OCita)
        {
            // Validar que el vehículo exista

            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                // Verificar si el ID del vehículo existe
                SqlCommand checkVehicleCommand = new SqlCommand("SELECT COUNT(*) FROM Vehiculos WHERE Id = @vehiculo_id", OConnection);
                checkVehicleCommand.Parameters.AddWithValue("@vehiculo_id", OCita.VehiculoId);

                OConnection.Open();
                int vehicleCount = (int)checkVehicleCommand.ExecuteScalar();

                if (vehicleCount == 0)
                {
                    OConnection.Close();

                    // El ID del vehículo no existe, devuelve un mensaje de error..
                    return Json(new { success = false, message = "Error Vehiculo no existe cree uno si aun no lo agrega" });
                }


                SqlCommand checkCommand = new SqlCommand("SELECT COUNT(*) FROM Citas WHERE vehiculo_id = @vehiculo_id AND enddate > @initdate", OConnection);
                checkCommand.Parameters.AddWithValue("vehiculo_id", OCita.VehiculoId);
                checkCommand.Parameters.AddWithValue("initdate", OCita.InitDate);

                int count = (int)checkCommand.ExecuteScalar();
                if (count > 0)
                {
                    return Json(new { success = false, message = "Horario ocupado seleciona otra fecha y hora" });
                }
                else
                {

                    SqlCommand cmd = new SqlCommand("sp_EditCita", OConnection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("id", OCita.Id);
                    cmd.Parameters.AddWithValue("vehiculo_id", OCita.VehiculoId);
                    cmd.Parameters.AddWithValue("initdate", OCita.InitDate);
                    cmd.Parameters.AddWithValue("estado", "En espera");
                    cmd.Parameters.AddWithValue("comentario", OCita.Comentario ?? "");
                    cmd.Parameters.AddWithValue("enddate", OCita.InitDate.AddHours(2));

                    cmd.ExecuteNonQuery();
                }
                OConnection.Close();
                return Json(new { success = true, message = "Cambio realizado correctamente." });
            }
            //return RedirectToAction("Index", "Home");

        }


        [HttpGet]
        public ActionResult Eliminar(int? Id)
        {
            if (Id == null) { 
                return RedirectToAction("Index", "Home");
            }

            HomeCitas OCita = OLista.Where(c => c.Id == Id).FirstOrDefault();
            return View(OCita);
        }



        [HttpPost]
        public ActionResult Eliminar(string Id)
        {
            using (SqlConnection OConnection = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteCita", OConnection);
                cmd.Parameters.AddWithValue("id", Id);
                cmd.CommandType = CommandType.StoredProcedure;

                OConnection.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index", "Home");

        }



        public ActionResult Contact()
        {
            return View();
        }


        //[HttpPost]
        //public ActionResult Registrar(HomeCitas OCita)
        //{
        //    using (SqlConnection OConnection = new SqlConnection(conexion))
        //    {
        //        SqlCommand cmd = new SqlCommand("sp_", oconexion);

        //        cmd.CommandType = CommandType.StoredProcedure;
        //        OConnection.Open();
        //        cmd.ExecuteNonQuery();
        //    }
        //    return RedirectToAction("Inicio", "Home");
        //}

    }
}