namespace ReadingListPlus.Common
{
    public static class Extension
    {
        public static string Truncate(this string value, int length) =>
            value.Length > length ? value.Substring(0, length) + Constants.Ellipsis : value;
    }
}
