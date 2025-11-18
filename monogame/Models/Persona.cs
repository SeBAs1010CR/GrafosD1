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

        public int Edad =>
            (FechaDefuncion ?? DateTime.Now).Year - FechaNacimiento.Year -
            ((FechaDefuncion ?? DateTime.Now).DayOfYear < FechaNacimiento.DayOfYear ? 1 : 0);
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
    }
}