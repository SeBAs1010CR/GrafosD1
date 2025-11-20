using System.Collections.Generic;
using System.Linq;

namespace Proyecto.Models
{
    public class ArbolGenealogico
    {
        private readonly List<Persona> _personas;

        public ArbolGenealogico(List<Persona> personas)
        {
            _personas = personas;
        }

        // Padre directo
        public Persona ObtenerPadre(Persona persona)
        {
            return persona.Padre;
        }

        // Madre directa
        public Persona ObtenerMadre(Persona persona)
        {
            return persona.Madre;
        }

        // Hijos directos
        public List<Persona> ObtenerHijos(Persona persona)
        {
            return persona.Hijos.ToList(); 
        }

        // Hermanos: comparten padre y madre
        public List<Persona> ObtenerHermanos(Persona persona)
        {
            return _personas
                .Where(p =>
                    p != persona &&
                    p.Padre == persona.Padre &&
                    p.Madre == persona.Madre)
                .ToList();
        }
    }
}
