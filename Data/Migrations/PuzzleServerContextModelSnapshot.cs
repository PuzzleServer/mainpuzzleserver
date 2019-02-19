﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ServerCore.DataModel;

namespace Data.Migrations
{
    [DbContext(typeof(PuzzleServerContext))]
    partial class PuzzleServerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.2-servicing-10034")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .HasMaxLength(128);

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("ServerCore.DataModel.Annotation", b =>
                {
                    b.Property<int>("PuzzleID");

                    b.Property<int>("TeamID");

                    b.Property<int>("Key");

                    b.Property<string>("Contents")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<DateTime>("Timestamp");

                    b.Property<int>("Version");

                    b.HasKey("PuzzleID", "TeamID", "Key");

                    b.HasIndex("TeamID");

                    b.ToTable("Annotations");
                });

            modelBuilder.Entity("ServerCore.DataModel.ContentFile", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("EventID");

                    b.Property<int>("FileType");

                    b.Property<int>("PuzzleID");

                    b.Property<string>("ShortName")
                        .IsRequired();

                    b.Property<string>("UrlString")
                        .IsRequired();

                    b.HasKey("ID");

                    b.HasIndex("PuzzleID");

                    b.HasIndex("EventID", "ShortName")
                        .IsUnique();

                    b.ToTable("ContentFiles");
                });

            modelBuilder.Entity("ServerCore.DataModel.Event", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("AllowFeedback");

                    b.Property<DateTime>("AnswerSubmissionEnd");

                    b.Property<DateTime>("AnswersAvailableBegin");

                    b.Property<string>("ContactEmail");

                    b.Property<DateTime>("EventBegin");

                    b.Property<string>("HomePartial");

                    b.Property<bool>("IsInternEvent");

                    b.Property<double>("LockoutDurationMultiplier");

                    b.Property<int>("LockoutIncorrectGuessLimit");

                    b.Property<double>("LockoutIncorrectGuessPeriod");

                    b.Property<int>("MaxExternalsPerTeam");

                    b.Property<int>("MaxNumberOfTeams");

                    b.Property<long>("MaxSubmissionCount");

                    b.Property<int>("MaxTeamSize");

                    b.Property<string>("Name")
                        .IsRequired();

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

                    b.HasIndex("UrlString")
                        .IsUnique()
                        .HasFilter("[UrlString] IS NOT NULL");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("ServerCore.DataModel.EventAdmins", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AdminID");

                    b.Property<int>("EventID");

                    b.HasKey("ID");

                    b.HasIndex("AdminID");

                    b.HasIndex("EventID");

                    b.ToTable("EventAdmins");
                });

            modelBuilder.Entity("ServerCore.DataModel.EventAuthors", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AuthorID");

                    b.Property<int>("EventID");

                    b.HasKey("ID");

                    b.HasIndex("AuthorID");

                    b.HasIndex("EventID");

                    b.ToTable("EventAuthors");
                });

            modelBuilder.Entity("ServerCore.DataModel.Feedback", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Difficulty");

                    b.Property<int>("Fun");

                    b.Property<int>("PuzzleID");

                    b.Property<DateTime>("SubmissionTime");

                    b.Property<int>("SubmitterID");

                    b.Property<string>("WrittenFeedback");

                    b.HasKey("ID");

                    b.HasIndex("PuzzleID");

                    b.HasIndex("SubmitterID");

                    b.ToTable("Feedback");
                });

            modelBuilder.Entity("ServerCore.DataModel.Hint", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Content")
                        .IsRequired();

                    b.Property<int>("Cost");

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<int>("DisplayOrder");

                    b.Property<int>("PuzzleID");

                    b.HasKey("Id");

                    b.HasIndex("PuzzleID");

                    b.ToTable("Hints");
                });

            modelBuilder.Entity("ServerCore.DataModel.HintStatePerTeam", b =>
                {
                    b.Property<int>("TeamID");

                    b.Property<int>("HintID");

                    b.Property<DateTime?>("UnlockTime");

                    b.HasKey("TeamID", "HintID");

                    b.HasIndex("HintID");

                    b.ToTable("HintStatePerTeam");
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

            modelBuilder.Entity("ServerCore.DataModel.Piece", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Contents")
                        .IsRequired()
                        .HasMaxLength(4096);

                    b.Property<int>("ProgressLevel");

                    b.Property<int>("PuzzleID");

                    b.HasKey("ID");

                    b.HasIndex("PuzzleID");

                    b.ToTable("Pieces");
                });

            modelBuilder.Entity("ServerCore.DataModel.Prerequisites", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("PrerequisiteID");

                    b.Property<int>("PuzzleID");

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

                    b.Property<int>("EventID");

                    b.Property<string>("Group");

                    b.Property<int>("HintCoinsForSolve");

                    b.Property<bool>("IsCheatCode");

                    b.Property<bool>("IsFinalPuzzle");

                    b.Property<bool>("IsGloballyVisiblePrerequisite");

                    b.Property<bool>("IsMetaPuzzle");

                    b.Property<bool>("IsPuzzle");

                    b.Property<int>("MaxAnnotationKey");

                    b.Property<int>("MinPrerequisiteCount");

                    b.Property<int>("MinutesOfEventLockout");

                    b.Property<int?>("MinutesToAutomaticallySolve");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("OrderInGroup");

                    b.Property<int>("SolveValue");

                    b.Property<string>("SupportEmailAlias");

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

                    b.Property<int>("AuthorID");

                    b.Property<int>("PuzzleID");

                    b.HasKey("ID");

                    b.HasIndex("AuthorID");

                    b.HasIndex("PuzzleID");

                    b.ToTable("PuzzleAuthors");
                });

            modelBuilder.Entity("ServerCore.DataModel.PuzzleStatePerTeam", b =>
                {
                    b.Property<int>("PuzzleID");

                    b.Property<int>("TeamID");

                    b.Property<bool>("IsEmailOnlyMode");

                    b.Property<DateTime?>("LockoutExpiryTime");

                    b.Property<string>("Notes");

                    b.Property<bool>("Printed");

                    b.Property<DateTime?>("SolvedTime");

                    b.Property<DateTime?>("UnlockedTime");

                    b.Property<long>("WrongSubmissionCountBuffer");

                    b.HasKey("PuzzleID", "TeamID");

                    b.HasIndex("TeamID");

                    b.ToTable("PuzzleStatePerTeam");
                });

            modelBuilder.Entity("ServerCore.DataModel.PuzzleUser", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Email");

                    b.Property<string>("EmployeeAlias");

                    b.Property<string>("IdentityUserId")
                        .IsRequired();

                    b.Property<bool>("IsGlobalAdmin");

                    b.Property<string>("Name");

                    b.Property<string>("PhoneNumber");

                    b.Property<string>("TShirtSize");

                    b.Property<bool>("VisibleToOthers");

                    b.HasKey("ID");

                    b.ToTable("PuzzleUsers");
                });

            modelBuilder.Entity("ServerCore.DataModel.Response", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsSolution");

                    b.Property<string>("Note");

                    b.Property<int>("PuzzleID");

                    b.Property<string>("ResponseText")
                        .IsRequired();

                    b.Property<string>("SubmittedText")
                        .IsRequired();

                    b.HasKey("ID");

                    b.HasIndex("PuzzleID");

                    b.ToTable("Responses");
                });

            modelBuilder.Entity("ServerCore.DataModel.Submission", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("PuzzleID");

                    b.Property<int?>("ResponseID");

                    b.Property<string>("SubmissionText")
                        .IsRequired();

                    b.Property<int>("SubmitterID");

                    b.Property<int>("TeamID");

                    b.Property<int?>("TeamID1");

                    b.Property<DateTime>("TimeSubmitted");

                    b.HasKey("ID");

                    b.HasIndex("PuzzleID");

                    b.HasIndex("ResponseID");

                    b.HasIndex("SubmitterID");

                    b.HasIndex("TeamID");

                    b.HasIndex("TeamID1");

                    b.ToTable("Submissions");
                });

            modelBuilder.Entity("ServerCore.DataModel.Team", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CustomRoom");

                    b.Property<int>("EventID");

                    b.Property<int>("HintCoinCount");

                    b.Property<int>("HintCoinsUsed");

                    b.Property<string>("Name");

                    b.Property<string>("PrimaryContactEmail");

                    b.Property<string>("PrimaryPhoneNumber");

                    b.Property<int?>("RoomID");

                    b.Property<string>("SecondaryPhoneNumber");

                    b.HasKey("ID");

                    b.HasIndex("EventID");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("ServerCore.DataModel.TeamApplication", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("PlayerID");

                    b.Property<int>("TeamID");

                    b.HasKey("ID");

                    b.HasIndex("PlayerID");

                    b.HasIndex("TeamID");

                    b.ToTable("TeamApplications");
                });

            modelBuilder.Entity("ServerCore.DataModel.TeamMembers", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("Team.ID")
                        .IsRequired();

                    b.Property<int?>("User.ID")
                        .IsRequired();

                    b.HasKey("ID");

                    b.HasIndex("Team.ID");

                    b.HasIndex("User.ID");

                    b.ToTable("TeamMembers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.Annotation", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.ContentFile", b =>
                {
                    b.HasOne("ServerCore.DataModel.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventID")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany("Contents")
                        .HasForeignKey("PuzzleID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.EventAdmins", b =>
                {
                    b.HasOne("ServerCore.DataModel.PuzzleUser", "Admin")
                        .WithMany()
                        .HasForeignKey("AdminID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.EventAuthors", b =>
                {
                    b.HasOne("ServerCore.DataModel.PuzzleUser", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.Feedback", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.PuzzleUser", "Submitter")
                        .WithMany()
                        .HasForeignKey("SubmitterID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.Hint", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.HintStatePerTeam", b =>
                {
                    b.HasOne("ServerCore.DataModel.Hint", "Hint")
                        .WithMany()
                        .HasForeignKey("HintID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamID")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("ServerCore.DataModel.Invitation", b =>
                {
                    b.HasOne("ServerCore.DataModel.Team")
                        .WithMany("Invitations")
                        .HasForeignKey("TeamID");
                });

            modelBuilder.Entity("ServerCore.DataModel.Piece", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.Prerequisites", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Prerequisite")
                        .WithMany()
                        .HasForeignKey("PrerequisiteID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.Puzzle", b =>
                {
                    b.HasOne("ServerCore.DataModel.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.PuzzleAuthors", b =>
                {
                    b.HasOne("ServerCore.DataModel.PuzzleUser", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.PuzzleStatePerTeam", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamID")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("ServerCore.DataModel.Response", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany()
                        .HasForeignKey("PuzzleID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.Submission", b =>
                {
                    b.HasOne("ServerCore.DataModel.Puzzle", "Puzzle")
                        .WithMany("Submissions")
                        .HasForeignKey("PuzzleID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.Response", "Response")
                        .WithMany()
                        .HasForeignKey("ResponseID");

                    b.HasOne("ServerCore.DataModel.PuzzleUser", "Submitter")
                        .WithMany()
                        .HasForeignKey("SubmitterID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamID")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("ServerCore.DataModel.Team")
                        .WithMany("Submissions")
                        .HasForeignKey("TeamID1");
                });

            modelBuilder.Entity("ServerCore.DataModel.Team", b =>
                {
                    b.HasOne("ServerCore.DataModel.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.TeamApplication", b =>
                {
                    b.HasOne("ServerCore.DataModel.PuzzleUser", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.Team", "Team")
                        .WithMany()
                        .HasForeignKey("TeamID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerCore.DataModel.TeamMembers", b =>
                {
                    b.HasOne("ServerCore.DataModel.Team", "Team")
                        .WithMany()
                        .HasForeignKey("Team.ID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ServerCore.DataModel.PuzzleUser", "Member")
                        .WithMany()
                        .HasForeignKey("User.ID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
