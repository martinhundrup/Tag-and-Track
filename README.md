# Tag & Track

## Project summary

### One-sentence description of the project
Tag & Track is a mobile application designed for the Conner Museum of Vertebrates to generate and scan QR codes that track specimen loans, locations, and histories while integrating with the museum’s existing Arctos database.

### Additional information about the project
Tag & Track modernizes the museum’s paper-based specimen loan system. The app will let staff generate labels containing QR codes, scan specimens to check them in or out, and record each transaction with borrower and staff information. It will interface read-only with the existing Arctos database to verify specimen records, while maintaining a local database for loan activity and physical-location tracking. The app will also support offline mode for rooms with poor connectivity, queueing scans until a stable connection is available. Automatic email receipts and digital loan sheets will streamline communication and archival processes.

## Installation

### Prerequisites
Development Requirements:
* Visual Studio Community with .NET Maui installed
* Git
* Access to test Arctos specimen data (read-only)

### Add-ons
* `ZXing.Net.Maui.Controls`
    * install with `dotnet add package ZXing.Net.Maui.Controls --version 0.4.0`
* `mobile_scanner` – for scanning QR/barcodes with the device camera
* `SQLite` – for managing the local offline database
* `emailer` – for sending automated email receipts
* `http` – for interfacing with the Arctos API

### Installation Steps

```
git clone https://github.com/martinhundrup/Tag-and-Track.git
cd Tag-and-Track/code/Tag-And-Track
dotnet restore
dotnet build
```

## Functionality
The current version of the app supports:
* Scanning QR codes
* Viewing specimens or other data
    * This data is part of a test dataset
* Navigating to other pages from the main page

## Known Problems
The team has identified the following challenges:
* Ensuring iPadOS deployment without App Store distribution
* Handling limited Wi-Fi connectivity in specimen storage rooms
* Designing a clean and intuitive interface for non-technical users
* Ensuring proper database synchronization between offline and online modes
* Loading of large amounts of data in an efficient amount of time
    * To replicate: try to load up a 1000 lines of data into a DataTable

## Contributing
1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

## Additional Documentation
* [Project Description](docs/Project_Description.pdf)
* [Requirements Document](docs/Tag_and_Track_Assignment1_Requirements.pdf)
* [Team Inventory](docs/Team_Inventory.pdf)
* [Sprint Reports](docs/sprints/)
* [Abstract](docs/abstract.pdf)

## Credits
<a href="https://www.flaticon.com/free-icons/info" title="info icons">Info icons created by Freepik - Flaticon</a>
<a href="https://www.flaticon.com/free-icons/cross" title="cross icons">Cross icons created by Ilham Fitrotul Hayat - Flaticon</a>
<a href="https://www.flaticon.com/free-icons/success" title="success icons">Success icons created by Ilham Fitrotul Hayat - Flaticon</a>
<a href="https://www.flaticon.com/free-icons/trash-can" title="trash can icons">Trash can icons created by Md Tanvirul Haque - Flaticon</a>

## License
MIT License. See `LICENSE.txt` for details.
