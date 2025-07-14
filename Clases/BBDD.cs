

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

        // ---------------------------------------------------------------------------------------------------------------------------

        // Constructor: Inicializa la cadena de conexión a la base de datos (Si en algún momento se cambia la BBDD se debe cambiar esta configuración)
       
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

        // ---------------------------------------------------------------------------------------------------------------------------

        // Obtiene las recetas desde la tabla especificada con un JOIN para incluir datos relacionados.
        // Mapea cada fila del resultado a un objeto Recetas utilizando reflexión para asignar propiedades.
        // Retorna una lista con todos los objetos Recetas cargados desde la base de datos.

        public List<Recetas> GetRecetas(string TablaRecetas)
        {
            // Lista donde guardaremos los objetos Recetas que obtengamos de la base de datos.
            List<Recetas> ListadoRecetas = new List<Recetas>();

            // Usamos "using" para que la conexión se cierre automáticamente al terminar, evitando fugas de recursos.
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos la conexión a la base de datos.
                connection.Open();

                // Aquí definimos la consulta SQL para obtener datos de la tabla especificada.
                // Usamos un JOIN para obtener también el nombre relacionado desde la tabla Materias.
                string query = $"SELECT R.ID, T2.Nombre AS ID_Father, R.nombreReceta, R.Bloqueada, R.Creada, R.Modificada, R.Eliminada FROM {TablaRecetas} R JOIN Materias T2 ON R.ID_Father = T2.ID";

                // Creamos un comando para ejecutar la consulta en la conexión abierta.
                SqlCommand command = new SqlCommand(query, connection);

                // Ejecutamos la consulta y obtenemos un lector para recorrer los resultados.
                SqlDataReader reader = command.ExecuteReader();

                // Recorremos cada fila que nos devuelve la consulta.
                while (reader.Read())
                {
                    // Creamos un nuevo objeto Recetas donde guardaremos los datos de esta fila.
                    Recetas Fila = Activator.CreateInstance<Recetas>();

                    // Recorremos cada columna de la fila para asignar su valor a la propiedad correspondiente del objeto.
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);              // Nombre de la columna actual.
                        object columnValue = reader.GetValue(i);            // Valor de la columna actual.

                        // Usamos reflexión para obtener la propiedad del objeto Recetas que tiene el mismo nombre que la columna.
                        PropertyInfo property = typeof(Recetas).GetProperty(columnName);

                        // Si la propiedad existe en el objeto
                        if (property != null)
                        {
                            // Verificamos que el valor no sea NULL en la base de dato
                            if (columnValue != DBNull.Value)
                            {
                                // Asignamos el valor de la columna a la propiedad del objeto.
                                property.SetValue(Fila, columnValue);
                            }
                            else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                // Si la propiedad acepta valores nulos, la ponemos en null
                                property.SetValue(Fila, null);
                            }
                            else
                            {
                                // Si la propiedad no acepta null pero la base de datos tiene null, mostramos un aviso
                                Console.WriteLine($"⚠️ La propiedad {property.Name} no acepta NULL pero el valor es NULL en la DB");
                            }
                        }
                    }
                    // Agregamos el objeto completo a la lista que vamos a devolver.
                    ListadoRecetas.Add(Fila);
                }
                // Cerramos el lector y la conexión cuando terminamos para liberar recursos.
                reader.Close();
                connection.Close();
            }
            // Devolvemos la lista completa con las recetas cargadas.
            return ListadoRecetas;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Obtiene cualquier tabla de la base de datos y la convierte en una lista de objetos del tipo genérico T.
        // Usa reflexión para mapear dinámicamente los campos de la base de datos a las propiedades del objeto.
        // Retorna una lista de objetos ya mapeados desde la tabla especificada.

        public List<T> GetTabla<T>(string tableName)
        {
            // Lista donde se almacenarán los objetos generados desde la tabla
            List<T> Filas = new List<T>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos la conexión a la base de datos
                connection.Open();

                // Consulta que trae todos los registros de la tabla
                string query = $"SELECT * FROM {tableName}";
                SqlCommand command = new SqlCommand(query, connection);

                // Ejecutamos la consulta y leemos los datos
                SqlDataReader reader = command.ExecuteReader();

                // Iteramos sobre cada fila de la tabla
                while (reader.Read())
                {
                    // Creamos una instancia del tipo T dinámicamente
                    T Fila = Activator.CreateInstance<T>();

                    // Recorremos cada columna del resultado
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);          // Nombre de la columna
                        object columnValue = reader.GetValue(i);        // Valor de la columna

                        // Buscamos una propiedad en el objeto T que coincida con el nombre de la columna
                        PropertyInfo property = typeof(T).GetProperty(columnName);

                        if (property != null && columnValue != DBNull.Value)
                        {
                            // Si la columna es "ID_Father", se transforma a string explícitamente
                            if (columnName == "ID_Father")
                                property.SetValue(Fila, columnValue.ToString());
                            else
                                // Se asigna el valor directamente
                                property.SetValue(Fila, columnValue);
                        }
                    }
                    // Agregamos el objeto construido a la lista
                    Filas.Add(Fila);
                }
                // Cerramos el lector de datos
                reader.Close();
            }
            // Devolvemos la lista final con los objetos mapeados
            return Filas;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Ejecuta una consulta SQL personalizada y convierte los resultados a una lista de objetos del tipo T.
        // Utiliza reflexión para mapear dinámicamente columnas a propiedades del objeto.
        // Retorna una lista de objetos mapeados desde la consulta.

        public List<T> GetDatos<T>(string query)
        {
            // Lista para almacenar los resultados mapeados
            List<T> Filas = new List<T>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos la conexión
                connection.Open();

                // Creamos el comando con la consulta
                SqlCommand command = new SqlCommand(query, connection);
                // Ejecutamos y leemos los resultados
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    // Instancia del objeto T
                    T Fila = Activator.CreateInstance<T>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);      // Nombre de columna
                        object columnValue = reader.GetValue(i);    // Valor de la columna

                        PropertyInfo property = typeof(T).GetProperty(columnName);

                        // Asignamos valor si la propiedad existe y no es nulo
                        if (property != null && columnValue != DBNull.Value)
                        {
                            property.SetValue(Fila, columnValue);
                        }
                    }
                    // Añadimos la fila mapeada a la lista
                    Filas.Add(Fila);
                }
                // Cerramos el lector
                reader.Close();
                // Cerramos la conexión
                connection.Close();
            }
            // Devolvemos la lista resultante
            return Filas;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Devuelve el ID de una fila de la tabla especificada filtrando por el nombre.
        // Utiliza una condición de búsqueda con LIKE para localizar el registro.
        // Retorna el valor entero del ID si lo encuentra, o 0 en caso contrario.

        public int GetValorTabla(string tabla, string Condicion)
        {
            // Inicializamos la variable que devolveremos
            int Valor = 0;

            // Creamos y abrimos una conexión a la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Abrimos la conexión
                connection.Open();

                // Creamos la consulta SQL para buscar el ID según el nombre
                // Usamos parámetros para evitar inyecciones SQL
                string query = $"SELECT ID FROM {tabla} WHERE Nombre LIKE '{Condicion}'";

                // Preparamos el comando SQL con la conexión y la consulta
                SqlCommand command = new SqlCommand(query, connection);

                // Ejecutamos el comando y obtenemos un lector de resultados
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    // Obtenemos el primer valor (columna ID) del resultado
                    Valor = reader.GetInt32(0);
                }
                // Cerramos el lector de datos
                reader.Close();
            }
            // Devolvemos el valor encontrado (o 0 si no hubo resultados)
            return Valor;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Este método obtiene todos los IDs relacionados con una receta desde varias tablas.
        // Parámetros:
        //   - NombresTablas: un arreglo con los nombres de las tablas donde buscar
        //   - ID_Receta: el ID de la receta que se quiere consultar
        // Devuelve:
        //   - Una lista de arrays, donde cada array contiene los IDs encontrados en una tabla

        public async Task<List<int[]>> GetListadosID(string[] NombresTablas, int ID_Receta)
        {
            // Lista principal que guardará todos los arrays de IDs
            List<int[]> Listado = new List<int[]>();

            // Abrimos la conexión a la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Apertura asincrónica
                await connection.OpenAsync();

                // Recorremos cada tabla en el arreglo
                foreach (string tabla in NombresTablas)
                {
                    // Consulta para contar cuántos registros hay con ese ID_RECETA
                    string countQuery = $"SELECT COUNT(*) FROM {tabla} WHERE ID_RECETA = {ID_Receta}";
                    SqlCommand countCommand = new SqlCommand(countQuery, connection);

                    // Devuelve un único valor (cantidad)
                    int longArray = (int)await countCommand.ExecuteScalarAsync();

                    // Consulta para obtener todos los ID de esa tabla con el ID_RECETA indicado
                    string selectQuery = $"SELECT ID FROM {tabla} WHERE ID_RECETA = {ID_Receta}";
                    SqlCommand selectCommand = new SqlCommand(selectQuery, connection);

                    // Ejecutamos la lectura
                    SqlDataReader reader = await selectCommand.ExecuteReaderAsync();

                    // Creamos un array para guardar los IDs obtenidos
                    int[] IDs = new int[longArray];
                    int contador = 0;

                    // Recorremos los resultados y guardamos cada ID en el array
                    while (await reader.ReadAsync())
                    {
                        IDs[contador] = reader.GetInt32(0);
                        contador++;
                    }
                    // Agregamos el array a la lista general
                    Listado.Add(IDs);
                    // Cerramos el lector antes de continuar con la siguiente tabla
                    reader.Close();
                }
                // Cerramos la conexión a la base de datos
                connection.Close();
            }
            // Retornamos la lista de arrays de IDs
            return Listado;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Devuelve los nombres de todos los procesos desde la tabla Procesos.
        // Agrega "Etapas" como valor inicial por defecto en el arreglo.
        // Retorna un arreglo de strings con los nombres ordenados por ID.

        public async Task<string[]> GetNombresProcesos()
        {
            List<string> nombres = new List<string>();
            nombres.Add("Etapas"); // Valor por defecto

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Consulta SQL para obtener los nombres de procesos ordenados por ID
                string query = "SELECT Nombre FROM Procesos ORDER BY ID";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();

                // Se agregan los nombres obtenidos a la lista
                while (await reader.ReadAsync())
                {
                    nombres.Add(reader.GetString(0));
                }

                reader.Close();
                connection.Close();
            }
            // Se retorna la lista como un arreglo de strings
            return nombres.ToArray();
        }

        // ---------------------------------------------------------------------------------------------------------------------------
       
        // Ejecuta una consulta SQL personalizada y devuelve una lista de materias primas.
        // Utiliza la clase MMPP para mapear cada fila del resultado.
        // Retorna la lista de objetos MMPP obtenida desde la base de datos.

        public List<MMPP> GetListadoMMP(string query)
        {
            // Se crea una lista vacía donde se guardarán los resultados
            List<MMPP> ListaMMPP = new List<MMPP>();

            // Se abre una conexión a la base de datos usando la cadena de conexión configurada
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();          // Abre la conexión

                // Se crea un comando SQL con la consulta recibida por parámetro
                SqlCommand command = new SqlCommand(query, connection);

                // Se ejecuta el comando y se obtiene un lector de datos
                SqlDataReader reader = command.ExecuteReader();

                // Se recorre cada fila del resultado
                while (reader.Read())
                {
                    MMPP MMPP = new MMPP
                    {
                        N_MateriaPrima = reader.GetInt32(0),        // Lee el primer campo (ID)
                        MateriaPrima = reader.GetString(1)          // Lee el segundo campo (Nombre)
                    };
                    // Añade el objeto a la lista
                    ListaMMPP.Add(MMPP);
                }
                // Cierra el lector de datos
                reader.Close();
            }
            // Devuelve la lista completa
            return ListaMMPP;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        #endregion

        #region Eliminar Datos

        // ---------------------------------------------------------------------------------------------------------------------------

        // Elimina registros de varias tablas en la base de datos usando sus IDs.
        // Recibe un arreglo con nombres de tablas y listas con los IDs a borrar.
        // Ejecuta un DELETE para cada ID en cada tabla indicada.

        public async Task DeleteElementsBBDD(string[] NombresTablas, List<int[]> ListadoElementos)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();               // Abre la conexión a la base de datos

                // Recorre cada tabla en el arreglo de tablas
                for (int i = 0; i < NombresTablas.Length; i++)
                {
                    // Para cada ID en la lista correspondiente de esa tabla
                    foreach (int ID in ListadoElementos[i])
                    {
                        if (ID == 0) continue;                  // Ignora IDs con valor 0
                        // Construye la consulta para eliminar el registro por ID
                        string query = $"DELETE FROM {NombresTablas[i]} WHERE ID = {ID}";
                        SqlCommand command = new SqlCommand(query, connection);
                        await command.ExecuteNonQueryAsync();   // Ejecuta la eliminación
                    }
                }
                // Cierra la conexión
                connection.Close();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Elimina datos antiguos de una receta que ya no existen en la nueva versión.
        // Compara los IDs actuales en la base de datos con los IDs recibidos en JSON.
        // Borra los registros que están en la base de datos pero no en la receta actual.

        public async Task EliminarDatos(JsonElement DatosReceta, CabeceraReceta CabeceraReceta)
        {
            // Convierte el JSON recibido a objeto manejable
            JSON json = new JSON(DatosReceta);

            // Instancia clase para comparar listas
            Utiles utiles = new Utiles();

            string[] NombresTablas = await GetNombresProcesos(); // Obtiene nombres de tablas a procesar
            List<int[]> ListadoIDs_BBDD = await GetListadosID(NombresTablas, CabeceraReceta.ID);// IDs actuales en la base de datos
            List<int[]> ListadosIDs_Receta = json.GetListadosID(NombresTablas.Length, CabeceraReceta.NumeroEtapas);// IDs de la receta nueva

            // Compara listas y obtiene IDs que existen en BBDD pero no en la receta
            List<int[]> ListIDsEliminar = utiles.CompareListArrayINT(NombresTablas.Length, ListadoIDs_BBDD, ListadosIDs_Receta);

            // Elimina de la base de datos los IDs que ya no existen en la receta
            await DeleteElementsBBDD(NombresTablas, ListIDsEliminar);
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Elimina por completo una receta y todos sus datos relacionados en varias tablas.
        // Ejecuta varias sentencias DELETE para borrar la receta y sus procesos asociados.
        // Utiliza conexiones y comandos SQL asíncronos para mayor eficiencia.

        public async Task EliminarReceta(int ID_Receta)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();// Abre conexión a la base de datos

                // Consultas para eliminar la receta y sus procesos relacionados
                string[] queries = new[]
                {
                    $"DELETE FROM Recetas WHERE ID = {ID_Receta}",
                    $"DELETE FROM ProcesoPrincipal WHERE ID_Receta = {ID_Receta}",
                    $"DELETE FROM ProcesoAgitacion WHERE ID_Receta = {ID_Receta}",
                    $"DELETE FROM ProcesoTemperatura WHERE ID_Receta = {ID_Receta}"
                };

                // Ejecuta cada consulta para eliminar los datos correspondientes
                foreach (var query in queries)
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        #endregion

        #region Actualizar Datos

        // ---------------------------------------------------------------------------------------------------------------------------

        // Inserta una nueva cabecera de receta o actualiza la existente según si ya existe en la base de datos.
        // Primero verifica si la cabecera ya está registrada, luego ejecuta el comando SQL correspondiente.

        public async Task ActualizarCabeceraReceta(CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 ConsignaProceso)
        {
            Querys query = new Querys();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();// Abre la conexión a la base de datos

                // Verifica si la cabecera de la receta ya existe
                SqlCommand commandExistencia = new SqlCommand(query.Select(4, CabeceraReceta.ID.ToString()), connection);
                int count = (int)commandExistencia.ExecuteScalar();

                //count = 0;
                // Decide si debe actualizar o insertar, dependiendo de la existencia
                string sql = count > 0
                    ? query.Update(0, CabeceraReceta, CabeceraEtapa, ConsignaProceso)  // Actualiza si existe
                    : query.Insert(0, CabeceraReceta, CabeceraEtapa, ConsignaProceso); // Inserta si no existe
                
                Console.WriteLine("Consulta SQL generada:");
                Console.WriteLine(sql);

                SqlCommand command = new SqlCommand(sql, connection);

                command.ExecuteNonQuery();// Ejecuta la consulta
                // Cierra la conexión
                connection.Close();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Inserta o actualiza la cabecera de una etapa en la base de datos.
        // Verifica si la etapa ya existe para decidir si hace un UPDATE o un INSERT.

        public async Task ActualizarCabeceraEtapa(CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 ConsignaProceso)
        {
            Querys query = new Querys();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();// Abre la conexión a la base de datos

                // Consulta para verificar si la cabecera de la etapa ya existe
                SqlCommand commandExistencia = new SqlCommand(query.Select(5, CabeceraEtapa.ID.ToString()), connection);
                int count = (int)commandExistencia.ExecuteScalar();

                // Elige entre actualizar o insertar según si existe la etapa
                string sql = count > 0
                    ? query.Update(1, CabeceraReceta, CabeceraEtapa, ConsignaProceso)   // Actualiza si existe
                    : query.Insert(1, CabeceraReceta, CabeceraEtapa, ConsignaProceso);  // Inserta si no existe

                SqlCommand command = new SqlCommand(sql, connection);

                // Ejecuta la consulta
                command.ExecuteNonQuery();

                // Cierra la conexión
                connection.Close();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Inserta consignas de proceso en la base de datos para una etapa específica.
        // Solo realiza inserciones, porque los datos anteriores se borran antes para evitar actualizaciones.

        public async Task ActualizarConsignasProceso(CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 ConsignaProceso)
        {
            Querys query = new Querys();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();  // Abre la conexión a la base de datos

                // Determina el nombre del proceso según el tipo de consigna
                string Proceso = (ConsignaProceso.Tipo == "Carga" || ConsignaProceso.Tipo == "Espera" || ConsignaProceso.Tipo == "Operador")
                    ? "Principal"
                    : ConsignaProceso.Tipo;

                short etapax = (short)CabeceraEtapa.N_Etapa;

                // Prepara la consulta para verificar si la consigna ya existe (aunque no se usa para update)
                string selectQuery = query.Select(11, ConsignaProceso.Valor.ToString(), etapax, Proceso, CabeceraReceta.ID, ConsignaProceso.Tipo, ConsignaProceso.Consigna);
                SqlCommand commandExistencia = new SqlCommand(selectQuery, connection);

                // Solo hacemos un insert ya que anteriormente se han borrado los datos para no tener que hacer un update.
                int count = (int)commandExistencia.ExecuteScalar();

                // Se omite el update porque se borraron datos antiguos antes, así que solo inserta.
                string sql = query.Insert(2, CabeceraReceta, CabeceraEtapa, ConsignaProceso, Proceso);

                // Ejecuta la inserción
                SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();

                // Cierra la conexión
                connection.Close();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Elimina una etapa específica del proceso principal para una receta dada.
        // Usa parámetros para evitar problemas de seguridad con la consulta SQL.

        public async Task EliminarDatoEtapa_PPL(short N_Etapa, int ID_Receta)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();  // Abre la conexión a la base de datos de forma asíncrona

                string query = "DELETE FROM ProcesoPrincipal WHERE N_Etapa = @N_Etapa AND ID_Receta = @ID_Receta";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Añade los parámetros para evitar inyección SQL
                    command.Parameters.AddWithValue("@N_Etapa", N_Etapa);
                    command.Parameters.AddWithValue("@ID_Receta", ID_Receta);

                    // Ejecuta la consulta para eliminar la etapa
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        #endregion
    }
}



