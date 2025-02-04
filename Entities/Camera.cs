using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


namespace CompSci_NEA.Entities
{
    public class Camera
    {
        private Vector2 position;
        private float lerpAmount = 0.1f;

        public Matrix Transform { get; private set; }

        public void Update(Vector2 targetPosition)
        {
            position = Vector2.Lerp(position, targetPosition, lerpAmount);
            Transform = Matrix.CreateTranslation(new Vector3(-position + new Vector2(1920/2, 1080/2), 0));
        }
    }
}
