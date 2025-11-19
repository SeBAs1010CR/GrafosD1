//Clase Familia para gestionar el árbol completo
using System;
using System.Collections.Generic;
using System.Linq; //

namespace Proyecto.Models
{
    public class Familia //Clase Familia
    {
        public Persona Raiz { get; set; }
        private Dictionary<string, Persona> _personasPorCedula = new Dictionary<string, Persona>();

        public void AgregarPersona(Persona persona, Persona padre = null, Persona madre = null)
        {
            // Validar cédula única
            if (_personasPorCedula.ContainsKey(persona.Cedula))
                throw new InvalidOperationException($"Ya existe una persona con cédula {persona.Cedula}");

            // Validar relaciones familiares coherentes
            ValidarRelacionesFamiliares(persona, padre, madre);

            // Establecer relaciones
            if (padre != null)
            {
                persona.Padre = padre;
                padre.Hijos.Add(persona);
            }

            if (madre != null)
            {
                persona.Madre = madre;
                madre.Hijos.Add(persona);
            }

            _personasPorCedula[persona.Cedula] = persona;

            // Si es la primera persona, establecer como raíz
            if (Raiz == null)
                Raiz = persona;
        }

        private void ValidarRelacionesFamiliares(Persona persona, Persona padre, Persona madre)
        {
            // Validar que no sea su propio padre/madre
            if (persona == padre || persona == madre)
                throw new InvalidOperationException("Una persona no puede ser su propio padre/madre");

            // Validar que los padres no sean descendientes
            if (padre != null && persona.EsAscendienteDe(padre))
                throw new InvalidOperationException($"El padre {padre.Nombre} no puede ser descendiente de {persona.Nombre}");

            if (madre != null && persona.EsAscendienteDe(madre))
                throw new InvalidOperationException($"La madre {madre.Nombre} no puede ser descendiente de {persona.Nombre}");

            // Validar fechas coherentes (los padres deben ser mayores)
            if (padre != null && padre.FechaNacimiento >= persona.FechaNacimiento)
                throw new InvalidOperationException("El padre debe ser mayor que el hijo");

            if (madre != null && madre.FechaNacimiento >= persona.FechaNacimiento)
                throw new InvalidOperationException("La madre debe ser mayor que el hijo");

            // Validar que no se creen ciclos en el árbol
            if (padre != null && madre != null)
            {
                if (padre.EsAscendienteDe(madre) || madre.EsAscendienteDe(padre))
                    throw new InvalidOperationException("Los padres no pueden ser ascendientes entre sí");
            }
        }

        public void EstablecerPareja(Persona persona1, Persona persona2)
        {
            if (persona1 == persona2)
                throw new InvalidOperationException("Una persona no puede ser pareja de sí misma");

            if (persona1.EsAscendienteDe(persona2) || persona2.EsAscendienteDe(persona1))
                throw new InvalidOperationException("No se puede establecer pareja entre ascendiente y descendiente");

            persona1.Pareja = persona2;
            persona2.Pareja = persona1;
        }

        public Persona BuscarPorCedula(string cedula)
        {
            return _personasPorCedula.ContainsKey(cedula) ? _personasPorCedula[cedula] : null;
        }

        public List<Persona> ObtenerAncestros(Persona persona)
        {
            var ancestros = new List<Persona>();
            ObtenerAncestrosRecursivo(persona, ancestros);
            return ancestros;
        }

        private void ObtenerAncestrosRecursivo(Persona persona, List<Persona> ancestros)
        {
            if (persona.Padre != null)
            {
                ancestros.Add(persona.Padre);
                ObtenerAncestrosRecursivo(persona.Padre, ancestros);
            }
            if (persona.Madre != null)
            {
                ancestros.Add(persona.Madre);
                ObtenerAncestrosRecursivo(persona.Madre, ancestros);
            }
        }

        public List<Persona> ObtenerDescendientes(Persona persona)
        {
            var descendientes = new List<Persona>();
            ObtenerDescendientesRecursivo(persona, descendientes);
            return descendientes;
        }

        private void ObtenerDescendientesRecursivo(Persona persona, List<Persona> descendientes)
        {
            foreach (var hijo in persona.Hijos)
            {
                descendientes.Add(hijo);
                ObtenerDescendientesRecursivo(hijo, descendientes);
            }
        }

        public List<Persona> ObtenerHermanos(Persona persona)
        {
            var hermanos = new List<Persona>();
            
            if (persona.Padre != null)
            {
                hermanos.AddRange(persona.Padre.Hijos.Where(h => h != persona));
            }
            
            return hermanos.Distinct().ToList();
        }

        public bool ValidarArbolCoherente()
        {
            foreach (var persona in _personasPorCedula.Values)
            {
                // Verificar que nadie sea su propio ancestro
                if (persona.EsAscendienteDe(persona))
                    return false;

                // Verificar fechas coherentes
                foreach (var hijo in persona.Hijos)
                {
                    if (persona.FechaNacimiento >= hijo.FechaNacimiento)
                        return false;
                }
            }
            return true;
        }

        public int TotalPersonas => _personasPorCedula.Count;
        public IEnumerable<Persona> TodasLasPersonas => _personasPorCedula.Values;
    }
}