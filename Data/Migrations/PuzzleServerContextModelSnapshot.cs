﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ServerCore.Models;

namespace Data.Migrations
{
    [DbContext(typeof(PuzzleServerContext))]
    partial class PuzzleServerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ServerCore.DataModel.Event", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("AdminsID");

                    b.Property<bool>("AllowFeedback");

                    b.Property<DateTime>("AnswerSubmissionEnd");

                    b.Property<DateTime>("AnswersAvailableBegin");

                    b.Property<DateTime>("EventBegin");

                    b.Property<bool>("IsInternEvent");

                    b.Property<int>("MaxExternalsPerTeam");

                    b.Property<int>("MaxNumberOfTeams");

                    b.Property<int>("MaxTeamSize");

                    b.Property<string>("Name");

                    b.Property<bool>("ShowFastestSolves");

                    b.Property<DateTime>("StandingsAvailableBegin");

                    b.Property<bool>("StandingsOverride");

                    b.Property<DateTime>("TeamDeleteEnd");

                    b.Property<DateTime>("TeamMembershipChangeEnd");

                    b.Property<DateTime>("TeamMiscDataChangeEnd");

                    b.Property<DateTime>("TeamNameChangeEnd");

                    b.Property<DateTime>("TeamRegistrationBegin");

                    b.Property<DateTime>("TeamRegistrationEnd");

                    b.Property<string>("UrlString");

                    b.HasKey("ID");

                    b.HasIndex("AdminsID");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("ServerCore.DataModel.EventAdmins", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("Event.ID");

                    b.Property<int?>("User.ID");

                    b.HasKey("ID");

                    b.HasIndex("Event.ID");

                    b.HasIndex("User.ID");

                    b.ToTable("EventAdmins");
                });

            modelBuilder.Entity("ServerCore.DataModel.EventAuthors", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("Event.ID");

                    b.Property<int?>("User.ID");

                    b.HasKey("ID");

                    b.HasIndex("Event.ID")
                        .IsUnique()
                        .HasFilter("[Event.ID] IS NOT NULL");

                    b.HasIndex("User.ID");

                    b.ToTable("EventAuthors");
                });

            modelBuilder.Entity("ServerCore.DataModel.EventOwners", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("Event.ID");

                    b.Property<int?>("User.ID");

                    b.HasKey("ID");

                    b.HasIndex("Event.ID");

                    b.HasIndex("User.ID");

                    b.ToTable("EventOwners");
                });

            modelBuilder.Entity("ServerCore.DataModel.EventTeams", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("Event.ID");

                    b.Property<int?>("Teams.ID");

                    b.HasKey("ID");

                    b.HasIndex("Event.ID")
                        .IsUnique()
                        .HasFilter("[Event.ID] IS NOT NULL");

                    b.HasIndex("Teams.ID");

                    b.ToTable("EventTeams");
                });

            modelBuilder.Entity("ServerCore.DataModel.Feedback", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Difficulty");

                    b.Property<int>("Fun");

                    b.Property<int?>("PuzzleID");

                    b.Property<DateTime>("SubmissionTime");

                    b.Property<int?>("SubmitterID");

                    b.Property<string>("WrittenFeedback");

                    b.HasKey("ID");

                    b.HasIndex("PuzzleID");

                    b.HasIndex("SubmitterID");

                    b.ToTable("Feedback");
                });

            modelBuilder.Entity("ServerCore.DataModel.Invitation", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EmailAddress");

                    b.Property<DateTime>("Expiration");

                    b.Property<Guid>("InvitationCode");

                    b.Property<string>("InvitationType");

                    b.Property<int?>("TeamID");

                    b.HasKey("ID");

                    b.HasIndex("TeamID");

                    b.ToTable("Invitations");
                });

            modelBuilder.Entity("ServerCore.DataModel.Prerequisites", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("PrerequisiteID");

                    b.Property<int?>("PuzzleID");

                    b.HasKey("ID");

                    b.HasIndex("PrerequisiteID");

                    b.HasIndex("PuzzleID");

                    b.ToTable("Prerequisites");
                });

            modelBuilder.Entity("ServerCore.DataModel.Puzzle", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AnswerUrlString");

                    b.Property<int?>("EventID");

                    b.Property<string>("Group");

                    b.Property<bool>("IsFinalPuzzle");

                    b.Property<bool>("IsGloballyVisiblePrerequisite");

                    b.Property<bool>("IsMetaPuzzle");

                    b.Property<bool>("IsPuzzle");

                    b.Property<string>("MaterialsUrlString");

                    b.Property<int>("MinPrerequisiteCount");

                    b.Property<string>("Name");

                    b.Property<int>("OrderInGroup");

                    b.Property<string>("PuzzleUrlString");

                    b.Property<int>("SolveValue");

                    b.Property<string>("Token");

                    b.HasKey("ID");

                    b.HasIndex("EventID");

                    b.ToTable("Puzzles");
                });

            modelBuilder.Entity("ServerCore.DataModel.PuzzleAuthors", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("Puzzle.ID");

                    b.Property<int?>("User.ID");

                    b.HasKey("ID");

                    b.HasIndex("Puzzle.ID");

                    b.HasIndex("User.ID");

                    b.ToTable("PuzzleAuthors");
                });

            modelBuilder.Entity("ServerCore.DataModel.PuzzleStatePerTeam", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Notes");

                    b.Property<bool>("Printed");

                    b.Property<int?>("PuzzleID");

                    b.Property<DateTime?>("SolvedTime");

                    b.Property<int?>("TeamID");

                    b.Property<DateTime?>("UnlockedTime");

                    b.HasKey("ID");

                    b.HasIndex("PuzzleID");

                    b.HasIndex("TeamID");

                    b.ToTable("PuzzleStatePerTeam");
                });

            modelBuilder.Entity("ServerCore.DataModel.Response", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsSolution");

                    b.Property<string>("Note");

                    b.Property<int?>("PuzzleID");

                    b.Property<string>("ResponseText");

                    b.Property<string>("SubmittedText");

                    b.HasKey("ID");

                    b.HasIndex("PuzzleID");

                    b.ToTable("Responses");
                });

            modelBuilder.Entity("ServerCore.DataModel.Submission", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("PuzzleID");

                    b.Property<int?>("ResponseID");

                    b.Property<string>("SubmissionText");

                    b.Property<int?>("SubmitterID");

                    b.Property<int?>("TeamID");

                    b.Property<DateTime>("TimeSubmitted");

                    b.HasKey("ID");

                    b.HasIndex("PuzzleID");

                    b.HasIndex("ResponseID");

                    b.HasIndex("SubmitterID");

                    b.HasIndex("TeamID");

                    b.ToTable("Submissions");
                });

            modelBuilder.Entity("ServerCore.DataModel.Team", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CustomRoom");

                    b.Property<int?>("EventID");

                    b.Property<string>("Name");

                    b.Property<string>("PrimaryContactEmail");

                    b.Property<string>("PrimaryPhoneNumber");

                    b.Property<int?>("RoomID");

                    b.Property<string>("SecondaryPhoneNumber");

                    b.HasKey("ID");

                    b.HasIndex("EventID");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("ServerCore.DataModel.TeamMembers", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("Team.ID");

                    b.Property<int?>("User.ID");

                    b.HasKey("ID");

                    b.HasIndex("Team.ID")
                        .IsUnique()
                        .HasFilter("[Team.ID] IS NOT NULL");

                    b.HasIndex("User.ID");

                    b.ToTable("TeamMembers");
                });

            modelBuilder.Entity("ServerCore.DataModel.User", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EmailAddress");

                    b.Property<string>("EmployeeAlias");

                    b.Property<string>("Name");

                    b.Property<string>("PhoneNumber");

                    b.Property<string>("TShirtSize");

                    b.Property<bool>("VisibleToOthers");

                    b.HasKey("ID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ServerCore.DataModel.Event", b =>
                {
                    b.HasOne("ServerCore.DataModel.EventAdmins", "Admins")
                        .WithMany()
                        .HasForeignKey("AdminsID");
                });

            modelBuilder.Entity("ServerCore.DataModel.EventAdmins", b =>
                {
                    b.HasOne("ServerCore.DataModel.EventOwners", "Event")
                        .WithMany()
                        .HasForeignKey("Event.ID");

                    b.HasOne("ServerCore.DataModel.User", "Admin")
                        .WithMany()
                        .HasForeignKey("User.ID");
                });

            modelBuilder.Entity("ServerCore.DataModel.EventAuthors", b =>
                {
                    b.HasOne("ServerCore.DataModel.Event", "Event")
                        .WithOne("Authors")
                        .HasForeignKey("ServerCore.DataModel.EventAuthors", "Event.ID");

                    b.HasOne("ServerCore.DataModel.User", "Author")
                        .WithMany()
                        .HasForeignKey("User.ID");
                });

            modelBuilder.Entity("ServerCore.DataModel.EventOwners", b =>
                {
                    b.HasOne("ServerCore.DataModel.Event", "Event")
                        .WithMany()
                        .HasForeignKey("Event.ID");

                    b.HasOne("ServerCore.DataModel.User", "Owner")
                        .WithMany()
                        .HasForeignKey("User.ID");
                });

            modelBuilder.Entity("ServerCore.DataModel.EventTeams", b =>
                {
                    b.HasOne("ServerCore.DataModel.Event", "Event")
                        .WithOne("Teams")
                        .HasForeignKey("ServerCore.DataModel.EventTeams", "Event.ID");

                    b.HasOne("ServerCore.DataModel.Team", "Team")
                        .WithMany()
                        .HasForeignKey("Teams.ID");
                });

            modelBuilder.Entity("ServerCore.DataModel.Feedback", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID");

                    b.HasOne("ServerCore.DataModel.User", "Submitter")
                        .WithMany()
                        .HasForeignKey("SubmitterID");
                });

            modelBuilder.Entity("ServerCore.DataModel.Invitation", b =>
                {
                    b.HasOne("ServerCore.DataModel.Team")
                        .WithMany("Invitations")
                        .HasForeignKey("TeamID");
                });

            modelBuilder.Entity("ServerCore.DataModel.Prerequisites", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Prerequisite")
                        .WithMany()
                        .HasForeignKey("PrerequisiteID");

                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID");
                });

            modelBuilder.Entity("ServerCore.DataModel.Puzzle", b =>
                {
                    b.HasOne("ServerCore.DataModel.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventID");
                });

            modelBuilder.Entity("ServerCore.DataModel.PuzzleAuthors", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("Puzzle.ID");

                    b.HasOne("ServerCore.DataModel.User", "Author")
                        .WithMany()
                        .HasForeignKey("User.ID");
                });

            modelBuilder.Entity("ServerCore.DataModel.PuzzleStatePerTeam", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID");

                    b.HasOne("ServerCore.DataModel.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamID");
                });

            modelBuilder.Entity("ServerCore.DataModel.Response", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID");
                });

            modelBuilder.Entity("ServerCore.DataModel.Submission", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID");

                    b.HasOne("ServerCore.DataModel.Response", "Response")
                        .WithMany()
                        .HasForeignKey("ResponseID");

                    b.HasOne("ServerCore.DataModel.User", "Submitter")
                        .WithMany()
                        .HasForeignKey("SubmitterID");

                    b.HasOne("ServerCore.DataModel.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamID");
                });

            modelBuilder.Entity("ServerCore.DataModel.Team", b =>
                {
                    b.HasOne("ServerCore.DataModel.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventID");
                });

            modelBuilder.Entity("ServerCore.DataModel.TeamMembers", b =>
                {
                    b.HasOne("ServerCore.DataModel.Team", "Team")
                        .WithOne("Members")
                        .HasForeignKey("ServerCore.DataModel.TeamMembers", "Team.ID");

                    b.HasOne("ServerCore.DataModel.User", "Member")
                        .WithMany()
                        .HasForeignKey("User.ID");
                });
#pragma warning restore 612, 618
        }
    }
}
