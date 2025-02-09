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
        private float zoom = 1.0f; // Default zoom level
        private float minZoom = 0.001f; // Minimum zoom
        private float maxZoom = 1.0f; // Maximum zoom
        private float zoomStep = 0.1f; // Fixed zoom step
        private int previousScrollValue; // Store previous scroll wheel value

        public Matrix Transform { get; private set; }

        public void Update(Vector2 targetPosition)
        {
            zoomStep = (float)Math.Pow(2.7f, zoom)/(zoom+16)-0.06f;
            //Console.WriteLine(zoomStep.ToString());
            // Smoothly interpolate camera position
            position = Vector2.Lerp(position, targetPosition, lerpAmount);

            // Get current scroll wheel value
            MouseState mouseState = Mouse.GetState();
            int scrollDelta = mouseState.ScrollWheelValue - previousScrollValue;

            // Adjust zoom in fixed steps
            if (scrollDelta > 0) zoom += zoomStep;  // Scroll up -> Zoom in
            if (scrollDelta < 0) zoom -= zoomStep;  // Scroll down -> Zoom out

            // Clamp zoom to prevent excessive zooming
            zoom = MathHelper.Clamp(zoom, minZoom, maxZoom);

            // Store current scroll value for next frame
            previousScrollValue = mouseState.ScrollWheelValue;

            // Create the transformation matrix (position + zoom)
            Transform =
                Matrix.CreateTranslation(new Vector3(-position, 0)) *
                Matrix.CreateScale(zoom) *
                Matrix.CreateTranslation(new Vector3(1920 / 2, 1080 / 2, 0));
        }

        /*private Vector2 position;
        private float lerpAmount = 0.1f;

        public Matrix Transform { get; private set; }

        public void Update(Vector2 targetPosition)
        {
            position = Vector2.Lerp(position, targetPosition, lerpAmount);
            Transform = Matrix.CreateTranslation(new Vector3(-position + new Vector2(1920/2, 1080/2), 0));
        }*/
    }
}
