

using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using GestionRecetas.Models;

/*

namespace GestionRecetas.Clases
{
    public class JSON
    {

        private readonly JsonElement jsonData;

        public JSON(JsonElement jsonData)
        {
            this.jsonData = jsonData;
        }

        public CabeceraReceta ObtenerCabeceraReceta()
        {


            CabeceraReceta Cabecera = new CabeceraReceta();
            string jsonString = jsonData.ToString();

            JArray jsonArray = JArray.Parse(jsonString);

            //Console.WriteLine("- DEBUG previo array en json---------------------");
            Console.WriteLine(jsonArray.ToString(Newtonsoft.Json.Formatting.Indented));

            // Acceder a la cabecera de la receta
            JObject primerObjeto = (JObject)jsonArray[0][0];
            Cabecera.ID = primerObjeto.Value<int>("id");
            Cabecera.NombreReceta = primerObjeto.Value<string>("nombreReceta");
            Cabecera.NombreReactor = primerObjeto.Value<string>("nombreReactor");
            Cabecera.NumeroEtapas = primerObjeto.Value<short>("numeroEtapas");

            if (primerObjeto["modificada"].Type != JTokenType.Null)
            {
                Cabecera.Creada = primerObjeto.Value<DateTime>("creada");
            }

            if (primerObjeto["modificada"].Type != JTokenType.Null)
            {
                Cabecera.Modificada = primerObjeto.Value<DateTime>("modificada");
            }

            if (primerObjeto["eliminada"].Type != JTokenType.Null)
            {
                Cabecera.Eliminada = primerObjeto.Value<DateTime>("eliminada");
            }

           // Console.WriteLine("- DEBUG cABECERA 999 ---------------------");
           // Console.WriteLine(JsonSerializer.Serialize(Cabecera, new JsonSerializerOptions { WriteIndented = true }));

            return Cabecera;
        }

        public CabeceraEtapa ObtenerCabeceraEtapa(short NumeroEtapa)
        {
            CabeceraEtapa Cabecera = new CabeceraEtapa();
            string jsonString = jsonData.ToString();

            JArray jsonArray = JArray.Parse(jsonString);

            // Acceder ala cabecera de la etapa
            JObject primerObjeto = (JObject)jsonArray[NumeroEtapa][0][0];
            Cabecera.ID = primerObjeto.Value<int>("id");
            Cabecera.EtapaActiva = primerObjeto.Value<short>("etapaActiva");
            Cabecera.N_Etapa = primerObjeto.Value<short>("n_Etapa");
            Cabecera.Nombre = primerObjeto.Value<string>("nombre");

            return Cabecera;
        }

        public List<CsgProceso1> ObtenerConsignasProceso(int NumeroEtapa, int NumeroProceso)
        {
            List<CsgProceso1> ListaConsignas = new List<CsgProceso1>();
            string jsonString = jsonData.ToString();

            JArray jsonArray = JArray.Parse(jsonString);

            // Acceder a las consignas individuales de cada proceso según la etapa
            JArray proceso = (JArray)jsonArray[NumeroEtapa][NumeroProceso];
            int numeroConsignas = proceso.Count;

            //Console.WriteLine($"Numero etapa: {NumeroEtapa}  - NumeroProceso: {NumeroProceso}  - Numero consignas: {numeroConsignas}");

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

            return ListaConsignas;
        }



        public List<int[]> GetListadosID(int NumeroProcesos, int NumeroEtapas)
        {
            List<int[]> Listado = new List<int[]>();

            string jsonString = jsonData.ToString();
            JArray jsonArray = JArray.Parse(jsonString);

            int[] IDsEtapas = new int[NumeroEtapas];
            int[] IDsConsignas;
            int NumeroConsignas;
            int Indice;

            //Se obtiene el array de IDs de las etapas
            for (int Etapa = 0; Etapa < NumeroEtapas; Etapa++)
            {
                JObject etapa = (JObject)jsonArray[Etapa + 1][0][0];
                IDsEtapas[Etapa] = etapa.Value<int>("id");
            }
            Listado.Add(IDsEtapas);

            //Se obtienen los IDs de las distintas consignas
            for (int Proceso = 1; Proceso < NumeroProcesos; Proceso++)
            {
                Indice = 0;
                NumeroConsignas = 0;

                for (int Etapa = 1; Etapa <= NumeroEtapas; Etapa++)
                {
                    JArray proceso = (JArray)jsonArray[Etapa][Proceso];
                    NumeroConsignas = proceso.Count + NumeroConsignas;
                }
                Console.WriteLine($"Numero de consignas del proceso {Proceso}: {NumeroConsignas}");
                IDsConsignas = new int[NumeroConsignas];

                for (int Etapa = 1; Etapa <= NumeroEtapas; Etapa++)
                {
                    JArray proceso = (JArray)jsonArray[Etapa][Proceso];
                    int NumeroConsignasProceso = proceso.Count;

                    for (int NumeroConsigna = 0; NumeroConsigna < NumeroConsignasProceso; NumeroConsigna++)
                    {
                        JObject consigna = (JObject)jsonArray[Etapa][Proceso][NumeroConsigna];
                        IDsConsignas[Indice] = consigna.Value<int>("id");
                        Console.WriteLine($"Etapa: {Etapa}  - ID Consigna: {IDsConsignas[Indice]}");
                        Indice++;
                    }
                }
                Listado.Add(IDsConsignas);
            }

            return Listado;

        }
    }
}

*/

namespace GestionRecetas.Clases
{
    // Clase para manejar y extraer datos desde un JSON en formato JsonElement
    public class JSON
    {
        private readonly JsonElement jsonData; // Almacena el JSON crudo que se recibe como entrada

        public JSON(JsonElement jsonData)
        {
            this.jsonData = jsonData;
        }

        // Extrae la cabecera de la receta (información general de la receta)
        public CabeceraReceta ObtenerCabeceraReceta()
        {
            CabeceraReceta Cabecera = new CabeceraReceta();
            string jsonString = jsonData.ToString();

            // Convierte el JsonElement en un JArray para trabajar fácilmente con él
            JArray jsonArray = JArray.Parse(jsonString);

            // Muestra el JSON en consola para depuración
            //Console.WriteLine(jsonArray.ToString(Newtonsoft.Json.Formatting.Indented));

            // Accede a la primera sección que contiene la cabecera de la receta
            JObject primerObjeto = (JObject)jsonArray[0][0];

            // Asigna valores al objeto Cabecera
            Cabecera.ID = primerObjeto.Value<int>("id");
            Cabecera.NombreReceta = primerObjeto.Value<string>("nombreReceta");
            Cabecera.NombreReactor = primerObjeto.Value<string>("nombreReactor");
            Cabecera.NumeroEtapas = primerObjeto.Value<short>("numeroEtapas");

            // Asigna fechas solo si existen
            if (primerObjeto["modificada"].Type != JTokenType.Null)
                Cabecera.Creada = primerObjeto.Value<DateTime>("creada");

            if (primerObjeto["modificada"].Type != JTokenType.Null)
                Cabecera.Modificada = primerObjeto.Value<DateTime>("modificada");

            if (primerObjeto["eliminada"].Type != JTokenType.Null)
                Cabecera.Eliminada = primerObjeto.Value<DateTime>("eliminada");

            return Cabecera;
        }

        // Obtiene la cabecera de una etapa específica
        public CabeceraEtapa ObtenerCabeceraEtapa(short NumeroEtapa)
        {
            CabeceraEtapa Cabecera = new CabeceraEtapa();
            string jsonString = jsonData.ToString();
            JArray jsonArray = JArray.Parse(jsonString);

            // Accede a la información de la etapa correspondiente
            JObject primerObjeto = (JObject)jsonArray[NumeroEtapa][0][0];

            Cabecera.ID = primerObjeto.Value<int>("id");
            Cabecera.EtapaActiva = primerObjeto.Value<short>("etapaActiva");
            Cabecera.N_Etapa = primerObjeto.Value<short>("n_Etapa");
            Cabecera.Nombre = primerObjeto.Value<string>("nombre");

            return Cabecera;
        }

        // Obtiene una lista de consignas de proceso para una etapa y proceso especificado
        public List<CsgProceso1> ObtenerConsignasProceso(int NumeroEtapa, int NumeroProceso)
        {
            List<CsgProceso1> ListaConsignas = new List<CsgProceso1>();
            string jsonString = jsonData.ToString();
            JArray jsonArray = JArray.Parse(jsonString);

            // Accede al array de consignas para la etapa y proceso indicados
            JArray proceso = (JArray)jsonArray[NumeroEtapa][NumeroProceso];
            int numeroConsignas = proceso.Count;

            // Recorre cada consigna y la convierte en objeto CsgProceso1
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

            return ListaConsignas;
        }

        // Extrae los IDs de las etapas y consignas para todos los procesos
        public List<int[]> GetListadosID(int NumeroProcesos, int NumeroEtapas)
        {
            List<int[]> Listado = new List<int[]>();
            string jsonString = jsonData.ToString();
            JArray jsonArray = JArray.Parse(jsonString);

            int[] IDsEtapas = new int[NumeroEtapas];

            // Obtiene los IDs de las etapas (comienza desde el índice 1 en adelante)
            for (int Etapa = 0; Etapa < NumeroEtapas; Etapa++)
            {
                JObject etapa = (JObject)jsonArray[Etapa + 1][0][0];
                IDsEtapas[Etapa] = etapa.Value<int>("id");
            }
            Listado.Add(IDsEtapas);

            // Para cada proceso (empezando del 1), obtiene los IDs de sus consignas
            for (int Proceso = 1; Proceso < NumeroProcesos; Proceso++)
            {
                int Indice = 0;
                int NumeroConsignas = 0;

                // Calcula el número total de consignas para el proceso
                for (int Etapa = 1; Etapa <= NumeroEtapas; Etapa++)
                {
                    JArray proceso = (JArray)jsonArray[Etapa][Proceso];
                    NumeroConsignas += proceso.Count;
                }
                Console.WriteLine($"Numero de consignas del proceso {Proceso}: {NumeroConsignas}");
                int[] IDsConsignas = new int[NumeroConsignas];

                // Extrae los IDs de cada consigna
                for (int Etapa = 1; Etapa <= NumeroEtapas; Etapa++)
                {
                    JArray proceso = (JArray)jsonArray[Etapa][Proceso];
                    int NumeroConsignasProceso = proceso.Count;

                    for (int NumeroConsigna = 0; NumeroConsigna < NumeroConsignasProceso; NumeroConsigna++)
                    {
                        JObject consigna = (JObject)jsonArray[Etapa][Proceso][NumeroConsigna];
                        IDsConsignas[Indice] = consigna.Value<int>("id");
                        //Console.WriteLine($"Etapa: {Etapa}  - ID Consigna: {IDsConsignas[Indice]}");
                        Indice++;
                    }
                }
                Listado.Add(IDsConsignas);
            }

            return Listado;
        }
    }
}