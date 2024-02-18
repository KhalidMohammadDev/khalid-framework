using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Khalid.Core.Framework
{

    public class BaseDbContext : DbContext
    {

        private readonly IServiceProvider _serviceProvider;

        public BaseDbContext(DbContextOptions<BaseDbContext> options, IServiceProvider serviceProvider) : base(options)
        {
            _serviceProvider = serviceProvider;
        }

        private void OnBeforeSaveChanges()
        {
            var userIdString = _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.Items["AuditTrailUserId"]?.ToString();
            int? userId = null;
            if (userIdString != null && int.TryParse(userIdString, out int userIdInt)) userId = userIdInt;

            ChangeTracker.DetectChanges();

            //get all changes to add to audit table 
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
                {
                    if (entry.Entity is IModificationEntity oEntity)
                    {

                        switch (entry.State)
                        {
                            case EntityState.Added:
                                {
                                    oEntity.CreateDate = DateTime.Now;
                                    oEntity.LastUpdateDate = DateTime.Now;
                                    oEntity.CreateByUserId = userId;
                                    oEntity.LastUpdateByUserId = userId;
                                    break;
                                }
                            case EntityState.Modified:
                                {
                                    Entry(oEntity).Property(x => x.CreateDate).IsModified = false;
                                    Entry(oEntity).Property(x => x.CreateByUserId).IsModified = false;
                                    oEntity.LastUpdateDate = DateTime.Now;
                                    oEntity.LastUpdateByUserId = userId;
                                    break;
                                }
                        }
                    }


                }
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaveChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChanges();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }





}
