# CONTRIBUTION GUIDELINES #

This guide is for ermeX tools developers. Not for final users

**_please feedback_**


## Solutions ##
Currently, ermeX has two solutions **both in Visual Studio 2010** :
  * _Main_ : Main is the main solution, which contains the code and the unit & integration tests
  * _AcceptanceTests_ : Contains the acceptance tests. This solution can be used until we have a proper Quick Start solution to illustrate the usage of the tool through the class **WorldGate** . Please, note that it needs the merged dll to be generated previously by using the release runner with the Compile option(See utils for developers)


## SOLUTION CONFIGURATIONS ##
Make sure you select one combination of the following before you compile from the IDE:
  * Platforms: x86, x64 (ANY\_CPU not available at the moment)
  * Frameworks: 4.0 Full
  * Configurations: Debug, Release


## Source control ##
ermeX uses subversion. You'll need a client like http://tortoisesvn.net/

### SVN Folders ###

  * blessed_this is the main trunk, and is intended to be used for merging/branching **ONLY** by the project admins
  * tasks\_branches_ it contains all the branches currently under development
  * attic_contains the branches created for tasks already merged. The purpose of this folder is to store the branch after merge until the task is verified.
  * wiki_ wiki
  * WorkingTrunks_Contains the trunk for each version where to branch from. the current version under development will contain the suffix **CURRENT**. Notice that all this trunks are branches from_blessed_* CI\_Backup_ contains the backups of the CI current configuration

---

## ISSUES & NEW REQUIREMENTS ##

  1. The issues page is only used to report defects and to collect feedback from users to be organized into iterations
  1. The new requirements and other discussions are discussed in the **FORUM**(TO BE CREATED)
  1. Develop only the task you have picked: Id another tasks identified you can create an issue on the issues page that will be translated to a proper task in further iterations
---

## WORKFLOW ##

  1. Request the current iteration tasks available list to the admin. **MANAGEMENT TO BE DISCUSSED IN THE FUTURE**
  1. Branch from the version specified in the task under _WorkingTrunks_ to _tasks\_branches_ with format: ISSUE-NUM\_TITLE\_OF\_THE\_ISSUE
  1. Develop in this branch. Merge from your source trunk frequently
  1. Commit your code to your branch _with comments_ using the format "ISSUE-NUM:" as prefix of the comment
  1. if your task is **MERGEABLE** _(see mergeable tasks below)_ Merge it to the source trunk under _WorkingTrunks_. In the case the source trunk is not the current, them merge to that one and merge the revision trunk by trunk until the current. Ensure every merge commit is **MERGEABLE**
  1. Move your branch to _attic_. It will stay there until is verified and it will be deleted by the project admin
---
## SVN COMMENTS ##
All the comments must have the prefix "ISSUE\_NUM:" and then action text: i.e. Branch created, branch commit, merged to trunk, task commit...
---

## WHAT IS A MERGEABLE TASK ##

A mergeable task is a task that after merged locally(Request info about how to run these in in your local machine in one step):
  * All tests run sucessfully in release mode for every platform
  * The Acceptance tests run sucessfully in your local machine
  * Other tasks run sucessfully(TODO)

A mergeable task can be commited to the version trunk under working trunks

If everything went fine, then you can merge your code

## UTILS FOR DEVELOPERS ##
  * SQL Server Setup: ermeX supports SQL Server as well as SQLite, for the SQL server tests you will need to create an account with full permissions and add the following environment variables:
    1. TEST\_SQL\_SERVER\_ADDRESS: i.e. localhost\sqlserver
    1. TEST\_SQL\_SERVER\_ADMIN\_LOGIN: the user you have created
    1. TEST\_SQL\_SERVER\_ADMIN\_PASSWORD: the password for the user
  * Run test from nUnit GUI: use links located at folder _Utilities_. note that you will need to compile the solution in one of the Configurations explained above.
  * Build tasks in local environment
    1. Files ReleaseRunnerFRAMEWORK_PLATFORM_.cmd
    1. Params _Compile_ or _Pack_ or _UnitTests_ or _AcceptanceTests_. The last two options need _Compile_ to be invoked previously

## Solution projects ##
  * The main solution is ermeX.sln, In this solution all the types but those needed for configuration and the WorldGate class are internal as we dont want to expose them to the final user