using Encog.Engine.Network.Activation;
using Encog.MathUtil.Randomize;
using Encog.MathUtil.RBF;
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
        public float Lift = 120f;

        float Xpos = 100;

        public float Rotation = 0f;

        public Vector2 Origin;

        public float Width, Height;
        public float MaxVelocity = 15f;
        Texture2D TX;

        Random Random => new Random();

        public ContentManager C { get; }
        public GameWindow Window { get; }

        public float CollisionRadius = 30f;

        public bool Alive = true;

        public BasicNetwork Brain;


        public float fitness = 0;
        public float score = 1.1f;

        int MemoryActionBits = 0;
        public Bird(ContentManager C, GameWindow window, BasicNetwork _brain = null)
        {
            Width = window.ClientBounds.Width;
            Height = window.ClientBounds.Height;
            TX = C.Load<Texture2D>("Bird");
            Origin = new Vector2(TX.Width / 2, TX.Height / 2);


            var InputActivation = new ActivationLinear();
            var HiddenActivation = new ActivationStep(-5, 0, 5);
            var OutputActivation = new ActivationStep(-5, 0, 5);

            int hiddenLayerNeurons = 10;
            int depth = 6;

            Brain = new BasicNetwork();
            Brain.AddLayer(new BasicLayer(InputActivation, false, 5 + MemoryActionBits));
            for (int i = 0; i < depth; i++)
            {
                Brain.AddLayer(new BasicLayer(HiddenActivation, false, hiddenLayerNeurons));
            }
            Brain.AddLayer(new BasicLayer(OutputActivation, false, 2 + MemoryActionBits));
            Brain.Structure.FinalizeStructure();
            Brain.Reset();
            if (_brain != null)
            {
                Brain.DecodeFromArray(_brain.Flat.Weights);
            }
            Reset();
            this.C = C;
            Window = window;
        }
        public void Reset()
        {
            Velocity = 0f;
            Acceleration = 0f;
            Position.X = Xpos;
            Position.Y = Height / 2;
            actionMemoryBits.Clear();
            for (int i = 0; i < MemoryActionBits; i++)
            {
                actionMemoryBits.Add(Random.NextDouble() - 0.5f);
            }
        }
        public void Draw(SpriteBatch Batch)
        {
            Batch.Draw(TX, Position, null, Color.White, Rotation, Origin, .08f, SpriteEffects.None, 0);
            Batch.DrawCircle(Position, CollisionRadius, 15, Color.White);
        }
        public void Update(Pipe Pipe)
        {
            Think(Pipe);
            #region PhysicsControl
            Acceleration += Gravity;
            Velocity += Acceleration;
            Velocity = Math.Clamp(Velocity, -MaxVelocity, MaxVelocity);
            Position.Y += Velocity;
            Acceleration *= 0;
            #endregion
            float AngleDelta = Velocity;
            Rotation = MathHelper.Lerp(Rotation, Math.Clamp(AngleDelta / 10, MathHelper.ToRadians(-60), MathHelper.ToRadians(90)), 0.2f);
            if (Pipe.Top.ToRectangleF().DistanceTo(Position.ToPoint()) < CollisionRadius ||
                    Pipe.Bottom.ToRectangleF().DistanceTo(Position.ToPoint()) < CollisionRadius)
            {
                Alive = false;
            }
            score += 1f;
        }
        List<double> actionMemoryBits = [];
        public void Think(Pipe Pipe)
        {
            float Xdif = Position.X - Pipe.X;
            List<double> inputs = [Velocity / MaxVelocity, Xdif / Width, Position.Y / Height, Pipe.Y / Height, Position.Y / Height];
            inputs.AddRange(actionMemoryBits);
            double[] Outputs = new double[Brain.OutputCount];
            Brain.Compute([.. inputs], Outputs);
            int startIdx = 2;
            actionMemoryBits.Clear();
            for (int i = 0; i < MemoryActionBits; i++)
            {
                actionMemoryBits.Add(Outputs[startIdx + i]);
            }
            /*
            var dist = (float)Math.Abs(Outputs[0] - Outputs[1]);
            fitness += dist;*/


            // If Output 0 is greater than Output 1, jump
            if (Outputs[0] > Outputs[1])
            {
                Up();
            }
            /*
            // If the difference between the two outputs is greater than 0.5, jump
            if (Math.Abs(Outputs[0] - Outputs[1]) > 0.5f)
            {
                Up();
            }*/

            /*// Difference between the two outputs is the probability of jumping
            if (Random.NextDouble() < Math.Abs(Outputs[0] - Outputs[1]))
            {
                Up();
            }*/

            /*
            // sample the output as a gaussian distribution Output[0] is jump , Output[1] is not jump
            // outputs[0] is the mean of the distribution
            // outputs[1] is the standard deviation
            var gaussianRandomizer = new GaussianRandomizer(Outputs[0], Outputs[1]);
            if (gaussianRandomizer.NextDouble() > Outputs[2])
            {
                Up();
            }*/

        }
        public void Up()
        {
            Velocity += -Lift;
        }
        public void mutate(double rate)
        {
            double mutate(double val)
            {
                if (Random.NextDouble() < rate)
                {
                    return val + (Random.NextDouble()*2-1) * 0.05f;
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
        public Bird Cross(Bird Other)
        {
            var A = Brain.Flat.Weights;
            var B = Other.Brain.Flat.Weights;

            var ChildW = new double[A.Length];
            int turnPointFreq = Random.Next(A.Length/2-1)+1;
            bool right = true;
            for (int i = 0; A.Length > i; i++)
            {
                if (i % turnPointFreq == 0)
                    right = !right;
                ChildW[i] = right ? A[i] : B[i];
            }
            var brain = Other.Brain;
            brain.Flat.Weights = ChildW;
            Bird Child = new(C, Window, brain);
            Child.fitness = (fitness + Other.fitness) / 2;
            return Child;
        }
        public bool Dead()
        {
            return (Position.Y > Height || Position.Y < 20) || !Alive;
        }
    }
}
