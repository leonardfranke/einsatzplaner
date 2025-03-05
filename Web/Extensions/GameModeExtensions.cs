using Web.Models;

namespace Web.Extensions
{
    public static class GameModeExtensions
    {
        public static string Description(this GameMode gameMode, List<Role> helperCategories)
        {
            var descriptionList = gameMode.HelperCategoryIds.Select(pair =>
            {
                var category = helperCategories.FirstOrDefault(category => category.Id == pair.Key);
                return $"{pair.Value}x {category?.Name}";
            });

            return string.Join(", ", descriptionList);
        }
    }
}
