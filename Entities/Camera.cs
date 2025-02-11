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
        private Vector2 position;
        private float lerpAmount = 0.1f;
        private float zoom = 1.0f; 
        private float minZoom = 0.001f; //this is zoom out
        private float maxZoom = 4.0f; //this is zoom in
        private float zoomStep = 0.1f; 
        private int previousScrollValue; 

        public Matrix Transform { get; private set; }

        public void Update(Vector2 targetPosition)
        {
            zoomStep = (float)Math.Pow(2.7f, zoom)/(zoom+20)-0.05f;
            position = Vector2.Lerp(position, targetPosition, lerpAmount);

            MouseState mouseState = Mouse.GetState();
            int scrollDelta = mouseState.ScrollWheelValue - previousScrollValue;
            if (scrollDelta > 0) zoom += zoomStep;
            if (scrollDelta < 0) zoom -= zoomStep; 
            zoom = MathHelper.Clamp(zoom, minZoom, maxZoom);
            previousScrollValue = mouseState.ScrollWheelValue;
            Transform =
                Matrix.CreateTranslation(new Vector3(-position, 0)) *
                Matrix.CreateScale(zoom) *
                Matrix.CreateTranslation(new Vector3(1920 / 2, 1080 / 2, 0));
        }
    }
}
