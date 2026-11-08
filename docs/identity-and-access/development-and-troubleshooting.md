# Identity and Access - Development and Troubleshooting

[Section home](README.md) | [Account lifecycle](sign-in-and-account-lifecycle.md) |
[Permissions](authorization-and-permissions.md)

## Start here

Complete the [existing setup guide](https://github.com/PuzzleServer/mainpuzzleserver/wiki/First-time-set-up),
then inspect [Startup.cs](../../ServerCore/Startup.cs),
[IdentityHostingStartup.cs](../../ServerCore/Areas/Identity/IdentityHostingStartup.cs),
and [DeploymentConfiguration.cs](../../ServerCore/Areas/Deployment/DeploymentConfiguration.cs).
Use an isolated database and authorized development provider registration.
Never put credentials, real user exports, or authentication cookies in source,
issues, screenshots, or logs.

## Configuration: existing knobs versus potential changes

The configuration builders in `Startup` and `IdentityHostingStartup` load
`appsettings.json`, then optional `appsettings.<Environment>.json`, then
environment variables. Later providers override earlier values.
`Program` uses the default host builder, but these custom builders rebuild
configuration without explicitly adding user secrets. Although the project has
a `UserSecretsId`, do not assume secrets are included in the configuration used
by both identity and startup; inspect the effective provider chain if using
that mechanism.

Options read inside startup registrations are applied at startup. Restart the
application after changing these settings; JSON reload support alone does not
guarantee already-registered provider/database options change.

| Item | Current definition / effect | How the team changes it |
| --- | --- | --- |
| `Authentication-Microsoft-ApplicationId` | Read by `AddMicrosoftAccount` as client ID. | Existing config key: development environment or deployment settings. Restart required for reliable application. |
| `Authentication-Microsoft-Password` | Read by `AddMicrosoftAccount` as client secret. | Existing sensitive config key: approved secret storage/environment/deployment settings, never checked-in values. Restart after rotation. |
| Microsoft redirect URI | Provider defaults to `/signin-microsoft`; registration must match application origin and path. | Microsoft application registration plus application callback-option code if changing the path. |
| `ASPNETCORE_ENVIRONMENT` | Selects environment behavior; directly read for first-global-admin development bootstrap. | Environment configuration and restart. Do not change production to Development for access troubleshooting. |
| `WEBSITE_SITE_NAME` | Used with environment checks to select named Azure database branches and startup behavior. | Deployment setting, not a user privilege toggle. Verify both name and environment. |
| `ConnectionStrings:PuzzleServerSQLConnectionString` | SQL connection used for the recognized Azure staging/production branches. | Existing sensitive configuration; environment form uses `ConnectionStrings__PuzzleServerSQLConnectionString`. Restart. |
| `ConnectionStrings:PuzzleServerContextLocal` | SQL connection used in other environment/site-name branches. | Existing sensitive configuration; environment form uses `ConnectionStrings__PuzzleServerContextLocal`. Restart. |
| `ConnectionStrings:AzureStorageConnectionString` | Storage configuration used by startup; local development also creates a sync table. | Existing sensitive configuration. It is not an Identity credential, but startup can fail before sign-in without working storage. |
| `IdentityOptions.User.RequireUniqueEmail = true` | Explicit application override supporting email-based external-login recovery. | Code in `Startup`, not a currently bound configuration key. Review callback logic before changing. |
| `IdentityOptions.Stores.MaxLengthForKeys = 450` | Explicit override in `IdentityHostingStartup` to stabilize schema generation; `AddDefaultIdentity` uses 128 before this override in v10.0.5. | Code/schema decision. Review Identity migrations before changing; not an operational tuning setting. |
| Confirmed-email/account sign-in requirements | Framework defaults are false; application does not enable them. | Potential Identity-options code changes plus completion of confirmation and new-account flows. |
| Cookie lifetime / persistent login | Base framework cookie options; sign-in calls explicitly pass `isPersistent: false`. | Potential cookie-options and sign-in code changes, not existing config toggles. |
| External sign-in two-factor handling | Callback explicitly passes `bypassTwoFactor: true`. | Code-level product decision; coordinate with account UI and validation. |
| Event/team/puzzle permissions | Custom handlers plus relationships/settings in the database. | Assignments/settings through relevant administrative workflows; different rule semantics require code. |

The names above are setting names only; this guide intentionally provides no
credential values. The database-selection logic is more specific than simply
"Azure versus local": inspect the exact environment/site-name branches.

**Framework references:** [configuration precedence](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0),
[Identity registration defaults](https://github.com/dotnet/aspnetcore/blob/v10.0.5/src/Identity/UI/src/IdentityServiceCollectionUIExtensions.cs),
and the [lifecycle guide's version-matched option references](sign-in-and-account-lifecycle.md).

## Change recipes

### Protect a new event page

1. Read the nearest existing page with the same resource/operation. Confirm its
   route and policy, not just its navigation visibility.
2. Use `EventSpecificPageModel` when event/role binding and its mode filter apply.
   Ensure route parameters match the helper's expected names.
3. Apply the existing policy whose actual conditions match the operation. For
   event-admin-only work, follow `Events/Details` with
   `[Authorize(Policy = "IsEventAdmin")]` and `admin` route mode.
4. Check resource existence, event relationships, and operation-specific rules
   before reading or mutating data. Do not assume the base model validates all
   puzzle/team IDs or that a `play` route requires registration.
5. Exercise permitted and denied users through GET and POST, including direct
   requests without navigation.

For an endpoint without event-role routing, do not copy route-dependent policies
blindly. Follow a suitable existing explicit/resource check, or implement a
requirement that safely obtains the resource context.

### Add or alter a named policy

Define an `IAuthorizationRequirement` and its handler(s) under
`Areas/Identity/UserAuthorizationPolicy`. Register both the named policy and
the scoped `IAuthorizationHandler` implementations in `Startup`.
Reuse appropriate domain helpers; validate route resources before dereferencing.

For an OR rule, follow the existing one-requirement/multiple-handler pattern.
Separate requirements are AND-ed, not OR-ed. Update affected page, controller,
component-service, and file-access paths where the intended rule applies.
Do not use `[AllowAnonymous]` or a global-admin bypass as a fix for missing
context or stale membership data.

### Modify registration or account management

Follow `ExternalLoginModel` for the transaction spanning Identity account,
external association, and domain profile. Preserve existing-account recovery
unless deliberately changing its contract.

For profile changes, follow `Manage/IndexModel`'s server-side preservation of
privileged fields. Hidden HTML inputs are not protection on their own.
If adding a field, decide whether it belongs to Identity, `PuzzleUser`, or
event registration; update its actual readers as well as its editor.

For personal-data work, complete the existing domain-data TODOs and choose
retention/association behavior before exposing controls. For confirmation/MFA
work, wire the new-account direct-sign-in path as well as the existing-account
callback and UI.

## Troubleshooting map

| Symptom | Trace / likely distinction | Safe next check |
| --- | --- | --- |
| Startup fails before login | Provider options, database selection, or development storage initialization. | Check the failing registration/startup stage and effective setting names; avoid dumping connection strings or secrets. |
| Login view has no provider button | `LoginModel` enumerates registered external authentication schemes. | Confirm Microsoft provider registration succeeds; inspect configuration presence without printing secret values. |
| Microsoft rejects redirect | Middleware callback and provider registration do not match. | Compare scheme/host/port/path with `/signin-microsoft`; distinguish it from the Razor Page callback. |
| Callback reports missing external information | Temporary external state was not available at callback. | Trace challenge/callback, HTTPS/origin, cookies, and environment policy. Do not log cookie or token contents. |
| Previously known user is asked to register | Provider-key association is absent or email recovery did not identify the original Identity account. | Inspect `AspNetUserLogins` and `IdentityUser.Email` in authorized tooling; a `PuzzleUser.Email` match alone does not drive recovery. |
| Signed in but domain profile is missing | `GetPuzzleUserForCurrentUser` cannot resolve `IdentityUserId`. | Inspect both records and the registration transaction; do not create a duplicate account as a default repair. |
| Staff association exists but access is denied | Wrong requested role, missing event relationship, or `MayBeAdminOrAuthor == false`. | Compare route mode and target event with the exact policy helper; use the supported assignment workflow. |
| Global admin cannot open an event-admin page | Global admin is not a universal override. | Check event-admin assignment; event creation grants its creator explicit memberships. |
| Registered player fails a team check | `PlayerInEvent` and `TeamMembers` represent different conditions. | Confirm caller's team for the event and route `teamId`. |
| Puzzle visible in a page but file access differs | Visibility policy and `FilesController.IsAuthorized` have distinct branches/file types. | Compare unlock/solve state, answer timing, alpha-testing conditions, and requested file type. |
| New policy denies or throws on an API route | Route helper lacks expected IDs/mode, or entities did not resolve. | Inspect actual route values; do not assume query/body IDs become route context. |
| Profile email changed but linking did not | Self-service editor changes domain email, not Identity email. | Trace both fields and the intended account-lifecycle change. |
| Personal-data screen has no export/delete controls | UI is explicitly "Coming Soon". | Read scaffolded handlers and domain-data TODOs; do not treat them as complete or expose them just to bypass the UI. |

Distinguish an authentication challenge from authorization failure and missing
resources. Browser pages may redirect to login/access denied; API behavior
depends on endpoint metadata and framework version. Existing handlers also
explicitly return `Unauthorized`, `Forbid`, `Challenge`, or `NotFound`.
Record the operation and failing branch rather than relying on HTTP status alone.

## Validation checklist

The repository's [UnitTest1.cs](../../ServerCore.Test/UnitTest1.cs) contains an
empty placeholder test. There is no meaningful automated identity/authorization
coverage in that file. Do not equate a successful existing test run with
validation of an access change.

For code changes, build the application with the repository's required SDK and
dependencies:

```sh
dotnet build ServerCore/ServerCore.csproj
```

Use the existing test project for added targeted tests:

```sh
dotnet test ServerCore.Test/ServerCore.Test.csproj
```

Validate these scenarios in isolated development data; add targeted automated
coverage for the behavior being changed:

| Scenario family | Cases to cover |
| --- | --- |
| Sign-in | Existing login; new-account confirmation; provider error; missing email/external info; duplicate email; provider-key recovery; local return URL; logout. |
| Identity/domain association | Correct `IdentityUserId`; rollback on incomplete account creation; missing profile; privileged fields unchanged by profile POST. |
| Event roles | Admin, author, player, archive, and supported impersonation; permitted user in the wrong role; global admin without event membership. |
| Membership | Registered without a team; team in another event; caller targeting another team; staff relationship with the fast-path flag missing. |
| Puzzle access | Locked/unlocked team puzzle; global and individual single-player unlock; answer-release boundaries; alpha testing; archive available/unavailable. |
| Direct operations | GET and POST; page and API; file access; component service operations; missing/invalid IDs and cross-event resource combinations. |
| Config changes | Provider callback round trip; effective settings after restart; matching database branch; changed confirmation/cookie behavior where applicable. |

Do not exercise account deletion on shared or production data. Documentation-only
changes do not need application builds.

## Keeping these guides current

Update the policy matrix when changing handlers or route semantics. Update the
lifecycle guide when changing email linking, profile storage, sign-in options,
or exposing account features. Recheck framework references when package/runtime
versions change. Preserve the distinction between an existing configurable
setting and a possible future code-level choice.
