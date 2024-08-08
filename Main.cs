using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections.Generic;
using System.Linq;

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
        bool Paused = false;
        bool ShowNetwork = false;
        private void Listener_KeyPressed(object sender, KeyboardEventArgs e)
        {
            if (e.Key == Keys.Right)
            {
                Speed++;
            }
            if (e.Key == Keys.Up)
            {
                Speed += 10;
            }
            if (e.Key == Keys.Down)
            {
                Speed = 1;
            }
            else if (e.Key == Keys.Left)
            {
                Speed--;
                if (Speed < 1)
                {
                    Speed = 1;
                }
            }
            if (e.Key == Keys.Space)
            {
                Paused = !Paused;
            }
            if (e.Key == Keys.M)
            {
                SBO = !SBO;
            }

            if (e.Key == Keys.N)
            {
                ShowNetwork = !ShowNetwork;
            }

            if (e.Key == Keys.R)
            {
                GA = new GA(Content, Window);
                Pipe = new Pipe(Window, Content);
            }
        }

        int Speed = 1;
        Texture2D BG;
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 500;
            _graphics.PreferredBackBufferHeight = 1200;
            _graphics.ApplyChanges();
            Window.Title = "NeuroEvolutionary FlappyBird";
            base.Initialize();
        }
        GA GA;
        Pipe Pipe;
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            GA = new GA(Content, Window);
            BG = Content.Load<Texture2D>("background");
            Font = Content.Load<SpriteFont>("Font");
            Pipe = new Pipe(Window, Content);

            // TODO: use this.Content to load your game content here
        }
        float BestScore = 0;

        protected override void Update(GameTime gameTime)
        {
            Listener.Update(gameTime);

            if (Paused)
            {
                return;
            }
            for (int s = 0; s < Speed; s++)
            {
                for (int i = 0; i < GA.Birds.Count; i++)
                {
                    GA.Birds[i].Update(Pipe);
                    if (GA.Birds[i].Dead())
                    {
                        GA.SavedBirds.Add(GA.Birds[i]);
                        GA.Birds.RemoveAt(i);
                    }
                }
                Pipe.Update();
                if (GA.Birds.Count == 1)
                {
                    GA.SavedBirds.Add(GA.Birds[0]);
                }
                if (GA.Birds.Count == 0)
                {
                    GA.nextGeneration(ref Pipe);
                }
            }
            base.Update(gameTime);

        }
        bool SBO = false;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            _spriteBatch.Draw(BG, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);

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
            Pipe.Draw(_spriteBatch);
            string Stuts = $"Speed:{Speed}" +
                $"\nScore:{(int)GA.Birds[0].score}" +
                $"\nHScore:{(int)BestScore}" +
                $"\nGenNo:{GA.GenerationNo}" +
                $"\nRemaining From Generation: {GA.Birds.Count}" +
                $"\nShow best only?(M): {SBO}" +
                $"\nPause/Resume(Space) : {Paused}" +
                $"\nVisualize Network(N): {ShowNetwork}" +
                $"\n(R) to Reset";

            _spriteBatch.DrawString(Font, Stuts, new Vector2(0,Window.ClientBounds.Height-Font.MeasureString(Stuts).Y*1.2f), Color.Black);
            _spriteBatch.FillRectangle(new Rectangle(0, Window.ClientBounds.Height - 20, Window.ClientBounds.Width, 20), Color.Gray);

            // Best Neural Network Visualization
            if (ShowNetwork)
            {
                var bestNetwork = GA.Birds[0].Brain;

                var layerCount = bestNetwork.LayerCount;
                int[] layerSizes = [.. (from layerIdx in Enumerable.Range(0, layerCount) select bestNetwork.Flat.GetLayerNeuronCount(layerIdx))];

                float margin = 10;
                float neuronRadius = 10;
                float spacing = neuronRadius + 10;

                RectangleF visArea = new(0, 10, Window.ClientBounds.Width, 300);

                float xGap = (visArea.Width - margin * 2) / layerCount;

                float x = visArea.X + xGap;

                // Neuron visualization
                for (int i = 0; i < layerCount; i++)
                {
                    float nCount = layerSizes[i];

                    float yGap = (visArea.Height - margin * 2) / nCount;




                    for (int j = 0; j < nCount; j++)
                    {
                        float y = visArea.Y + margin + yGap * (j + 0.5f);

                        // neuron color
                        float activationValue = (float)bestNetwork.GetLayerOutput(i, j);

                        // Calculate the interpolation value between blue and red
                        float t = (activationValue + 1) / 2;
                        Color neuronColor = new Color();

                        if (t < 0.5)
                        {
                            neuronColor = Color.Lerp(Color.Blue, Color.Black, t * 2);
                        }
                        else
                        {
                            neuronColor = Color.Lerp(Color.Black, Color.Red, (t - 0.5f) * 2);
                        }

                        if (i < layerCount - 1)
                        {
                            float nCountNext = layerSizes[i + 1];
                            float yGapNext = (visArea.Height - margin * 2) / nCountNext;
                            for (int k = 0; k < nCountNext; k++)
                            {
                                float yNext = visArea.Y + margin + yGapNext * (k + 0.5f);

                                float weight = (float)bestNetwork.GetWeight(i, j, k);

                                // Calculate the interpolation value between blue and red
                                float t1 = (weight + 1) / 2;
                                Color weightColor = Color.Lerp(Color.Blue, Color.Red, t1);

                                _spriteBatch.DrawLine(new Vector2(x, y), new Vector2(x + xGap, yNext), weightColor, 2, 0);

                            }
                        }
                        // Draw the neuron
                        _spriteBatch.DrawCircle(new Vector2(x, y), radius: neuronRadius, sides: 10, color: neuronColor, thickness: neuronRadius);
                        // Draw the value if the mouse is over the neuron
                        if (new RectangleF(x - neuronRadius, y - neuronRadius, neuronRadius * 2, neuronRadius * 2).Contains(Mouse.GetState().Position))
                        {
                            // Show the activation value
                            var txt = activationValue.ToString("0.00");
                            _spriteBatch.DrawString(Font, txt, new Vector2(x, visArea.Bottom + 10), Color.White, 0, Font.MeasureString(txt) / 2, 1.5f, SpriteEffects.None, 0);
                        }
                    }
                    x += xGap;
                }
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
