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
            modelBuilder.Entity<Product>(product =>
            {
                product.HasMany(p => p.ProductBrands).WithOne(f => f.ProductBrands);
                product.HasMany(p => p.ProductEvents).WithOne(e => e.Product);
                product.HasMany(p => p.ProductFunctions).WithOne(f => f.Product);
                product.HasMany(p => p.ProductNameplates).WithOne(f => f.ProductNameplates);
                product.HasMany(p => p.ProductProperties).WithOne(f => f.ProductProperties);
            });
            modelBuilder.Entity<ProductEvent>()
                .HasMany(e => e.ProductEventProperties)
                .WithOne(f => f.ProductEvent);
            modelBuilder.Entity<ProductFunction>(productFunction =>
            {
                productFunction.HasMany(f => f.ProductFunctionInputs).WithOne(f => f.ProductFunctionInputs);
                productFunction.HasMany(f => f.ProductFunctionOutputs).WithOne(f => f.ProductFunctionOutputs);
            });
            modelBuilder.Entity<ProductField>()
                .HasOne(f => f.MyDataType)
                .WithOne(d => d.ProductField)
                .HasForeignKey<MyDataType>(t => t.ProductFieldId);

            modelBuilder.Entity<Device>(device =>
            {
                device.HasMany(d => d.DeviceNameplates).WithOne(f => f.DeviceNameplates);
                device.HasMany(d => d.DeviceProperties).WithOne(f => f.DeviceProperties);
            });
            /*
            modelBuilder.Entity<ProductEvent>().HasOne(e => e.Product).WithMany(p => p.ProductEvents);//.HasForeignKey(f => f.ProductId);
            modelBuilder.Entity<ProductFunction>().HasOne(f => f.Product).WithMany(p => p.ProductFunctions);//.HasForeignKey(f => f.ProductId);
            modelBuilder.Entity<ProductField>(productField =>
            {
                productField.HasOne(f => f.ProductBrands).WithMany(b => b.ProductBrands);//.HasForeignKey(f => f.ProductBrandsId);
                productField.HasOne(f => f.ProductNameplates).WithMany(n => n.ProductNameplates);//.HasForeignKey(f => f.ProductNameplatesId);
                productField.HasOne(f => f.ProductProperties).WithMany(p => p.ProductProperties);//.HasForeignKey(f => f.ProductPropertiesId);
                productField.HasOne(f => f.ProductFunctionInputs).WithMany(i => i.ProductFunctionInputs);//.HasForeignKey(f => f.ProductFunctionInputsId);
                productField.HasOne(f => f.ProductFunctionOutputs).WithMany(o => o.ProductFunctionOutputs);//.HasForeignKey(f => f.ProductFunctionOutputsId);
                productField.HasOne(f => f.ProductEvent).WithMany(e => e.ProductEventProperties);//.HasForeignKey(f => f.ProductEventId);
            });
            modelBuilder.Entity<DeviceField>(deviceField =>
            {
                deviceField.HasOne(f => f.DeviceNameplates).WithMany(n => n.DeviceNameplates);//.HasForeignKey(f => f.DeviceNameplatesId);
                deviceField.HasOne(f => f.DeviceProperties).WithMany(p => p.DeviceProperties);//.HasForeignKey(f => f.DevicePropertiesId);
                //deviceField.HasOne(f => f.ProductField).WithMany(p => p.DeviceFields);//.HasForeignKey(f => f.ProductFieldId);
            });
            */
            modelBuilder.Entity<MyDataType>()
                .HasOne(d => d.ProductField)
                .WithOne(f => f.MyDataType);//                .HasForeignKey<MyDataType>(d => d.ProductFieldId);

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
        }
    }
}
