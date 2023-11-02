//Author: Dan Lichtin
//File Name: Cyclops.cs
//Project Name: PASS3
//Creation Date: January 12, 2022
//Modified Date: January 22, 2023
//Description: A child class of the parent enemy class; handles the logic and data of the cyclops enemy
using Animation2D;
using Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
//PROOF OF CONCEPT:
//OOP - Cyclops is the chid class of the enemy object;
//It holds the functions and data of the enemy parent class but carries its own movement speed and animations
//The cyclops enemy has a unique ability to explode and kill itself when a wider rec collides with the player

namespace PASS3___Grade_12
{
    class Cyclops : Enemy
    {
        //Due to anim rec being too large, another rec will be used for collision
        private Rectangle cyclopsRec;

        //Extra visible rec for explosion
        private GameRectangle cyclopsExposionVisibleRec;

        public Cyclops(Texture2D[] enemyImgs, GraphicsDevice gd) : base(enemyImgs, gd)
        {
            //Defining anims
            enemyAnims[CYCLOPS + RUN] = new Animation(enemyImgs[CYCLOPS + RUN], 12, 1, 12, 0, 0,
                 Animation.ANIMATE_FOREVER, 8, new Vector2(randomXVal, 0), 2f, true);
            enemyAnims[CYCLOPS + DEATH] = new Animation(enemyImgs[CYCLOPS + DEATH], 11, 1, 11, 0, 0,
                 Animation.ANIMATE_ONCE, 8, new Vector2(randomXVal, 0), 2f, true);
            enemyAnims[CYCLOPS + ATTACK] = new Animation(enemyImgs[CYCLOPS + ATTACK], 7, 1, 7, 0, 0,
                Animation.ANIMATE_ONCE, 6, new Vector2(randomXVal, 0), 2f, true);

            //Setting attributes
            rightSpeed = 1;
            leftSpeed = -1;
            enemyState = CYCLOPS + RUN;
            enemyType = CYCLOPS;

            //Special test recs
            enemyVisibleRec = new GameRectangle(gd, cyclopsRec);
            cyclopsExposionVisibleRec = new GameRectangle(gd, enemyAnims[CYCLOPS + RUN].destRec);
        }

        //Pre: None
        //Post: None
        //Desc: Updates the visible recs
        public override void SetTestRec()
        {
            //Defining the visible recs when necessary
            if (Game1.showCollisionRecs)
            {
                enemyVisibleRec = new GameRectangle(gd, cyclopsRec);
                cyclopsExposionVisibleRec = new GameRectangle(gd, enemyAnims[CYCLOPS + RUN].destRec);
            }
        }

        //Pre: SpriteBatch for drawing
        //Post: None
        //Desc: Drawing the test recs
        public override void DrawTestRec(SpriteBatch spriteBatch)
        {
            //Drawing the visible recs when necessary
            if (Game1.showCollisionRecs)
            {
                enemyVisibleRec.Draw(spriteBatch, Color.Yellow * 0.5f, true);
                cyclopsExposionVisibleRec.Draw(spriteBatch, Color.Red * 0.5f, true);
            }
        }

        //Pre: A player object
        //Post: None
        //Desc: Triggering the explosion of the cyclops enemy, playing the death sfx, and deducting player health
        private void TriggerExplosion(Player player)
        {
            //Access this statement if the cyclops is falling at a speed less than one
            if (enemySpeed.Y <= 1)
            {
                //Changing enemy state
                enemyState = CYCLOPS + ATTACK;

                //Reducing health to 0 & play death sfx
                health = 0;
                Game1.sfxManager.PlayDeath();

                //Deducting player health
                player.HealthDeduction();
            }
        }

        //Pre: None
        //Post: Bool on wether if the enemy is dead or not
        //Desc: Returns true if the enemy is dead
        public override bool isDead()
        {
            //Access this statement if the enemy health is 0
            if (health == 0)
            {
                //Access this statement if the enemy isn't exploding or dying (if those anims aren't playing)
                if (!enemyAnims[CYCLOPS + DEATH].isAnimating || !enemyAnims[CYCLOPS + ATTACK].isAnimating)
                {
                    //Returning that the enemy is dead
                    return true;
                }
            }

            //Returning that the enemy isn't dead
            return false;
        }

        //Pre: None
        //Post: Rectangle of the enemy
        //Desc: Returns the rectangle of the enemy,
        //The rectangle will be impossible to collide with if the enemy is dying
        public override Rectangle GetCollisionRec()
        {
            //Access this statement if the enemy is dying
            if (isDying())
            {
                //Returning an impossible collision rectangle
                return new Rectangle(0, 0, 0, 0);
            }

            //Returning the rectangle of the cyclops
            return cyclopsRec;
        }

        //Pre: None
        //Post: None
        //Desc: Updates the cyclops rec
        private void UpdateCyclopsRec()
        {
            //Depending on the direction of the enemy, redefine the cyclops rec 
            if (dir == RIGHT)
            {
                cyclopsRec = new Rectangle((int)(enemyAnims[CYCLOPS + RUN].destRec.X + 37.5), enemyAnims[CYCLOPS + RUN].destRec.Y,
                  (int)(enemyAnims[CYCLOPS + RUN].destRec.Width / 8.5), enemyAnims[CYCLOPS + RUN].destRec.Height);
            }
            else
            {
                cyclopsRec = new Rectangle(enemyAnims[CYCLOPS + RUN].destRec.X  + 150, enemyAnims[CYCLOPS + RUN].destRec.Y,
                    (int)(enemyAnims[CYCLOPS + RUN].destRec.Width / 8.5), enemyAnims[CYCLOPS + RUN].destRec.Height);
            }
        }

        //Pre: GameTime, 2D array of the tileset rectangles, List of the player object
        //Post: None
        //Desc: Updates and handles all enemy logic
        public override void EnemyLogic(GameTime gameTime, Rectangle[,] tileRecs, List<Player> players)
        {
            //Depending on the state of the enemy, change speed accordingly
            switch (enemyState)
            {
                case CYCLOPS + RUN:
                    enemySpeed.X = currentSpeed;
                    break;
                case CYCLOPS + ATTACK:
                    enemySpeed.Y = 0;
                    enemySpeed.X = 0;
                    break;
                default:
                    enemySpeed.X = 0;
                    break;
            }

            //Updating the cyclops rec
            UpdateCyclopsRec();

            //Updating the generic enemy data (recs, speed, etc.)
            UpdateEnemy(gameTime, dir, tileRecs);

            //Access this statement if the cyclops is in it's natural anim
            if (enemyState == CYCLOPS + RUN)
            {
                //Loop through all players
                for (int i = 0; i < players.Count; i++)
                {
                    //Trigger the cyclops explosion if the player is intersecting with it's rec
                    if (enemyAnims[CYCLOPS + RUN].destRec.Intersects(players[i].GetCollisionRec()))
                    {
                        TriggerExplosion(players[i]);
                    }
                }
            }
        }

        //Pre: 2D array of the tileset rectangles, Texture2D image of the enemy
        //Post: None
        //Desc: Updates the collision logic of the enemy relating to the tileset and screen width's
        protected override void CollisionLogic(Rectangle[,] tileRecs, Texture2D enemyImg)
        {
            //Updating platform collision
            PlatformCollision(tileRecs, cyclopsRec);

            //Access this statement if the enemy is out of bounds, and trigger their death
            if (cyclopsRec.X > Game1.screenWidth || cyclopsRec.X + cyclopsRec.Width < - 20)
            {
                TriggerDeath(false);
            }
        }

    }
}
