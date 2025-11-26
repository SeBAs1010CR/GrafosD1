using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Proyecto.Models;
using System;

namespace Proyecto
{
    public class UIManager
    {
        private Texture2D _buttonTexture;

        private Rectangle _btnAgregar;
        private Rectangle _btnEliminar;
        private Rectangle _btnAlternar;

        private MouseState _prevMouse;
        public Action OnAlternarClick;


        // Tamaño de botones
        private const int BUTTON_WIDTH = 150;
        private const int BUTTON_HEIGHT = 40;
        private const int BUTTON_SPACING = 10;

        // Ancho del mapa dado por el usuario
        private const int MAP_WIDTH = 1375;
        private const int BUTTON_X = MAP_WIDTH - 100; // 900

        public Texture2D DefaultPhoto { get; private set; }

        

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
                        rnd.Next(-180, 180),
                        null,

                        null
                    );
                    grafo.AgregarNodo(persona);
                    System.Console.WriteLine($"Persona agregada: {persona.Nombre}");
                }

                if (_btnEliminar.Contains(mousePos))
                {
                    // grafo.EliminarPersona();
                    System.Console.WriteLine("Persona eliminada");
                }
                if (mouse.LeftButton == ButtonState.Pressed && 
                    _prevMouse.LeftButton == ButtonState.Released)
                {
                    if (_btnAlternar.Contains(mousePos))
                    {
                        OnAlternarClick?.Invoke();   
                    }
                }
            }

            _prevMouse = mouse;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _buttonTexture = new Texture2D(graphicsDevice, 1, 1);
            _buttonTexture.SetData(new[] { Color.White });

            DefaultPhoto = content.Load<Texture2D>("defaultPhoto");

            int y = 20;

            _btnAgregar = new Rectangle(
                BUTTON_X,
                y,
                BUTTON_WIDTH,
                BUTTON_HEIGHT
            );

            y += BUTTON_HEIGHT + BUTTON_SPACING;

            _btnEliminar = new Rectangle(
                BUTTON_X,
                y,
                BUTTON_WIDTH,
                BUTTON_HEIGHT
            );

            y += BUTTON_HEIGHT + BUTTON_SPACING;

            _btnAlternar = new Rectangle(
                BUTTON_X,
                y,
                BUTTON_WIDTH,
                BUTTON_HEIGHT
            );
        }
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Agregar
            spriteBatch.Draw(_buttonTexture, _btnAgregar, Color.LightGray);
            spriteBatch.DrawString(font, "Agregar",
                new Vector2(_btnAgregar.X + 20, _btnAgregar.Y + 10),
                Color.Black);

            // Eliminar
            spriteBatch.Draw(_buttonTexture, _btnEliminar, Color.LightGray);
            spriteBatch.DrawString(font, "Eliminar",
                new Vector2(_btnEliminar.X + 20, _btnEliminar.Y + 10),
                Color.Black);

            // Alternar (árbol / mapa)
            spriteBatch.Draw(_buttonTexture, _btnAlternar, Color.DarkGray);
            spriteBatch.DrawString(font,
                "Alternar",
                new Vector2(_btnAlternar.X + 20, _btnAlternar.Y + 10),
                Color.Black);
        }

        
    }
}
