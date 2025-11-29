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
                var datos = new DatosPersistencia();

                // Guardar cada persona con referencias a padres
                foreach (var persona in _familia.TodasLasPersonas)
                {
                    var personaData = new PersonaData
                    {
                        Persona = persona,
                        CedulaPadre = persona.Padre?.Cedula,
                        CedulaMadre = persona.Madre?.Cedula
                    };
                    datos.Personas.Add(personaData);
                }

                // Guardar relaciones de pareja
                datos.Parejas = ObtenerRelacionesParejas();

                string json = JsonSerializer.Serialize(datos, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                });

                File.WriteAllText(_archivoDatos, json);
                Console.WriteLine($" Datos guardados: {_familia.TotalPersonas} personas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error guardando datos: {ex.Message}");
            }
        }

        private List<RelacionPareja> ObtenerRelacionesParejas()
        {
            var relaciones = new List<RelacionPareja>();
            var personasProcesadas = new HashSet<string>();

            foreach (var persona in _familia.TodasLasPersonas)
            {
                if (persona.Pareja != null && !personasProcesadas.Contains(persona.Cedula))
                {
                    relaciones.Add(new RelacionPareja
                    {
                        Cedula1 = persona.Cedula,
                        Cedula2 = persona.Pareja.Cedula
                    });
                    personasProcesadas.Add(persona.Pareja.Cedula);
                }
            }
            return relaciones;
        }

        private void CargarDatos()
        {
            if (!File.Exists(_archivoDatos))
            {
                Console.WriteLine(" No hay datos previos para cargar - empezando con árbol vacío");
                return;
            }

            try
            {
                string json = File.ReadAllText(_archivoDatos);
                var datos = JsonSerializer.Deserialize<DatosPersistencia>(json);

                if (datos?.Personas == null)
                {
                    Console.WriteLine("Formato de archivo inválido");
                    return;
                }

                Console.WriteLine($" Cargando {datos.Personas.Count} personas del archivo...");

                // PRIMERA PASADA: Crear todas las personas sin relaciones
                var personasTemp = new Dictionary<string, Persona>();
                foreach (var personaData in datos.Personas)
                {
                    personasTemp[personaData.Persona.Cedula] = personaData.Persona;
                }

                // SEGUNDA PASADA: Reconstruir relaciones familiares
                foreach (var personaData in datos.Personas)
                {
                    var persona = personasTemp[personaData.Persona.Cedula];
                    Persona padre = null;
                    Persona madre = null;

                    // Buscar padre si existe
                    if (!string.IsNullOrEmpty(personaData.CedulaPadre) &&
                        personasTemp.ContainsKey(personaData.CedulaPadre))
                    {
                        padre = personasTemp[personaData.CedulaPadre];
                    }

                    // Buscar madre si existe
                    if (!string.IsNullOrEmpty(personaData.CedulaMadre) &&
                        personasTemp.ContainsKey(personaData.CedulaMadre))
                    {
                        madre = personasTemp[personaData.CedulaMadre];
                    }

                    // Agregar al árbol con sus relaciones
                    _familia.AgregarPersona(persona, padre, madre);
                }

                // TERCERA PASADA: Reconstruir relaciones de pareja
                foreach (var relacion in datos.Parejas)
                {
                    if (personasTemp.ContainsKey(relacion.Cedula1) &&
                        personasTemp.ContainsKey(relacion.Cedula2))
                    {
                        var persona1 = personasTemp[relacion.Cedula1];
                        var persona2 = personasTemp[relacion.Cedula2];
                        _familia.EstablecerPareja(persona1, persona2);
                    }
                }

                Console.WriteLine($" Árbol cargado exitosamente: {_familia.TotalPersonas} personas");
                Console.WriteLine($"Coherencia del árbol: {_familia.ValidarArbolCoherente()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error cargando datos: {ex.Message}");
                Console.WriteLine("Iniciando con árbol vacío debido al error");
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

        // CLASES AUXILIARES PARA PERSISTENCIA atte: dilan
        private class DatosPersistencia
        {
            public List<PersonaData> Personas { get; set; } = new List<PersonaData>();
            public List<RelacionPareja> Parejas { get; set; } = new List<RelacionPareja>();
        }

        private class PersonaData
        {
            public Persona Persona { get; set; }
            public string CedulaPadre { get; set; }
            public string CedulaMadre { get; set; }
        }

        private class RelacionPareja
        {
            public string Cedula1 { get; set; }
            public string Cedula2 { get; set; }
        }
        // MÉTODO PARA PROBAR LA PERSISTENCIA 
        public void ProbarPersistencia()
        {
            Console.WriteLine(" PROBANDO PERSISTENCIA DEL ÁRBOL:");
            Console.WriteLine($"- Personas en árbol: {TotalPersonas}");
            Console.WriteLine($"- Archivo de datos: {_archivoDatos}");
            Console.WriteLine($"- Árbol coherente: {ArbolEsCoherente}");
            
            // Mostrar relaciones de ejemplo
            if (TotalPersonas > 0)
            {
                var primeraPersona = ObtenerTodasPersonas().First();
                Console.WriteLine($"- Ejemplo: {primeraPersona.Nombre} tiene {primeraPersona.Hijos.Count} hijos");
                
                if (primeraPersona.Padre != null)
                    Console.WriteLine($"- Padre: {primeraPersona.Padre.Nombre}");
                if (primeraPersona.Madre != null)  
                    Console.WriteLine($"- Madre: {primeraPersona.Madre.Nombre}");
            }
        }

    }
}