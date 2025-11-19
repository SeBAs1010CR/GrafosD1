using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Proyecto.Models;

namespace ProyectoUI
{
    
    public class FormAgregarPersona
    {
        private SpriteFont _font;
        private Texture2D _panelTex, _inputTex;
        private KeyboardState _prevKeyboard;
        private GrafoResidencias _grafo; 

        private string nombre = "";
        private string cedula = "";
        private string fechaNac = "";
        private string latitud = "";
        private string longitud = "";
        private string nombremadre = "";
        private string nombrepadre = "";

        private int campoActivo = 0; // 0=nombre, 1=cedula, 2=fecha, 3=lat, 4=lon 5=madre, 6=padre
        private bool visible = false;

        public Action<Persona> OnGuardar; // callback al guardar

        public FormAgregarPersona(GrafoResidencias grafo)
        {
            _grafo = grafo;
        }

        public void LoadContent(GraphicsDevice gd, SpriteFont font)
        {
            _font = font;
            _panelTex = new Texture2D(gd, 1, 1);
            _panelTex.SetData(new[] { new Color(0, 0, 0, 180) }); // semi-transparente
            _inputTex = new Texture2D(gd, 1, 1);
            _inputTex.SetData(new[] { Color.LightGray });
        }

        public void Mostrar() => visible = true;
        public void Ocultar() => visible = false;

        public void Update(GameTime gameTime)
        {
            if (!visible) return;

            var kb = Keyboard.GetState();

            // Cambiar campo activo con Tab
            if (kb.IsKeyDown(Keys.Tab) && _prevKeyboard.IsKeyUp(Keys.Tab))
                campoActivo = (campoActivo + 1) % 7;

            // Escribir texto en el campo activo
            foreach (var key in kb.GetPressedKeys())
            {
                if (_prevKeyboard.IsKeyUp(key))
                {
                    if (kb.IsKeyDown(Keys.Back) && _prevKeyboard.IsKeyUp(Keys.Back))
                    {
                        switch (campoActivo)
                        {
                            case 0:
                                if (nombre.Length > 0) nombre = nombre[..^1];
                                break;
                            case 1:
                                if (cedula.Length > 0) cedula = cedula[..^1];
                                break;
                            case 2:
                                if (fechaNac.Length > 0) fechaNac = fechaNac[..^1];
                                break;
                            case 3:
                                if (latitud.Length > 0) latitud = latitud[..^1];
                                break;
                            case 4:
                                if (longitud.Length > 0) longitud = longitud[..^1];
                                break;
                            case 5:
                                if (nombremadre.Length > 0) nombremadre = nombremadre[..^1];
                                break;
                            case 6:
                                if (nombrepadre.Length > 0) nombrepadre = nombrepadre[..^1];
                                break;
                        }
                    }
                    char c = KeyToChar(key);
                    if (c != '\0')
                    {
                    
                        switch (campoActivo)
                        {
                            case 0: nombre += c; break;
                            case 1: cedula += c; break;
                            case 2: fechaNac += c; break;
                            case 3: latitud += c; break;
                            case 4: longitud += c; break;
                            case 5: nombremadre += c; break;
                            case 6: nombrepadre += c; break;
                        }
                    }
                }
            }

            // Guardar con Enter
            if (kb.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
            {
                try
                {
                    var madreObj = _grafo.BuscarPorNombre(nombremadre); // método por agregar
                    var padreObj = _grafo.BuscarPorNombre(nombrepadre);
                   // FORMA CORRECTA - usando parámetros nombrados
                    var persona = new Persona(
                        nombre: nombre,
                        cedula: cedula,
                        fechaNacimiento: DateTime.Parse(fechaNac),
                        lat: double.Parse(latitud),
                        lon: double.Parse(longitud),
                        fotoPath: null,  // o tu ruta de foto si tienes
                        madre: madreObj,
                        padre: padreObj
                    );
                    OnGuardar?.Invoke(persona);
                    Ocultar();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            // Cancelar con Escape
            if (kb.IsKeyDown(Keys.Escape)) Ocultar();

            _prevKeyboard = kb;
        }

        public void Draw(SpriteBatch sb, GraphicsDevice gd)
        {
            if (!visible) return;

            
            // Panel principal
            sb.Draw(_panelTex, new Rectangle(20, 50, 500, 700), Color.White);
            

            // Campos
            string[] etiquetas = { "Nombre", "Cedula", "Fecha Nac (YYYY-MM-DD)", "Latitud", "Longitud", "Madre", "Padre"};
            string[] valores = { nombre, cedula, fechaNac, latitud, longitud, nombremadre, nombrepadre };

            for (int i = 0; i < etiquetas.Length; i++)
            {
                int y = 150 + i * 60;
                sb.DrawString(_font, etiquetas[i], new Vector2(40, y), Color.White);
                sb.Draw(_inputTex, new Rectangle(180, y - 5, 260, 30), i == campoActivo ? Color.White : Color.Gray);
                sb.DrawString(_font, valores[i], new Vector2(190, y), Color.Black);
            }

            sb.DrawString(_font, "Presione ENTER para guardar o ESC para cancelar", new Vector2(23, 550), Color.Yellow);
            
        }

        private char KeyToChar(Keys key)
        {
            // Traducción básica de teclas a caracteres
            if (key >= Keys.A && key <= Keys.Z)
                return (char)('a' + (key - Keys.A));
            if (key >= Keys.D0 && key <= Keys.D9)
                return (char)('0' + (key - Keys.D0));
            if (key == Keys.Space)
                return ' ';
            if (key == Keys.OemPeriod)
                return '.';
            if (key == Keys.OemMinus)
                return '-';
            return '\0';
        }
    }
}
