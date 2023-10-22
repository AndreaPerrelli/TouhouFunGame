using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Touhou;

internal class PickUpItem
{
    // Properties
    public Texture2D Texture { get; set; }
    public Vector2 Position { get; set; }
    public bool IsActive { get; set; } = true;
    public Vector2 Velocity { get; set; } = new Vector2(0, 2);
    public static int BonusPoints => BONUS_POINTS;

    // Constants
    private const int BONUS_POINTS = 200;
    private const float SCALE = 0.05f;

    // Update the position of the item
    public void Update()
    {
        Position += Velocity;
        // Add more update logic if needed, e.g., remove the item if it goes off-screen
    }

    // Draw the item if it's active
    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsActive)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, 0, Vector2.Zero, SCALE, SpriteEffects.None, 0);
        }
    }

    // Check collision with the player
    public bool CheckCollision(Player player)
    {
        Rectangle itemRect = new Rectangle(
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

        return itemRect.Intersects(playerRect);
    }
}
