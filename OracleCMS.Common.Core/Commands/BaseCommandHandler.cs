using AutoMapper;
using OracleCMS.Common.Core.Base.Models;
using OracleCMS.Common.Data;
using OracleCMS.Common.Utility.Validators;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using static LanguageExt.Prelude;

namespace OracleCMS.Common.Core.Commands;

/// <summary>
/// A base class for CRUD command handlers.
/// </summary>
/// <typeparam name="TContext">The type of the context that will be used by the command handler. Must derive from <see cref="AuditableDbContext<TContext>"/></typeparam>
/// <typeparam name="TEntity">The type of the entity that will be processed by this handler. Must derive from <see cref="BaseEntity"/></typeparam>
/// <typeparam name="TCommand">The type of the command extending this base class. Must derive from <see cref="IEntity"/></typeparam>
public abstract class BaseCommandHandler<TContext, TEntity, TCommand>
    where TContext : AuditableDbContext<TContext>
    where TEntity : BaseEntity
    where TCommand : IEntity
{
    /// <summary>
    /// An instance of TContext
    /// </summary>
    protected readonly TContext Context;

    /// <summary>
    /// An instance of <see cref="IMapper"/>
    /// </summary>
    protected readonly IMapper Mapper;

    /// <summary>
    /// An instance of <see cref="CompositeValidator{T}"/>
    /// </summary>
    protected readonly CompositeValidator<TCommand> Validators;

    /// <summary>
    /// Initializes an instance of <see cref="BaseCommandHandler{TContext, TEntity, TCommand}"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="mapper"></param>
    /// <param name="validator"></param>
    public BaseCommandHandler(TContext context,
                              IMapper mapper,
                              CompositeValidator<TCommand> validator)
    {
        Context = context;
        Mapper = mapper;
        Validators = validator;
    }

    /// <summary>
    /// Adds a record in the database based on the specified <paramref name="request"/>.
    /// </summary>
    /// <param name="request">
    /// The object that will be mapped to the record that will be added to the database.
    /// <typeparamref name="TCommand"/> must have a mapping configuration to <typeparamref name="TEntity"/>.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<Validation<Error, TEntity>> Add(TCommand request, CancellationToken cancellationToken = default)
    {
        var entity = Mapper.Map<TEntity>(request);
        Context.Add(entity);
        _ = await Context.SaveChangesAsync(cancellationToken);
        return Success<Error, TEntity>(entity);
    }

    /// <summary>
    /// Updates a record in the database based on the specified <paramref name="request"/>.
    /// </summary>
    /// <param name="request">
    /// The object that will be mapped to the record that will be updated.
    /// <typeparamref name="TCommand"/> must have a mapping configuration to <typeparamref name="TEntity"/>.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<Validation<Error, TEntity>> Edit(TCommand request, CancellationToken cancellationToken = default) =>
        await Context.GetSingle<TEntity>(e => e.Id == request.Id, tracking: true, cancellationToken: cancellationToken).MatchAsync(
            Some: async entity =>
            {
                Mapper.Map(request, entity);
                Context.Update(entity);
                _ = await Context.SaveChangesAsync(cancellationToken);
                return Success<Error, TEntity>(entity);
            },
            None: () => Fail<Error, TEntity>($"Record with id {request.Id} does not exist"));

    /// <summary>
    /// Deletes the record with the specified <paramref name="id"/> in the database.
    /// </summary>
    /// <param name="id">Id of the record that will be deleted</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<Validation<Error, TEntity>> Delete(string id, CancellationToken cancellationToken = default) =>
        await Context.GetSingle<TEntity>(e => e.Id == id, cancellationToken: cancellationToken).MatchAsync(
            Some: async entity =>
            {
                Context.Remove(entity);
                _ = await Context.SaveChangesAsync(cancellationToken);
                return Success<Error, TEntity>(entity);
            },
            None: () => Fail<Error, TEntity>($"Record with id {id} does not exist"));
			
	protected async Task UpdateEntitySubCollectionAsync<TMainEntity, TSubCollection>(
	   string id,
	   string foreignKeyField,
	   string subCollectionFieldName,
	   TMainEntity entity,
	   CancellationToken cancellationToken)
	   where TSubCollection : class
	   where TMainEntity : class
	{
		var collectionPropertyAccessor = GetPropertyAccessor(typeof(TMainEntity), subCollectionFieldName);
		var subCollections = (collectionPropertyAccessor(entity) as IEnumerable<TSubCollection>)?.ToList();
		// Fetch all IDs from the database for TSubCollection entities related to the main entity
		var idsInDatabase = await Context.Set<TSubCollection>()
			.Where(x => EF.Property<string>(x, foreignKeyField) == id)
			.Select(x => EF.Property<string>(x, "Id"))
			.ToListAsync(cancellationToken);
		// Extract IDs from the entity's sub-collections, handling null collections gracefully
		var idPropertyAccessor = GetPropertyAccessor(typeof(TSubCollection), "Id");
		// Use the accessor in a loop or wherever needed
		var idsInEntity = subCollections?.Select(sc => idPropertyAccessor(sc)?.ToString()).ToList() ?? [];
		// Determine which entities need to be deleted (present in database but not in the new list)
		var idsToDelete = idsInDatabase.Except(idsInEntity).ToList();
		if (idsToDelete.Count != 0)
		{
			var entitiesToDelete = await Context.Set<TSubCollection>()
				.Where(x => idsToDelete.Contains(EF.Property<string>(x, "Id")))
				.ToListAsync(cancellationToken);
			Context.RemoveRange(entitiesToDelete);
		}
		// Process updates or additions for entities in the sub-collection
		if (subCollections != null)
		{
			foreach (var subCollection in subCollections)
			{
				var subCollectionIdProp = subCollection.GetType().GetProperty("Id");
				var subCollectionId = subCollectionIdProp?.GetValue(subCollection)?.ToString();
				// Check if the sub-collection ID is already in the database
				if (idsInDatabase.Contains(subCollectionId!))
				{
					Context.Entry(subCollection).State = EntityState.Modified;
					// Prevent update to CreatedDate and UpdatedDate if the item exists in the database
					var entry = Context.Entry(subCollection);
					entry.Property(nameof(BaseEntity.Entity)).IsModified = false;
					entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
					entry.Property(nameof(BaseEntity.CreatedDate)).IsModified = false;
				}
				else
				{
					Context.Entry(subCollection).State = EntityState.Added;
				}
			}
		}
	}
	protected void AddEntitySubCollection<TMainEntity, TSubCollection>(TMainEntity entity, string collectionPropertyName)
	where TMainEntity : class
	where TSubCollection : class
	{
		// Use cached property accessor to get the collection property
		var collectionAccessor = GetPropertyAccessor(typeof(TMainEntity), collectionPropertyName);
		IEnumerable<TSubCollection>? collection = collectionAccessor(entity) as IEnumerable<TSubCollection>;
		if (collection != null && collection.Any())
		{
			foreach (var item in collection)
			{
				Context.Entry(item).State = EntityState.Added;
			}
		}
	}
	/// <summary>
	/// Caching Reflection Results to improve performance
	/// </summary>
	private static readonly ConcurrentDictionary<string, Delegate> _cache = new();
	private static Func<object, object> GetPropertyAccessor(Type type, string propertyName)
	{
		string cacheKey = $"{type.FullName}.{propertyName}";
		var accessor = (Func<object, object>)_cache.GetOrAdd(cacheKey, key =>
		{
			var param = Expression.Parameter(typeof(object), "x");
			var body = Expression.PropertyOrField(Expression.Convert(param, type), propertyName);
			var lambda = Expression.Lambda<Func<object, object>>(Expression.Convert(body, typeof(object)), param);
			return lambda.Compile();
		});
		return accessor;
	}
}
