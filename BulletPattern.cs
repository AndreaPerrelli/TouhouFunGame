using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

internal class BulletPattern
{
    private readonly Random random;

    public BulletPattern()
    {
        random = new Random();
    }

    // Direct Shot
    public List<Vector2> DirectShot(Vector2 startPosition, float bulletSpeed)
    {
        return new List<Vector2> { new Vector2(0, bulletSpeed) };
    }

    // Fan Shot
    public List<Vector2> FanShot(Vector2 startPosition, int numBullets, float spreadAngle, float bulletSpeed)
    {
        var velocities = new List<Vector2>();
        float startAngle = -(spreadAngle / 2);
        float increment = spreadAngle / (numBullets - 1);

        for (int i = 0; i < numBullets; i++)
        {
            float angle = startAngle + (increment * i);
            velocities.Add(CalculateVelocity(angle, bulletSpeed));
        }

        return velocities;
    }

    // Spiral Shot
    public List<Vector2> SpiralShot(Vector2 startPosition, int numBullets, float bulletSpeed)
    {
        var velocities = new List<Vector2>();
        float increment = (float)(2 * Math.PI / numBullets);

        for (int i = 0; i < numBullets; i++)
        {
            velocities.Add(CalculateVelocity(increment * i, bulletSpeed));
        }

        return velocities;
    }

    // Sequential Shot
    public Vector2 SequentialShot(Vector2 startPosition, float bulletSpeed, int direction)
    {
        return direction switch
        {
            0 => new Vector2(0, -bulletSpeed),
            1 => new Vector2(bulletSpeed, 0),
            2 => new Vector2(0, bulletSpeed),
            3 => new Vector2(-bulletSpeed, 0),
            _ => Vector2.Zero,
        };
    }

    // Burst Shot
    public List<Vector2> BurstShot(Vector2 startPosition, int numBullets, float bulletSpeed)
    {
        return new List<Vector2>(new Vector2(0, bulletSpeed).Repeat(numBullets));
    }

    // Wave Shot
    public Vector2 WaveShot(Vector2 startPosition, float bulletSpeed)
    {
        return new Vector2(0, bulletSpeed);
    }

    // Chasing Shot
    public Vector2 ChasingShot(Vector2 startPosition, Vector2 playerPosition, float bulletSpeed)
    {
        Vector2 direction = playerPosition - startPosition;
        direction.Normalize();
        return direction * bulletSpeed;
    }

    // Get Random Pattern
    public List<Vector2> GetRandomPattern(Vector2 startPosition, Vector2 playerPosition, float bulletSpeed)
    {
        return random.Next(0, 7) switch
        {
            0 => DirectShot(startPosition, bulletSpeed),
            1 => FanShot(startPosition, 5, (float)Math.PI / 3, bulletSpeed),
            2 => SpiralShot(startPosition, 8, bulletSpeed),
            3 => new List<Vector2> { SequentialShot(startPosition, bulletSpeed, random.Next(0, 4)) },
            4 => BurstShot(startPosition, random.Next(3, 10), bulletSpeed),
            5 => new List<Vector2> { WaveShot(startPosition, bulletSpeed) },
            6 => new List<Vector2> { ChasingShot(startPosition, playerPosition, bulletSpeed) },
            _ => DirectShot(startPosition, bulletSpeed),
        };
    }

    // Helper Method to calculate velocity
    private Vector2 CalculateVelocity(float angle, float bulletSpeed)
    {
        return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * bulletSpeed;
    }
}

// Extension method to create a list with repeated items
public static class Extensions
{
    public static IEnumerable<T> Repeat<T>(this T item, int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return item;
        }
    }
}
