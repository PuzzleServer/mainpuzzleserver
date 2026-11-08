# Identity and Access

This section helps new contributors trace sign-in and permission decisions,
find the responsible code, and distinguish framework behavior from team choices.
It describes the implementation, not a promise that every scaffolded feature is
complete or exposed.

## Reading order

| Guide | Use it to |
| --- | --- |
| [Sign-in and Account Lifecycle](sign-in-and-account-lifecycle.md) | Follow Microsoft login, understand the two user records, and find account-management behavior. |
| [Authorization and Permissions](authorization-and-permissions.md) | Select a policy and understand event roles, membership checks, and puzzle visibility. |
| [Development and Troubleshooting](development-and-troubleshooting.md) | Configure a development environment, make changes, and diagnose failures. |

Start with the existing [First time Setup wiki page](https://github.com/PuzzleServer/mainpuzzleserver/wiki/First-time-set-up)
for general environment setup. These guides cover the identity-specific details.

## Three layers of behavior

Throughout this section, behavior is labeled as:

| Layer | Meaning | Example |
| --- | --- | --- |
| **Framework** | ASP.NET Core behavior inherited unless overridden. Defaults are version-dependent. | Identity uses an application authentication cookie and a temporary external sign-in scheme. |
| **PuzzleServer** | Application code, conventions, and explicit overrides. | `PuzzleUser` is separate from `IdentityUser`; permission policies query event/team/puzzle records. |
| **Team choice** | An existing configuration knob, an administrative data change, or a possible code change. These are explicitly distinguished. | Microsoft credentials are configuration; adding a provider or changing policy semantics requires code. |

The application targets `net10.0`; Identity UI and Microsoft Account packages are
pinned to `10.0.5` in [ServerCore.csproj](../../ServerCore/ServerCore.csproj).
Framework source references in these guides use that release. The installed
shared-framework patch can differ; recheck effective options and defaults when
upgrading or diagnosing a deployment.

## Mental model

```text
Microsoft account
    |
    | external authentication
    v
IdentityUser + external-login association
    |
    | PuzzleUser.IdentityUserId = IdentityUser.Id
    v
PuzzleUser
    |
    +-- IsGlobalAdmin
    +-- EventAdmins / EventAuthors / PuzzleAuthors
    +-- PlayerInEvent (event registration)
    +-- TeamMembers (team membership)
    +-- puzzle unlock state (team or individual)
```

**Authentication** establishes the signed-in `IdentityUser`.
**Authorization** decides whether that person may perform an operation on a
particular resource. Successful login does not grant event registration, team
membership, authorship, or administrative access.

`eventRole` in a URL selects the requested operating mode; it is not an
ASP.NET Identity role and does not grant permission. For example, requesting an
`admin` URL still requires the relevant event-admin relationship.

## Code map

| Entry point | Responsibility |
| --- | --- |
| [Program.cs](../../ServerCore/Program.cs) | Creates the host using `Startup`. |
| [IdentityHostingStartup.cs](../../ServerCore/Areas/Identity/IdentityHostingStartup.cs) | Registers default Identity services and EF-backed stores. |
| [Startup.cs](../../ServerCore/Startup.cs) | Registers Microsoft authentication, named policies, handlers, options, and middleware. |
| [Account pages](../../ServerCore/Areas/Identity/Pages/Account) | Login, callback, registration confirmation, logout, and account management. |
| [PuzzleUser.cs](../../Data/DataModel/PuzzleUser.cs) | Domain profile, Identity ID lookup, event participation, and staff checks. |
| [PuzzleServerContext.cs](../../Data/PuzzleServerContext.cs) | Stores both Identity and puzzle-domain data. |
| [AuthorizationHelper.cs](../../ServerCore/Areas/Identity/UserAuthorizationPolicy/AuthorizationHelper.cs) | Reads route context and evaluates most named requirements. |
| [UserAuthorizationPolicy](../../ServerCore/Areas/Identity/UserAuthorizationPolicy) | Requirement types and their handlers. |
| [EventSpecificPageModel.cs](../../ServerCore/ModelBases/EventSpecificPageModel.cs) | Binds event/role context and checks permitted page operating modes. |
| [UserEventHelper.cs](../../ServerCore/Helpers/UserEventHelper.cs) | Resolves team membership and puzzle authorship/administration. |

**Use `Data/` for active models.** `ServerCore.csproj` excludes
`ServerCore/Data/**` and `ServerCore/DataModel/**` from compilation and references
the separate `Data` project. The older similarly named files are not the active
implementation.

## Important boundaries

Global admin is a domain-user flag, not an automatic override for every event
policy. Event admin, event author, and puzzle author are distinct relationships.
Registration and team membership are also separate.

Named policies are not the only enforcement layer. Page-model filters,
controller checks, and service methods also make permission decisions. Hiding a
navigation item is not server-side authorization.

The account UI primarily exposes external sign-in. Password and two-factor
navigation items are commented out. Personal-data export/deletion is marked
"Coming Soon"; its scaffolded handlers do not cover `PuzzleUser` data. See the
[lifecycle guide](sign-in-and-account-lifecycle.md#account-management-and-incomplete-surfaces)
before modifying or exposing those features.

## Wiki publication

The main repository is the reviewable source for this section. GitHub wikis do
not support native pull requests; merging a documentation PR does not update
the wiki automatically.

When publishing, create these pages and rewrite relative document/source links
for the wiki:

| Repository file | Wiki page title |
| --- | --- |
| `README.md` in this directory | Identity and Access |
| `sign-in-and-account-lifecycle.md` | Identity and Access - Sign-in and Account Lifecycle |
| `authorization-and-permissions.md` | Identity and Access - Authorization and Permissions |
| `development-and-troubleshooting.md` | Identity and Access - Development and Troubleshooting |

Add an **Identity and Access** section to the wiki home page, linking to its
landing page and the three guides. Preserve existing setup and technology links.
Source links should point to the corresponding main-repository files, not to
paths inside the wiki Git repository.
