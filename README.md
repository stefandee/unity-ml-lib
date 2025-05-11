# Unity Machine Learning Library

Goals of this project are:
* implement machine learning algorithms from scratch;
* create visualisations for these algorithms;
* use the algorithms to solve game development related problems;
* re-learn calculus :)

Long term goal is to create a "plug-and-play" AI for racing games, which is agnostic, as much as possible, of the game.

## Install&Usage

This project requires Unity 6, however, the core library can be used in earlier versions.

## Samples

### Multi Arm Bandit

Implements and visualises the [multi armed bandit problem](https://en.wikipedia.org/wiki/Multi-armed_bandit)

### Q Learning

Implements and visualises Q learning algoritm applied to finding an optimal path in a 1D and 2D grid world.

## Self Driving Cars (Reinforced Learning and Genetic Algorithms)

Implements feed forward neural networks and reinforced learning to have cars learn to drive any random track. 

Since optimisation/back-propagation is not yet implemented, the simulation manager uses a genetic algorithm-like approach to select the best cars.

The simulation might take some time to find a neural network (weights/biases) that can drive around the track.

Use WASD and right-mouse button to navigate and look around. Use left-mouse click to speed up movement (FreeCamera script).

An inspector is available in Tools > Piron Games > MLLib > Self Driving Cars > Car Inspector. Click on a car to inspect its parameters (currently lacks neural network visualisation).

![Reinforced Learning Self Driving Cars](.media/rlselfdrivingcars.gif "Reinforced Learning Self Driving Cars")

## TODO

Core library:
* implement an optimization algorithms (gradient descent)
* implement neural network back-propagation
* implement DQN
* documentation
* unit tests

Samples/Visualisation:

RL self driving cars:
* implement neural network visualisation in Car Inspector tool using GraphView
* make Car Inspector available for Canvas as well
* improved ability to manipulate neural network snapshots for the self driving cars simulation (copy/paste from one car to another, etc)
* experiment with short and long range sensors for the cars
* improve the simulation manager spawning (loitering -> from first waypoint, out of bounds -> from last waypoint)
* further experimentation for the track learning manager using genetic algorithms

## License

This project is licensed under [MIT license](https://opensource.org/license/mit).

Uses assets from [Kenney](https://kenney.nl/), which are licensed under [Creative Commons Zero](https://creativecommons.org/public-domain/cc0/).

Uses [Unity Vehicle Tools](https://github.com/Unity-Technologies/VehicleTools), which has no license.