
# ROSE Online Login Manager

A WPF application used to automatically log in to [ROSE Online].

This project is being worked on for fun to practice and learn more about C#, databinding, MVVM design pattern, SQLite database, and encryption algorithms.

[ROSE Online]: https://www.roseonlinegame.com/
## FAQ

#### Where and how is my data being stored?

Your data is being stored within a local SQLite database and encrypted using AES with a randomly generated IV and Hardware ID as your key.

The database file, data.sqlite, is located within "%AppData%/Roaming/ROSE Online Login Manager".


## Planned Features
- Launch clients with specific screen coordinates
- Update ROSE client with current logged in account name and class
- Track if account is already logged in and display it within the profile list
- Mod Support
- Additional rose.toml settings


## Screenshots

![Main Menu Screenshot](https://i.imgur.com/RRNh0fU.png)

![Profile List Screenshot](https://i.imgur.com/lHiB0RH.png)

![Settings View Screenshot](https://i.imgur.com/uFm6xzd.png)

![DB Screenshot](https://i.imgur.com/rGvelwA.png)


