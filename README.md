
# ROSE Online Login Manager

A WPF application used to automatically log in to [ROSE Online].

This project is being worked on for fun to practice and learn more about C#, databinding, MVVM design pattern, SQLite database, and encryption algorithms.

[ROSE Online]: https://www.roseonlinegame.com/
## FAQ

#### Where and how is my data being stored?

Your data is being stored within a local SQLite database and encrypted using AES with a randomly generated IV and Hardware ID as your key.

The database file, data.sqlite, is located within "%AppData%/Roaming/ROSE Online Login Manager".


## Planned Features

- Maximum Client Limit: 2 (per ROSE Guidelines)
- Party system to launch paired accounts together with a single click
- Launch clients with specific screen coordinates
- Update ROSE client with current logged in account name and class
- Track if account is already logged in and display it within the profile list
- Automatically update using the official ROSE Online Updater
- Display game announcements similar to splash page of the official ROSE Online Updater


## Screenshots

![Main Menu Screenshot](https://i.imgur.com/2PbQHdW.png)

![Profile List Screenshot](https://i.imgur.com/sERTl1y.png)

![DB Screenshot](https://i.imgur.com/rGvelwA.png)


