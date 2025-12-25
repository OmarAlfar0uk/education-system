using System.ComponentModel.DataAnnotations;

namespace EduocationSystem.Shared.Responses
{
    /// <summary>
    /// Sorting direction enumeration
    /// </summary>
    public enum SortDirection
    {
        Asc = 0,
        Desc = 1
    }

    /// <summary>
    /// Base pagination parameters DTO with strongly-typed sorting
    /// </summary>
    /// <typeparam name="TSortBy">Enum type for sort fields</typeparam>
    public class PaginationParamsDto<TSortBy>
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, MaxPageSize, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
        }

        [MaxLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? Search { get; set; }

        /// <summary>
        /// Field to sort by (enum)
        /// </summary>
        public TSortBy? SortBy { get; set; }

        /// <summary>
        /// Sort direction
        /// </summary>
        public SortDirection SortDirection { get; set; } = SortDirection.Asc;
    }

    /// <summary>
    /// Generic paged result wrapper for API responses
    /// </summary>
    /// <typeparam name="T">Type of items in the collection</typeparam>
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
        public int StartIndex => (Page - 1) * PageSize + 1;
        public int EndIndex => Math.Min(Page * PageSize, TotalCount);

        public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            if (page < 1) throw new ArgumentException("Page must be greater than 0", nameof(page));
            if (pageSize < 1) throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            var list = items.ToList();
            Items = list.AsReadOnly();
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
            TotalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        }

        public static PagedResult<T> Create(IEnumerable<T> source, int page, int pageSize)
        {
            var totalCount = source.Count();
            var items = source.Skip((page - 1) * pageSize).Take(pageSize);
            return new PagedResult<T>(items, totalCount, page, pageSize);
        }
    }

    /// <summary>
    /// Paged result with additional response metadata
    /// </summary>
    /// <typeparam name="T">Type of items in the collection</typeparam>
    public class PagedResultWithMetadata<T> : PagedResult<T>
    {
        /// <summary>Arbitrary metadata about the result</summary>
        public Dictionary<string, object> Metadata { get; }

        /// <summary>Filters applied to produce this page</summary>
        public Dictionary<string, string> Filters { get; }

        /// <summary>Sorting information included in response</summary>
        public SortInfo? SortInfo { get; }

        public PagedResultWithMetadata(
            IEnumerable<T> items,
            int totalCount,
            int page,
            int pageSize,
            Dictionary<string, object>? metadata = null,
            Dictionary<string, string>? filters = null,
            SortInfo? sortInfo = null)
            : base(items, totalCount, page, pageSize)
        {
            Metadata = metadata ?? new Dictionary<string, object>();
            Filters = filters ?? new Dictionary<string, string>();
            SortInfo = sortInfo;
        }
    }

    /// <summary>
    /// Information about sort field and direction for response metadata
    /// </summary>
    public class SortInfo
    {
        /// <summary>Name of the sorted field</summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>Direction of the sort</summary>
        public SortDirection Direction { get; set; }

        /// <summary>Text representation ("asc"/"desc")</summary>
        public string DirectionText => Direction.ToString().ToLowerInvariant();
    }


}
