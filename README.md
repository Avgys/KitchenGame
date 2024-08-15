<h1 align="center">Hi there</a> </h1>
<br>
<div>
  <p align="center">
    <img src="https://habrastorage.org/r/w1560/getpro/habr/upload_files/b5e/aa4/b1a/b5eaa4b1ae8399cf4b599c3459572aa6.jpg" width="400" height="300" >
  </p>
  <p>
    I built unity multiplayer game for 4 player in overcooked style.
    Game includes solo- and multiplayer gamemodes, lobby system using unity lobby service and allows to connect mupltiple players accross internet without restrictions of local-network like NAT, masking and other. This achived using Unity Relay service.
  </p>

  <p>
    Game built in Entity Component architecture. This is default architecture for unity game development, that uses MonoBehaviour components for game objects. That means every gameobject include self data, method update that called every game frame and exposure API for other game objects to allow interaction between them. 
  </p>
  <p>
    Network communication written using unity Netcode. Primarily that uses RpcMethods and NetworkVariables.
  </p>
  <p>
    Lobby created using Unity Lobby Service. That allows default behaviour like: create (public or private lobby), join lobby by code, quick join first available or join lobby using list of available lobbies. 
  </p>
  <p>
    Main diffuculties in development in decising what data needed to syncronize or repeat and clients, divide methods in RPC methods by senders, and write asyncronous code implement request-response system. Decide which authority use, client or server to reach best responosive gameplay and restrict sensetive data for server.
  </p>
  <p>
    You could play my game. It's available in release tab in build archive. Unpack game and it ready to go in any gamemode you want.
  </p>
</div>
