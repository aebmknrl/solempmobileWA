using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
// using System.Web.Script.Serialization;


namespace Libreria
{
    public class DBFHelper
    {
        // Contiene el ultimo error ocurrido dentro de la clase
        public LastError LastError = new LastError();
        public string CaminoComun = "";

        // Tarea para consulta de usuarios y autenticación Bearer
        public Task<string> chkUserAndPasswordAsync(string userID, string Password)
        {
            return Task.Run(() =>
            {
                string cRet = "";
                OleDbConnection conn = null;
                try
                {

                    // Rompe el programa si SOLEMP esta bloqueado o si ocurrio un error revisando 
                    this.VerifyIsLockSOLEMP();
                    // Extrae los usuarios
                    conn = getConnection();
                    if (conn == null) { cRet = null; }
                    else
                    {
                        // Retorna el usuario 

                        OleDbCommand cmd = new OleDbCommand();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "getUser";
                        cmd.Connection = conn;
                        cmd.Parameters.Add("userName", OleDbType.Char).Value = userID;
                        cmd.Parameters.Add("Password", OleDbType.Char).Value = Password;

                        DataSet ds = new DataSet();
                        OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                        da.Fill(ds, "miTabla");

                        if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 0)
                        {
                            cRet = null;
                        }
                        else
                        {
                            cRet = "Correcto";
                        }
                    }
                }
                catch (Exception e)
                {
                    cRet = "Error " + e.Message;
                }
                finally
                {
                    // Se asegura de cerrar la conexion en caso de estar abierta
                    if (conn != null && conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
                return cRet;
            });
        }


        // Constructor de la clase
        #region Constructor
        public DBFHelper(string cCaminoComun)
        {
            this.CaminoComun = cCaminoComun;
        }
        #endregion

        // Retorna un objeto Connection para ser usado con la DB Comun
        #region getConnection()
        public OleDbConnection getConnection()
        {
            return getConnection("COMUN.DBC");
        }
        #endregion

        // Retorna un objeto Connection para ser usado con la DB indicada
        #region getConnection("Db")
        public OleDbConnection getConnection(string Db)
        {
            OleDbConnection conn = null;
            try
            {
                this.LastError.Clean();
                string dbName = this.CaminoComun+
                                Db;

                conn = new OleDbConnection("Provider=VFPOLEDB.1 ;Data Source=" + dbName);
                conn.Open();
            }
            catch (Exception e)
            {
                LastError.initWithException(e,"DBHelper","getConnection(string Db)");
            }
            return conn;
        }
        #endregion

        // Obtiene el listado de todos los usuarios
        #region getUsers()       
        public string getUsers()
        {
            

            string cRet = "";
            OleDbConnection conn = null;
            try
            {

                // Rompe el programa si SOLEMP esta bloqueado o si ocurrio un error revisando 
                this.VerifyIsLockSOLEMP();


                // Extrae los usuarios
                conn = getConnection();
                if (conn == null) { cRet = LastError.ToJSON(); }
                else
                {

                    this.LastError.Clean();
                    DataSet ds = new DataSet();
                    string sql = "select * from Usuarios";

                    OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);
                    da.Fill(ds, "miTabla");
                    cRet = ReturnOK(ClasesComunes.DataTableSerialize(ds.Tables[0]));

                }
            }
            catch (Exception e)
            {
                cRet = LastError.ToJSON(e, "DBFHelper", "getUsers()");
            }
            finally
            {
                // Se asegura de cerrar la conexion en caso de estar abierta
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return cRet;
        }
        #endregion

        // Construye la primera parte del JSON a retornar cuando todo funcion OK
        #region ReturnOK
        public string ReturnOK(string cJson)
        {
            return "{\"Result\": \"OK\",\"Data\" : " + cJson + "," + "\"Error\" : \"\"}";
        }
        #endregion

        /* Metodo que lanza las excepciones en caso de que SOLEMP se encuentre bloqueado.
        Se usa dentro de un Try...Catch para forzar a que rompa el programa y no continue */
        #region VerifyIsLockSOLEMP()
        public void VerifyIsLockSOLEMP()
        {
            int nRet = this.isLockSOLEMP();

            switch (nRet)
            {
                case -1:
                    {
                        this.LastError.goThrow("Error revisando si SOLEMP estaba bloqueado: " + LastError.ErrorMsg);
                        break;
                    }
                case 1:
                    {
                        this.LastError.goThrow("SOLEMP se sencuentra bloqueado.");
                        break;
                    }
            }
        }
        #endregion

        /* Metodo que indica si SOLEMP se encuentra bloqueado
         -1 Ocurrio un error y se debe revisar LastError
          0 SOLEMP esta correcto
          1 SOLEMP esta bloqueado */
        #region isLockSOLEMP()
        public int isLockSOLEMP()
        {
            int nRet = -1;
            OleDbConnection conn = null;
            try
            {
                conn = getConnection("");

                // Ocurrio un error obteniendo la conexion
                if (conn == null)
                {
                    throw new ArgumentException();
                }

                // Se procede a verificar si SOLEMP esta bloqueado
                this.LastError.Clean();
                DataSet ds = new DataSet();
                string sql = "select * From ConfigSBO.DBF";

                OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);
                da.Fill(ds, "miTabla");
                DataTable dt = ds.Tables[0];
                DataRow dr = dt.Rows[0];

                nRet = Convert.ToBoolean(dr["isLock"].ToString()) ? 1 : 0;

            }
            catch (Exception e)
            {
                LastError.initWithException(e, "DBFHelper", "isLockSOLEMP()");
                nRet = -1;
            }
            finally
            {
                // Se asegura de cerrar la conexion en caso de estar abierta
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return nRet;

        }
        #endregion

        // Retorna la información del usuario de hoteles y compañías **Ali Briceño**
        #region getAllUserInfo()
        public string getAllUserInfo(string userID)
        {
            string cRet = "";
            OleDbConnection conn = null;
            try
            {
                // Rompe el programa si SOLEMP esta bloqueado o si ocurrio un error revisando 
                this.VerifyIsLockSOLEMP();
                // Extrae los usuarios
                conn = getConnection();
                if (conn == null) { cRet = LastError.ToJSON(); }
                else
                {
                    this.LastError.Clean();

                    // Retorna el usuario 

                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "getUser";
                    cmd.Connection = conn;
                    cmd.Parameters.Add("userName", OleDbType.Char).Value = userID;
                    //cmd.Parameters.Add("Password", OleDbType.Char).Value = Password;

                    DataSet ds = new DataSet();
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(ds, "miTabla");

                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 0)
                    {
                        this.LastError.goThrow("Usuario o Password incorrecto!");
                    }

                    // Se serializa el Datatable con la informacion del usuario
                    string cUser = ClasesComunes.DataTableSerialize(ds.Tables[0]);

                    // Retorna los hoteles que maneja un usuario
                    OleDbCommand cmd1 = new OleDbCommand();
                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.CommandText = "getHotelsByUserID";
                    cmd1.Connection = conn;
                    cmd1.Parameters.Add("userID", OleDbType.Char).Value = userID;

                    DataSet ds1 = new DataSet();
                    OleDbDataAdapter da1 = new OleDbDataAdapter(cmd1);
                    da1.Fill(ds1, "miTabla");


                    // Retorna las Empreas que maneja un usuario
                    OleDbCommand cmd2 = new OleDbCommand();
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.CommandText = "getCompaniesByUserID";
                    cmd2.Connection = conn;
                    cmd2.Parameters.Add("userID", OleDbType.Char).Value = userID;

                    DataSet ds2 = new DataSet();
                    OleDbDataAdapter da2 = new OleDbDataAdapter(cmd2);
                    da2.Fill(ds2, "miTabla");



                    // Agrega el campo Hotels al JSON que retorna
                    string cHotels = "";

                    if (ds1.Tables[0] != null && ds1.Tables[0].Rows.Count > 0)
                    {
                        cHotels = ClasesComunes.DataTableSerialize(ds1.Tables[0]);
                    }

                    string cUnion = cUser.Substring(0, cUser.Length - 2).Trim() + ",\"Hotels\": ";

                    if (cHotels.Length == 0)
                    {
                        cUnion = cUnion + "\"\"";
                    }
                    else
                    {
                        cUnion = cUnion + cHotels;
                    }


                    // Agrega el campo Companies
                    string cCompanies = "";
                    if (ds2.Tables[0] != null && ds2.Tables[0].Rows.Count > 0)
                    {
                        cCompanies = ClasesComunes.DataTableSerialize(ds2.Tables[0]);
                    }

                    cUnion = cUnion + ",\"Companies\":";


                    if (cCompanies.Length == 0)
                    {
                        cUnion = cUnion + "\"\"";
                    }
                    else
                    {
                        cUnion = cUnion + cCompanies;
                    }



                    // Completa el JSON y construye el JSONOK
                    cUnion = cUnion + "}]";
                    cRet = ReturnOK(cUnion);

                }
            }
            catch (Exception e)
            {
                cRet = LastError.ToJSON(e, "DBFHelper", "getUserObject()");
            }
            finally
            {
                // Se asegura de cerrar la conexion en caso de estar abierta
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return cRet;
        }
        #endregion
        // Retorna un Objeto con los permisos y caracteristica de un usuario
        #region getUser()
        public string getUser(string userID, string Password)
        {

            string cRet = "";
            OleDbConnection conn = null;
            try
            {

                // Rompe el programa si SOLEMP esta bloqueado o si ocurrio un error revisando 
                this.VerifyIsLockSOLEMP();


                // Extrae los usuarios
                conn = getConnection();
                if (conn == null) { cRet = LastError.ToJSON(); }
                else
                {

                    this.LastError.Clean();

                    // Retorna el usuario 

                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "getUser";
                    cmd.Connection = conn;
                    cmd.Parameters.Add("userName", OleDbType.Char).Value = userID;
                    cmd.Parameters.Add("Password", OleDbType.Char).Value = Password;

                    DataSet ds = new DataSet();
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(ds, "miTabla");

                    if (ds.Tables[0]!=null && ds.Tables[0].Rows.Count==0)
                    {
                        this.LastError.goThrow("Usuario o Password incorrecto!");
                    }


                    // Se serializa el Datatable con la informacion del usuario
                    string cUser = ClasesComunes.DataTableSerialize(ds.Tables[0]);


                    // Retorna los hoteles que maneja un usuario
                    OleDbCommand cmd1 = new OleDbCommand();
                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.CommandText = "getHotelsByUserID";
                    cmd1.Connection = conn;
                    cmd1.Parameters.Add("userID", OleDbType.Char).Value = userID;

                    DataSet ds1 = new DataSet();
                    OleDbDataAdapter da1 = new OleDbDataAdapter(cmd1);
                    da1.Fill(ds1, "miTabla");

                    
                    // Retorna las Empreas que maneja un usuario
                    OleDbCommand cmd2 = new OleDbCommand();
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.CommandText = "getCompaniesByUserID";
                    cmd2.Connection = conn;
                    cmd2.Parameters.Add("userID", OleDbType.Char).Value = userID;

                    DataSet ds2 = new DataSet();
                    OleDbDataAdapter da2 = new OleDbDataAdapter(cmd2);
                    da2.Fill(ds2, "miTabla");


                
                    // Agrega el campo Hotels al JSON que retorna
                    string cHotels = "";

                    if (ds1.Tables[0] != null && ds1.Tables[0].Rows.Count > 0)
                    {
                        cHotels = ClasesComunes.DataTableSerialize(ds1.Tables[0]);
                    }

                    string cUnion = cUser.Substring(0, cUser.Length - 2).Trim() + ",\"Hotels\": ";

                    if (cHotels.Length==0)
                    {
                        cUnion = cUnion + "\"\"";
                    }
                    else
                    {
                        cUnion = cUnion + cHotels;
                    }


                    // Agrega el campo Companies
                    string cCompanies = "";
                    if(ds2.Tables[0] !=null && ds2.Tables[0].Rows.Count > 0)
                    {
                        cCompanies = ClasesComunes.DataTableSerialize(ds2.Tables[0]);
                    }

                    cUnion = cUnion + ",\"Companies\":";


                    if (cCompanies.Length==0)
                    {
                        cUnion = cUnion + "\"\"";
                    }
                    else
                    {
                        cUnion = cUnion + cCompanies;
                    }

                    
                    
                    // Completa el JSON y construye el JSONOK
                    cUnion = cUnion + "}]";
                    cRet = ReturnOK(cUnion);

                }
            }
            catch (Exception e)
            {
                cRet = LastError.ToJSON(e, "DBFHelper", "getUserObject()");
            }
            finally
            {
                // Se asegura de cerrar la conexion en caso de estar abierta
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return cRet;

        }
        #endregion

        // Retorna un Objeto con las Politicas de Seguridad establecidas para un usuario y una empresa
        #region getUserProfileForCompanyID(string userID, CompanyID)
        public string getUserProfileByCompanyID(string userID, string CompanyID)
        {
            string cRet = "";
            OleDbConnection conn = null;
            try
            {

                // Rompe el programa si SOLEMP esta bloqueado o si ocurrio un error revisando 
                this.VerifyIsLockSOLEMP();


                // Extrae los permisos por Usuario + Empresa
                conn = getConnection();
                if (conn == null) { cRet = LastError.ToJSON(); }
                else
                {

                    this.LastError.Clean();

                    // Retorna las Politicas de Seguridad para la Empresa

                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "getUserProfileByCompanyID";
                    cmd.Connection = conn;
                    cmd.Parameters.Add("userName", OleDbType.Char).Value = userID;
                    cmd.Parameters.Add("CompanyID", OleDbType.Char).Value = CompanyID;

                    DataSet ds = new DataSet();
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(ds, "miTabla");

                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 0)
                    {
                        this.LastError.goThrow("Usuario sin politicas de seguridad establecidas para la empresa seleccionada!");
                    }

                    cRet = ReturnOK(ClasesComunes.DataTableSerialize(ds.Tables[0]));

                }
            }
            catch (Exception e)
            {
                cRet = LastError.ToJSON(e, "DBFHelper", "getUserProfileByCompanyID()");
            }
            finally
            {
                // Se asegura de cerrar la conexion en caso de estar abierta
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return cRet;
        }
        #endregion

        // Retorna la informacion para la pantalla inicial de la App
        #region getDataForMainScreen(string userName, companyID)
        public string getDataForMainScreen(string userName, string companyID)
        {

            string cRet = "";
            OleDbConnection conn = null;
            try
            {

                // Rompe el programa si SOLEMP esta bloqueado o si ocurrio un error revisando 
                this.VerifyIsLockSOLEMP();


                // Extrae los permisos por Usuario + Empresa
                conn = getConnection();
                if (conn == null) { cRet = LastError.ToJSON(); }
                else
                {

                    this.LastError.Clean();

                    // Retorna un DataTable con la informacion de la pantalla inicial

                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "getDataForMainScreen";
                    cmd.Connection = conn;
                    cmd.Parameters.Add("userName", OleDbType.Char).Value = userName;
                    cmd.Parameters.Add("CompanyID", OleDbType.Char).Value = companyID;

                    DataSet ds = new DataSet();
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(ds, "miTabla");

                    if ( ds.Tables[0] != null && ds.Tables[0].Rows.Count == 0) 
                    {
                        this.LastError.goThrow("Usuario sin permisos para la empresa seleccionada.");
                    }
                    else
                    {
                        if(System.Convert.ToInt32(ds.Tables[0].Rows[0]["PPPendientes"].ToString()) + 
                           System.Convert.ToInt32(ds.Tables[0].Rows[0]["ORDPendientes"].ToString()) +
                           System.Convert.ToInt32(ds.Tables[0].Rows[0]["REQPendientes"].ToString()) == 0)
                        {
                            this.LastError.goThrow("No existen Programaciones de Pago.");
                        }
                    }

                    cRet = ReturnOK(ClasesComunes.DataTableSerialize(ds.Tables[0]));

                }
            }
            catch (Exception e)
            {
                cRet = LastError.ToJSON(e, "DBFHelper", "getDataForMainScreen()");
            }
            finally
            {
                // Se asegura de cerrar la conexion en caso de estar abierta
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return cRet;
        }
        #endregion

        // Retorna el conteo de las Programaciones de Pago
        #region getProgPagByStatus(string status, string companyID)
        public string getProgPagByStatus(string status, string companyID)
        {
            string cRet = "";
            OleDbConnection conn = null;
            try
            {

                // Rompe el programa si SOLEMP esta bloqueado o si ocurrio un error revisando 
                this.VerifyIsLockSOLEMP();


                // Extrae los permisos por Usuario + Empresa
                conn = getConnection();
                if (conn == null) { cRet = LastError.ToJSON(); }
                else
                {

                    this.LastError.Clean();

                    // Retorna un DataTable con la informacion de la pantalla inicial
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "getProgPagByStatus";
                    cmd.Connection = conn;
                    cmd.Parameters.Add("status", OleDbType.Char).Value = status;
                    cmd.Parameters.Add("CompanyID", OleDbType.Char).Value = companyID;

                    DataSet ds = new DataSet();
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(ds, "miTabla");

                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 0)
                    {
                        this.LastError.goThrow("No existe informacion para las Programaciones de Pago.");
                    }

                    cRet = ReturnOK(ClasesComunes.DataTableSerialize(ds.Tables[0]));

                }
            }
            catch (Exception e)
            {
                cRet = LastError.ToJSON(e, "DBFHelper", "getProgPagByStatus()");
            }
            finally
            {
                // Se asegura de cerrar la conexion en caso de estar abierta
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return cRet;
        }
        #endregion

        // Retorna el listado de Programaciones de Pago por Estatus
        #region listProgPagByStatus(string status, string companyID)
        public string listProgPagByStatus(string status, string companyID)
        {
            string cRet = "";
            OleDbConnection conn = null;
            try
            {

                // Rompe el programa si SOLEMP esta bloqueado o si ocurrio un error revisando 
                this.VerifyIsLockSOLEMP();


                // Extrae los permisos por Usuario + Empresa
                conn = getConnection();
                if (conn == null) { cRet = LastError.ToJSON(); }
                else
                {

                    this.LastError.Clean();

                    // Retorna un DataTable con la informacion de la pantalla inicial
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "listProgPagByStatus";
                    cmd.Connection = conn;
                    cmd.Parameters.Add("status", OleDbType.Char).Value = status;
                    cmd.Parameters.Add("CompanyID", OleDbType.Char).Value = companyID;

                    DataSet ds = new DataSet();
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(ds, "miTabla");

                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 0)
                    {
                        this.LastError.goThrow("No existe informacion para las Programaciones de Pago.");
                    }

                    cRet = ReturnOK(ClasesComunes.DataTableSerialize(ds.Tables[0]));

                }
            }
            catch (Exception e)
            {
                cRet = LastError.ToJSON(e, "DBFHelper", "listProgPagByStatus()");
            }
            finally
            {
                // Se asegura de cerrar la conexion en caso de estar abierta
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return cRet;
        }
        #endregion

        // Retorna el detalle de una Programacion de Pago por ID
        #region listDetailProPagByID(string idProgPag, string companyID)
        public string listDetailProPagByID(string idProgPag, string companyID)
        {
            string cRet = "";
            OleDbConnection conn = null;
            try
            {

                // Rompe el programa si SOLEMP esta bloqueado o si ocurrio un error revisando 
                this.VerifyIsLockSOLEMP();


                // Extrae los permisos por Usuario + Empresa
                conn = getConnection();
                if (conn == null) { cRet = LastError.ToJSON(); }
                else
                {

                    this.LastError.Clean();

                    // Retorna un DataTable con la informacion de la pantalla inicial
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "listDetailProPagByID";
                    cmd.Connection = conn;
                    cmd.Parameters.Add("idProgPag", OleDbType.Char).Value = idProgPag;
                    cmd.Parameters.Add("CompanyID", OleDbType.Char).Value = companyID;

                    DataSet ds = new DataSet();
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    da.Fill(ds, "miTabla");

                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 0)
                    {
                        this.LastError.goThrow("No existe informacion para las Programaciones de Pago.");
                    }

                    cRet = ReturnOK(ClasesComunes.DataTableSerialize(ds.Tables[0]));

                }
            }
            catch (Exception e)
            {
                cRet = LastError.ToJSON(e, "DBFHelper", "listDetailProPagByID()");
            }
            finally
            {
                // Se asegura de cerrar la conexion en caso de estar abierta
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return cRet;
        }
        #endregion
    }
}
