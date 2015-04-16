# Demon
A Demonstration for Transportation Simulation Using GMap.NET

2015 4 17 MengYue

Code For:
Simulation for traffic of taxi based on GMap.NET and MongoDB

HAVE DONE:
1.Fixed Multi Cars Timing Problem.
2.Successfully using mongo for store the data.(Just need to use mongoimport to import .csv to the database)
3.Write two bat file for setup database path and prepare for the pre work of mongo(import work is not included)

Problems:
1.Awful visual effect caused by too many cars.
2.Car all move too slow(because can't find a well solution to "find" record in the database by time, string type can't be one of the key for searching when serilizing)

UNFINISHED:
1.Chasing one car when choose it.
2.Draw the track of one car.
