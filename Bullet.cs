using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Touhou
{
    internal class Bullet
    {
        private Vector2 position;
        private Vector2 velocity;
        private Texture2D texture;
        public const int Size = 20;
        public int Damage { get; set; } = 25;
        public Vector2 Position { get => position; set => position = value; }
        public Vector2 Velocity { get => velocity; set => velocity = value; }

        public Texture2D Texture { get  => texture; set => texture = value; }

        public Bullet(Vector2 initialPosition, bool isShootingUp = true, float speed = 10)
        {
            Position = initialPosition;
            if (isShootingUp)
                Velocity = new Vector2(0, -speed);
            else
                Velocity = new Vector2(0, speed);
        }

        public Bullet(Vector2 initialPosition, Vector2 velocity, int damage, Texture2D texture)
        {
            Position = initialPosition;
            Velocity = velocity;
            this.Damage = damage;
            this.texture = texture;
        }

        public Bullet(Vector2 initialPosition, Vector2 velocity, Texture2D texture)
        {
            Position = initialPosition;
            Velocity = velocity;
            this.texture = texture;

        }

        public void Update()
        {
            Position += Velocity;
        }
    }
}
