using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Apps
{
    [Export]
    public sealed class AppCategoriesCoordinator
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public AppCategoriesCoordinator(IRepository repository, ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<AppModel> GetApps()
        {
            return repository.GetFiltered<Aplication>(a => a.UserID == trackingService.SelectedUserID,
                                                             a => a.Categories)
                                                            .Select(a => new AppModel(a))
                                                            .ToList();
        }

        public IEnumerable<AppCategoryModel> GetCategories()
        {
            var appCategories = repository.GetFiltered<AppCategory>(c => c.Applications.Where(a => a.UserID == trackingService.SelectedUserID)
                                                                            .Any(),
                                                       c => c.Applications);
            var models = appCategories.Select(c =>
            {
                var model = new AppCategoryModel(c);
                model.SetApps(c.Applications);
                return model;
            });

            return models;
        }

        public async Task SaveChangesAsync(IEnumerable<AppCategoryModel> deletedCategories,
                                           IEnumerable<AppCategoryModel> newCategories,
                                           IEnumerable<AppCategoryModel> modifiedCategories)
        {
            await repository.DeleteByIdsAsync<AppCategory>(deletedCategories.Select(c => c.ID));

            await SaveNewCategories(newCategories);

            await SaveModifiedCategories(modifiedCategories);
        }

        private async Task SaveNewCategories(IEnumerable<AppCategoryModel> newCategories)
        {
            //ef can manage many to many relationships only when changes are tracked, we don't track changes, so drop to raw sql
            var insertCategoriesSQL = @"INSERT INTO [AppCategories] (Name) VALUES({0})";
            var insertAppCategoriesSQL = @"INSERT INTO [ApplicationCategories] VALUES({0},{1})";
            foreach (var cat in newCategories)
            {
                await repository.ExecuteSql(insertCategoriesSQL, cat.Name);
                var newCat = await repository.GetSingleAsync<AppCategory>(c => c.Name == cat.Name);
                foreach (var app in cat.ObservableApplications)
                {
                    await repository.ExecuteSql(insertAppCategoriesSQL, app.ApplicationID, newCat.ID);
                }
            }
        }

        private async Task SaveModifiedCategories(IEnumerable<AppCategoryModel> modifiedCategories)
        {
            var deleteSQL = @"DELETE FROM [ApplicationCategories] WHERE AppCategoryID = {0}";
            var insertAppCategoriesSQL = @"INSERT INTO [ApplicationCategories] VALUES({0},{1})";
            foreach (var cat in modifiedCategories)
            {
                var dbCat = await repository.GetSingleAsync<AppCategory>(cat.ID);
                await repository.ExecuteSql(deleteSQL, dbCat.ID);
                foreach (var app in cat.ObservableApplications)
                {
                    await repository.ExecuteSql(insertAppCategoriesSQL, app.ApplicationID, dbCat.ID);
                }
            }
        }

    }
}
