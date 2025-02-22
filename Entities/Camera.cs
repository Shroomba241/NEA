using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace CompSci_NEA.Entities
{
    public class Camera
    {
        private Vector2 _position;
        private float _lerpAmount = 0.1f;
        private float _zoom = 1.0f; 
        private float _minZoom = 0.001f; //this is _zoom out
        private float _maxZoom = 4.0f; //this is _zoom in
        private float _zoomStep = 0.1f; 
        private int _previousScrollValue; 

        public Matrix Transform { get; private set; }

        public void Update(Vector2 targetPosition)
        {
            _zoomStep = (float)Math.Pow(2.7f, _zoom)/(_zoom+20)-0.05f;
            _position = Vector2.Lerp(_position, targetPosition, _lerpAmount);

            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.OemMinus)) _zoom = 1.0f;
            int scrollDelta = mouseState.ScrollWheelValue - _previousScrollValue;
            if (scrollDelta > 0) _zoom += _zoomStep;
            if (scrollDelta < 0) _zoom -= _zoomStep; 
            _zoom = MathHelper.Clamp(_zoom, _minZoom, _maxZoom);
            _previousScrollValue = mouseState.ScrollWheelValue;
            Transform =
                Matrix.CreateTranslation(new Vector3(-_position, 0)) *
                Matrix.CreateScale(_zoom) *
                Matrix.CreateTranslation(new Vector3(1920 / 2, 1080 / 2, 0));
        }
    }
}
