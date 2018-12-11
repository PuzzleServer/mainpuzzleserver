using Microsoft.AspNetCore.Authorization;
using PoliciesAuthApp1.Services.Requirements;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ServerCore.Areas.Identity
{
  public class IsAuthorInEventRequirement : IAuthorizationRequirement
  {
      public Event ThisEvent { get; private set; }

      public IsAuthorInEventRequirement(Event thisEvent)
      {
          ThisEvent = thisEvent;
      }
  }

  public class IsAuthorInEventHandler : AuthorizationHandler<IsAuthorInEventRequirement>
  {
      private readonly IPuzzleServerContext puzzleContext;
      private readonly UserManager<IdentityUser> userManager;

      public IsAuthorInEventHandler(IPuzzleServerContext pContext, UserManager<IdentityUser> manager)
      {
        puzzleContext = pContext;
        userManager = manager;
      }

      protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                     IsAuthorInEventRequirement requirement)
      {
          PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleContext, context.User, userManager);

          if (puzzleUser.IsAuthor(requirement.ThisEvent))
          {
              context.Succeed(requirement);
          }

          return Task.CompletedTask;
      }
   }
   
     public class IsAdminInEventRequirement : IAuthorizationRequirement
  {
      public Event ThisEvent { get; private set; }

      public IsAdminInEventRequirement(Event thisEvent)
      {
          ThisEvent = thisEvent;
      }
  }

  public class IsAdminInEventHandler : AuthorizationHandler<IsAuthorInEventRequirement>
  {
      private readonly IPuzzleServerContext puzzleContext;
      private readonly UserManager<IdentityUser> userManager;

      public IsAdminInEventHandler(IPuzzleServerContext pContext, UserManager<IdentityUser> manager)
      {
        puzzleContext = pContext;
        userManager = manager;
      }

      protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                     IsAdminInEventRequirement requirement)
      {
          PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleContext, context.User, userManager);

          if (puzzleUser.IsAdmin(requirement.ThisEvent))
          {
              context.Succeed(requirement);
          }

          return Task.CompletedTask;
      }
   }
 }
