<h1 align="center">Totally accurate kitchen simulator game</a> </h1>
<div class="markdown-heading">
  <p align="center">
    <img src="https://github.com/user-attachments/assets/f0b10f34-0e96-4430-8197-efe20a5ad95c" width="400" height="300" >
  </p>
</div>
<div>
  <h2>What the project?</h2> 
    <p>Kitchen game is a multiplayer game for up to 4 player in overcooked style written using Unity Game engine.</p>
  <h2>Main functionality</h2>
    <p>Game includes solo- and multiplayer gamemodes, lobby system using unity lobby service and allows to connect mupltiple players accross internet without restrictions of local-network like NAT, masking and other. This achived using Unity Relay service.</p>
</div>
<div>
  <h2>What technology used?</h2> 
  <p>
    Game built in Entity Component architecture. This is default architecture for unity game development, that uses MonoBehaviour components for game objects. That means every gameobject include self data, method update that called every game frame and exposure API for other game objects to allow interaction between them. 
  </p>
</div>
<div>
  <h2>How created multiplayer?</h2>
  <p>
    Network communication written using unity Netcode. Primarily that uses RpcMethods and NetworkVariables.
  </p>
</div>
<div>
  <h2>How works lobby system?</h2>
  <p>
    Lobby created using Unity Lobby Service. That allows default behaviour like: 
    <ul>
      <li>Create (public or private lobby)</li>
      <li>Join lobby by code</li>
      <li>Quick join first available</li>
      <li>Join lobby using list of available lobbies.</li>
    </ul>
  </p> 
  <p align="center">    
    <img src="https://github.com/user-attachments/assets/fe6d736f-bd3c-4524-9632-2c99ee96aa8f" align="center" width="800" height="300" >
  </p>
</div>
<div>
  <h2>Personal opinion on project</h2>
  <p>
    Main diffuculties in development in decising what data needed to syncronize or repeat and clients, divide methods in RPC methods by senders, and write asyncronous code implement request-response system. Decide which authority use, client or server to reach best responosive gameplay and restrict sensetive data for server.
  </p>
</div>
<div>
  <p>
    You could play my game. It's available in release tab in build archive. Unpack game and it ready to go in any gamemode you want.
  </p>
</div>
<div>
  <h2>Additional links</h2>
</div>
