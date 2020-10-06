namespace AMStock.Web.Models
{
    public static class HelperUtility
    {
        public static int GetPages(int totalCount, int pageSize)
        {
            var pages = (int)totalCount / pageSize;
            if (totalCount % pageSize > 0)
                pages++;
            return pages;
        }
    }
}