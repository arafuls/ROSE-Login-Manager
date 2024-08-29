
# ROSE Online Login Manager
ROSE Login Manager is a utility designed to automate and streamline the process of logging into [ROSE Online] accounts. It provides features to manage multiple accounts, automate login procedures, and manage log files for troubleshooting and monitoring.

This project is being worked on for fun to practice and learn more about C#, databinding, MVVM design pattern, SQLite database, encryption algorithms, and memory hacking.

[ROSE Online]: https://www.roseonlinegame.com/


## FAQ
#### Where and how is my data being stored?
Your data is being stored within a local SQLite database and encrypted using AES with a randomly generated IV and Hardware ID as your key.
The database file, data.sqlite, is located within "%AppData%/Roaming/ROSE Online Login Manager".


## Features
- **Account Management**:
  - Create, Modify, and Delete profile accounts for logging into ROSE Online.
  - Store and manage multiple ROSE Online accounts securely.
  - Account information is encrypted using AES.
  
- **Automatic Functions**:
  - **Automatic Login**: Quickly log into ROSE Online accounts with saved credentials.
  - **Automatic Patching**: Checks on start-up for the latest game version and updates automatically if needed.
  - **Automatic Game Locating**: Finds the ROSE install location using the Windows Registry (thanks to ZeroPoke!).
  
- **User Interface Enhancements**:
  - **One-Click Launch**: Quickly start ROSE from the main menu.
  - **Drag and Drop**: Reorganize Profile Cards with drag-and-drop functionality.
  - **Integrated Updater**: ROSE-Updater behavior with a progress bar for seamless updates.
  
- **Client Settings Management**:
  - Manage ROSE client settings, including `rose.toml` file settings.
    - Toggle the Skip Planet Cutscenes option.
  - **Change Login Screen Preferences**:
    - Random
    - Treehouse
    - Adventure Plains
    - Junon Polis
  
- **Email Address Management**:
  - Display and mask email addresses as needed.
- **Client Behavior Customization**:
  - Launch ROSE behind the client to keep it unobtrusive.
  
- **Advanced Features**:
  - **Game Memory Reading**: Access game memory data to overwrite client-side data, such as updating the Window Title with your active character name.
  - **Logging and Debugging**: Logging using NLog to capture and debug issues, with an Event Log View for real-time log statements.


## Upcoming Planned Features
- TCP packet sniffing for price aggregation of player-run shops
- Launch clients with specific screen coordinates
- Mod support


## Installation
See **Releases** for detailed installations instructions. 


## Screenshots
![Main Menu Screenshot](https://i.imgur.com/RRNh0fU.png)
![Settings View Screenshot](https://i.imgur.com/uFm6xzd.png)
![DB Screenshot](https://i.imgur.com/rGvelwA.png)


