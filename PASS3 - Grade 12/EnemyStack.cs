//Author: Dan Lichtin
//File Name: EnemyStack.cs
//Project Name: PASS3
//Creation Date: December 27, 2022
//Modified Date: January 22, 2023
//Description: Carries enemies and pops them given a time interval
using Microsoft.Xna.Framework.Graphics;
//PROOF OF CONCEPT:
//Stack - EnemyStack is a stack that carries an array of enemy objects and is held by a queue;
//EnemyStack holds some of the functionality/data that a stack has; Pop, Size (popped enemies), and carrying an array/list

namespace PASS3___Grade_12
{
    class EnemyStack
    {
        //Base number of how many enemies will be in one drop (A.K.A one part of the wave)
        private int maxEnemyNum = 2;

        //Tracker to count how many enemies have been popped
        private int poppedEnemies = 0;

        //Array of enemies
        private Enemy[] enemies;

        public EnemyStack(Texture2D[] enemyImgs, GraphicsDevice gd, Gun gun, int waveNum)
        {
            //For every wave num, add +1 to the max amount of enemies in a drop
            maxEnemyNum += waveNum;

            //Define the length of the enemy array 
            enemies = new Enemy[maxEnemyNum];

            //Loop through the max amount of enemies in a drop
            for (int i = 0; i < maxEnemyNum; i++)
            {
                //Depending on the iteration ticker, add a different type of enemey
                if (i < 2)
                {
                    //Adding a long legs enemy
                    enemies[i] = new LongLegs(enemyImgs, gd, gun);
                }
                else if (i < 4)
                {
                    //Adding a goop enemy
                    enemies[i] = new Goop(enemyImgs, gd);
                }
                else
                {
                    //Adding a cyclops enemy
                    enemies[i] = new Cyclops(enemyImgs, gd);
                }

                //Enemy spawn randomizer 
                enemies[i].RandomizeSpawn();
            }
        }

        //Pre: None
        //Post: int of the max enemy num
        //Desc: Returns the max amount of enemeies in the stack
        public int GetEnemyNum()
        {
            return maxEnemyNum;
        }

        //Pre: None
        //Post: Enemy object
        //Desc: Returns an enemy from the top of the enemy array 
        public Enemy Pop()
        {
            //Access this statement if there is at least one enemy in the array
            if (enemies.Length != 0)
            {
                //Add +1 to the popped enemies tracker and return the last enemy in the array
                poppedEnemies++;
                return enemies[enemies.Length - poppedEnemies];
            }

            return null;
        }
    }
}
