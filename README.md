
# ROSE Online Login Manager
ROSE Login Manager is a utility designed to automate and streamline the process of logging into [ROSE Online] accounts. It provides features to manage multiple accounts, automate login procedures, and manage log files for troubleshooting and monitoring.

This project is being worked on for fun to practice and learn more about C#, databinding, MVVM design pattern, SQLite database, encryption algorithms, and memory hacking.

[ROSE Online]: https://www.roseonlinegame.com/


## FAQ
#### Where and how is my data being stored?
Your data is being stored within a local SQLite database and encrypted using AES with a randomly generated IV and Hardware ID as your key.
The database file, data.sqlite, is located within "%AppData%/Roaming/ROSE Online Login Manager".


## Features
- **Automatic Login**: Quickly log into ROSE Online accounts with saved credentials.
- **Automatic Patching**: Checks on start-up for latest game version and updates automatically if needed.
- **Automatic Game Locating**: Attempts to find the game directory for you using the Windows Registry.
- **Account Management**: Store and manage multiple ROSE Online accounts securely.
- **Customizable Settings**: Adjust settings for the ROSE Client, `rose.toml` file settings through the client along with various other settings.
- **Game Memory Reading**: Option to access game memory data for client-side writing purposes.
- **Log Viewer**: Monitor and review login attempts and errors with a comprehensive log viewer.


## Upcoming Planned Features
- TCP packet sniffing for price aggregation of player-run shops
- Launch clients with specific screen coordinates
- Mod support


## Installation
**Prerequisites**
- .NET 8 Runtime
- ROSE Online installed and properly configured


## Screenshots
![Main Menu Screenshot](https://i.imgur.com/RRNh0fU.png)
![Settings View Screenshot](https://i.imgur.com/uFm6xzd.png)
![DB Screenshot](https://i.imgur.com/rGvelwA.png)


