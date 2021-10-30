using Encog.ML.EA.Genome;
using Encog.Neural.Flat;
using Encog.Neural.Freeform.Basic;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Text;

namespace GaNN
{
    class Bird
    {
        public Vector2 Position;
        public float Velocity = 0f;
        public float Acceleration = 0f;
        public float Gravity = .8f;
        public float Lift = 12f;

        float Xpos = 50;

        public float Rotation = 0f;

        public Vector2 Origin;

        public float Width,Height;
        public float MaxVelocity = 15f;
        Texture2D TX;

        Random Random => new Random();


        public float CollisionRadius = 25f;

        public bool Alive = true;

        public BasicNetwork Brain;


        public float fitness = 0;
        public float score = 1.1f;

        public Bird(ContentManager C, GameWindow window, BasicNetwork _brain = null)
        {
            Width = window.ClientBounds.Width;
            Height = window.ClientBounds.Height;
            TX = C.Load<Texture2D>("Bird");
            Origin = new Vector2(TX.Width/2,TX.Height/2);
            if (_brain==null)
            {
                Brain = new BasicNetwork();
                Brain.AddLayer(new BasicLayer(3));
                Brain.AddLayer(new BasicLayer(5));
                Brain.AddLayer(new BasicLayer(2));
                Brain.Structure.FinalizeStructure();
                Brain.Reset();
            }
            else
            {
                Brain = (BasicNetwork)_brain.Clone();
            }
            Reset();
        }
        public void Reset()
        {
            Velocity = 0f;
            Acceleration = 0f;
            Position.X = Xpos;
            Position.Y = Height / 2;
        }
        public void Draw(SpriteBatch Batch)
        {
            Batch.Draw(TX, Position, null, Color.White, Rotation, Origin, .08f, SpriteEffects.None, 0);
            //Batch.DrawCircle(Position, CollisionRadius, 15, Color.White);
        }
        public void Update(List<Pipe> Pipes)
        {
            Think(Pipes);
            #region PhysicsControl
            Acceleration += Gravity;
            Velocity += Acceleration;
            Velocity = Math.Clamp(Velocity, -MaxVelocity, MaxVelocity);
            Position.Y += Velocity;
            Acceleration *= 0;
            #endregion
            float AngleDelta = Velocity;
            Rotation = Math.Clamp(AngleDelta/10, MathHelper.ToRadians(-60), MathHelper.ToRadians(90));
            foreach (var pipe in Pipes)
            {
                if (pipe.Top.ToRectangleF().DistanceTo(Position.ToPoint()) < CollisionRadius ||
                    pipe.Bottom.ToRectangleF().DistanceTo(Position.ToPoint()) < CollisionRadius)
                {
                    Alive = false;
                }
            }
            score+=(float)Math.Log10(score);
        }
        public void Think(List<Pipe> Pipes)
        {
            Pipe C = null;
            float CD = float.PositiveInfinity;
            for (int i = 0; i < Pipes.Count; i++)
            {
                float D = Pipes[i].X-Position.X;
                if (D<CD&&D>0)
                {
                    CD = D;
                    C = Pipes[i];
                }
            }
            if (C!=null)
            {
                float Xdif = Position.X - C.X;
                float Ydif = Position.Y - C.Y;
                double[] inputs = new double[3];
                inputs[0] = Velocity / MaxVelocity;
                inputs[1] = Xdif / Width;
                inputs[2] = Ydif / Height;
                double[] Outputs = new double[2];
                Brain.Compute(inputs, Outputs);
                if (Outputs[0] > Outputs[1])
                {
                    Up();
                }
            }
        }
        public void Up()
        {
            Velocity = -Lift;
        }
        public void mutate(double rate)
        {
            double mutate(double val)
            {
                if (Random.NextDouble() < rate)
                {
                    return val + Random.NextDouble()*0.1;
                }
                else
                {
                    return val;
                }
            }
            double[] Old = Brain.Flat.EncodeNetwork();
            double[] New = Brain.Flat.EncodeNetwork();
            for (int i = 0; i < Brain.Flat.EncodeNetwork().Length; i++)
            {
                New[i] = mutate(Old[i]);
            }
            Brain.Flat.DecodeNetwork(New);
        }
        public bool Dead()
        {
            return (Position.Y > Height || Position.Y < 20) || !Alive;
        }
    }
}
