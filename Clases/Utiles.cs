
// Compara dos listas de arrays de enteros y detecta diferencias por posición.
// Retorna una lista con los elementos que están en el primer listado pero no en el segundo.
// Si no hay diferencias en una posición, se retorna un array con 0.

using System;
using System.Collections.Generic;
using System.Data;

namespace GestionRecetas.Clases
{
    public class Utiles
    {

        public List<int[]> CompareListArrayINT(int LongListado, List<int[]> Listado1, List<int[]> Listado2)
        {
            List<int[]> ListDiferentes = new List<int[]>();

            for (int i = 0; i < LongListado; i++)
            {
                int[] Array1 = Listado1[i];
                int[] Array2 = Listado2[i];

                // Convertir los arrays a conjuntos para comparación eficiente
                HashSet<int> hashSet_Array1 = new HashSet<int>(Array1);
                HashSet<int> hashSet_Array2 = new HashSet<int>(Array2);

                // Obtener elementos que están en Array1 pero no en Array2
                HashSet<int> HasSet_Diferentes = new HashSet<int>(hashSet_Array1.Except(hashSet_Array2));

                if (HasSet_Diferentes.Count == 0)
                {
                    // Si no hay diferencias, se agrega un array con el valor 0
                    int[] Diferentes = new int[1];
                    Diferentes[0] = 0;
                    ListDiferentes.Add(Diferentes);
                }
                else
                {
                    // Si hay diferencias, se agregan al resultado
                    int[] Diferentes = HasSet_Diferentes.ToArray();
                    ListDiferentes.Add(Diferentes);
                }
            }
            return ListDiferentes;
        }
    }
}
