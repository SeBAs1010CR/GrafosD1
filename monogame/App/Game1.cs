using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Proyecto.Models;
using ProyectoUI; // donde está FormAgregarPersona
using System.Collections.Generic;

namespace Proyecto
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _mapTexture;
        private SpriteFont _font;

        private UIManager _ui;
        private GrafoResidencias _grafo;
        private FormAgregarPersona _form;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1400;  // Ancho deseado
            _graphics.PreferredBackBufferHeight = 700;  // Alto deseado
            _graphics.ApplyChanges();
            _grafo = new GrafoResidencias();
            _ui = new UIManager();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // 🗺️ Mapa de fondo
            _mapTexture = Content.Load<Texture2D>("mapamundi");

            // 🔤 Fuente
            _font = Content.Load<SpriteFont>("DefaultFont"); // asegúrate que existe DefaultFont.spritefont

            // 🧭 Interfaz de usuario
            _ui.LoadContent(Content, GraphicsDevice);

            // 🧍 Formulario CRUD
            _form = new FormAgregarPersona();
            _form.LoadContent(GraphicsDevice, _font);
            _form.OnGuardar = (persona) =>
            {
                _grafo.AgregarNodo(persona);
                System.Console.WriteLine($"Persona agregada: {persona.Nombre} ({persona.Cedula})");
            };
        }

        protected override void Update(GameTime gameTime)
        {
            // Salir del juego
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Mostrar formulario con tecla N
            if (Keyboard.GetState().IsKeyDown(Keys.N))
                _form.Mostrar();

            // Actualizar formulario y UI
            _form.Update(gameTime);
            _ui.Update(gameTime, _grafo);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin();

            // 🗺️ Dibuja el mapa
            _spriteBatch.Draw(_mapTexture, new Rectangle(0, 0, 1280, 720), Color.White);

            // 👥 Dibuja las personas del grafo
            foreach (var persona in _grafo.ListarPersonas())
            {
                Vector2 pos = ConvertirCoordenadas(persona.Latitud, persona.Longitud);
                _spriteBatch.Draw(_ui.DefaultPhoto, new Rectangle((int)pos.X, (int)pos.Y, 40, 40), Color.White);
                _spriteBatch.DrawString(_font, persona.Nombre, pos + new Vector2(0, 45), Color.Black);
            }

            // 🧮 Dibuja interfaz y formulario
            _ui.Draw(_spriteBatch, _font);
            _form.Draw(_spriteBatch, GraphicsDevice);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        // 🌍 Convierte latitud/longitud a posición en el mapa (simplificado)
        private Vector2 ConvertirCoordenadas(double lat, double lon)
        {
            float x = (float)((lon + 180) / 360 * 1280);
            float y = (float)((90 - lat) / 180 * 720);
            return new Vector2(x, y);
        }
    }
}
