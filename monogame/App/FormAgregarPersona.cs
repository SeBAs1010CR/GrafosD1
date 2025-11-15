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

        private string nombre = "";
        private string cedula = "";
        private string fechaNac = "";
        private string latitud = "";
        private string longitud = "";

        private int campoActivo = 0; // 0=nombre, 1=cedula, 2=fecha, 3=lat, 4=lon
        private bool visible = false;

        public Action<Persona> OnGuardar; // callback al guardar

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
                campoActivo = (campoActivo + 1) % 5;

            // Escribir texto en el campo activo
            foreach (var key in kb.GetPressedKeys())
            {
                if (_prevKeyboard.IsKeyUp(key))
                {
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
                        }
                    }
                }
            }

            // Guardar con Enter
            if (kb.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
            {
                try
                {
                    var persona = new Persona(
                        nombre,
                        cedula,
                        DateTime.Parse(fechaNac),
                        double.Parse(latitud),
                        double.Parse(longitud)
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
            sb.Draw(_panelTex, new Rectangle(200, 100, 700, 400), Color.White);

            // Campos
            string[] etiquetas = { "Nombre", "Cedula", "Fecha Nac (YYYY-MM-DD)", "Latitud", "Longitud" };
            string[] valores = { nombre, cedula, fechaNac, latitud, longitud };

            for (int i = 0; i < etiquetas.Length; i++)
            {
                int y = 150 + i * 60;
                sb.DrawString(_font, etiquetas[i], new Vector2(220, y), Color.White);
                sb.Draw(_inputTex, new Rectangle(420, y - 5, 300, 30), i == campoActivo ? Color.White : Color.Gray);
                sb.DrawString(_font, valores[i], new Vector2(430, y), Color.Black);
            }

            sb.DrawString(_font, "Presione ENTER para guardar o ESC para cancelar", new Vector2(230, 450), Color.Yellow);
            
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
