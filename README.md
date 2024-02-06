# DOTS Blast

 This project was made using Unity Version 2022.3.18f1, using Unity's Entities package version 1.0.16

The project is a color matching blast game resembling games like WonderBlast or ToonBlast.

***When opening the project for the first time please make sure to load the GameScene scene with the subscene named GameEntityScene enabled and with Board game object as the child.***

The Board game object contains the settings for the game such as board width and height, available tile types(like red, blue etc.) and so on.

Since the project is made using DOP architecture, the files mainly consist of Datas and Systems.

There are also helper classes like authoring classes to help with the entity/gameobject communications and some other monobehaviour classes.

Datas contain datas such as block fall speed, grid positions and Tag components that are used by systems to query for the specific types of entities.

The project systems in execution order are:

BoardInitializationSystem:
This system handles the initial spawning of board elements.

PlayerInputSystem
This system handles the player input by detecting if and which block is clicked on and ands the block entity a "BlockClickedTag"

MatchFindingSystem
This system uses a floodfill algorithm to find similar colored blocks and boxes around the "ClickTag"ed block and adds them the "BlockPopTag" 

BlockPopingSystem
This system pops the BlockPopTagged entities meaning deals 1 damage to them. If the blocks hp is 0 they are destroyed.

BlockFallTargetAssignmentSystem
This system checks for each blocks representing column entity to find the empty slots underneath and if there are any empty slots adds the "BlockFallTag"

BlockFallMovementSystem
This system uses parallel jobs to move the blocks that have the "BlockFallTag"

BlockTypeAssignmentSystem
This system traverses the board using a floodfill algorithm and assigns block types to block so that their sprite changes according to the amount of same colored adjacent blocks.

BlockSpawningFromTopSystem
This system queries over the column entities to get the empty positions and spawn neceserry amount of Blocks from the top with the "BlockFallTag"

ShuffleSystem
This system check the board to find if any matches possible, if there aren't then it check if there is a solution possible with shuffle and if there is it uses a sorting algorithm to assign shuffle targets to the blocks. Due to the sorting algortihm used here, the board sometimes needs to get shuffled more than once but it should be rare occurance.

ShuffleMovementSystem
This system move the "ShuffleTag"ed blocks to their shuffle positions using parallel jobs.

and

BlockPresentationGOSystem
This system handles Block entity and Block Gameobject synchronizations. This was necessary due to the use of sprite renderer.

--------------------------------------------------------------------------------------------------------------------------------------------------------------

***Things that could be improved:***

Due to the lack of resources and time there are lots of things that could be better, to name a few:

* The Box creation code is just a place holder. I would like to make a level editor, use scriptable objects for board data and create the boxes accordingly. Currently there is only one box that spawns at a random grid position just to show case the health data working.
* The enums used for box type and sprites are not ideal. Tried to use bitwise operations instead but there were some issues so didn't want to waste more time on it.
* The systems are using UnityEngine.Random instead of a RandomStruct which renders these part of the code unable to use the burst compile.
* The usage of NativeCollections could be lessened.
* Some componentdatas could be merged.
* Could have used Job system more.
* The Shuffle System could be imporved.
* Could use pooling systems for block gameobjects.
* Could add some acceleration to the falling blocks.
* There a few parts against the DRY principle repeating the same code.
* The flood fill algorithm could maybe writen as a single struct file instead of repeated code in matchfinding and  type assigning systems.

All in all this project was really fun to build, thank you!

***Resizable Board:***
![](https://github.com/TrasqualInterviewCases/Blast/blob/main/2024-01-3018-13-32-ezgif.com-video-to-gif-converter%20(1).gif)


***Shuffle System:***
![](https://github.com/TrasqualInterviewCases/Blast/blob/main/2024-01-3018-12-34-ezgif.com-video-to-gif-converter.gif)
