# Sign-in reference

[Start here](README.md) | [Debugging auth](development-and-troubleshooting.md#debug-a-reported-failure)

Read this when changing account handling, not as a prerequisite for adding a
protected page. For local credentials and provider registration, start with the
[External Authentication Setup wiki](https://github.com/PuzzleServer/mainpuzzleserver/wiki/External-Authentication-Setup)
and the [configuration checks](development-and-troubleshooting.md#what-past-bugs-taught-us).

## Two records, two callbacks

Identity owns the login account (`IdentityUser`) and external-login association.
PuzzleServer owns the profile (`PuzzleUser`), linked by
`PuzzleUser.IdentityUserId = IdentityUser.Id`. Both are stored through
[PuzzleServerContext](../../Data/PuzzleServerContext.cs). Sign-in alone does
not guarantee a matching profile or any event/team permission.

The flow starts in [Login](../../ServerCore/Areas/Identity/Pages/Account/Login.cshtml.cs).
The Microsoft provider redirects to **`/signin-microsoft`**, handled by framework
middleware, then to **`ExternalLogin`'s `Callback` handler**, our account logic.
Don't register the Razor Page callback as the provider redirect URI.

[ExternalLogin.cshtml.cs](../../ServerCore/Areas/Identity/Pages/Account/ExternalLogin.cshtml.cs)
signs returning users in via their provider association. If the provider key
is missing, it can associate an existing Identity account by the provider's
email claim. This custom recovery depends on `RequireUniqueEmail` in `Startup`;
don't disable it independently or change recovery without reviewing the
provider's email guarantees.

New-account confirmation creates the Identity account, provider association,
and profile in a transaction, commits, then signs in. In Development only, the
first global admin can be bootstrapped here. This is not a production procedure.

## Before changing account behavior

**Profile email isn't login email.** The
[profile editor](../../ServerCore/Areas/Identity/Pages/Account/Manage/Index.cshtml.cs)
changes `PuzzleUser.Email`, not `IdentityUser.Email` or username. Recovery uses
the Identity email. If a returning user is prompted to register, inspect the
provider association and account/profile link before creating a duplicate.

Our callback requests non-persistent sign-in and bypasses **application**
two-factor authentication for returning users; this does not bypass Microsoft's
MFA. Account-confirmation requirements aren't enabled, and new-account
confirmation calls `SignInAsync` directly. Enabling confirmation or application
MFA needs coordinated callback/UI changes for both new and returning users.

Password/two-factor navigation is commented out, email verification does not
send mail, and personal-data controls are hidden. Backing handlers still exist.
[Export](../../ServerCore/Areas/Identity/Pages/Account/Manage/DownloadPersonalData.cshtml.cs)
and [deletion](../../ServerCore/Areas/Identity/Pages/Account/Manage/DeletePersonalData.cshtml.cs)
have TODOs for profile data; they aren't complete domain-data workflows.
Don't expose these features by just uncommenting navigation.

Preserve server-side protection of profile IDs and privilege flags, transaction
boundaries, and local redirect validation.
[Logout](../../ServerCore/Areas/Identity/Pages/Account/Logout.cshtml.cs) signs out
on POST, not GET, and does not necessarily end the Microsoft browser session.
