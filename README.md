# Tag & Track

## Project summary

### One-sentence description of the project
Tag & Track is a mobile application designed for the Conner Museum of Vertebrates to generate and scan QR codes that track specimen loans, locations, and histories while integrating with the museum’s existing Arctos database.

### Additional information about the project
Tag & Track modernizes the museum’s paper-based specimen loan system. The app will let staff generate labels containing QR codes, scan specimens to check them in or out, and record each transaction with borrower and staff information. It will interface read-only with the existing Arctos database to verify specimen records, while maintaining a local database for loan activity and physical-location tracking. The app will also support offline mode for rooms with poor connectivity, queueing scans until a stable connection is available. Automatic email receipts and digital loan sheets will streamline communication and archival processes.

## Installation

### Prerequisites
Because the team has not yet begun implementation, these are projected requirements:
* Flutter SDK (latest stable version)
* Android Studio or Visual Studio Code with Flutter extension
* Git
* Access to test Arctos specimen data (read-only)

### Add-ons
Planned add-ons include:
* `qr_flutter` – for generating QR codes
* `mobile_scanner` – for scanning QR/barcodes with the device camera
* `sqflite` – for managing the local offline database
* `emailer` – for sending automated email receipts
* `http` – for interfacing with the Arctos API

### Installation Steps
Implementation has not yet started, but installation will likely follow these steps once development begins:

```
git clone https://github.com/martinhundrup/Tag-and-Track.git
cd Tag-and-Track
flutter pub get
flutter run
```

## Functionality
The first sprint focuses on design, requirements gathering, and planning. No functional code exists yet, but the initial version of the app will eventually support:
* Scanning QR codes to view or update specimen status
* Generating new QR labels for untagged specimens
* Checking specimens in and out and logging all transactions
* Sending automatic email receipts and saving digital loan sheets
* Operating offline and syncing data once connected
* Exporting transaction logs to CSV for upload to Arctos

## Known Problems
As of Sprint 1, no code has been written, but the team has identified potential challenges:
* Ensuring iPadOS deployment without App Store distribution
* Handling limited Wi-Fi connectivity in specimen storage rooms
* Designing a clean and intuitive interface for non-technical users
* Ensuring proper database synchronization between offline and online modes

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

## License
MIT License. See `LICENSE.txt` for details.
