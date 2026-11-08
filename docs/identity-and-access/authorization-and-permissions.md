# Policy reference

[Start here](README.md) | [Adding policies and debugging](development-and-troubleshooting.md)

Use this as a lookup, not a list to memorize. Policies are registered in
[Startup.cs](../../ServerCore/Startup.cs); requirements and handlers live in
[UserAuthorizationPolicy](../../ServerCore/Areas/Identity/UserAuthorizationPolicy).
Most checks delegate to [AuthorizationHelper](../../ServerCore/Areas/Identity/UserAuthorizationPolicy/AuthorizationHelper.cs).

## Our conventions

**URL mode is not permission.** `eventRole` selects `admin`, `author`, `play`,
`archive`, or `impersonateteam-<id>`. A user must also have permission for that
mode. Missing or unrecognized modes parse as `play`.

The helper reads `eventId`, `eventRole`, `puzzleId`, and `teamId` from **route
values**, not arbitrary query/body fields. Match the `.cshtml` or controller
route to the policy. For example,
`/{eventId}/admin/Events/Details` needs event-admin membership, not just sign-in.
[EventSpecificPageModel](../../ServerCore/ModelBases/EventSpecificPageModel.cs)
also binds the event and rejects missing events or disallowed modes; it does
not require registration or a team on every `play` page.

**Membership is application data, not Identity roles.**
[PuzzleUser](../../Data/DataModel/PuzzleUser.cs) checks `EventAdmins`,
`EventAuthors`, `PlayerInEvent`, and `TeamMembers`. Registration and team
membership are separate. Global admin is not an automatic event-policy override.
Staff assignment must set `MayBeAdminOrAuthor` as well as membership; `false`
skips staff queries. Follow the [staff picker](../../ServerCore/Pages/Events/AddAdminOrAuthorPicker.razor.cs).

## Named policies

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
| `PlayerCanSeePuzzle` | Archive availability or applicable puzzle visibility; details below. |
| `IsMicrosoftOrCommunity` | Identity email ends with `@microsoft.com` OR profile has `MayBeAdminOrAuthor`; not merely use of Microsoft login. |

Plain `[Authorize]` requires sign-in. `[Authorize(Policy = "PlayerCanSeePuzzle")]`
instead requires that named policy to succeed; it does **not** automatically
require sign-in too. Our `PlayerCanSeePuzzle` policy can succeed for a released
archive without a signed-in user.

To require both sign-in and the policy, add `RequireAuthenticatedUser()` to the
policy or apply both plain `[Authorize]` and the named policy attribute.
However, `[AllowAnonymous]` bypasses both attributes, so anonymous archive pages
must explicitly check access in their handlers. Before changing these rules,
check the policy and handler logic to avoid blocking public archives or exposing
non-archive operations.

### Puzzle visibility and resource boundaries

`PlayerCanSeePuzzleCheck` allows archive mode when `CanArchive`. Otherwise it
requires a profile, event, and puzzle, then allows released answers or alpha
testing; failing those, it checks single-player global/personal unlocks or the
caller's team unlock. Impersonation can select a team after an event-admin or
puzzle-author check. This policy alone is **not** a registration/team check:
[Teams/Hints](../../ServerCore/Pages/Teams/Hints.cshtml.cs) also requires
`PlayerIsOnTeam`, and both must succeed.

`CanArchive` equals `AreAnswersAvailableNow`, which is true strictly after
`AnswersAvailableBegin` in [Event.cs](../../Data/DataModel/Event.cs).

Validate record existence and event/resource relationships. In particular,
`IsPuzzleAdminCheck` and `IsPuzzleAuthorCheck` resolve an event and puzzle but
do not verify that the puzzle belongs to the URL event. Some helper branches
also assume records exist. A passing policy is not a substitute for that
validation. Routes without this context need explicit/resource-aware checks.

File access uses separate checks in [FilesController](../../ServerCore/Pages/FilesController.cs),
including file type and unlock/solve state. Thread operations have checks in
[PuzzleThreadService](../../ServerCore/Pages/Threads/PuzzleThreadService.cs).
Do not assume page access grants every downstream operation.
