//Author: Dan Lichtin
//File Name: Game1.cs
//Project Name: PASS3
//Creation Date: December 2, 2022
//Modified Date: January 22, 2023
//Description: Sci Opolis is a single/co-op 2D wave based shooter;
//society has fallen and your player has found themselves trapped,
//your goal is to survive as long as possible, before meeting your impending demise 

//CONCEPTS;
//OOP - Player, all enemy classes, all gun classes
//2D Array's/List - Gun class (Bullets), File Manager (2D Tileset), Game1 (List of enemies/players)
//File I/O - File Manager (Reads/Saves multiple game components)
//Stacks & Queues - WaveQueue class, and EnemyStack class
//Binary Trees - Upgrade Tree/Upgrade Node
//Recursion - Upgrade Tree (Recursive tree scanner)

//////////////////////
//TOTAL - 6 CONCEPTS//
//////////////////////
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using Helper;
//HOW TO WIN ENDLESSLY; 
//1. Head to gun class and set reload timer to 10
//2. Then Set shooting timer to 50
//3. Head to player class and set health to 100 
//4. Done

namespace PASS3___Grade_12
{
    public class Game1 : Game
    {
        //Graphics & Sprite Batch
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Controller/KB
        GamePadState gamePad1;
        GamePadState gamePad2;
        MouseState mouse;
        MouseState prevMouse;
        KeyboardState kb;
        KeyboardState prevKb;

        //Game states
        const int MENU = 0;
        const int UPGRADE = 1;
        const int GAMEOVER = 4;
        const int STAT = 5;
        const int GAMEPLAY = 2;
        int gameState = MENU;

        //IDENTIFIERS 
        public const int IDLE = 0;
        public const int RUN_PLAYER = 1;
        public const int DEATH = 1;
        public const int SHOOTING = 1;
        public const int LONG_LEGS = 0;
        public const int GOOP = 2;
        public const int CYCLOPS = 5;
        public const int ATTACK = 2;
        public const int RUN = 0;
        public const int AK47 = 0;
        public const int SAWED_OFF = 2;
        public const int RIGHT = 1;
        public const int LEFT = 0;
        public const int PLAYER = 0;
        public const int ENEMY = 1;

        //Screen data
        //Width and Height should be mutiples of 32 (for the tile set of the game)
        //Stage layouts must be formatted according to the screen width and height
        //1152/32 means 36 tiles per row (each col in notes should be 36 * 2 units long, * 2 because of the comma delimiter)
        public static int screenWidth = 1216;
        public static int screenHeight = 704;
        //TILE DATA
        //Tile identifiers
        const int EMPTY = 0;
        const int UNDERGROUND = 1;
        const int BASE = 2;
        const int PLATFORM = 3;
        const int PIPE_REGULAR = 4;
        const int PIPE_TOP = 5;
        //Tile info
        const int TILE_IMG_LENGTH_AND_HEIGHT = 32;
        int[,] tileSet;
        //Tile recs & imgs
        Texture2D[] tileImgs = new Texture2D[6];
        Rectangle[,] tileSetRecs = new Rectangle[screenHeight / TILE_IMG_LENGTH_AND_HEIGHT, screenWidth / TILE_IMG_LENGTH_AND_HEIGHT];

        //ALL UI Data
        //UI IMAGES
        Texture2D menuBg;
        Texture2D healthImg;
        Texture2D leftClickPrompt;
        Texture2D reloadIcon;
        Texture2D coinImg; 
        //UI RECS/LOCS
        Rectangle bgRec;
        Vector2 leftClickPromptLoc;
        Rectangle reloadIconRec1;
        Rectangle reloadIconRec2;
        Rectangle coinRec;
        Vector2 coinTxtLoc;
        Rectangle sawedOffRec;
        Rectangle ak47Rec;
        //UI MENU OPTIONS
        bool singlePlayerHover = false;
        bool twoPlayerHover = false;
        bool upgradeHover = false;
        bool statHover = false;
        bool backHover = false;
        bool exitHover = false;

        //Gameplay Data
        Texture2D gameplayBg;
        static int coinsEarned = 0;
        static int enemiesKilled = 0;

        //Statistics data
        Texture2D statsBg; 

        //File Manager
        static FileManager fileManager = new FileManager();

        //Fonts
        SpriteFont menuFont;
        SpriteFont statFont;
        SpriteFont coinFont;
        SpriteFont upgradeFont;
        SpriteFont waveFont;
        
        //Cursor data
        Rectangle cursorRec;
        Texture2D cursorImg;

        //PLAYER
        //Player images 
        Texture2D[] playerImgs = new Texture2D[2];
        //Player objects
        Player player1;
        Player player2;
        List<Player> players = new List<Player>();
        //Player saved data
        static public string[,] playerData;
        double bestSurvivalTime;
        Timer survivalTimer;
        //Player saved data identifiers
        const int BEST_SURVIVAL_TIME = 0;
        const int COINS = 1;
        const int COINS_SPENT = 2;
        const int TIMES_LOGGED_IN = 3;
        const int WAVES_SURVIVED = 4;
        const int ENEMIES_KILLED = 5;
        //Text & number version of the data in the 2d array
        const int TEXT = 0;
        const int NUM = 1;
        //Amount of stats
        const int NUM_OF_STATS = 6;

        //Weapon data
        Texture2D[] gunImgs = new Texture2D[4];
        Texture2D bulletImg;
        //Weapon selection
        Color ak47Color = Color.White;
        Color sawedOffColor = Color.White;
        Gun ak47Player1;
        Gun ak47Player2;
        Gun sawedOffPlayer1;
        Gun sawedOffPlayer2;
        Gun selectedGun;
        Gun secondSelectedGun;

        //Enemy data
        Texture2D[] enemyImgs = new Texture2D[8];

        //Music & Sound Effects Data
        Song menuMusic;
        Song gameplayMusic;
        Song upgradeMusic;
        List<SoundEffect> soundSFX = new List<SoundEffect>();
        public static SFXManager sfxManager;

        //Testing data
        public static bool showCollisionRecs = false;

        ////////////////////////
        ////WAVE SYSTEM DATA////
        //////////////////////// 
        WaveQueue wave;
        List<Enemy> enemies = new List<Enemy>();
        int waveNum = 1;

        /////////////////////////
        ////UPGRADE TREE DATA////
        /////////////////////////
        //Amount of upgrades the player will have; constant
        const int UPGRADE_AMOUNT = 5;
        //Upgrade Background & Images
        Texture2D upgradeBg;
        Texture2D[] upgradeIcons = new Texture2D[UPGRADE_AMOUNT];
        //Upgrade image scaler
        float upgradeIconScaler = 1f;
        //Upgrade data
        string[] upgradeDescs = new string[UPGRADE_AMOUNT] { "Double Jump", "Speed Upgrade", 
            "Gun Upgrade", "Health Upgrade", "Sawed Off Unlock" };
        static int[] upgradeCosts = new int[UPGRADE_AMOUNT] { 1100, 1000, 1400, 800, 1250 };
        //Upgrade identifiers
        public const int DOUBLE_JUMP_UNLOCK = 0;
        public const int SPEED_UPGRADE = 1;
        public const int DAMAGE_UPGRADE = 2;
        public const int HEALTH_UPGRADE = 3;
        public const int SAWED_OFF_UNLOCK = 4;
        const int UPDATE = 0;
        const int DRAW = 1;
        //Upgrade save data
        static bool[] unlocked = fileManager.ReadUpgrades("Upgrades.txt", UPGRADE_AMOUNT);
        //Upgrade tree
        UpgradeTree upgradeTree;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary> 
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.graphics.PreferredBackBufferWidth = screenWidth;
            this.graphics.PreferredBackBufferHeight = screenHeight;

            //Turning off vsync
            graphics.PreferMultiSampling = true;
            graphics.SynchronizeWithVerticalRetrace = false;

            this.graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Loading background rectangle
            bgRec = new Rectangle(0, 0, screenWidth, screenHeight);

            //LOADING ALL BACKGROUNDS
            menuBg = Content.Load<Texture2D>("Images/Background/MenuBg");
            gameplayBg = Content.Load<Texture2D>("Images/Background/GameplayBg");
            upgradeBg = Content.Load<Texture2D>("Images/Background/UpgradeBg");
            statsBg = Content.Load<Texture2D>("Images/Background/StatsBg");

            //LOADING ALL UI
            cursorImg = Content.Load<Texture2D>("Images/Sprites/UI/Cursor");
            leftClickPrompt = Content.Load<Texture2D>("Images/Sprites/UI/LeftClick");
            reloadIcon = Content.Load<Texture2D>("Images/Sprites/UI/ReloadIcon");
            reloadIconRec1 = new Rectangle(5, 0 + (int)(reloadIcon.Width/3.55), reloadIcon.Width/3, reloadIcon.Height/3);
            reloadIconRec2 = new Rectangle((int)(screenWidth - (reloadIcon.Width/3 * 1.1)), 0 + (int)(reloadIcon.Width / 3.55), 
                reloadIcon.Width / 3, reloadIcon.Height / 3);
            leftClickPromptLoc = new Vector2(screenWidth - leftClickPrompt.Width, 0);
            cursorRec = new Rectangle(screenWidth / 3, screenHeight / 2, cursorImg.Width / 2, cursorImg.Height / 2);
            coinImg = Content.Load<Texture2D>("Images/Sprites/UI/Coin");
            coinRec = new Rectangle(5, 5, coinImg.Width / 3, coinImg.Height / 3);
            coinTxtLoc = new Vector2((int)(coinImg.Width/2.7), 6);

            //LOADING ALL FONTS
            menuFont = Content.Load<SpriteFont>("Fonts/MenuFont");
            statFont = Content.Load<SpriteFont>("Fonts/StatFont");
            coinFont = Content.Load<SpriteFont>("Fonts/CoinFont");
            upgradeFont = Content.Load<SpriteFont>("Fonts/UpgradeFont");
            waveFont = Content.Load<SpriteFont>("Fonts/WaveFont");

            //LOADING ALL PLAYER IMGS/UI
            playerImgs[IDLE] = Content.Load<Texture2D>("Images/Sprites/Player/Player1/idle");
            playerImgs[RUN_PLAYER] = Content.Load<Texture2D>("Images/Sprites/Player/Player1/run");
            healthImg = Content.Load<Texture2D>("Images/Sprites/UI/HealthImg");

            //LOADING ALL ENEMIES
            enemyImgs[LONG_LEGS + RUN] = Content.Load<Texture2D>("Images/Sprites/Enemies/LongLegs/LongLegWalk");
            enemyImgs[LONG_LEGS + DEATH] = Content.Load<Texture2D>("Images/Sprites/Enemies/LongLegs/LongLegDeath");
            enemyImgs[GOOP + RUN] = Content.Load<Texture2D>("Images/Sprites/Enemies/Goop/GoopVulnerable");
            enemyImgs[GOOP + DEATH] = Content.Load<Texture2D>("Images/Sprites/Enemies/Goop/GoopDeath");
            enemyImgs[GOOP + ATTACK] = Content.Load<Texture2D>("Images/Sprites/Enemies/Goop/GoopAttack");
            enemyImgs[CYCLOPS + RUN] = Content.Load<Texture2D>("Images/Sprites/Enemies/Cyclops/CyclopsRun");
            enemyImgs[CYCLOPS + DEATH] = Content.Load<Texture2D>("Images/Sprites/Enemies/Cyclops/CyclopsDeath");
            enemyImgs[CYCLOPS + ATTACK] = Content.Load<Texture2D>("Images/Sprites/Enemies/Cyclops/CyclopsAttack");

            //LOADING ALL GUNS & AMMO
            gunImgs[AK47 + IDLE] = Content.Load<Texture2D>("Images/Sprites/Weapons/AK47_IDLE");
            gunImgs[AK47 + SHOOTING] = Content.Load<Texture2D>("Images/Sprites/Weapons/AK47_SHOOTING");
            gunImgs[SAWED_OFF + IDLE] = Content.Load<Texture2D>("Images/Sprites/Weapons/SAWEDOFF_IDLE");
            gunImgs[SAWED_OFF + SHOOTING] = Content.Load<Texture2D>("Images/Sprites/Weapons/SAWEDOFF_SHOOTING");
            bulletImg = Content.Load<Texture2D>("Images/Sprites/Weapons/Bullet");
            ak47Rec = new Rectangle((int)(coinTxtLoc.X), (int)(coinTxtLoc.Y * 12.5), gunImgs[AK47 + IDLE].Width, gunImgs[AK47 + IDLE].Height);
            sawedOffRec = new Rectangle((int)(ak47Rec.X + gunImgs[SAWED_OFF + IDLE].Width * 1.1), ak47Rec.Y, 
                gunImgs[SAWED_OFF + IDLE].Width, gunImgs[SAWED_OFF + IDLE].Height);

            //LOADING ALL TILES
            tileImgs[UNDERGROUND] = Content.Load<Texture2D>("Images/Sprites/Tiles/UndergroundTile");
            tileImgs[BASE] = Content.Load<Texture2D>("Images/Sprites/Tiles/BaseFloor");
            tileImgs[PLATFORM] = Content.Load<Texture2D>("Images/Sprites/Tiles/RegularTile");
            tileImgs[PIPE_REGULAR] = Content.Load<Texture2D>("Images/Sprites/Tiles/PipeRegular");
            tileImgs[PIPE_TOP] = Content.Load<Texture2D>("Images/Sprites/Tiles/PipeRegular");

            //LOADING THE UPGRADE SECTION
            //Upgrade images
            upgradeIcons[DOUBLE_JUMP_UNLOCK] = Content.Load<Texture2D>("Images/Sprites/Upgrades/DoubleJump");
            upgradeIcons[SAWED_OFF_UNLOCK] = Content.Load<Texture2D>("Images/Sprites/Upgrades/SawedOff");
            upgradeIcons[HEALTH_UPGRADE] = Content.Load<Texture2D>("Images/Sprites/Upgrades/Health");
            upgradeIcons[DAMAGE_UPGRADE] = Content.Load<Texture2D>("Images/Sprites/Upgrades/Damage");
            upgradeIcons[SPEED_UPGRADE] = Content.Load<Texture2D>("Images/Sprites/Upgrades/Speed");
            //Upgrade tree
            upgradeTree = new UpgradeTree(upgradeFont);
            //Upgrade nodes
            upgradeTree.AddUpgradeNode(GraphicsDevice, upgradeIcons[SPEED_UPGRADE], upgradeCosts[SPEED_UPGRADE], 
                upgradeIconScaler, upgradeDescs[SPEED_UPGRADE], unlocked[SPEED_UPGRADE]);
            upgradeTree.AddUpgradeNode(GraphicsDevice, upgradeIcons[SAWED_OFF_UNLOCK], upgradeCosts[SAWED_OFF_UNLOCK], 
                upgradeIconScaler, upgradeDescs[SAWED_OFF_UNLOCK], unlocked[SAWED_OFF_UNLOCK]);
            upgradeTree.AddUpgradeNode(GraphicsDevice, upgradeIcons[DOUBLE_JUMP_UNLOCK], upgradeCosts[DOUBLE_JUMP_UNLOCK], 
                upgradeIconScaler, upgradeDescs[DOUBLE_JUMP_UNLOCK], unlocked[DOUBLE_JUMP_UNLOCK]);
            upgradeTree.AddUpgradeNode(GraphicsDevice, upgradeIcons[DAMAGE_UPGRADE], upgradeCosts[DAMAGE_UPGRADE], 
                upgradeIconScaler, upgradeDescs[DAMAGE_UPGRADE], unlocked[DAMAGE_UPGRADE]);
            upgradeTree.AddUpgradeNode(GraphicsDevice, upgradeIcons[HEALTH_UPGRADE], upgradeCosts[HEALTH_UPGRADE], 
                upgradeIconScaler, upgradeDescs[HEALTH_UPGRADE], unlocked[HEALTH_UPGRADE]);

            //LOADING ALL MUSIC & SOUND EFFECTS
            //Loading music
            gameplayMusic = Content.Load<Song>("Audio/Music/GameplayMusic");
            upgradeMusic = Content.Load<Song>("Audio/Music/UpgradeMusic");
            menuMusic = Content.Load<Song>("Audio/Music/MenuMusic");
            //Loading sound effects
            soundSFX.Add(Content.Load<SoundEffect>("Audio/Sounds/Click"));
            soundSFX.Add(Content.Load<SoundEffect>("Audio/Sounds/Error"));
            soundSFX.Add(Content.Load<SoundEffect>("Audio/Sounds/Purchase"));
            soundSFX.Add(Content.Load<SoundEffect>("Audio/Sounds/Walking"));
            soundSFX.Add(Content.Load<SoundEffect>("Audio/Sounds/Shooting"));
            soundSFX.Add(Content.Load<SoundEffect>("Audio/Sounds/Reload"));
            soundSFX.Add(Content.Load<SoundEffect>("Audio/Sounds/Death"));
            soundSFX.Add(Content.Load<SoundEffect>("Audio/Sounds/Wave"));
            soundSFX.Add(Content.Load<SoundEffect>("Audio/Sounds/Hit"));
            soundSFX.Add(Content.Load<SoundEffect>("Audio/Sounds/Jump"));
            //Sfx & music settings
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.25f;
            SoundEffect.MasterVolume = 1f;
            //Playing the menu music on launch
            MediaPlayer.Play(menuMusic);
            //Creating the sfx manager
            sfxManager = new SFXManager(soundSFX);

            //Reading in the tile set for the stage
            tileSet = fileManager.ReadStageLayout("Stage.txt", screenWidth / TILE_IMG_LENGTH_AND_HEIGHT, screenHeight / TILE_IMG_LENGTH_AND_HEIGHT);

            //Reading in saved player data (second parameter is how many stats there are)
            playerData = fileManager.ReadStatsAndInventory("Statistics.txt", NUM_OF_STATS);

            //Creating the rectangles for the tile set
            for (int i = 0; i < tileSet.GetLength(0); i++)
            {
                for (int j = 0; j < tileSet.GetLength(1); j++)
                {
                    //Access this statement if the tile isn't empty (meaning val of 0)
                    if (tileSet[i,j] != 0)
                    {
                        //Creating the rectangle for the tile
                        tileSetRecs[i, j] = new Rectangle(tileImgs[UNDERGROUND].Width * j, tileImgs[UNDERGROUND].Height * i, 
                            TILE_IMG_LENGTH_AND_HEIGHT, TILE_IMG_LENGTH_AND_HEIGHT);
                    }
                }

            }

            //Loading the weapons
            ak47Player1 = new AK47(GraphicsDevice, gunImgs, bulletImg, reloadIcon, reloadIconRec1, null, null, PLAYER, unlocked[DAMAGE_UPGRADE]);
            ak47Player2 = new AK47(GraphicsDevice, gunImgs, bulletImg, reloadIcon, reloadIconRec2, null, null, PLAYER, unlocked[DAMAGE_UPGRADE]);
            sawedOffPlayer1 = new SawedOff(GraphicsDevice, gunImgs, bulletImg, reloadIcon, reloadIconRec1, null, null, PLAYER, unlocked[DAMAGE_UPGRADE]);
            sawedOffPlayer2 = new SawedOff(GraphicsDevice, gunImgs, bulletImg, reloadIcon, reloadIconRec2, null, null, PLAYER, unlocked[DAMAGE_UPGRADE]);
            //Default gun is ak
            selectedGun = ak47Player1;
            secondSelectedGun = ak47Player2;

            //Loading timers
            survivalTimer = new Timer(Timer.INFINITE_TIMER, false);
            bestSurvivalTime = Convert.ToDouble(playerData[BEST_SURVIVAL_TIME, NUM]);

            //+1 times logged in
            TimesLoggedIn();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //Updating game pad & kb
            gamePad1 = GamePad.GetState(PlayerIndex.One);
            gamePad2 = GamePad.GetState(PlayerIndex.Two);
            prevMouse = mouse;
            mouse = Mouse.GetState();
            prevKb = kb;
            kb = Keyboard.GetState();

            //Update gamestate 
            switch (gameState)
            {
                case MENU:
                    UpdateMenu();
                    break;
                case UPGRADE:
                    UpdateUpgrade();
                    break;
                case GAMEPLAY:
                    UpdateGameplay(gameTime);
                    break;
                case STAT:
                    UpdateStats();
                    break;
                case GAMEOVER:
                    UpdateGameOver();
                    break;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            switch (gameState)
            {
                case MENU:
                    DrawMenu();
                    break;
                case UPGRADE:
                    DrawUpgrade();
                    break;
                case GAMEPLAY:
                    DrawGameplay();
                    break;
                case STAT:
                    DrawStats();
                    break;
                case GAMEOVER:
                    DrawGameOver();
                    break;
            }
            spriteBatch.End();

            base.Draw(gameTime);

        }

        //Pre: None
        //Post: None
        //Desc: Handles the logic of the input, cursor, and set's the relevant data for the next playthrough
        private void UpdateMenu()
        {      
            //Updating the cursor
            cursorRec.X = mouse.X;
            cursorRec.Y = mouse.Y;

            //Access this statement if the mouse is clicked
            if ((mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released))
            {
                //Checking to see if any options are being hovered over
                if (singlePlayerHover)
                {
                    //Resetting the gun and creating a new player
                    selectedGun.ResetGun();
                    players = new List<Player>();
                    player1 = new Player(playerImgs, selectedGun, GraphicsDevice, healthImg, Color.White, unlocked);
                    players.Add(player1);

                    //Loading the wave
                    wave = new WaveQueue(enemyImgs, GraphicsDevice,
                        new AK47(GraphicsDevice, gunImgs, bulletImg, reloadIcon, reloadIconRec1, null, players, ENEMY, false), waveNum);

                    //Resetting & Activating survival timer
                    survivalTimer.ResetTimer(true);

                    //SFX & Music
                    MediaPlayer.Play(gameplayMusic);
                    sfxManager.PlayClick();

                    //Gamestate change
                    gameState = GAMEPLAY;
                }
                else if (twoPlayerHover)
                {
                    //Resetting the guns and creating two new players
                    selectedGun.ResetGun();
                    secondSelectedGun.ResetGun();
                    players = new List<Player>();
                    player1 = new Player(playerImgs, selectedGun, GraphicsDevice, healthImg, Color.White, unlocked);
                    player2 = new Player(playerImgs, secondSelectedGun, GraphicsDevice, healthImg, Color.Turquoise, unlocked);
                    players.Add(player1);
                    players.Add(player2);

                    //Loading the wave
                    wave = new WaveQueue(enemyImgs, GraphicsDevice,
                        new AK47(GraphicsDevice, gunImgs, bulletImg, reloadIcon, reloadIconRec1, null, players, ENEMY, false), waveNum);

                    //Resetting & Activating survival timer
                    survivalTimer.ResetTimer(true);

                    //SFX & Music
                    MediaPlayer.Play(gameplayMusic);
                    sfxManager.PlayClick();

                    //Gamestate change
                    gameState = GAMEPLAY;
                }
                else if (upgradeHover)
                {
                    //SFX & Music
                    sfxManager.PlayClick();
                    MediaPlayer.Play(upgradeMusic);

                    //Gamestate change
                    gameState = UPGRADE;
                }
                else if (statHover)
                {
                    //SFX & Music
                    sfxManager.PlayClick();
                    MediaPlayer.Play(upgradeMusic);

                    //Gamestate change
                    gameState = STAT;
                }
                else if (ak47Rec.Intersects(cursorRec))
                {
                    //SFX & Music
                    sfxManager.PlayClick();

                    //Sawpping guns
                    selectedGun = ak47Player1;
                    secondSelectedGun = ak47Player2;
                }
                else if (sawedOffRec.Intersects(cursorRec))
                {
                    if (unlocked[SAWED_OFF_UNLOCK])
                    {
                        //SFX 
                        sfxManager.PlayClick();

                        //Swapping guns
                        selectedGun = sawedOffPlayer1;
                        secondSelectedGun = sawedOffPlayer2;
                    }
                    else
                    {
                        //SFX
                        sfxManager.PlayError();
                    }
                }
                else if (exitHover)
                {
                    //Quit game
                    Exit();
                }
            }

            //Color coding the selected gun based on it's type
            switch (selectedGun.GetGunType())
            {
                case "Assault Rifle":
                    //Set other gun to transparent
                    ak47Color = Color.White;
                    sawedOffColor = Color.White * 0.5f;
                    break;
                case "Shotgun":
                    //Access this statement if the shotgun has been unlocked
                    if (unlocked[SAWED_OFF_UNLOCK])
                    {
                        //Set other gun to transparent
                        sawedOffColor = Color.White;
                        ak47Color = Color.White * 0.5f;
                    }
                    break;
            }
        }

        //Pre: None
        //Post: None
        //Desc: Draws all the menu options, background, weapon selection, currency, etc.
        private void DrawMenu()
        {
            //Drawing the background
            spriteBatch.Draw(menuBg, bgRec, Color.White);

            //Drawing the menu options & title
            TextDrawer(menuFont, "Sci Opolis", screenHeight / 12, false, Color.SlateBlue, Color.White);
            singlePlayerHover = TextDrawer(menuFont, "Single Player", (int)(screenHeight / 4.5), true, Color.Black, Color.LightPink);
            twoPlayerHover = TextDrawer(menuFont, "Two Player", (int)(screenHeight / 2.85), true, Color.Black, Color.LightPink);
            upgradeHover = TextDrawer(menuFont, "Upgrades", (int)(screenHeight / 2.1), true, Color.Black, Color.LightPink);
            statHover = TextDrawer(menuFont, "Stats", (int)(screenHeight / 1.675), true, Color.Black, Color.LightPink);
            exitHover = TextDrawer(menuFont, "Exit", (int)(screenHeight / 1.4), true, Color.Black, Color.DarkRed);

            //Drawing the sawed off with a color according to it's lock status
            if (unlocked[SAWED_OFF_UNLOCK])
            {
                spriteBatch.Draw(gunImgs[SAWED_OFF + IDLE], sawedOffRec, sawedOffColor);
            }
            else
            {
                spriteBatch.Draw(gunImgs[SAWED_OFF + IDLE], sawedOffRec, Color.Black);
            }

            //Drawing the ak
            spriteBatch.Draw(gunImgs[AK47 + IDLE], ak47Rec, ak47Color);

            //Drawing the coin image and currrency
            spriteBatch.Draw(coinImg, coinRec, Color.White);
            spriteBatch.DrawString(coinFont, playerData[COINS, NUM], coinTxtLoc, Color.Gold);

            //Drawing the cursor
            spriteBatch.Draw(cursorImg, cursorRec, Color.White);
        }

        //Pre: None
        //Post: None
        //Desc: Updates all gameplay logic, timers, and input detection & handles the relevant outer gameplay logic (stat tracking, currency addition, etc.)
        private void UpdateGameplay(GameTime gameTime)
        {
            //Looping through all enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                //Updating the enemy
                enemies[i].EnemyLogic(gameTime, tileSetRecs, players);

                //Access the statement if enemy is dead
                if (enemies[i].isDead())
                {
                    //Adding a different amount of coins after enemy death depending on enemy type
                    switch (enemies[i].GetEnemyType())
                    {
                        case LONG_LEGS:
                            coinsEarned += 21;
                            break;
                        case CYCLOPS:
                            coinsEarned += 29;
                            break;
                        case GOOP:
                            coinsEarned += 37;
                            break;
                    }

                    //Adding +1 to the amount of enemies killed
                    enemiesKilled++;

                    //Removing the enemy
                    enemies.RemoveAt(i);
                    i--;
                }
            }

            //Access this statement if player 1 is dead
            if (player1.IsDead())
            {
                //Removing player
                players.Remove(player1);
            }

            //Access this statement if there is a player 2 and if they're dead
            if (player2 != null && player2.IsDead())
            {
                //Removing player
                players.Remove(player2);
            }
            
            //Access this statement if there are 0 players left
            if (players.Count == 0)
            {
                //Saving the stats after player death
                SaveGame();

                //Changing gamestate & playing music
                MediaPlayer.Play(menuMusic);
                gameState = GAMEOVER;
            }

            //Adding enemies to Game1 enemy list and setting them in ALL players
            enemies.AddRange(wave.UpdateWaveQueue(gameTime));
            for (int i = 0; i < players.Count; i++)
            {
                players[i].SetEnemies(enemies);
            }

            //Checking if wave drops are over and all enemies were killed
            if (wave.IsWaveFinished() && enemies.Count == 0)
            {
                //SFX
                sfxManager.PlayWave();

                //New wave & +1 to wav num
                waveNum++;
                wave = new WaveQueue(enemyImgs, GraphicsDevice, new AK47(GraphicsDevice, gunImgs, bulletImg, reloadIcon, reloadIconRec1, enemies, players, ENEMY, false), waveNum);
            }

            //Check for test input
            //Colours the collision boxes of ALL entities (player, enemy, bullet, etc.)
            if (kb.IsKeyDown(Keys.D1) && !prevKb.IsKeyDown(Keys.D1))
            {
                showCollisionRecs = !showCollisionRecs;
            }

            //Updating logic of both players
            player1.PlayerLogic(gamePad1, kb, prevKb, gameTime, tileSetRecs, PlayerIndex.One);
            if (player2 != null)
            {
                player2.PlayerLogic(gamePad2, prevKb, kb, gameTime, tileSetRecs, PlayerIndex.Two);
            }

            //Updating all timers
            survivalTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
        }

        //Pre: None
        //Post: None
        //Desc: Draws all players (which draw their respective data/objects), enemies, tileset, background, and UI
        private void DrawGameplay()
        {
            //Background 
            spriteBatch.Draw(gameplayBg, bgRec, Color.White);

            //Drawing the tileset by looping through the 2D tileset array
            for (int i = 0; i < tileSet.GetLength(0); i++)
            {
                for (int j = 0; j < tileSet.GetLength(1); j++)
                {
                    //Draws a different tile depending on the value of the index that the 2D array has
                    switch (tileSet[i, j])
                    {
                        case EMPTY:
                            break;
                        case UNDERGROUND:
                            spriteBatch.Draw(tileImgs[UNDERGROUND], tileSetRecs[i, j], Color.White);
                            break;
                        case BASE:
                            spriteBatch.Draw(tileImgs[BASE], tileSetRecs[i, j], Color.White);
                            break;
                        case PLATFORM:
                            spriteBatch.Draw(tileImgs[PLATFORM], tileSetRecs[i, j], Color.White);
                            break;
                        case PIPE_REGULAR:
                            spriteBatch.Draw(tileImgs[PIPE_REGULAR], tileSetRecs[i, j], Color.White);
                            break;
                        case PIPE_TOP:
                            spriteBatch.Draw(tileImgs[PIPE_TOP], tileSetRecs[i, j], Color.White);
                            break;

                    }
                }
            }

            //Drawing ALL enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                //Drawing the enemy with a different direction depending on enemy type
                if (enemies[i].GetEnemyType() == GOOP)
                {
                    enemies[i].DrawEnemy(spriteBatch, LEFT);
                }
                else
                {
                    enemies[i].DrawEnemy(spriteBatch, RIGHT);
                }
            }

            //Drawing all players
            for (int i = 0; i < players.Count; i++)
            {
                players[i].DrawPlayer(spriteBatch);
            }

            //Drawing the wave number
            TextDrawer(waveFont, "Wave Number: " + waveNum, 0, false, Color.White, Color.White);
        }

        //Pre: None
        //Post: None
        //Desc: Updates input and cursor logic, also resets objects and playthrough stats
        private void UpdateGameOver()
        {
            //Updating the cursor
            cursorRec.X = mouse.X;
            cursorRec.Y = mouse.Y;

            //Access when the mouse is clicked
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                //Access if user is hovering over the back button
                if (backHover)
                {
                    //RESETTING RELEVANT INFO FOR NEXT PLAYTHROUGH
                    player1 = null;
                    player2 = null;
                    coinsEarned = 0;
                    enemiesKilled = 0;
                    enemies = new List<Enemy>();
                    waveNum = 1;

                    //Playing sfx & changing gamestate
                    sfxManager.PlayClick();
                    gameState = MENU;
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Draws playthrough stats, background, and cursor
        private void DrawGameOver()
        {
            //Drawing the background
            spriteBatch.Draw(menuBg, bgRec, Color.White);

            //Drawing the back prompt
            backHover = TextDrawer(menuFont, "Back", (int)(screenHeight / 1.5), true, Color.Black, Color.White);

            //Drawing the playthrough statistics
            TextDrawer(menuFont, "Coins Earned: " + coinsEarned, (int)(screenHeight / 3.25), false, Color.Gold, Color.White);
            TextDrawer(menuFont, "Enemies Killed: " + enemiesKilled, (int)(screenHeight / 2.5), false, Color.Red, Color.White);
            TextDrawer(menuFont, "Waves Survived: " + (waveNum - 1), (int)(screenHeight / 2), false, Color.Blue, Color.White);

            //Drawing the cursor
            spriteBatch.Draw(cursorImg, cursorRec, Color.White);
        }

        //Pre: None
        //Post: None
        //Desc: Updates input and cursor logic
        private void UpdateStats()
        {
            //Updating the cursor
            cursorRec.X = mouse.X;
            cursorRec.Y = mouse.Y;

            //Access when the mouse is clicked
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                //Access if user is hovering over the back button
                if (backHover)
                {
                    //Playing sfx & music 
                    sfxManager.PlayClick();
                    MediaPlayer.Play(menuMusic);

                    //Changing gamestate
                    gameState = MENU;
                }
            }           
        }

        //Pre: None
        //Post: None
        //Desc: Draws stats, background, and cursor
        private void DrawStats()
        {
            //Drawing the background
            spriteBatch.Draw(statsBg, bgRec, Color.White);

            //Drawing the title
            TextDrawer(menuFont, "Stats", screenHeight / 12, false, Color.LightGreen, Color.White);

            //Drawing the statistics
            TextDrawer(statFont, playerData[BEST_SURVIVAL_TIME, TEXT] + playerData[BEST_SURVIVAL_TIME, NUM], screenHeight / 5, false, Color.White, Color.White);
            TextDrawer(statFont, playerData[COINS_SPENT, TEXT] + playerData[COINS_SPENT, NUM], (int)(screenHeight / 3.25), false, Color.White, Color.White);
            TextDrawer(statFont, playerData[WAVES_SURVIVED, TEXT] + playerData[WAVES_SURVIVED, NUM], (int)(screenHeight / 2.5), false, Color.White, Color.White);
            TextDrawer(statFont, playerData[TIMES_LOGGED_IN, TEXT] + playerData[TIMES_LOGGED_IN, NUM], (int)(screenHeight / 2), false, Color.White, Color.White);
            TextDrawer(statFont, playerData[ENEMIES_KILLED, TEXT] + playerData[ENEMIES_KILLED, NUM], (int)(screenHeight / 1.675), false, Color.White, Color.White);

            //Draw the back prompt
            backHover = TextDrawer(menuFont, "Back", (int)(screenHeight / 1.4), true, Color.LightGreen, Color.DarkRed);

            //Drawing the cursor
            spriteBatch.Draw(cursorImg, cursorRec, Color.White);
        }

        //Pre: None
        //Post: None
        //Desc: Updates input and upgrade tree logic, resets the gun incase of the upgrades affecting them
        private void UpdateUpgrade()
        {
            //Updating the cursor
            cursorRec.X = mouse.X;
            cursorRec.Y = mouse.Y;

            //Access when the mouse is clicked
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                //Access if user is hovering over the back button
                if (backHover)
                {
                    //Playing sfx & music
                    sfxManager.PlayClick();
                    MediaPlayer.Play(menuMusic);
                    
                    //Redefining guns
                    ak47Player1 = new AK47(GraphicsDevice, gunImgs, bulletImg, reloadIcon, reloadIconRec1, null, null, PLAYER, unlocked[DAMAGE_UPGRADE]);
                    ak47Player2 = new AK47(GraphicsDevice, gunImgs, bulletImg, reloadIcon, reloadIconRec2, null, null, PLAYER, unlocked[DAMAGE_UPGRADE]);
                    sawedOffPlayer1 = new SawedOff(GraphicsDevice, gunImgs, bulletImg, reloadIcon, reloadIconRec1, null, null, PLAYER, unlocked[DAMAGE_UPGRADE]);
                    sawedOffPlayer2 = new SawedOff(GraphicsDevice, gunImgs, bulletImg, reloadIcon, reloadIconRec2, null, null, PLAYER, unlocked[DAMAGE_UPGRADE]);

                    //Changing gamestate
                    gameState = MENU;
                }
            }

            //Updating the upgrade tree
            upgradeTree.DrawUpdateTree(spriteBatch, UPDATE, mouse.Position, mouse.LeftButton);

        }

        //Pre: None
        //Post: None
        //Desc: Draws the background, upgrade tree, currency, and cursor
        private void DrawUpgrade()
        {
            //Drawing the background
            spriteBatch.Draw(upgradeBg, bgRec, Color.White);

            //Drawing the back prompt
            backHover = TextDrawer(menuFont, "Back", (int)(screenHeight / 1.25), true, Color.Black, Color.White);

            //Drawing the upgrade tree
            upgradeTree.DrawUpdateTree(spriteBatch, DRAW, mouse.Position, mouse.LeftButton);

            //Drawing the cursor
            spriteBatch.Draw(cursorImg, cursorRec, Color.White);

            //Drawing the coin image and currrency
            spriteBatch.Draw(coinImg, coinRec, Color.White);
            spriteBatch.DrawString(coinFont, playerData[COINS, NUM], coinTxtLoc, Color.Gold);
        }

        //Pre: A sprite font, the desired text, and the y coordinates that the text will be drawn at,
        //Wether or not the text should be detectable, and what color the text should be
        //Post: Returns a bool stating wether or not the text rectanlge contains mouse position (True if it is, False if it isn't)
        //Desc: Draws specified text at the y coordinates, and middles the text automatically (x);
        //Also returns a bool describing wether or not the text is being hovered over
        public bool TextDrawer(SpriteFont font, string txt, int y, bool detectable, Color color, Color hoverColor)
        {
            //Creating text data (locs, recs, etc.)
            Vector2 textSize = font.MeasureString(txt);
            int x = screenWidth / 2 - (int)textSize.X / 2;
            Vector2 textLoc = new Vector2(x, y);
            Rectangle textRec = new Rectangle(x, y, (int)textSize.X, (int)textSize.Y);

            //Drawing the text
            spriteBatch.DrawString(font, txt, textLoc, color);

            //If the text is supposed to be detectable
            if (detectable)
            {
                //If the text rectangle contains the mouse position
                if (textRec.Contains(mouse.Position))
                {
                    //Drawing the text with a different color, and drawing another image showing it can be clicked
                    spriteBatch.DrawString(font, txt, textLoc, hoverColor);
                    spriteBatch.Draw(leftClickPrompt, leftClickPromptLoc, Color.White);

                    //Return that text is being hovered over
                    return true;
                }
            }

            //Return that text isn't being hovered over (or is undetectable)
            return false;
        }

        //Pre: int amount of coins to be deducted
        //Post: None
        //Desc: Reduces the number of coins the player has and the number of coins the player has spent; then saves
        public static void CoinDeduction(int amount)
        {
            playerData[COINS, NUM] = Convert.ToString(Convert.ToInt32(playerData[COINS, NUM]) - amount);
            playerData[COINS_SPENT, NUM] = Convert.ToString(Convert.ToInt32(playerData[COINS_SPENT, NUM]) + amount);

            //Saving
            SaveStatsAndInventory();
        }

        //Pre: None
        //Post: None
        //Desc: Adds +1 to the amount of times the user has logged in; then saves
        public static void TimesLoggedIn()
        {
            playerData[TIMES_LOGGED_IN, NUM] = Convert.ToString(Convert.ToInt32(playerData[TIMES_LOGGED_IN, NUM]) + 1);

            //Saving
            SaveStatsAndInventory();
        }

        //Pre: None
        //Post: None
        //Desc: Calls to the file manager and saves all stats based off the player data array
        public static void SaveStatsAndInventory()
        {
            //Saving
            fileManager.SaveStatsAndInventory("Statistics.txt", playerData);
        }

        //Pre: int amount of coins to be added 
        //Post: None
        //Desc: Add's a specified amount of coins to the players currency; then saves
        public static void CoinAddition(int amount)
        {
            playerData[COINS, NUM] = Convert.ToString(Convert.ToInt32(playerData[COINS, NUM]) + amount);

            //Saving
            SaveStatsAndInventory();
        }

        //Pre: int amount of enemies killed
        //Post: None
        //Desc: Add's a specified amount of killed enemies to the players stats; then saves
        public static void EnemiesKilledAddition(int amount)
        {
            playerData[ENEMIES_KILLED, NUM] = Convert.ToString(Convert.ToInt32(playerData[ENEMIES_KILLED, NUM]) + amount);

            //Saving
            SaveStatsAndInventory();
        }

        //Pre: int amount of waves survived
        //Post: None
        //Desc: Add's a specified amount of waves survived to the players stats; then saves
        public static void WavesSurvivedAddition(int amount)
        {
            playerData[WAVES_SURVIVED, NUM] = Convert.ToString(Convert.ToInt32(playerData[WAVES_SURVIVED, NUM]) + amount);

            //Saving
            SaveStatsAndInventory();
        }

        //Pre: Timer of a playthrough, and a double variable of the best survival time
        //Post: None
        //Desc: Compares the passed in survival timer to the best survival time; if the timer is longer then the best survival time, then;
        //save and set best time to the survival timer
        public static void BestSurvivalTime(Timer survivalTimer, double bestSurvivalTime)
        {
            //Access this statement if the survival timer is a greater number then the best survival time
            if (Convert.ToDouble(survivalTimer.GetTimePassedAsString(Timer.FORMAT_SEC_MIL)) > bestSurvivalTime)
            {
                playerData[BEST_SURVIVAL_TIME, NUM] = survivalTimer.GetTimePassedAsString(Timer.FORMAT_SEC_MIL);

                //Setting the new best survival time
                bestSurvivalTime = Convert.ToDouble(survivalTimer.GetTimePassed());

                //Saving
                SaveStatsAndInventory();
            }
        }

        //Pre: None
        //Post: None
        //Desc: Saves all the statistics/currency that come from a playthrough (coins gained, enemies killed, waves survived, and a new survival time)
        public void SaveGame()
        {
            CoinAddition(coinsEarned);
            EnemiesKilledAddition(enemiesKilled);
            WavesSurvivedAddition(waveNum - 1);
            BestSurvivalTime(survivalTimer, bestSurvivalTime);
        }

        //Pre: int cost of upgrade
        //Post: None
        //Desc: Matches, saves, and unlocks the upgrade depending on the passed in cost
        public static void SaveUpgrades(int cost)
        {
            //Looping through the upgrade costs
            for (int i = 0; i < upgradeCosts.Length; i++)
            {
                //Access this statement if the cost matches the current looped upgrade cost
                if (cost == upgradeCosts[i])
                {
                    //Unlocking and saving the upgrade
                    unlocked[i] = true;
                    fileManager.SaveUpgrades("Upgrades.txt", unlocked);

                    break;
                }
            }
        }
    }
}
