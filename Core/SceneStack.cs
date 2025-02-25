using System.Runtime.CompilerServices; 
using CompSci_NEA.Scenes; 
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CompSci_NEA.Core
{
    public class SceneStack
    {

        private sealed class Node
        {
            public readonly Scene Scene;
            public Node Next;

            public Node(Scene scene)
            {
                Scene = scene;
            }
        }

        private Node _top; 
        private readonly Main _game;

        public SceneStack(Main game)
        {
            _game = game;
            _top = null;
        }

        public Scene CurrentScene => _top?.Scene;

        public void ChangeScene(Scene newScene)
        {
            while (_top != null)
            {
                PopScene();
            }
            PushScene(newScene);
        }

        public void PushScene(Scene newScene)
        {
            var newNode = new Node(newScene)
            {
                Next = _top 
            };
            _top = newNode; 

            newScene.LoadContent();
            _game.pauseCurrentSceneUpdating = _top.Next != null;
        }


        public void PopScene()
        {
            if (_top == null)
                return; 

            _top.Scene.Shutdown(); 
            _top = _top.Next; 

            _game.pauseCurrentSceneUpdating = _top?.Next != null; 
        }

        public void Update(GameTime gameTime)
        {
            _top?.Scene.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _top?.Scene.Draw(spriteBatch);
        }
    }
}
