# Sprint 4 Report 1/20/26 - 2/17/26
## YouTube Link: 

## What's New (User Facing)
* Data tables have gained significant loading and visual improvements and are now searchable.
* Containers
* There is now a login page that is the first loaded page.
* Some icon implementation.

## Work Summary (Developer Facing)
During this sprint, the codebase went under a refactor. The refactor changed the page flow a bit and increased efficiency, the test database, and addressed potential memory issues. On top of this, a container view was made, the emailer updated, and the data tables underwent general improvements. Communication is being had to obtain an Arctos API key and it is suspected that we may be able to gain access to the Arctos data by the end of the next sprint.

## Unfinished Work
Due to the continuance of not having an API key, we still are unable to use legitimate data for the application. For now, we have a temporary database that will be used for testing purposes. If we still do not have the API key, we will likely go with a manual data entry that will take time to set up. We have more user interface improvements that we would like to make for the application. Since improving the data table’s efficiency was a main goal for this sprint, the UI work was not expanded far past that. We have a few additional exporting goals that we would like to address as well, such as exporting a csv of a loan or specimens and a document with all QR codes and specimen information needed to print and apply to specimens.

## Completed Issues/User Stories
Here are links to the issues that we completed in this sprint:
* US-08
- https://github.com/martinhundrup/Tag-and-Track/pull/37 
* US-07
- https://github.com/martinhundrup/Tag-and-Track/pull/37 
* Emailer
- https://github.com/martinhundrup/Tag-and-Track/pull/39 
* General improvements
- https://github.com/martinhundrup/Tag-and-Track/pull/33
- https://github.com/martinhundrup/Tag-and-Track/pull/34 

## Incomplete Issues/User Stories
Here are links to issues we worked on but did not complete in this sprint:
* US-12
- https://github.com/martinhundrup/Tag-and-Track/pull/39 
* US-09
- https://github.com/martinhundrup/Tag-and-Track/pull/33 


## Code Files for Review
Please review the following code files, which were actively developed during this sprint, for quality:
* Data base service:
- https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Data/DbService.cs 
* Entities:
- https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Data/Entities/ContainerEntity.cs 
- https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Data/Entities/EmployeeEntity.cs 
-https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Data/Entities/LoanEntity.cs
- https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Backend/Data/Entities/SpecimenEntity.cs
* User Interface:
- https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Components/DataTable/DataTable.cs
- https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Components/DataTable/DataTableColumn.cs
- https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Components/DataTable/DataTableColumnBuilder.cs
- https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Components/DataTable/FuncConverter.cs
- https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Pages/MainPages/LoginPage.cs
* Emailer
- https://github.com/martinhundrup/Tag-and-Track/blob/development/code/TagAndTrack/TagAndTrack/Pages/SupportPages/FinalizeLoanPage.cs 

## Retrospective Summary
Here's what went well:
* Loan workflow.
* Data flow improvements.
* Data Table improvements.
* Emailer improvements.

Here's what we'd like to improve:
* Data integration (or manual fallback if not possible).
* User interface needs more work.
* Exporting capabilities (csv and QR).

Here are changes we plan to implement in the next sprint:
* More UI improvements.
* Arctos API integration (if possible).
* More loan workflow/exporting improvements.
* Test case development.