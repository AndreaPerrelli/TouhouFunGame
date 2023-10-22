using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Touhou
{
    internal class Enemy
    {
        private Vector2 position;
        private Vector2 velocity = new Vector2(0, 2);  // Puoi cambiare la velocità come preferisci
        private int health = 100;
        public int MaxHealth { get; set; } = 100;
        public const int Size = 40;
        public Microsoft.Xna.Framework.Color TintColor { get; set; } = Microsoft.Xna.Framework.Color.White;
        private float timeSinceHit = 0;
        private const float flashDuration = 0.1f; // Durata in secondi

        private BulletPattern bulletPattern = new BulletPattern();

        private Texture2D texture;



        public float TimeSinceLastShot { get; set; } = 0;

        // Supponiamo che ogni nemico abbia un intervallo casuale tra gli spari
        public float ShootInterval { get; set; } = 0.5f + (float)new Random().NextDouble() * 0.5f;  // valore casuale tra 1 e 5 secondi
        public Vector2 Position { get => position; set => position = value; }
        public Vector2 Velocity { get => velocity; set => velocity = value; }

        public Texture2D Texture { get => texture; set => texture = value; }

        public int Health { get => health; set => health = value; }

        public Enemy(Vector2 position, Texture2D texture)
        {
            this.position = position;
            this.texture = texture;
        }

        public void Hit()
        {
            TintColor = Microsoft.Xna.Framework.Color.Red; // o qualsiasi altro colore che desideri
            timeSinceHit = 0;
        }


        public void Update(float elapsedSeconds)
        {
            if (TintColor != Microsoft.Xna.Framework.Color.White)
            {
                timeSinceHit += elapsedSeconds; // aggiorna con il tempo trascorso dall'ultimo frame
                if (timeSinceHit >= flashDuration)
                {
                    TintColor = Microsoft.Xna.Framework.Color.White;
                }
            }
            Position += Velocity;
        }

        public bool CheckCollision(Bullet bullet)
        {
            // Usa una semplice collisione basata su rettangoli
            bool isCollided = bullet.Position.X + Bullet.Size > Position.X &&
                           bullet.Position.X < Position.X + Size &&
                           bullet.Position.Y + Bullet.Size > Position.Y &&
                           bullet.Position.Y < Position.Y + Size;

            if (isCollided)
            {
                // Diminuisce la salute del nemico in base al danno del proiettile
                Health = Health - bullet.Damage;
            }

            return isCollided;
        }

    }
}
