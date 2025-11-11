using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
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
        // CREATE
        public void AgregarPersona(Persona persona)
        {
            if (_adyacencias.Keys.Any(p => p.Cedula == persona.Cedula))
                throw new Exception("Ya existe una persona con esa cédula.");
            _adyacencias[persona] = new List<(Persona, double)>();
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
    }

}