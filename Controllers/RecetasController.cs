
using Microsoft.AspNetCore.Mvc;
using GestionRecetas.Models;
using GestionRecetas.Clases;
using GestionRecetas.Datos;
using System.Text.Json;
using System;
using Newtonsoft.Json;

/*
namespace GestionRecetas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecetasController : ControllerBase
    {

        // GET: api/Recetas
        [HttpGet("Reactores")]
        public IActionResult Get()
        {
            //Console.WriteLine("DEBUG -> 0");
            SQLServerManager BBDD = new SQLServerManager();

            List<object> Objeto = new List<object>();
            List<Reactores> Reactores = BBDD.GetTabla<Reactores>("Reactores");
            List<Recetas> Recetas = BBDD.GetRecetas("Recetas");

            Objeto.Add(Reactores);
            Objeto.Add(Recetas);
            //Console.WriteLine("DEBUG -> 1");
            return Ok(Objeto);
        }

        // GET: api/Recetas
        [HttpGet("Materias")]
        public IActionResult Get_Materias()
        {
            //Console.WriteLine("DEBUG -> 10");
            SQLServerManager BBDD = new SQLServerManager();

            List<object> Objeto = new List<object>();
            List<Materias> Materias = BBDD.GetTabla<Materias>("Materias");
            
            List<Recetas> Recetas = BBDD.GetRecetas("Recetas");

            //Console.WriteLine(JsonSerializer.Serialize(Recetas));

            //Console.WriteLine($"Materias obtenidas: {Materias.Count}");
            //Console.WriteLine($"Recetas obtenidas: {Recetas.Count}");



            //Console.WriteLine("DEBUG -> 19");
            // Agrupamos los datos en un objeto anónimo o lista para enviar juntos
            var Aux_Objeto = new { Materias, Recetas };

            //Console.WriteLine($"Materias: {System.Text.Json.JsonSerializer.JsonSerializer.Serialize(Aux_Objeto, new JsonSerializerOptions { WriteIndented = true })}");

            return Ok(Aux_Objeto);
            

        }

        [HttpGet("{NombreReceta}")]
        public IActionResult Get(string NombreReceta)
        {
            //Console.WriteLine("DEBUG -> 20");
            short NumeroEtapas;
            int NumeroProcesos;

            SQLServerManager BBDD = new SQLServerManager();
            Querys Query = new Querys();

            List<object> Receta = new List<object>();

            //Se obtiene la cabecera de la receta
            List<CabeceraReceta> CabeceraReceta = BBDD.GetDatos<CabeceraReceta>(Query.Select(0, NombreReceta));
            //Se obtiene un listado de los procesos
            List<Procesos> ListaProcesos = BBDD.GetDatos<Procesos>(Query.Select(3));

            Receta.Add(CabeceraReceta);

            NumeroEtapas = CabeceraReceta[0].NumeroEtapas;
            NumeroProcesos = ListaProcesos.Count();

            for (short NumeroEtapa = 1; NumeroEtapa <= (NumeroEtapas); NumeroEtapa++)
            {
                List<object> Etapa = new List<object>();

                Receta.Add(Etapa);

                //Se obtiene la cabecera de la etapa
                List<CabeceraEtapa> CabeceraEtapa = BBDD.GetDatos<CabeceraEtapa>(Query.Select(1, NombreReceta, NumeroEtapa));

                Etapa.Add(CabeceraEtapa);

                for (short NumeroProceso = 1; NumeroProceso <= (NumeroProcesos); NumeroProceso++)
                {
                    //Se obtienen los procesos y consignas de la etapa seleccionada
                    List<CsgProceso> ObjetosEtapa = BBDD.GetDatos<CsgProceso>(Query.Select(2, NombreReceta, NumeroEtapa, ListaProcesos[NumeroProceso - 1].Nombre));
                    Etapa.Add(ObjetosEtapa);
                }
            }
            
            //Console.WriteLine("Esta es la receta:\n" + JsonConvert.SerializeObject(Receta, Formatting.Indented));

            return Ok(Receta);
        }

        [HttpGet("ListadoMMPP_Reactor/{NombreReactor}/{ID_Father}")]
        public IActionResult Get_MMPP_Reactor(string NombreReactor, int ID_Reactor)
        {
            Console.WriteLine("DEBUG -> 30");
            SQLServerManager BBDD = new SQLServerManager();

            string query = $@"
                            SELECT N_MateriaPrima, MateriaPrima
                            FROM MateriasPrimas
                            WHERE ID_Father = (SELECT ID FROM Reactores WHERE Nombre like '{NombreReactor}')";

            List<MMPP> MateriasPrimas = BBDD.GetListadoMMP(query);

            Console.WriteLine("DEBUG -> 39");
            return Ok(MateriasPrimas);
        }

        [HttpGet("ListadoMMPP_Materias/{NombreMateria}/{ID_Father}")]
        public IActionResult Get_MMPP_Materia(string NombreMateria, int ID_Reactor)
        {
            Console.WriteLine("DEBUG -> 40");
            SQLServerManager BBDD = new SQLServerManager();

            string query = $@"
                            SELECT N_MateriaPrima, MateriaPrima
                            FROM MateriasPrimas
                            WHERE ID_Father = (SELECT ID FROM Materias WHERE Nombre like '{NombreMateria}')";

            List<MMPP> MateriasPrimas = BBDD.GetListadoMMP(query);

            Console.WriteLine("DEBUG -> 449");
            Console.WriteLine("Listado MMPP:", MateriasPrimas);
            return Ok(MateriasPrimas);
        }


        [HttpPost("DatosReceta/{DatosReceta}")]
        public async Task<IActionResult> Put(JsonElement DatosReceta)
        {
            Console.WriteLine("DEBUG -> 50");
            SQLServerManager BBDD = new SQLServerManager();

            //CabeceraReceta CabeceraReceta = new CabeceraReceta();
            JSON json = new JSON(DatosReceta);

            CabeceraReceta CabeceraReceta = json.ObtenerCabeceraReceta();

            Console.WriteLine("Cabecera Receta:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(CabeceraReceta, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));


            CabeceraEtapa CabeceraEtapa = new CabeceraEtapa();
            CsgProceso1 Consignas = new CsgProceso1();
                                                         
            await BBDD.EliminarDatos(DatosReceta,CabeceraReceta);
            
            await BBDD.ActualizarCabeceraReceta(CabeceraReceta, CabeceraEtapa, Consignas);

            // Buscamos el ID Receta y ID etapa si un valor con estos datoas esta lo eliminados

            
            bool Deleteado = false;
            short Aux_N_Etapas = 0;

            
            for (short NumeroEtapa = 1; NumeroEtapa <= (CabeceraReceta.NumeroEtapas); NumeroEtapa++)
            {
                CabeceraEtapa = json.ObtenerCabeceraEtapa(NumeroEtapa);

                await BBDD.ActualizarCabeceraEtapa(CabeceraReceta, CabeceraEtapa, Consignas);


                await BBDD.EliminarDatoEtapa_PPL(NumeroEtapa, CabeceraReceta.ID);

                for (short NumeroProceso = 1; NumeroProceso <= (5); NumeroProceso++)
                {
                    int NumeroConsignas;
                    List<CsgProceso1> ConsignasProceso = new List<CsgProceso1>();

                    Console.WriteLine($"[DEBUG] Etapa {NumeroEtapa}, Proceso {NumeroProceso} -> {ConsignasProceso.Count} consignas");

                    ConsignasProceso = json.ObtenerConsignasProceso(NumeroEtapa, NumeroProceso);
                    NumeroConsignas = ConsignasProceso.Count();

                    //for (int NumeroConsigna = 0; NumeroConsigna < NumeroConsignas; NumeroConsigna++)
                    foreach (var consigna in ConsignasProceso)
                    {

                        Console.WriteLine($"[DEBUG]    -> ID: {consigna.ID}, Tipo: {consigna.Tipo}, Consigna: {consigna.Consigna}, Valor: {consigna.Valor}");



                        if (Aux_N_Etapas != CabeceraReceta.NumeroEtapas)
                        {
                            if (!Deleteado)
                            {
                                Aux_N_Etapas = CabeceraReceta.NumeroEtapas;
                                Console.WriteLine("-----");
                                Console.WriteLine($"Aux_N_Etapas: {Aux_N_Etapas}");
                                Console.WriteLine($"Deleteado ->>> {Deleteado}");
                                Console.WriteLine("-----");
                            }
                        }

                      

                        await BBDD.ActualizarConsignasProceso(CabeceraReceta, CabeceraEtapa, consigna);
                        
                    }
                }
            }
            Console.WriteLine("DEBUG -> 59");
            return Ok(json.ObtenerCabeceraReceta());
        }

        [HttpDelete("EliminarReceta/{ID_Receta}")]
        public async Task<IActionResult> Delete(int ID_Receta)
        {
            Console.WriteLine("DEBUG -> 100");
            SQLServerManager BBDD = new SQLServerManager();

            await BBDD.EliminarReceta(ID_Receta);
            Console.WriteLine("DEBUG -> 101");
            return Ok("Receta Eliminada");
        }


    }
}
*/

namespace GestionRecetas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecetasController : ControllerBase
    {
        // GET: api/Recetas/Reactores
        [HttpGet("Reactores")]
        public IActionResult Get()
        {
            // Se inicializa la conexión a base de datos
            SQLServerManager BBDD = new SQLServerManager();

            // Se crean las listas para almacenar los datos
            List<object> Objeto = new List<object>();
            List<Reactores> Reactores = BBDD.GetTabla<Reactores>("Reactores");
            List<Recetas> Recetas = BBDD.GetRecetas("Recetas");

            // Se añaden los datos a una lista para enviarlos juntos
            Objeto.Add(Reactores);
            Objeto.Add(Recetas);

            return Ok(Objeto); // Se retorna el resultado
        }

        // GET: api/Recetas/Materias
        [HttpGet("Materias")]
        public IActionResult Get_Materias()
        {
            SQLServerManager BBDD = new SQLServerManager();

            List<Materias> Materias = BBDD.GetTabla<Materias>("Materias");
            List<Recetas> Recetas = BBDD.GetRecetas("Recetas");

            // Se agrupan materias y recetas en un objeto anónimo
            var Aux_Objeto = new { Materias, Recetas };

            return Ok(Aux_Objeto); // Se retorna el resultado
        }

        // GET: api/Recetas/{NombreReceta}
        [HttpGet("{NombreReceta}")]
        public IActionResult Get(string NombreReceta)
        {
            short NumeroEtapas;
            int NumeroProcesos;

            SQLServerManager BBDD = new SQLServerManager();
            Querys Query = new Querys();

            List<object> Receta = new List<object>();

            // Se obtiene la cabecera de la receta
            List<CabeceraReceta> CabeceraReceta = BBDD.GetDatos<CabeceraReceta>(Query.Select(0, NombreReceta));

            // Se obtiene el listado de procesos
            List<Procesos> ListaProcesos = BBDD.GetDatos<Procesos>(Query.Select(3));
            Receta.Add(CabeceraReceta);

            NumeroEtapas = CabeceraReceta[0].NumeroEtapas;
            NumeroProcesos = ListaProcesos.Count();

            for (short NumeroEtapa = 1; NumeroEtapa <= NumeroEtapas; NumeroEtapa++)
            {
                List<object> Etapa = new List<object>();
                Receta.Add(Etapa);

                // Se obtiene la cabecera de la etapa
                List<CabeceraEtapa> CabeceraEtapa = BBDD.GetDatos<CabeceraEtapa>(Query.Select(1, NombreReceta, NumeroEtapa));
                Etapa.Add(CabeceraEtapa);

                // Se obtienen todas las consignas para cada proceso de la etapa
                for (short NumeroProceso = 1; NumeroProceso <= NumeroProcesos; NumeroProceso++)
                {
                    List<CsgProceso> ObjetosEtapa = BBDD.GetDatos<CsgProceso>(
                        Query.Select(2, NombreReceta, NumeroEtapa, ListaProcesos[NumeroProceso - 1].Nombre));
                    Etapa.Add(ObjetosEtapa);
                }
            }

            return Ok(Receta); // Se retorna la receta estructurada
        }

        // GET: api/Recetas/ListadoMMPP_Reactor/{NombreReactor}/{ID_Father}
        [HttpGet("ListadoMMPP_Reactor/{NombreReactor}/{ID_Father}")]
        public IActionResult Get_MMPP_Reactor(string NombreReactor, int ID_Reactor)
        {
            SQLServerManager BBDD = new SQLServerManager();

            // Consulta SQL personalizada para obtener materias primas del reactor
            string query = $@"
                SELECT N_MateriaPrima, MateriaPrima
                FROM MateriasPrimas
                WHERE ID_Father = (SELECT ID FROM Reactores WHERE Nombre like '{NombreReactor}')";

            List<MMPP> MateriasPrimas = BBDD.GetListadoMMP(query);
            return Ok(MateriasPrimas);
        }

        // GET: api/Recetas/ListadoMMPP_Materias/{NombreMateria}/{ID_Father}
        [HttpGet("ListadoMMPP_Materias/{NombreMateria}/{ID_Father}")]
        public IActionResult Get_MMPP_Materia(string NombreMateria, int ID_Reactor)
        {
            SQLServerManager BBDD = new SQLServerManager();

            // Consulta SQL para obtener materias primas de una materia
            string query = $@"
                SELECT N_MateriaPrima, MateriaPrima
                FROM MateriasPrimas
                WHERE ID_Father = (SELECT ID FROM Materias WHERE Nombre like '{NombreMateria}')";

            List<MMPP> MateriasPrimas = BBDD.GetListadoMMP(query);
            return Ok(MateriasPrimas);
        }

        // POST: api/Recetas/DatosReceta/{DatosReceta}
        [HttpPost("DatosReceta/{DatosReceta}")]
        public async Task<IActionResult> Put(JsonElement DatosReceta)
        {
            SQLServerManager BBDD = new SQLServerManager();

            // Convertimos el JSON a una clase manejable
            JSON json = new JSON(DatosReceta);
            CabeceraReceta CabeceraReceta = json.ObtenerCabeceraReceta();

            // DEBUG: Mostrar la cabecera de la receta (sirve para comprobar qué receta estás guardando)
            //Console.WriteLine($"[DEBUG JSON]: JSON de la RECETA cabecera: {JsonConvert.SerializeObject(CabeceraReceta, Formatting.Indented)}");

            // DEBUG: Mostrar el JSON bruto recibido (si quieres ver la estructura completa del JSON original)
            //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(DatosReceta, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

            // Eliminamos datos previos de la receta en la base de datos
            await BBDD.EliminarDatos(DatosReceta, CabeceraReceta);

            CabeceraEtapa CabeceraEtapa = new CabeceraEtapa();
            CsgProceso1 Consignas = new CsgProceso1();

            await BBDD.ActualizarCabeceraReceta(CabeceraReceta, CabeceraEtapa, Consignas);

            short Aux_N_Etapas = 0;
            bool Deleteado = false;

            for (short NumeroEtapa = 1; NumeroEtapa <= CabeceraReceta.NumeroEtapas; NumeroEtapa++)
            {
                CabeceraEtapa = json.ObtenerCabeceraEtapa(NumeroEtapa);
                await BBDD.ActualizarCabeceraEtapa(CabeceraReceta, CabeceraEtapa, Consignas);

                await BBDD.EliminarDatoEtapa_PPL(NumeroEtapa, CabeceraReceta.ID);

                for (short NumeroProceso = 1; NumeroProceso <= 5; NumeroProceso++)
                {
                    List<CsgProceso1> ConsignasProceso = json.ObtenerConsignasProceso(NumeroEtapa, NumeroProceso);

                    foreach (var consigna in ConsignasProceso)
                    {
                        // Solo eliminar una vez por receta
                        if (Aux_N_Etapas != CabeceraReceta.NumeroEtapas && !Deleteado)
                        {
                            Aux_N_Etapas = CabeceraReceta.NumeroEtapas;
                            Deleteado = true;
                        }
                        await BBDD.ActualizarConsignasProceso(CabeceraReceta, CabeceraEtapa, consigna);
                    }
                }
            }

            return Ok(json.ObtenerCabeceraReceta());
        }

        // DELETE: api/Recetas/EliminarReceta/{ID_Receta}
        [HttpDelete("EliminarReceta/{ID_Receta}")]
        public async Task<IActionResult> Delete(int ID_Receta)
        {
            SQLServerManager BBDD = new SQLServerManager();
            await BBDD.EliminarReceta(ID_Receta);
            return Ok("Receta Eliminada");
        }
    }
}
