//Author: Dan Lichtin
//File Name: Enemy.cs
//Project Name: PASS3
//Creation Date: December 19, 2022
//Modified Date: January 22, 2023
//Description: This class will handle all the enemy data, movement, and collision
using Animation2D;
using Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
//PROOF OF CONCEPT:
//OOP - Enemy is the parent class of all enemies; it holds functions that the child enemies can call, and the general enemy data that the enemies carry
//2D arrays - Enemies detect collision with every object; they loop through the tileset (a 2d array) and check for intersections with every single tile

namespace PASS3___Grade_12
{
    class Enemy
    {
        //Enemy data
        protected Animation[] enemyAnims;
        protected Texture2D[] enemyImgs;
        protected Rectangle enemyRec;
        protected Vector2 enemySpeed;
        protected GameRectangle enemyVisibleRec;
        protected int health;
        protected Gun enemyGun;
        protected int enemyType;

        //Enemy physics data
        protected const float GRAVITY = 9.8f / 50;
        protected Vector2 forces = new Vector2(0, GRAVITY);
        protected int rightSpeed;
        protected int leftSpeed;
        protected int currentSpeed;

        //Graphics Device
        protected GraphicsDevice gd;

        //Enemy types
        public const int LONG_LEGS = Game1.LONG_LEGS;
        public const int GOOP = Game1.GOOP;
        public const int CYCLOPS = Game1.CYCLOPS;

        //Enemy states
        protected const int RUN = Game1.RUN;
        protected const int DEATH = Game1.DEATH;
        protected const int ATTACK = Game1.ATTACK;
        protected const int RIGHT = 1;
        protected const int LEFT = 0;
        protected const int SHOOTING = 1;
        protected int dir;
        protected int enemyState;
        private SpriteEffects flipType;

        //Data used for randomizing enemy spawns
        //Rules --- Enemies will only spawn from random X values, the Y values stay the same
        protected static Random rng = new Random();
        protected int randomXVal;
        protected int lowXLimit = 1;
        protected int HighXLimit = Game1.screenWidth;

        public Enemy(Texture2D[] enemyImgs, GraphicsDevice gd)
        {
            this.enemyImgs = enemyImgs;
            this.gd = gd;

            //Defining the enemy anims array
            enemyAnims = new Animation[enemyImgs.Length]; 
        }

        //Pre: GameTime, 2D array of the tileset rectangles, List of the player object
        //Post: None
        //Desc: Updates and handles all enemy logic
        public virtual void EnemyLogic(GameTime gameTime, Rectangle[,] tileRecs, List<Player> players)
        {
        }

        //Pre: None
        //Post: None
        //Desc: Updates the visible rec
        public virtual void SetTestRec()
        {
            //Defining the visible rec when necessary
            if (Game1.showCollisionRecs)
            {
                enemyVisibleRec = new GameRectangle(gd, enemyAnims[enemyState].destRec);
            }
        }

        //Pre: SpriteBatch for drawing
        //Post: None
        //Desc: Drawing the test recs
        public virtual void DrawTestRec(SpriteBatch spriteBatch)
        {
            //Drawing the visible rec when necessary
            if (Game1.showCollisionRecs)
            {
                enemyVisibleRec.Draw(spriteBatch, Color.Yellow * 0.5f, true);
            }
        }

        //Pre: None
        //Post: An int of the enemy type 
        //Desc: Returns the enemy type as an int
        public virtual int GetEnemyType()
        {
            return enemyType;
        }

        //Pre: None
        //Post: Rectangle of the enemy
        //Desc: Returns the rectangle of the enemy,
        //The rectangle will be impossible to collide with if the enemy is dying
        public virtual Rectangle GetCollisionRec()
        {
            //Access this statement if the enemy is dying
            if (isDying())
            {
                //Returning an impossible collision rectangle
                return new Rectangle(0, 0, 0, 0);
            }

            //Returning the rectangle of the cyclops
            return enemyAnims[enemyState].destRec;
        }

        //Pre: A bool regarding wether the death sound should be played or not
        //Post: None
        //Desc: Triggering the death of the enemy, playing the death sfx (if sound on == true),
        //and deducting player health
        public virtual void TriggerDeath(bool soundOn)
        {
            //Changing enemy state and reducing health to 0
            enemyState = enemyType + DEATH;
            health = 0;

            //Access this statement if soundOn is true
            if (soundOn)
            {
                //Play death sfx
                Game1.sfxManager.PlayDeath();
            }
        }

        //Pre: None
        //Post: Bool on wether if the enemy is dead or not
        //Desc: Returns true if the enemy is dead
        public virtual bool isDead()
        {
            //Access this statement if the enemy health is 0
            if (health == 0)
            {
                //Access this statement if the enemy isn't dying (if the anim isn't playing)
                if (!enemyAnims[enemyType + DEATH].isAnimating)
                {
                    //Returning that the enemy is dead
                    return true;
                }
            }

            //Returning that the enemy isn't dead
            return false;
        }

        //Pre: None
        //Post: Bool on wether if the enemy is dying or not
        //Desc: Returns true if the enemy is dying
        public virtual bool isDying()
        {
            //Access this statement if the enemy is in it's death state
            if (enemyState == enemyType + DEATH)
            {
                return true;
            }

            return false;
        }

        //Pre: None
        //Post: None
        //Desc: Randomizes the direction and the x value of the enemy on spawn
        public virtual void RandomizeSpawn()
        {
            //Randomizing direction and x val
            dir = rng.Next(LEFT, RIGHT + 1);
            randomXVal = rng.Next(lowXLimit + enemyAnims[enemyState].destRec.Width, HighXLimit - enemyAnims[enemyState].destRec.Width);

            //Setting the enemy rec's to the x val
            enemyRec.X = randomXVal;
            enemyAnims[enemyState].destRec.X = (int)enemyRec.X;
        }

        //Pre: GameTime, 2D array of the tileset rectangles, and the direction of the enemy (int)
        //Post: None
        //Desc: Updates generic enemy data; speed, recs, anims, collision, etc.
        protected void UpdateEnemy(GameTime gameTime, int dir, Rectangle[,] tileRecs)
        {
            //Gravity 
            enemySpeed.Y += forces.Y;

            //Adding speed 
            enemyRec.X += (int)enemySpeed.X;
            enemyRec.Y += (int)enemySpeed.Y;

            //Updating the animation dest recs
            enemyAnims[enemyState].destRec.X = (int)enemyRec.X;
            enemyAnims[enemyState].destRec.Y = (int)enemyRec.Y;

            //Updating the animation & timers
            enemyAnims[enemyState].Update(gameTime);

            //Updating the test rec
            SetTestRec();

            //Updating collision
            CollisionLogic(tileRecs, enemyImgs[enemyType]);

            //Assigning the correct speed according to the direction
            if (dir == RIGHT)
            {
                currentSpeed = rightSpeed;
            }
            else
            {
                currentSpeed = leftSpeed;
            }
        }

        //Pre: Sprite Batch required to draw, and an int of original direction of the enemy (it's direction in its image)
        //Post: None
        //Desc: Draws the enemy, their collision rectanlges (if needed), and their gun (if they have one)
        public virtual void DrawEnemy(SpriteBatch spriteBatch, int originalDir)
        {
            //Access inner statements depending on the enemy's current direction 
            if (dir == LEFT)
            {
                //Depending on the enemy's original direction, flip
                if (originalDir == LEFT)
                {
                    flipType = Animation.FLIP_NONE;
                }
                else
                {
                    flipType = Animation.FLIP_HORIZONTAL;
                }
            }
            else
            {
                //Depending on the enemy's original direction, flip
                if (originalDir == LEFT)
                {
                    flipType = Animation.FLIP_HORIZONTAL;
                }
                else
                {
                    flipType = Animation.FLIP_NONE;
                }
            }

            //Drawing the enemy
            enemyAnims[enemyState].Draw(spriteBatch, Color.White, flipType);

            //Drawing the gun, if the enemy has one 
            if (enemyGun != null)
            {
                enemyGun.DrawGun(spriteBatch, flipType, SHOOTING);
            }

            //Drawing the test rec 
            DrawTestRec(spriteBatch);
        }

        //Pre: 2D array of the tileset rectangles, Texture2D image of the enemy
        //Post: None
        //Desc: Updates the collision logic of the enemy relating to the tileset and screen width's
        protected virtual void CollisionLogic(Rectangle[,] tileRecs, Texture2D enemyImg)
        {
            //Updating platform collision
            PlatformCollision(tileRecs, enemyAnims[enemyState].destRec);

            //Depending on where the enemy is out of bounds, flip their direction
            if (enemyAnims[enemyState].destRec.X + enemyImg.Width / 3 >= Game1.screenWidth)
            {
                dir = LEFT;
            }
            else if (enemyAnims[enemyState].destRec.X < 0)
            {
                dir = RIGHT;
            }
        }

        //Pre: 2D array of rectangles, and the rectangle which will be intersected with
        //Post: None
        //Desc: Handles collision with the tileset
        protected void PlatformCollision(Rectangle[,] tileRecs, Rectangle intersectingRec)
        {
            //Looping through the tiles and testing collsion between the enemy and every platform.
            for (int i = 0; i < tileRecs.GetLength(0); i++)
            {
                for (int j = 0; j < tileRecs.GetLength(1); j++)
                {
                    //Access this statement if the intersecting rectangle is colliding with the tile
                    if (intersectingRec.Intersects(tileRecs[i, j]))
                    {
                        //Adjusting enemy rectangle, enemy animation rec, and speed
                        enemyRec.Y = tileRecs[i, j].Y - enemyAnims[enemyState].destRec.Height;
                        enemyAnims[enemyState].destRec.Y = enemyRec.Y;
                        enemySpeed.Y = 0f;
                    }
                }
            }
        }
    }
}
