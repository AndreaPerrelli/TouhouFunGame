using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;
using Microsoft.Xna.Framework.Audio;

namespace Touhou
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        #region Player Variables

        private Player player;
        private List<Bullet> bullets = new List<Bullet>();
        private const float PLAYER_SPEED = 6.0f;
        private float timeSinceLastPlayerShot = 0f;
        private const float playerShootCooldown = 0.2f;

        #endregion

        #region Enemy Variables

        private List<Enemy> enemies = new List<Enemy>();
        private List<Bullet> enemyBullets = new List<Bullet>();
        private double enemySpawnTime = 5;
        private double timeSinceLastSpawn = 0;
        private Random random = new();
        private float timeSinceLastEnemyShot = 0;
        private float enemyShootInterval = 5f;

        #endregion

        #region Game Mechanics

        private const float SPAWN_DECREASE_RATE = 0.99f;
        private const float SHOOT_DECREASE_RATE = 0.99f;
        private const float MIN_SPAWN_TIME = 0.5f;
        private const float MIN_SHOOT_INTERVAL = 0.3f;

        #endregion

        #region Score & Game State

        private int score = 0;
        private float timeSinceLastScoreIncrement = 0;
        private bool isGameOver = false;
        public GameState currentState = GameState.Menu;
        private bool hasDrawnGameOverScreen = false;
        private static readonly List<ScoreEntry> scoreEntries = new List<ScoreEntry>();
        private List<ScoreEntry> scores = scoreEntries;

        #endregion

        #region Textures and Fonts

        private Texture2D whitePixel;
        private SpriteFont gameOverFont;
        private SpriteFont scoreFont;

        #endregion

        #region Audio

        private Song mySong;

        #endregion

        private const int HEALTH_BAR_WIDTH = 50; // Larghezza della barra della salute
        private const int HEALTH_BAR_HEIGHT = 5;  // Altezza della barra della salute
        private Color HEALTH_BAR_COLOR = Color.Green; // Colore della barra quando è piena
        private Color HEALTH_BAR_BG_COLOR = Color.Red; // Colore di sfondo della barra

        private List<PowerUp> powerUps = new List<PowerUp>();
        private Texture2D powerUpTexture;
        private Texture2D pickUpItemTexture;
        private Texture2D mainCharacterTexture;
        private Texture2D enemiesTexture;
        private Texture2D enemiesBulletsTexture;
        private Texture2D playerBulletsTexture;
        private Random rng = new Random();
        private List<PickUpItem> pickUpItems = new List<PickUpItem>();
        private SoundEffect playerShootSound;
        private SoundEffect explosionSound;
        private SoundEffect powerUpSound;
        private SoundEffect pickUpItemSound;


        // Variabili Membro
        private Texture2D backgroundNear;  // Texture dello sfondo vicino
        private Texture2D backgroundFar;   // Texture dello sfondo lontano
        private Vector2 backgroundNearPosition; // Posizione iniziale dello sfondo vicino
        private Vector2 backgroundFarPosition;  // Posizione iniziale dello sfondo lontano
        private float backgroundNearSpeed = 1f;  // Velocità dello sfondo vicino
        private float backgroundFarSpeed = 1f;  // Velocità dello sfondo lontano

        private float scaleFactorNear;
        private float scaleFactorFar;

        private BulletPattern bulletPattern = new BulletPattern();

        private float scale = 0.1f;  // Ad esempio, riduci le dimensioni del 50%

        private const float ENEMY_BULLET_SPEED = 5.0f; // Definisci questa costante all'inizio della tua classe

        // Definisci le costanti per le risorse
        private const string BACKGROUND_NEAR_TEXTURE_PATH = "background_close";
        private const string BACKGROUND_FAR_TEXTURE_PATH = "background_distant";
        private const string POWER_UP_TEXTURE_PATH = "Doritos_Chips";
        private const string PICK_UP_ITEM_TEXTURE_PATH = "monster_energy";
        private const string ENEMIES_TEXTURE_PATH = "enemies";
        private const string ENEMIES_BULLETS_TEXTURE_PATH = "bullets";
        private const string PLAYER_BULLETS_TEXTURE_PATH = "player_bullets";

        private const string PLAYER_SHOOT_SOUND_PATH = "laser_beam";
        private const string EXPLOSION_SOUND_PATH = "explosion";
        private const string POWER_UP_SOUND_PATH = "powerup";
        private const string PICK_UP_ITEM_SOUND_PATH = "coin_collect";

        private const string FONT_PATH = "SpriteFont1";
        private const string FILE_NAME = "scores.json";
        private const string CONTENT_DIRECTORY = "Content";
        private const string GAME_OVER_TEXT = "YOU FAILED TO DESTROY MCDONALD EMPIRE";
        private const string MAIN_CHARACTER_ASSET = "main_character";
        private const string SONG_ASSET = "A Soul as Red as a Ground Cherry";

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = CONTENT_DIRECTORY;
            IsMouseVisible = true;
        }


        protected override void Initialize()
        {
            LoadMainCharacter();
            InitializePlayer();
            LoadAndPlayBackgroundMusic();
            LoadGameScores();

            currentState = GameState.Playing;

            base.Initialize();
        }

        protected override void LoadContent()
        {
           _spriteBatch = new SpriteBatch(GraphicsDevice);

            LoadTextures();
            LoadSounds();
            LoadFonts();
            InitializeBackground();

        }

        protected override void Update(GameTime gameTime)
        {
            HandleGlobalInput();
            // TODO: Add your update logic here
            switch (currentState)
            {
                case GameState.Playing:
                    // Tutta la logica del gioco in corso qui
                    UpdatePlayingState(gameTime);

                    break;

                case GameState.GameOver:
                    HandleGameOverState();
                    break;

                case GameState.Menu:
                    // TODO: Menu logic here
                    HandleMenuState();
                    //if (/* Player starts the game */)
                    //{
                    //    currentState = GameState.Playing;
                    //}
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            switch (currentState)
            {
                case GameState.Playing:
                    DrawPlayingState();
                    break;

                case GameState.GameOver:
                    DrawGameOverState();
                    break;

                case GameState.Menu:
                    DrawMenuState();
                    break;
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void ResetGame()
        {
            // Reinizializza il giocatore, i proiettili, i nemici, ecc.
            player = new Player
            (
                new Vector2(_graphics.PreferredBackBufferWidth / 2 - Player.Size / 2, _graphics.PreferredBackBufferHeight - Player.Size - 10),
                Vector2.Zero,
                0.1f,
                Player.DefaultBulletsAmount,// Resetta il numero di proiettili del giocatore al valore predefinito
                Player.DefaultDamage,  // Resetta il danno del giocatore al valore predefinito             
                mainCharacterTexture
            );
            bullets.Clear();
            enemies.Clear();
            enemyBullets.Clear();
            powerUps.Clear(); // Aggiunto per ripulire i power up.
            pickUpItems.Clear(); // Aggiunto per ripulire gli oggetti PickUpItem.

            backgroundNearPosition = Vector2.Zero; // Resetta la posizione di sfondo.
            backgroundFarPosition = new Vector2(0, backgroundFar.Height); // Considera che il backgroundFar inizi alla fine del backgroundNear.

            timeSinceLastSpawn = 0;
            enemyShootInterval = 1f;
            enemySpawnTime = 2;
            score = 0;
            timeSinceLastPlayerShot = 0f;
            isGameOver = false;

            // Suoni e musica
            MediaPlayer.Pause();
            // Nota: Non stiamo caricando nuovamente i suoni o cambiando il volume qui. Se hai cambiato dinamicamente il volume durante il gioco e vuoi resettarlo, dovrai farlo in questo metodo.
        }

        internal void SaveScores(List<ScoreEntry> updatedScores)
        {
            // Ordina la lista di punteggi in ordine decrescente e conserva solo i primi 10 punteggi
            updatedScores = updatedScores.OrderByDescending(s => s.Score).Take(10).ToList();

            // Converte la lista in formato JSON e salva su disco
            string json = JsonConvert.SerializeObject(updatedScores);
            File.WriteAllText(FILE_NAME, json);
        }


        public void DrawGameOverScreen(SpriteBatch spriteBatch)
        {
            // ... (disegnare altri elementi come il titolo "Game Over")

            var loadedScores = LoadScores();

            int startY = 100;  // Inizia da una certa y
            int xOffset = 50;
            int yOffset = 30;

            for (int i = 0; i < loadedScores.Count; i++)
            {
                var score = loadedScores[i];
                string text = $"{score.PlayerName}: {score.Score}";
                spriteBatch.DrawString(gameOverFont, text, new Vector2(xOffset, startY + (i * yOffset)), Color.White);
            }
        }

        internal void AddNewScore(String playerName, int newScoreValue)
        {
            // 1. Carica gli score esistenti.
            var scores = LoadScores();

            // 2. Crea un nuovo ScoreEntry e aggiungilo all'elenco.
            var newScore = new ScoreEntry { PlayerName = playerName, Score = newScoreValue};

            // 3. Controlla se lo score con lo stesso nome del giocatore e punteggio esiste già
            if (!scores.Any(s => s.PlayerName == playerName && s.Score == score))
            {
                // 4. Aggiungi lo score
                scores.Add(new ScoreEntry { PlayerName = playerName, Score = score });
            }

            // 5. Salva l'elenco aggiornato.
            SaveScores(scores);
        }

        private bool ShouldDropPowerUp()
        {
            const float DROP_PROBABILITY = 0.25f; // 25% di probabilità di rilasciare un power-up
            return random.NextDouble() < DROP_PROBABILITY;
        }

        private bool ShouldDropPickUpItem()
        {
            const float DROP_PROBABILITY = 0.50f; // 25% di probabilità di rilasciare un power-up
            return random.NextDouble() < DROP_PROBABILITY;
        }

        private void UpdatePlayingState(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            ResumeMediaPlayer();
            UpdateTimeTrackers(gameTime);
            UpdateEnemyTimings();
            HandleKeyboardInput(keyboardState);
            UpdateEntities(gameTime);
            UpdateBackground();
        }

        private void ResumeMediaPlayer()
        {
            MediaPlayer.Resume();
        }

        private void UpdateTimeTrackers(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            timeSinceLastPlayerShot += elapsedSeconds;
            timeSinceLastSpawn += elapsedSeconds;
            timeSinceLastEnemyShot += elapsedSeconds;
            timeSinceLastScoreIncrement += elapsedSeconds;
        }

        private void UpdateEnemyTimings()
        {
            enemySpawnTime *= SPAWN_DECREASE_RATE;
            enemyShootInterval *= SHOOT_DECREASE_RATE;

            enemySpawnTime = Math.Max(enemySpawnTime, MIN_SPAWN_TIME);
            enemyShootInterval = Math.Max(enemyShootInterval, MIN_SHOOT_INTERVAL);
        }

        private void HandleKeyboardInput(KeyboardState keyboardState)
        {
            player.Velocity = Vector2.Zero;

            UpdatePlayerMovement(keyboardState);
            HandlePlayerShooting(keyboardState);
        }

        private void UpdatePlayerMovement(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Left))
                player.Velocity += new Vector2(-PLAYER_SPEED, 0);
            if (keyboardState.IsKeyDown(Keys.Right))
                player.Velocity += new Vector2(PLAYER_SPEED, 0);
            if (keyboardState.IsKeyDown(Keys.Up))
                player.Velocity += new Vector2(0, -PLAYER_SPEED);
            if (keyboardState.IsKeyDown(Keys.Down))
                player.Velocity += new Vector2(0, PLAYER_SPEED);
        }

        private void HandlePlayerShooting(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Space) && timeSinceLastPlayerShot >= playerShootCooldown)
            {
                player.Shoot(bullets, playerBulletsTexture);
                playerShootSound.Play(0.2f, 0.0f, 0.0f);
                timeSinceLastPlayerShot = 0;
            }
        }

        private void UpdateEntities(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            player.Update();
            UpdateBullets();
            SpawnEnemies();            
            UpdateEnemies(elapsedSeconds);
            HandleEnemyShooting(gameTime);
            UpdateAndCleanupEnemyBullets();
            CheckEnemyBulletCollisions();
            UpdateScore();
            UpdatePowerUps();
            UpdatePickUpItems();
        }

        private void UpdateBullets()
        {
            bullets.ForEach(bullet => bullet.Update());
            bullets.RemoveAll(bullet => bullet.Position.Y < 0);
        }

        private void SpawnEnemies()
        {
            if (timeSinceLastSpawn >= enemySpawnTime)
            {
                enemies.Add(new Enemy(new Vector2(random.Next(0, _graphics.PreferredBackBufferWidth - Enemy.Size), 0), enemiesTexture));
                timeSinceLastSpawn = 0;
            }
        }

        private void UpdateEnemies(float elapsedSeconds)
        {
            foreach (var enemy in enemies)
            {
                enemy.Update(elapsedSeconds);
            }
            enemies.RemoveAll(enemy => enemy.Position.Y > _graphics.PreferredBackBufferHeight);
        }

        private void CheckEnemyBulletCollisions()
        {
            HandleBulletEnemyCollisions();
            HandleBulletPlayerCollisions();
        }

        private void UpdateScore()
        {
            if (timeSinceLastScoreIncrement >= 1)
            {
                score += 50;
                timeSinceLastScoreIncrement -= 1;
            }
        }

        private void UpdatePowerUps()
        {
            foreach (var powerUp in powerUps.ToList())
            {
                if (powerUp.CheckCollision(player))
                {
                    int bonusScore = player.PowerUp();
                    score += bonusScore;
                    powerUpSound.Play(0.5f, 0, 0);
                    powerUps.Remove(powerUp);
                }
                powerUp.Update();
            }
        }

        private void UpdatePickUpItems()
        {
            foreach (var pickUpItem in pickUpItems.ToList())
            {
                if (pickUpItem.IsActive)
                {
                    if (pickUpItem.CheckCollision(player))
                    {
                        score += PickUpItem.BonusPoints;
                        pickUpItemSound.Play(0.5f, 0, 0);
                        pickUpItem.IsActive = false;
                    }
                    pickUpItem.Update();
                }
                else
                {
                    pickUpItems.Remove(pickUpItem);
                }
            }
        }

        private void UpdateBackground()
        {
            backgroundNearPosition.Y += backgroundNearSpeed;
            backgroundFarPosition.Y += backgroundFarSpeed;

            if (backgroundNearPosition.Y >= GraphicsDevice.Viewport.Height)
            {
                backgroundNearPosition.Y = backgroundFarPosition.Y - backgroundNear.Height * scaleFactorNear;
            }

            if (backgroundFarPosition.Y >= GraphicsDevice.Viewport.Height)
            {
                backgroundFarPosition.Y = backgroundNearPosition.Y - backgroundFar.Height * scaleFactorFar;
            }
        }

        private void HandleBulletEnemyCollisions()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                for (int j = bullets.Count - 1; j >= 0; j--)
                {
                    if (enemies[i].CheckCollision(bullets[j]))
                    {
                        enemies[i].Hit();
                        bullets.RemoveAt(j);

                        if (enemies[i].Health <= 0)
                        {
                            HandleEnemyDeath(i);
                        }
                        break;
                    }
                }
            }
        }

        private void HandleEnemyDeath(int enemyIndex)
        {
            if (ShouldDropPowerUp())
            {
                powerUps.Add(new PowerUp(powerUpTexture, enemies[enemyIndex].Position));
            }
            else if (ShouldDropPickUpItem())
            {
                pickUpItems.Add(new PickUpItem
                {
                    Texture = pickUpItemTexture,
                    Position = enemies[enemyIndex].Position
                });
            }
            explosionSound.Play(0.2f, 0.0f, 0.0f);
            enemies.RemoveAt(enemyIndex);
            score += 100;
        }

        private void HandleBulletPlayerCollisions()
        {
            for (int i = enemyBullets.Count - 1; i >= 0; i--)
            {
                if (player.CheckCollision(enemyBullets[i]))
                {
                    currentState = GameState.GameOver;
                    hasDrawnGameOverScreen = false;
                    // Exit();  // Uncomment this line if you want the game to close immediately upon collision
                }
            }
        }

        private void HandleEnemyShooting(GameTime gameTime)
        {
            foreach (var enemy in enemies)
            {
                enemy.TimeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (enemy.TimeSinceLastShot >= enemy.ShootInterval)
                {
                    List<Vector2> bulletsDirections = bulletPattern.GetRandomPattern(enemy.Position, player.Position, ENEMY_BULLET_SPEED);

                    foreach (var direction in bulletsDirections)
                    {
                        Vector2 bulletPosition = new Vector2(enemy.Position.X + Enemy.Size / 2 - Bullet.Size / 2, enemy.Position.Y + Enemy.Size);
                        enemyBullets.Add(new Bullet(bulletPosition, direction, enemiesBulletsTexture));
                    }

                    enemy.TimeSinceLastShot = 0;  // Resetta il timer del nemico
                }
                enemy.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        private void UpdateAndCleanupEnemyBullets()
        {
            foreach (var bullet in enemyBullets)
            {
                bullet.Update();
            }

            enemyBullets.RemoveAll(bullet => bullet.Position.Y < 0 || bullet.Position.Y > _graphics.PreferredBackBufferHeight);
        }


        private void HandleGlobalInput()
        {
            var keyboardState = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
                Exit();
        }


        private void HandleGameOverState()
        {
            string currentWindowsUserName = Environment.UserName;
            AddNewScore(currentWindowsUserName, score);
            MediaPlayer.Stop();

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Enter))
            {
                currentState = GameState.Playing;
                ResetGame();
                MediaPlayer.Play(mySong);
            }
        }

        private void HandleMenuState()
        {
            // TODO: Implement menu logic here
            // You can further decompose it as you see fit.
        }


        private void DrawPlayingState()
        {
            DrawBackgrounds();
            DrawPlayer();
            DrawBullets();
            DrawEnemies();
            DrawEnemyBullets();
            DrawPowerUps();
            DrawPickUpItems();
            DrawScore();
        }

        private void DrawBackgrounds()
        {
            DrawBackground(backgroundFar, ref backgroundFarPosition);
            DrawBackground(backgroundNear, ref backgroundNearPosition);
        }

        private void DrawBackground(Texture2D texture, ref Vector2 position)
        {
            float scaleX = (float)GraphicsDevice.Viewport.Width / texture.Width;
            float scaleY = (float)GraphicsDevice.Viewport.Height / texture.Height;
            float scaleFactor = Math.Max(scaleX, scaleY);

            if (position.Y < GraphicsDevice.Viewport.Height)
            {
                _spriteBatch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, scaleFactor, SpriteEffects.None, 0);
            }
        }

        private void DrawPlayer()
        {
            Vector2 textureDimensions = new Vector2(mainCharacterTexture.Width * scale, mainCharacterTexture.Height * scale);
            Vector2 drawPosition = new Vector2(player.Position.X - textureDimensions.X / 2, player.Position.Y - textureDimensions.Y / 2);
            _spriteBatch.Draw(mainCharacterTexture, drawPosition, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        private void DrawBullets()
        {
            foreach (var bullet in bullets)
            {
                _spriteBatch.Draw(bullet.Texture, new Rectangle((int)bullet.Position.X, (int)bullet.Position.Y, Bullet.Size, Bullet.Size), Color.Red);
            }
        }

        private void DrawEnemies()
        {
            foreach (var enemy in enemies)
            {
                _spriteBatch.Draw(enemy.Texture, new Rectangle((int)enemy.Position.X, (int)enemy.Position.Y, Enemy.Size, Enemy.Size), enemy.TintColor);
                DrawEnemyHealthBar(enemy);
            }
        }

        private void DrawEnemyHealthBar(Enemy enemy)
        {
            int currentHealthBarWidth = (int)((float)enemy.Health / enemy.MaxHealth * HEALTH_BAR_WIDTH);
            Vector2 healthBarPosition = new Vector2(enemy.Position.X + (Enemy.Size - HEALTH_BAR_WIDTH) / 2, enemy.Position.Y - HEALTH_BAR_HEIGHT - 2);
            _spriteBatch.Draw(whitePixel, new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y, HEALTH_BAR_WIDTH, HEALTH_BAR_HEIGHT), HEALTH_BAR_BG_COLOR);
            _spriteBatch.Draw(whitePixel, new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y, currentHealthBarWidth, HEALTH_BAR_HEIGHT), HEALTH_BAR_COLOR);
        }

        private void DrawEnemyBullets()
        {
            Console.WriteLine($"Drawing {enemyBullets.Count} enemy bullets");  // Questo ti dirà quanti proiettili stai cercando di disegnare.

            foreach (var bullet in enemyBullets)
            {
                _spriteBatch.Draw(bullet.Texture, new Rectangle((int)bullet.Position.X, (int)bullet.Position.Y, Bullet.Size, Bullet.Size), Color.Yellow);
            }
        }

        private void DrawPowerUps()
        {
            foreach (var powerUp in powerUps)
            {
                powerUp.Draw(_spriteBatch);
            }
        }

        private void DrawPickUpItems()
        {
            foreach (var item in pickUpItems)
            {
                item.Draw(_spriteBatch);
            }
        }

        private void DrawScore()
        {
            _spriteBatch.DrawString(scoreFont, $"Score: {score}", new Vector2(10, 10), Color.White);
        }

        private void DrawGameOverState()
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.DrawString(gameOverFont, GAME_OVER_TEXT, new Vector2(_graphics.PreferredBackBufferWidth / 2 - 150, _graphics.PreferredBackBufferHeight / 2 - 10), Color.Red);
            DrawGameOverScreen(_spriteBatch);
        }

        private void DrawMenuState()
        {
            // Implement your menu drawing logic here.
        }

        private void LoadTextures()
        {
            whitePixel = CreateWhitePixelTexture();

            backgroundNear = Content.Load<Texture2D>(BACKGROUND_NEAR_TEXTURE_PATH);
            backgroundFar = Content.Load<Texture2D>(BACKGROUND_FAR_TEXTURE_PATH);
            powerUpTexture = Content.Load<Texture2D>(POWER_UP_TEXTURE_PATH);
            pickUpItemTexture = Content.Load<Texture2D>(PICK_UP_ITEM_TEXTURE_PATH);
            enemiesTexture = Content.Load<Texture2D>(ENEMIES_TEXTURE_PATH);
            enemiesBulletsTexture = Content.Load<Texture2D>(ENEMIES_BULLETS_TEXTURE_PATH);
            playerBulletsTexture = Content.Load<Texture2D>(PLAYER_BULLETS_TEXTURE_PATH);
        }

        private void LoadSounds()
        {
            playerShootSound = Content.Load<SoundEffect>(PLAYER_SHOOT_SOUND_PATH);
            explosionSound = Content.Load<SoundEffect>(EXPLOSION_SOUND_PATH);
            powerUpSound = Content.Load<SoundEffect>(POWER_UP_SOUND_PATH);
            pickUpItemSound = Content.Load<SoundEffect>(PICK_UP_ITEM_SOUND_PATH);
        }

        private void LoadFonts()
        {
            gameOverFont = Content.Load<SpriteFont>(FONT_PATH);
            scoreFont = Content.Load<SpriteFont>(FONT_PATH);
        }

        private void InitializeBackground()
        {
            float scaleXNear = (float)GraphicsDevice.Viewport.Width / backgroundNear.Width;
            float scaleYNear = (float)GraphicsDevice.Viewport.Height / backgroundNear.Height;
            scaleFactorNear = Math.Max(scaleXNear, scaleYNear);

            float scaleXFar = (float)GraphicsDevice.Viewport.Width / backgroundFar.Width;
            float scaleYFar = (float)GraphicsDevice.Viewport.Height / backgroundFar.Height;
            scaleFactorFar = Math.Max(scaleXFar, scaleYFar);

            backgroundNearPosition = new Vector2(0, 0);
            backgroundFarPosition = new Vector2(0, -backgroundNear.Height * scaleFactorNear);
        }

        private Texture2D CreateWhitePixelTexture()
        {
            Texture2D texture = new Texture2D(GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }

        internal List<ScoreEntry> LoadScores()
        {
            if (File.Exists(FILE_NAME))
            {
                string json = File.ReadAllText(FILE_NAME);
                scores = JsonConvert.DeserializeObject<List<ScoreEntry>>(json);
            }
            return scores;
        }


        private void LoadMainCharacter()
        {
            mainCharacterTexture = Content.Load<Texture2D>(MAIN_CHARACTER_ASSET);
        }

        private void InitializePlayer()
        {
            Vector2 initialPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2 - Player.Size / 2,
                _graphics.PreferredBackBufferHeight - Player.Size - 10
            );

            player = new Player(
                initialPosition,
                Vector2.Zero,
                0.1f,
                Player.DefaultBulletsAmount,
                Player.DefaultDamage,
                mainCharacterTexture
            );
        }

        private void LoadAndPlayBackgroundMusic()
        {
            mySong = Content.Load<Song>(SONG_ASSET);
            MediaPlayer.Play(mySong);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
        }

        private void LoadGameScores()
        {
            scores = LoadScores();
        }

    }

}
