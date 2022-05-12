﻿// <auto-generated />
using System;
using GroupManager.DataLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GroupManager.Migrations
{
    [DbContext(typeof(ManagerContext))]
    [Migration("20220505184350_updateGroupTableForCurseWords2")]
    partial class updateGroupTableForCurseWords2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.3");

            modelBuilder.Entity("GroupManager.DataLayer.Models.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("BanOnCurse")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("BanOnMaxWarn")
                        .HasColumnType("INTEGER");

                    b.Property<long>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<short>("MaxWarns")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("MuteOnCurse")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("MuteOnMaxWarn")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("MuteTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("WarnOnCurse")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("GroupManager.DataLayer.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<short>("GifLimits")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsBanned")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsBot")
                        .HasColumnType("INTEGER");

                    b.Property<short>("PhotoLimits")
                        .HasColumnType("INTEGER");

                    b.Property<short>("SentGif")
                        .HasColumnType("INTEGER");

                    b.Property<short>("SentPhotos")
                        .HasColumnType("INTEGER");

                    b.Property<short>("SentStickers")
                        .HasColumnType("INTEGER");

                    b.Property<short>("SentVideos")
                        .HasColumnType("INTEGER");

                    b.Property<short>("StickerLimits")
                        .HasColumnType("INTEGER");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<short>("VideoLimits")
                        .HasColumnType("INTEGER");

                    b.Property<short>("Warns")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
