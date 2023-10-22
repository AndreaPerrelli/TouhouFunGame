using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Touhou;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

internal class PickUpItem
{
    public Texture2D Texture { get; set; }
    public Vector2 Position;
    public bool IsActive { get; set; } = true;
    public const int BONUS_POINTS = 200;
    public Vector2 Velocity = new Vector2(0, 2);  // Puoi cambiare la velocità come preferisci

    float scale = 0.05f;  // Riduci la dimensione del 30% ad esempio

    public void Update()
    {
        Position += Velocity;
        // Aggiungi altre logiche dell'update se necessario, ad esempio rimuovere l'oggetto se esce dallo schermo
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsActive)
        {
            spriteBatch.Draw(Texture, Position, null, Microsoft.Xna.Framework.Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
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
}

