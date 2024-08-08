using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Text;

namespace GaNN
{
    class Pipe
    {
        public Rectangle Top;
        public Rectangle Bottom;

        public float PipeWidth = 100;
        public float Spacing = 100;
        public float Speed = 7f;

        public float Y;
        public float X;

        GameWindow Window;

        Texture2D Tx;

        Random Random = new Random(1);

        public Color C = Color.White;
        public Pipe(GameWindow Win,ContentManager C)
        {
            Window = Win;
            Tx = C.Load<Texture2D>("Pipe1");
            Reset(true);
        }
        public void Reset(bool deepReset = false)
        {
            if(deepReset)
                Random = new Random(1);
            X = Window.ClientBounds.Width+PipeWidth+10;
            int Z = Window.ClientBounds.Height / 10;
            Y = Random.Next(Z*2,Z*8);
            Top.Width = (int)PipeWidth;
            Bottom.Width = (int)PipeWidth;

            Top.X = (int)X;
            Bottom.X = (int)X;

            Top.Height = Window.ClientBounds.Height;
            Bottom.Height = Window.ClientBounds.Height;

            Top.Y = (int)(Y-Top.Height-Spacing);
            Bottom.Y = (int)(Spacing + Y);
        }
        public void Draw(SpriteBatch batch)
        {
            batch.Draw(Tx, Top   ,null,C,0,new Vector2(),SpriteEffects.FlipVertically,0);
            batch.Draw(Tx, Bottom,null,C,0,new Vector2(),SpriteEffects.None,0);
        }
        public void Update()
        {
            X -= Speed;
            Top.X = (int)X;
            Bottom.X = (int)X;
            offScreen();
        }
        public void offScreen()
        {
            if (X+PipeWidth < 0)
            {
                Reset();
            }
        }
    }
}
