## Set up your environment
In order to contribute to this project, you will need to set up your environment according to these specifications:

### Choice 1: Visual Studio 2017
- Someone else who actually uses VS 2017 should fill this in...

### Choice 2: Visual Studio Code
- Download and install Visual Studio Code
- Install the C# extension for VS Code (Identifier: `ms-vscode.csharp`)
    - If you are new to VS Code, reach the Extensions search box using Ctrl+Shift+X
- [Download and install the .NET SDK](https://www.microsoft.com/net/learn/get-started-with-dotnet-tutorial)
- Restart your computer

## Set up your git branch
Refer to THIS WIKI LINK

## Build the solution

### Choice 1: Visual Studio 2017
- Someone else who actually uses VS 2017 should fill this in...

### Choice 2: Visual Studio Code
- Use the built-in Terminal (Ctrl+\`)
- `cd` to the `ServerCore` folder
- Build using `dotnet run`

## Contributing

### Creating your own fork
If you wish to contribute changes back to the `mainpuzzleserver` repository, start by creating your own fork of the repository. This is essential. This will keep the number of branches on the main repository to a small count. This will also prevent mistakes in pushing upstream to the `mainpuzzleserver` repository. In your own fork, you can create as many branches as you like.

- Navigate to Github with a browser and login to your github account. For the sake of this document, lets assume your account is johndoe.
- Navigate to `mainpuzzleserver` repository in the same browser session.
- Click on the fork button at the top right corner of the page.
- Create the fork on your user name. Your github profile should now show mainpuzzleserver as one of your repositories.
- Create a folder on your device and clone your fork of the mainpuzzleserver repository. e.g. https://github.com/**johndoe**/mainpuzzleserver.git. Notice how your github user name is in the repository location.
```
    > git clone https://github.com/johndoe/mainpuzzleserver.git
```

### Setting up the upstream repository
Before starting to contribute changes, please setup your upstream repository to the primary `mainpuzzleserver` repository.

When you run `git remote -v`, you should see only your fork in the output list
```
> git remote -v

     origin  https://github.com/johndoe/mainpuzzleserver.git (fetch)

     origin  https://github.com/johndoe/mainpuzzleserver.git (push)
```
Map the primary `mainpuzzleserver` repository as the upstream remote
```
> git remote add upstream https://github.com/PuzzleServer/mainpuzzleserver.git
```
Now running `git remote -v` should show the upstream repository also
```
> git remote -v

     origin  https://github.com/johndoe/mainpuzzleserver.git (fetch)

     origin  https://github.com/johndoe/mainpuzzleserver.git (push)

     upstream        https://github.com/PuzzleServer/mainpuzzleserver.git (fetch)

     upstream        https://github.com/PuzzleServer/mainpuzzleserver.git (push)
```
At this point you are ready to start branching and contributing back changes!

### Merging upstream master into your fork master
From time to time, your fork will get out of sync with the upstream remote. Use the following commands to get your fork up up to date.
```
git fetch upstream
git checkout master
git pull upstream master
git push
```
## Merging upstream master into your current branch
From time to time, your current branch will get out of sync with the upstream remote. Use the following commands to get your branch up to date.
```
git fetch upstream
git pull upstream master
git push
```
