# Sprint 4 Report 2/15/26 - 3/15/26
## YouTube Link: 

## What's New (User Facing)
* Users are able to provide a signature when checking out a loan.
* Manual specimen entry for the database is now available.
* The application has more tests integrated.
* The UI has a cleaner look.

## Work Summary (Developer Facing)
During this sprint, we prioritized preparing the application for a deployment to the client. We focused on the larger goals that would be needed such as the capability to support a signature for the user to verify a loan checkout. We also focused on the ability to manually add a specimen to the database, as we were no longer able to wait for an Arctos API key. The user interface also has more icons, looking more clean overall. We made test cases as well during this sprint.

## Unfinished Work
Since we prioritized features that would ensure good deployment, there were still a few features that we have yet to implement. For example, a manual specimen entry for a loan if for some reason scanning a QR code doesn’t work. We also would like to support exporting all specimen information alongside their QR codes in a document for easy printing.

## Completed Issues/User Stories
Here are links to the issues that we completed in this sprint:
* US-09
 - https://github.com/martinhundrup/Tag-and-Track/pull/48 
* US-12
 - https://github.com/martinhundrup/Tag-and-Track/pull/43 

## Incomplete Issues/User Stories
Here are links to issues we worked on but did not complete in this sprint:
* US-06
 - https://github.com/martinhundrup/Tag-and-Track/pull/44 
* US-11
 - https://github.com/martinhundrup/Tag-and-Track/pull/43 


## Code Files for Review
Please review the following code files, which were actively developed during this sprint, for quality:
* Signature
 - https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Components/SignaturePadView.cs 
* Manual entry to database
 - https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Pages/MainPages/AddItemPage.cs 
* Testing
 - https://github.com/martinhundrup/Tag-and-Track/tree/development/tests 

## Retrospective Summary
Here's what went well:
* Test integration.
* Signature and manual entry creation for the client.
* UI improvements.
* Bigfixes.

Here's what we'd like to improve:
* Fallbacks in case a QR code scan doesn’t work.
* Initial database integration (we may be able to import a csv with their specimens, eliminating the need for the tedious manual entry setup).

Here are changes we plan to implement in the next sprint:
* We want to finish up our stretch goals (exporting and importing)
* Addressing any bugs the client comes across through testing.
* Addressing any feedback from the testing during their testing of the application.