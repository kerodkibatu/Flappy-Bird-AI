using Encog.ML.Genetic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace GaNN
{
    class GA
    {
        public List<Bird> Birds = new List<Bird>();
        public List<Bird> SavedBirds = new List<Bird>();
        Random Random => new Random();

        ContentManager C;
        GameWindow GameWindow;

        public int pCount = 250;

        public int GenerationNo = 0;
        public GA(ContentManager _c,GameWindow _win)
        {
            C = _c;
            GameWindow = _win;
            for (int i = 0; i < pCount; i++)
            {
                Birds.Add(new Bird(C, GameWindow));
            }
        }

        public void nextGeneration(List<Pipe> pipes)
        {
            GenerationNo++;
            calculateFitness();
            for (int i = 0; i < pCount; i++)
            {
                Birds.Add(pickOne());
            }
            SavedBirds.Clear();
            pipes.Clear();
            pipes.Add(new Pipe(GameWindow, C));
            pipes.Add(new Pipe(GameWindow, C) { X = GameWindow.ClientBounds.Width * 2 });
        }
        public Bird pickOne()
        {
            int index = 0;
            double r = Random.NextDouble();
            while (r > 0)
            {
                if (SavedBirds[index]!=null)
                {
                    r = r - SavedBirds[index].fitness;
                }
                else
                {
                    r = 0;
                }
                
                index++;
            }
            index--;
            Bird bird = SavedBirds[index];
            Bird child = new Bird(C,GameWindow,bird.Brain);
            child.mutate(0.1);
            return child;
        }
        public void calculateFitness()
        {
            float sum = 0;
            foreach (var bird in SavedBirds)
            {
                sum += bird.score;
            }
            foreach (var bird in SavedBirds)
            {
                bird.fitness = bird.score / sum;
            }
        }
    }
}
