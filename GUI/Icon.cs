using CompSci_NEA.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CompSci_NEA.GUI
{
    public class Icon
    {
        public Vector2 position;

        private Texture2D iconTexture;
        private Color iconColour;
        private float scale;
        private string text;
        private Text _visText;
        private Vector2 textRelativePos; //relative to the icon's pos

        public Icon(Texture2D texture, Vector2 position, Color colour, float scale = 3f)
        {
            iconTexture = texture;
            this.position = position;
            iconColour = colour;
            this.scale = scale;
            text = null;
        }

        public Icon(Texture2D texture, Vector2 position, Color colour, float scale, string text, Vector2 textRelativePos)
        {
            iconTexture = texture;
            this.position = position;
            iconColour = colour;
            this.scale = scale;
            this.text = text;
            this.textRelativePos = textRelativePos;
            _visText = new Text(TextureManager.DefaultFont, text, Vector2.Zero, colour, scale/2);
            _visText.position = position + textRelativePos;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(iconTexture, position, null, iconColour, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            if (text != null)
            {
                //_visText.position = position + textRelativePos; //probably not necessary - add back if errors
                _visText.Draw(spriteBatch);
            }
        }

        public void UpdateText(string newContent)
        {
            if (text != null)
            {
                _visText.UpdateContent(newContent);
            }
        }
    }
}
