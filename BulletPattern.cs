using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Touhou
{
    internal class BulletPattern
    {
        private Random random;
        private Player player;  // Riferimento al giocatore per il pattern ChasingShot.

        public BulletPattern()
        {
            random = new Random();
        }

        public List<Vector2> DirectShot(Vector2 startPosition, float bulletSpeed)
        {
            List<Vector2> velocities = new List<Vector2>
        {
            new Vector2(0, bulletSpeed)
        };

            return velocities;
        }

        public List<Vector2> FanShot(Vector2 startPosition, int numBullets, float spreadAngle, float bulletSpeed)
        {
            List<Vector2> velocities = new List<Vector2>();
            float startAngle = -(spreadAngle / 2);
            float increment = spreadAngle / (numBullets - 1);

            for (int i = 0; i < numBullets; i++)
            {
                float angle = startAngle + (increment * i);
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * bulletSpeed;
                velocities.Add(velocity);
            }

            return velocities;
        }

        // TODO: Implement other shot patterns: SpiralShot, SequentialShot, ...

        public List<Vector2> SpiralShot(Vector2 startPosition, int numBullets, float bulletSpeed)
        {
            List<Vector2> velocities = new List<Vector2>();
            float increment = (float)(2 * Math.PI / numBullets);

            for (int i = 0; i < numBullets; i++)
            {
                float angle = increment * i;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * bulletSpeed;
                velocities.Add(velocity);
            }

            return velocities;
        }

        public Vector2 SequentialShot(Vector2 startPosition, float bulletSpeed, int direction)
        {
            // Esempio con 4 direzioni: 0=up, 1=right, 2=down, 3=left
            switch (direction)
            {
                case 0: return new Vector2(0, -bulletSpeed);
                case 1: return new Vector2(bulletSpeed, 0);
                case 2: return new Vector2(0, bulletSpeed);
                case 3: return new Vector2(-bulletSpeed, 0);
                default: return Vector2.Zero;
            }
        }

        public List<Vector2> BurstShot(Vector2 startPosition, int numBullets, float bulletSpeed)
        {
            List<Vector2> velocities = new List<Vector2>();

            for (int i = 0; i < numBullets; i++)
            {
                velocities.Add(new Vector2(0, bulletSpeed));
            }

            return velocities;
        }

        // Questo potrebbe richiedere una modifica alla classe Bullet per supportare l'oscillazione.
        public Vector2 WaveShot(Vector2 startPosition, float bulletSpeed)
        {
            return new Vector2(0, bulletSpeed);  // La logica di oscillazione verrà gestita dalla classe Bullet.
        }

        public Vector2 ChasingShot(Vector2 startPosition, Vector2 playerPosition, float bulletSpeed)
        {
            Vector2 direction = playerPosition - startPosition;
            direction.Normalize();
            return direction * bulletSpeed;
        }

        // This method will be called by the enemy to randomly choose a shot pattern.
        public List<Vector2> GetRandomPattern(Vector2 startPosition, Vector2 playerPosition, float bulletSpeed)
        {
            int choice = random.Next(0, 7);  // Adatta il valore massimo alla quantità di pattern disponibili.

            switch (choice)
            {
                case 0: return DirectShot(startPosition, bulletSpeed);
                case 1: return FanShot(startPosition, 5, (float)Math.PI / 3, bulletSpeed);
                case 2: return SpiralShot(startPosition, 8, bulletSpeed); // Ho scelto 8 come numero di proiettili per la spirale.
                case 3:
                    int randomDirection = random.Next(0, 4);  // Ad esempio, scegliendo tra 4 direzioni possibili: 0 (sinistra), 1 (destra), 2 (giù), 3 (su).
                    List<Vector2> seqShots = new List<Vector2>
                    {
                        SequentialShot(startPosition, bulletSpeed, randomDirection)
                    };
                    return seqShots;

                case 4:
                    int randomNumBullets = random.Next(3, 10); // Ad esempio, scegliendo tra 3 a 9 proiettili.
                    return BurstShot(startPosition, randomNumBullets, bulletSpeed);

                case 5:
                    List<Vector2> waveShots = new List<Vector2>
                    {
                        WaveShot(startPosition, bulletSpeed)
                    };
                    return waveShots;

                case 6:
                    List<Vector2> chasingShots = new List<Vector2>
                    {
                        ChasingShot(startPosition, playerPosition, bulletSpeed)
                    };
                    return chasingShots;

                default: return DirectShot(startPosition, bulletSpeed);
            }
        }
    }
}
