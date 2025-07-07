
/*using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using GestionRecetas.Models;
using GestionRecetas.Datos;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GestionRecetas.Clases
{
    public class SQLServerManager
    {
        private readonly string connectionString;

        public SQLServerManager()
        {
            string nombreServidor = Environment.MachineName;
            string ServidorSQL = $"{nombreServidor}\\SQLEXPRESS";
            string BaseDatos = "Recetas";
            string Usuario = "sa";
            string Password = "GomezMadrid2021";
            string connectionString = $"Data Source={ServidorSQL};Initial Catalog={BaseDatos};User ID={Usuario};Password={Password};";
            this.connectionString = connectionString;
        }


        #region Obtencion Datos
        public List<Recetas> GetRecetas(string TablaRecetas)
        {
            List<Recetas> ListadoRecetas = new List<Recetas>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT R.ID, T2.Nombre AS ID_Father, R.nombreReceta, R.Bloqueada, R.Creada, R.Modificada, R.Eliminada FROM {TablaRecetas} R JOIN Materias T2 ON R.ID_Father = T2.ID";

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Recetas Fila = Activator.CreateInstance<Recetas>();


                 
                    for (int i = 0; i < reader.FieldCount; i++)
                    {

                        string columnName = reader.GetName(i);
                        object columnValue = reader.GetValue(i);

                        PropertyInfo property = typeof(Recetas).GetProperty(columnName);

                        if (property != null)
                        {
                            if (columnValue != DBNull.Value)
                            {
                                property.SetValue(Fila, columnValue);

                            }
                            else
                            {
                                // Solo asignar null directamente si la propiedad es nullable
                                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    property.SetValue(Fila, null);
                                }
                                else
                                {
                                    Console.WriteLine($"⚠️ La propiedad {property.Name} no acepta NULL pero el valor es NULL en la DB");
                                    // Aquí puedes decidir lanzar una excepción o establecer un valor por defecto
                                }
                            }
                        }
                        else
                        {
                        }
                    }
                    ListadoRecetas.Add(Fila);
                }

                reader.Close();
                connection.Close();
            }
            return ListadoRecetas;
        }
        public List<T> GetTabla<T>(string tableName)
        {
            List<T> Filas = new List<T>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT * FROM {tableName}";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    T Fila = Activator.CreateInstance<T>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        object columnValue = reader.GetValue(i);

                        PropertyInfo property = typeof(T).GetProperty(columnName);

                        if (property != null && columnValue != DBNull.Value)
                        {
                            if (columnName == "ID_Father")  // <- ID_Reactor
                            {
                                property.SetValue(Fila, columnValue.ToString());
                            }
                            else
                            {
                                property.SetValue(Fila, columnValue);
                            }

                        }
                    }

                    Filas.Add(Fila);
                }

                reader.Close();
            }

            return Filas;
        }
        public List<T> GetDatos<T>(string query)
        {
            List<T> Filas = new List<T>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                Console.WriteLine("Chequeo Query: " + query);


                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    T Fila = Activator.CreateInstance<T>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        object columnValue = reader.GetValue(i);

                        PropertyInfo property = typeof(T).GetProperty(columnName);

                        if (property != null && columnValue != DBNull.Value)
                        {
                            property.SetValue(Fila, columnValue);
                        }
                    }

                    Filas.Add(Fila);
                }
                reader.Close();
                connection.Close();
            }

            return Filas;
        }
        public int GetValorTabla(string tabla, string Condicion)
        {
            int Valor = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT ID FROM {tabla} WHERE Nombre LIKE '{Condicion}'";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Valor = reader.GetInt32(0);
                }

                reader.Close();
            }

            return Valor;
        }
        public async Task<List<int[]>> GetListadosID(string[] NombresTablas, int ID_Receta)
        {
            List<int[]> Listado = new List<int[]>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                foreach (string tabla in NombresTablas)
                {
                    string countQuery = $"SELECT COUNT(*) FROM {tabla} WHERE ID_RECETA = {ID_Receta}";
                    SqlCommand countCommand = new SqlCommand(countQuery, connection);
                    int longArray = (int)await countCommand.ExecuteScalarAsync();

                    string selectQuery = $"SELECT ID FROM {tabla} WHERE ID_RECETA = {ID_Receta}";
                    SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
                    SqlDataReader reader = await selectCommand.ExecuteReaderAsync();

                    int[] IDs = new int[longArray];
                    int contador = 0;

                    while (await reader.ReadAsync())
                    {
                        IDs[contador] = reader.GetInt32(0);
                        contador++;
                    }

                    Listado.Add(IDs);

                    reader.Close();
                }

                connection.Close();
            }

            return Listado;
        }
        public async Task<string[]> GetNombresProcesos()
        {
            List<string> nombres = new List<string>();

            nombres.Add("Etapas");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Nombre FROM Procesos ORDER BY ID";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    string nombre = reader.GetString(0); // Obtener el valor del Nombre
                    nombres.Add(nombre);
                }

                reader.Close();
                connection.Close();
            }

            return nombres.ToArray();
        }

        public List<MMPP> GetListadoMMP(string query)
        {
            List<MMPP> ListaMMPP = new List<MMPP>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    MMPP MMPP = new MMPP();
                    MMPP.MateriaPrima = reader.GetString(1);
                    MMPP.N_MateriaPrima = reader.GetInt32(0);

                    ListaMMPP.Add(MMPP);
                }

                reader.Close();
            }
            return ListaMMPP;
        }
        #endregion


        #region Eliminar Datos
        public async Task DeleteElementsBBDD(string[] NombresTablas, List<int[]> ListadoElementos)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                for (int Tabla = 0; Tabla < NombresTablas.Length; Tabla++)
                {
                    string query;
                    int LongArray = ListadoElementos[Tabla].Length;

                    for (int Elemento = 0; Elemento < LongArray; Elemento++)
                    {
                        int ID = ListadoElementos[Tabla][Elemento];

                        if (ID != 0)
                        {
                            //Console.WriteLine($"Elemento para eliminar con ID: {ID} ");
                            if (NombresTablas[Tabla] == "Etapas")
                            {
                                query = $"DELETE FROM Etapas WHERE ID = {ID}";
                            }
                            else
                            {
                                query = $"DELETE FROM {NombresTablas[Tabla]} WHERE ID = {ID}";
                            }

                            SqlCommand command = new SqlCommand(query, connection);
                            await command.ExecuteNonQueryAsync();
                        }

                    }
                }

                connection.Close();
            }
        }
        public async Task EliminarDatos(JsonElement DatosReceta, CabeceraReceta CabeceraReceta)
        {
            JSON json = new JSON(DatosReceta);

            Utiles utiles = new Utiles();

            

            //Se obtiene un listado con los nombres de las tablas de los procesos ordenados
            string[] NombresTablas = await GetNombresProcesos();

            //Obtener todos los ID de las etapas de la receta en la BBDD ordenados como el listadod e nombres de los procesos
            List<int[]> ListadoIDs_BBDD = await GetListadosID(NombresTablas, CabeceraReceta.ID);

            //Obtener todos los ID de los datos de la receta
            List<int[]> ListadosIDs_Receta = json.GetListadosID(NombresTablas.Length, CabeceraReceta.NumeroEtapas);

            // Comparar los ID de la BBDD con los de la receta y obtener listados con IDs que no existes
            List<int[]> ListIDsEliminar = utiles.CompareListArrayINT(NombresTablas.Length, ListadoIDs_BBDD, ListadosIDs_Receta);

            //Eliminar de la BBDD los elementos que ya no existan
            await DeleteElementsBBDD(NombresTablas, ListIDsEliminar);
            


        }
        public async Task EliminarReceta(int ID_Receta)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string deleteQueryReceta = $"DELETE FROM Recetas WHERE ID = {ID_Receta}";
                SqlCommand deleteCommandReceta = new SqlCommand(deleteQueryReceta, connection);
                await deleteCommandReceta.ExecuteNonQueryAsync();

                string deleteQueryPrincipal = $"DELETE FROM ProcesoPrincipal WHERE ID_Receta = {ID_Receta}";
                SqlCommand deleteCommandPrincipal = new SqlCommand(deleteQueryPrincipal, connection);
                await deleteCommandPrincipal.ExecuteNonQueryAsync();

                string deleteQueryAgitacion = $"DELETE FROM ProcesoAgitacion WHERE ID_Receta = {ID_Receta}";
                SqlCommand deleteCommandAgitacion = new SqlCommand(deleteQueryAgitacion, connection);
                await deleteCommandAgitacion.ExecuteNonQueryAsync();

                string deleteQueryTemperatura = $"DELETE FROM ProcesoTemperatura WHERE ID_Receta = {ID_Receta}";
                SqlCommand deleteCommandTemperatura = new SqlCommand(deleteQueryTemperatura, connection);
                await deleteCommandTemperatura.ExecuteNonQueryAsync();
            }
        }
        #endregion


        #region Actualizar Datos
        public async Task ActualizarCabeceraReceta(CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 ConsignaProceso)
        {

        Querys query = new Querys();
            

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
               
                connection.Open();
                
                // Comprobar si la receta existe
                SqlCommand commandExistencia = new SqlCommand(query.Select(4, (CabeceraReceta.ID).ToString()), connection);
                
                int count = (int)commandExistencia.ExecuteScalar();
                
                if (count > 0)
                {
                    
                    // Actualizar la receta existente con la fecha
                    SqlCommand commandActualizar = new SqlCommand(query.Update(0, CabeceraReceta, CabeceraEtapa, ConsignaProceso), connection);
                    
                    commandActualizar.ExecuteNonQuery();
                    
                }
                else
                {
                    // Insertar una nueva receta con la fecha creada
                    
                    SqlCommand commandInsertar = new SqlCommand(query.Insert(0, CabeceraReceta, CabeceraEtapa, ConsignaProceso), connection);
                    
                    
                    commandInsertar.ExecuteNonQuery();
                    
                }

                connection.Close();
            }

        }
        public async Task ActualizarCabeceraEtapa(CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 ConsignaProceso)
        {
            
            Querys query = new Querys(); 



            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Comprobar si la etapa existe
                SqlCommand commandExistencia = new SqlCommand(query.Select(5, (CabeceraEtapa.ID).ToString()), connection);
                int count = (int)commandExistencia.ExecuteScalar();

                if (count > 0)
                {
                    // Actualizar la etapa existente
                    SqlCommand commandActualizar = new SqlCommand(query.Update(1, CabeceraReceta, CabeceraEtapa, ConsignaProceso), connection);
                    commandActualizar.ExecuteNonQuery();
                }
                else
                {
                    // Insertar una nueva etapa
                    SqlCommand commandInsertar = new SqlCommand(query.Insert(1, CabeceraReceta, CabeceraEtapa, ConsignaProceso), connection);

                    commandInsertar.ExecuteNonQuery();
                }

                connection.Close();
            }

        }

        
        public async Task ActualizarConsignasProceso(CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 ConsignaProceso)
        {
            Querys query = new Querys();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string Proceso;
                if(ConsignaProceso.Tipo == "Carga" || ConsignaProceso.Tipo == "Espera" || ConsignaProceso.Tipo == "Operador") // <-- Se ha añadido Operador, ya que estaba dando error al no existir la tabla en la bbdd
                {
                    Proceso = "Principal";
                }
                else
                {
                    Proceso = ConsignaProceso.Tipo;
                }

                // Comprobar si la consigna existe

                short etapax = (short)CabeceraEtapa.N_Etapa;
                string selectQuery = query.Select(11, ((ConsignaProceso.Valor).ToString()), etapax, Proceso, CabeceraReceta.ID, ConsignaProceso.Tipo, ConsignaProceso.Consigna) ;

                Console.WriteLine($"DEBUG Select existencia: {selectQuery}");
                SqlCommand commandExistencia = new SqlCommand(selectQuery, connection);


                int count = (int)commandExistencia.ExecuteScalar();

                count = 0;
                Console.WriteLine($"Existe: {count}");

                if (count > 0)
                {
                    string updateQuery = query.Update(2, CabeceraReceta, CabeceraEtapa, ConsignaProceso, Proceso);
                   
                    Console.WriteLine($"DEBUG Update query: {updateQuery}");

                    SqlCommand commandActualizar = new SqlCommand(updateQuery, connection);
                    commandActualizar.ExecuteNonQuery();
                    
                }
                else
                {                  
                    string insertQuery = query.Insert(2, CabeceraReceta, CabeceraEtapa, ConsignaProceso, Proceso);
                    Console.WriteLine($"DEBUG Insert query: {insertQuery}");
                    SqlCommand commandInsertar = new SqlCommand(insertQuery, connection);
                    commandInsertar.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        
        public async Task EliminarDatoEtapa_PPL(short N_Etapa, int ID_Receta)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "DELETE FROM ProcesoPrincipal WHERE N_Etapa = @N_Etapa AND ID_Receta = @ID_Receta";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                    command.Parameters.AddWithValue("@ID_Receta", ID_Receta);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }



        #endregion
    }
}

*/
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using GestionRecetas.Models;
using GestionRecetas.Datos;

namespace GestionRecetas.Clases
{
    public class SQLServerManager
    {
        private readonly string connectionString;

        // Constructor: Inicializa la cadena de conexión a la base de datos (Si en algun momento se cambia la BBDD se debe cambiar esta cnonfiguración)
        public SQLServerManager()
        {
            string nombreServidor = Environment.MachineName;
            string ServidorSQL = $"{nombreServidor}\\SQLEXPRESS";
            string BaseDatos = "Recetas";
            string Usuario = "sa";
            string Password = "GomezMadrid2021";

            // Cadena de conexión completa
            this.connectionString = $"Data Source={ServidorSQL};Initial Catalog={BaseDatos};User ID={Usuario};Password={Password};";
        }

        #region Obtención de Datos

        // Obtiene las recetas desde la tabla indicada y las devuelve como una lista de objetos Recetas
        public List<Recetas> GetRecetas(string TablaRecetas)
        {
            List<Recetas> ListadoRecetas = new List<Recetas>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT R.ID, T2.Nombre AS ID_Father, R.nombreReceta, R.Bloqueada, R.Creada, R.Modificada, R.Eliminada FROM {TablaRecetas} R JOIN Materias T2 ON R.ID_Father = T2.ID";

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Recetas Fila = Activator.CreateInstance<Recetas>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        object columnValue = reader.GetValue(i);

                        PropertyInfo property = typeof(Recetas).GetProperty(columnName);

                        if (property != null)
                        {
                            if (columnValue != DBNull.Value)
                            {
                                property.SetValue(Fila, columnValue);
                            }
                            else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                property.SetValue(Fila, null);
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ La propiedad {property.Name} no acepta NULL pero el valor es NULL en la DB");
                            }
                        }
                    }

                    ListadoRecetas.Add(Fila);
                }

                reader.Close();
                connection.Close();
            }

            return ListadoRecetas;
        }

        // Obtiene cualquier tabla genérica de la base de datos y la convierte a una lista de objetos genéricos T
        public List<T> GetTabla<T>(string tableName)
        {
            List<T> Filas = new List<T>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT * FROM {tableName}";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    T Fila = Activator.CreateInstance<T>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        object columnValue = reader.GetValue(i);

                        PropertyInfo property = typeof(T).GetProperty(columnName);

                        if (property != null && columnValue != DBNull.Value)
                        {
                            if (columnName == "ID_Father")
                                property.SetValue(Fila, columnValue.ToString());
                            else
                                property.SetValue(Fila, columnValue);
                        }
                    }

                    Filas.Add(Fila);
                }

                reader.Close();
            }

            return Filas;
        }

        // Ejecuta una consulta SQL personalizada y la convierte a una lista de objetos del tipo T
        public List<T> GetDatos<T>(string query)
        {


            List<T> Filas = new List<T>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    T Fila = Activator.CreateInstance<T>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i); 
                        object columnValue = reader.GetValue(i);

                        PropertyInfo property = typeof(T).GetProperty(columnName);

                        if (property != null && columnValue != DBNull.Value)
                        {
                            property.SetValue(Fila, columnValue);
                        }
                    }

                    Filas.Add(Fila);
                }

                reader.Close();
                connection.Close();
            }

            return Filas;
        }

        // Devuelve el ID de una tabla filtrando por nombre
        public int GetValorTabla(string tabla, string Condicion)
        {
            int Valor = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT ID FROM {tabla} WHERE Nombre LIKE '{Condicion}'";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Valor = reader.GetInt32(0);
                }

                reader.Close();
            }

            return Valor;
        }

        // Obtiene todos los ID relacionados con una receta desde múltiples tablas
        public async Task<List<int[]>> GetListadosID(string[] NombresTablas, int ID_Receta)
        {
            List<int[]> Listado = new List<int[]>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                foreach (string tabla in NombresTablas)
                {
                    string countQuery = $"SELECT COUNT(*) FROM {tabla} WHERE ID_RECETA = {ID_Receta}";
                    SqlCommand countCommand = new SqlCommand(countQuery, connection);
                    int longArray = (int)await countCommand.ExecuteScalarAsync();

                    string selectQuery = $"SELECT ID FROM {tabla} WHERE ID_RECETA = {ID_Receta}";
                    SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
                    SqlDataReader reader = await selectCommand.ExecuteReaderAsync();

                    int[] IDs = new int[longArray];
                    int contador = 0;

                    while (await reader.ReadAsync())
                    {
                        IDs[contador] = reader.GetInt32(0);
                        contador++;
                    }

                    Listado.Add(IDs);

                    reader.Close();
                }

                connection.Close();
            }

            return Listado;
        }

        // Devuelve el listado de procesos definidos en la base de datos
        public async Task<string[]> GetNombresProcesos()
        {
            List<string> nombres = new List<string>();
            nombres.Add("Etapas"); // Valor por defecto

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Nombre FROM Procesos ORDER BY ID";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    nombres.Add(reader.GetString(0));
                }

                reader.Close();
                connection.Close();
            }

            return nombres.ToArray();
        }

        // Devuelve una lista de materias primas
        public List<MMPP> GetListadoMMP(string query)
        {
            List<MMPP> ListaMMPP = new List<MMPP>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    MMPP MMPP = new MMPP
                    {
                        N_MateriaPrima = reader.GetInt32(0),
                        MateriaPrima = reader.GetString(1)
                    };

                    ListaMMPP.Add(MMPP);
                }

                reader.Close();
            }

            return ListaMMPP;
        }

        #endregion

        #region Eliminar Datos

        // Elimina elementos de múltiples tablas por sus IDs
        public async Task DeleteElementsBBDD(string[] NombresTablas, List<int[]> ListadoElementos)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                for (int i = 0; i < NombresTablas.Length; i++)
                {
                    foreach (int ID in ListadoElementos[i])
                    {
                        if (ID == 0) continue;

                        string query = $"DELETE FROM {NombresTablas[i]} WHERE ID = {ID}";
                        SqlCommand command = new SqlCommand(query, connection);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                connection.Close();
            }
        }

        // Elimina los datos antiguos de la receta si ya no existen
        public async Task EliminarDatos(JsonElement DatosReceta, CabeceraReceta CabeceraReceta)
        {
            JSON json = new JSON(DatosReceta);
            Utiles utiles = new Utiles();

            string[] NombresTablas = await GetNombresProcesos();
            List<int[]> ListadoIDs_BBDD = await GetListadosID(NombresTablas, CabeceraReceta.ID);
            List<int[]> ListadosIDs_Receta = json.GetListadosID(NombresTablas.Length, CabeceraReceta.NumeroEtapas);

            List<int[]> ListIDsEliminar = utiles.CompareListArrayINT(NombresTablas.Length, ListadoIDs_BBDD, ListadosIDs_Receta);

            await DeleteElementsBBDD(NombresTablas, ListIDsEliminar);
        }

        // Elimina completamente una receta
        public async Task EliminarReceta(int ID_Receta)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string[] queries = new[]
                {
                    $"DELETE FROM Recetas WHERE ID = {ID_Receta}",
                    $"DELETE FROM ProcesoPrincipal WHERE ID_Receta = {ID_Receta}",
                    $"DELETE FROM ProcesoAgitacion WHERE ID_Receta = {ID_Receta}",
                    $"DELETE FROM ProcesoTemperatura WHERE ID_Receta = {ID_Receta}"
                };

                foreach (var query in queries)
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        #endregion

        #region Actualizar Datos

        // Inserta o actualiza la cabecera de la receta
        public async Task ActualizarCabeceraReceta(CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 ConsignaProceso)
        {
            Querys query = new Querys();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand commandExistencia = new SqlCommand(query.Select(4, CabeceraReceta.ID.ToString()), connection);
                int count = (int)commandExistencia.ExecuteScalar();

                string sql = count > 0
                    ? query.Update(0, CabeceraReceta, CabeceraEtapa, ConsignaProceso)
                    : query.Insert(0, CabeceraReceta, CabeceraEtapa, ConsignaProceso);

                SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        // Inserta o actualiza la cabecera de una etapa
        public async Task ActualizarCabeceraEtapa(CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 ConsignaProceso)
        {
            Querys query = new Querys();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand commandExistencia = new SqlCommand(query.Select(5, CabeceraEtapa.ID.ToString()), connection);
                int count = (int)commandExistencia.ExecuteScalar();

                string sql = count > 0
                    ? query.Update(1, CabeceraReceta, CabeceraEtapa, ConsignaProceso)
                    : query.Insert(1, CabeceraReceta, CabeceraEtapa, ConsignaProceso);

                SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        // Inserta o actualiza consignas de proceso
        public async Task ActualizarConsignasProceso(CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 ConsignaProceso)
        {
            Querys query = new Querys();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string Proceso = (ConsignaProceso.Tipo == "Carga" || ConsignaProceso.Tipo == "Espera" || ConsignaProceso.Tipo == "Operador")
                    ? "Principal"
                    : ConsignaProceso.Tipo;

                short etapax = (short)CabeceraEtapa.N_Etapa;

                string selectQuery = query.Select(11, ConsignaProceso.Valor.ToString(), etapax, Proceso, CabeceraReceta.ID, ConsignaProceso.Tipo, ConsignaProceso.Consigna);
                SqlCommand commandExistencia = new SqlCommand(selectQuery, connection);

                // -- Solo hacemos un insert ya que anteriormente se han borrado los datos para no tener que hacer un update.

                int count = (int)commandExistencia.ExecuteScalar();

                /*
                string sql = count > 0
                    ? query.Update(2, CabeceraReceta, CabeceraEtapa, ConsignaProceso, Proceso)
                    : query.Insert(2, CabeceraReceta, CabeceraEtapa, ConsignaProceso, Proceso);
                */

                string sql = query.Insert(2, CabeceraReceta, CabeceraEtapa, ConsignaProceso, Proceso);

                //Console.WriteLine($"[DEBUG QUERY] -> {sql}");

                SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        // Elimina una etapa del proceso principal por etapa y receta
        public async Task EliminarDatoEtapa_PPL(short N_Etapa, int ID_Receta)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "DELETE FROM ProcesoPrincipal WHERE N_Etapa = @N_Etapa AND ID_Receta = @ID_Receta";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                    command.Parameters.AddWithValue("@ID_Receta", ID_Receta);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        #endregion
    }
}



