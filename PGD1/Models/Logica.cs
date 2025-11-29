using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

namespace Proyecto.Models
{
    public class GrafoResidencias
    {
        private Dictionary<Persona, List<(Persona destino, double distancia)>> _adyacencias = new();

        public void AgregarNodo(Persona persona)
        {
            if (!_adyacencias.ContainsKey(persona))
                _adyacencias[persona] = new List<(Persona, double)>();
        }
        private int CalcularNivelPersona(Persona persona, Dictionary<Persona, int> memo)
        {
            if (memo.ContainsKey(persona))
                return memo[persona];

            // Si no tiene padres, está en la raíz (nivel 0)
            int nivelPadre = -1;
            int nivelMadre = -1;

            if (persona.Padre != null)
                nivelPadre = CalcularNivelPersona(persona.Padre, memo);

            if (persona.Madre != null)
                nivelMadre = CalcularNivelPersona(persona.Madre, memo);

            int nivel = Math.Max(nivelPadre, nivelMadre) + 1; // si no tiene padres => -1 + 1 = 0

            memo[persona] = nivel;
            return nivel;
        }
        public void AsignarPosicionesJerarquicas()
        {
            var personas = ListarPersonas();
            if (personas.Count == 0) return;

            // 1. Calcular nivel de cada persona
            var niveles = new Dictionary<Persona, int>();
            foreach (var p in personas)
            {
                CalcularNivelPersona(p, niveles);
            }

            // 2. Agrupar por nivel
            var grupos = niveles
                .GroupBy(kv => kv.Value)
                .OrderBy(g => g.Key)
                .ToList();

            // 3. Parámetros del layout
            float inicioX = 300f;
            float anchoPanel = 650f;
            float inicioY = 80f;
            float alturaNivel = 120f;

            // 4. Posicionar las personas por nivel
            foreach (var grupo in grupos)
            {
                int nivel = grupo.Key;
                var personasEnNivel = grupo.Select(kv => kv.Key).ToList();
                int count = personasEnNivel.Count;

                float espacio = (anchoPanel - 100) / Math.Max(count, 1);

                for (int i = 0; i < count; i++)
                {
                    var persona = personasEnNivel[i];

                    float x = inicioX + (i + 1) * espacio;
                    float y = inicioY + nivel * alturaNivel;

                    persona.Position = new Vector2(x, y);
                }
            }

            // 5.POSICIONAR PAREJAS
            foreach (var p in personas)
            {
                if (p.Pareja != null)
                {
                    p.Pareja.Position = new Vector2(
                        p.Position.X + 80,  // distancia horizontal
                        p.Position.Y        // misma altura, mismo nivel
                    );
                }
            }
        }



        public void Conectar(Persona a, Persona b)
        {
            double distancia = CalcularDistancia(a, b);
            _adyacencias[a].Add((b, distancia));
            _adyacencias[b].Add((a, distancia));
        }

        public double CalcularDistancia(Persona a, Persona b)
        {
            double R = 6371; // radio Tierra en km
            double dLat = (b.Latitud - a.Latitud) * Math.PI / 180;
            double dLon = (b.Longitud - a.Longitud) * Math.PI / 180;
            double lat1 = a.Latitud * Math.PI / 180;
            double lat2 = b.Latitud * Math.PI / 180;

            double hav = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                         Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            return 2 * R * Math.Asin(Math.Sqrt(hav));
        }


        
///////// Distancia desde un nodo /////////////////////////////
        public List<(Persona destino, double distancia)> DistanciasDesde(Persona origen)
        {
            var lista = new List<(Persona destino, double distancia)>();

            foreach (var p in _adyacencias.Keys)
            {
                if (p == origen) continue;
                double d = CalcularDistancia(origen, p);
                lista.Add((p, d));
            }

            return lista.OrderBy(x => x.distancia).ToList();
        }

////////  PAR MAS LEJOS /////////////////////////////

        public (Persona A, Persona B, double distancia) ParMasLejos()
        {
            double maxD = -1;
            Persona A = null, B = null;

            var personas = ListarPersonas();

            for (int i = 0; i < personas.Count; i++)
            {
                for (int j = i + 1; j < personas.Count; j++)
                {
                    double d = CalcularDistancia(personas[i], personas[j]);
                    if (d > maxD)
                    {
                        maxD = d;
                        A = personas[i];
                        B = personas[j];
                    }
                }
            }

            return (A, B, maxD);
        }

////////  PAR MAS CERCA  /////////////////////////////

        public (Persona A, Persona B, double distancia) ParMasCerca()
        {
            double minD = double.MaxValue;
            Persona A = null, B = null;

            var personas = ListarPersonas();

            for (int i = 0; i < personas.Count; i++)
            {
                for (int j = i + 1; j < personas.Count; j++)
                {
                    double d = CalcularDistancia(personas[i], personas[j]);
                    if (d < minD)
                    {
                        minD = d;
                        A = personas[i];
                        B = personas[j];
                    }
                }
            }

            return (A, B, minD);
        }
  ////////   Dist promedio /////////////////////////////      
        public double DistanciaPromedio()
        {
            var personas = ListarPersonas();
            double suma = 0;
            int count = 0;

            for (int i = 0; i < personas.Count; i++)
            {
                for (int j = i + 1; j < personas.Count; j++)
                {
                    suma += CalcularDistancia(personas[i], personas[j]);
                    count++;
                }
            }

            if (count == 0) return 0;

            return suma / count;
        }

   

        ////////////////////////CRUD///////////////////////////////////
        // CREATE
        public void AgregarPersona(Persona persona)
        {
            if (_adyacencias.Keys.Any(p => p.Cedula == persona.Cedula))
                throw new Exception("Ya existe una persona con esa cédula.");
            _adyacencias[persona] = new List<(Persona, double)>();
        }
        public void AsignarPosiciones()
        {
            int x = 100;
            int y = 100;
            int stepX = 200;
            int stepY = 150;

            int index = 0;

            foreach (var persona in _adyacencias.Keys)
            {
                persona.Position = new Vector2(
                    x + (index % 5) * stepX,
                    y + (index / 5) * stepY
                );
                index++;
            }
        }


        // READ
        public Persona BuscarPorCedula(string cedula)
        {
            return _adyacencias.Keys.FirstOrDefault(p => p.Cedula == cedula);
        }

        public List<Persona> ListarPersonas()
        {
            return _adyacencias.Keys.ToList();
        }

        // UPDATE
        public void ActualizarPersona(string cedula, string nuevoNombre, double nuevaLat, double nuevaLon)
        {
            var persona = BuscarPorCedula(cedula);
            if (persona == null) throw new Exception("Persona no encontrada.");

            persona.Nombre = nuevoNombre;
            persona.Latitud = nuevaLat;
            persona.Longitud = nuevaLon;
        }

        // DELETE
        public void EliminarPersona(string cedula)
        {
            var persona = BuscarPorCedula(cedula);
            if (persona == null) throw new Exception("Persona no encontrada.");

            foreach (var vecinos in _adyacencias.Values)
                vecinos.RemoveAll(x => x.destino == persona);

            _adyacencias.Remove(persona);
        }
        public void ConectarParentesco(Persona padre, Persona hijo)
        {
            hijo.Padre = padre;
            AgregarNodo(padre);
            AgregarNodo(hijo);
        }
        public void AgregarPersona(Persona persona, int index)
        {
            // Distribuir a las personas en una cuadrícula
            int cols = 5;   // máximo 5 personas por fila
            int x = 100 + (index % cols) * 150;
            int y = 100 + (index / cols) * 150;

            persona.Position = new Vector2(x, y);

            _adyacencias[persona] = new List<(Persona, double)>();
        }
        public Persona BuscarPorNombre(string nombre)
        {
            return _adyacencias.Keys.FirstOrDefault(p => p.Nombre == nombre);
        }

        public List<Persona> ObtenerHermanos(Persona persona)
        {
            return _adyacencias.Keys
                .Where(p => p.Padre == persona.Padre && p != persona)
                .ToList();
        }

        public List<Persona> ObtenerHijos(Persona p)
        {
            return _adyacencias.Keys.Where(h => h.Padre == p || h.Madre == p).ToList();
        }



    }

}