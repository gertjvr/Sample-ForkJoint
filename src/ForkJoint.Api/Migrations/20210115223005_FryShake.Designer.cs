﻿// <auto-generated />
using System;
using ForkJoint.Api.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ForkJoint.Api.Migrations
{
    [DbContext(typeof(ForkJointSagaDbContext))]
    [Migration("20210115223005_FryShake")]
    partial class FryShake
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Automatonymous.Requests.RequestState", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("ConversationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CurrentState")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ExpirationTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("FaultAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResponseAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SagaAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("SagaCorrelationId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("CorrelationId");

                    b.HasIndex("SagaCorrelationId")
                        .IsUnique();

                    b.ToTable("RequestState");
                });

            modelBuilder.Entity("ForkJoint.Api.Components.StateMachines.BurgerState", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Burger")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Completed")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<int>("CurrentState")
                        .HasColumnType("int");

                    b.Property<string>("ExceptionInfo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Faulted")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("RequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ResponseAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("TrackingNumber")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("CorrelationId");

                    b.ToTable("BurgerState");
                });

            modelBuilder.Entity("ForkJoint.Api.Components.StateMachines.FryState", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("Completed")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<int>("CurrentState")
                        .HasColumnType("int");

                    b.Property<string>("ExceptionInfo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Faulted")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("RequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ResponseAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Size")
                        .HasColumnType("int");

                    b.HasKey("CorrelationId");

                    b.ToTable("FryState");
                });

            modelBuilder.Entity("ForkJoint.Api.Components.StateMachines.OnionRingsState", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("Completed")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<int>("CurrentState")
                        .HasColumnType("int");

                    b.Property<string>("ExceptionInfo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Faulted")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<Guid?>("RequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ResponseAddress")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CorrelationId");

                    b.ToTable("OnionRingsState");
                });

            modelBuilder.Entity("ForkJoint.Api.Components.StateMachines.OrderState", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("Completed")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<int>("CurrentState")
                        .HasColumnType("int");

                    b.Property<DateTime?>("Faulted")
                        .HasColumnType("datetime2");

                    b.Property<int>("LineCount")
                        .HasColumnType("int");

                    b.Property<string>("LinesCompleted")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LinesFaulted")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LinesPending")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("RequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ResponseAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Updated")
                        .HasColumnType("datetime2");

                    b.HasKey("CorrelationId");

                    b.ToTable("OrderState");
                });

            modelBuilder.Entity("ForkJoint.Api.Components.StateMachines.ShakeState", b =>
                {
                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("Completed")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<int>("CurrentState")
                        .HasColumnType("int");

                    b.Property<string>("ExceptionInfo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Faulted")
                        .HasColumnType("datetime2");

                    b.Property<string>("Flavor")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("RequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ResponseAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Size")
                        .HasColumnType("int");

                    b.HasKey("CorrelationId");

                    b.ToTable("ShakeState");
                });
#pragma warning restore 612, 618
        }
    }
}
