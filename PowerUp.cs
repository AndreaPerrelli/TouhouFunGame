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
    internal class PowerUp
    {
        public Vector2 Position;
        public Texture2D Texture { get; private set; }

        public Vector2 Velocity = new Vector2(0, 2);  // Puoi cambiare la velocità come preferisci
        float scale = 0.05f;  // Riduci la dimensione del 30% ad esempio

        public PowerUp(Texture2D texture, Vector2 startPosition)
        {
            this.Texture = texture;
            this.Position = startPosition;
        }

        public void Update()
        {
            // Il power up si muove verso il basso, come un nemico.
            Position += Velocity;
        }

        public bool CheckCollision(Player player)
        {
            // Dimensioni del giocatore.
            float playerWidth = Player.Size;  // se Player.Size rappresenta la larghezza
            float playerHeight = Player.Size;  // assumiamo che la larghezza e l'altezza siano uguali; cambia se necessario

            // Collisione basata su rettangoli.
            bool collided = player.GetPosition().X + playerWidth > Position.X &&
                            player.GetPosition().X < Position.X + Texture.Width * scale &&
                            player.GetPosition().Y + playerHeight > Position.Y &&
                            player.GetPosition().Y < Position.Y + Texture.Height * scale;

            return collided;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
           spriteBatch.Draw(Texture, Position, null, Microsoft.Xna.Framework.Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }
}
