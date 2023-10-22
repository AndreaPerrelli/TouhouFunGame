using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1.Effects;

namespace Touhou
{
    internal class Player
    {
        private Vector2 Position;
        private Vector2 Velocity;
        public const int Size = 50; // grandezza dello sprite
        public static int HitboxSize { get; } = 20; // Una hitbox più piccola all'interno del sprite
        private float scale;

        public int BulletsAmount { get; set; } = 3;  // Numero di proiettili sparati contemporaneamente
        public int Damage { get; set; } = 30;         // Danno base del proiettile

        public static int DefaultDamage = 30;
        public static int DefaultBulletsAmount = 3;

        private Texture2D playerTexture;


        const int MAX_DAMAGE = 50; // o qualsiasi altro valore tu ritenga appropriato
        const int MAX_NUMBEROFBULLETS = 10; // o qualsiasi altro valore tu ritenga appropriato


        // Proprietà per accedere e modificare il valore di position
        public Vector2 GetPosition()
        { return Position; }


        // Proprietà per accedere e modificare il valore di position
        public void SetPosition(Vector2 value)
        { Position = value; }

        // Proprietà per accedere e modificare il valore di velocity
        public Vector2 GetVelocity()
        { return Velocity; }

        // Proprietà per accedere e modificare il valore di velocity
        public void SetVelocity(Vector2 value)
        { Velocity = value; }

        // Proprietà per accedere e modificare il valore di scale
        public float GetScale()
        { return scale; }

        // Proprietà per accedere e modificare il valore di scale
        public void SetScale(float value)
        { scale = value; }

        // Proprietà solo-lettura per accedere al valore di playerTexture
        public Texture2D GetPlayerTexture()
        { return playerTexture; }


        public Player(Vector2 Position, Vector2 Velocity, float scale, Texture2D texture)
        {
            this.SetPosition(Position);
            this.SetVelocity(Velocity);
            this.scale = scale;
            playerTexture = texture;
        }

        public Player(Vector2 position, Vector2 velocity, float scale, int bulletsAmount, int damage, Texture2D playerTexture)
        {
            SetPosition(position);
            SetVelocity(velocity);
            this.scale = scale;
            BulletsAmount = bulletsAmount;
            Damage = damage;
            this.playerTexture = playerTexture;
        }

        public Rectangle Hitbox
        {
            get
            {
                // Supponiamo che la posizione del giocatore sia il centro della texture
                Vector2 playerCenter = Position;

                // Calcola le dimensioni originali scalate dell'hitbox
                int originalHitboxWidth = (int)(playerTexture.Width * scale);
                int originalHitboxHeight = (int)(playerTexture.Height * scale);

                // Riduci le dimensioni dell'hitbox del 50%
                int reducedHitboxWidth = (int)(originalHitboxWidth * 0.5f);
                int reducedHitboxHeight = (int)(originalHitboxHeight * 0.5f);

                // Calcola l'angolo in alto a sinistra dell'hitbox in modo che sia centrato rispetto alla posizione del giocatore
                int hitboxX = (int)(playerCenter.X - reducedHitboxWidth / 2);
                int hitboxY = (int)(playerCenter.Y - reducedHitboxHeight / 2);

                return new Rectangle(hitboxX, hitboxY, reducedHitboxWidth, reducedHitboxHeight);
            }
        }


        public void Update()
        {
            SetPosition(GetPosition() + GetVelocity());
        }

        // metodo per sparare
        public void Shoot(List<Bullet> bullets, Texture2D texture)
        {
            for (int i = 0; i < BulletsAmount; i++)
            {
                float angle = (-15 + (i * (30f / (BulletsAmount - 1)))) * (float)(Math.PI / 180); // Calcola l'angolo in radianti
                Vector2 bulletPosition = new Vector2(GetPosition().X + Player.Size / 2 - Bullet.Size / 2, GetPosition().Y); // Adattare secondo le tue necessità
                Vector2 bulletVelocity = new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle)) * 10; // Nota il '-' davanti al Cos per far sì che il proiettile si muova verso l'alto
                bullets.Add(new Bullet(bulletPosition, bulletVelocity, this.Damage, texture));
            }
        }

        //metodo per gestire collisione con il proiettile
        public bool CheckCollision(Bullet bullet)
        {
            // Centra la hitbox all'interno del sprite del giocatore
            Rectangle playerRect = new Rectangle(
                (int)GetPosition().X + (Size - HitboxSize) / 2,
                (int)GetPosition().Y + (Size - HitboxSize) / 2,
                HitboxSize,
                HitboxSize
            );
            Rectangle bulletRect = new Rectangle(
                (int)bullet.Position.X,
                (int)bullet.Position.Y,
                Bullet.Size,
                Bullet.Size
            );

            return playerRect.Intersects(bulletRect);
        }

        public int PowerUp()
        {
            int bonusScore = 0;

            // Se DAMAGE e NUMBEROFBULLETS hanno raggiunto i loro limiti massimi
            if (this.Damage >= MAX_DAMAGE && this.BulletsAmount >= MAX_NUMBEROFBULLETS)
            {
                bonusScore = 500; // Aggiungi punti extra al punteggio. Puoi cambiare 500 con qualsiasi valore tu ritenga appropriato.
            }
            else
            {
                // Aumenta DAMAGE solo se non supera il limite massimo.
                if (this.Damage < MAX_DAMAGE)
                {
                    this.Damage += 1; // o qualsiasi altro incremento tu ritenga appropriato
                }

                // Aumenta NUMBEROFBULLETS solo se non supera il limite massimo.
                if (this.BulletsAmount < MAX_NUMBEROFBULLETS)
                {
                    this.BulletsAmount += 1; // o qualsiasi altro incremento tu ritenga appropriato
                }
            }

            return bonusScore;
        }




    }


}
