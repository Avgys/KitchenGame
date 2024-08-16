<h1> Totally accurate kitchen simulator game</h1>
<p align="center">
<img src="https://github.com/user-attachments/assets/f0b10f34-0e96-4430-8197-efe20a5ad95c" width="600px">
</p>
    
<h2>About project</h2> 

Kitchen game is a multiplayer game for up to 4 playerы in [Overcooked](https://store.steampowered.com/app/448510/overcooked/) game style written using Unity game engine. Main goal of the game is to cook dishes via combining ingredients and deliver them to customers to gain game points in limited time frame.

https://private-user-images.githubusercontent.com/61652541/358651294-e48a085a-d164-4bd5-a569-63c7d635e871.mp4
  
<h2>Main functionality</h2>

Game includes solo- and multiplayer gamemodes, lobby system using [Unity Lobby Service](https://docs.unity.com/ugs/manual/lobby/manual/unity-lobby-service) and allows to connect mupltiple players accross internet without restrictions of local-network like NAT, masking and other. This achived using [Relay Unity Service](https://docs.unity.com/ugs/manual/relay/manual/introduction).

<h2>What main approaches used?</h2> 

Game built according to Entity Component architecture, the default architecture for unity game development, that uses MonoBehaviour components for GameObjects. That means every gameobject includes data about it self, method update that called every game frame and gives API for other GameObjects to allow interaction.

<h2>How works multiplayer?</h2>

Network communication written using Unity [Netcode](https://docs-multiplayer.unity3d.com/netcode/current/about/). Primarily that uses [RpcMethods](https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/rpc/) and [NetworkVariables](https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable/) that auto synchronize between clients.

Connection uses Unity built-in [Network Manager](https://docs-multiplayer.unity3d.com/netcode/current/components/networkmanager/) that allows connect using ip address directly to server, but considering NAT technology for regular user that allows connect only in local area. There comes [Relay Unity Service](https://docs.unity.com/ugs/manual/relay/manual/introduction) that manages connection between players in Global network using Proxy servers to allow Peer-2-Peer connection.

<h2>How works lobby system?</h2>

Lobby created using [Unity Lobby Service](https://docs.unity.com/ugs/manual/lobby/manual/unity-lobby-service). That allows default behaviour like: 
<ul>
  <li>Create (public or private lobby)</li>
  <li>Join lobby by code</li>
  <li>Quick join first available</li>
  <li>Join lobby using list of available lobbies.</li>
</ul>

Also lobbies work separate on https protocolcs using polling for data update and used only to find players and team up, as the game start lobby closees and players connect to server via UDP P2P connection over proxy server.

![LobbyStatistic](https://github.com/user-attachments/assets/19a4e5e5-aa51-4181-b0da-f3a5d94f59af)

https://github.com/user-attachments/assets/70cf14e6-bea4-4970-86c6-2631e7cbcfa5

<h2>Personal opinion on project</h2>

Main diffuculties during development was:
<ul>
  <li>decising between directly synchronize behaviour between clients and send minimal data to repeat actions with more responsiveness</li>
  <li>divide methods in RPC methods by senders and consider client and server authoritives</li>
  <li>Decide which authority to use, client or server to reach best responsive gameflow and hide sensetive data for clients</li>
  <li>write asyncronous code implement request-response system</li>
</ul>
