# Master-Server
Simple implementation of the master server with a menu.
***
To work, you need to create a new project and put the data folders with the files in it.

There are four scenes that you need to specify in the settings: Scene_Start, Scene_Menu, Scene_Game, Scene_Server;

**Scene_Start** - loads the settings and translation, then loads the menu scene (Scene_Menu), in which you can either create a game server, or join the server. When you create / join to the server, a game scene (Scene_Game) will be loaded.

The scene with the master server (Scene_Server) is launched in a separate application and it connects the game servers to the clients. Due to this, clients can find the server and then connect to them directly.
***
The network is built on the transport level Unity. As a player, a simple cube appears for the example. When placing the master server in scripts, you must specify its address accordingly.
***
*Author Gridnev Sergey Olegovich*
