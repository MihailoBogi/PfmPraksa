﻿namespace PersonalFinance.Application.Common.Pagination
{
    public class PagedResult<T>
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    }
}
