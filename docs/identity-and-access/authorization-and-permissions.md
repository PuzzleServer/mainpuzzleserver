# Identity and Access - Authorization and Permissions

[Section home](README.md) | [Account lifecycle](sign-in-and-account-lifecycle.md) |
[Development](development-and-troubleshooting.md)

## Where permissions come from

**PuzzleServer** stores permissions in application records, not ASP.NET Identity
roles. See [PuzzleUser](../../Data/DataModel/PuzzleUser.cs) and
[UserEventHelper](../../ServerCore/Helpers/UserEventHelper.cs).

| Record / flag | Meaning |
| --- | --- |
| `PuzzleUser.IsGlobalAdmin` | Global administration; not an automatic override for every event policy. |
| `EventAdmins` / `EventAuthors` | Admin or author membership for one event. |
| `PuzzleAuthors` | Authorship of one puzzle, distinct from event authorship. |
| `PlayerInEvent` / `TeamMembers` | Event registration and team membership, respectively. One does not imply the other. |
| `PuzzleUser.MayBeAdminOrAuthor` | A shortcut flag: `false` skips event staff membership queries; `true` alone does not grant that membership. The community policy uses the flag directly. |
| Team / individual puzzle state | Unlock information used for player access. |

The [staff picker](../../ServerCore/Pages/Events/AddAdminOrAuthorPicker.razor.cs)
sets `MayBeAdminOrAuthor` when assigning staff. Any new assignment path must do
the same. [Event creation](../../ServerCore/Pages/Events/Create.cshtml.cs) gives
its creator explicit admin and author memberships; these are not inherited from
global-admin status.

**Team choice:** Staff assignments and event settings are administrative data.
Changing the rules that interpret those records requires code.

## URL roles select a mode, not a permission

[AuthorizationHelper](../../ServerCore/Areas/Identity/UserAuthorizationPolicy/AuthorizationHelper.cs)
reads `eventId`, `eventRole`, `puzzleId`, and `teamId` from URL route values.
[EventRole.Parse](../../ServerCore/ModelBases/EventRole.cs) recognizes `admin`,
`author`, `play`, `archive`, and `impersonateteam-<id>`; missing or unrecognized
values fall back to `play`.

For example, an event author cannot use an `admin` URL just because they are
staff. The requested mode must match a permission they actually hold.

```text
/{eventId}/admin/Events/Details
    -> IsEventAdmin policy
    -> IsAdminInEventHandler
    -> AuthorizationHelper checks admin mode and event-admin membership
    -> EventSpecificPageModel binds the event and checks the operating mode
    -> page handler runs
```

[EventSpecificPageModel](../../ServerCore/ModelBases/EventSpecificPageModel.cs)
rejects missing events and disallowed admin, author, archive, or impersonation
modes. It does not automatically require registration or team membership for
every `play` page.

**Route-context limitation:** These helpers generally read the URL, not an
explicit resource passed to authorization. IDs supplied only in a query string
or request body do not automatically become that context. Some branches assume
records exist and can throw instead of cleanly denying an invalid request.

Also, `IsPuzzleAdminCheck` and `IsPuzzleAuthorCheck` resolve both event and puzzle,
but do not check that the puzzle belongs to the event named in the URL. For
example, a request naming Event B and a puzzle from Event A may pass the puzzle
permission check; other endpoint checks may still reject it. Callers must
validate the relationship. This is a limitation of these helpers, not proof
that every affected endpoint permits unauthorized access.

## Policy reference

**Framework:** Plain `[Authorize]` requires a signed-in user through the default
policy. A named policy uses requirements, which are individual permission
conditions. All requirements must succeed. Multiple handlers can satisfy the
same requirement as alternatives; explicitly failing a requirement can block
success.

**PuzzleServer:** Policies and handlers are registered in
[Startup](../../ServerCore/Startup.cs). Our "admin OR author" policies use one
requirement with separate handlers. Two requirements would mean "admin AND
author." See [framework policy documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-10.0).

| Policy | What satisfies it |
| --- | --- |
| `IsGlobalAdmin` | Current profile has `IsGlobalAdmin`; no URL role dependency. |
| `IsEventAdmin` | Event-admin membership in `admin` mode. |
| `IsEventAuthor` | Event-author membership in `author` mode. |
| `IsAuthorOfPuzzle` | Puzzle-author membership in `author` mode; the helper also requires an event to resolve. See the route-context limitation above. |
| `IsEventAdminOrEventAuthor` | Either of the corresponding mode-specific event checks. |
| `IsEventAdminOrAuthorOfPuzzle` | In `admin` mode, administration of the puzzle's own event; OR puzzle authorship in `author` mode. Uses the puzzle helpers described above. |
| `PlayerIsOnTeam` | In `play`, caller's team for the event matches URL `teamId`. In impersonation, that team belongs to the event and caller is its event admin. |
| `IsEventAdminOrPlayerOnTeam` | Event admin in `admin` mode OR the `PlayerIsOnTeam` check, including its impersonation branch. |
| `IsRegisteredForEvent` | Admin, author, or registered player in the matching mode; archive mode succeeds when `CanArchive`. Does not require a team. |
| `PlayerCanSeePuzzle` | Archive/answer availability, alpha testing, or an applicable unlock; details below. |
| `IsMicrosoftOrCommunity` | Identity email ends with `@microsoft.com` OR profile has `MayBeAdminOrAuthor`. This is custom eligibility, not a Microsoft-login or global-admin check. |

The matrix describes requirements, not the entire endpoint. Additional policies
and handler checks may apply; archive success alone does not remove a separate
authentication requirement.

### Puzzle access and impersonation

`PlayerCanSeePuzzleCheck` evaluates these branches in order:

1. In archive mode, allow when `CanArchive`, then return.
2. Otherwise require a profile, puzzle, and event. Released answers or an
   alpha-testing event satisfy the requirement.
3. For single-player puzzles, check a global unlock, then the caller's unlock.
4. For team puzzles, require the caller's team unlock. Impersonation can select
   a team after an event-admin or puzzle-author check.

This is not a registration/team-membership policy on its own.
[Teams/Hints](../../ServerCore/Pages/Teams/Hints.cshtml.cs), for example, also
requires `PlayerIsOnTeam`; both policies must succeed.

In [Event.cs](../../Data/DataModel/Event.cs), `AreAnswersAvailableNow` becomes
true strictly after `AnswersAvailableBegin`; `CanArchive` returns the same value.
The team can adjust the event timing as data. Changing access branches is code.

Impersonation means viewing a selected team, not signing in as another user.
The shared page filter accepts event admins or event authors in that mode,
but individual requirements can be stricter: the team policy requires an event
admin, while puzzle visibility accepts an event admin or that puzzle's author.

## Checks outside named policies

Do not assume a policy attribute or hidden navigation link tells the whole story.

| Surface | Where to look |
| --- | --- |
| Pages | Check attributes, `.cshtml` routes, inherited filters, and handlers. [Puzzles/Play](../../ServerCore/Pages/Puzzles/Play.cshtml.cs) allows anonymous metadata but challenges non-archive callers without a profile. |
| Files | [FilesController](../../ServerCore/Pages/FilesController.cs) requires authentication and separately checks file type, staff, unlock/solve state, alpha testing, and released answers. Its route has no `eventRole`. |
| API | [PuzzleApiController](../../ServerCore/Pages/PuzzleApiController.cs) applies puzzle visibility to `PostSubmitAnswerAsync`; other actions have different checks. |
| Threads | [PuzzleThreadModel](../../ServerCore/Pages/Threads/PuzzleThread.cshtml.cs) checks page access; [PuzzleThreadService](../../ServerCore/Pages/Threads/PuzzleThreadService.cs) checks sender, ownership, and staff privileges for operations. |
| Server messages | [ServerMessageHub](../../ServerCore/ServerMessages/ServerMessageHub.cs) is a broadcast bridge, not a policy-protected player API. Extensions target the `servers` group, not all connected clients. |

`Startup` declares folder conventions for `/Pages` and `/ModelBases`; those
literal paths do not prove every page or controller is protected. Verify the
actual endpoint and operation. For implementation steps, see
[change recipes](development-and-troubleshooting.md#change-recipes).
