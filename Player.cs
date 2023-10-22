using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Touhou
{
    internal class Player
    {
        private Vector2 _position;
        private Vector2 _velocity;
        private float _scale;
        private Texture2D _playerTexture;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector2 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public Texture2D PlayerTexture => _playerTexture;
        public int BulletsAmount { get; set; } = DefaultBulletsAmount;
        public int Damage { get; set; } = DefaultDamage;

        public const int Size = 50;
        public static readonly int HitboxSize = 20;
        public const int DefaultDamage = 30;
        public const int DefaultBulletsAmount = 3;
        private const int MaxDamage = 50;
        private const int MaxBulletsAmount = 10;

        public Player(Vector2 position, Vector2 velocity, float scale, Texture2D texture)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            _playerTexture = texture;
        }

        public Player(Vector2 position, Vector2 velocity, float scale, int bulletsAmount, int damage, Texture2D texture)
            : this(position, velocity, scale, texture)
        {
            BulletsAmount = bulletsAmount;
            Damage = damage;
        }

        public Rectangle Hitbox => new Rectangle(
            (int)(Position.X + (Size - HitboxSize) * 0.5f),
            (int)(Position.Y + (Size - HitboxSize) * 0.5f),
            HitboxSize,
            HitboxSize
        );

        public void Update()
        {
            Position += Velocity;
        }

        public void Shoot(List<Bullet> bullets, Texture2D bulletTexture)
        {
            for (int i = 0; i < BulletsAmount; i++)
            {
                float angle = (-15 + i * (30f / (BulletsAmount - 1))) * (float)(System.Math.PI / 180);
                Vector2 bulletPosition = new Vector2(Position.X + Size / 2 - Bullet.Size / 2, Position.Y);
                Vector2 bulletVelocity = new Vector2((float)System.Math.Sin(angle), -(float)System.Math.Cos(angle)) * 10;
                bullets.Add(new Bullet(bulletPosition, bulletVelocity, Damage, bulletTexture));
            }
        }

        public bool CheckCollision(Bullet bullet)
        {
            Rectangle bulletRect = new Rectangle((int)bullet.Position.X, (int)bullet.Position.Y, Bullet.Size, Bullet.Size);
            return Hitbox.Intersects(bulletRect);
        }

        public int PowerUp()
        {
            if (Damage >= MaxDamage && BulletsAmount >= MaxBulletsAmount)
            {
                return 500;
            }
            if (Damage < MaxDamage) Damage++;
            if (BulletsAmount < MaxBulletsAmount) BulletsAmount++;
            return 0;
        }
    }
}
