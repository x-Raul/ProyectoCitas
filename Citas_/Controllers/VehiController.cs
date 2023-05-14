using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Citas_.Models;
using System.Configuration;
using System.Data.Common;
using System.Collections;
using Citas_.Filters;
namespace Citas_.Controllers
{
    [Sessions]
    public class VehiController : Controller
    {
        private string conexion = ConfigurationManager.ConnectionStrings["ConnDB"].ToString();

        private static List<Vehiculos> OListaV = new List<Vehiculos>();
        // GET: Vehi

        public ActionResult Index()
        {
            OListaV.Clear();

            int userId = Convert.ToInt32(Session["usuario_id"]);

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
                        // Crear una nueva instancia
                        Vehiculos NewCita = new Vehiculos();
                        NewCita.Id = (int)dataReader["id"];
                        NewCita.Marca = dataReader["marca"].ToString();
                        NewCita.Modelo = dataReader["modelo"].ToString();
                        NewCita.Anio = (int)dataReader["anio"];
                        NewCita.IdUsuario = userId;

                        // Agregar la nueva cita a la lista de citas
                        OListaV.Add(NewCita);
                    }
                }
            }
            return View(OListaV);

        }



        // GET: Vehi/Create
        public ActionResult Create()
        {
            return View();
        }

        //POST: Vehi/Create
        [HttpPost]
        public ActionResult Create(Vehiculos OVehi)
        {
            int userId = Convert.ToInt32(Session["usuario_id"]);

            try
            {
                using (SqlConnection OConnection = new SqlConnection(conexion))
                {
                    OConnection.Open();
                    SqlCommand cmd = new SqlCommand("sp_CreateVehiculo", OConnection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@cliente_id", userId);
                    cmd.Parameters.AddWithValue("@marca", OVehi.Marca);
                    cmd.Parameters.AddWithValue("@modelo", OVehi.Modelo);
                    cmd.Parameters.AddWithValue("@anio", OVehi.Anio);

                    cmd.ExecuteNonQuery();
                    OConnection.Close();

                }
                return Json(new { success = true, message = "Vehiculo agregado correctamente." });


        }
            catch
            {
                return View();
            }
        }

        // GET: Vehi/Edit/
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Vehi");
            }
            Vehiculos oVhei = OListaV.Where(c => c.Id == id).FirstOrDefault();

            return View(oVhei);
        }

        // POST: Vehi/Edit/
        [HttpPost]
        public ActionResult Edit(Vehiculos OVehi)
        {
            int userId = Convert.ToInt32(Session["usuario_id"]);

            try
            {
                using (SqlConnection OConnection = new SqlConnection(conexion))
                {
                    OConnection.Open();
                    {

                        SqlCommand cmd = new SqlCommand("sp_EditVehiculo", OConnection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id", OVehi.Id);
                        cmd.Parameters.AddWithValue("@cliente_id", userId);
                        cmd.Parameters.AddWithValue("@marca", OVehi.Marca);
                        cmd.Parameters.AddWithValue("@modelo", OVehi.Modelo);
                        cmd.Parameters.AddWithValue("@anio", OVehi.Anio);

                        cmd.ExecuteNonQuery();
                    }
                    OConnection.Close();
                    return Json(new { success = true, message = "Vehiculo agregado correctamente." });
                }
            }
            catch
            {
                return Json(new { success = true, message = "Error al agregar el vehiculo." });

            }
        }

        // GET: Vehi/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Vehi");
            }

            Vehiculos OVehi = OListaV.Where(c => c.Id == id).FirstOrDefault();
            return View(OVehi);
        }

        // POST: Vehi/Delete/5
        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                using (SqlConnection OConnection = new SqlConnection(conexion))
                {
                    SqlCommand cmd = new SqlCommand("sp_DeleteVehiculo", OConnection);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.CommandType = CommandType.StoredProcedure;

                    OConnection.Open();
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index", "Vehi");
            }
            catch
            {
                return View();
            }
        }
    }
}
