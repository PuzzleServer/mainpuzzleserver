# Identity and Access

Authentication identifies the signed-in account. Authorization decides what
that account can do. Microsoft Account and ASP.NET Core Identity handle sign-in;
our event memberships, teams, and puzzle state determine permissions.

## Which authorization approach do I need?

PuzzleServer uses three approaches, often together. These are **not** the three
variants of the `[Authorize]`/`[AllowAnonymous]` attributes.

| What are you protecting? | Approach | Start with |
| --- | --- | --- |
| A whole page or controller action with one rule, such as event-admin access. | **Attributes:** declare the entry rule; the framework enforces it before the handler runs. | [Attribute authorization](authorization-and-permissions.md#1-attributes-a-rule-for-the-whole-endpoint) |
| Event-aware pages that all need the same event/mode checks after route binding. | **Page filter:** inherit `EventSpecificPageModel` to run the shared checks before each handler. | [Shared page filter](authorization-and-permissions.md#2-page-filter-shared-checks-after-event-binding) |
| A particular record, mutation, or conditional branch, such as editing your own message or viewing public archives but requiring sign-in during play. | **In-code checks:** check permission in the handler/service once the necessary data is available, before reading protected data or changing it. | [Operation-level authorization](authorization-and-permissions.md#3-in-code-checks-a-rule-for-the-resource-or-operation) |

An event-aware staff page usually needs an attribute **and** the shared filter.
Add an operation-level check when those don't cover the requested record or
action. Making a button invisible is not a substitute for any of these checks.

## Read only what you need next

[Authorization approaches and policy lookup](authorization-and-permissions.md)
explains the three models with local examples.
[Changing and debugging auth](development-and-troubleshooting.md) covers
implementation and the paper trail from past fixes.
[Sign-in reference](sign-in-and-account-lifecycle.md) is for account changes.

## The references we build on

Use Microsoft's [policy guide](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0),
[Barry Dorrans walkthrough](https://learn.microsoft.com/en-us/shows/seth-juarez/advanced-aspnet-core-authorization-barry-dorrans),
and [authorization sample](https://github.com/dotnet/AspNetCore.Docs.Samples/tree/main/security/authorization/policies/6.0/AuthorizationPoliciesSample)
for the framework pattern. The video/sample predate our `net10.0` app; use
current docs for APIs and our scoped registrations for database-backed handlers.

The wiki already covers [Roles](https://github.com/PuzzleServer/mainpuzzleserver/wiki/Roles),
[Authentication Resources](https://github.com/PuzzleServer/mainpuzzleserver/wiki/Authentication-Resources),
and [External Authentication Setup](https://github.com/PuzzleServer/mainpuzzleserver/wiki/External-Authentication-Setup).
These guides connect those references to current code, rather than replace them.

These files are reviewed here; merging does not publish the wiki. When publishing,
create an Identity and Access landing page with the three guides, convert relative
links to wiki/source URLs, and preserve existing setup links.
