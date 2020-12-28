using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleTextAdventure
{
    class Player
    {
        public GameLoop gameLoop;
        public Zone currentZone;
        public List<Item> inventory = new List<Item>();
        public bool hasLightSource;
        public bool hasUsedRopeOnBalcony;
        public int staircaseDepth;

        public Player(Zone startingZone)
        {
            currentZone = startingZone;
            currentZone.playerHasVisited = true;
        }

        public void PrintInventory()
        {
            if (inventory.Count == 0)
            {
                Program.PrintWrappedText("You are carrying nothing.");
            }
            else if (inventory.Count == 1)
            {
                Program.PrintWrappedText("You are carrying 1 item: " + string.Join(", ", inventory));
            }
            else
            {
                Program.PrintWrappedText("You are carrying " + inventory.Count + " items: " + string.Join(", ", inventory));
            }
        }

        public void LookAction(Direction direction)
        {
            currentZone.PrintLook(direction, hasLightSource);
        }

        public void MoveAction(Direction direction)
        {
            switch (direction)
            {
                case Direction.Invalid:
                    Program.PrintWrappedText("Not a valid direction.");
                    break;
                case Direction.Here:
                    Program.PrintWrappedText("You must specify a direction to move.");
                    break;
                default:
                    if (currentZone.locks.ContainsKey(direction))
                    {
                        if (inventory.Contains(currentZone.locks[direction]))
                        {
                            Program.PrintWrappedText("You unlock the way with " + currentZone.locks[direction] + ".");
                            currentZone.UnlockExit(direction);
                        }
                        else
                        {
                            Program.PrintWrappedText("That way is locked. You will need something to unlock it.");
                            return;
                        }
                    }
                    if (currentZone.exits.ContainsKey(direction))
                    {
                        if (currentZone.codeName == "chamber" && direction == Direction.In)
                        {
                            bool holdingScepter = inventory.Find(x => x.codeName == "scepter") != default;
                            if (holdingScepter)
                            {
                                gameLoop.RemoveItemFromPlayer(inventory.Find(x => x.codeName == "scepter"), this);
                                Program.PrintWrappedText("The scepter disappears from your hands and the world changes around you as you walk through the archway.");
                            }
                            else
                            {
                                Program.PrintWrappedText("You walk through the archway. You remain in the same room.");
                                return;
                            }
                        }
                        else if (currentZone.codeName == "library" && direction == Direction.In)
                        {
                            bool holdingScepter = inventory.Find(x => x.codeName == "scepter") != default;
                            if (holdingScepter)
                            {
                                // TODO: End of game
                                Program.PrintWrappedText("[END GAME TEXT TO BE ADDED]");
                                Program.GameOver(0);
                            }
                            else
                            {
                                Program.PrintWrappedText("You walk through the archway. You remain in the same room.");
                                return;
                            }
                        }
                        else if (currentZone.codeName == "balcony" && direction == Direction.Down && !hasUsedRopeOnBalcony)
                        {
                            gameLoop.RemoveItemFromPlayer(inventory.Find(x => x.codeName == "rope"), this);
                            Program.PrintWrappedText("You tie one end of the rope to the balcony railing and toss the line down to the atrium floor below.");
                            currentZone.exits[Direction.Down].AddExit(Direction.Up, currentZone);
                            hasUsedRopeOnBalcony = true;
                        }
                        
                        Program.PrintWrappedText("You move " + direction + ". ");

                        if (currentZone.codeName == "alleyway" && direction == Direction.Up)
                        {
                            Program.PrintWrappedText("After you climb up the fire escape, the rusty hinges give way and the ladder falls to the ground far below.");
                            currentZone.examineText[0] = currentZone.examineText[0].Substring(0, currentZone.examineText[0].Length - 64) + "A fire escape ladder lies on the ground.";
                            Zone alleyway = currentZone;
                            currentZone = currentZone.exits[direction];
                            alleyway.exits.Remove(direction);
                        }
                        else
                        {
                            currentZone = currentZone.exits[direction];
                        }

                        Program.PrintWrappedText("You arrive at " + currentZone.name + ".");

                        if (currentZone.codeName == "staircase" && direction == Direction.Down)
                        {
                            staircaseDepth++;
                            if (staircaseDepth > 6)
                            {
                                Program.GameOver(2);
                            }
                            else if (staircaseDepth == 6 && hasLightSource)
                            {
                                Program.PrintWrappedText("Suddenly a chill runs through your body. You thought you saw a ghostly face out of the corner of your eye but when you looked you didn't see it. You feel like you are being watched.");
                            }
                            else if (staircaseDepth == 6)
                            {
                                Program.PrintWrappedText("Suddenly a chill runs through your body. You hear the voice of a small child somewhere further down the stairs, weakly crying for help.");
                            }
                        }
                        else if (currentZone.codeName == "office" && direction == Direction.Up)
                        {
                            if (staircaseDepth > 1)
                            {
                                Program.PrintWrappedText("The trip back up the staircase was shorter than the distance you went down.");
                            }
                            staircaseDepth = 0;
                        }

                        if (!currentZone.playerHasVisited)
                        {
                            currentZone.PrintExamineText(hasLightSource);
                            //if (!currentZone.isDark || hasLightSource)
                            //{
                            //    currentZone.playerHasVisited = true;
                            //}
                        }
                    }
                    else
                    {
                        Program.PrintWrappedText("You can't move that way.");
                    }
                    break;
            }
        }

        public void ExamineAction(Parameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Program.PrintWrappedText("You examine " + currentZone.name + ".");
                currentZone.PrintExamineText(hasLightSource);
            }
            else if (currentZone.isDark && !hasLightSource)
            {
                Program.PrintWrappedText("It is too dark to see anything.");
            }
            else
            {
                bool targetFound = false;
                if (currentZone.items.Count > 0)
                {
                    foreach (Item item in currentZone.items)
                    {
                        if (item.codeName == parameters[0].stringParameter)
                        {
                            item.PrintExamineText();
                            targetFound = true;
                        }
                    }
                }
                if (inventory.Count > 0)
                {
                    foreach (Item item in inventory)
                    {
                        if (item.codeName == parameters[0].stringParameter)
                        {
                            item.PrintExamineText();
                            targetFound = true;
                        }
                    }
                }
                if (currentZone.codeName == parameters[0].stringParameter || currentZone.codeName == parameters[0].directionParameter.ToString().ToLower())
                {
                    Program.PrintWrappedText("You examine " + currentZone.name + ".");
                    currentZone.PrintExamineText(hasLightSource);
                    targetFound = true;
                }
                foreach (KeyValuePair<Direction, Zone> exit in currentZone.exits)
                {
                    if (exit.Value.codeName == parameters[0].stringParameter)
                    {
                        Program.PrintWrappedText("You are too far away to examine " + exit.Value.name + ".");
                        targetFound = true;
                    }
                }
                if (!targetFound)
                {
                    Program.PrintWrappedText("Unrecognized target.");
                }
            }
        }

        bool TryFindTarget(Parameter[] parameters, out Parameter result)
        {
            if (parameters == null || parameters.Length == 0 || parameters[0].type != ParameterType.String)
            {
                result = new Parameter("");
                return false;
            }

            if (parameters[0].stringParameter == "all")
            {
                result = new Parameter("all");
                return true;
            }

            if (parameters[0].stringParameter == currentZone.name)
            {
                result = new Parameter(currentZone);
                return true;
            }

            foreach (KeyValuePair<Direction, Zone> exit in currentZone.exits)
            {
                if (exit.Value.codeName == parameters[0].stringParameter)
                {
                    result = new Parameter(currentZone.exits[exit.Key]);
                    return true;
                }
            }

            foreach (Item item in inventory)
            {
                if (item.codeName == parameters[0].stringParameter)
                {
                    result = new Parameter(item);
                    return true;
                }
            }

            foreach (Item item in currentZone.items)
            {
                if (item.codeName == parameters[0].stringParameter)
                {
                    result = new Parameter(item);
                    return true;
                }
            }

            result = parameters[0];
            return false;
        }

        bool TryFindTargetItems(Parameter[] parameters, out Parameter firstTarget, out Parameter secondTarget)
        {
            if (parameters.Length < 2)
            {
                firstTarget = new Parameter("");
                secondTarget = new Parameter("");
                return false;
            }
            else
            {
                Parameter[] secondParameterArray = new Parameter[] { parameters[1] };
                if (TryFindTarget(parameters, out Parameter firstParameter) && firstParameter.type == ParameterType.Item && TryFindTarget(secondParameterArray, out Parameter secondParameter) && secondParameter.type == ParameterType.Item)
                {
                    firstTarget = firstParameter;
                    secondTarget = secondParameter;
                    return true;
                }
                else
                {
                    firstTarget = new Parameter("");
                    secondTarget = new Parameter("");
                    return false;
                }
            }
        }

        List<Item> LocateItem(Item item)
        {
            if (inventory.Contains(item))
            {
                return inventory;
            }
            else if (currentZone.items.Contains(item))
            {
                return currentZone.items;
            }
            else return null;
        }

        public void TakeAction(Parameter[] parameters)
        {
            if (currentZone.isDark && !hasLightSource)
            {
                Program.PrintWrappedText("It is too dark to see anything.");
                return;
            }
            if (TryFindTarget(parameters, out Parameter target) && target.type == ParameterType.Item)
            {
                if (LocateItem(target.itemParameter) == currentZone.items)
                {
                    inventory.Add(target.itemParameter);
                    currentZone.items.Remove(target.itemParameter);
                    Program.PrintWrappedText("You take " + target.itemParameter.name + ".");
                }
                else if (inventory.Contains(target.itemParameter))
                {
                    Program.PrintWrappedText("You already have that item.");
                }
                else
                {
                    Program.PrintWrappedText("This message shouldn't ever appear in the game. What did you do?");
                }
            }
            else if (target.stringParameter == "all")
            {
                if (currentZone.items.Count > 0)
                {
                    while (currentZone.items.Count > 0)
                    {
                        inventory.Add(currentZone.items[0]);
                        currentZone.items.Remove(currentZone.items[0]);
                    }
                    Program.PrintWrappedText("You take all the items in this area.");
                }
                else
                {
                    Program.PrintWrappedText("There are no items to take.");
                }
            }
            else
            {
                Program.PrintWrappedText("Not a valid target.");
            }
        }

        public void DropAction(Parameter[] parameters)
        {
            if (currentZone.isDark && !hasLightSource)
            {
                Program.PrintWrappedText("It would be unwise to drop something in the dark.");
                return;
            }
            if (TryFindTarget(parameters, out Parameter target) && target.type == ParameterType.Item)
            {
                if (LocateItem(target.itemParameter) == inventory)
                {
                    if (currentZone.isDark && target.itemParameter.type == "Light")
                    {
                        Program.PrintWrappedText("It would be unwise to drop a light source in the dark.");
                    }
                    else
                    {
                        currentZone.items.Add(target.itemParameter);
                        inventory.Remove(target.itemParameter);
                        Program.PrintWrappedText("You drop " + target.itemParameter.name + ".");
                    }
                }
                else
                {
                    Program.PrintWrappedText("You aren't carrying that item.");
                }
            }
            else if (target.stringParameter == "all")
            {
                if (currentZone.isDark)
                {
                    Program.PrintWrappedText("It would be unwise to drop everything in the dark.");
                }
                if (inventory.Count > 0)
                {
                    while (inventory.Count > 0)
                    {
                        currentZone.items.Add(inventory[0]);
                        inventory.Remove(inventory[0]);
                    }
                    Program.PrintWrappedText("You drop all the items you were carrying.");
                }
                else
                {
                    Program.PrintWrappedText("You are carrying no items.");
                }
            }
            else
            {
                Program.PrintWrappedText("Not a valid target.");
            }
        }

        public void UseAction(Parameter[] parameters)
        {
            if (TryFindTarget(parameters, out Parameter target) && target.type == ParameterType.Item)
            {
                if (LocateItem(target.itemParameter) == inventory)
                {
                    if (target.itemParameter.type == "Key")
                    {
                        if (currentZone.locks.ContainsValue(target.itemParameter))
                        {
                            Direction lockDirection = currentZone.locks.First(x => x.Value == target.itemParameter).Key;
                            currentZone.UnlockExit(lockDirection);
                            Program.PrintWrappedText("You unlock the way in the direction of " + lockDirection + ".");
                        }
                        else
                        {
                            Program.PrintWrappedText("Nothing interesting happens.");
                        }
                        return;
                    }
                    target.itemParameter.UseItem(inventory);
                    if (target.itemParameter.type == "Light")
                    {
                        hasLightSource = (target.itemParameter as Light).isActive;
                        if (currentZone.isDark && !currentZone.playerHasVisited)
                            currentZone.PrintExamineText(hasLightSource);
                    }
                }
                else
                {
                    Program.PrintWrappedText("You aren't carrying that item.");
                }
            }
            else
            {
                Program.PrintWrappedText("Not a valid target.");
            }
        }

        public void CombineAction(Parameter[] parameters)
        {
            if (TryFindTargetItems(parameters, out Parameter firstTarget, out Parameter secondTarget))
            {
                if (inventory.Contains(firstTarget.itemParameter) && inventory.Contains(secondTarget.itemParameter))
                {
                    firstTarget.itemParameter.CombineItem(inventory, secondTarget.itemParameter);
                }
                else
                {
                    Program.PrintWrappedText("You are not holding those items.");
                }
            }
            else
            {
                Program.PrintWrappedText("You must specify two items to combine.");
            }
        }
    }
}
