using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;


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
        public int Edad =>
            (FechaDefuncion ?? DateTime.Now).Year - FechaNacimiento.Year -
            ((FechaDefuncion ?? DateTime.Now).DayOfYear < FechaNacimiento.DayOfYear ? 1 : 0);
        public Persona(string nombre, string cedula, DateTime fechaNacimiento, double lat, double lon, string fotoPath = null)
        {
            Nombre = nombre;
            Cedula = cedula;
            FechaNacimiento = fechaNacimiento;
            Latitud = lat;
            Longitud = lon;
            FotoPath = fotoPath;
        }
    }
}