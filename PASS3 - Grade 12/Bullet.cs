//Author: Dan Lichtin
//File Name: Bullet.cs
//Project Name: PASS3
//Creation Date: December 16, 2022
//Modified Date: January 22, 2023
//Description: Handles the bullet logic, all guns use this object/class
using Animation2D;
using Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//PROOF OF CONCEPT:
//OOP - Bullet is an object that the gun carries; each bullet carries it's own speed, animation, and hit box

namespace PASS3___Grade_12
{
    class Bullet
    {
        //Bullet data
        private Animation bulletAnim;
        private Vector2 bulletSpeed = new Vector2(0, 0);
        private Rectangle bulletRec;
        private GameRectangle bulletVisibleRec;
        private Texture2D bulletImg;
        private SpriteEffects flipType;

        //Bullet physics 
        private const float ACCELERATION = 5f;
        private const float MAX_SPEED = 10f;

        //Game time
        private GameTime gameTime;

        //Graphics Device
        private GraphicsDevice gd;

        //Direction identifiers
        private const int LEFT = 0;
        private const int RIGHT = 1;
        private int originDir;

        public Bullet(Texture2D bulletImg, Vector2 gunLoc, int originDir, GraphicsDevice gd)
        {
            //Defining anim
            bulletAnim = new Animation(bulletImg, 5, 1, 5, 0, 0, Animation.ANIMATE_ONCE, 12, gunLoc, 0.6f, true);

            this.originDir = originDir;
            this.gd = gd;
            this.bulletImg = bulletImg;

            if (originDir == RIGHT)
            {
                flipType = Animation.FLIP_NONE;
            }
            else
            {
                flipType = Animation.FLIP_HORIZONTAL;
            }

            //Setting bullet rec
            SetBulletRec();
            bulletVisibleRec = new GameRectangle(gd, bulletRec);
        }

        //Pre: None
        //Post: An int of the bullet's x value
        //Desc: Returns the X coorindates of the bullet
        public int GetBulletX()
        {
            return bulletAnim.destRec.X;
        }

        //Pre: None
        //Post: An int of the bullet's y value
        //Desc: Returns the Y coorindates of the bullet
        public int GetBulletY()
        {
            return bulletAnim.destRec.Y;
        }

        //Pre: None
        //Post: A rectangle
        //Desc: Returns the rectangle of the bullet
        public Rectangle GetBulletRec()
        {
            return bulletRec;
        }

        //Pre: None
        //Post: None
        //Desc: Updates the bullet speed, rectangle, anim, etc.
        public void UpdateBullet()
        {
            //Depending on the direction the bullet was shot at, change it's speed
            if (originDir == RIGHT)
            {
                //Moving right
                bulletSpeed.X += ACCELERATION;
                bulletSpeed.X = MathHelper.Clamp(bulletSpeed.X, -MAX_SPEED, MAX_SPEED);
            }
            else if (originDir == LEFT)
            {
                //Moving left
                bulletSpeed.X -= ACCELERATION;
                bulletSpeed.X = MathHelper.Clamp(bulletSpeed.X, -MAX_SPEED, MAX_SPEED);
            }

            //Updating recs
            bulletRec.X += (int)bulletSpeed.X;
            bulletAnim.destRec.X = (int)bulletRec.X;

            //Updating anim
            bulletAnim.Update(gameTime);

            //Updating the visible bullet rec
            SetBulletRec();
        }

        //Pre: Sprite Batch required to draw
        //Post: None
        //Desc: Draws the bullet, and it's visible rectangle (if neccesary)
        public void DrawBullet(SpriteBatch spriteBatch)
        {
            //Drawing the bullet
            bulletAnim.Draw(spriteBatch, Color.Orange, flipType);

            //Drawing the visible recs when necessary
            if (Game1.showCollisionRecs)
            {
                bulletVisibleRec.Draw(spriteBatch, Color.Yellow * 0.5f, true);
            }
        }

        //Pre: None
        //Post: None
        //Desc: Updates the bullet rec
        private void SetBulletRec()
        {
            //Redefining the bullet rectangle
            bulletRec = new Rectangle(bulletAnim.destRec.X, bulletAnim.destRec.Y,
                bulletImg.Width/10, bulletImg.Height/2);

            //Defining the visible recs when necessary
            if (Game1.showCollisionRecs)
            {
                bulletVisibleRec = new GameRectangle(gd, bulletRec);
            }
        }

        //Pre: None
        //Post: Bool on wether or not the bullet is animating
        //Desc: Returns true if the bullet is animating and false if otherwise
        public bool IsBulletAnimated()
        {
            //Access this statement is the bullet is animating
            if (bulletAnim.isAnimating)
            {
                //Returning that the bullet is animating
                return true;
            }

            //Returning that the bullet is not animating
            return false;
        }
    }
}
