using System.Web;

namespace Web.Helper
{
    public static class QueryBuilder
    {
        public static string Build(params (string param, object? value)[] valuePairs)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var pair in valuePairs)
            {
                if (string.IsNullOrEmpty(pair.param))
                    throw new NullReferenceException("Query builder: param was null");

                if (pair.value is string str)
                {
                    query.Add(pair.param, str);
                }
                else if(pair.value is List<string> strList)
                {
                    foreach(var strElement in strList)
                    {
                        if(!string.IsNullOrEmpty(strElement))
                            query.Add(pair.param, strElement);
                    }
                }
                else if(pair.value != null)
                {
                    throw new ArgumentException("Can only add string or string list params");
                }                
            }            

            var queryString = query.ToString();
            if(string.IsNullOrEmpty(queryString))
                return string.Empty;
            else
                return $"?{queryString}";
        }
    }
}
