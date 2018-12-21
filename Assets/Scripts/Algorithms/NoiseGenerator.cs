using UnityEngine;

public class NoiseGenerator
{
    public NoiseGenerator()
    {

    }

    // generate a pseudo-random number within the range
    private float GeneratePseudoRandom(int xCoord, float range)
    {
        return Random.Range(-range / 2.0f, range / 2.0f); ;
    }

    // compute the noise at each coordinate
    public float GetNoise(int xCoord, int numDivisionsX, float minHeight, float maxHeight)
    {
        float noise = 0.0f;
        float range = maxHeight - minHeight;

        numDivisionsX /= 2;

        // while we can still divide the number of divisions (the number of vertices that make the hill)
        // generate finer-grain noise
        while (numDivisionsX > 0)
        {
            if(xCoord % numDivisionsX == 0)
            {
                noise += GeneratePseudoRandom(xCoord, range);
            }

            numDivisionsX /= 2;
            range /= 2.0f;
        }
        
        return noise;
    }
}
