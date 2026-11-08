# Identity and Access

This section explains how people sign in, how PuzzleServer decides what they
can do, and where to look when changing or troubleshooting that behavior.

**Authentication asks "Who are you?" Authorization asks "Are you allowed to do
this?"** Signing in does not automatically register someone for an event, put
them on a team, or make them an author or administrator.

## Where to start

| Guide | What you'll find |
| --- | --- |
| [Sign-in and Account Lifecycle](sign-in-and-account-lifecycle.md) | The login flow, account/profile records, framework defaults, and which account features are complete. |
| [Authorization and Permissions](authorization-and-permissions.md) | The permission rules, URL roles, puzzle access, and checks outside named policies. |
| [Development and Troubleshooting](development-and-troubleshooting.md) | Configuration settings, change recipes, common failures, and testing limitations. |

For general environment setup, use the existing
[First time Setup wiki page](https://github.com/PuzzleServer/mainpuzzleserver/wiki/First-time-set-up).

## How to read these guides

We distinguish three kinds of behavior:

| Label | Meaning |
| --- | --- |
| **Framework** | Behavior provided by ASP.NET Core unless the application overrides it. |
| **PuzzleServer** | Our application code and explicit overrides. |
| **Team choice** | Something we can change through configuration, administrative data, or code. The guides say which kind of change is needed. |

The app targets `net10.0`; Identity UI and Microsoft Account packages are pinned
to `10.0.5` in [ServerCore.csproj](../../ServerCore/ServerCore.csproj). Framework
source links use that release. A deployment's shared-framework patch can differ,
so recheck defaults when upgrading.

## Code map

| Code | Responsibility |
| --- | --- |
| [Program.cs](../../ServerCore/Program.cs), [Startup.cs](../../ServerCore/Startup.cs) | Start the app; register Microsoft login, policies, options, and middleware. |
| [IdentityHostingStartup.cs](../../ServerCore/Areas/Identity/IdentityHostingStartup.cs) | Register Identity services and database-backed account storage. |
| [Account pages](../../ServerCore/Areas/Identity/Pages/Account) | Login, account creation, logout, and account management. |
| [PuzzleUser.cs](../../Data/DataModel/PuzzleUser.cs), [PuzzleServerContext.cs](../../Data/PuzzleServerContext.cs) | Application profiles and the database containing both account and puzzle data. |
| [UserAuthorizationPolicy](../../ServerCore/Areas/Identity/UserAuthorizationPolicy) | Named permission requirements, handlers, and `AuthorizationHelper`. |
| [EventRole.cs](../../ServerCore/ModelBases/EventRole.cs), [EventSpecificPageModel.cs](../../ServerCore/ModelBases/EventSpecificPageModel.cs) | URL operating modes, event binding, and shared page checks. |
| [UserEventHelper.cs](../../ServerCore/Helpers/UserEventHelper.cs) | Team lookup and puzzle-author/admin relationships. |

**Edit models in `Data/`, not `ServerCore/DataModel/`.** The app excludes the
older `ServerCore/Data/` and `ServerCore/DataModel/` files from compilation and
references the separate `Data` project.

## Publishing to the wiki

These files are reviewed in the main repository. GitHub wikis do not support
native pull requests, and merging this documentation does not update the wiki.

| File in this directory | Wiki page title |
| --- | --- |
| `README.md` | Identity and Access |
| `sign-in-and-account-lifecycle.md` | Identity and Access - Sign-in and Account Lifecycle |
| `authorization-and-permissions.md` | Identity and Access - Authorization and Permissions |
| `development-and-troubleshooting.md` | Identity and Access - Development and Troubleshooting |

When publishing, add an **Identity and Access** section to the wiki home page.
Convert relative guide links to wiki links and source links to main-repository
URLs. Preserve existing setup and technology links.
