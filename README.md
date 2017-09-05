# 7DTD-SingleMapRenderer

A very simple SinglePlayer Map Renderer for 7 Days To Die.

Visit https://7daystodie.com/forums/showthread.php?30280-SinglePlayer-Map-Renderer for more information and discussion.

## Usage

### GUI

TODO

### Command line

7DTD-SingleMapRenderer.exe /? will print the following help:

```
  /m   or /map=<value>           Path to the map file
  /i   or /image=<value>         Path to the image file
  /p   or /poi=<value>           Path to the POI file
  /ts  or /tilesize=<value>      Size of each map tile (16, 8, 4, 2, 1)
  /bg  or /background            Draws background from in-game map
  /g   or /grid                  Draws grid
  /gs  or /gridsize=<value>      Size of a grid cell
  /gc  or /gridcolor=<value>     Name of the grid color
  /ga  or /gridalpha=<value>     Alpha value of the grid color
  /rn  or /region                Draws region numbers
  /rfn or /regfontname=<value>   Name of font for region numbers
  /rfs or /regfontsize=<value>   Size of font for region numbers
  /w   or /waypoints             Draws waypoints
  /wfn or /wayfontname=<value>   Name of font for waypoint names
  /wfs or /wayfontsize=<value>   Size of font for waypoint names
  /wfc or /wayfontcolor=<value>  Name of the waypoint font color
  /ds  or /datastore             Uses a DataStore for all map tiles

Only the map-switch is required. All other switches are optional.

Example:
7DTD-SingleMapRenderer.exe /map="%appdata%\7DaysToDie\Saves\<Path to file>.map" /i="%userprofile%\Desktop\map.png" /bg
Will render the map to your desktop with the in-game background.
```

# Kudos

Inspiration for this project
* https://github.com/nicolas-f/7DTD-leaflet

Writing an awesome library for reading/writing a player's profile
* https://github.com/Karlovsky120/7DaysProfileEditor
