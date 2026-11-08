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

## What to learn first

These are **reading priorities based on the code**, not measured usage rankings.
We have not analyzed production traffic. A feature can be used rarely and still
be important, especially when it controls privileged operations.

| Priority | Topics | Why this grouping |
| --- | --- | --- |
| **Core** | [Microsoft login and profile lookup](sign-in-and-account-lifecycle.md#follow-a-login); [event/team/puzzle permissions](authorization-and-permissions.md#policy-reference) | These support normal authenticated use. Learn them before changing player-facing behavior. |
| **Staff-specific** | Global/event administration, authorship, and [team impersonation](authorization-and-permissions.md#puzzle-access-and-impersonation) | These serve particular roles, rather than every player. They are critical when working on staff tools. |
| **Occasional / supporting** | [Profile edits and external-login management](sign-in-and-account-lifecycle.md#account-management-available-versus-unfinished); [provider-key recovery](sign-in-and-account-lifecycle.md#returning-users) | These are user-initiated or condition-triggered flows. Their actual frequency is unknown. |
| **Incomplete / not exposed in navigation** | Password/two-factor navigation, email verification, and personal-data export/deletion | See the [account-feature status](sign-in-and-account-lifecycle.md#account-management-available-versus-unfinished). Hidden controls do not prove that backing endpoints are unreachable or unused. |

### How to establish actual usage

Code inspection can show available navigation, callers, and endpoint checks.
It cannot tell us how many people use a feature. To measure that, use existing
aggregate request telemetry and login events over a representative event period,
including registration, live play, and post-event access.

Count both page visits and operations: opening login is not a successful
sign-in, and opening a profile is not saving it. Permission checks often run
within page/API requests, so endpoint counts alone do not count policy
evaluations. Recovery needs an explicit outcome event if existing logs do not
distinguish it from ordinary login.

Record the observation period and telemetry coverage with any usage ranking.
Exclude automated traffic where identifiable and account for sampling or missing
logs. Use aggregate data; do not collect credentials, tokens, cookies, or personal
profile values. There is no new telemetry implementation in these guides.

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
