# Authorization approaches and policy lookup

[Start here](README.md) | [Changing and debugging auth](development-and-troubleshooting.md)

## 1. Attributes: a rule for the whole endpoint

Use an attribute when the same access rule applies to the whole Razor Page or
controller action. For example, [Events/Details](../../ServerCore/Pages/Events/Details.cshtml.cs)
uses `[Authorize(Policy = "IsEventAdmin")]` because the page is for event admins.

| Attribute | Meaning in this app |
| --- | --- |
| `[Authorize]` | Require sign-in. It does not check event registration, team membership, or record ownership. |
| `[Authorize(Policy = "...")]` | Require the named policy to succeed. Choose an existing rule from the lookup below. Multiple policy attributes require **all** of those policies. |
| `[AllowAnonymous]` | Skip the framework's enforcement of `[Authorize]` for this endpoint. Our code can still deny access. |

Put attributes on the **PageModel class**, not `OnGet`/`OnPost` methods: they
apply to the page's handlers as a group. MVC controllers can put them on the
controller or individual actions. See [Razor Pages attribute guidance](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/security/authorization/simple?view=aspnetcore-10.0).

**A named policy doesn't automatically require sign-in.** For example,
`PlayerCanSeePuzzle` can succeed for a released archive without a signed-in
user. To require both, add `RequireAuthenticatedUser()` to the policy or apply
plain `[Authorize]` alongside the named attribute.

**`[AllowAnonymous]` does not mean "everyone can do everything here."** It means
the framework won't reject the request based on `[Authorize]`, including
inherited attributes. An attribute policy on that endpoint won't provide
protection, but authentication and our page filter, handler, and service checks
still run. A signed-in visitor can still be identified. Our
[PuzzleThread page](../../ServerCore/Pages/Threads/PuzzleThread.cshtml.cs) has
`[AllowAnonymous]` yet its `OnGetAsync` challenges callers without a profile.
For a page that permits signed-out visitors, [Login](../../ServerCore/Areas/Identity/Pages/Account/Login.cshtml.cs)
is a simpler example.

## 2. Page filter: shared checks after event binding

Use [EventSpecificPageModel](../../ServerCore/ModelBases/EventSpecificPageModel.cs)
for Razor Pages that need our event context and mode rules. Its
`OnPageHandlerExecutionAsync` runs after model binding and before the selected
handler. It loads `LoggedInUser`, rejects a missing event, and checks the mode:

- `admin` requires event-admin membership; `author` requires event-author membership.
- `archive` requires `CanArchive`; impersonation requires event-admin or event-author membership.
- `play` does not itself require sign-in, registration, or a team.

This is shared protection, **not** permission to access every puzzle, file, or
team in the event. Inherit the base model and provide `eventId`/`eventRole` in
the route, as [Events/Details.cshtml](../../ServerCore/Pages/Events/Details.cshtml)
does. If overriding the filter, preserve the base checks and don't let the
handler run after denial. Don't put page-specific ownership rules in the shared
base just to avoid a check in one handler.

The wiki's [PageFilter example](https://github.com/PuzzleServer/mainpuzzleserver/wiki/PageFilter-and-on-page-authorization-check-example)
illustrates the mechanism, but uses the removed `IsEventPlayer` policy and
blocking `.Result` calls. Follow the current base implementation instead.

## 3. In-code checks: a rule for the resource or operation

Use explicit checks when permission depends on a loaded record, the operation,
or a branch that a page-wide attribute cannot express. Reuse
[PuzzleUser](../../Data/DataModel/PuzzleUser.cs) membership methods and
[UserEventHelper](../../ServerCore/Helpers/UserEventHelper.cs) where they fit.
A helper returning `false` doesn't stop a request: the caller must deny access
or exclude unauthorized records before returning data or performing a mutation.

**Loaded record:** [Puzzles/Feedback](../../ServerCore/Pages/Puzzles/Feedback.cshtml.cs)
shows how all three approaches work together. Its attribute admits event
admins/authors; the base filter checks event/mode; its handler then loads the
puzzle within that event and denies an author who doesn't own that puzzle:

```csharp
if (EventRole == EventRole.author &&
    !await UserEventHelper.IsAuthorOfPuzzle(_context, puzzle, LoggedInUser))
{
    return Forbid();
}
```

Without a selected puzzle, the handler instead limits the author's query to
their puzzles. This is why a broad staff attribute alone isn't sufficient.

**Conditional access:** [Puzzles/Play](../../ServerCore/Pages/Puzzles/Play.cshtml.cs)
uses `[AllowAnonymous]` to let archive requests reach the code. The base filter
still rejects unavailable archives; `OnGetAsync` challenges non-archive callers
without a profile and selects only visible puzzles. Adding a POST to that page
would need its own operation checks: the GET checks don't protect another handler.

**Services/controllers:** [FilesController](../../ServerCore/Pages/FilesController.cs)
requires sign-in, then calls `IsAuthorized` for the actual file type and
unlock/solve state. [PuzzleThreadService](../../ServerCore/Pages/Threads/PuzzleThreadService.cs)
checks message ownership or staff membership for mutations, independently of
the page filter. Keep checks at the operation boundary; don't rely on an earlier
page load or component button. A new caller must obtain the acting user from
trusted server-side identity, not a user ID supplied by the request.

For a reusable loaded-resource rule, the framework also supports explicit
`IAuthorizationService.AuthorizeAsync(User, resource, policyName)`;
see [resource-based authorization](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/security/authorization/resource-based?view=aspnetcore-10.0).
That requires a handler that uses the resource. Our current `AuthorizationHelper`
policies read the URL instead; passing a resource won't change their behavior.
Explicit authorization calls are not disabled by `[AllowAnonymous]`.

## Local policy lookup

Policies/handlers are registered in [Startup](../../ServerCore/Startup.cs) and
implemented in [UserAuthorizationPolicy](../../ServerCore/Areas/Identity/UserAuthorizationPolicy).
Most delegate to [AuthorizationHelper](../../ServerCore/Areas/Identity/UserAuthorizationPolicy/AuthorizationHelper.cs),
which reads `eventId`, `eventRole`, `puzzleId`, and `teamId` from **route values**,
not arbitrary query/body fields.

URL mode selects an operating mode, not a permission. Missing/unrecognized modes
parse as `play`. Membership lives in application records, not Identity roles:
registration and team membership are separate; global admin does not automatically
override event policies. Staff assignment must also set `MayBeAdminOrAuthor`,
or event staff queries are skipped; follow the
[staff picker](../../ServerCore/Pages/Events/AddAdminOrAuthorPicker.razor.cs).

| Policy | Required access / context |
| --- | --- |
| `IsGlobalAdmin` | Profile's `IsGlobalAdmin` flag; no URL mode dependency. |
| `IsEventAdmin` | Event-admin membership in `admin` mode. |
| `IsEventAuthor` | Event-author membership in `author` mode. |
| `IsAuthorOfPuzzle` | Puzzle authorship in `author` mode, with a resolvable event. |
| `IsEventAdminOrEventAuthor` | Either mode-specific event check above. |
| `IsEventAdminOrAuthorOfPuzzle` | Admin of the puzzle's event in `admin` mode OR puzzle author in `author` mode. |
| `PlayerIsOnTeam` | In `play`, caller's event team matches route `teamId`. In impersonation, target team belongs to the event and caller is its event admin. |
| `IsEventAdminOrPlayerOnTeam` | Event admin in `admin` mode OR `PlayerIsOnTeam`, including impersonation. |
| `IsRegisteredForEvent` | Admin, author, or registered player in the matching mode; archive succeeds when `CanArchive`. No team required. |
| `PlayerCanSeePuzzle` | Available archive; otherwise a profile, event, and puzzle plus released answers, alpha testing, or global/personal/team unlock. Impersonation selects a team after an event-admin or puzzle-author check. |
| `IsMicrosoftOrCommunity` | Identity email ends with `@microsoft.com` OR profile has `MayBeAdminOrAuthor`; not merely use of Microsoft login. |

`PlayerCanSeePuzzle` alone doesn't check registration/team membership.
[Teams/Hints](../../ServerCore/Pages/Teams/Hints.cshtml.cs) also requires
`PlayerIsOnTeam`. `CanArchive` is true strictly after `AnswersAvailableBegin`
in [Event.cs](../../Data/DataModel/Event.cs).

Validate record existence and event/resource relationships. In particular,
`IsPuzzleAdminCheck` and `IsPuzzleAuthorCheck` don't verify that the puzzle
belongs to the URL event, and some helper branches assume records exist.
A passing policy is not a substitute for those checks.
