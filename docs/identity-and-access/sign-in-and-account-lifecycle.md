# Identity and Access - Sign-in and Account Lifecycle

[Section home](README.md) | [Permissions](authorization-and-permissions.md) |
[Development](development-and-troubleshooting.md)

## One person, two records

PuzzleServer separates the login account from the player's application profile:

```text
Microsoft account
    -> IdentityUser: login account and external-login association
    -> PuzzleUser: profile and application permissions
       linked by PuzzleUser.IdentityUserId = IdentityUser.Id
```

**Framework:** Identity manages `IdentityUser` and external-login associations.
[PuzzleServerContext](../../Data/PuzzleServerContext.cs) inherits `IdentityDbContext`
and stores these alongside application data.

**PuzzleServer:** [PuzzleUser](../../Data/DataModel/PuzzleUser.cs) has its own
integer ID. `GetPuzzleUserForCurrentUser` uses the signed-in Identity ID to find
that profile. An account without a matching profile is not enough for normal
application permission checks.

**The two email fields are not interchangeable.** Login recovery uses
`IdentityUser.Email`. The self-service profile editor changes `PuzzleUser.Email`
without changing the Identity email or username. Changing a profile email
therefore does not change how future logins are matched.

## Follow a login

The main application logic is in
[ExternalLoginModel](../../ServerCore/Areas/Identity/Pages/Account/ExternalLogin.cshtml.cs).

```text
Open /Identity/Account/Login
    -> clear temporary external cookie; show registered login providers
Submit the login form
    -> send the browser to Microsoft
Microsoft redirects to /signin-microsoft
    -> authentication middleware handles the provider response
    -> redirects to the application's ExternalLogin Callback handler
       -> existing account: sign in and return to the local requested page
       -> new account: show a profile confirmation form
Submit the confirmation form
    -> create IdentityUser, external-login association, and PuzzleUser
    -> commit the database transaction, sign in, and return locally
```

There are **two callbacks**: `/signin-microsoft` is handled by the framework;
the Razor Page's `Callback` handler handles our account logic.
The [login view](../../ServerCore/Areas/Identity/Pages/Account/Login.cshtml)
shows external providers, not a local username/password form.

### Returning users

`OnGetCallbackAsync` looks up the provider's account identifier (the "provider
key"). If that association is missing but Microsoft supplies an email claim,
it looks for an existing Identity account with that email and adds the login
association. This is our recovery mechanism for provider-key changes, not a
standard step automatically supplied by Microsoft login.

`Startup` explicitly requires unique Identity emails to support this lookup.
Changing recovery requires code and review of the provider's email guarantees;
do not disable unique-email enforcement independently.

Provider errors or missing external information redirect to login with an error.
A locked-out account goes to the Lockout page. Other unsuccessful sign-in
results show the profile confirmation form.

### New users

`OnPostConfirmationAsync` creates the Identity account with the submitted email
as both username and email, links the provider, and saves the validated profile
in a database transaction. It commits before signing in.

In `Development`, this flow makes the new user a global admin if no global
admins exist. This is **PuzzleServer bootstrap code**, not a production setup
procedure. Use isolated development data; never switch production to
`Development` to grant someone access.

## Defaults and team choices

The [configuration guide](development-and-troubleshooting.md#configuration-settings)
lists actual setting names. The table below explains behavior and which changes
would require code.

| Topic | Framework default / PuzzleServer behavior | What the team can change |
| --- | --- | --- |
| Microsoft login | Framework supplies OAuth endpoints, claims mapping, PKCE, Graph `user.read` scope, and `/signin-microsoft`. PuzzleServer registers the provider in `Startup`. | Credentials and registered redirect URIs are deployment/provider configuration. Adding providers or changing scopes/callback paths requires code and provider setup; update login text too. |
| Identity services | `AddDefaultIdentity` supplies cookies, account UI, and token providers. PuzzleServer uses EF storage and overrides key length to 450 rather than the registration default of 128. | Storage/key-length changes require code and schema review, not a configuration toggle. |
| Login persistence | Base cookie options use a 14-day ticket lifetime and sliding expiration. Our sign-in calls request `isPersistent: false`; that is not a guaranteed 14-day browser login. Development sets a minimum SameSite policy of `Lax`. | Custom application-cookie options and persistent sign-in require code changes. There is no `ConfigureApplicationCookie` override in our startup code. |
| Two-factor authentication | Our existing-account callback explicitly passes `bypassTwoFactor: true`. This does not disable Microsoft's own MFA. | Changing application MFA requires coordinated callback and UI work, not just a navigation link. |
| Account confirmation | Framework confirmed-email, phone, and account requirements default to false; our registrations do not enable them. | Enabling them requires Identity-options code and a working confirmation flow. New-account creation calls `SignInAsync` directly, so it must be updated too. |

Version-matched sources:
[Microsoft options](https://github.com/dotnet/aspnetcore/blob/v10.0.5/src/Security/Authentication/MicrosoftAccount/src/MicrosoftAccountOptions.cs),
[Identity registration](https://github.com/dotnet/aspnetcore/blob/v10.0.5/src/Identity/UI/src/IdentityServiceCollectionUIExtensions.cs),
[cookie options](https://github.com/dotnet/aspnetcore/blob/v10.0.5/src/Security/Authentication/Cookies/src/CookieAuthenticationOptions.cs),
and [confirmation options](https://github.com/dotnet/aspnetcore/blob/v10.0.5/src/Identity/Extensions.Core/src/SignInOptions.cs).

## Account management: available versus unfinished

| Feature | Current behavior and limits |
| --- | --- |
| [Profile](../../ServerCore/Areas/Identity/Pages/Account/Manage/Index.cshtml.cs) | Updates `PuzzleUser` and refreshes sign-in. The handler preserves stored IDs, Identity link, admin/staff flags, and shirt size rather than trusting posted values. |
| [External logins](../../ServerCore/Areas/Identity/Pages/Account/Manage/ExternalLogins.cshtml.cs) | Lists and links providers to the signed-in account; removal calls `RemoveLoginAsync`. The UI shows removal only when another login or password exists, but the POST handler does not repeat that safeguard. |
| Password / two-factor navigation | Links are commented out in [_ManageNav.cshtml](../../ServerCore/Areas/Identity/Pages/Account/Manage/_ManageNav.cshtml). Do not treat framework support as an exposed, complete application feature. |
| Email verification | The profile button is commented out. Its handler generates a token and URL but does not send mail. |
| [Personal data](../../ServerCore/Areas/Identity/Pages/Account/Manage/PersonalData.cshtml) | UI says "Coming Soon" and hides export/delete controls. Backing handlers still exist; hidden links do not prove an endpoint is unreachable. |
| [Export](../../ServerCore/Areas/Identity/Pages/Account/Manage/DownloadPersonalData.cshtml.cs) / [deletion](../../ServerCore/Areas/Identity/Pages/Account/Manage/DeletePersonalData.cshtml.cs) | Export covers Identity properties marked `[PersonalData]`; deletion calls `UserManager.DeleteAsync`. Both have TODOs for `PuzzleUser`. Neither is a complete domain-data workflow. |
| [Administrative account editor](../../ServerCore/Areas/Identity/Pages/Account/Edit.cshtml.cs) | Separate `PuzzleUser` editor protected by `IsGlobalAdmin`, not the self-service profile page. |

Changing editable fields requires view and handler changes. Exposing unfinished
features also requires decisions about domain data, related records, and
retention; uncommenting the controls is not sufficient.

## Sign-out

[LogoutModel.OnPost](../../ServerCore/Areas/Identity/Pages/Account/Logout.cshtml.cs)
signs out and optionally redirects to a local page. GET does not sign out.
Leaving PuzzleServer does not necessarily end the Microsoft browser session.
