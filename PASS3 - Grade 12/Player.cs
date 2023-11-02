//Author: Dan Lichtin
//File Name: Player.cs
//Project Name: PASS3
//Creation Date: December 2, 2022
//Modified Date: January 22, 2023
//Description: This class will handle all the player's data and movement and drawing
using Animation2D;
using Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
//PROOF OF CONCEPT:
//OOP - The player class is the blueprint of the player objects;
//it holds functions, player data, and the phyisics logic that a player requires

namespace PASS3___Grade_12
{
    class Player
    {
        //Player data
        private Animation[] playerAnims;
        private Rectangle playerRec = new Rectangle(0, 0, 35, 500);
        private Rectangle[] playerRecs = new Rectangle[4];
        private GameRectangle[] playerVisibleRecs = new GameRectangle[4];
        private Vector2 playerLoc;
        private Color playerColor;
        private float playerOpacity;
        private int playerHealth = 5;
        private Vector2 playerSpeed = new Vector2(0, 0);
        private bool isPlayerGrounded = true;
        private bool doubleJump = false;
        private Timer invincibilityTimer = new Timer(2000, false);
        private SpriteEffects flipType;

        //Player upgrade data
        const int DOUBLE_JUMP_UNLOCK = Game1.DOUBLE_JUMP_UNLOCK;
        const int SPEED_UPGRADE = Game1.SPEED_UPGRADE;
        const int HEALTH_UPGRADE = Game1.HEALTH_UPGRADE;
        private bool[] unlocked;

        //Weapon 
        private Gun playerGun;

        //Enemy data
        private List<Enemy> enemies; 

        //Physics data
        private const float GRAVITY = 9.8f / 30;
        private const float ACCELERATION = 0.75f;
        private const float FRICTION = ACCELERATION * 0.55f;
        private const float TOLERANCE = FRICTION * 0.5f;
        private float maxSpeed = 5f;
        private float jumpSpeed = -8.5f;
        private Vector2 forces = new Vector2(FRICTION, GRAVITY);

        //Graphics Device
        private GraphicsDevice gd;

        //Player body
        private const int FEET = 0;
        private const int HEAD = 1;
        private const int LEFT_BODY = 2;
        private const int RIGHT_BODY = 3;

        //Anim & Dir & Gun states
        private int playerState;
        private int gunState;
        private int dir;
        private const int LEFT = 0;
        private const int RIGHT = 1; 
        private const int IDLE = 0;
        private const int RUN = 1;
        private const int SHOOTING = 1;

        //Player UI
        private Texture2D healthImg;
        private Rectangle[] healthRecs;

        //Magic values for the collision player rectangles
        private const float widthMultiplier = 0.55f;

        public Player(Texture2D[] playerImgs, Gun gun, GraphicsDevice gd, Texture2D healthImg, Color playerColor, bool[] unlocked)
        {
            //Defining the player animations
            playerAnims = new Animation[playerImgs.Length];
            playerAnims[IDLE] = new Animation(playerImgs[IDLE], 1, 9, 9, 0, 0, Animation.ANIMATE_FOREVER, 7, playerLoc, 1.7f, true);
            playerAnims[RUN] = new Animation(playerImgs[RUN], 1, 10, 10, 0, 0, Animation.ANIMATE_FOREVER, 7, playerLoc, 1.7f, true);
            
            this.gd = gd;
            this.healthImg = healthImg;
            this.playerGun = gun;
            this.playerColor = playerColor;
            this.unlocked = unlocked;

            //Setting the player's upgrades
            if (unlocked[HEALTH_UPGRADE])
            {
                playerHealth = 6;
            }
            if (unlocked[SPEED_UPGRADE])
            {
                maxSpeed = 7.5f;
            }

            //Defining the health rec array
            healthRecs = new Rectangle[playerHealth];

            //Looping through the health rectangles
            for (int i = 0; i < healthRecs.Length; i++)
            {
                //Create the health rectangles based on the player color
                if (playerColor == Color.White)
                {
                    //Defining the health rectangle
                    healthRecs[i] = new Rectangle(0 + (healthImg.Width / 6) * i, 5, healthImg.Width / 6, healthImg.Height / 6);
                }
                else
                {
                    //Defining the health rectangle
                    healthRecs[i] = new Rectangle((Game1.screenWidth - healthImg.Width/6) - (healthImg.Width / 6) * i, 5, 
                        healthImg.Width / 6, healthImg.Height / 6);
                }
            }
        }

        //Pre: Gamepad, keyboard, and previous keyboard for the player to handle movement, 
        //gameTime variable, tile rectangles (needed for collision detection with the player), and a playerindex
        //Post: None
        //Desc: Handles the movement/physics of the player, player collision, the holding of the player gun, etc.
        public void PlayerLogic(GamePadState gamePad, KeyboardState kb, KeyboardState prevKb, GameTime gameTime, Rectangle[,] tileRecs, PlayerIndex playerIndex)
        {
            //Gravity 
            playerSpeed.Y += forces.Y;

            //Access this statement if player is grounded
            if (isPlayerGrounded)
            {
                //Decelerate in the opposite direction the player is moving
                playerSpeed.X += -Math.Sign(playerSpeed.X) * forces.X;

                //Access this statement if player speed is below the tolerance value
                if (Math.Abs(playerSpeed.X) <= TOLERANCE)
                {
                    //Reduce player speed to 0
                    playerSpeed.X = 0f;
                }
            }

            //Jumping
            if (kb.IsKeyDown(Keys.Space) && !prevKb.IsKeyDown(Keys.Space) || gamePad.DPad.Up == ButtonState.Pressed || gamePad.Buttons.A == ButtonState.Pressed)
            {
                //Allow the player to jump depending on if the player is grounded or if the player has unlocked the double jump
                if (isPlayerGrounded)
                {
                    playerSpeed.Y = jumpSpeed;
                    Game1.sfxManager.PlayJump();
                }
                else if (unlocked[DOUBLE_JUMP_UNLOCK])
                {
                    //Access this statement if the user hasn't triggered the double jump already
                    if (!doubleJump)
                    {
                        playerSpeed.Y = jumpSpeed;
                        doubleJump = true;
                        Game1.sfxManager.PlayJump();
                    }
                }

                //Player is no longer grounded
                isPlayerGrounded = false;
            }

            //Depending on if the player is shooting, assign the according gunstate and vibration
            if (kb.IsKeyDown(Keys.P) || gamePad.Buttons.RightShoulder == ButtonState.Pressed)
            {
                //Changing gun state
                gunState = SHOOTING;

                //Vibrating
                GamePad.SetVibration(playerIndex, 0.5f, 0.5f);
            }
            else
            {
                //Changing gun state
                gunState = IDLE;

                //Ending vibration
                GamePad.SetVibration(playerIndex, 0, 0);
            }

            //Move the player depending on kb/gamepad input
            if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D) 
                || gamePad.DPad.Right == ButtonState.Pressed)
            {
                //Moving right; adding positive horizontal speed to the player and setting proper player anim and direction
                playerSpeed.X += ACCELERATION;
                playerSpeed.X = MathHelper.Clamp(playerSpeed.X, -maxSpeed, maxSpeed);
                playerState = RUN;
                dir = RIGHT;
                flipType = Animation.FLIP_NONE;

                //Access this statement if the player is grounded
                if (isPlayerGrounded)
                {
                    //sfx
                    Game1.sfxManager.PlayWalk();
                }
            }
            else if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A) 
                || gamePad.DPad.Left == ButtonState.Pressed)
            {
                //Moving left; adding negative horizontal speed to the player and setting proper player anim and direction
                playerSpeed.X -= ACCELERATION;
                playerSpeed.X = MathHelper.Clamp(playerSpeed.X, -maxSpeed, maxSpeed);
                playerState = RUN;
                dir = LEFT;
                flipType = Animation.FLIP_HORIZONTAL;

                //Access this statement if the player is grounded
                if (isPlayerGrounded)
                {
                    //sfx
                    Game1.sfxManager.PlayWalk();
                }
            }
            else
            {
                //Moving right or left depending on thumbstick position; adding positive/negative speed to the player and setting proper player anim and direction
                playerSpeed.X += (int)(maxSpeed * gamePad.ThumbSticks.Left.X);
                playerSpeed.X = MathHelper.Clamp(playerSpeed.X, -maxSpeed, maxSpeed);
                playerState = IDLE;

                //Setting the player direction depending on the thumbstick direction
                if (gamePad.ThumbSticks.Left.X > 0)
                {
                    flipType = Animation.FLIP_NONE;
                    dir = RIGHT;

                    //Access this statement if the player is grounded
                    if (isPlayerGrounded)
                    {
                        //sfx
                        Game1.sfxManager.PlayWalk();
                    }
                }
                else if (gamePad.ThumbSticks.Left.X < 0) 
                {
                    dir = LEFT;
                    flipType = Animation.FLIP_HORIZONTAL;

                    //Access this statement if the player is grounded
                    if (isPlayerGrounded)
                    {
                        //sfx
                        Game1.sfxManager.PlayWalk();
                    }
                }
            }

            //Updating player vector and adding player speed 
            playerLoc.X += playerSpeed.X;
            playerLoc.Y += playerSpeed.Y;

            //Updating player rectangle from the player vector
            playerRec.X = (int)playerLoc.X;
            playerRec.Y = (int)playerLoc.Y;

            //Updating all player animations
            for (int i = 0; i < playerAnims.Length; i++)
            {
                playerAnims[i].destRec.X = (int)playerRec.X;
                playerAnims[i].destRec.Y = (int)playerRec.Y;
            }

            //Updating all timers
            playerAnims[IDLE].Update(gameTime);
            playerAnims[RUN].Update(gameTime);
            invincibilityTimer.Update(gameTime.ElapsedGameTime.Milliseconds);

            //Updating the player recs
            SetPlayerRecs();

            //Check for collisions between the player and all platforms and walls
            PlatformWallCollision(tileRecs);

            //Updating the gun
            playerGun.UpdateGun(playerRec, gameTime, dir, gunState);

            //Access this statement if the enemies list isn't null
            if (enemies != null)
            {
                //Looping through ALL enemies
                for (int i = 0; i < enemies.Count; i++)
                {
                    //Check for collisions between the player and the enemy
                    if (enemies[i].GetCollisionRec().Intersects(playerAnims[playerState].destRec))
                    {
                        //Deducting health
                        HealthDeduction();
                    }
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Updates all body character recs (head, left/right body, and feet)
        private void SetPlayerRecs()
        {
            //Head rec 
            playerRecs[HEAD] = new Rectangle(playerAnims[IDLE].destRec.X + 15, playerAnims[IDLE].destRec.Y - 5,
            (int)(playerAnims[IDLE].destRec.Width * widthMultiplier), (int)(playerAnims[IDLE].destRec.Height * 0.4f));

            //Left and Right body recs
            playerRecs[LEFT_BODY] = new Rectangle((int)(playerAnims[IDLE].destRec.X + 5), playerRecs[HEAD].Y + playerRecs[HEAD].Height,
                (int)(playerAnims[IDLE].destRec.Width * widthMultiplier), (int)(playerAnims[IDLE].destRec.Height * 0.45f));
            playerRecs[RIGHT_BODY] = new Rectangle(playerRecs[LEFT_BODY].X + playerRecs[LEFT_BODY].Width, playerRecs[HEAD].Y + playerRecs[HEAD].Height,
                (int)(playerAnims[IDLE].destRec.Width * widthMultiplier), (int)(playerAnims[IDLE].destRec.Height * 0.45f));

            //Feet rec
            playerRecs[FEET] = new Rectangle(playerAnims[IDLE].destRec.X + 15, playerRecs[LEFT_BODY].Y + (int)(playerRecs[LEFT_BODY].Height * 1.03), 
                (int)(playerAnims[IDLE].destRec.Width * widthMultiplier), (int)(playerAnims[IDLE].destRec.Height * 0.3f));

            //Defining the visible recs when necessary
            if (Game1.showCollisionRecs)
            {
                playerVisibleRecs[HEAD] = new GameRectangle(gd, playerRecs[HEAD]);
                playerVisibleRecs[LEFT_BODY] = new GameRectangle(gd, playerRecs[LEFT_BODY]);
                playerVisibleRecs[RIGHT_BODY] = new GameRectangle(gd, playerRecs[RIGHT_BODY]);
                playerVisibleRecs[FEET] = new GameRectangle(gd, playerRecs[FEET]);
            }
        }

        //Pre: Sprite Batch required to draw
        //Post: None
        //Desc: Draws the player, their collision rectanlges (if needed), their gun, and their UI
        public void DrawPlayer(SpriteBatch spriteBatch)
        { 
            //Drawing the gun
            playerGun.DrawGun(spriteBatch, flipType, gunState);

            //Loop for how much health the player has
            for(int i = 0; i < playerHealth; i++)
            {
                //Drawing a health icon
                spriteBatch.Draw(healthImg, healthRecs[i], playerColor);
            }

            //Depending on if the player is invincible, change player opacity accordingly
            if (invincibilityTimer.IsActive())
            {
                playerOpacity = 0.5f;
            }
            else
            {
                playerOpacity = 1;
            }

            //Drawing the player
            playerAnims[playerState].Draw(spriteBatch, playerColor * playerOpacity, flipType);

            //Drawing the visible recs when necessary
            if (Game1.showCollisionRecs)
            {
                playerVisibleRecs[HEAD].Draw(spriteBatch, Color.Yellow * 0.5f, true);
                playerVisibleRecs[LEFT_BODY].Draw(spriteBatch, Color.Red * 0.5f, true);
                playerVisibleRecs[RIGHT_BODY].Draw(spriteBatch, Color.Blue * 0.5f, true);
                playerVisibleRecs[FEET].Draw(spriteBatch, Color.Green * 0.5f, true);
            }
        }

        //Pre: 2D array of rectangles 
        //Post: None
        //Desc: Handles collision with the tileset and screen bounds
        //Also checks to see which part of the player is being collided with (Feet take highest priority)
        private void PlatformWallCollision(Rectangle [,] tileRecs)
        {
            //Collision tracker
            bool collision = false;

            //Loop through the tileset recs (2d array)
            for (int i = 0; i < tileRecs.GetLength(0); i++)
            {
                for (int j = 0; j < tileRecs.GetLength(1); j++)
                {
                    //Access this statement if the general player rectangle is colliding with the current tile set rectangle
                    if (playerRec.Intersects(tileRecs[i,j]))
                    {
                        //Depending on which player rec is colliding with the tile rectangle, adjust accordingly
                        if (playerRecs[FEET].Intersects(tileRecs[i,j]))
                        {
                            //Adjusting player rectangle, player animation rec, player loc, and speed
                            playerRec.Y = tileRecs[i,j].Y - playerAnims[playerState].destRec.Height;
                            playerAnims[playerState].destRec.Y = playerRec.Y;
                            playerLoc.Y = playerRec.Y;
                            playerSpeed.Y = 0f;

                            isPlayerGrounded = true;
                            doubleJump = false;
                            collision = true;
                        }
                        else if (playerRecs[HEAD].Intersects(tileRecs[i, j]))
                        {
                            //Adjusting player rectangle, player animation rec, player loc, and speed
                            playerRec.Y = (int)(tileRecs[i, j].Y + playerAnims[playerState].destRec.Height/2.25);
                            playerAnims[playerState].destRec.Y = playerRec.Y;
                            playerLoc.Y = playerRec.Y;
                            playerSpeed.Y = 0;

                            collision = true;
                        }
                        else if (playerRecs[LEFT].Intersects(tileRecs[i,j]))
                        {
                            //Adjusting player rectangle, player animation rec, player loc, and speed
                            playerAnims[playerState].destRec.X = playerRec.X;
                            playerLoc.X = playerRec.X;
                            playerSpeed.X = 0; 

                            collision = true;
                        }
                        else if (playerRecs[RIGHT].Intersects(tileRecs[i,j]))
                        {
                            //Adjusting player rectangle, player animation rec, player loc, and speed
                            playerRec.X = tileRecs[i, j].Width + playerAnims[playerState].destRec.Width;
                            playerAnims[playerState].destRec.Y = playerRec.X;
                            playerLoc.X = playerRec.X;
                            playerSpeed.X = 0; 

                            collision = true;
                        }

                        //Access this statement if a collsion happend
                        if (collision == true)
                        {
                            //Set player recs again and reset collision tracker
                            SetPlayerRecs();
                            collision = false;
                        }
                    }
                }
            }

            //Depending on the player location, adjust the players position
            if (playerLoc.X >= Game1.screenWidth)
            {
                //Set player to left of screen
                playerRec.X = 0;
                playerLoc.X = playerRec.X;
            }
            else if (playerLoc.X < 0 - playerRec.Width)
            {
                //Set player to right of screen
                playerRec.X = Game1.screenWidth - playerRec.Width;
                playerLoc.X = playerRec.X;
            }
            else if (playerLoc.Y >= Game1.screenHeight)
            {
                //Set player to top of screen
                playerRec.X = 50;
                playerRec.Y = 50;
                playerLoc.X = playerRec.X;
                playerLoc.Y = playerRec.Y;
            }
        }

        //Pre: List of enemies
        //Post: None
        //Desc: Setting the enemies to the gun and this player 
        public void SetEnemies(List<Enemy> enemies)
        {
            playerGun.SetEnemies(enemies);
            this.enemies = enemies;
        }

        //Pre: None
        //Post: Player animation dest rectangle
        //Desc: Returns the rectangle of the current player animation
        public Rectangle GetCollisionRec()
        {
            return playerAnims[playerState].destRec;
        }

        //Pre: None
        //Post: None
        //Desc: Deducts health from the player (-1), plays the hit sfx, and starts the invincibility timer
        public void HealthDeduction()
        {
            //Access this statement if the player isn't invincible
            if (invincibilityTimer.IsFinished() || invincibilityTimer.IsInactive())
            {
                //Reduce health
                playerHealth--;

                //Play sfx and reset invincibilty timer
                Game1.sfxManager.PlayHit();
                invincibilityTimer.ResetTimer(true);
            }
        }

        //Pre: None
        //Post: Bool on wether if the player is dead or not
        //Desc: Returns true if the player is dead
        public bool IsDead()
        {
            //Access this statement if the player health is less then 1
            if (playerHealth <= 0)
            {
                //Returning that the player is dead
                return true;
            }

            //Returning that the player is alive
            return false;
        }
    }
}
