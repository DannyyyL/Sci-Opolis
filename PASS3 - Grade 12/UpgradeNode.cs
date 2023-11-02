//Author: Dan Lichtin
//File Name: UpgradeNode.cs
//Project Name: PASS3
//Creation Date: December 31, 2022
//Modified Date: January 22, 2022
//Description: The upgrade node object held by the tree, holds a parent (unless it's the root), and can hold left/right child nodes
//Also stores the upgrade image, rec, it's cost, unlock status, etc.
using Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
//PROOF OF CONCEPT:
//Binary Tree - This is a node of a binary tree; carries a left, right, and parent node;
//also carries the functionality that nodes in binary tree have (set & get left/right/parent)

namespace PASS3___Grade_12
{
    class UpgradeNode
    {
        //Storing nodes
        private UpgradeNode parent;
        private UpgradeNode left;
        private UpgradeNode right;

        //NODE DATA
        //Node image
        private Texture2D upgradeImg;
        //Node rec
        private Rectangle upgradeRec;
        //Node loc's
        private Vector2 upgradeLoc;
        private Vector2 upgradeStatusLoc;
        private Vector2 upgradeDescLoc;
        private Vector2 descCoinLoc;
        //Node unlock 
        private bool unlocked = false;
        //Node cost and
        private int upgradeCost;
        //Node description
        private string upgradeDesc;
        //Node hover
        bool nodeHover = false;

        //Other node data
        private Vector2 parentUpgradeLoc;

        //Font
        private SpriteFont upgradeFont;

        //Data identifiers
        const int COIN = 1;
        const int NUM = 1; 

        //Line data
        private GameLine upgradeConnector;
        private const int CONNECTOR_WIDTH = 5;

        //Graphics Device
        private GraphicsDevice gd;

        //Constructor for a new UpgradeNode object with the given parameters
        public UpgradeNode(GraphicsDevice gd, Texture2D upgradeImg, SpriteFont upgradeFont, Rectangle upgradeRec, 
            int upgradeCost, string upgradeDesc, bool unlocked)
        {
            //Storing all variables needed for the upgrade node
            this.unlocked = unlocked;
            this.upgradeImg = upgradeImg;
            this.upgradeCost = upgradeCost;
            this.upgradeRec = upgradeRec;
            this.gd = gd;
            this.upgradeFont = upgradeFont;
            this.upgradeDesc = upgradeDesc;

            //Defining and Setting vectors
            upgradeLoc = new Vector2(upgradeRec.X, upgradeRec.Y);
            upgradeDescLoc = new Vector2(Game1.screenWidth / 2 - (int)(upgradeFont.MeasureString(upgradeDesc).X * 0.85), 0);
            upgradeStatusLoc = new Vector2(upgradeDescLoc.X + (int)(upgradeFont.MeasureString(upgradeDesc).X * 1.05), upgradeDescLoc.Y);
            upgradeLoc.X += upgradeRec.Width / 2;
            descCoinLoc = upgradeLoc;
        }

        /////////////////
        ////MODIFIERS////
        /////////////////

        //Pre: None
        //Post: None
        //Desc: Unlocks the upgrade node
        public void Unlock()
        {
            unlocked = true;
        }

        //Pre: UpgradeNode object
        //Post: None
        //Desc: Sets the parent UpgradeNode of this node;
        //Also creates the game line which will connect this node to the parent (visually)
        public void SetParent(UpgradeNode parent)
        {
            this.parent = parent;

            //Creating the line & it's location once the parent has been set
            parentUpgradeLoc = parent.GetUpgradeLoc();
            parentUpgradeLoc.Y += upgradeRec.Height - 1;
            upgradeConnector = new GameLine(gd, parentUpgradeLoc, upgradeLoc, CONNECTOR_WIDTH);
        }

        //Pre: UpgradeNode object
        //Post: None
        //Desc: Sets the left UpgradeNode of this node
        public void SetLeft(UpgradeNode left)
        {
            this.left = left;
        }

        //Pre: UpgradeNode object
        //Post: None
        //Desc: Sets the right UpgradeNode of this node
        public void SetRight(UpgradeNode right)
        {
            this.right = right;
        }

        /////////////////
        ////ACCESSORS////
        /////////////////

        //Pre: None
        //Post: The left node
        //Desc: Returns the left node 
        public UpgradeNode GetLeft()
        {
            return left;
        }

        //Pre: None
        //Post: The right node
        //Desc: Returns the right node
        public UpgradeNode GetRight()
        {
            return right;
        }

        //Pre: None
        //Post: A bool of the unlock status
        //Desc: Returns the unlock status of the node
        public bool IsUnlocked()
        {
            return unlocked;
        }

        //Pre: None
        //Post: A vector2 of the node's location
        //Desc: Returns the location of the node
        public Vector2 GetUpgradeLoc()
        {
            return upgradeLoc;
        }

        //Pre: None
        //Post: An int of the node's upgrade cost
        //Desc: Returns the upgrade cost of the node
        public int GetUpgradeCost()
        {
            return upgradeCost;
        }

        //////////////////
        ////BEHAVIOURS////
        //////////////////

        //Pre: A point and a button state
        //Post: None
        //Desc: Updates the upgrade node, detects if the passed in Point paramater (the mouse) is intersecting with the node
        //and wether or not the passsed in ButtonState paramater (the left click of the mouse) is being pressed on the node
        //Handles the purchasing logic 
        public void UpdateUpgradeNode(Point position, ButtonState buttonState)
        {
            //Setting the node hover depending on if the mouse (position) is on the upgrade node
            if (upgradeRec.Contains(position))
            {
                //Setting node hover to true
                nodeHover = true;

                //Access this if statement if the node has been clicked
                if (buttonState == ButtonState.Pressed)
                {
                    if (!IsUnlocked())
                    {
                        //Access this if statement if the node hasn't been unlocked and the player has more coins then the node costs
                        if (Convert.ToInt32(Game1.playerData[COIN, NUM]) >= upgradeCost)
                        {
                            //If statement for if the node has no parent or the parent has been unlocked
                            if (parent == null || parent.IsUnlocked())
                            {
                                //Unlocking the upgrade
                                Unlock();

                                //Subtracting from the player's coins and saving the newly unlocked upgrade
                                Game1.CoinDeduction(upgradeCost);
                                Game1.SaveUpgrades(upgradeCost);
                                Game1.sfxManager.PlayPurchase();
                            }
                            else
                            {
                                Game1.sfxManager.PlayError();
                            }
                        }
                        else
                        {
                            Game1.sfxManager.PlayError();
                        }
                    }
                    else
                    {
                        Game1.sfxManager.PlayPurchase();
                    }
                }
            }
            else
            {
                //Setting node hover to false
                nodeHover = false;
            }
        }

        //Pre: SpriteBatch
        //Post: None
        //Desc: Draws the upgrade node, it's parent connecting line (if possible), the description of the upgrade and
        //it's purchase status (unlocked, locked, purchasable, not enough credit)
        public void DrawUpgradeNode(SpriteBatch spriteBatch)
        {
            //Drawing the upgrade icon and it's data depending on if it's being hovered over
            if (nodeHover)
            {
                //Drawing the upgrade description
                spriteBatch.DrawString(upgradeFont, upgradeDesc, upgradeDescLoc, Color.White);

                //Drawing the upgrade purchase status based on the node's unlock status, the user's currency, and the parent node information
                if (unlocked)
                {
                    //Drawing the purchase status as unlocked
                    spriteBatch.DrawString(upgradeFont, ": Unlocked", upgradeStatusLoc, Color.Green);
                }
                else if (parent == null || parent.IsUnlocked())
                {
                    //Drawing the purchase status depending on the user's currency 
                    if (Convert.ToInt32(Game1.playerData[COIN, NUM]) < upgradeCost)
                    {
                        //Drawing the purchase status as not enough credit
                        spriteBatch.DrawString(upgradeFont, ": Not Enough Credit", upgradeStatusLoc, Color.Red);
                    }
                    else
                    {
                        //Drawing the purchase status as purchasable
                        spriteBatch.DrawString(upgradeFont, ": Purchasable", upgradeStatusLoc, Color.Gold);
                    }
                }
                else
                {
                    //Drawing the purchase status as locked
                    spriteBatch.DrawString(upgradeFont, ": Locked", upgradeStatusLoc, Color.Red);
                }

                //Drawing the upgrade icon (in chocolate color)
                spriteBatch.Draw(upgradeImg, upgradeRec, Color.Chocolate);
            }
            else
            {
                //Drawing the upgrade icon (in it's original color)
                spriteBatch.Draw(upgradeImg, upgradeRec, Color.White);
            }

            //Drawing the cost of the upgrade if the node hasn't been unlocked
            if(!unlocked)
            {
                spriteBatch.DrawString(upgradeFont, Convert.ToString(upgradeCost), descCoinLoc, Color.Gold);               
            }

            //Only access the if statement if the node has a parent
            if (parent != null)
            {
                //Drawing a connecting line to the previous parent; changes colour depending on if the parent node has been unlocked
                if (parent.IsUnlocked() == false)
                {
                    //Drawing the connecting line (in black)
                    upgradeConnector.Draw(spriteBatch, Color.Black * 0.8f);
                }
                else
                {
                    //Drawing the connecting line (in gold)
                    upgradeConnector.Draw(spriteBatch, Color.Gold);
                }
            }

        }
    }
}
