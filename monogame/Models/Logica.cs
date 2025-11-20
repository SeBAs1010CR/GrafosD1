using System.Collections.Generic;

namespace Proyecto.Models
{
    public class Logica
    {
        public List<Persona> Personas { get; set; } = new();

        public ArbolGenealogico Arbol { get; private set; }
        public GrafoPersonas Grafo { get; private set; }

        public Logica()
        {
            Arbol = new ArbolGenealogico(Personas);
            Grafo = new GrafoPersonas();
        }

        public void AgregarPersona(Persona p)
        {
            Personas.Add(p);
            Grafo.AgregarPersona(p);
            Grafo.Reconstruir();
        }
    }
}
