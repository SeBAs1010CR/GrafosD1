using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Proyecto.Models;

namespace Proyecto
{
    public class UIManager
    {
        private Texture2D _buttonTexture;
        private Rectangle _btnAgregar, _btnEliminar;
        private MouseState _prevMouse;

        public Texture2D DefaultPhoto { get; private set; }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            // Crear textura de 1x1 píxel para dibujar botones simples
            _buttonTexture = new Texture2D(graphicsDevice, 1, 1);
            _buttonTexture.SetData(new[] { Color.White });

            // Imagen por defecto para las personas
            DefaultPhoto = content.Load<Texture2D>("defaultPhoto");

            // Posiciones de los botones
            _btnAgregar = new Rectangle(1200, 20, 150, 40);
            _btnEliminar = new Rectangle(1200, 80, 150, 40);
        }

        public void Update(GameTime gameTime, GrafoResidencias grafo)
        {
            var mouse = Mouse.GetState();
            var mousePos = new Point(mouse.X, mouse.Y);

            // Clic izquierdo
            if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
            {
                if (_btnAgregar.Contains(mousePos))
                {
                    // Agrega una persona aleatoria de prueba
                    var rnd = new System.Random();
                    var persona = new Persona(
                        $"Persona{rnd.Next(100)}",
                        rnd.Next(1000, 9999).ToString(),
                        new System.DateTime(1990 + rnd.Next(20), rnd.Next(1, 12), rnd.Next(1, 28)),
                        rnd.Next(-90, 90),
                        rnd.Next(-180, 180)
                    );
                    grafo.AgregarNodo(persona);
                    System.Console.WriteLine($"Persona agregada: {persona.Nombre}");
                }

                if (_btnEliminar.Contains(mousePos))
                {
                    // grafo.EliminarPersona();
                    System.Console.WriteLine("Persona eliminada");
                }
            }

            _prevMouse = mouse;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Botón Agregar
            spriteBatch.Draw(_buttonTexture, _btnAgregar, Color.LightGray);
            spriteBatch.DrawString(font, "Agregar", new Vector2(_btnAgregar.X + 20, _btnAgregar.Y + 10), Color.Black);

            // Botón Eliminar
            spriteBatch.Draw(_buttonTexture, _btnEliminar, Color.LightGray);
            spriteBatch.DrawString(font, "Eliminar", new Vector2(_btnEliminar.X + 20, _btnEliminar.Y + 10), Color.Black);
        }
    }
}
