
using Microsoft.AspNetCore.Mvc;
using GestionRecetas.Models;
using GestionRecetas.Clases;
using GestionRecetas.Datos;
using System.Text.Json;
using System;
using Newtonsoft.Json;

namespace GestionRecetas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecetasController : ControllerBase
    {
        // ---------------------------------------------------------------------------------------------------------------------------

        // Método GET que obtiene la lista de Reactores y Recetas desde la base de datos.
        // Devuelve ambos conjuntos de datos combinados en una sola respuesta JSON.
        // Se utiliza en el endpoint: GET api/Recetas/Reactores

        [HttpGet("Reactores")]
        public IActionResult Get()
        {
            // Se inicializa la conexión a base de datos
            SQLServerManager BBDD = new SQLServerManager();

            // Obtiene datos de las tablas Reactores y Recetas
            List<object> Objeto = new List<object>();
            List<Reactores> Reactores = BBDD.GetTabla<Reactores>("Reactores");
            List<Recetas> Recetas = BBDD.GetRecetas("Recetas");

            // Combina ambos resultados en una sola respuesta
            Objeto.Add(Reactores);
            Objeto.Add(Recetas);

            // Retorna los datos con código HTTP 200
            return Ok(Objeto);
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Método GET que obtiene la lista de Materias y Recetas desde la base de datos.
        // Devuelve ambas listas agrupadas en un objeto anónimo.
        // Endpoint expuesto: GET api/Recetas/Materias

        [HttpGet("Materias")]
        public IActionResult Get_Materias()
        {
            // Inicializa la conexión a la base de datos
            SQLServerManager BBDD = new SQLServerManager();

            // Obtiene datos de las tablas Materias y Recetas
            List<Materias> Materias = BBDD.GetTabla<Materias>("Materias");
            List<Recetas> Recetas = BBDD.GetRecetas("Recetas");

            // Agrupa ambos conjuntos de datos en un objeto anónimo
            var Aux_Objeto = new { Materias, Recetas };

            // Retorna el objeto con código HTTP 200
            return Ok(Aux_Objeto);
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Método GET que devuelve la receta completa con sus etapas y procesos según el nombre dado.
        // Construye una estructura jerárquica con cabecera, etapas y consignas.
        // Endpoint: GET api/Recetas/{NombreReceta}

        [HttpGet("{NombreReceta}")]
        public IActionResult Get(string NombreReceta)
        {
            short NumeroEtapas;
            int NumeroProcesos;

            // Inicializa conexión a base de datos y consultas
            SQLServerManager BBDD = new SQLServerManager();
            Querys Query = new Querys();
            List<object> Receta = new List<object>();

            // Obtiene la cabecera principal de la receta
            List<CabeceraReceta> CabeceraReceta = BBDD.GetDatos<CabeceraReceta>(Query.Select(0, NombreReceta));

            // Obtiene la lista de procesos definidos en el sistema
            List<Procesos> ListaProcesos = BBDD.GetDatos<Procesos>(Query.Select(3));
            Receta.Add(CabeceraReceta);

            NumeroEtapas = CabeceraReceta[0].NumeroEtapas;
            NumeroProcesos = ListaProcesos.Count();

            // Recorre cada etapa de la receta
            for (short NumeroEtapa = 1; NumeroEtapa <= NumeroEtapas; NumeroEtapa++)
            {
                List<object> Etapa = new List<object>();
                Receta.Add(Etapa);

                // Obtiene la cabecera de la etapa actual
                List<CabeceraEtapa> CabeceraEtapa = BBDD.GetDatos<CabeceraEtapa>(Query.Select(1, NombreReceta, NumeroEtapa));
                Etapa.Add(CabeceraEtapa);

                // Recorre todos los procesos y añade sus consignas a la etapa
                for (short NumeroProceso = 1; NumeroProceso <= NumeroProcesos; NumeroProceso++)
                {
                    List<CsgProceso> ObjetosEtapa = BBDD.GetDatos<CsgProceso>(
                        Query.Select(2, NombreReceta, NumeroEtapa, ListaProcesos[NumeroProceso - 1].Nombre));
                    Etapa.Add(ObjetosEtapa);
                }
            }

            // Se retorna la receta estructurada
            return Ok(Receta); 
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Devuelve el listado de materias primas asociadas a un reactor específico.
        // Utiliza el nombre del reactor para filtrar los datos desde la base de datos.
        // Endpoint: GET api/Recetas/ListadoMMPP_Reactor/{NombreReactor}/{ID_Father}

        [HttpGet("ListadoMMPP_Reactor/{NombreReactor}/{ID_Father}")]
        public IActionResult Get_MMPP_Reactor(string NombreReactor, int ID_Reactor)
        {
            SQLServerManager BBDD = new SQLServerManager();

            // Consulta SQL para obtener materias primas vinculadas al reactor por nombre
            string query = $@"
                SELECT N_MateriaPrima, MateriaPrima
                FROM MateriasPrimas
                WHERE ID_Father = (SELECT ID FROM Reactores WHERE Nombre like '{NombreReactor}')";

            // Ejecuta la consulta y devuelve la lista resultante
            List<MMPP> MateriasPrimas = BBDD.GetListadoMMP(query);

            // Retorna las materias primas encontradas
            return Ok(MateriasPrimas);
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Devuelve el listado de materias primas asociadas a una materia específica.
        // Utiliza el nombre de la materia para filtrar desde la base de datos.
        // Endpoint: GET api/Recetas/ListadoMMPP_Materias/{NombreMateria}/{ID_Father}

        [HttpGet("ListadoMMPP_Materias/{NombreMateria}/{ID_Father}")]
        public IActionResult Get_MMPP_Materia(string NombreMateria, int ID_Reactor)
        {
            SQLServerManager BBDD = new SQLServerManager();

            // Consulta SQL que obtiene materias primas según el nombre de la materia
            string query = $@"
                SELECT N_MateriaPrima, MateriaPrima
                FROM MateriasPrimas
                WHERE ID_Father = (SELECT ID FROM Materias WHERE Nombre like '{NombreMateria}')";

            // Ejecuta la consulta personalizada y obtiene los resultados
            List<MMPP> MateriasPrimas = BBDD.GetListadoMMP(query);
            // Devuelve la lista como respuesta
            return Ok(MateriasPrimas);
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Recibe y guarda una receta completa en la base de datos desde un JSON.
        // Elimina cualquier dato anterior de la receta y actualiza cabeceras y consignas.
        // Endpoint: POST api/Recetas/DatosReceta/{DatosReceta}

        [HttpPost("DatosReceta/{DatosReceta}")]
        public async Task<IActionResult> Put(JsonElement DatosReceta)
        {
            Console.WriteLine("-");
            Console.WriteLine("Datos Receta");
            Console.WriteLine("-");
            SQLServerManager BBDD = new SQLServerManager();
            // Convertimos el JSON recibido a una clase manejable
            JSON json = new JSON(DatosReceta);
            
            CabeceraReceta CabeceraReceta = json.ObtenerCabeceraReceta();

            // Eliminamos la receta anterior (si existe) en la base de datos
            await BBDD.EliminarDatos(DatosReceta, CabeceraReceta);
            CabeceraEtapa CabeceraEtapa = new CabeceraEtapa();
            CsgProceso1 Consignas = new CsgProceso1();

            // Insertamos o actualizamos la cabecera de la receta
            await BBDD.ActualizarCabeceraReceta(CabeceraReceta, CabeceraEtapa, Consignas);
            short Aux_N_Etapas = 0;
            bool Deleteado = false;

            // Iteramos por cada etapa de la receta
            for (short NumeroEtapa = 1; NumeroEtapa <= CabeceraReceta.NumeroEtapas; NumeroEtapa++)
            {
                CabeceraEtapa = json.ObtenerCabeceraEtapa(NumeroEtapa);
                // Guardamos cabecera de etapa
                await BBDD.ActualizarCabeceraEtapa(CabeceraReceta, CabeceraEtapa, Consignas);
                // Eliminamos datos anteriores de la etapa
                await BBDD.EliminarDatoEtapa_PPL(NumeroEtapa, CabeceraReceta.ID);
                // Recorremos los 5 procesos posibles
                for (short NumeroProceso = 1; NumeroProceso <= 5; NumeroProceso++)
                {
                    List<CsgProceso1> ConsignasProceso = json.ObtenerConsignasProceso(NumeroEtapa, NumeroProceso);
                    foreach (var consigna in ConsignasProceso)
                    {
                        // Asegura que los datos anteriores de las etapas solo se eliminen una vez
                        if (Aux_N_Etapas != CabeceraReceta.NumeroEtapas && !Deleteado)
                        {
                            Aux_N_Etapas = CabeceraReceta.NumeroEtapas;
                            Deleteado = true;
                        }

                        // Guardamos cada consigna en base de datos
                        await BBDD.ActualizarConsignasProceso(CabeceraReceta, CabeceraEtapa, consigna);
                    }
                }
            }
            // Devolvemos la cabecera de la receta como respuesta
            return Ok(json.ObtenerCabeceraReceta());
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Elimina una receta completa de la base de datos a partir de su ID.
        // Se invoca mediante el endpoint DELETE con el ID como parámetro.
        // Endpoint: DELETE api/Recetas/EliminarReceta/{ID_Receta}

        [HttpDelete("EliminarReceta/{ID_Receta}")]
        public async Task<IActionResult> Delete(int ID_Receta)
        {
            // Inicializa el gestor de base de datos
            SQLServerManager BBDD = new SQLServerManager();

            // Llama al método que elimina la receta por ID
            await BBDD.EliminarReceta(ID_Receta);

            // Retorna una confirmación de éxito
            return Ok("Receta Eliminada");
        }

        // ---------------------------------------------------------------------------------------------------------------------------
    }
}
