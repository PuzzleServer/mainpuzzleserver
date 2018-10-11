//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using ServerCore.DataModel;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ServerCore.Test.Mocks
//{
//    [TestClass]
//    class PuzzleServerContextMock : IPuzzleServerContext
//    {
//        public DbSet<Event> Event { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<EventAdmins> EventAdmins { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<EventAuthors> EventAuthors { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<EventOwners> EventOwners { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<EventTeams> EventTeams { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<Feedback> Feedback { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<Invitation> Invitations { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<Puzzle> Puzzle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<PuzzleAuthors> PuzzleAuthors { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<PuzzleStatePerTeam> PuzzleStatePerTeam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<Response> Responses { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<State> States { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<Submission> Submissions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<TeamMembers> TeamMembers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<Team> Teams { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public DbSet<User> Users { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//        //public static DbContextOptions<PuzzleServerContext> TestDbContextOptions()
//        //{
//        //    //var serviceProvider = new ServiceCollection()
//        //    //    .AddEntityFrameworkInMemoryDatabase()
//        //    //    .BuildServiceProvider();

//        //    //var builder = new DbContextOptionsBuilder<PuzzleServerContext>()
//        //    //    .UseInMemoryDatabase("ServerCoreUnitTests")
//        //    //    .UserInternalServiceProvider(serviceProvider);

//        //    //return builder.Options;
//        //}

//        //Mock<PuzzleServerContext> mockContext = new Mock<PuzzleServerContext>(TestDbContextOptions());

//    }
//}
