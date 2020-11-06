using FrameworkCore.Metadata.DataTypes;
using FrameworkCore.Metadata.DeviceDefine;
using FrameworkCore.Metadata.ProductDefine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace FrameworkCore.Database
{
    public class ModelDbContext : DbContext
    {
        public DbSet<IntType> IntTypes { get; set; }
        public DbSet<StringType> StringTypes { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<Device> Devices { get; set; }

        public ModelDbContext(DbContextOptions<ModelDbContext> dbContextOptions) : base(dbContextOptions) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IntType>(intType =>
            {
                intType.Property(e => e.Max).HasColumnName("Max");
                intType.Property(e => e.Min).HasColumnName("Min");
            });

            modelBuilder.Entity<StringType>(stringType =>
            {
                stringType.Property(f => f.MaxLength).HasColumnName("Max");
                stringType.Property(f => f.MinLength).HasColumnName("Min");
            });

            modelBuilder.Entity<ProductField>(productField =>
            {
                productField.HasOne(f => f.ProductBrands).WithMany(b => b.ProductBrands);
                productField.HasOne(f => f.ProductNameplates).WithMany(n => n.ProductNameplates);
                productField.HasOne(f => f.ProductProperties).WithMany(p => p.ProductProperties);
                productField.HasOne(f => f.ProductEvent).WithMany(e => e.ProductEventProperties);
                productField.HasOne(f => f.ProductFunctionInputs).WithMany(i => i.ProductFunctionInputs);
                productField.HasOne(f => f.ProductFunctionOutputs).WithMany(o => o.ProductFunctionOutputs);
            });

            modelBuilder.Entity<ProductFunction>().HasOne(f => f.Product).WithMany(p => p.ProductFunctions);

            modelBuilder.Entity<ProductEvent>().HasOne(e => e.Product).WithMany(p => p.ProductEvents);

            modelBuilder.Entity<Device>().HasOne(d => d.Product).WithMany(p => p.Devices).HasForeignKey(d => d.ProductId);

            modelBuilder.Entity<DeviceField>(deviceField =>
            {
                deviceField.HasOne(f => f.DeviceNameplates).WithMany(n => n.DeviceNameplates).HasForeignKey(f => f.DeviceNameplatesId);
                deviceField.HasOne(f => f.DeviceProperties).WithMany(p => p.DeviceProperties).HasForeignKey(f => f.DevicePropertiesId);
                deviceField.HasOne(f => f.ProductField).WithMany(f => f.DeviceFields).HasForeignKey(f => f.ProductFieldId);
            });
        }
    }
}
