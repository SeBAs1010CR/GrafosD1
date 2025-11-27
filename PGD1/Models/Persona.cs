using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public Vector2 Position { get; set; }
        // RELACIONES FAMILIARES
        public Persona Madre { set; get; }
        public Persona Padre { set; get; }
        public List<Persona> Hijos { get; set; } = new List<Persona>();
        public Persona Pareja { get; set; }

        public Texture2D Foto { get; set; }


        // PROPIEDADES CALCULADAS
        public int Edad
        {
            get
            {
                DateTime fechaFinal = FechaDefuncion ?? DateTime.Now;
                int edad = fechaFinal.Year - FechaNacimiento.Year;
                
                // Ajustar si aún no ha cumplido años este año
                if (fechaFinal < FechaNacimiento.AddYears(edad)) 
                    edad--;
                    
                return edad;
            }
        }

        public bool EstaVivo => FechaDefuncion == null;

        // CONSTRUCTOR CORREGIDO
        public Persona(string nombre, string cedula, DateTime fechaNacimiento, 
                      double lat, double lon, string fotoPath = null,
                      Persona madre = null, Persona padre = null)  // Parámetros opcionales
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

        // CONSTRUCTOR SIN PARENTESCO (para flexibilidad)
        public Persona(string nombre, string cedula, DateTime fechaNacimiento, 
                      double lat, double lon, string fotoPath = null)
        {
            Nombre = nombre;
            Cedula = cedula;
            FechaNacimiento = fechaNacimiento;
            Latitud = lat;
            Longitud = lon;
            FotoPath = fotoPath;
        }

        // MÉTODOS PARA VALIDACIONES FAMILIARES
        public bool EsAscendienteDe(Persona posibleDescendiente)
        {
            if (posibleDescendiente == null) 
                return false;
            
            // Verificar si esta persona es padre/madre directo
            if (posibleDescendiente.Padre == this || posibleDescendiente.Madre == this)
                return true;
            
            // Verificar ascendencia recursiva
            bool porPadre = posibleDescendiente.Padre != null && EsAscendienteDe(posibleDescendiente.Padre);
            bool porMadre = posibleDescendiente.Madre != null && EsAscendienteDe(posibleDescendiente.Madre);
            
            return porPadre || porMadre;
        }

        public bool EsDescendienteDe(Persona posibleAscendiente)
        {
            return posibleAscendiente?.EsAscendienteDe(this) ?? false;
        }

        // MÉTODO PARA AGREGAR HIJO (mantiene consistencia)
        public void AgregarHijo(Persona hijo)
        {
            if (hijo == null) return;
            
            if (!Hijos.Contains(hijo))
            {
                Hijos.Add(hijo);
                
                // Establecer parentesco automáticamente
                if (this == hijo.Padre || this == hijo.Madre)
                {
                    // Ya está establecido, no hacer nada
                }
                else
                {
                    // Asumir que es la madre si no está establecido
                    if (hijo.Madre == null)
                        hijo.Madre = this;
                    // O el padre si la madre ya está establecida
                    else if (hijo.Padre == null)  
                        hijo.Padre = this;
                }
            }
        }

        // MÉTODO PARA VALIDAR DATOS BÁSICOS
        public bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
                return false;
                
            if (string.IsNullOrWhiteSpace(Cedula) || Cedula.Length < 9)
                return false;
                
            if (FechaNacimiento > DateTime.Now)
                return false;
                
            if (Latitud < -90 || Latitud > 90 || Longitud < -180 || Longitud > 180)
                return false;
                
            return true;
        }

        // OVERRIDE PARA MEJOR VISUALIZACIÓN
        public override string ToString()
        {
            return $"{Nombre} ({Cedula}) - {Edad} años";
        }
    }
}