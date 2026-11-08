# Identity and Access

Start here when protecting a page or changing auth. The linked references are
for the task at hand, not an onboarding reading assignment.

## We use ASP.NET Core auth, not a home-grown replacement

ASP.NET Core Identity handles accounts and cookies; the Microsoft Account
provider handles sign-in. We use the framework's authorization attributes,
policies, requirements, and handlers. Our custom part is deciding access from
event membership, team membership, and puzzle state.

**The book:** Microsoft's [policy-based authorization guide](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0).
For a walkthrough, watch [Advanced ASP.NET Core Authorization with Barry Dorrans](https://learn.microsoft.com/en-us/shows/seth-juarez/advanced-aspnet-core-authorization-barry-dorrans).
For runnable code, use the official [AuthorizationPoliciesSample](https://github.com/dotnet/AspNetCore.Docs.Samples/tree/main/security/authorization/policies/6.0/AuthorizationPoliciesSample),
especially `MinimumAgeRequirement`, `MinimumAgeHandler`, and policy registration.
The video and sample explain the pattern; they predate our `net10.0` app.
Use current docs for APIs, and our scoped registrations for database-backed handlers.

This continues the existing wiki's [Authentication Resources](https://github.com/PuzzleServer/mainpuzzleserver/wiki/Authentication-Resources)
and [Roles](https://github.com/PuzzleServer/mainpuzzleserver/wiki/Roles).
For Microsoft provider setup, use [Microsoft Account authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/microsoft-logins?view=aspnetcore-10.0)
alongside our [External Authentication Setup](https://github.com/PuzzleServer/mainpuzzleserver/wiki/External-Authentication-Setup).
The wiki's older framework examples are background, not current copy/paste templates.

## Which attribute should I use?

| Attribute | Use it for | Watch out for |
| --- | --- | --- |
| `[AllowAnonymous]` | Intentionally public pages, login, or public archive access. | It bypasses attribute-based authorization, including inherited `[Authorize]` policies. Do not combine them expecting the policy to protect part of the page. Check non-public operations explicitly. |
| `[Authorize]` | An operation requiring sign-in but no additional permission at the attribute layer. | Sign-in does **not** establish a profile, event registration, team membership, ownership, or staff access. Check those separately where needed. |
| `[Authorize(Policy = "...")]` | An operation requiring a specific application permission. | Reuse a [named policy](authorization-and-permissions.md), supply its route context, and validate the requested resource. Multiple policy attributes mean **AND**, not OR. |

Put attributes on the **PageModel class**, not `OnGet`/`OnPost` methods.
For different handler permissions, use separate pages or explicit server-side
checks; see [Razor Pages attribute guidance](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/security/authorization/simple?view=aspnetcore-10.0).
MVC controllers can apply attributes to the controller or individual actions.
Hidden links and buttons are not access control; protect POSTs, APIs, files,
and service operations as well as the page.

**Local examples:** [Login](../../ServerCore/Areas/Identity/Pages/Account/Login.cshtml.cs)
is anonymous; [Player/Details](../../ServerCore/Pages/Player/Details.cshtml.cs)
uses plain authorization; [Events/Details](../../ServerCore/Pages/Events/Details.cshtml.cs)
uses `IsEventAdmin`.

**Archive exception:** [Puzzles/Play](../../ServerCore/Pages/Puzzles/Play.cshtml.cs)
is anonymous so an archive crawler needs no account. Its handler challenges
non-archive callers without a profile and forbids archiving before `CanArchive`.
It also filters visible puzzles. Copy the complete access checks, not just the
attribute, and check every handler you add.

## Where do I go next?

- [Policy reference](authorization-and-permissions.md): choose an existing rule and understand our route/membership conventions.
- [Adding policies and debugging](development-and-troubleshooting.md): implementation steps and lessons from actual fixes.
- [Sign-in reference](sign-in-and-account-lifecycle.md): account linkage and the two login callbacks, only when working on accounts.

These files are the reviewable source. Merging them does not publish the wiki.
When publishing, use an **Identity and Access** landing page with the three
reference pages above, convert relative links to wiki/source URLs, and preserve
the existing setup links.
