using System;
using System.Collections.Generic;
using GestionRecetas.Models;



namespace GestionRecetas.Datos
{
    public class Querys
    {

        public string Select(int Consulta, string Valor = "", short Etapa = 0, string Proceso = "", int ID_Receta =0, string Tipo = "",string Consigna = "")
        {
           List<string> querys = new List<string>();

            //0 - Obtener cabecera de la receta
            querys.Add($@"SELECT
                            R.ID,
                            R.NombreReceta AS NombreReceta,
                            T2.Nombre AS NombreReactor,
                            R.NumeroEtapas,
                            R.Creada,
                            R.Modificada,
                            R.Eliminada
                        FROM Recetas R
                        JOIN Materias T2 ON R.ID_Father = T2.ID
                        WHERE R.nombreReceta LIKE '{Valor}' AND T2.ID = R.ID_Father;");

            //1 - Obtener cabecera de la etapa
            querys.Add($@"SELECT ID, N_Etapa, Nombre, EtapaActiva
                        FROM Etapas
                        WHERE N_Etapa = {Etapa}
                        AND ID_Receta IN(SELECT ID FROM Recetas WHERE nombreReceta LIKE '{Valor}')");

            //2 - Obtener datos del proceso seleccionado
            querys.Add($@"SELECT ID, Tipo, Consigna, Valor
                        From {Proceso} --Viene definido por listado obtenido
                        WHERE N_Etapa = {Etapa}
                        AND ID_Receta IN (SELECT ID FROM Recetas WHERE nombreReceta LIKE '{Valor}')");

            //3 - Obtener datos del proceso secundario
            querys.Add($@"SELECT ID, Nombre FROM Procesos ORDER BY ID");

            //4 - Comprobar si la receta existe en la BBDD 
            querys.Add($@"SELECT COUNT(*) FROM Recetas WHERE ID = '{Valor}'");

            //5 - Comprobar si la etapa existe en la BBDD 
            querys.Add($@"SELECT COUNT(*) FROM Etapas WHERE ID = '{Valor}'");

            //6 - Comprobar si la consigna existe en la BBDD 
            querys.Add($@"SELECT COUNT(*) FROM Proceso{Proceso} WHERE ID = '{Valor}'");

            //7 - Se obtienen todos los IDs de las etapas de la receta seleccionadas
            querys.Add($@"SELECT ID FROM Etapas WHERE ID_Receta = '{Valor}'");

            //8 - Se obtienen todos los IDs de las consignas de las tablas de proceso de la receta seleccionadas
            querys.Add($@"SELECT ID FROM Proceso{Proceso} WHERE ID_Receta = '{Valor}'");

            //9 - Contador de elementos de una tabla condicionado
            querys.Add($@"SELECT COUNT(*) FROM {Proceso} WHERE ID_Receta = '{Valor}'");

            //10 - Contador de elementos de una tabla
            querys.Add($@"SELECT COUNT(*) FROM {Valor} ");

            //11 - TEST
            querys.Add($@"SELECT COUNT(*) FROM Proceso{Proceso}
              WHERE ID_Receta = {ID_Receta}
                AND N_Etapa = {Etapa}
                AND Tipo = '{Tipo.Replace("'", "''")}'
                AND Consigna = '{Consigna.Replace("'", "''")}'
                AND Valor = '{Valor.Replace("'", "''")}'");

            return querys[Consulta];
        }

        public string Insert(int Consulta, CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 Consigna, string Proceso = "")
        {
            List<string> querys = new List<string>();

            //0 - Se inserta la receta en caso de que no exista
            
            querys.Add($@"INSERT INTO Recetas (ID_Father, NombreReceta, Bloqueada, NumeroEtapas, Creada,Operacion,Material,alternativa,Version) 
                            VALUES (
                                    (SELECT ID FROM Materias WHERE Nombre LIKE '{CabeceraReceta.NombreReactor}'), 
                                    '{CabeceraReceta.NombreReceta}',
                                    {0},
                                    {CabeceraReceta.NumeroEtapas},
                                    GETDATE(),
                                    (
                                      SELECT Nombre 
                                      FROM Materias 
                                      WHERE ID = (SELECT ID FROM Materias WHERE Nombre LIKE '{CabeceraReceta.NombreReactor}')                                    
                                    ),
                                    (
                                      SELECT Operacion 
                                      FROM Materias 
                                      WHERE ID = (SELECT ID FROM Materias WHERE Nombre LIKE '{CabeceraReceta.NombreReactor}')
                                    ),

                                    {0},
                                    {0}
                                    )"
             );
            
            //

            //1 - Se inserta la cabecera de la etapa en caso de que no exista 
            querys.Add($@"INSERT INTO Etapas (ID_Receta, EtapaActiva, N_Etapa, Nombre) 
                            VALUES ({CabeceraReceta.ID}, {CabeceraEtapa.EtapaActiva}, {CabeceraEtapa.N_Etapa}, '{CabeceraEtapa.Nombre}')");

            //2 - Se inserta la consigna en caso de que no exista en la BBDD 
            querys.Add($@"INSERT INTO Proceso{Proceso} (ID_Receta, N_Etapa, Tipo, Consigna, Valor) 
                            VALUES ({CabeceraReceta.ID}, {CabeceraEtapa.N_Etapa}, '{Consigna.Tipo}', '{Consigna.Consigna}', {Consigna.Valor})");

            return querys[Consulta];
        }

        public string Update(int Consulta, CabeceraReceta CabeceraReceta, CabeceraEtapa CabeceraEtapa, CsgProceso1 Consigna, string Proceso = "")
        {
            List<string> querys = new List<string>();

            //0 - Se actualizan los valores de la receta 
            
            querys.Add($@"UPDATE Recetas 
                            SET ID_Father = (SELECT ID FROM Materias WHERE Nombre LIKE '{CabeceraReceta.NombreReactor}'),
                                NombreReceta = '{CabeceraReceta.NombreReceta}',
                                Bloqueada = {0},
                                NumeroEtapas = {CabeceraReceta.NumeroEtapas},
                                Modificada = GETDATE(),
                                Operacion =  (
                                      SELECT Operacion 
                                      FROM Materias 
                                      WHERE ID = (SELECT ID FROM Materias WHERE Nombre LIKE '{CabeceraReceta.NombreReactor}')
                                    ),
                                Material =  (
                                      SELECT Nombre 
                                      FROM Materias 
                                      WHERE ID = (SELECT ID FROM Materias WHERE Nombre LIKE '{CabeceraReceta.NombreReactor}')
                                    )
                            WHERE ID LIKE '{CabeceraReceta.ID}'");
            
 

            //1 - Se actualizan los valores de la cabecera
            querys.Add($@"UPDATE Etapas 
                            SET ID_Receta =  {CabeceraReceta.ID},
                                EtapaActiva = {CabeceraEtapa.EtapaActiva},
                                N_Etapa = {CabeceraEtapa.N_Etapa},
                                Nombre = '{CabeceraEtapa.Nombre}'
                            WHERE ID LIKE {CabeceraEtapa.ID}");

            //2 - Se actualizan los valores de la consigna
            querys.Add($@"UPDATE Proceso{Proceso} 
                            SET ID_Receta =  {CabeceraReceta.ID},
                                N_Etapa = {CabeceraEtapa.N_Etapa},
                                Tipo = '{Consigna.Tipo}',
                                Consigna = '{Consigna.Consigna}',
                                Valor = {Consigna.Valor}
                            WHERE ID LIKE {Consigna.ID}");

     
            return querys[Consulta];
        }

        public string Delete(int Consulta, string Tabla, int ID)
        {
            List<string> querys = new List<string>();
            
            //0 - Se eliminan todas las entradas que ya no existen en la receta
            querys.Add($@"DELETE FROM {Tabla} WHERE ID = {ID}");

            return querys[Consulta];
        }

    }
}

