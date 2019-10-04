# FourRooms2D
##### Unity Editor Version: 2018.3.1f1

This is a 2D version of the FourRooms Unity project, created for running experiments in an MRI scanner.
The gameplay is taken from my 3D repo FourRooms, with minor changes accounting for player control, the 3D nature of pickups and other gameobject interactions.


## Overview

- You will need to edit filepath.path in filepath.cs script for an appropriate  location for the log file.

- To run an experiment, you must start the game from the persistent scene, 'Persistent'
- The gameplay is controlled *almost* entirely from the singleton GameController.cs. There is some gameplay that is local to short scripts attached to objects in different scenes, but where possible these scripts trigger functions within GameController.cs to keep gameplay centralised and readable. A finite-state-machine tracks within-trial changes to the game state.

- The data management is operated through DataController.cs. Any configuration file that needs to be read or loaded, any online changes to trial list sequencing, and any saving of the data (to either the Summerfield Lab webserver or to a location on your local computer), is performed here. There is one instance of DataController that persists between scenes and is fetched/found by other smaller scripts when needed - so it is effectively a singleton but implemented slightly differently.

- The experiment configuration is controlled through the script ExperimentConfig.cs, which specifies the trial sequencing, randomisation, and experiment-specific controlled variables e.g. the duration and frequency of restbreaks.

- When playing the game, movements are controlled with the arrow keys. The space-bar is used to lift boulders.
