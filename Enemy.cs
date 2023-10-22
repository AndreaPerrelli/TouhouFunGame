using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Touhou
{
    internal class Enemy
    {
        // Campi privati
        private Vector2 position;
        private Vector2 velocity = new Vector2(0, 2);
        private int health;
        private float timeSinceHit = 0;

        private Texture2D texture;

        private const float FLASH_DURATION = 0.1f;

        // Proprietà
        public int MaxHealth { get; set; } = 100;
        public const int Size = 40;
        public Color TintColor { get; set; } = Color.White;
        public float TimeSinceLastShot { get; set; } = 0;
        public float ShootInterval { get; private set; } = 0.5f + (float)new Random().NextDouble() * 0.5f;

        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public Vector2 Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        public Texture2D Texture
        {
            get => texture;
            set => texture = value;
        }

        public int Health
        {
            get => health;
            set => health = value;
        }

        // Costruttore
        public Enemy(Vector2 position, Texture2D texture)
        {
            this.position = position;
            this.texture = texture;
            this.health = MaxHealth;
        }

        // Metodi
        public void Hit()
        {
            TintColor = Color.Red;
            timeSinceHit = 0;
        }

        public void Update(float elapsedSeconds)
        {
            if (TintColor != Color.White)
            {
                timeSinceHit += elapsedSeconds;

                if (timeSinceHit >= FLASH_DURATION)
                    TintColor = Color.White;
            }

            Position += Velocity;
        }

        public bool CheckCollision(Bullet bullet)
        {
            Rectangle enemyRect = new Rectangle((int)position.X, (int)position.Y, Size, Size);
            Rectangle bulletRect = new Rectangle((int)bullet.Position.X, (int)bullet.Position.Y, Bullet.Size, Bullet.Size);

            bool isCollided = enemyRect.Intersects(bulletRect);

            if (isCollided)
                Health -= bullet.Damage;

            return isCollided;
        }
    }
}
