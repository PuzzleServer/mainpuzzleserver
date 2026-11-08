# Identity and Access - Authorization and Permissions

[Section home](README.md) | [Account lifecycle](sign-in-and-account-lifecycle.md) |
[Development](development-and-troubleshooting.md)

## Start here

Read policy/handler registration in [Startup.cs](../../ServerCore/Startup.cs),
then [AuthorizationHelper.cs](../../ServerCore/Areas/Identity/UserAuthorizationPolicy/AuthorizationHelper.cs),
[EventRole.cs](../../ServerCore/ModelBases/EventRole.cs), and
[EventSpecificPageModel.cs](../../ServerCore/ModelBases/EventSpecificPageModel.cs).
Do not infer a policy's behavior from its name or XML comment alone.

## Framework mechanics versus application rules

**Framework:** `[Authorize]` without a named policy uses the default policy,
which requires an authenticated user. Named policies consist of requirements;
every requirement in a policy must succeed. Multiple handlers for the same
requirement can implement alternatives: any handler can mark that requirement
successful. Explicit failure can prevent success, so it is not interchangeable
with declining to call `Succeed`.

See [policy-based authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0).

**PuzzleServer:** Named requirements generally delegate to `AuthorizationHelper`.
Composite "or" policies register multiple handlers for one requirement, rather
than adding two separate requirements. For example,
[IsEventAdminOrEventAuthor.cs](../../ServerCore/Areas/Identity/UserAuthorizationPolicy/IsEventAdminOrEventAuthor.cs)
has separate admin and author handlers.

**Team choice:** Policy names, handler registration, route conventions, and
membership rules are code-level decisions. Staff/team assignments and event
settings are administrative data, not Identity role configuration.

## Permission data

| Record / flag | Meaning |
| --- | --- |
| `PuzzleUser.IsGlobalAdmin` | Satisfies the dedicated global-admin requirement. It is not automatically OR-ed into other policies. |
| `EventAdmins` | Administrative relationship for a specific event. |
| `EventAuthors` | Author relationship for a specific event. |
| `PuzzleAuthors` | Author relationship for a particular puzzle; distinct from event authorship. |
| `PlayerInEvent` | The person completed event registration. |
| `TeamMembers` | The person belongs to a team; registration alone does not establish this. |
| `PuzzleUser.MayBeAdminOrAuthor` | Fast-path flag used by event staff checks. `false` short-circuits those checks; `true` alone does not grant event staff membership. The community policy also uses it directly. |
| Team / individual puzzle state | Determines puzzle access when normal unlock checks apply. |

The active implementations are [PuzzleUser](../../Data/DataModel/PuzzleUser.cs)
and [UserEventHelper](../../ServerCore/Helpers/UserEventHelper.cs).
The [staff picker](../../ServerCore/Pages/Events/AddAdminOrAuthorPicker.razor.cs)
sets `MayBeAdminOrAuthor` when adding an event staff association.
Do not introduce new assignment paths that omit this invariant.

## URL context and request trace

`AuthorizationHelper` gets context from `HttpContext.Request.RouteValues`:

| Value | Resolution |
| --- | --- |
| `eventId` | Event lookup through `EventHelper.GetEventFromEventId`. |
| `eventRole` | `EventRole.Parse`: `admin`, `author`, `play`, `archive`, or `impersonateteam-<id>`. Missing/unrecognized values fall back to `play`. |
| `puzzleId` | Parsed as an integer and loaded from `Puzzles`. |
| `teamId` | Parsed as an integer and loaded from `Teams`. |

These policies do not generally use the authorization `Resource` parameter to
get this context. A policy added to a differently shaped route, a query-string
only resource ID, or a background operation will not automatically see the
same data. Some helper branches assume resolved entities are non-null; missing
context can cause an exception rather than a clean denial.

A representative protected page is
[Events/Details](../../ServerCore/Pages/Events/Details.cshtml.cs):

```text
/{eventId}/{eventRole}/Events/Details
    -> [Authorize(Policy = "IsEventAdmin")]
    -> IsAdminInEventHandler
    -> AuthorizationHelper.IsEventAdminCheck
       -> requested mode must be admin
       -> resolve signed-in PuzzleUser and event
       -> check that user's event-admin relationship
    -> EventSpecificPageModel binds event/role
       -> rejects missing event or disallowed operating mode
    -> page handler
```

**PuzzleServer:** `EventSpecificPageModel.OnPageHandlerExecutionAsync` adds
checks for admin, author, archive, and impersonation modes before executing
handlers. It does not make every `play` page registered-only or team-only.
Individual pages still need the appropriate policy or explicit checks.

## Named policy matrix

All names below are registered in `Startup`. The requirement files are under
[UserAuthorizationPolicy](../../ServerCore/Areas/Identity/UserAuthorizationPolicy).
The matrix summarizes current helper branches, not a universal access hierarchy.

| Policy | Actual condition / operating mode |
| --- | --- |
| `IsGlobalAdmin` | Current `PuzzleUser.IsGlobalAdmin`; no event role dependency. |
| `IsEventAdmin` | `admin` mode plus event-admin membership. |
| `IsEventAuthor` | `author` mode plus event-author membership. |
| `IsAuthorOfPuzzle` | `author` mode plus puzzle-author membership; helper also requires a resolved event. |
| `IsEventAdminOrEventAuthor` | Admin check in `admin` mode OR event-author check in `author` mode. |
| `IsEventAdminOrAuthorOfPuzzle` | In `admin` mode, administration of the event containing the puzzle; OR puzzle authorship in `author` mode. This uses `IsPuzzleAdminCheck`, not the general event-admin helper. |
| `IsEventAdminOrPlayerOnTeam` | Event admin in `admin` mode OR the route-team membership check; the latter has a separate impersonation branch. |
| `PlayerIsOnTeam` | In `play` mode, caller's team for the event must match route `teamId`. In impersonation mode, route team must belong to the event and caller must be its event admin. |
| `IsRegisteredForEvent` | Admin, author, or registered player in the corresponding mode; `archive` succeeds when `Event.CanArchive`. Team membership is not required. |
| `PlayerCanSeePuzzle` | Archive availability, released answers/alpha testing, or applicable team/individual unlock state; see details below. |
| `IsMicrosoftOrCommunity` | `IdentityUser.Email` ends with `@microsoft.com`, OR the domain user's `MayBeAdminOrAuthor` is true. This is a custom eligibility rule, not provider authentication or a global-admin test. |

Selecting an `admin` URL does not let an author satisfy an author-mode branch;
the requested mode matters even when the person has other memberships.
Likewise, archive success in a custom requirement does not automatically erase
authentication requirements imposed by other metadata on an endpoint.

### Puzzle visibility

`PlayerCanSeePuzzleCheck` branches in this order:

1. In `archive` mode, succeed if `Event.CanArchive`, then return.
2. Otherwise resolve a domain user, puzzle, and event. Answers currently available
   or an alpha-testing event satisfy this requirement.
3. For single-player puzzles, check a global unlock first, then the caller's
   individual unlock state.
4. For team puzzles, resolve the caller's team, or in impersonation mode a
   selected team after an event-admin/puzzle-author check, then require an unlock.

This requirement is not a team-membership or registration requirement by itself.
Some operations combine it with additional metadata. For example,
[Teams/Hints](../../ServerCore/Pages/Teams/Hints.cshtml.cs) also uses
`PlayerIsOnTeam`; stacked policies must both succeed.

**Team choice:** Unlock state and event settings affect access as data.
`AreAnswersAvailableNow` in [Event.cs](../../Data/DataModel/Event.cs) becomes true
strictly after `AnswersAvailableBegin`; `CanArchive` currently returns that same
value. Inspect that timing before adjusting an event. Changing branch order or
visibility semantics is a code change with effects on pages and API actions
using this policy.

### Impersonation is a separate mode

`impersonateteam-<id>` allows supported staff views to resolve a selected team.
It is not signing in as another Identity user.
The page-model filter permits event admins or event authors into this mode,
but individual requirements can be stricter: `PlayerIsOnTeam` requires event
admin membership, and puzzle visibility uses an event-admin/puzzle-author check.
Do not assume every author can perform every impersonated operation.

## Enforcement beyond named policies

| Surface | What to inspect |
| --- | --- |
| Razor Pages | `[Authorize]` / `[AllowAnonymous]`, the `.cshtml` route, inherited filters, and handler-specific checks. Attribute inspection alone is insufficient. |
| Anonymous-capable pages | [Puzzles/Play](../../ServerCore/Pages/Puzzles/Play.cshtml.cs) permits anonymous metadata but challenges non-archive callers without a domain user; archive mode still checks availability. |
| File access | [FilesController](../../ServerCore/Pages/FilesController.cs) uses `[Authorize]` plus its own file-type, staff, unlock, solve, alpha-testing, and answer-release logic. Its route lacks `eventRole`; it does not reuse the policy helper. |
| Puzzle API | [PuzzleApiController](../../ServerCore/Pages/PuzzleApiController.cs) applies `PlayerCanSeePuzzle` to `PostSubmitAnswerAsync`; other actions use different checks. Do not assume the whole controller shares this policy. |
| Thread components/services | [PuzzleThreadModel](../../ServerCore/Pages/Threads/PuzzleThread.cshtml.cs) performs explicit page checks; [PuzzleThreadService](../../ServerCore/Pages/Threads/PuzzleThreadService.cs) checks sender, team/individual ownership, and staff privileges for operations. Parent page authorization does not replace service-operation checks. |
| Server-message hub | [ServerMessageHub](../../ServerCore/ServerMessages/ServerMessageHub.cs) is a server broadcast bridge, not a policy-protected player API. Extensions target the `servers` group; do not infer player authorization from a hub connection or broadcast to all clients. |

`Startup` also declares Razor Page folder conventions for `/Pages` and
`/ModelBases`. Those literal convention paths should not be treated as proof
that every page or controller is globally protected. Preserve and verify actual
endpoint metadata and checks when extending the application.

## Making a permission change

Use an existing policy when its exact route and membership semantics match.
Otherwise define the requirement, register the policy and every handler, and
wire all relevant entry points. Keep alternative handlers attached to a single
requirement for OR semantics. Validate resolved resources and their event
relationships rather than treating caller-supplied IDs as authority.

Review administrative assignment paths as well as enforcement paths. For
example, global-admin-created events add their creator as both event admin and
event author in [Events/Create](../../ServerCore/Pages/Events/Create.cshtml.cs);
this is explicit application behavior, not inheritance from `IsGlobalAdmin`.

See the [change recipes and validation checklist](development-and-troubleshooting.md#change-recipes).
