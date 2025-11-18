using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Dynamic;

namespace Proyecto.Models
{
    public class Persona
    {
        public Guid id { set; get; } = Guid.NewGuid();
        public string Nombre { set; get; }
        public string Cedula { set; get; }
        public DateTime FechaNacimiento { set; get; }
        public DateTime? FechaDefuncion { set; get; }
        public double Latitud { set; get; }
        public double Longitud { set; get; }
        public string FotoPath { set; get; }
        public Persona Madre { set; get; }
        public Persona Padre { set; get; }
        public Vector2 Position { get; set; }

        
        // RELACIONES FAMILIARES (nuevo q hice, atte: dilan)
        public Persona Padre { get; set; }
        public Persona Madre { get; set; }
        public List<Persona> Hijos { get; set; } = new List<Persona>();
        public Persona Pareja { get; set; }

        public int Edad =>
            (FechaDefuncion ?? DateTime.Now).Year - FechaNacimiento.Year -
            ((FechaDefuncion ?? DateTime.Now).DayOfYear < FechaNacimiento.DayOfYear ? 1 : 0);

        public bool EstaVivo => FechaDefuncion == null;

        public Persona(string nombre, string cedula, DateTime fechaNacimiento, double lat, double lon, string fotoPath = null)
        public Persona(string nombre, string cedula, DateTime fechaNacimiento, double lat, double lon, Persona madre, Persona padre, string fotoPath = null)
        {
            Nombre = nombre;
            Cedula = cedula;
            FechaNacimiento = fechaNacimiento;
            Latitud = lat;
            Longitud = lon;
            FotoPath = fotoPath;
            Madre = madre;
            Padre = padre;
        }

        // MÉTODOS PARA VALIDACIONES FAMILIARES
        public bool EsAscendienteDe(Persona posibleDescendiente)
        {
            if (posibleDescendiente == null) return false;
            
            // Verificar si esta persona es padre/madRE o ascendiente
            return posibleDescendiente.Padre == this || 
                   posibleDescendiente.Madre == this ||
                   (posibleDescendiente.Padre != null && EsAscendienteDe(posibleDescendiente.Padre)) ||
                   (posibleDescendiente.Madre != null && EsAscendienteDe(posibleDescendiente.Madre));
        }

        public bool EsDescendienteDe(Persona posibleAscendiente)
        {
            return posibleAscendiente?.EsAscendienteDe(this) ?? false;
        }
    }
}