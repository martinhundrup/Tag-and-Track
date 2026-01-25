# Sprint 2 Report 10/5/25-11/5/25
## YouTube link of Sprint 2 Video: https://youtu.be/I8pjIRLQ538

## What's New (User Facing)
* ButtonTemplate created and used in the main page.
* DataTemplate created and used.
* Dark Mode/Light Mode capabilities have been implemented.
* Buttons on main pages navigate to related pages.
* A few pages have some UI elements for page mock-ups.

## Work Summary (Developer Facing)
The team worked on the first steps of implementing the project. With the framework chosen and a Mac device that could be used for testing, we started with some basic functionality. Basic UI classes were made that are intended to be reused across the program as development continues. A few pages were given a rough UI as a result of this. QR scanning capabilities were created and started. A basic database was created for development use and testing until the Arctos API key can be obtained.

## Unfinished Work
We were unable to finish work on certain parts of the application. The specimen display feature was a part we were unable to complete fully. While we were able to get some work done by making the UI components and being able to put dummy data in, we lacked the time to finish these requirements. This was due to time constraints and our inability to use genuine data due to the fact that we still have yet to obtain the Arctos API key. We are planning on continuing our work on these aspects in the next sprint by using a local database or the Arctos database if we are able to get the correct data.

## Completed Issues/User Stories
Here are links to the issues that we completed in this sprint:
* Item View Pages: https://github.com/martinhundrup/Tag-and-Track/pull/21
* QR Code Generation: https://github.com/martinhundrup/Tag-and-Track/pull/13
* Support Ipad deployment: https://github.com/martinhundrup/Tag-and-Track/pull/8 
* Support cross-platform development: https://github.com/martinhundrup/Tag-and-Track/pull/9 
* US-03, Backend - QR code scanner: https://github.com/martinhundrup/Tag-and-Track/pull/11 
* UI - Creation of base pages: https://github.com/martinhundrup/Tag-and-Track/pull/9 
* UI - Button template: https://github.com/martinhundrup/Tag-and-Track/pull/10 
* UI - Textbox template: https://github.com/martinhundrup/Tag-and-Track/pull/14 
* UI - Dark/Light mode: https://github.com/martinhundrup/Tag-and-Track/pull/15 
* UI - Data table template: https://github.com/martinhundrup/Tag-and-Track/pull/16 

## Incomplete Issues/User Stories
Here are links to issues we worked on but did not complete in this sprint:
* US-05:
-https://github.com/martinhundrup/Tag-and-Track/pull/18/files 
	-https://github.com/martinhundrup/Tag-and-Track/pull/16 
* US-07:
-https://github.com/martinhundrup/Tag-and-Track/pull/18/files 
	-https://github.com/martinhundrup/Tag-and-Track/pull/16 
* US-08:
-https://github.com/martinhundrup/Tag-and-Track/pull/18/files 
-https://github.com/martinhundrup/Tag-and-Track/pull/16 

## Code Files for Review
Please review the following code files, which were actively developed during this sprint, for quality:
* MainPage (https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/MainPage.xaml.cs) 
* ScanItemsPage (https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Pages/ScanItemPage.cs)
* EmployeeManager (https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Employees/EmployeeManager.cs) 
* ItemManager.cs (https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Items/ItemManager.cs) 
* QRCodeView (https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Components/QrCodeView.cs) 
* TagAndTrackButton (https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Components/TagAndTrackButton.cs) 
* DataTableTemplate (https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Components/DataTableTemplate.cs) 
* CurrentTheme (https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Components/Themes/CurrentTheme.cs) 

## Retrospective Summary
Here's what went well:
* QR code scanning capabilities.
* Test database creation.
* UI component development.

Here's what we'd like to improve:
* Database integration.
* UI view.
* Testing implementation.

Here are changes we plan to implement in the next sprint:
* Test cases for backend/database work.
* Further QR responsibilities such as generation.
* Further implementation of backend and database interactions.
* More pages using UI components.
