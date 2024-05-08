namespace BoardingSimulationV3.Helpers
{
        public class NormalDistribution
    {
        /* 
           int arraySize = 100; // Size of the array
           double mean = 30.0;  // Mean of the distribution
           double stdDev = 10.0; // Standard deviation of the distribution
           int minValue = 0;    // Minimum value of age
           int maxValue = 110;  // Maximum value of age

           // Generate the array
           int[] ages = GenerateNormalDistributionArray(arraySize, mean, stdDev, minValue, maxValue);

           // Output the result
           foreach (var age in ages) { Console.WriteLine(age); }
         
         */
        public  static int[] GenerateNormalDistributionArray(int size, double mean, double stdDev, int minValue, int maxValue)
        {
            Random rand = new Random();
            int[] values = new int[size];

            for (int i = 0; i < size; i++)
            {
                double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
                double u2 = 1.0 - rand.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

                // Truncate to bounds
                randNormal = Math.Max(minValue, Math.Min(maxValue, randNormal));
                values[i] = (int)Math.Round(randNormal);
            }

            return values;
        }
    }
}
