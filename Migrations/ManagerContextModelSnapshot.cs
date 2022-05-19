﻿// <auto-generated />
using System;
using GroupManager.DataLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GroupManager.Migrations
{
    [DbContext(typeof(ManagerContext))]
    partial class ManagerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.5");

            modelBuilder.Entity("GroupManager.DataLayer.Models.FloodSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("BanOnDetect")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<long>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Interval")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("MessageCountPerInterval")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("MuteOnDetect")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("RestrictTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GroupId")
                        .IsUnique();

                    b.ToTable("FloodSettings");
                });

            modelBuilder.Entity("GroupManager.DataLayer.Models.ForceJoinChannel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("GroupId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.ToTable("ForceJoinChannels");
                });

            modelBuilder.Entity("GroupManager.DataLayer.Models.Group", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AntiBot")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AntiJoin")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("BanOnCurse")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("BanOnMaxWarn")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableMessageLimitPerUser")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ForceJoin")
                        .HasColumnType("INTEGER");

                    b.Property<long>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("MaxMessagePerUser")
                        .HasColumnType("INTEGER");

                    b.Property<short>("MaxWarns")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValueSql("3");

                    b.Property<bool>("MuteOnCurse")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("MuteOnMaxWarn")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("MuteTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("'03:00:00'");

                    b.Property<bool>("SayWelcome")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("TimeBasedMute")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TimeBasedMuteFromTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("TimeBasedMuteFuncHashId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TimeBasedMuteUntilTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("TimeBasedUnmuteFuncHashId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("WarnOnCurse")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WelcomeMessage")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("'Welcome To Group!'");

                    b.HasKey("Id");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("GroupManager.DataLayer.Models.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<uint>("GifLimits")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsBanned")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsBot")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("MessageCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("PhotoLimits")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("SentGif")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("SentPhotos")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("SentStickers")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("SentVideos")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("StickerLimits")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("VideoLimits")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Warns")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("GroupManager.DataLayer.Models.FloodSettings", b =>
                {
                    b.HasOne("GroupManager.DataLayer.Models.Group", "Group")
                        .WithOne("FloodSetting")
                        .HasForeignKey("GroupManager.DataLayer.Models.FloodSettings", "GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("GroupManager.DataLayer.Models.ForceJoinChannel", b =>
                {
                    b.HasOne("GroupManager.DataLayer.Models.Group", "Group")
                        .WithMany("ForceJoinChannel")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("GroupManager.DataLayer.Models.Group", b =>
                {
                    b.Navigation("FloodSetting")
                        .IsRequired();

                    b.Navigation("ForceJoinChannel");
                });
#pragma warning restore 612, 618
        }
    }
}
