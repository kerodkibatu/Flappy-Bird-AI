using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using System.Collections.Generic;

namespace GaNN
{
    public class Main : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        KeyboardListener Listener;
        SpriteFont Font;
        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Listener = new KeyboardListener();
            Listener.KeyPressed += Listener_KeyPressed;
        }

        private void Listener_KeyPressed(object sender, KeyboardEventArgs e)
        {
            if (e.Key == Keys.Right)
            {
                Speed++;
            }
            else if (e.Key == Keys.Left)
            {
                Speed--;
                if (Speed<1)
                {
                    Speed = 1;
                }
            }
            if (e.Key == Keys.M)
            {
                SBO = !SBO;
            }
            if (e.Key == Keys.R)
            {
                GA = new GA(Content, Window);
            }
        }

        int Speed = 1;
        Texture2D BG;
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 480;
            _graphics.PreferredBackBufferHeight = 852;
            _graphics.ApplyChanges();
            Window.Title = "NeuroEvolutionary FlappyBird";
            base.Initialize();
        }
        GA GA;
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            GA = new GA(Content, Window);
            BG = Content.Load<Texture2D>("background");
            Font = Content.Load<SpriteFont>("Font");
            Pipes.Clear();
            Pipes.Add(new Pipe(Window,Content));
            
            // TODO: use this.Content to load your game content here
        }
        List<Pipe> Pipes = new List<Pipe>();
        float BestScore = 0;
        protected override void Update(GameTime gameTime)
        {
            Listener.Update(gameTime);
            for (int i = 0; i < Pipes.Count; i++)
            {
                if (Pipes[i].offScreen())
                {
                    Pipes.Clear();
                    Pipes.Add(new Pipe(Window, Content));
                }
            }
            
            for (int s = 0; s < Speed; s++)
            {
                for (int i = 0; i < GA.Birds.Count; i++)
                {
                    GA.Birds[i].Update(Pipes);
                    if (GA.Birds[i].Dead())
                    {
                        GA.SavedBirds.Add(GA.Birds[i]);
                        GA.Birds.RemoveAt(i);
                    }
                }
                for (int i = 0; i < Pipes.Count; i++)
                {
                    Pipes[i].Update();
                }
                if (GA.Birds.Count == 1)
                {
                    GA.SavedBirds.Add(GA.Birds[0]);
                }
                if (GA.Birds.Count == 0)
                {
                    GA.nextGeneration(Pipes);
                }
            }
            base.Update(gameTime);
            
        }
        bool SBO = false;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            _spriteBatch.Draw(BG, new Rectangle(0,0,Window.ClientBounds.Width,Window.ClientBounds.Height), Color.White);
            
            if (GA.Birds[0].score > BestScore)
            {
                BestScore = GA.Birds[0].score;
            }
            if (SBO)
            {
                GA.Birds[0].Draw(_spriteBatch);
            }
            else
            {
                for (int i = 0; i < GA.Birds.Count; i++)
                {
                    GA.Birds[i].Draw(_spriteBatch);
                }
            }
            for (int i = 0; i < Pipes.Count; i++)
            {
                Pipes[i].Draw(_spriteBatch);
            }
            string Stuts = $"Speed:{Speed}" +
                $"\nScore:{(int)GA.Birds[0].score}" +
                $"\nHScore:{(int)BestScore}" +
                $"\nGenNo:{GA.GenerationNo}" +
                $"\nRemaining From Generation: {GA.Birds.Count}" +
                $"\nShow best only?(M): {SBO}";
            
            _spriteBatch.DrawString(Font, Stuts, new Vector2(0), Color.White);
            _spriteBatch.FillRectangle(new Rectangle(0, Window.ClientBounds.Height - 20, Window.ClientBounds.Width, 20), Color.Gray);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
