---
page_type: sample
name: "Rock, Paper, Scissors, Lizard, Spock"
urlFragment: azure-rock-paper-scissors
description: "Rock, Paper, Scissors, Lizard, Spock is the geek version of the classic Rock, Paper, Scissors game."
languages:
- csharp
- powershell
- html
- php
- python
- javascript
- java
products:
- azure-cosmos-db
- azure-kubernetes-service
- dotnet-core
- azure-cognitive-services
- vs
- vs-code
azureDeploy: https://github.com/microsoft/RockPaperScissorsLizardSpock/blob/master/Deploy/arm/deployment.json
---

# Rock, Paper, Scissors, Lizard, Spock - Sample Multiplayer Application

Now you can enhance the game to play with a friend with the help of [Playfab platform](https://playfab.com/).
Playfab platform will be used only for the matchmaking queue logic and the leaderboard score.

## Setup
In order to support playfab you will need a platfab account with a game created (title in playfab)

1. Create a new Playfab account in the [sign up page](https://developer.playfab.com/en-us/sign-up) or if you already have one log in with it.
2. With the account creation as developer it creates a Title by default, if not create a new title. We named it RPSLS.

![](../Images/screen-playfab-title.png)

3. Open the title just created and navigate to title settings

![](../Images/screen-playfab-title-overview.png)

4. In the API Features tab select the "Allow client to post player statistics". Required to update leaderboards and matchmaking queues from code.

![](../Images/screen-playfab-title-api-features.png)

5. Copy the title Id from _Api features_ tab. Go to Secret Keys tab and copy the Secret key created by default. Both parameters are required for the RPSLS game-api service configuration, we will require them in the Generate-Config.ps1 script or on local development in the docker-compose.override.yml parameters.

6. Make sure that in the "Client Profile Options" tab have the client access "Display Name" allowed. Already checked by default.

![](../Images/screen-playfab-title-client-options.png)

The rest of title configurations for the matchmaking queue and the Leaderboard are created directly on code.