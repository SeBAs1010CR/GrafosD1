using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Proyecto.Models
{
    public class ArbolGenealogicoService
    {
        private Familia _familia;
        private string _archivoDatos = "arbol_familia.json";

        public ArbolGenealogicoService()
        {
            _familia = new Familia();
            CargarDatos();
        }

        // MÉTODO PRINCIPAL - Agregar persona al árbol
        public void AgregarPersona(string nombre, string cedula, DateTime fechaNacimiento,
                                 double latitud, double longitud, string fotoPath = null,
                                 string cedulaPadre = null, string cedulaMadre = null)
        {
            ValidarDatosPersona(nombre, cedula, fechaNacimiento, latitud, longitud);

            var padre = !string.IsNullOrEmpty(cedulaPadre) ? _familia.BuscarPorCedula(cedulaPadre) : null;
            var madre = !string.IsNullOrEmpty(cedulaMadre) ? _familia.BuscarPorCedula(cedulaMadre) : null;

            var persona = new Persona(nombre, cedula, fechaNacimiento, latitud, longitud, fotoPath);
            _familia.AgregarPersona(persona, padre, madre);

            GuardarDatos();
        }

        // VALIDACIONES DE DATOS 
        private void ValidarDatosPersona(string nombre, string cedula, DateTime fechaNacimiento, 
                                       double latitud, double longitud)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío");

            if (string.IsNullOrWhiteSpace(cedula) || cedula.Length < 9)
                throw new ArgumentException("La cédula debe tener al menos 9 caracteres");

            if (fechaNacimiento > DateTime.Now)
                throw new ArgumentException("La fecha de nacimiento no puede ser futura");

            // Validación de coordenadas (tu responsabilidad)
            if (latitud < -90 || latitud > 90)
                throw new ArgumentException("La latitud debe estar entre -90 y 90");

            if (longitud < -180 || longitud > 180)
                throw new ArgumentException("La longitud debe estar entre -180 y 180");
        }

        // CONSULTAS DEL ÁRBOL
        public List<Persona> ObtenerAncestros(string cedula)
        {
            var persona = _familia.BuscarPorCedula(cedula);
            return persona != null ? _familia.ObtenerAncestros(persona) : new List<Persona>();
        }

        public List<Persona> ObtenerDescendientes(string cedula)
        {
            var persona = _familia.BuscarPorCedula(cedula);
            return persona != null ? _familia.ObtenerDescendientes(persona) : new List<Persona>();
        }

        public List<Persona> ObtenerHermanos(string cedula)
        {
            var persona = _familia.BuscarPorCedula(cedula);
            return persona != null ? _familia.ObtenerHermanos(persona) : new List<Persona>();
        }

        // PERSISTENCIA (guardar/leer datos)
        private void GuardarDatos()
        {
            try
            {
                var datos = new
                {
                    Personas = _familia.TodasLasPersonas.ToList(),
                    FechaGuardado = DateTime.Now
                };

                string json = JsonSerializer.Serialize(datos, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_archivoDatos, json);
                Console.WriteLine($"Datos guardados: {_familia.TotalPersonas} personas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando datos: {ex.Message}");
            }
        }

        private void CargarDatos()
        {
            if (!File.Exists(_archivoDatos)) 
            {
                Console.WriteLine("No hay datos previos para cargar");
                return;
            }

            try
            {
                string json = File.ReadAllText(_archivoDatos);
                var datos = JsonSerializer.Deserialize<JsonElement>(json);
                
                if (datos.TryGetProperty("Personas", out var personasElement))
                {
                    var personas = JsonSerializer.Deserialize<List<Persona>>(personasElement.GetRawText());
                    
                    // Reconstruir el árbol
                    foreach (var persona in personas)
                    {
                        // Aquí necesitarías reconstruir las relaciones padre/madre
                        // basado en las cédulas guardadas en propiedades adicionales
                        _familia.AgregarPersona(persona);
                    }
                    
                    Console.WriteLine($"Datos cargados: {_familia.TotalPersonas} personas");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando datos: {ex.Message}");
                // Si hay error, empezar con árbol vacío
                _familia = new Familia();
            }
        }

        // MÉTODOS PÚBLICOS PARA LA UI
        public Persona BuscarPersona(string cedula) => _familia.BuscarPorCedula(cedula);
        public List<Persona> ObtenerTodasPersonas() => _familia.TodasLasPersonas.ToList();
        public int TotalPersonas => _familia.TotalPersonas;
        public bool ArbolEsCoherente => _familia.ValidarArbolCoherente();
        
        // MÉTODO PARA ESTABLECER PAREJA
        public void EstablecerPareja(string cedula1, string cedula2)
        {
            var persona1 = _familia.BuscarPorCedula(cedula1);
            var persona2 = _familia.BuscarPorCedula(cedula2);

            if (persona1 == null || persona2 == null)
                throw new InvalidOperationException("Una o ambas personas no existen");

            _familia.EstablecerPareja(persona1, persona2);
            GuardarDatos();
        }
    }
}