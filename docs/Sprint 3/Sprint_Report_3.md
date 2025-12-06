# Sprint 3 Report 11/5/25-12/5/25
## YouTube Link: https://youtu.be/B4jEnfD2o2U 

## What's New (User Facing)
* Data tables include required information as well as buttons depending on what information needs to be displayed.
* Users can create, finalize, and check in loans.
* Buttons in data tables can navigate to other pages or edit a loan.
* Users are able to receive emails when a loan is checked out.
* Pages now have a header that contains either a “home” button or the WSU logo (whichever is applicable for the page).

## Work Summary (Developer Facing)
During this sprint, the loan workflow, database, and user interface underwent many changes. The loan workflow was developed much further during this time. Users are now able to add specimens to loans, create new loans, edit pending loans, finalize the loan, and check in a loan all at once. The local database was further developed during this sprint, with temporary data and useful methods created and edited. With these changes, full integration with the local database will be near seamless once it is fully ready to replace the dummy test data. On top of this, the user interface had some work done. Data tables now display more information and can have buttons that allow the user to either view info or remove a specimen from a loan. There are also headers at the top of each page that allows for the user to navigate to the home page.

## Unfinished Work
Due to some technical difficulties the database, while much more developed, has yet to be integrated into the rest of the program. A goal for the upcoming sprint will be to get this database integrated much more. While the loan workflow is much more implemented now, we still need to give the user the capability to check in individual items for a loan instead of checking in the entire loan at once. The UI also still requires work, with some pages not having a fluid and professional UI.

## Completed Issues/User Stories
Here are links to the issues that we completed in this sprint:
* US-07
https://github.com/martinhundrup/Tag-and-Track/pull/25
* US-08
https://github.com/martinhundrup/Tag-and-Track/pull/25
* Emailer
https://github.com/martinhundrup/Tag-and-Track/pull/27
* Loan creation workflow
https://github.com/martinhundrup/Tag-and-Track/pull/28
https://github.com/martinhundrup/Tag-and-Track/pull/31
* Database
https://github.com/martinhundrup/Tag-and-Track/pull/29 
* UI - Headers
https://github.com/martinhundrup/Tag-and-Track/pull/24
* UI - Page development
https://github.com/martinhundrup/Tag-and-Track/pull/25
https://github.com/martinhundrup/Tag-and-Track/pull/26

## Incomplete Issues/User Stories
Here are links to issues we worked on but did not complete in this sprint:
* US-04
https://github.com/martinhundrup/Tag-and-Track/pull/31 
* US-08
https://github.com/martinhundrup/Tag-and-Track/pull/25
* US-09
https://github.com/martinhundrup/Tag-and-Track/pull/25
* US-12
https://github.com/martinhundrup/Tag-and-Track/pull/27

## Code Files for Review
Please review the following code files, which were actively developed during this sprint, for quality:
* Emailer
https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Utils/Emailer.cs
* Loan creator
https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Utils/LoanCreator.cs
* Item manager
https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Items/ItemManager.cs
* Database
https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Databases/Database.cs
* Finalize Loan page
https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Pages/SupportPages/FinalizeLoanPage.cs
View item page
https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Pages/SupportPages/ViewItemPage.cs 

## Retrospective Summary
Here's what went well:
* Loan workflow development.
* Database development.
* UI page improvements.
* QR code interactions with program.

Here's what we'd like to improve:
* Testing implementation.
* UI pages and icons.

Here are changes we plan to implement in the next sprint:
* Database integration.
* More efficient UI.
* Arctos API integration (if possible).
* More loan workflow improvements.