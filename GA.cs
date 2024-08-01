using Encog.ML.Genetic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public GA(ContentManager _c, GameWindow _win)
        {
            C = _c;
            GameWindow = _win;
            for (int i = 0; i < pCount; i++)
            {
                Birds.Add(new Bird(C, GameWindow));
            }
        }

        public void nextGeneration(ref Pipe pipe)
        {
            GenerationNo++;
            calculateFitness();


            for (int i = 0; i < pCount * 0.5; i++)
            {
                var A = pickOne();
                var B = pickOne();

                var child = A.Cross(B);
                child.mutate(0.05f);
                Birds.Add(child);
            }

            Birds.Add(SavedBirds.MaxBy(b => b.fitness));

            while (Birds.Count < pCount)
            {
                var B = pickOne();
                B.mutate(0.05f);
                Birds.Add(B);
            }

            SavedBirds.Clear();
            pipe.Reset(true);
        }
        public Bird pickOne()
        {
            int index = 0;
            float r = (float)Random.NextDouble();
            while (r > 0)
            {
                r -= SavedBirds[index % SavedBirds.Count].fitness;
                index++;
            }
            index--;
            // Todo: fix index loop
            return new Bird(C, GameWindow, SavedBirds[index % SavedBirds.Count].Brain);
        }
        public void calculateFitness()
        {
            float sum = 0;
            foreach (var bird in SavedBirds)
            {
                sum += bird.score * bird.score;
            }
            foreach (var bird in SavedBirds)
            {
                bird.fitness = bird.score * bird.score / sum;
            }
        }
    }
}
