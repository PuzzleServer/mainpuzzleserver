# Identity and Access - Development and Troubleshooting

[Section home](README.md) | [Account lifecycle](sign-in-and-account-lifecycle.md) |
[Permissions](authorization-and-permissions.md)

Use the [existing setup guide](https://github.com/PuzzleServer/mainpuzzleserver/wiki/First-time-set-up)
for general prerequisites. Use isolated development data and an approved
Microsoft application registration. Never include credentials, tokens, cookies,
or real user exports in source, issues, or diagnostic output.

## How settings are loaded

There are three configuration paths in the code. They should not be described
as one identical settings loader:

| Path | What the source shows |
| --- | --- |
| [Program](../../ServerCore/Program.cs) and `Startup(IConfiguration, IWebHostEnvironment)` | `WebHost.CreateDefaultBuilder` creates the host. This constructor accepts host-provided configuration; it does not rebuild it. `Startup` reads Microsoft credentials from its `Configuration`. |
| Alternate `Startup(IWebHostEnvironment)` constructor | Builds its own configuration from `appsettings.json`, optional environment-specific JSON, then environment variables. It does not explicitly add user secrets. |
| [IdentityHostingStartup](../../ServerCore/Areas/Identity/IdentityHostingStartup.cs) | Builds the same JSON/environment collection, assigns it to the hosting context, and uses it to configure database/Identity services. |

For the custom builders, later sources override earlier ones. The default host
builder can include additional sources, including development user secrets, but
these custom builders do not explicitly include them. The project's
`UserSecretsId` alone does not prove that every startup component sees them.
**The effective user-secrets behavior across these paths has not been confirmed
at runtime.** If a setting appears missing, inspect which providers supply it
without printing its value.

Restart after changing provider, environment, or database settings. JSON reload
support does not guarantee already-registered options will update.
See [ASP.NET configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0)
and [Startup.cs](../../ServerCore/Startup.cs).

## Configuration settings

These are existing settings, not proposed toggles. Behavior that requires code
changes, such as login persistence or account confirmation, is covered once in
[Defaults and team choices](sign-in-and-account-lifecycle.md#defaults-and-team-choices).

| Setting / external configuration | Purpose |
| --- | --- |
| `Authentication-Microsoft-ApplicationId` | Microsoft login client ID, read in `Startup`. |
| `Authentication-Microsoft-Password` | Microsoft login client secret. Store securely, never in checked-in values. |
| Microsoft application redirect URI | Must match the app's scheme, host, port, and `/signin-microsoft` callback. Configure in the provider registration. |
| `ASPNETCORE_ENVIRONMENT` | Selects environment behavior and development-only global-admin bootstrap. Never change production to Development for troubleshooting. |
| `WEBSITE_SITE_NAME` | Together with environment, selects specific Azure database/startup branches; not a privilege switch. |
| `ConnectionStrings:PuzzleServerSQLConnectionString` | Database used in recognized Azure staging/production branches. |
| `ConnectionStrings:PuzzleServerContextLocal` | Database used in other environment/site-name branches. |
| `ConnectionStrings:AzureStorageConnectionString` | Startup storage configuration, including development sync-table initialization. Missing storage can prevent startup before login. |

For nested environment settings, use double underscores, for example
`ConnectionStrings__PuzzleServerContextLocal`.
[DeploymentConfiguration](../../ServerCore/Areas/Deployment/DeploymentConfiguration.cs)
contains the exact database branches; selection is not simply "Azure vs local."

## Change recipes

### Add a protected page or policy

1. Find an existing page for the same kind of operation. For event-admin work,
   [Events/Details](../../ServerCore/Pages/Events/Details.cshtml.cs) uses
   `[Authorize(Policy = "IsEventAdmin")]` and an `admin` URL mode.
2. Use `EventSpecificPageModel` where its event binding and mode checks apply.
   Choose a policy using the [policy reference](authorization-and-permissions.md#policy-reference),
   including its route-context limitations. Validate that requested records
   exist and belong to the requested event before accessing or changing them.
3. If no policy fits, add a requirement and handler(s) under
   `Areas/Identity/UserAuthorizationPolicy`, then register the policy and scoped
   handlers in `Startup`. Follow the existing single-requirement pattern for
   OR rules.
4. Apply the intended rule to every relevant entry point, including POST/API
   and component-service operations. Do not fix access errors by adding
   `[AllowAnonymous]` or assuming global admin overrides everything.

An endpoint without the expected route context needs an explicit/resource-aware
check, not a blindly copied URL-dependent policy.

### Change account behavior

Start with the [account lifecycle guide](sign-in-and-account-lifecycle.md) and
the handler it links. Decide whether a field belongs to the login account,
application profile, or event registration. Update its readers as well as its
editor. Preserve transaction boundaries and server-side protection of privileged
fields; hidden inputs are not protection.

Before exposing unfinished account features, complete their domain-data
behavior. Confirmation/MFA changes must cover both returning users and
new-account sign-in, not only the account navigation.

## Troubleshooting

| Symptom | First useful check |
| --- | --- |
| Startup fails / no login provider appears | Identify the failing startup stage; check configuration presence, selected database branch, and storage without logging secrets. |
| Microsoft rejects the redirect | Compare provider registration with the exact origin and `/signin-microsoft`, not the Razor Page callback. |
| Callback has no external login information | Trace the login redirect, HTTPS/origin, and temporary cookies. |
| Returning user is asked to register | Check external-login association and Identity email; profile email does not drive recovery. |
| Signed in but profile is missing | Check `PuzzleUser.IdentityUserId` against the Identity account. Don't create a duplicate account as a default repair. |
| Staff denied / global admin denied | Check URL mode, event membership, and `MayBeAdminOrAuthor`; global admin is not an event-policy override. |
| Registered player fails team access | Check their team for that event and the requested `teamId`. |
| Puzzle page and file access differ | Compare the visibility policy with `FilesController.IsAuthorized`, including file type and unlock/solve/answer timing. |
| API policy denies or throws | Check actual route values, record existence, and event/resource relationships. |
| Profile edit doesn't change login / export controls missing | See the lifecycle guide's two-email distinction and unfinished account features. |

A login challenge, permission denial, and missing record are different failures.
Pages may redirect while API actions return statuses directly. Record the
operation and failing check rather than diagnosing from HTTP status alone.

## Validation checklist

For application code changes, build with the required SDK/dependencies:

```sh
dotnet build ServerCore/ServerCore.csproj
```

**The existing test project is not ready to validate this area.**
[ServerCore.Test.csproj](../../ServerCore.Test/ServerCore.Test.csproj) targets
`netcoreapp2.1`, does not reference the `net10.0` app, and
[UnitTest1.cs](../../ServerCore.Test/UnitTest1.cs) is an empty placeholder.
Before adding application tests, update its framework/packages and project
references, or establish a compatible test project. A passing placeholder test
is not evidence that login or permissions work.

For each change, exercise relevant cases in isolated development data and add
targeted automated coverage once the test setup is suitable:

| Area | Cases |
| --- | --- |
| Login/accounts | Returning/new user, provider error, missing email/info, duplicate email, recovery, transaction rollback, local redirect, logout, protected profile fields. |
| Roles/membership | Allowed and denied callers; wrong URL mode; global admin without event membership; registration without a team; missing staff flag; impersonation. |
| Puzzle access | Locked/unlocked team and individual puzzles, global unlock, answer-release boundary, alpha testing, available/unavailable archive. |
| Operations/configuration | GET/POST/API/file/service paths; invalid IDs; another team's ID; a puzzle from another event; effective settings after restart and provider callback. |

Never test deletion on shared or production data. Documentation-only changes
do not need application builds. Keep the lifecycle/defaults, policy reference,
and configuration instructions current when their owning code changes.
