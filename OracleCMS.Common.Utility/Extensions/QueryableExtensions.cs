using OracleCMS.Common.Utility.Models;
using System.Linq.Dynamic.Core;
using X.PagedList.Extensions;

namespace OracleCMS.Common.Utility.Extensions;

/// <summary>
/// Extension methods for <see cref="IQueryable{T}"/>.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Converts <see cref="IQueryable{T}"/> to <see cref="PagedListResponse{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="searchColumns">Columns to search</param>
    /// <param name="searchValue">Global search value</param>
    /// <param name="sortColumn">Column where sorting will be applied</param>
    /// <param name="sortOrder">Asc or Desc</param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static PagedListResponse<T> ToPagedResponse<T>(this IQueryable<T> query,
                                                                      string[]? searchColumns, string? searchValue,
                                                                      string? sortColumn, string? sortOrder,
                                                                      int pageNumber, int pageSize)
    {        
	    if (!string.IsNullOrWhiteSpace(searchValue) && searchColumns != null)
        {
			searchValue = searchValue.Replace("\\", "");
            var filter = string.Join(" OR ", searchColumns.Select(x => $"{x}.Contains(\"{searchValue}\")"));
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(filter);
            }
        }
        if (!string.IsNullOrWhiteSpace(sortColumn) && !string.IsNullOrWhiteSpace(sortOrder))
        {
            query = query.OrderBy($"{sortColumn} {sortOrder}");
        }
		var totalCount = query.Count();
		if (pageSize == -1)
		{
			pageSize = totalCount;
			if (pageSize == 0)
			{
				pageSize = 1;
			}        
		}
		var data = query.ToPagedList(pageNumber, pageSize);
		return new PagedListResponse<T>(data, totalCount);
    }
}