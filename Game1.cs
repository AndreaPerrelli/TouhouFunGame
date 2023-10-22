using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;
using Newtonsoft.Json;
using System.IO;
using SharpDX.Direct2D1;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;
using Microsoft.Xna.Framework.Audio;
using SharpDX.Direct2D1.Effects;
using SharpDX.Direct3D9;

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

        const float SPAWN_DECREASE_RATE = 0.99f;
        const float SHOOT_DECREASE_RATE = 0.99f;
        const float MIN_SPAWN_TIME = 0.5f;
        const float MIN_SHOOT_INTERVAL = 0.3f;

        #endregion

        #region Score & Game State

        int score = 0;
        float timeSinceLastScoreIncrement = 0;
        bool isGameOver = false;
        public GameState currentState = GameState.Menu;
        private bool hasDrawnGameOverScreen = false;
        private static readonly List<ScoreEntry> scoreEntries = new List<ScoreEntry>();
        List<ScoreEntry> scores = scoreEntries;

        #endregion

        #region Textures and Fonts

        private Texture2D whitePixel;
        private SpriteFont gameOverFont;
        private SpriteFont scoreFont;

        #endregion

        #region Audio

        Song mySong;

        #endregion

        const int HEALTH_BAR_WIDTH = 50; // Larghezza della barra della salute
        const int HEALTH_BAR_HEIGHT = 5;  // Altezza della barra della salute
        Color HEALTH_BAR_COLOR = Color.Green; // Colore della barra quando è piena
        Color HEALTH_BAR_BG_COLOR = Color.Red; // Colore di sfondo della barra

        private List<PowerUp> powerUps = new List<PowerUp>();
        private Texture2D powerUpTexture;
        private Texture2D pickUpItemTexture;
        private Texture2D mainCharacterTexture;
        private Texture2D enemiesTexture;
        private Texture2D enemiesBulletsTexture;
        private Texture2D playerBulletsTexture;
        Random rng = new Random();
        List<PickUpItem> pickUpItems = new List<PickUpItem>();
        SoundEffect playerShootSound;
        SoundEffect explosionSound;
        SoundEffect powerUpSound;
        SoundEffect pickUpItemSound;


        // Variabili Membro
        Texture2D backgroundNear;  // Texture dello sfondo vicino
        Texture2D backgroundFar;   // Texture dello sfondo lontano
        Vector2 backgroundNearPosition; // Posizione iniziale dello sfondo vicino
        Vector2 backgroundFarPosition;  // Posizione iniziale dello sfondo lontano
        float backgroundNearSpeed = 1f;  // Velocità dello sfondo vicino
        float backgroundFarSpeed = 1f;  // Velocità dello sfondo lontano


        float transitionAlpha = 0;
        bool isTransitioning = false;

        float scaleFactorNear;
        float scaleFactorFar;

        private BulletPattern bulletPattern = new BulletPattern();

        float scale = 0.1f;  // Ad esempio, riduci le dimensioni del 50%



        const float ENEMY_BULLET_SPEED = 5.0f; // Definisci questa costante all'inizio della tua classe
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        internal List<ScoreEntry> LoadScores()
        {
            if (File.Exists("scores.json"))
            {
                string json = File.ReadAllText("scores.json");
                scores = JsonConvert.DeserializeObject<List<ScoreEntry>>(json);
            }
            return scores;
        }


        protected override void Initialize()
        {

            //carico texture del personaggio
            mainCharacterTexture = Content.Load<Texture2D>("main_character");
            // TODO: Add your initialization logic here
            player = new Player(new Vector2(_graphics.PreferredBackBufferWidth / 2 - Player.Size / 2, _graphics.PreferredBackBufferHeight - Player.Size - 10),
                                Vector2.Zero,
                                0.1f,
                                Player.DefaultBulletsAmount,
                                Player.DefaultDamage,                                
                                mainCharacterTexture);

            //carico l'OST per tutta la durata del gioco
            mySong = Content.Load<Song>("A Soul as Red as a Ground Cherry");
            MediaPlayer.Play(mySong);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f; // imposta il volume al 50%
            scores = LoadScores();
            currentState = GameState.Playing;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            whitePixel.SetData(new[] { Color.White });

            //carico font per schermata di game over
            gameOverFont = Content.Load<SpriteFont>("SpriteFont1");
            
            //carico font per la scritta dello Score
            scoreFont = Content.Load<SpriteFont>("SpriteFont1");

            //carico texture per power up
            powerUpTexture = Content.Load<Texture2D>("Doritos_Chips");

            //carico texture per item pickup
            pickUpItemTexture = Content.Load<Texture2D>("monster_energy");

            //carico suono di sparo del giocatore
            playerShootSound = Content.Load<SoundEffect>("laser_beam");

            //carico suono di esplosione dei nemici
            explosionSound = Content.Load<SoundEffect>("explosion");

            //carico suono del powerup
            powerUpSound = Content.Load<SoundEffect>("powerup");

            //carico suono di quando si raccoglie item
            pickUpItemSound = Content.Load<SoundEffect>("coin_collect");

            // background vicino
            backgroundNear = Content.Load<Texture2D>("background_close");

            // background lontano
            backgroundFar = Content.Load<Texture2D>("background_distant");

            //carico texture dei nemici
            enemiesTexture = Content.Load<Texture2D>("enemies");

            //carico texture dei proiettili
            enemiesBulletsTexture = Content.Load<Texture2D>("bullets");

            //carico texture dei proiettili del giocatore
            playerBulletsTexture = Content.Load<Texture2D>("player_bullets");


            // Calcola i fattori di scala per adattare le texture allo schermo
            // Calcola i fattori di scala per adattare le texture allo schermo
            float scaleXNear = (float)GraphicsDevice.Viewport.Width / backgroundNear.Width;
            float scaleYNear = (float)GraphicsDevice.Viewport.Height / backgroundNear.Height;
            scaleFactorNear = Math.Max(scaleXNear, scaleYNear);

            float scaleXFar = (float)GraphicsDevice.Viewport.Width / backgroundFar.Width;
            float scaleYFar = (float)GraphicsDevice.Viewport.Height / backgroundFar.Height;
            scaleFactorFar = Math.Max(scaleXFar, scaleYFar);

            backgroundNearPosition = new Vector2(0, 0);
            backgroundFarPosition = new Vector2(0, -backgroundNear.Height * scaleFactorNear);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            var keyboardState = Keyboard.GetState();
            switch (currentState)
            {
                case GameState.Playing:
                    // Tutta la logica del gioco in corso qui

                    MediaPlayer.Resume();
                    timeSinceLastPlayerShot += (float)gameTime.ElapsedGameTime.TotalSeconds; // dove gameTime è l'argomento del metodo Update
                    enemySpawnTime *= SPAWN_DECREASE_RATE;
                    enemyShootInterval *= SHOOT_DECREASE_RATE;
                    // Dopo aver diminuito i tempi, controlla i limiti minimi:
                    if (enemySpawnTime < MIN_SPAWN_TIME) enemySpawnTime = MIN_SPAWN_TIME;
                    if (enemyShootInterval < MIN_SHOOT_INTERVAL) enemyShootInterval = MIN_SHOOT_INTERVAL;
                    KeyboardState previousKeyboardState;

                    player.SetVelocity(Vector2.Zero); // Azzera la velocità a ogni frame

                    if (keyboardState.IsKeyDown(Keys.Left))
                        player.SetVelocity(new Vector2(-PLAYER_SPEED, player.GetVelocity().Y));
                    if (keyboardState.IsKeyDown(Keys.Right))
                        player.SetVelocity(new Vector2(PLAYER_SPEED, player.GetVelocity().Y));
                    if (keyboardState.IsKeyDown(Keys.Up))
                        player.SetVelocity(new Vector2(player.GetVelocity().X, -PLAYER_SPEED));
                    if (keyboardState.IsKeyDown(Keys.Down))
                        player.SetVelocity(new Vector2(player.GetVelocity().X, PLAYER_SPEED));
                    if (keyboardState.IsKeyDown(Keys.Space) && timeSinceLastPlayerShot >= playerShootCooldown)
                    {
                        player.Shoot(bullets, playerBulletsTexture);
                        playerShootSound.Play(0.2f, 0.0f, 0.0f);
                        timeSinceLastPlayerShot = 0;
                    }



                    previousKeyboardState = keyboardState;

                    player.Update();
                    bullets.ForEach(bullet => bullet.Update());
                    bullets.RemoveAll(bullet => bullet.Position.Y < 0);  // Rimuovi i proiettili che escono dallo schermo.

                    timeSinceLastSpawn += gameTime.ElapsedGameTime.TotalSeconds;
                    if (timeSinceLastSpawn >= enemySpawnTime)
                    {

                        enemies.Add(new Enemy(new Vector2(random.Next(0, _graphics.PreferredBackBufferWidth - Enemy.Size), 0), enemiesTexture));
                        timeSinceLastSpawn = 0;
                    }

                    // Aggiorna i nemici
                    foreach (var enemy in enemies)
                    {
                        enemy.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                    }

                    // Controlla le collisioni tra nemici e proiettili
                    for (int i = enemies.Count - 1; i >= 0; i--)
                    {
                        for (int j = bullets.Count - 1; j >= 0; j--)
                        {
                            if (enemies[i].CheckCollision(bullets[j]))
                            {
                                enemies[i].Hit(); // chiama il metodo Hit per cambiare il colore
                                bullets.RemoveAt(j);  // Rimuove il proiettile che ha colpito il nemico

                                if (enemies[i].Health <= 0)
                                {
                                    if (ShouldDropPowerUp())
                                    {
                                        powerUps.Add(new PowerUp(powerUpTexture, enemies[i].Position));

                                    }
                                    else
                                    {
                                        if (ShouldDropPickUpItem())
                                            pickUpItems.Add(new PickUpItem
                                            {
                                                Texture = pickUpItemTexture, // La tua texture per il PickUpItem
                                                Position = enemies[i].Position
                                            });
                                    }
                                    explosionSound.Play(0.2f, 0.0f, 0.0f); // riproduce suono esplosione
                                    enemies.RemoveAt(i);  // Rimuove il nemico solo se la sua salute è a zero
                                    score += 100;  // Aggiorna lo score solo se il nemico è effettivamente "morto"
                                                   // Decide se droppare un power-up, puoi usare la probabilità qui

                                }

                                break;
                            }
                        }
                    }


                    // Rimuovi i nemici che sono fuori dallo schermo
                    enemies.RemoveAll(enemy => enemy.Position.Y > _graphics.PreferredBackBufferHeight);


                    // aggiungere logica per proiettili nemici

                    timeSinceLastEnemyShot += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    foreach (var enemy in enemies)
                    {
                        enemy.TimeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (enemy.TimeSinceLastShot >= enemy.ShootInterval)
                        {
                            // Usa GetRandomPattern invece della logica predefinita
                            List<Vector2> bulletsDirections = bulletPattern.GetRandomPattern(enemy.Position, player.GetPosition(), ENEMY_BULLET_SPEED);

                            foreach (var direction in bulletsDirections)
                            {
                                Vector2 bulletPosition = new Vector2(enemy.Position.X + Enemy.Size / 2 - Bullet.Size / 2, enemy.Position.Y + Enemy.Size);
                                enemyBullets.Add(new Bullet(bulletPosition, direction, enemiesBulletsTexture));
                            }

                            enemy.TimeSinceLastShot = 0;  // Resetta il timer del nemico
                        }

                        enemy.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                    }

                    foreach (var bullet in enemyBullets)
                    {
                        bullet.Update();
                    }

                    // Aggiungere una collisione tra i proiettili nemici e il giocatore 
                    for (int i = enemyBullets.Count - 1; i >= 0; i--)
                    {
                        if (player.CheckCollision(enemyBullets[i]))
                        {
                            // Qui gestirai la logica della sconfitta
                            currentState = GameState.GameOver;
                            hasDrawnGameOverScreen = false;
                            //                   Exit(); // per ora chiudiamo il gioco
                        }
                    }

                    // Aggiorno il punteggio ogni secondo
                    timeSinceLastScoreIncrement += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timeSinceLastScoreIncrement >= 1)
                    {
                        score += 50;
                        timeSinceLastScoreIncrement -= 1;
                    }


                    //Nel loop di aggiornamento, controlla se il giocatore raccoglie un power up:
                    foreach (var powerUp in powerUps.ToList())
                    {
                        if (powerUp.CheckCollision(player))
                        {
                            // Aggiorna il giocatore con il power up qui.
                            // Supponiamo che questa parte venga eseguita quando il giocatore raccoglie un power-up:
                            int bonusScore = player.PowerUp();
                            score += bonusScore;
                            powerUpSound.Play(0.5f, 0, 0);
                            // Potresti anche rimuovere il power up dalla lista qui, ma dipende da come vuoi gestirlo.
                            powerUps.Remove(powerUp);
                        }
                    }

                    // Aggiorna tutti i power up attivi.
                    foreach (var powerUp in powerUps)
                    {
                        powerUp.Update();
                    }

                    // Aggiungo la logica per rilevare la collisione con PickUpItem:
                    foreach (var item in pickUpItems)
                    {
                        if (item.IsActive && item.CheckCollision(player))
                        {
                            score += PickUpItem.BONUS_POINTS;
                            pickUpItemSound.Play(0.5f, 0, 0);
                            item.IsActive = false;
                            
                        }
                    }

                    // Aggiorna tutti gli pickupitem attivi.
                    foreach (var pickUpItem in pickUpItems)
                    {
                       pickUpItem.Update();
                    }

                    // Gestione dello sfondo
                    backgroundNearPosition.Y += backgroundNearSpeed;
                    backgroundFarPosition.Y += backgroundFarSpeed;

                    // Controllo per lo sfondo vicino (Near)
                    if (backgroundNearPosition.Y >= GraphicsDevice.Viewport.Height)
                    {
                        backgroundNearPosition.Y = backgroundFarPosition.Y - backgroundNear.Height * scaleFactorNear;
                    }

                    // Controllo per lo sfondo lontano (Far)
                    if (backgroundFarPosition.Y >= GraphicsDevice.Viewport.Height)
                    {
                        backgroundFarPosition.Y = backgroundNearPosition.Y - backgroundFar.Height * scaleFactorFar;
                    }






                    break;






                case GameState.GameOver:
                    
                    //Implemento logica per gestire Game Over
                AddNewScore("Sans", score);
                MediaPlayer.Stop();
                if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    currentState = GameState.Playing;
                    ResetGame();
                    MediaPlayer.Play(mySong);  // Riprendi la canzone quando il gioco viene resettato
                }
                    break;

                case GameState.Menu:
                    // TODO: Menu logic here
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

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            switch (currentState)
            {
                case GameState.Playing:
                    // Inserisco logica per gioco in corso

                    // Sfondo lontano
                    // Per lo sfondo "Far"
                    float scaleXFar = (float)GraphicsDevice.Viewport.Width / backgroundFar.Width;
                    float scaleYFar = (float)GraphicsDevice.Viewport.Height / backgroundFar.Height;
                    float scaleFactorFar = Math.Max(scaleXFar, scaleYFar);

                    // Per lo sfondo "Near"
                    float scaleXNear = (float)GraphicsDevice.Viewport.Width / backgroundNear.Width;
                    float scaleYNear = (float)GraphicsDevice.Viewport.Height / backgroundNear.Height;
                    float scaleFactorNear = Math.Max(scaleXNear, scaleYNear);

                    if (backgroundFarPosition.Y < GraphicsDevice.Viewport.Height)
                    {
                        Vector2 position = new Vector2(0, backgroundFarPosition.Y);
                        _spriteBatch.Draw(backgroundFar, backgroundFarPosition, null, Color.White, 0, Vector2.Zero, scaleFactorFar, SpriteEffects.None, 0);
                    }

                    // Sfondo vicino
                    int textureHeightNear = backgroundNear.Height;
                    if (backgroundNearPosition.Y < GraphicsDevice.Viewport.Height)
                    {
                        Vector2 position = new Vector2(0, backgroundNearPosition.Y);
                        _spriteBatch.Draw(backgroundNear, backgroundNearPosition, null, Color.White, 0, Vector2.Zero, scaleFactorNear, SpriteEffects.None, 0);
                    }

                    // Disegna il giocatore
                    // Prendiamo le dimensioni della texture in considerazione della scala
                    Vector2 textureDimensions = new Vector2(mainCharacterTexture.Width * scale, mainCharacterTexture.Height * scale);

                    // Calcola la posizione in cui iniziare a disegnare la texture per assicurarsi che sia centrata sulla hitbox
                    Vector2 drawPosition = new Vector2(player.GetPosition().X - textureDimensions.X / 2, player.GetPosition().Y - textureDimensions.Y / 2);

                    _spriteBatch.Draw(mainCharacterTexture, drawPosition, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                    // Disegna i proiettili
                    foreach (var bullet in bullets)
                    {
                        _spriteBatch.Draw(bullet.Texture, new Rectangle((int)bullet.Position.X, (int)bullet.Position.Y, Bullet.Size, Bullet.Size), Color.Red);
                    }

                    // Disegna i nemici
                    foreach (var enemy in enemies)
                    {
                        _spriteBatch.Draw(enemy.Texture, new Rectangle((int)enemy.Position.X, (int)enemy.Position.Y, Enemy.Size, Enemy.Size), enemy.TintColor);
                        // Calcola la larghezza attuale della barra della salute basata sulla percentuale di salute rimasta
                        int currentHealthBarWidth = (int)((float)enemy.Health / enemy.MaxHealth * HEALTH_BAR_WIDTH);

                        // Posizione della barra della salute (centrata sull'asse X del nemico e posta sopra di lui)
                        Vector2 healthBarPosition = new Vector2(enemy.Position.X + (Enemy.Size - HEALTH_BAR_WIDTH) / 2, enemy.Position.Y - HEALTH_BAR_HEIGHT - 2);

                        // Disegna lo sfondo della barra della salute
                        _spriteBatch.Draw(whitePixel, new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y, HEALTH_BAR_WIDTH, HEALTH_BAR_HEIGHT), HEALTH_BAR_BG_COLOR);

                        // Disegna la barra della salute effettiva
                       _spriteBatch.Draw(whitePixel, new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y, currentHealthBarWidth, HEALTH_BAR_HEIGHT), HEALTH_BAR_COLOR);
                    }

                    // Disegna i proiettili dei nemici
                    foreach (var bullet in enemyBullets)
                    {
                        _spriteBatch.Draw(bullet.Texture, new Rectangle((int)bullet.Position.X, (int)bullet.Position.Y, Bullet.Size, Bullet.Size), Color.Yellow); // O qualsiasi altro colore tu voglia.
                    }
                      
                    //_spriteBatch.Draw(whitePixel, player.Hitbox, Color.Yellow); // Questo mostrerà l'hitbox in giallo
                    _spriteBatch.DrawString(scoreFont, $"Score: {score}", new Vector2(10, 10), Color.White);


                    // Disegna i power up sullo schermo nel metodo Draw:
                    foreach (var powerUp in powerUps)
                    {
                        powerUp.Draw(_spriteBatch);
                    }

                    // Disegno gli oggetti pickupo
                    foreach (var item in pickUpItems)
                    {
                        item.Draw(_spriteBatch);
                    }




                    break;

                case GameState.GameOver:
                    //Inserisco logica per gestire il Game Over
                    GraphicsDevice.Clear(Color.Black);
                    _spriteBatch.DrawString(gameOverFont, "YOU FAILED TO DESTROY MCDONALD EMPIRE", new Vector2(_graphics.PreferredBackBufferWidth / 2 - 150, _graphics.PreferredBackBufferHeight / 2 - 10), Color.Red);
                        DrawGameOverScreen(_spriteBatch);

                    break;
                case GameState.Menu:
                    // Draw the menu
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
            File.WriteAllText("scores.json", json);
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
    }

    }
