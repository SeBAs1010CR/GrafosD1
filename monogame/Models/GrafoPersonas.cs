using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Proyecto.Models
{
    public class GrafoPersonas
    {
        // Lista de adyacencias: Persona -> (Vecino -> distancia)
        private readonly Dictionary<Persona, Dictionary<Persona, double>> _adyacencias = new();

        // ============================================
        // MÉTODOS REALES QUE TE HACÍAN FALTA
        // ============================================

        public void AgregarNodo(Persona p)
        {
            AgregarPersona(p);
            Reconstruir();
        }

        public IEnumerable<Persona> ListarPersonas()
        {
            return _adyacencias.Keys;
        }

        public Persona BuscarPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            return _adyacencias.Keys.FirstOrDefault(
                p => p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)
            );
        }

        // ----------------------------------------------------------
        // POSICIONES PARA ARBOL GENEALÓGICO (Game1 lo NECESITA)
        // ----------------------------------------------------------

        public void AsignarPosiciones()
        {
            // Posiciones "por defecto" usando lat/lon → mapa
            foreach (var persona in _adyacencias.Keys)
            {
                float x = (float)((persona.Longitud + 180) / 360f * 1280);
                float y = (float)((90 - persona.Latitud) / 180f * 720);
                persona.Position = new Vector2(x, y);
            }
        }

        public void AsignarPosicionesJerarquicas()
        {
            // Árbol genealógico: agrupar por generación
            var generaciones = new Dictionary<int, List<Persona>>();

            foreach (var persona in _adyacencias.Keys)
            {
                int gen = CalcularGeneracion(persona);
                if (!generaciones.ContainsKey(gen))
                    generaciones[gen] = new List<Persona>();
                generaciones[gen].Add(persona);
            }

            // Distribuir en columnas
            foreach (var kv in generaciones)
            {
                int gen = kv.Key;
                var personas = kv.Value;

                for (int i = 0; i < personas.Count; i++)
                {
                    personas[i].Position = new Vector2(
                        600 + i * 150,
                        100 + gen * 120
                    );
                }
            }
        }

        private int CalcularGeneracion(Persona p)
        {
            if (p.Padre == null && p.Madre == null)
                return 0;

            int padreGen = p.Padre != null ? CalcularGeneracion(p.Padre) : 0;
            int madreGen = p.Madre != null ? CalcularGeneracion(p.Madre) : 0;

            return Math.Max(padreGen, madreGen) + 1;
        }

        // ============================================
        // MÉTODOS DEL GRAFO ORIGINAL
        // ============================================

        public void AgregarPersona(Persona p)
        {
            if (!_adyacencias.ContainsKey(p))
                _adyacencias[p] = new Dictionary<Persona, double>();
        }

        // Reconstruye TODAS las distancias entre personas con coordenadas válidas
        public void Reconstruir()
        {
            foreach (var nodo in _adyacencias.Keys)
                _adyacencias[nodo].Clear();

            var lista = _adyacencias.Keys.ToList();

            for (int i = 0; i < lista.Count; i++)
            {
                for (int j = i + 1; j < lista.Count; j++)
                {
                    var a = lista[i];
                    var b = lista[j];

                    if (!TieneCoordenadas(a) || !TieneCoordenadas(b))
                        continue;

                    double d = GeoUtils.Haversine(
                        a.Latitud, a.Longitud,
                        b.Latitud, b.Longitud
                    );

                    _adyacencias[a][b] = d;
                    _adyacencias[b][a] = d;
                }
            }
        }

        private bool TieneCoordenadas(Persona p)
        {
            return !(p.Latitud == 0 && p.Longitud == 0);
        }

        // ---------------- ESTADÍSTICAS -----------------

        public (Persona A, Persona B, double Distancia) ParMasCercano()
        {
            double min = double.MaxValue;
            Persona pa = null, pb = null;

            foreach (var a in _adyacencias.Keys)
            {
                foreach (var kv in _adyacencias[a])
                {
                    if (kv.Value < min)
                    {
                        min = kv.Value;
                        pa = a;
                        pb = kv.Key;
                    }
                }
            }

            return (pa, pb, min);
        }

        public (Persona A, Persona B, double Distancia) ParMasLejano()
        {
            double max = double.MinValue;
            Persona pa = null, pb = null;

            foreach (var a in _adyacencias.Keys)
            {
                foreach (var kv in _adyacencias[a])
                {
                    if (kv.Value > max)
                    {
                        max = kv.Value;
                        pa = a;
                        pb = kv.Key;
                    }
                }
            }

            return (pa, pb, max);
        }

        public double DistanciaPromedio()
        {
            var distancias = new List<double>();

            foreach (var a in _adyacencias.Keys)
                foreach (var kv in _adyacencias[a])
                    distancias.Add(kv.Value);

            if (distancias.Count == 0)
                return 0;

            return distancias.Average();
        }
    }
}
