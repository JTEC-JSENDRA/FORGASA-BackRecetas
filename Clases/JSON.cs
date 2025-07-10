using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using GestionRecetas.Models;


namespace GestionRecetas.Clases
{
    // Clase para manejar y extraer datos desde un JSON en formato JsonElement
    public class JSON
    {
        private readonly JsonElement jsonData; // Almacena el JSON crudo que se recibe como entrada

        // ---------------------------------------------------------------------------------------------------------------------------

        public JSON(JsonElement jsonData)
        {
            this.jsonData = jsonData;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Extrae la información general (cabecera) de una receta desde un JSON.
        // Convierte el JSON en objetos C# para facilitar su manejo.

        public CabeceraReceta ObtenerCabeceraReceta()
        {
            CabeceraReceta Cabecera = new CabeceraReceta();
            string jsonString = jsonData.ToString();

            // Convierte el JsonElement en un arreglo JSON para facilitar el acceso
            JArray jsonArray = JArray.Parse(jsonString);

            // Accede al primer objeto que contiene la cabecera de la receta
            JObject primerObjeto = (JObject)jsonArray[0][0];

            // Asigna valores a las propiedades del objeto Cabecera
            Cabecera.ID = primerObjeto.Value<int>("id");
            Cabecera.NombreReceta = primerObjeto.Value<string>("nombreReceta");
            Cabecera.NombreReactor = primerObjeto.Value<string>("nombreReactor");
            Cabecera.NumeroEtapas = primerObjeto.Value<short>("numeroEtapas");

            // Asigna las fechas solo si existen en el JSON (no son nulas)
            if (primerObjeto["modificada"].Type != JTokenType.Null) // - se cambia creada por modificada - 10/07/2025
                Cabecera.Creada = primerObjeto.Value<DateTime>("creada");

            if (primerObjeto["modificada"].Type != JTokenType.Null)
                Cabecera.Modificada = primerObjeto.Value<DateTime>("modificada");

            if (primerObjeto["eliminada"].Type != JTokenType.Null)
                Cabecera.Eliminada = primerObjeto.Value<DateTime>("eliminada");

            // Devuelve el objeto con la información de la cabecera
            return Cabecera;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Obtiene la información general (cabecera) de una etapa específica desde un JSON.
        // Convierte los datos JSON en un objeto CabeceraEtapa para facilitar su uso.
        public CabeceraEtapa ObtenerCabeceraEtapa(short NumeroEtapa)
        {
            CabeceraEtapa Cabecera = new CabeceraEtapa();
            string jsonString = jsonData.ToString();

            // Convierte el JsonElement en un arreglo JSON para acceder fácilmente a los datos
            JArray jsonArray = JArray.Parse(jsonString);

            // Accede al objeto correspondiente a la etapa indicada (Número de etapa)
            JObject primerObjeto = (JObject)jsonArray[NumeroEtapa][0][0];

            // Asigna los valores de la etapa al objeto CabeceraEtapa
            Cabecera.ID = primerObjeto.Value<int>("id");
            Cabecera.EtapaActiva = primerObjeto.Value<short>("etapaActiva");
            Cabecera.N_Etapa = primerObjeto.Value<short>("n_Etapa");
            Cabecera.Nombre = primerObjeto.Value<string>("nombre");

            // Devuelve la información de la etapa
            return Cabecera;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Obtiene una lista de consignas de un proceso para una etapa y proceso específicos.
        // Convierte datos JSON en una lista de objetos CsgProceso1 para facilitar su uso.
        public List<CsgProceso1> ObtenerConsignasProceso(int NumeroEtapa, int NumeroProceso)
        {
            List<CsgProceso1> ListaConsignas = new List<CsgProceso1>();
            string jsonString = jsonData.ToString();

            // Convierte el JSON a un arreglo para poder acceder a sus elementos fácilmente
            JArray jsonArray = JArray.Parse(jsonString);

            // Accede al arreglo que contiene las consignas del proceso indicado
            JArray proceso = (JArray)jsonArray[NumeroEtapa][NumeroProceso];
            int numeroConsignas = proceso.Count;

            // Recorre cada consigna en el arreglo y la convierte en un objeto CsgProceso1
            for (int NumeroConsigna = 0; NumeroConsigna < numeroConsignas; NumeroConsigna++)
            {
                CsgProceso1 Consigna = new CsgProceso1();
                JObject primerObjeto = (JObject)jsonArray[NumeroEtapa][NumeroProceso][NumeroConsigna];

                Consigna.ID = primerObjeto.Value<int>("id");
                Consigna.Tipo = primerObjeto.Value<string>("tipo");
                Consigna.Consigna = primerObjeto.Value<string>("consigna");
                Consigna.Valor = primerObjeto.Value<string>("valor");

                ListaConsignas.Add(Consigna);
            }

            // Devuelve la lista con todas las consignas procesadas
            return ListaConsignas;
        }

        // ---------------------------------------------------------------------------------------------------------------------------

        // Extrae los IDs de las etapas y consignas para todos los procesos desde un JSON.
        // Devuelve una lista donde el primer array contiene los IDs de las etapas,
        // y los siguientes arrays contienen los IDs de las consignas para cada proceso.
        public List<int[]> GetListadosID(int NumeroProcesos, int NumeroEtapas)
        {
            List<int[]> Listado = new List<int[]>();
            string jsonString = jsonData.ToString();
            JArray jsonArray = JArray.Parse(jsonString);

            int[] IDsEtapas = new int[NumeroEtapas];

            // Obtiene los IDs de las etapas (los datos empiezan en el índice 1 del arreglo)
            for (int Etapa = 0; Etapa < NumeroEtapas; Etapa++)
            {
                JObject etapa = (JObject)jsonArray[Etapa + 1][0][0];
                IDsEtapas[Etapa] = etapa.Value<int>("id");
            }
            Listado.Add(IDsEtapas);

            // Para cada proceso, obtiene los IDs de todas sus consignas en todas las etapas
            for (int Proceso = 1; Proceso < NumeroProcesos; Proceso++)
            {
                int Indice = 0;
                int NumeroConsignas = 0;

                // Cuenta cuántas consignas tiene el proceso en total (sumando todas las etapas)
                for (int Etapa = 1; Etapa <= NumeroEtapas; Etapa++)
                {
                    JArray proceso = (JArray)jsonArray[Etapa][Proceso];
                    NumeroConsignas += proceso.Count;
                }
                Console.WriteLine($"Numero de consignas del proceso {Proceso}: {NumeroConsignas}");
                int[] IDsConsignas = new int[NumeroConsignas];

                // Extrae los IDs de cada consigna y los almacena en un array
                for (int Etapa = 1; Etapa <= NumeroEtapas; Etapa++)
                {
                    JArray proceso = (JArray)jsonArray[Etapa][Proceso];
                    int NumeroConsignasProceso = proceso.Count;

                    for (int NumeroConsigna = 0; NumeroConsigna < NumeroConsignasProceso; NumeroConsigna++)
                    {
                        JObject consigna = (JObject)jsonArray[Etapa][Proceso][NumeroConsigna];
                        IDsConsignas[Indice] = consigna.Value<int>("id");
                        
                        Indice++;
                    }
                }
                Listado.Add(IDsConsignas);
            }

            // Devuelve todos los IDs agrupados por etapas y procesos
            return Listado;
        }

        // ---------------------------------------------------------------------------------------------------------------------------
    }
}