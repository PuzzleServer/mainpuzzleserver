# Changing and debugging auth

[Start here](README.md) | [Authorization approaches](authorization-and-permissions.md) |
[Sign-in reference](sign-in-and-account-lifecycle.md)

## Place the check before adding a policy

Choose the [authorization approach](README.md#which-authorization-approach-do-i-need)
first. A new page doesn't necessarily need a new policy: reuse an
[existing policy](authorization-and-permissions.md#local-policy-lookup) or helper
when it already expresses the permission.
For a check shared by several operations, put it in their shared helper/service
rather than copying conditionals into each page.

## Add a policy

For a genuinely new rule:

1. Write down who can access which resource, including anonymous, archive,
   wrong-event, and impersonation cases. Decide whether sign-in is required.
2. Add an `IAuthorizationRequirement` and an
   `AuthorizationHandler<YourRequirement>` under
   `ServerCore/Areas/Identity/UserAuthorizationPolicy`.
   [IsEventAdmin.cs](../../ServerCore/Areas/Identity/UserAuthorizationPolicy/IsEventAdmin.cs)
   is the small local template: the handler injects `AuthorizationHelper` and
   awaits its check. Reuse helpers only when their route assumptions fit;
   implement new shared checks there when appropriate.
3. Call `context.Succeed(requirement)` only after the rule is satisfied.
   An unmet branch can return without succeeding. For **OR**, follow
   [IsEventAdminOrEventAuthor.cs](../../ServerCore/Areas/Identity/UserAuthorizationPolicy/IsEventAdminOrEventAuthor.cs):
   one requirement, alternative handlers. Do not call `context.Fail()` merely
   because one alternative doesn't match; it would veto the other.
   Multiple requirements are **AND**, and handler order isn't guaranteed.
4. Register the named policy inside `Startup.ConfigureServices`'s
   `AddAuthorization` block, and register every handler with
   `AddScoped<IAuthorizationHandler, YourHandler>()`. Our handlers use scoped
   database services; don't copy the official sample's singleton lifetime.
   Add `RequireAuthenticatedUser()` to a new policy if it must always require
   sign-in rather than relying on a profile lookup or a default policy.
5. Enforce the policy with an attribute or an explicit `AuthorizeAsync` call,
   depending on the approach chosen above. For loaded-resource authorization,
   use a resource-aware handler rather than a URL-dependent helper.
   For URL-dependent policies, include the required IDs/mode in the route.
   Validate that records exist and belong to the intended event. Protect all
   entry points, including mutations reached through APIs or component services.
   Update the policy reference.

## Check the change

Build application changes with `dotnet build ServerCore/ServerCore.csproj`.
Exercise allowed and denied callers in isolated development data: signed out,
signed in without permission, wrong URL mode, global admin without event
membership, invalid IDs, another team's ID, and a puzzle from another event.
For relevant policies, also cover registration without a team, locked/unlocked
puzzles, archive timing, and impersonation. Check GET and POST/API/file paths;
a successful page load is not sufficient.

The current [test project](../../ServerCore.Test/ServerCore.Test.csproj) targets
`netcoreapp2.1`, has no app reference, and contains an
[empty placeholder](../../ServerCore.Test/UnitTest1.cs).
It needs modernization before adding app regression tests; a passing placeholder
does not validate auth. Documentation-only edits do not need application builds.

## Debug a reported failure

Start with the existing [Troubleshooting User Errors playbook](https://github.com/PuzzleServer/mainpuzzleserver/wiki/Troubleshooting-User-Errors):
confirm the intended Microsoft account (try a private window), then match the
failure time/timezone and error-page RequestID to Application Insights Search
or Failures. Record the operation, route, and failing check. Do not paste
credentials, tokens, cookies, or personal profile data into diagnostics.

For a permission failure, follow the checks the operation actually uses:
attribute policy/handler if present, the shared page filter for event-aware
pages, then the handler/service's own check. With `[AllowAnonymous]`, automatic
policy enforcement is skipped but the latter checks still run. Put breakpoints
at those boundaries and inspect route values, profile linkage, memberships/staff
flag, and unlock state. Identify where success stops rather than removing a check.
A sign-in challenge, a permission denial, and a missing record are different
problems; redirects/status codes alone don't identify the cause.

### What past bugs taught us

These are documented fixes, not claims that one debugging technique is always best.

| Failure and paper trail | Useful check / lesson |
| --- | --- |
| Team authorization lacked route context ([auth checks and fixes, PR #278](https://github.com/PuzzleServer/mainpuzzleserver/pull/278)). The fix added team IDs to routes. | Inspect actual `Request.RouteValues`, including `teamId`, before changing membership logic. Check `.cshtml` routes and generated links together. |
| Signed-out users hit a pregame puzzle error ([PR #844](https://github.com/PuzzleServer/mainpuzzleserver/pull/844)). The fix added plain `[Authorize]` to the then-current single-player page. | Reproduce signed out and verify protection on the actual endpoint; folder conventions are not evidence that every page is covered. Today's anonymous archive pages need their explicit non-archive checks. |
| Registration policy checked team membership ([PR #891](https://github.com/PuzzleServer/mainpuzzleserver/pull/891)). The fix switched to `IsRegisteredForEvent`. | Test a registered user with **no team**. Inspect the handler's delegated method, not just the policy name. |
| Single-player access missed personal unlocks and could dereference missing global state ([PR #836](https://github.com/PuzzleServer/mainpuzzleserver/pull/836)). | Inspect both global unlock and per-player state. Reproduce with no global row and a personal unlock; absence of a row should not become an exception. |
| Local Chrome login failed with `Correlation failed` ([issue #746](https://github.com/PuzzleServer/mainpuzzleserver/issues/746), following [setup investigation #745](https://github.com/PuzzleServer/mainpuzzleserver/issues/745)). The recorded fix added Development-only SameSite cookie handling. | Check browser cookie warnings, HTTPS/origin, and the provider redirect URI. `Startup` still contains the Development-only `Lax` policy. Treat this as a historical fix, not a reason to relax production cookies or disable correlation validation. |

For missing client configuration or callback failures, compare
`Authentication-Microsoft-ApplicationId`, `Authentication-Microsoft-Password`,
and the provider's exact origin plus `/signin-microsoft`.
[Startup](../../ServerCore/Startup.cs) reads provider settings, while
[IdentityHostingStartup](../../ServerCore/Areas/Identity/IdentityHostingStartup.cs)
also builds configuration for account storage. Custom builders don't explicitly
add user secrets; the project's `UserSecretsId` alone doesn't establish that
every component sees them. Inspect provider presence without printing values,
check the selected database in
[DeploymentConfiguration](../../ServerCore/Areas/Deployment/DeploymentConfiguration.cs),
and restart after settings changes. Never switch production to Development to
debug or grant access.
