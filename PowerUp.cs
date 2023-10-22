using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Touhou
{
    internal class PowerUp
    {
        // Properties
        public Vector2 Position { get; private set; }
        public Texture2D Texture { get; private set; }
        public Vector2 Velocity { get; private set; } = new Vector2(0, 2);

        // Constants
        private const float SCALE = 0.05f;

        public PowerUp(Texture2D texture, Vector2 startPosition)
        {
            Texture = texture;
            Position = startPosition;
        }

        // Update power-up position
        public void Update()
        {
            Position += Velocity;
        }

        // Check collision with the player
        public bool CheckCollision(Player player)
        {
            Rectangle powerUpRect = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)(Texture.Width * SCALE),
                (int)(Texture.Height * SCALE)
            );

            Rectangle playerRect = new Rectangle(
                (int)player.Position.X,
                (int)player.Position.Y,
                Player.Size,
                Player.Size
            );

            return powerUpRect.Intersects(playerRect);
        }

        // Draw the power-up
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, 0, Vector2.Zero, SCALE, SpriteEffects.None, 0);
        }
    }
}
