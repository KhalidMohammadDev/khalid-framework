using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Khalid.Core.Framework
{

    public interface IModificationEntity : IEntity
    {

        DateTime CreateDate { get; set; }

        int? CreateByUserId { get; set; }

        DateTime LastUpdateDate { get; set; }

        int? LastUpdateByUserId { get; set; }
    }
    public interface IEntity
    {
        int Id { get; set; }

    }
    public interface IUserEntity : IEntity
    {
        List<int> UserRoles { get; }

    }
    public interface IOrderedEntity
    {
        int Order { get; set; }

    }

    public enum OrderType
    {
        Desc,
        Ase
    }

    public static class IQuerableExtensions
    {
        public static async Task<PaginationResult<T>> ToOrderedPaginationResultAsync<T>(this IQueryable<T> data, PaginationModel paginationModel, int totalRecords = 0)
        //where T : IEntity
        {
            if (typeof(T).IsAssignableTo(typeof(IOrderedEntity)))
                data = data.OrderBy("Order");
            else if (typeof(T).IsAssignableTo(typeof(IEntity)))
                data = data.OrderByDescending("Id");

            return await ToPaginationResultAsync(data, paginationModel, totalRecords);
        }

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string propertyName)
        {
            return Order(source, propertyName, OrderType.Ase);
        }


        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> source, string propertyName)
        {
            return Order(source, propertyName, OrderType.Desc);
        }


        public static IOrderedQueryable<TSource> Order<TSource>(this IQueryable<TSource> query, string propertyName, OrderType orderType)
        {
            string funcName;
            switch (orderType)
            {
                case OrderType.Ase:
                    funcName = "OrderBy";
                    break;
                case OrderType.Desc:
                    funcName = "OrderByDescending";
                    break;
                default:
                    throw new NotSupportedException(nameof(orderType));
            }

            var entityType = typeof(TSource);

            //Create x=>x.PropName
            var propertyInfo = entityType.GetProperty(propertyName);
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            MemberExpression property = Expression.Property(arg, propertyName);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });

            //Get System.Linq.Queryable.OrderBy() method.
            var enumarableType = typeof(System.Linq.Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == funcName && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     //Put more restriction here to ensure selecting the right overload                
                     return parameters.Count == 2;//overload that has 2 parameters
                 }).Single();
            //The linq's OrderBy<TSource, TKey> has two generic types, which provided here
            MethodInfo genericMethod = method
                 .MakeGenericMethod(entityType, propertyInfo.PropertyType);

            /*Call query.OrderBy(selector), with query and selector: x=> x.PropName
              Note that we pass the selector as Expression to the method and we don't compile it.
              By doing so EF can extract "order by" columns and generate SQL for it.*/
            var newQuery = (IOrderedQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }

        public static async Task<PaginationResult<T>> ToPaginationResultAsync<T>(this IQueryable<T> data, PaginationModel paginationModel, int totalRecords = 0)
        {
            int totalFilteredRecords = await data.CountAsync();
            totalRecords = totalRecords == 0 ? totalFilteredRecords : totalRecords;
            var filteredData = await data.Skip((paginationModel.PageNumber - 1) * paginationModel.PageSize).Take(paginationModel.PageSize).ToListAsync();
            int displayRecords = filteredData.Count;
            return new PaginationResult<T>
            {
                Data = filteredData,
                TotalDisplayRecords = displayRecords,
                TotalFilteredRecords = totalFilteredRecords,
                TotalRecords = totalRecords
            };
        }


    }

    public class PaginationResult<T>
    {
        public int TotalRecords { get; set; }

        public int TotalDisplayRecords { get; set; }
        public int TotalFilteredRecords { get; set; }

        public List<T> Data { get; set; }

        public PaginationResult()
        {

        }

        public PaginationResult<U> Create<U>(Func<T, U> converter)
        {
            return new Framework.PaginationResult<U>
            {
                TotalRecords = TotalRecords,
                TotalDisplayRecords = TotalDisplayRecords,
                TotalFilteredRecords = TotalFilteredRecords,
                Data = Data.Select(converter).ToList()
            };
        }

        public dynamic ToDatatableModel()
        {
            return new
            {
                // this is what datatables wants sending back
                //draw = model.draw,
                recordsTotal = TotalRecords,
                recordsFiltered = TotalFilteredRecords,
                data = Data
            };
        }
    }

    public class PaginationModel
    {
        [Required]
        public int PageSize { get; set; }

        [Required]
        public int PageNumber { get; set; }

    }
}
