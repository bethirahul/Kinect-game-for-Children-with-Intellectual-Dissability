This project was made by Rahul Bethi as a requirement for the final project of Masters degree in
Computer Science at Texas A&M University - Corpus Christi.

To build this game:

1. Download the the two folders 'Assets' and 'ProjectSettings' in to an empty folder, on a
Windows computer

2. Connect Kinect v2 to the computer using an adapter

3. Install Kinect SDK for Windows v2.0 (for drivers)
   https://www.microsoft.com/en-us/download/details.aspx?id=44561

4. Install Unity (free version 2017.1.0f3)
   https://unity3d.com/get-unity/download/archive?_ga=2.17157241.1560752389.1502840070-995122835.1494241648

5. Open Game2.unity (scene file) from the 'Assets' folder, which opens the project in Unity

Note: Libraries are automatically generated after the first run. Restart Unity to open the
project properly after the first run.

6. Press the Play button on the top-middle of the editor to start the game
   This will also force the editor to compile the code if it didn't

7. To build the game,
	
	Goto File -> Build Settings...
	Press Build button
	This will ask the location to save the game, give an empty folder and press Build.
	An executable file, and a Data dolder are created.
	They must be together in the same folder for the game to work.
	Open the executable file in that folder to run the game.
	

A build is already provided in the 'Game' folder, with the game's executable file and its data
folder

 - To edit the levels and add or delete levels.
   Data folder -> StreamingAssets -> Levels
   	Name of the level text files should in numeric and in assending order,
        starting with 1.txt

 - Similar to Levels, packages are also custumizable.
   Data folder -> StreamingAssets -> Packages
   	Packages are folders with numeric names in ascending order.
   	Any number of tree, bush and stone images can be added. A minimum of 1 each should be
   	present.
   	Similar folder and file name structure must be followed in all packages.

 - A minimum of one level text file and one package folder must be present.