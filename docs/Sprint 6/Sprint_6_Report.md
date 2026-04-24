# Sprint 6 Report 3/15/26 - 4/15/26
## YouTube Link: https://www.youtube.com/watch?v=dGELnfATYEE 

## What's New (User Facing)
* Checkout history of a specimen can now be viewed
* Specimens can be added to a loan without scanning QR codes
* Ability to import/export database csv’s
* Specimens can now be individually removed from the database
* Due date for loans is now available
* Minor textbox changes for better visibility
* Large tables are now filterable

## Work Summary (Developer Facing)
Much of the work done in this final sprint was meant to address feedback and finish any remaining tasks. One larger task was that the client wanted a way to import/export a csv of the database. This was created and tested successfully. Another functionality that was implemented this sprint was the ability to sort a table by whether or not something is checked out. It was also made possible to manually add specimen items to a loan. Instead of scanning a QR code, the user is able to pull up a page with a table where they can select all specimens they want to add to the loan. Some small changes were also made such as memory leaks being fixed and loans being able to be checked out without a signature if necessary.

## Unfinished Work
N/A, as this is the final sprint.

## Completed Issues/User Stories
Here are links to the issues that we completed in this sprint:
* US-06
 - https://github.com/martinhundrup/Tag-and-Track/pull/54
* US-09
 - https://github.com/martinhundrup/Tag-and-Track/pull/54 
* US-10
 - https://github.com/martinhundrup/Tag-and-Track/pull/54
* US-11
 - https://github.com/martinhundrup/Tag-and-Track/pull/54
* US-12
 - https://github.com/martinhundrup/Tag-and-Track/pull/54 

## Incomplete Issues/User Stories
All User Stories that could be completed were successfully completed during this sprint, so there are no incomplete issues/user stories.


## Code Files for Review
Please review the following code files, which were actively developed during this sprint, for quality:
* CSV import/export
 - https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Utils/CsvHelper.cs
 - https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Utils/CsvService.cs
 - https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Utils/SpecimenIdHelper.cs
* Checkout history
 - https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Pages/SupportPages/ViewItemPage.cs 
* Manual specimen entry for a loan
 - https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Pages/SupportPages/SelectSpecimensPage.cs
* Loan due dates
 - https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Pages/SupportPages/FinalizeLoanPage.cs 

## Retrospective Summary
Here's what went well:
* Improvements to the loan checkout structure (due date, manual entry)
* CSV import/export
* Addressing user feedback (UI element changes)
* Allowing for specimens to be added and removed from the database individually
* Showing checkout history of a specimen when looking at specimen details

Here's what we'd like to improve:
* Memory management
* Loading speeds

Here are changes we would have liked to implement given more time/resources:
* Integration with Arctos Database
