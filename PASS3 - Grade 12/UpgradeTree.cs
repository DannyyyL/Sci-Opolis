//Author: Dan Lichtin
//File Name: UpgradeTree.cs
//Project Name: PASS3
//Creation Date: December 31, 2022
//Modified Date: January 22, 2023
//Description: Holds the root upgrade node, allows for certain upgrades to be locked
//depending on wether the parent node has been unlocked or not
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//PROOF OF CONCEPT:
//Binary Tree - This is a binary tree; carries the root and the amount of nodes in the tree;
//Recursion (Not originally part of the 5 concepts to be proven) - Recursivley loops through the nodes that the tree has and 
//acceses different paths the tree has

namespace PASS3___Grade_12
{
    class UpgradeTree
    {
        //Storing root
        private UpgradeNode root;

        //Tree data
        private int count;
        private SpriteFont upgradeFont;

        //Action identifier
        private const int UPDATE = 0;
        private const int DRAW = 1;

        public UpgradeTree(SpriteFont upgradeFont)
        {
            this.upgradeFont = upgradeFont;

            //Setting size
            count = 0;
        }

        //Pre: Graphics device, an image of the upgrade (texture2d), the cost (int), the scale the image will be printed with (float),
        //the description of the upgrade, and wether or not the upgrade is locked/unlocked (bool)
        //Post: None
        //Desc: Adds an upgrade node to the tree; depending on it's cost, it puts it on the correct branch;
        //Also Automatially creates a position for the upgrade icon to be drawn in depending on it's parent location;
        //if it's the root, then it will automatically center the node;
        public void AddUpgradeNode(GraphicsDevice gd, Texture2D upgradeImg, int cost, 
            float scaler, string upgradeDesc, bool unlocked)
        {
            //The node to be added
            UpgradeNode node;

            //Depending on how many nodes the tree has, add the node in it's correct location
            if (count == 0)
            {
                //Setting the new node as a root and adding to the tree count
                node = new UpgradeNode(gd, upgradeImg, upgradeFont, new Rectangle(Game1.screenWidth / 2 - upgradeImg.Width / 2, Game1.screenHeight / 14,
                    (int)(upgradeImg.Width * scaler), (int)(upgradeImg.Width * scaler)), cost, upgradeDesc, unlocked);
                root = node;
                count++;
            }
            else
            {
                //Start searching the tree with a root
                UpgradeNode curNode = root;
                
                //Bool to track wether or not the node is valid
                bool validNode = true;

                //Handles how much vertical space there is between each icon
                int heightSpacer = 150;
                int widthSpacer = 200;

                //Stay in this while loop until the validNode bool is false
                while (validNode)
                {
                    //Depending on the cost of the upgrade, traverse left or right (if the cost matches the current node; then the node is invalid)
                    if (cost > curNode.GetUpgradeCost())
                    {
                        //Access this statement if the node to the right of the current node is null
                        if (curNode.GetRight() == null)
                        {
                            //Adding the node to the tree and setting the right and the parent nodes
                            node = new UpgradeNode(gd, upgradeImg, upgradeFont, new Rectangle((int)curNode.GetUpgradeLoc().X + widthSpacer,
                                (int)curNode.GetUpgradeLoc().Y + heightSpacer, (int)(upgradeImg.Width * scaler),
                                (int)(upgradeImg.Width * scaler)), cost, upgradeDesc, unlocked);
                            curNode.SetRight(node);
                            curNode.GetRight().SetParent(curNode);

                            //Adding to the tree count
                            count++;
                            break;
                        }

                        //Setting the cur node to the right of the cur node
                        curNode = curNode.GetRight();
                    }
                    else if (cost < curNode.GetUpgradeCost())
                    {
                        //Access this statement if the node to the left of the current node is null
                        if (curNode.GetLeft() == null)
                        {
                            //Adding the node to the tree and setting the left and the parent nodes
                            node = new UpgradeNode(gd, upgradeImg, upgradeFont, new Rectangle((int)curNode.GetUpgradeLoc().X - widthSpacer,
                                (int)curNode.GetUpgradeLoc().Y + heightSpacer, (int)(upgradeImg.Width * scaler),
                                (int)(upgradeImg.Width * scaler)), cost, upgradeDesc, unlocked);
                            curNode.SetLeft(node);
                            curNode.GetLeft().SetParent(curNode);

                            //Adding to the tree count
                            count++;
                            break;
                        }

                        //Setting the cur node to the left of the cur node
                        curNode = curNode.GetLeft();
                    }
                    else
                    {
                        //Node isn't valid
                        validNode = false;
                    }
                }
            }
        }

        //Pre: SpriteBatch to draw, an action identifier (int), a posiition (point), and a button state
        //Post: None
        //Desc: Handles update logic and draw logic depending on the action parameter put in
        public void DrawUpdateTree(SpriteBatch spriteBatch, int action, Point position, ButtonState buttonState)
        {
            //Access this statement if the tree has a root
            if (root != null)
            {
                //Depending on the action paramater, draw or update the node
                if (action == DRAW)
                {
                    root.DrawUpgradeNode(spriteBatch);
                }
                else
                {
                    root.UpdateUpgradeNode(position, buttonState);
                }
            }
            
            //Defaulting all nodes to null
            UpgradeNode left = null;
            UpgradeNode right = null;

            //If the right of the root isnt null, equal the right node to it
            if (root.GetRight() != null)
            {
                right = root.GetRight();

                //Depending on the action paramater, draw or update the node
                if (action == DRAW)
                {
                    right.DrawUpgradeNode(spriteBatch);
                }
                else
                {
                    right.UpdateUpgradeNode(position, buttonState);
                }
            }

            //If the left of the root isnt null, equal the left node to it
            if (root.GetLeft() != null)
            {
                left = root.GetLeft();

                //Depending on the action paramater, draw or update the node
                if (action == DRAW)
                {
                    left.DrawUpgradeNode(spriteBatch);
                }
                else
                {
                    left.UpdateUpgradeNode(position, buttonState);
                }
            }

            //Call out to a dif function that can handle two paths
            //(drawing/updating the left's left and right, and the right's left and right)
            DrawUpdateNodeLeftRight(spriteBatch, left, right, action, position, buttonState);
        }

        //Pre: SpriteBatch to draw, a left and right node (upgrade nodes), an action identifier (int),
        //a posiition (point), and a button state
        //Post: None
        //Desc: Handles update logic and draw logic depending on the action parameter put in, and also recursivley calls this same function (if needed)
        public void DrawUpdateNodeLeftRight(SpriteBatch spriteBatch, UpgradeNode node1, UpgradeNode node2, int action, Point position, ButtonState buttonState)
        {
            //Node1 is the left node
            //Node2 is the right node
            //Defaulting all nodes to null
            UpgradeNode node1left = null;
            UpgradeNode node1right = null;
            UpgradeNode node2left = null;
            UpgradeNode node2right = null;

            //Access this statement if node1 isn't null
            if (node1 != null)
            {
                //Access both or one depending on if their respective left/right node isn't null
                if (node1.GetRight() != null)
                {
                    //equal node1right to the right of node1
                    node1right = node1.GetRight();

                    //Depending on the action paramater, draw or update the node
                    if (action == DRAW)
                    {
                        node1right.DrawUpgradeNode(spriteBatch);
                    }
                    else
                    {
                        node1right.UpdateUpgradeNode(position, buttonState);
                    }
                }
                if (node1.GetLeft() != null)
                {
                    //equal node1left to the left of node1
                    node1left = node1.GetLeft();

                    //Depending on the action paramater, draw or update the node
                    if (action == DRAW)
                    {
                        node1left.DrawUpgradeNode(spriteBatch);
                    }
                    else
                    {
                        node1left.UpdateUpgradeNode(position, buttonState);
                    }
                }

                //Recursivley call the function
                DrawUpdateNodeLeftRight(spriteBatch, node2left, node2right, action, position, buttonState);
            }

            //Access this statement if node2 isn't null
            if (node2 != null)
            {
                //Access both or one depending on if their respective left/right node isn't null
                if (node2.GetRight() != null)
                {
                    //equal node2right to the right of node 2
                    node2right = node2.GetRight();

                    //Depending on the action paramater, draw or update the node
                    if (action == DRAW)
                    {
                        node2right.DrawUpgradeNode(spriteBatch);
                    }
                    else
                    {
                        node2right.UpdateUpgradeNode(position, buttonState);
                    }
                }
                if (node2.GetLeft() != null)
                {
                    //equal node2left to the left of node 2
                    node2left = node2.GetLeft();

                    //Depending on the action paramater, draw or update the node
                    if (action == DRAW)
                    {
                        node2left.DrawUpgradeNode(spriteBatch);
                    }
                    else
                    {
                        node2left.UpdateUpgradeNode(position, buttonState);
                    }
                }

                //Recursivley call the function
                DrawUpdateNodeLeftRight(spriteBatch, node2left, node2right, action, position, buttonState);
            }

        }

    }
}
