using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Proyecto.Models;
using ProyectoUI; // donde está FormAgregarPersona
using System;
using System.Collections.Generic;
using PGD1.Models;



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
        private Texture2D[] _avatars;
        private AvatarSelector _selector;
        
        private Persona personaSeleccionada = null;
        private Persona personaHover = null;

        private MouseState _prevMouse;

        private bool esperandoUbicacion = false;
        private Action<double, double> callbackUbicacion;
        public static Game1 Instancia { get; private set; }


        private Texture2D _infoIcon;
        private bool mostrarInfo = false;
        private Rectangle infoButtonRect = new Rectangle(10, 10, 30, 30);
        //Variable que agregue para poder usar arbolgenealogicoservice
        private ArbolGenealogicoService _arbolService;


        public Game1()
        {
            Instancia = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1400;  // Ancho deseado
            _graphics.PreferredBackBufferHeight = 700;  // Alto deseado
            _graphics.ApplyChanges();
            _arbolService = new ArbolGenealogicoService(); //Arbol DIlan
            _arbolService.ProbarPersistencia(); //para probar si se mantiene guardado el cochino arbol jaja
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
            _infoIcon = CreateCircleTexture(GraphicsDevice, 30);
            _avatars = new Texture2D[9];
            for (int i = 0; i < 9; i++)
            {
                _avatars[i] = Content.Load<Texture2D>($"AV{i + 1}");
            }

            // 2. Crear selector
            _selector = new AvatarSelector();
            _selector.LoadContent(GraphicsDevice, _font, name => Content.Load<Texture2D>(name));
            // 🧍 Formulario CRUD
            _form = new FormAgregarPersona(_grafo, _avatars);
            _form.LoadContent(GraphicsDevice, _font);
            _form.SetSelector(_selector);
            _form.OnGuardar = (persona) =>
            {
                //  
                _grafo.AgregarNodo(persona);
                _grafo.AsignarPosicionesJerarquicas();
                System.Console.WriteLine($"Persona agregada: {persona.Nombre} ({persona.Cedula})");
        
                try
                {
                    persona.Foto = Content.Load<Texture2D>(persona.FotoPath);
                }
                catch
                {
                    Console.WriteLine($"⚠ No se pudo cargar avatar '{persona.FotoPath}'. Se usará DefaultPhoto.");
                    persona.Foto = _ui.DefaultPhoto;
                }
                _grafo.AsignarPosicionesJerarquicas();
                System.Console.WriteLine($"Persona agregada: {persona.Nombre} ({persona.Cedula})");
                
                // Codigo que agregue atte: Dilan
                try
                {
                    _arbolService.AgregarPersona(
                        persona.Nombre,
                        persona.Cedula,
                        persona.FechaNacimiento,
                        persona.Latitud,
                        persona.Longitud,
                        persona.FotoPath,
                        cedulaPadre: _form.CedulaPadre,    // 🆕 Usar padre del formulario
                        cedulaMadre: _form.CedulaMadre     // 🆕 Usar madre del formulario
                    );
                    
                    Console.WriteLine($"Persona agregada al árbol genealógico: {persona.Nombre}");
                    
                    // 🆕 OPCIONAL: Establecer pareja si existe
                    if (!string.IsNullOrEmpty(_form.CedulaPareja))
                    {
                        _arbolService.EstablecerPareja(persona.Cedula, _form.CedulaPareja);
                        Console.WriteLine($" Pareja establecida: {persona.Nombre} con {_form.CedulaPareja}");
                    }
                    
                    // 🆕 Mostrar estadísticas del árbol
                    Console.WriteLine($"Árbol: {_arbolService.TotalPersonas} personas, Coherente: {_arbolService.ArbolEsCoherente}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Error en árbol genealógico: {ex.Message}");
                }
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

            

             if (Keyboard.GetState().IsKeyDown(Keys.F1))
{
            
            ElegirUbicacion((lat, lon) =>
            {
                Console.WriteLine($"Ubicación seleccionada: Lat={lat}, Lon={lon}");
                verArbol = true;    
                _form.Mostrar(); 
                _form.SetUbicacion(lat, lon);
                
                
            });
            

            _form.Ocultar();
            verArbol = false;
            Console.WriteLine("Modo seleccionar ubicación activado. Haga clic en el mapa.");
}


            if (esperandoUbicacion)
            {
                var mouseUb = Mouse.GetState();

                if (mouseUb.LeftButton == ButtonState.Pressed)
                {
                    // Coordenadas del mouse
                    Vector2 posMapa = mouseUb.Position.ToVector2();

                    double lon = (posMapa.X / 1280f) * 360 - 180;
                    double lat = 90 - (posMapa.Y / 720f) * 180;

                    esperandoUbicacion = false;
                    callbackUbicacion?.Invoke(lat, lon);
                    callbackUbicacion = null;
                }

                base.Update(gameTime);
                return;
            }
            // Salir del juego
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Mostrar formulario con tecla F2
            if (Keyboard.GetState().IsKeyDown(Keys.F2))
                _form.Mostrar();
            // Alternar árbol genealógico con coma (,)
            if (Keyboard.GetState().IsKeyDown(Keys.OemComma))
            {
                verArbol = true;
                _form.Mostrar();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.OemPeriod))
            {
                verArbol = false;  
                _form.Ocultar();   
            }

           
            // Actualizar formulario y UI
            _selector.Update();
            _form.Update(gameTime);
            _ui.Update(gameTime, _grafo);
            
            var mouse = Mouse.GetState();
            Point mousePos = mouse.Position;
            if (mouse.LeftButton == ButtonState.Pressed && 
                _prevMouse.LeftButton == ButtonState.Released)
            {
                if (infoButtonRect.Contains(mouse.Position))
                {
                    mostrarInfo = !mostrarInfo;
                }
            }

            // Reiniciar hover
            personaHover = null;
            
            // Detectar hover y click
            foreach (var persona in _grafo.ListarPersonas())
            {
                Vector2 pos = ConvertirCoordenadas(persona.Latitud, persona.Longitud);
                Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, 40, 40);

                // Detectar hover
                if (rect.Contains(mousePos))
                {
                    personaHover = persona;
                }

                // Detectar click
                if (mouse.LeftButton == ButtonState.Pressed && 
                    _prevMouse.LeftButton == ButtonState.Released &&
                    rect.Contains(mousePos))
                {
                    personaSeleccionada = persona;
                    Console.WriteLine($"Seleccionado: {persona.Nombre}");
                }
            


            }

            _prevMouse = mouse;


            base.Update(gameTime);
        }
        private void DrawInfoBox(SpriteBatch sb, Persona p)
        {
            if (p == null) return;

            var cerca = _grafo.ParMasCerca();
            var lejos = _grafo.ParMasLejos();
            double promedio = _grafo.DistanciaPromedio();

            string parCercaText = "N/A";
            if (cerca.A != null && cerca.B != null)
                parCercaText = $"{cerca.A.Nombre} - {cerca.B.Nombre} ({cerca.distancia:F2} km)";

            string parLejosText = "N/A";
            if (lejos.A != null && lejos.B != null)
                parLejosText = $"{lejos.A.Nombre} - {lejos.B.Nombre} ({lejos.distancia:F2} km)";

            string texto =
                $"Nombre: {p.Nombre}\n" +
                $"Cedula: {p.Cedula}\n" +
                $"Lat: {p.Latitud}\n" +
                $"Lon: {p.Longitud}\n\n" +
                "--- Estadisticas ---\n" +
                $"Par mas cerca: {parCercaText}\n" +
                $"Par mas lejos: {parLejosText}\n" +
                $"Promedio distancias: {promedio:F2} km\n";

            Vector2 pos = new Vector2(950, 400);
            Vector2 size = _font.MeasureString(texto);

            sb.Draw(_blackCanvas, new Rectangle((int)pos.X - 10, (int)pos.Y - 10,
                                                (int)size.X + 20, (int)size.Y + 20),
                                                Color.Black * 0.75f);

            sb.DrawString(_font, texto, pos, Color.White);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            // Dibuja el mapa
            _spriteBatch.Draw(_mapTexture, new Rectangle(0, 0, 1280, 720), Color.Black);
            
            DrawArbolGenealogico(_spriteBatch);
            // Dibuja las personas del grafo
            foreach (var persona in _grafo.ListarPersonas())
            {
                Vector2 pos = ConvertirCoordenadas(persona.Latitud, persona.Longitud);
                _spriteBatch.Draw(persona.Foto, new Rectangle((int)pos.X, (int)pos.Y, 40, 40), Color.Black);
                _spriteBatch.DrawString(_font, persona.Nombre, pos + new Vector2(0, 45), Color.White);
            }

            //Dibuja interfaz y formulario
            _ui.Draw(_spriteBatch, _font);

            _form.Draw(_spriteBatch, GraphicsDevice);
            _selector.Draw(_spriteBatch);

        
            

            

            if (verArbol)
            {
                // Lienzo
                _spriteBatch.Draw(_blackCanvas, new Rectangle(550, 0, 740, 800), Color.Black);

                DrawArbolGenealogico(_spriteBatch);
                
                foreach (var persona in _grafo.ListarPersonas())
                {
                    Vector2 pos = persona.Position;
                    _spriteBatch.Draw(persona.Foto, new Rectangle((int)pos.X-25, (int)pos.Y-25, 50, 50), Color.White);
                }
            }
            else
            {
                // Dibujas mapa normal
                _spriteBatch.Draw(_mapTexture, new Rectangle(0, 0, 1280, 720), Color.White);

                // Botón "i" de información
            _spriteBatch.Draw(_infoIcon, infoButtonRect, Color.CornflowerBlue);

            // Dibujar la "i" encima (si usaste CreateCircleTexture)
            _spriteBatch.DrawString(
                _font,
                "i",
                new Vector2(infoButtonRect.X + 10, infoButtonRect.Y + 5),
                Color.White
            );
            if (mostrarInfo)
            {
                Rectangle panel = new Rectangle(50, 50, 600, 250);

                // Fondo semitransparente
                _spriteBatch.Draw(_blackCanvas, panel, Color.Black * 0.7f);

                string texto =
                    "CONTROLES\n\n" +
                    "F1  - Seleccionar ubicacion en mapa\n" +
                    ", .  - Mostrar/ocultar arbol genealogico\n" +
                    "ESC - Salir\n\n" +
                    "Haga clic sobre una persona para ver distancias";

                _spriteBatch.DrawString(_font, texto, new Vector2(70, 70), Color.White);
            }
                
                if (personaHover != null)
                {
                    DrawInfoBox(_spriteBatch, personaHover);
                }
                else if (personaSeleccionada != null)
                
                {
                    var distancias = _grafo.DistanciasDesde(personaSeleccionada);

                    Vector2 origen = ConvertirCoordenadas(personaSeleccionada.Latitud, personaSeleccionada.Longitud) + new Vector2(20, 20);

                    foreach (var (destino, distancia) in distancias)
                    {
                        Vector2 destinoPos = ConvertirCoordenadas(destino.Latitud, destino.Longitud) + new Vector2(20, 20);

                        // Línea suave y elegante
                        DrawSmoothLine(
                            _spriteBatch,
                            origen,
                            destinoPos,
                            Color.White,
                            3f
                        );

                        // Poner texto en la mitad de la línea
                        Vector2 mid = (origen + destinoPos) / 2;

                        // Texto de distancia
                    string distText = $"{distancia:F1} km";
                    Vector2 textPos = mid + new Vector2(10, -10);
                    Vector2 textSize = _font.MeasureString(distText);

                    // Fondo negro con transparencia detrás del texto
                    Rectangle fondo = new Rectangle(
                        (int)textPos.X - 4,
                        (int)textPos.Y - 4,
                        (int)textSize.X + 8,
                        (int)textSize.Y + 8
                    );

                    _spriteBatch.Draw(_blackCanvas, fondo, Color.Black * 0.7f);

                    // Ahora sí, el texto encima
                    _spriteBatch.DrawString(
                        _font,
                        distText,
                        textPos,
                        Color.Red
                    );

                    }
                }
                // Dibuja personas en el mapa
                foreach (var persona in _grafo.ListarPersonas())
                {
                    Vector2 pos = ConvertirCoordenadas(persona.Latitud, persona.Longitud);
                    _spriteBatch.Draw(persona.Foto, new Rectangle((int)pos.X, (int)pos.Y, 40, 40), Color.White);
                    _spriteBatch.DrawString(_font, persona.Nombre, pos + new Vector2(0, 45), Color.Red);
                }
                if (esperandoUbicacion)
                {
                    _spriteBatch.DrawString(_font, 
                        "Haga clic en el mapa para seleccionar la ubicacion...",
                        new Vector2(400, 20),
                        Color.Yellow);

                    _spriteBatch.End();
                    return;
                }



            }



            _spriteBatch.End();

            base.Draw(gameTime);
        }

        //Convierte latitud/longitud a posición en el mapa (simplificado)
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
                DrawNameTag(sb, persona.Nombre, new Vector2(pos.X - 25, pos.Y - 45), 50);

                ///sb.DrawString(_font, persona.Nombre, pos + new Vector2(30, -5), Color.White);

                // Línea al padre
                if (persona.Padre != null)
                {
                    Vector2 padrePos = persona.Padre.Position;
                    DrawLine(sb,
                        new Vector2(pos.X, pos.Y - 25),
                        new Vector2(padrePos.X, padrePos.Y + 25),
                        Color.Cyan);
                }

                // Línea a la madre
                if (persona.Madre != null)
                {
                    Vector2 madrePos = persona.Madre.Position;
                    DrawLine(sb,
                        new Vector2(pos.X - 10, pos.Y - 25),
                        new Vector2(madrePos.X, madrePos.Y + 25),
                        Color.Cyan);
                }
                // Línea a la pareja 
                if (persona.Pareja != null)
                {
                    Vector2 parejaPos = persona.Pareja.Position;

                    // Dibujar línea horizontal (color distinto)
                    DrawLine(sb,
                        new Vector2(pos.X + 25, pos.Y),
                        new Vector2(parejaPos.X - 25, parejaPos.Y),
                        Color.Red);
                }

            }
        }
        public void ElegirUbicacion(Action<double, double> callback)
        {
            esperandoUbicacion = true;
            callbackUbicacion = callback;
        }
        private void DrawNameTag(SpriteBatch sb, string nombre, Vector2 posicion, int ancho)
        {
            // Altura fija del recuadro
            int alto = 20;

            // Rectángulo negro detrás del nombre
            Rectangle fondo = new Rectangle((int)posicion.X, (int)posicion.Y, ancho, alto);
            sb.Draw(_blackCanvas, fondo, Color.Black * 0.7f);  // 70% opaco

            // Dibujar texto centrado
            Vector2 size = _font.MeasureString(nombre);
            Vector2 textoPos = new Vector2(
                posicion.X + (ancho - size.X) / 2,
                posicion.Y + (alto - size.Y) / 2
            );

            sb.DrawString(_font, nombre, textoPos, Color.White);
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
        private void DrawSmoothLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            // Glow inferior (suaviza bordes)
            sb.Draw(_pixelTexture,
                new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), (int)(thickness * 2)),
                null,
                color * 0.25f,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0f);

            // Línea principal
            sb.Draw(_pixelTexture,
                new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), (int)thickness),
                null,
                color,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0f);
        }

    }
}
