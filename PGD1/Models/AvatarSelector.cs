using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace PGD1.Models
{
    public class AvatarSelector
    {
        public bool Visible = false;
        public Action<string> OnAvatarSelected;

        private Texture2D[] _avatares;
        private SpriteFont _font;
        private Texture2D _panelNegro;

        public void LoadContent(GraphicsDevice gd, SpriteFont font, Func<string, Texture2D> cargar)
        {
            _font = font;

            // Fondo semitransparente
            _panelNegro = new Texture2D(gd, 1, 1);
            _panelNegro.SetData(new[] { new Color(0, 0, 0, 200) });

            // Cargar 9 avatares AV1, AV2… AV9
            _avatares = new Texture2D[9];
            for (int i = 0; i < 9; i++)
                _avatares[i] = cargar($"AV{i + 1}");
        }

        public void Mostrar() => Visible = true;
        public void Ocultar() => Visible = false;

        public void Update()
        {
            if (!Visible) return;

            var mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                for (int i = 0; i < 9; i++)
                {
                    int col = i % 3;
                    int row = i / 3;

                    Rectangle rect = new Rectangle(100 + col * 150, 150 + row * 150, 100, 100);

                    if (rect.Contains(mouse.Position))
                    {
                        OnAvatarSelected?.Invoke($"AV{i + 1}");
                        Ocultar();
                    }
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (!Visible) return;

            // Fondo dim
            sb.Draw(_panelNegro, new Rectangle(0, 0, 1400, 700), Color.White);

            sb.DrawString(_font, "Seleccione un Avatar", new Vector2(175, 80), Color.White);

            for (int i = 0; i < 9; i++)
            {
                int col = i % 3;
                int row = i / 3;

                Rectangle rect = new Rectangle(100 + col * 150, 150 + row * 150, 100, 100);
                sb.Draw(_avatares[i], rect, Color.White);
            }
        }
    }
}
