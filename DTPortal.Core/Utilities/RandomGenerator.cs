using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Utilities
{
    class RandomGenerator
    {
        // Instantiate random string/number generator.  
        // It is better to keep a single Random instance 
        // and keep using Next on the same instance.  
        private readonly Random _random = new Random();

        // Generates a random number within a range.      
        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        public IList<int> GenerateRandomNumbers(int count)
        {
            RandomGenerator randomGenerator = new RandomGenerator();
            var randomNumberList = new List<int>();

            for (int i = 0; i < count; i++)
            {
                int randomNumber = randomGenerator.RandomNumber(100, 1000);
                if (randomNumberList.Contains(randomNumber))
                {
                    // Random number already exists,
                    // check for another random number
                    count++;
                }
                else
                {
                    randomNumberList.Add(randomNumber);
                }
            }

            return randomNumberList;
        }

        public int GetRandomIndex(int maxNumber)
        {
            // shuffle random codes
            var r = new Random();
            return r.Next(maxNumber);
        }
    }
}
