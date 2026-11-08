# Identity and Access - Sign-in and Account Lifecycle

[Section home](README.md) | [Permissions](authorization-and-permissions.md) |
[Development](development-and-troubleshooting.md)

## Start here

Read [IdentityHostingStartup.cs](../../ServerCore/Areas/Identity/IdentityHostingStartup.cs),
the authentication/options registrations in [Startup.cs](../../ServerCore/Startup.cs),
and then [ExternalLogin.cshtml.cs](../../ServerCore/Areas/Identity/Pages/Account/ExternalLogin.cshtml.cs).
The latter contains the main application-specific sign-in and account-creation
flow.

## Identity records versus domain records

**Framework:** `IdentityUser` holds authentication-account information.
Identity's EF store manages accounts and external-login associations.
[PuzzleServerContext](../../Data/PuzzleServerContext.cs) inherits
`IdentityDbContext`.

**PuzzleServer:** [PuzzleUser](../../Data/DataModel/PuzzleUser.cs) holds the
application profile and permission-related flags. `IdentityUserId` associates
it with `IdentityUser.Id`; it has its own integer `ID`.
`GetPuzzleUserForCurrentUser` resolves the authenticated principal's Identity ID
and queries `PuzzleUsers`. A valid Identity account without a corresponding
domain profile is not sufficient for normal application permission checks.

There are two email fields: `IdentityUser.Email` and `PuzzleUser.Email`.
Account linking uses the former. The self-service profile editor updates the
latter and does not call `UserManager.SetEmailAsync`. Do not assume a profile
email change changes the authentication account or future login matching.

## Sign-in trace

```text
GET /Identity/Account/Login
  -> clear temporary external cookie; list registered providers
POST /Identity/Account/ExternalLogin
  -> challenge selected provider
Microsoft redirects to /signin-microsoft
  -> Microsoft authentication middleware handles OAuth callback
  -> redirects to application's ExternalLogin Callback handler
  -> look up/link external login
  -> existing account: sign in and LocalRedirect(returnUrl)
  -> new account: show profile confirmation form
POST ExternalLogin Confirmation
  -> create IdentityUser + external login + PuzzleUser in a transaction
  -> commit, sign in, LocalRedirect(returnUrl)
```

The provider callback `/signin-microsoft` and the Razor Page's `Callback`
handler are different steps. The first belongs to authentication middleware;
the second performs application account handling.

### Provider configuration

**Framework:** `AddMicrosoftAccount` supplies OAuth endpoints, claims mapping,
PKCE enabled by default, a Graph `user.read` scope, and callback path
`/signin-microsoft`. See the version-matched
[MicrosoftAccountOptions source](https://github.com/dotnet/aspnetcore/blob/v10.0.5/src/Security/Authentication/MicrosoftAccount/src/MicrosoftAccountOptions.cs).

**PuzzleServer:** `Startup.ConfigureServices` explicitly registers this provider
using `Authentication-Microsoft-ApplicationId` and
`Authentication-Microsoft-Password`. The
[login view](../../ServerCore/Areas/Identity/Pages/Account/Login.cshtml) iterates
registered external schemes; it has no local username/password form.

**Team choice:** Credentials and the Microsoft application registration are
deployment configuration. Register the correct redirect URI for each supported
origin, including scheme, host, port, and `/signin-microsoft`. Changing callback
paths, scopes, or adding another provider requires authentication-registration
code changes as well as provider setup; the explanatory login text may also
need updating.

### Existing accounts and provider-key recovery

`OnGetCallbackAsync` handles provider errors or missing external information by
setting an error and redirecting to login.

Before calling `ExternalLoginSignInAsync`, it checks whether the provider/key
association exists. If not, and the provider supplies an email claim, it looks
up an existing `IdentityUser` by email and calls `AddLoginAsync`.

**PuzzleServer:** This email-based association is custom recovery behavior,
intended to handle provider-key changes. It is not merely standard Microsoft
OAuth behavior. `Startup` explicitly enables `IdentityOptions.User.RequireUniqueEmail`
to support the lookup.

**Team choice:** Changing this recovery strategy requires code and careful
review of the provider's email-claim guarantees and account-linking behavior.
Do not disable unique-email enforcement independently of this flow.

The existing-account sign-in call uses `isPersistent: false` and
`bypassTwoFactor: true`. A successful sign-in returns locally; a locked-out
account goes to the Lockout page; other unsuccessful results lead to profile
confirmation. These explicit arguments are application choices, not general
Identity defaults or proof that the external provider's own MFA is disabled.

### New-account confirmation

`OnPostConfirmationAsync` creates an `IdentityUser` using `Input.Email` for
username and email, associates the external login, assigns
`Input.IdentityUserId`, and saves the validated `PuzzleUser`.
These database operations are wrapped in a transaction committed before sign-in.

**PuzzleServer:** In the `Development` environment, if there are no domain
users with `IsGlobalAdmin`, this flow makes the newly created user a global
admin. This is bootstrap logic, not a framework default or production bootstrap
mechanism.

**Team choice:** Environment selection is configuration; changing bootstrap
criteria is code. Use isolated development data and do not set a production
deployment to `Development` to grant access.

## Cookies and sign-out

**Framework:** `AddDefaultIdentity` registers Identity application/external
authentication schemes, cookies, default UI, and token providers. See
[Identity registration source](https://github.com/dotnet/aspnetcore/blob/v10.0.5/src/Identity/UI/src/IdentityServiceCollectionUIExtensions.cs).
The base cookie options use a 14-day ticket lifetime and sliding expiration;
these are not a guarantee of a 14-day persistent browser login.
See [cookie options](https://github.com/dotnet/aspnetcore/blob/v10.0.5/src/Security/Authentication/Cookies/src/CookieAuthenticationOptions.cs).

**PuzzleServer:** The login flows explicitly request nonpersistent sign-in.
`Startup.Configure` applies a development-only minimum SameSite policy of
`Lax`. There is no application `ConfigureApplicationCookie` override in this
startup code.

**Team choice:** Custom application-cookie lifetime, redirect behavior, or
security settings would require options-registration code. Cookie/domain
changes must preserve the OAuth round trip and account-management behavior.

[LogoutModel.OnPost](../../ServerCore/Areas/Identity/Pages/Account/Logout.cshtml.cs)
calls `SignOutAsync` and optionally `LocalRedirect`. Its GET handler does not
perform sign-out. Application logout does not necessarily sign the person out
of their Microsoft browser session.

## Account management and incomplete surfaces

| Surface | Actual implementation | Layer / team change |
| --- | --- | --- |
| [Profile](../../ServerCore/Areas/Identity/Pages/Account/Manage/Index.cshtml.cs) | Edits `PuzzleUser`, preserves ID/Identity association, staff/admin flags and shirt size from stored data, saves, and refreshes sign-in. | PuzzleServer customization; changing editable fields requires view/model-handler changes. Preserve server-side protection of privileged fields. |
| [External logins](../../ServerCore/Areas/Identity/Pages/Account/Manage/ExternalLogins.cshtml.cs) | Lists current/available providers; signed-in linking verifies external info against the current Identity ID. Removal calls `RemoveLoginAsync` and refreshes sign-in. | Identity APIs used by scaffolded handlers. The UI shows removal only with a password or multiple logins; do not confuse this UI condition with an additional POST-handler check. |
| Password / two-factor navigation | Items are commented out in [_ManageNav.cshtml](../../ServerCore/Areas/Identity/Pages/Account/Manage/_ManageNav.cshtml). | Not exposed by this navigation. Enabling support is more than uncommenting links; review UI, endpoints, and the callback's two-factor bypass. |
| Email confirmation | Profile verification button is commented out. The send-verification handler creates a token/URL but does not send mail. | Incomplete application surface. Do not enable confirmed-email requirements without completing and exercising the flow. |
| [Personal data UI](../../ServerCore/Areas/Identity/Pages/Account/Manage/PersonalData.cshtml) | Displays "Coming Soon"; export/delete controls are commented out. | Not an exposed complete feature. Commented controls do not establish that all backing endpoints are unreachable. |
| [Download handler](../../ServerCore/Areas/Identity/Pages/Account/Manage/DownloadPersonalData.cshtml.cs) | Exports `[PersonalData]` properties of `IdentityUser`; has a TODO to integrate `PuzzleUser`. | Scaffolded partial implementation, not a full domain-data export. |
| [Delete handler](../../ServerCore/Areas/Identity/Pages/Account/Manage/DeletePersonalData.cshtml.cs) | Calls `UserManager.DeleteAsync`, signs out, and has a TODO to integrate `PuzzleUser` deletion. | Not a complete application-account cleanup workflow; domain associations/retention need design before exposure. |
| [Global-admin account editor](../../ServerCore/Areas/Identity/Pages/Account/Edit.cshtml.cs) | Separate page protected by `IsGlobalAdmin`, saving a `PuzzleUser`. | Domain administration, not the self-service profile editor. |

**Framework:** Confirmed-email, confirmed-phone, and confirmed-account sign-in
requirements default to false in
[SignInOptions](https://github.com/dotnet/aspnetcore/blob/v10.0.5/src/Identity/Extensions.Core/src/SignInOptions.cs).
The application's registrations do not enable them.

**Team choice:** Those are code-configurable Identity options, not existing
environment-variable toggles. New-account confirmation calls `SignInAsync`
directly, so any new confirmation requirement must also be wired into that
flow rather than assumed to gate every sign-in automatically.

## Before changing this area

Trace both existing-account and new-account paths. Check Identity and domain
records together, avoid conflating the two email fields, and preserve local
return redirects and transaction boundaries. If exposing scaffolded features,
complete their domain behavior and server-side checks first.

Use the [development guide](development-and-troubleshooting.md#validation-checklist)
for concrete scenarios.
