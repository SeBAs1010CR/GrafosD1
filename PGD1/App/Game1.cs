using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Proyecto.Models;
using ProyectoUI; // donde está FormAgregarPersona
using System;
using System.Collections.Generic;


namespace Proyecto
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _mapTexture;
        private Texture2D _circleTexture;
        private Texture2D _pixelTexture;

        private Texture2D _blackCanvas;

        private SpriteFont _font;

        private UIManager _ui;
        private GrafoResidencias _grafo;
        private FormAgregarPersona _form;
        private bool verArbol = false;
        


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

        private Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int diameter)
        {
            Texture2D texture = new Texture2D(graphicsDevice, diameter, diameter);
            Color[] data = new Color[diameter * diameter];

            float radius = diameter / 2f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);

                    if (dist <= radius)
                        data[y * diameter + x] = Color.White;
                    else
                        data[y * diameter + x] = Color.Transparent;
                }
            }

            texture.SetData(data);
            return texture;
        }


        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // 🗺️ Mapa de fondo
            _mapTexture = Content.Load<Texture2D>("mapamundi");

            // 🔤 Fuente
            _font = Content.Load<SpriteFont>("DefaultFont"); // asegúrate que existe DefaultFont.spritefont
            _circleTexture = CreateCircleTexture(GraphicsDevice, 50);

    // ▪️ Textura 1x1 para dibujar líneas
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            // 🧭 Interfaz de usuario
            _ui.LoadContent(Content, GraphicsDevice);

            // 🧍 Formulario CRUD
            _form = new FormAgregarPersona(_grafo);
            _form.LoadContent(GraphicsDevice, _font);
            _form.OnGuardar = (persona) =>
            {
                _grafo.AgregarNodo(persona);
                _grafo.AsignarPosicionesJerarquicas();
                System.Console.WriteLine($"Persona agregada: {persona.Nombre} ({persona.Cedula})");
            };
            _blackCanvas = new Texture2D(GraphicsDevice, 1, 1);
            _blackCanvas.SetData(new[] { Color.Black });
            _ui.OnAlternarClick = () =>
            {
                verArbol = !verArbol;   
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
            DrawArbolGenealogico(_spriteBatch);
            // 👥 Dibuja las personas del grafo
            foreach (var persona in _grafo.ListarPersonas())
            {
                Vector2 pos = ConvertirCoordenadas(persona.Latitud, persona.Longitud);
                _spriteBatch.Draw(_ui.DefaultPhoto, new Rectangle((int)pos.X, (int)pos.Y, 40, 40), Color.Black);
                _spriteBatch.DrawString(_font, persona.Nombre, pos + new Vector2(0, 45), Color.Black);
            }

            // 🧮 Dibuja interfaz y formulario
            _ui.Draw(_spriteBatch, _font);

            _form.Draw(_spriteBatch, GraphicsDevice);
            

            if (verArbol)
            {
                // Lienzo
                _spriteBatch.Draw(_blackCanvas, new Rectangle(550, 0, 740, 720), Color.Black);

                DrawArbolGenealogico(_spriteBatch);
            }
            else
            {
                // Dibujas mapa normal
                _spriteBatch.Draw(_mapTexture, new Rectangle(0, 0, 1280, 720), Color.White);

                // Dibuja personas en el mapa
                foreach (var persona in _grafo.ListarPersonas())
                {
                    Vector2 pos = ConvertirCoordenadas(persona.Latitud, persona.Longitud);
                    _spriteBatch.Draw(_ui.DefaultPhoto, new Rectangle((int)pos.X, (int)pos.Y, 40, 40), Color.White);
                    _spriteBatch.DrawString(_font, persona.Nombre, pos + new Vector2(0, 45), Color.Black);
                }
            }



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
        private void DrawArbolGenealogico(SpriteBatch sb)
        {
            foreach (var persona in _grafo.ListarPersonas())
            {
                Vector2 pos = persona.Position;

                // Nodo (círculo)
                sb.Draw(_circleTexture, new Rectangle((int)pos.X - 25, (int)pos.Y - 25, 50, 50), Color.White);

                // Nombre
                sb.DrawString(_font, persona.Nombre, pos + new Vector2(30, -5), Color.White);

                // Línea al padre
                if (persona.Padre != null)
                {
                    Vector2 padrePos = persona.Padre.Position;
                    DrawLine(sb,
                        new Vector2(pos.X, pos.Y - 25),
                        new Vector2(padrePos.X, padrePos.Y + 25),
                        Color.White);
                }

                // Línea a la madre
                if (persona.Madre != null)
                {
                    Vector2 madrePos = persona.Madre.Position;
                    DrawLine(sb,
                        new Vector2(pos.X - 10, pos.Y - 25),
                        new Vector2(madrePos.X, madrePos.Y + 25),
                        Color.LightPink);
                }
            }
        }

        private void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            sb.Draw(
                _pixelTexture,
                new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 2),
                null,
                color,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0f
            );
        }
    }
}
