# Tag & Track

## Project summary

### One-sentence description of the project
Tag & Track is a mobile application designed for the Conner Museum of Vertebrates to generate and scan QR codes that track specimen loans, locations, and histories while integrating with the museum’s existing Arctos database.

### Additional information about the project
Tag & Track modernizes the museum’s paper-based specimen loan system. The app lets staff generate labels containing QR codes, scan specimens to check them in or out, and record each transaction with borrower and staff information. It was meant to interface read-only with the existing Arctos database to verify specimen records, however do to difficulties with Arctos, this feature was changed to be a manual addition of specimen. The app also supports an offline mode for rooms with poor connectivity, queueing scans until a stable connection is available. Automatic email receipts and digital loan sheets will streamline communication and archival processes.

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
* Creating loans
* Manual Specimen creation
* Manual Specimen addition to Loans
* Edit/View Containers
* Filter Specimen in the Specimen view (Checked in/out)
* View loan history
* View specimen loan history
* Finalize a loan (sends email to customer)

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
- <a href="https://www.flaticon.com/free-icons/info" title="info icons">Info icons created by Freepik - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/cross" title="cross icons">Cross icons created by Ilham Fitrotul Hayat - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/success" title="success icons">Success icons created by Ilham Fitrotul Hayat - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/trash-can" title="trash can icons">Trash can icons created by Md Tanvirul Haque - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/home-button" title="home button icons">Home button icons created by IconBaandar - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/magnifying-glass" title="magnifying glass icons">Magnifying glass icons created by chehuna - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/eyes" title="eyes icons">Eyes icons created by Kiranshastry - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/fish" title="fish icons">Fish icons created by Those Icons - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/add" title="add icons">Add icons created by Pixel perfect - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/login" title="login icons">Login icons created by Pixel perfect - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/settings" title="settings icons">Settings icons created by Freepik - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/logout" title="logout icons">Logout icons created by Pixel perfect - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/qr-code" title="qr code icons">Qr code icons created by Nadiinko - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/document" title="document icons">Document icons created by smalllikeart - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/history" title="history icons">History icons created by joalfa - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/stock" title="stock icons">Stock icons created by Nikita Golubev - Flaticon</a>
- <a href="https://www.flaticon.com/free-icons/day-and-night" title="day and night icons">Day and night icons created by rizal2109 - Flaticon</a>

## License
MIT License. See `LICENSE.txt` for details.
